using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RandomLinkDownloader {
    public class URLSeed {
        static List<Uri> _Url;

        public static List<Uri> url {
            get { return _Url ?? (_Url = ReadSeedFile()); }
        }

        private static List<Uri> ReadSeedFile() {
            var list = new List<Uri>();
            using (var reader = new StreamReader(ConfigReader.SeedFilePath)) {
                while (true) {
                    var line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line)) break;
                    Uri uri;
                    if (Uri.TryCreate(line.Trim(), UriKind.Absolute, out uri)) {
                        list.Add(uri);
                    }
                }
            }
            return list;
        }
    }
}
