using Eternity.Captcha.capLib;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Eternity.Captcha
{
    internal static class CaptchaSolver
    {
        private static RuCaptchaClient RCC = null;
        public static string ApiKey;
        public static string Solve(string url)
        {
            var solved = string.Empty;
            var capId = string.Empty;

            var fileName = new Random().Next() + ".png";
            var key = File.ReadAllLines("Files\\antiCaptcha.txt")[0];
            var rcc = new RuCaptchaClient(key);

            using (var wc = new WebClient())
            {
                if (!Directory.Exists("Tmp"))
                    Directory.CreateDirectory("Tmp");

                wc.DownloadFile(url, $"Tmp\\{fileName}");
            }

            try
            {
                capId = rcc.UploadCaptchaFile($"Tmp\\{fileName}");
            }
            catch
            {

            }

            while (string.IsNullOrEmpty(solved))
            {
                try
                {
                    solved = rcc.GetCaptcha(capId);
                }
                catch
                {

                }

                Thread.Sleep(1000);
            }
            File.Delete($"Tmp\\{fileName}");
            return solved;
        }

        public static void SetKey(string key)
        {
            ApiKey = key;
            RCC = new RuCaptchaClient(key);
        }
        public static string GetBalance()
        {
            string result;
            if (RCC != null)
                result = RCC.GetBalance().ToString();
            else
                result = "?";
            return result;
        }
    }
}
