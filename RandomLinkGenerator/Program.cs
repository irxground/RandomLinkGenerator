using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace RandomLinkDownloader {
    class Program {

        const string outputFileName = @".\sitelist.txt";
        //static readonly WebProxy proxy = new WebProxy("157.78.32.243", 8080);
        
        static void Main(string[] args) {
            Console.WriteLine("start: {0:HH:mm:ss}", DateTime.Now);
            int num;
            try {
                num = Int32.Parse(args[0]);
            }
            catch {
                num = 500;
            }
            using (var writer = new StreamWriter(outputFileName)) {
                int count = 0;
                foreach (string str in EnumerateRandomLinks().Take(num)) {
                    writer.WriteLine(str);
                    writer.Flush();
                    Console.Write((++count) + "-");
                }
            }
            Console.WriteLine();
            Console.WriteLine("end: {0:HH:mm:ss}", DateTime.Now);
        }

        static IEnumerable<string> EnumerateRandomLinks() {
            Regex reg = new Regex("<a href=\"(?<url>.*?)\".*?>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string createURL = null;
            string previousURL = null;

            Random rand = new Random();

            while (true) {
                createURL = GenerateUrl(2, rand, reg, previousURL);
                previousURL = createURL;
                yield return createURL;
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static string GenerateUrl(int depth, Random rand, Regex reg, string previousURL, string startURL = null) {
            int errorCount = 0;
            while (true) {
                try {
                    string parentURL = startURL ?? URLSeed.url[rand.Next(URLSeed.url.Length)];
                    string html = string.Empty;

                    HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(parentURL);
                    //webreq.Proxy = proxy;
                    webreq.Timeout = 20000;
                    var sw = Stopwatch.StartNew();
                    HttpWebResponse webres = (HttpWebResponse)webreq.GetResponse();
                    using (StreamReader sr = new StreamReader(webres.GetResponseStream(), Encoding.GetEncoding("Shift_JIS"))) {
                        html = sr.ReadToEnd();
                    }
                    sw.Stop();
                    Trace.WriteLine(String.Format("LoadTime: {0,5}ms", sw.ElapsedMilliseconds));
                    sw.Reset();
                    sw.Start();
                    var al = new List<string>();
                    for (Match m = reg.Match(html); m.Success; m = m.NextMatch()) {
                        al.Add(m.Groups["url"].Value);
                    }

                    var createURL = GetPossibleURL(al, rand, parentURL);
                    sw.Stop();
                    Trace.WriteLine(String.Format("ParseTime:{0,5}ms", sw.ElapsedMilliseconds));

                    if (createURL == string.Empty ||
                        createURL == previousURL ||
                        createURL.IndexOf("javascript:") != -1 ||
                        createURL.IndexOf("mailto:") != -1 ||
                        !IsOnlyOneByteChar(createURL)) continue;

                    if (depth > 1) {
                        return createURL;
                    }
                    else {
                        return GenerateUrl(depth - 1, rand, reg, previousURL, createURL);
                    }
                }
                catch (WebException) {
                    errorCount++;
                    if (errorCount > 10)
                        throw;
                    else
                        continue;
                }
            }
        }

        private static string GetPossibleURL(List<string> al, Random rand, string parentURL) {
            if (al.Count == 0) return parentURL;

            int rnd = rand.Next(al.Count - 1);
            string url = al[rnd].ToString();
            if (url.IndexOf("javascript:") != -1 || url.IndexOf("Javascript:") != -1 || url.IndexOf("mailto:") != -1 || url.IndexOf("'") != -1 || url.IndexOf("+") != -1 || url.IndexOf("$") != -1 || url.IndexOf("<") != -1 || !IsOnlyOneByteChar(url)) {
                al.RemoveAt(rnd);
                url = GetPossibleURL(al, rand, parentURL);
            }
            else if (url.IndexOf("//") == 0) {
                url = "http:" + url;
            }
            else if (url.IndexOf("http://") != 0 && url.IndexOf("https://") != 0) {
                if (parentURL.LastIndexOf("/") == parentURL.Length - 1 && url.IndexOf("/") == 0) url = url.Remove(0, 1);
                url = parentURL + url;
            }
            return url;
        }

        private static bool IsOnlyOneByteChar(string str) {
            byte[] byte_data = Encoding.GetEncoding(932).GetBytes(str);
            if (byte_data.Length == str.Length) {
                return true;
            }
            else {
                return false;
            }
        }

    }
}
