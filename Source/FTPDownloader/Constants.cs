using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPDownloader
{
    public class Constants
    {
        public const string CONFIG_FILENAME = "Config.json";

        public enum EMessage
        {
            INFO = 0,
            SUCCESS = 1,
            ERROR = 2,
        }
    }
}
