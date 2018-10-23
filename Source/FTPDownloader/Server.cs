using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FTPDownloader
{
    public class FTPServer
    {
        private Config config;
        private bool isDownloaded;
        private List<string> localFileNames;

        public FTPServer() /* contructor */
        {
            isDownloaded = false;
            localFileNames = new List<string>();

            /* read config */
            ClientFunction.ShowMessage(string.Format("Call back FTP server at {0}", DateTime.Now.ToShortTimeString()), Constants.EMessage.INFO);
            string configPath = Path.Combine(Environment.CurrentDirectory, Constants.CONFIG_FILENAME);
            config = FileFunction.LoadJsonFile<Config>(configPath);

            /* config error */
            if (config == null)
                ClientFunction.ShowMessage("Unable to read config file.", Constants.EMessage.ERROR);

            /* config success */
            ClientFunction.ShowMessage("Readed config file.", Constants.EMessage.INFO);
        }

        public double GetConfigTimer()
        {
            return this.config?.Timer ?? 1.0;
        }

        /* check connect */
        public bool IsConnected()
        {
            if (config != null)
            {
                if (!string.IsNullOrEmpty(config.FTPFilePath) && !string.IsNullOrEmpty(config.FTPUser) && !string.IsNullOrEmpty(config.FTPPassword))
                {
                    try
                    {
                        // create an FTP client
                        using (FtpClient client = new FtpClient(config.FTPHost))
                        {
                            // create credentials
                            client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                            // begin connecting to the server
                            client.Connect();

                            ClientFunction.ShowMessage("Checked connect to FTP.", Constants.EMessage.INFO);

                            client.Disconnect();
                        }
                        return true;
                    }
                    catch (Exception ex) { }
                }
                ClientFunction.ShowMessage("Unable to connect FTP server.", Constants.EMessage.ERROR);
            }

            return false;
        }

        /* download, move file to another path, copy file to share network */
        public void Download()
        {
            try
            {
                // create an FTP client
                using (FtpClient client = new FtpClient(config.FTPHost))
                {
                    // create credentials
                    client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                    // begin connecting to FTP server
                    client.Connect();

                    // check if a directory exists
                    if (client.DirectoryExists(config.FTPFilePath))
                    {
                        // get all file in directory
                        foreach (FtpListItem item in client.GetListing(config.FTPFilePath))
                        {
                            // check if file is xml file
                            if (item.Type == FtpFileSystemObjectType.File && Path.GetExtension(item.Name).ToLower() == ".xml")
                            {
                                // download the file
                                string localFilePath = Path.Combine(config.LocalFilePath, item.Name);
                                client.DownloadFile(localFilePath, item.FullName);
                                localFileNames.Add(localFilePath);
                            }
                        }

                        isDownloaded = true;
                        ClientFunction.ShowMessage("Download successful!", Constants.EMessage.SUCCESS);
                    }
                    else
                        ClientFunction.ShowMessage(string.Format("File not found. {0}", config.FTPFilePath), Constants.EMessage.ERROR);

                    // disconnect to FTP server
                    client.Disconnect();
                }
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to download file.", Constants.EMessage.ERROR); }
        }

        public void MoveFTPFile()
        {
            try
            {
                if (isDownloaded)
                {
                    using (FtpClient client = new FtpClient(config.FTPHost))
                    {
                        // create credentials
                        client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                        // begin connecting to FTP server
                        client.Connect();

                        foreach (string fileFullName in localFileNames)
                        {
                            // get file name form local file path
                            string fileName = Path.GetFileName(fileFullName);

                            //upload a file to another path on ftp server 
                            client.RetryAttempts = 3; //and retry 3 times before giving up
                            client.UploadFile(fileFullName, Path.Combine(config.FTPMoveFileDirectory, fileName), FtpExists.Overwrite, true, FtpVerify.Retry);

                            //delete current file
                            client.DeleteFile(Path.Combine(config.FTPFilePath, fileName));
                        }

                        ClientFunction.ShowMessage("Move FTP file successful!", Constants.EMessage.SUCCESS);

                        client.Disconnect();
                    }
                }
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to move a file to FPT folder.", Constants.EMessage.ERROR); }
        }

        public void CopyLocalFile()
        {
            try
            {
                if (isDownloaded)
                {
                    // connect to shared folder
                    Uri uri = new Uri(config.SharedNetworkDirectory);
                    using (Impersonator.ImpersonateUser(config.SharedNetworkUser, uri.Host, config.SharedNetworkPassword))
                    {
                        foreach (string fileFullName in localFileNames)
                        {
                            /* copy-past to share folder */
                            File.Copy(fileFullName, Path.Combine(@config.SharedNetworkDirectory, Path.GetFileName(fileFullName)), true);
                        }

                        ClientFunction.ShowMessage("Move file to network successful!", Constants.EMessage.SUCCESS);
                    }
                }
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to move file to shared network folder.", Constants.EMessage.ERROR); }
        }
    }

    public class Config
    {
        public string FTPHost { get; set; }
        public string FTPFilePath { get; set; }
        public string FTPMoveFileDirectory { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string LocalFilePath { get; set; }
        public string SharedNetworkDirectory { get; set; }
        public string SharedNetworkUser { get; set; }
        public string SharedNetworkPassword { get; set; }
        public double Timer { get; set; }

        public Config()
        {
            FTPHost = "";
            FTPFilePath = "";
            FTPMoveFileDirectory = "";
            FTPUser = "";
            FTPPassword = "";
            LocalFilePath = "";
            SharedNetworkDirectory = "";
            SharedNetworkUser = "";
            SharedNetworkPassword = "";
            Timer = 1.0;
        }
    }
}