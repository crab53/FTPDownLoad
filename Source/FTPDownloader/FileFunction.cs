using Newtonsoft.Json;
using System;
using System.IO;

namespace FTPDownloader
{
    public class FileFunction
    {
        public static T LoadJsonFile<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                    }
                }
                else
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        Config config = new Config();
                        sw.WriteLine(JsonConvert.SerializeObject(config));
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return default(T);
        }
    }
}