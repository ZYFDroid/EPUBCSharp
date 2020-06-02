using eBdb.EpubReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                MessageBox.Show("将EPUB拖到程序图标上打开。");
                return;
            }
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static SHA1 sha1 = new SHA1CryptoServiceProvider();
        public static String getHash(this String str) { return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", ""); }
    }
}
