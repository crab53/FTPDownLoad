using FluentFTP;
using System;
using System.IO;
using System.Net;
using System.Security.Principal;

namespace FTPDownloader
{
    public class FTPServer
    {
        private Config config;

        public FTPServer() /* contructor */
        {
            /* read config */
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

                        // create credentials
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
                using (FtpClient client = new FtpClient(config.FTPHost))
                {
                    // create credentials
                    client.Credentials = new NetworkCredential(config.FTPUser, config.FTPPassword);

                    // begin connecting to FTP server
                    client.Connect();

                    // check if a file exists
                    if (client.FileExists(config.FTPFilePath))
                    {
                        // download the file
                        string fileName = Path.GetFileName(config.FTPFilePath);
                        string localFilePath = Path.Combine(config.LocalFilePath, fileName);
                        client.DownloadFile(localFilePath, config.FTPFilePath);

                        ClientFunction.ShowMessage("Download successful!", Constants.EMessage.SUCCESS);

                        try
                        {
                            // upload a file and retry 3 times before giving up
                            client.RetryAttempts = 3;
                            client.UploadFile(localFilePath, Path.Combine(config.FTPMoveFileDirectory, fileName), FtpExists.Overwrite, true, FtpVerify.Retry);

                            // delete current file
                            client.DeleteFile(config.FTPFilePath);

                            ClientFunction.ShowMessage("Move FTP file successful!", Constants.EMessage.SUCCESS);

                            // check network directory exists
                            if (Directory.Exists(config.SharedNetworkDirectory))
                            {
                                try
                                {
                                    // create window principal
                                    AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

                                    // logon to shared folder
                                    WindowsIdentity idnt = new WindowsIdentity(config.SharedNetworkUser, config.SharedNetworkPassword);
                                    WindowsImpersonationContext context = idnt.Impersonate();

                                    // copy local file to shared network folder
                                    string sharedNetworkFilePath = Path.Combine(config.SharedNetworkDirectory, fileName);
                                    File.Copy(localFilePath, Path.Combine(config.SharedNetworkDirectory, fileName), true);

                                    context.Undo();
                                }
                                catch (Exception ex) { ClientFunction.ShowMessage("Unable to move a file to shared network folder.", Constants.EMessage.ERROR); }
                            }
                            else
                                ClientFunction.ShowMessage(string.Format("Network directory not found. {0}", config.SharedNetworkDirectory), Constants.EMessage.ERROR);
                        }
                        catch (Exception ex) { ClientFunction.ShowMessage("Unable to move a file to FPT folder.", Constants.EMessage.ERROR); }
                    }
                    else
                        ClientFunction.ShowMessage(string.Format("File not found. {0}", config.FTPFilePath), Constants.EMessage.ERROR);

                    // disconnect to FTP server
                    client.Disconnect();
                }
            }
            catch (Exception ex) { ClientFunction.ShowMessage("Unable to download file.", Constants.EMessage.ERROR); }
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