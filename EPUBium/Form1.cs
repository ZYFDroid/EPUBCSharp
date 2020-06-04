﻿using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using eBdb.EpubReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            mBrowser.Load("http://epub.zyfdroid.com/static/index.html");
            mBrowser.ConsoleMessage += MBrowser_ConsoleMessage;
        }
        
        class SettingItem {
            public Size windowsize;
            public double bookzoom;
        }

        bool bookInited = false;

        private void MBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (e.Message.Contains("<EPUB_BOOK_INIT_START>")) {
                mBrowser.SetZoomLevel(usingZoom);
                initBook();
            }

            if (e.Message.Contains("<EPUB_BOOK_INIT_SUCCESS>")) {
                bookInited = true;
                readBookInfo();
            }

            if (e.Message.StartsWith("::EPUBTOC:")) {
                String json = e.Message.Substring("::EPUBTOC:".Length);
                bookEntries = JsonConvert.DeserializeObject<List<BookEntry>>(json);
                searchChapters(bookEntries);
                mBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("reportLocationAsync()");
            }

            if (e.Message.StartsWith("::EPUBINFO:"))
            {
                String json = e.Message.Substring("::EPUBINFO:".Length);
                BookInfo bi = JsonConvert.DeserializeObject<BookInfo>(json);
                Program.bookName = bi.title;
                Program.bookAuthor = bi.creator;
                runOnUiThread(() => this.Text = Program.bookName + " - " + Program.bookAuthor);
            }

            if (e.Message.StartsWith("::REPORT_CHAPTER:")) {
                String href = e.Message.Substring("::REPORT_CHAPTER:".Length);
                if (bookEntryById.ContainsKey(href)) {
                    runOnUiThread(() => lblChapterIndicator.Text = bookEntryById[href].label.Trim());
                    
                }
            }

            if (e.Message.StartsWith("::REPORT_LOCATION:"))
            {
                String page = e.Message.Substring("::REPORT_LOCATION:".Length);
                runOnUiThread(() => lblPageIndicator.Text = page) ;
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

            String script = "var book = ePub(\"../book/" + opf.Replace("\"", "\\\"") + "\");var renderH = book.renderTo(\"epubsub\");renderH.themes.fontSize(15);var displayed = renderH.display().then(()=>{console.log(\"<EPUB_BOOK_INIT_SUCCESS>\")});";

            String locationFile = Path.Combine("bookinfo", Program.bookID, "readingposition.json");

            if (File.Exists(locationFile))
            {
                script += "renderH.display(\"" + File.ReadAllText(locationFile) + "\");";
            }
            mBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(script);


        }

        public void runOnUiThread(Action act) {
            if (InvokeRequired)
            {
                Invoke(act);
            }
            else {
                act.Invoke();
            }
        }

        List<BookEntry> bookEntries = new List<BookEntry>();
        Dictionary<String, BookEntry> bookEntryById = new Dictionary<string, BookEntry>();
        void readBookInfo() {
            mBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("reportBookInfo()");
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            mBrowser.Load("javascript:renderH.prev();reportLocationAsync();");
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            mBrowser.Load("javascript:renderH.next();reportLocationAsync();");
        }

        private void inspectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mBrowser.GetBrowser().GetHost().ShowDevTools();
        }

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
                        mBrowser.Load("javascript:renderH.display(\"" + str.Replace("\"", "\\\"") + "\");");
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
            if (!bookInited) { return; }
            Task<JavascriptResponse> jsr = mBrowser.GetBrowser().MainFrame.EvaluateScriptAsync("renderH.location.start.cfi");
            jsr.Wait();
            String location = jsr.Result.Result.ToString();
            File.WriteAllText(Path.Combine("bookinfo", Program.bookID, "readingposition.json"), location);
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
            Task<JavascriptResponse> jsr = mBrowser.GetBrowser().MainFrame.EvaluateScriptAsync("renderH.location.start.cfi");
            jsr.Wait();
            String location = jsr.Result.Result.ToString();
            ShortTextWindow dlg = new ShortTextWindow();
            dlg.textBox1.Text = location;
            dlg.ShowDialog();
            dlg.Dispose();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShortTextWindow dlg = new ShortTextWindow();
            if (dlg.ShowDialog() == DialogResult.OK) {
                String cfi = dlg.textBox1.Text;
                dlg.Dispose();
                if (cfi.Length > 2)
                {
                    mBrowser.Load("javascript:renderH.display(\"" + cfi.Replace("\\","\\\\").Replace("\"", "\\\"") + "\");");
                }
                else {
                    MessageBox.Show("Invalid CFI Location");
                }
            }
        }
    }


    class Win32
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers control, Keys vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int WM_HOTKEYS = 0x0312;

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
    }
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }

    class BookEntry {
        public String id;
        public String href;
        public String label;
        public List<BookEntry> subitems;
    }

    class BookInfo
    {
        public String title;
        public String creator;
    }

}