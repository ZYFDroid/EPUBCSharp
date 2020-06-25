using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using eBdb.EpubReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EPUBium
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            IBrowserSettings bs;
            mBrowser = new ChromiumWebBrowser("http://epub.zyfdroid.com/static/index.html");
            bs = mBrowser.BrowserSettings;
            mBrowser.RequestHandler = new ModifiedRequestHandler();
            tblMain.Controls.Add(mBrowser);
            mBrowser.Dock = DockStyle.Fill;
        }

        ChromiumWebBrowser mBrowser;

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(Program.openingPath, "setting.json")))
            {
                SettingItem si = JsonConvert.DeserializeObject<SettingItem>(File.ReadAllText(Path.Combine(Program.openingPath, "setting.json")));
                Size = si.windowsize;
                usingZoom = si.bookzoom;
            }
            Icon = Properties.Resources.ic_book;
            createHotKeys();
            registerHotkeys(true);
            initConsoleMessageHandlers();
            mBrowser.ConsoleMessage += MBrowser_ConsoleMessage;
            mBrowser.Load("http://epub.zyfdroid.com/static/index.html");
            mBrowser.MenuHandler = new MyMenu(this);
        }

        class MyMenu : IContextMenuHandler
        {
            Form1 _this;

            public MyMenu(Form1 @this)
            {
                _this = @this;
            }

            public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                if (parameters.HasImageContents || parameters.SelectionText != "")
                {
                    model.AddSeparator();
                    model.AddItem(CefMenuCommand.UserFirst + 1, "加入笔记");
                }
            }

            public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                if (commandId == CefMenuCommand.UserFirst + 1) {
                    if (parameters.HasImageContents)
                    {
                        _this.Invoke(new Action<string>(_this.onNoteImageArrival), parameters.SourceUrl);
                    }
                    else if(parameters.SelectionText !="") {
                        _this.Invoke(new Action<string>(_this.onNoteTextArrival), parameters.SelectionText);
                    }
                    return true;
                }
                return false;
            }

            public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
            {
                
            }

            public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }

        class SettingItem {
            public Size windowsize;
            public double bookzoom;
        }

        bool bookInited = false;

        Dictionary<string, Action<String>> consoleMessageHandlers = new Dictionary<string, Action<string>>();


        String currentProgress = "";

        private void initConsoleMessageHandlers() {
            consoleMessageHandlers.Add("EPUB_BOOK_INIT_START", (a) =>
            {
                mBrowser.SetZoomLevel(usingZoom);
                initBook();
            });

            consoleMessageHandlers.Add("EPUB_BOOK_INIT_SUCCESS", (a) =>
            {
                bookInited = true;
                executeJavascriptFunction("reportBookInfo");
            });

            consoleMessageHandlers.Add("EPUBTOC", (json) =>
            {
                bookEntries = JsonConvert.DeserializeObject<List<BookEntry>>(json);
                searchChapters(bookEntries);
                executeJavascriptFunction("reportLocationAsync");
            });

            consoleMessageHandlers.Add("EPUBINFO", (json) =>
            {
                BookInfo bi = JsonConvert.DeserializeObject<BookInfo>(json);
                Program.bookName = bi.title;
                Program.bookAuthor = bi.creator;
                this.Text = Program.bookName + " - " + Program.bookAuthor;
            });

            consoleMessageHandlers.Add("REPORT_CHAPTER", (href) =>
            {
                if (bookEntryById.ContainsKey(href))
                {
                    lblChapterIndicator.Text = bookEntryById[href].label.Trim();
                }
            });

            consoleMessageHandlers.Add("REPORT_LOCATION", (page) =>
            {
                lblPageIndicator.Text = page;
            });

            consoleMessageHandlers.Add("GET_SAVING", (rp) => currentProgress = rp);
        }

        private void MBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (e.Message.StartsWith("<::")) {
                int end = e.Message.IndexOf(':', 3);
                string type = (e.Message.Substring(3, end - 3));
                string data = (e.Message.Substring(end + 1));

                if (consoleMessageHandlers.ContainsKey(type))
                {
                    Invoke(consoleMessageHandlers[type], data);
                }
                else {
                    Console.WriteLine("Dropped event " + type + " with data " + data);
                }
            }
        }

        void searchChapters(IEnumerable<BookEntry> root) {
            root.ToList().ForEach(c => {
                if (bookEntryById.ContainsKey(c.href)) {
                    bookEntryById[c.href] = c;
                }
                else
                {
                    bookEntryById.Add(c.href, c);
                }
                searchChapters(c.subitems);
            });
        }

        void initBook() {

            String metaFile = Path.Combine("bookinfo", Program.bookID, "cache", "META-INF", "container.xml");
            if (!File.Exists(metaFile))
            {
                MessageBox.Show("Invalid EPUB file.");
                this.FormClosing -= Form1_FormClosing;
                Application.Exit();
            }
            XDocument xld = XDocument.Load(metaFile);
            XElement temp = xld.Elements().Where(w => w.Name.LocalName == "container").First();
            temp = temp.Elements().Where(w => w.Name.LocalName == "rootfiles").First();
            temp = temp.Elements().Where(w => w.Name.LocalName == "rootfile").First();
            String opf = temp.Attribute("full-path").Value;
            String locationFile = Path.Combine("bookinfo", Program.bookID, "readingposition.json");
            String location = "";
            if (File.Exists(locationFile))
            {
                location = File.ReadAllText(locationFile);
            }
            executeJavascriptFunction("loadBookAtUrl", opf, location);
        }

        public void executeJavascriptFunction(String functionName, params object[] arguments) {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append(functionName).Append("(");
            for (int i = 0; i < arguments.Length; i++)
            {
                object obj = arguments[i];
                if (obj == null) {
                    scriptBuilder.Append("null");
                } else if (obj is string)
                {
                    scriptBuilder.Append((obj as string).escapeText());
                }
                else if (obj is bool) {
                    scriptBuilder.Append(obj.ToString().ToLower());
                }
                else {
                    scriptBuilder.Append(obj.ToString());
                }
                if (i != arguments.Length - 1) {
                    scriptBuilder.Append(",");
                };
            }
            scriptBuilder.Append(");");
            mBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(scriptBuilder.ToString());
        }

        List<BookEntry> bookEntries = new List<BookEntry>();
        Dictionary<String, BookEntry> bookEntryById = new Dictionary<string, BookEntry>();

        private void btnLeft_Click(object sender, EventArgs e)
        {
            executeJavascriptFunction("prev");
        }

        private void btnRight_Click(object sender, EventArgs e)
        {

            executeJavascriptFunction("next");
        }

        private void inspectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mBrowser.GetBrowser().GetHost().ShowDevTools();
        }

        #region Hotkeys

        private void Form1_Activated(object sender, EventArgs e)
        {
            registerHotkeys();
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            unRegisterHotKeys();
        }


        void createHotKeys() {
            addHotkey(KeyModifiers.None, Keys.Left, () => { btnLeft_Click(null, null); });
            addHotkey(KeyModifiers.None, Keys.Up, () => { btnLeft_Click(null, null); });
            addHotkey(KeyModifiers.None, Keys.PageUp, () => { btnLeft_Click(null, null); });
            addHotkey(KeyModifiers.None, Keys.Right, () => { btnRight_Click(null, null); });
            addHotkey(KeyModifiers.None, Keys.Down, () => { btnRight_Click(null, null); });
            addHotkey(KeyModifiers.None, Keys.PageDown, () => { btnRight_Click(null, null); });
        }
        

        void unRegisterHotKeys(bool hasPersist = false)
        {

            foreach (int id in hotkeys.Keys)
            {
                if (hasPersist)
                {
                    Win32.UnregisterHotKey(this.Handle, id);
                }
                else
                {
                    if (!hotkeys[id].persist)
                    {
                        Win32.UnregisterHotKey(this.Handle, id);
                    }
                }
            }
            Console.WriteLine("Unregister hotkeys");
        }
        void registerHotkeys(bool hasPersist = false)
        {
            foreach (HotkeyClass hkey in hotkeys.Values)
            {
                Win32.RegisterHotKey(Handle, hkey.id, hkey.mods, hkey.mKey);

                if (hasPersist)
                {
                    Win32.RegisterHotKey(Handle, hkey.id, hkey.mods, hkey.mKey);
                }
                else
                {
                    if (!hkey.persist)
                    {
                        Win32.RegisterHotKey(Handle, hkey.id, hkey.mods, hkey.mKey);
                    }
                }

            }



            Console.WriteLine("Register hotkeys");
        }
        private int _lastId = 100;
        void addHotkey(KeyModifiers mod, Keys key, Action action, bool isPersist = false)
        {
            int keyid = ++_lastId;
            hotkeys.Add(keyid, new HotkeyClass()
            {
                id = keyid,
                mKey = key,
                mods = mod,
                act = action,
                persist = isPersist
            });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            unRegisterHotKeys(true);
            mBrowser.Dispose();
        }

        Dictionary<int, HotkeyClass> hotkeys = new Dictionary<int, HotkeyClass>();

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Win32.WM_HOTKEYS)
            {
                hotkeys[m.WParam.ToInt32()].act.Invoke();
            }
            base.WndProc(ref m);
        }

        class HotkeyClass
        {
            public int id;
            public Keys mKey;
            public KeyModifiers mods;
            public Action act;
            public bool persist = false;
        }

        #endregion

        #region epubjs invoke
        class ModifiedRequestHandler : RequestHandler {
            ModifiedResourceHandler mResHandler = new ModifiedResourceHandler();
            protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            {
                if (request.Url.StartsWith("http://epub.zyfdroid.com/"))
                {
                    return mResHandler;
                }
                return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
            }
        }
        class ModifiedResourceHandler : ResourceRequestHandler
        {
            protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
            {
                if (request.Url.StartsWith("http://epub.zyfdroid.com/static")) {
                    String fileName = request.Url.Replace("http://epub.zyfdroid.com/static", Program.staticPath);
                    if (!File.Exists(fileName))
                    {
                        return ResourceHandler.ForErrorMessage("404 Not Found", System.Net.HttpStatusCode.NotFound);
                    }
                    return ResourceHandler.FromFilePath(fileName,Cef.GetMimeType(Path.GetExtension(fileName).Replace(".", "").ToLower()));
                }
                if (request.Url.StartsWith("http://epub.zyfdroid.com/book"))
                {
                    String fileName = request.Url.Replace("http://epub.zyfdroid.com/book", Program.openingPath);
                    if (!File.Exists(fileName)) {
                        return ResourceHandler.ForErrorMessage("404 Not Found", System.Net.HttpStatusCode.NotFound);
                    }
                    return ResourceHandler.FromFilePath(fileName, Cef.GetMimeType(Path.GetExtension(fileName).Replace(".", "").ToLower()));
                }

                return base.GetResourceHandler(chromiumWebBrowser, browser, frame, request);
            }
        }
        #endregion
        
        #region UI 
        private void mnuChapter_Click(object sender, EventArgs e)
        {
            if (!bookInited) { return; }
            FrmChapters chapters = new FrmChapters();
            TreeView v = chapters.treeView1;
            TreeNode root = new TreeNode(Program.bookName);
            addNodes(root, bookEntries);
            v.Nodes.Add(root);
            root.Expand();
            v.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler((Object s,TreeNodeMouseClickEventArgs ev) =>
            {
                if (ev.Button == MouseButtons.Left) {

                    String str = ev.Node.Tag as String;
                    if (null != str)
                    {
                        executeJavascriptFunction("renderH.display", str);
                        chapters.Close();
                    }
                }
            });
            chapters.ShowDialog();
        }

        void addNodes(TreeNode node, IEnumerable<BookEntry> points) {
            points.ToList().ForEach(np => {
                TreeNode tn = new TreeNode() { Text = np.label.Trim(), Tag = np.href };
                node.Nodes.Add(tn);
                addNodes(tn, np.subitems);
                });
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(Path.Combine(Program.openingPath, "setting.json"), JsonConvert.SerializeObject(new SettingItem() {bookzoom = usingZoom,windowsize = Size }));
            File.WriteAllText(Path.Combine("bookinfo", Program.bookID, "readingposition.json"), currentProgress);
        }

        private double usingZoom = 0;

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            mBrowser.SetZoomLevel(0);
            usingZoom = 0;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            mBrowser.SetZoomLevel(1.33);
            usingZoom=(1.33);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            mBrowser.SetZoomLevel(1.66);
            usingZoom = (1.66);
        }

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            mBrowser.SetZoomLevel(2);
            usingZoom = (2);
        }

        private void toolStripMenuItem3_Click_1(object sender, EventArgs e)
        {
            mBrowser.SetZoomLevel(2.7);
            usingZoom = (2.7);
        }

        private void toolStripMenuItem4_Click_1(object sender, EventArgs e)
        {
            if (!bookInited) { return; }
            ShortTextWindow dlg = new ShortTextWindow();
            dlg.textBox1.Text = currentProgress;
            dlg.ShowDialog();
            dlg.Dispose();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShortTextWindow dlg = new ShortTextWindow();
            if (dlg.ShowDialog() == DialogResult.OK) {
                String cfi = dlg.textBox1.Text;
                dlg.Dispose();
                if (cfi.Length > 7)
                {
                    executeJavascriptFunction("renderH.display", cfi);
                }
                else {
                    MessageBox.Show("Invalid CFI Location");
                }
            }
        }

        public void onNoteImageArrival(string url) {
            string path = url.Replace("http://epub.zyfdroid.com/book", Program.openingPath).Replace("/", "\\");
            string outname = "";
            string notePath = Path.Combine(Program.openingPath, "notes");
            if (!Directory.Exists(notePath)) { Directory.CreateDirectory(notePath); }
            if (FrmFileNameDialog.show(out outname)) {
                if (File.Exists(path))
                {
                    File.Copy(path, Path.Combine(notePath, outname + Path.GetExtension(path)),true);
                    MessageBox.Show("笔记保存成功。（刷新笔记查看）");
                }
                else {
                    MessageBox.Show("系统找不到指定的文件");
                }
            }
        }

        public void onNoteTextArrival(string text) {
            string outname = "";
            string notePath = Path.Combine(Program.openingPath, "notes");
            if (!Directory.Exists(notePath)) { Directory.CreateDirectory(notePath); }
            if (FrmFileNameDialog.show(out outname))
            {
                File.WriteAllText(Path.Combine(notePath, outname + ".txt"), text);
                MessageBox.Show("笔记保存成功。（刷新笔记查看）");
            }
        }

        private void clearCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo(Path.GetFullPath("EBookNote.exe"));
            psi.WorkingDirectory = Program.openingPath;
            Process.Start(psi);
        }
    }


    class Win32
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers control, Keys vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int WM_HOTKEYS = 0x0312;
    }
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }

#pragma warning disable CS0649

    class BookEntry {
        public string id;
        public string href;
        public string label;
        public List<BookEntry> subitems;
    }

    class BookInfo
    {
        public string title;
        public string creator;
    }

#pragma warning restore CS0649
    public static class TextUtils {

        public static string escapeText(this string src)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char chr in src.ToCharArray())
            {
                switch (chr) {
                    case '\0': sb.Append("\\0");break;
                    case '\b': sb.Append("\\b");break;
                    case '\t': sb.Append("\\t");break;
                    case '\n': sb.Append("\\n");break;
                    case '\u000B': sb.Append("\\v");break;
                    case '\u000C': sb.Append("\\f");break;
                    case '\r': sb.Append("\\f");break;
                    case '\"': sb.Append("\\\"");break;
                    case '\'': sb.Append("\\\'");break;
                    case '\\': sb.Append("\\\\");break;
                    default: sb.Append(chr);break;
                }
            }
            return sb.Append("\"").ToString();
        }
    }

}
