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
using System.Threading;

namespace RandomLinkDownloader {
    class Program {

        static void Main(string[] args) {

            Console.WriteLine("start: {0:HH:mm:ss}", DateTime.Now);
            int num = ConfigReader.OutputUrlCount;
            var gen = new Generator();
            using (var writer = new StreamWriter(ConfigReader.OutputFilePath)) {
                int count = 0;
                foreach (var url in gen.EnumerateRandomLinks().Take(num)) {
                    writer.WriteLine(url);
                    writer.Flush();
                    count++;
                    Console.WriteLine("{0,4}: {1}", count, url);
                }
            }
            Console.WriteLine();
            Console.WriteLine("end: {0:HH:mm:ss}", DateTime.Now);
        }
    }
}
