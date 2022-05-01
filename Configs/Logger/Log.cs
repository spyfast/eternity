using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Eternity.Configs.Logger
{
    internal static class Log
    {
        public static Queue<string> Logs;
        public static bool Enabled = true;
        public static bool EnabledLogFile = false;
        static Log()
            => Logs = new Queue<string>();

        public static void Push(string message, string login = "", bool isLogs = false, TypeLogger type = TypeLogger.Push)
        {
            var timeFormat = "dd.MM.yyyy, HH:mm:ss";
            var path = string.Empty;
            if (!string.IsNullOrEmpty(login))
                path = $"{login}:";

            if (Enabled)
            {
                Logs.Enqueue($"[{DateTime.Now.ToString(timeFormat)}]. {path} {message}");
            }

            if (isLogs)
                Logs.Enqueue($"[{DateTime.Now.ToString(timeFormat)}]. {path} {message}");

            if (EnabledLogFile)
            {
                if (type == TypeLogger.File)
                {
                    if (!File.Exists("loggers.txt"))
                        File.Create("loggers.txt").Close();

                    File.AppendAllText("loggers.txt", $"[{DateTime.Now.ToString(timeFormat)}]. {path} " + message + "\r\n");
                }
            }
        }
        private const string Name = "Eternity";
        public static void Show(string message, TypeLogShow typeLogShow = TypeLogShow.Info)
        {
            switch (typeLogShow)
            {
                case TypeLogShow.Error:
                    MessageBox.Show(message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case TypeLogShow.Warning:
                    MessageBox.Show(message, Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                default:
                    MessageBox.Show(message, Name);
                    break;
            }
        }

        public enum TypeLogShow
        {
            Info,
            Error,
            Warning
        }
        public enum TypeLogger
        {
            Push,
            File
        }
    }
}
