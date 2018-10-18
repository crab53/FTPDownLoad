using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTPDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1) return;

            Console.Title = "XML Downloader";
            Console.WriteLine("###### XML Downloader Started ######");

            while (true)
            {
                TaskRun();
            }
        }

        private static void TaskRun()
        {
            if ((DateTime.Now - lastTime).TotalHours >= timer)
            {
                lastTime = DateTime.Now;
                FTPServer server = new FTPServer();
                timer = server.GetConfigTimer();
                if (server.IsConnected())
                {
                    server.Download();
                }
                Console.WriteLine("");
            }
            Thread.Sleep(5 * 60 * 1000);
        }

        private static DateTime lastTime;
        private static double timer = 1.0;
    }
}
