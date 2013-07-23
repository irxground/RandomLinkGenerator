using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Net;

namespace RandomLinkDownloader {
    public static class ConfigReader {

        public static string OutputFilePath { get { return Path.Combine(ExePath, Get("OutputFile")); } }

        public static string SeedFilePath { get { return Path.Combine(ExePath, Get("SeedFile")); } }

        public static int OutputUrlCount { get { return GetIntOr("OutputUrlCount", 0); } }

        public static int Sleep { get { return GetIntOr("ConnectionSleep", 0); } }

        public static int ConnectionTimeout { get { return GetIntOr("ConnectionTimeout", 0); } }

        public static int ConnectionRetry { get { return GetIntOr("ConnectionRetry", 0); } }

        public static WebProxy ConnectionProxy {
            get {
                var str = Get("ConnectionProxy");
                if (!String.IsNullOrWhiteSpace(str)) {
                    try {
                        return new WebProxy(str);
                    }
                    catch { }
                }
                return null;
            }
        }

        #region Helper

        private static string ExePath {
            get { return Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location); }
        }

        private static string Get(string key) {
            return ConfigurationManager.AppSettings[key];
        }

        private static int GetIntOr(string key, int defaultValue) {
            int val;
            if (Int32.TryParse(Get(key), out val)) {
                return val;
            }
            return defaultValue;
        }

        #endregion

    }
}
