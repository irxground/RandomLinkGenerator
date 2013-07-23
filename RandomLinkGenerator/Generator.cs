using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Web;

namespace RandomLinkDownloader {
    public class Generator {
        private static readonly Regex Pattern =
            new Regex("<a href=\"(?<url>.*?)\".*?>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly Random rand = new Random();


        public IEnumerable<Uri> EnumerateRandomLinks() {
            Uri createURL = null;
            Uri previousURL = null;

            while (true) {
                createURL = GenerateUrl(2, previousURL);
                previousURL = createURL;
                yield return createURL;
            }
        }

        private Uri GenerateUrl(int depth, Uri previousURL, Uri startURL = null) {
            int errorCount = 0;
            while (true) {
                try {
                    Thread.Sleep(ConfigReader.Sleep);
                    var parentURL = startURL ?? URLSeed.url[rand.Next(URLSeed.url.Count - 1)];
                    string html = string.Empty;

                    HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(parentURL);
                    var proxy = ConfigReader.ConnectionProxy;
                    if (proxy != null) { webreq.Proxy = proxy; }
                    webreq.Timeout = ConfigReader.ConnectionTimeout;
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
                    for (Match m = Pattern.Match(html); m.Success; m = m.NextMatch()) {
                        al.Add(m.Groups["url"].Value);
                    }

                    var createURL = ChooseURL(al, parentURL);
                    sw.Stop();
                    Trace.WriteLine(String.Format("ParseTime:{0,5}ms", sw.ElapsedMilliseconds));

                    if (createURL == previousURL) continue;
                    if (depth > 1) {
                        return createURL;
                    }
                    else {
                        return GenerateUrl(depth - 1, previousURL, createURL);
                    }
                }
                catch (WebException) {
                    errorCount++;
                    if (errorCount > ConfigReader.ConnectionRetry)
                        throw;
                    else
                        continue;
                }
            }
        }

        private Uri ChooseURL(List<string> candidates, Uri parentURL) {
            while (candidates.Count > 0) {
                int randomIndex = rand.Next(candidates.Count - 1);
                Uri uri;
                if (Uri.TryCreate(parentURL, HttpUtility.HtmlDecode(candidates[randomIndex]), out uri)) {
                    bool isHttp = (uri.Scheme == "http" || uri.Scheme == "https");
                    bool isAlpha = IsOnlyOneByteChar(uri.ToString());
                    if (isHttp && isAlpha) {
                        return uri;
                    }
                }
                candidates.RemoveAt(randomIndex);
            }
            return parentURL;
        }

        private bool IsOnlyOneByteChar(string str) {
            byte[] byte_data = Encoding.GetEncoding(932).GetBytes(str);
            return (byte_data.Length == str.Length);
        }
    }
}
