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
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(config.FTPFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        Stream responseStream = response.GetResponseStream();
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
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(config.FTPFilePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string localFilePath = Path.Combine(config.LocalFilePath, Path.GetFileName(config.FTPFilePath));
                File.WriteAllText(localFilePath, reader.ReadToEnd());

                reader.Close();
                response.Close();
                ClientFunction.ShowMessage("Download successful!", Constants.EMessage.SUCCESS);
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to download file.", Constants.EMessage.ERROR); }
        }

        private FtpWebRequest CreateFtpWebRequest(string ftpFilePath, string ftpUser, string ftpPassword, bool keepAlive = false)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpFilePath));
            request.Proxy = null;
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

            return request;
        }
    }

    public class Config
    {
        public string FTPFilePath { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string LocalFilePath { get; set; }
        public double Timer { get; set; }

        public Config()
        {
            FTPFilePath = "";
            FTPUser = "";
            FTPPassword = "";
            LocalFilePath = "";
            Timer = 1.0;
        }
    }
}
