using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPDownloader
{
    public class FTPServer
    {
        Config config;

        public FTPServer()
        {
            ClientFunction.ShowMessage(string.Format("Call back FTP server at {0}", DateTime.Now.ToShortTimeString()), Constants.EMessage.INFO);
            string configPath = Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FILENAME);
            config = FileFunction.LoadJsonFile<Config>(configPath);
            if (config == null)
                ClientFunction.ShowMessage("Unable to read config file.", Constants.EMessage.ERROR);
            ClientFunction.ShowMessage("Readed config file.", Constants.EMessage.INFO);
        }

        public double GetConfigTimer()
        {
            return this.config?.Timer ?? 1.0;
        }

        public bool IsConnected()
        {
            if (config != null)
            {
                if (!string.IsNullOrEmpty(config.FTPFilePath) && !string.IsNullOrEmpty(config.FTPUser) && !string.IsNullOrEmpty(config.FTPPassword))
                {
                    try
                    {
                        // create an FTP client
                        FtpClient client = new FtpClient(config.FTPHost);

                        // if you don't specify login credentials, we use the "anonymous" user account
                        client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                        // begin connecting to the server
                        client.Connect();

                        ClientFunction.ShowMessage("Checked connect to FTP.", Constants.EMessage.INFO);
                        return true;
                    }
                    catch (Exception ex) { }
                }
                ClientFunction.ShowMessage("Unable to connect FTP server.", Constants.EMessage.ERROR);
            }

            return false;
        }

        public void Download()
        {
            try
            {
                // create an FTP client
                FtpClient client = new FtpClient(config.FTPHost);

                // if you don't specify login credentials, we use the "anonymous" user account
                client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                // begin connecting to the server
                client.Connect();

                // download the file again
                string localFilePath = Path.Combine(config.LocalFilePath, Path.GetFileName(config.FTPFilePath));
                client.DownloadFile(localFilePath, config.FTPFilePath);

                ClientFunction.ShowMessage("Download successful!", Constants.EMessage.SUCCESS);
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to download file.", Constants.EMessage.ERROR); }
        }
    }

    public class Config
    {
        public string FTPHost { get; set; }
        public string FTPFilePath { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string LocalFilePath { get; set; }
        public double Timer { get; set; }

        public Config()
        {
            FTPHost = "";
            FTPFilePath = "";
            FTPUser = "";
            FTPPassword = "";
            LocalFilePath = "";
            Timer = 1.0;
        }
    }
}
