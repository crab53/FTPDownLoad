using System;

namespace FTPDownloader
{
    public class ClientFunction
    {
        public static void ShowMessage(string message, Constants.EMessage level)
        {
            if (!string.IsNullOrEmpty(message))
            {
                switch (level)
                {
                    case Constants.EMessage.INFO:
                        Console.ResetColor();
                        Console.WriteLine("### INFO{" + DateTime.Now.ToShortTimeString() + "}:\t" + message);
                        break;

                    case Constants.EMessage.SUCCESS:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("### SUCCESS{" + DateTime.Now.ToShortTimeString() + "}:\t" + message);
                        break;

                    case Constants.EMessage.ERROR:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("### ERROR{" + DateTime.Now.ToShortTimeString() + "}:\t" + message);
                        break;
                }
                Console.ResetColor();
            }
        }
    }
}