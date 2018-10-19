using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;

namespace FTPDownloader
{
    internal class Program
    {
        private static System.Timers.Timer aTimer;

        private static void Main(string[] args)
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length > 1) return;

            Console.Title = "XML Downloader";
            Console.WriteLine("###### XML Downloader Started ######");

            /* first download */
            FTPServer server = new FTPServer();
            var timer = server.GetConfigTimer();
            if (server.IsConnected())
            {
                server.Download();
            }

            /* set timer */
            SetTimer(timer);

            System.Threading.Thread.Sleep(-1);
        }

        private static void SetTimer(double timer)
        {
            // Create a timer with milisecond .
            aTimer = new System.Timers.Timer(timer * 60 * 60 * 1000);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        /* event timer */

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            FTPServer server = new FTPServer();

            if (server.IsConnected())   /* check connect */
            {
                server.Download();      /* download */
            }
            Console.WriteLine("");
        }
    }
}