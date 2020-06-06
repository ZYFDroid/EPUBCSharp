using eBdb.EpubReader;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPUBium
{
    static class Program
    {

        public static String staticPath = "webres\\static";
        public static String openingPath = "webres\\book";
        public static String bookPath = "";

        public static String bookName = "";
        public static String bookAuthor = "";

        public static String bookID = "";

        public static String workDictionary = "";

        public static List<NavPoint> epubChapters = new List<NavPoint>();

        [STAThread]
        static void Main()
        {
            

            String[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                bookPath = Path.GetFullPath(Environment.GetCommandLineArgs()[1]);
                Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                bookID = bookPath.getHash();
                String extractionPath = Path.Combine("bookinfo", bookPath.getHash(), "cache");
                if (!Directory.Exists(extractionPath))
                {
                    Directory.CreateDirectory(extractionPath);
                    Ionic.Zip.ZipFile.Read(bookPath).ExtractAll(extractionPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                }
                openingPath = extractionPath;
                workDictionary = Path.Combine("bookinfo", bookPath.getHash());
            }
            else {
                MessageBox.Show("将EPUB拖到程序图标上打开。建议将此程序设为EPUB默认打开方式");
                try
                {
                    if (MessageBox.Show("是否设置默认打开方式？", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        setDefaultOpenMethod();
                        MessageBox.Show("设置成功，双击epub打开。");
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    MessageBox.Show("设置失败。或许你应该用管理员权限运行，或者手动设置");
                }
                return;
            }
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static void setDefaultOpenMethod() {
            String thisPath =Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName);
            String category = "ZYFDroid.EPUBCSharp.OpenWith";
            String extension = ".epub";

            RegistryKey classroot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

            classroot.CreateSubKey(extension).SetValue("", category);
            using (RegistryKey rk = classroot.CreateSubKey(category)) {
                rk.SetValue("", "EPUB Book");
                rk.SetValue("FriendlyTypeName", "EPUB Book");
                rk.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("","\""+thisPath+"\" \"%1\"",RegistryValueKind.ExpandString);
            }
        }

        private static SHA1 sha1 = new SHA1CryptoServiceProvider();
        public static String getHash(this String str) { return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", ""); }
    }
}
