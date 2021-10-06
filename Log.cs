using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace TheLogger
{
    public enum LogType
    {
        Critical = 1,
        Error = 2,
        Info = 3,
        Warning = 4,
        Debug = 5,
    }

    public enum AppType
    {
        None = 0,
        Console = 1
    }

    /// <summary>
    /// Log
    /// * Ever to file
    /// * Sometimes to debug console
    /// * Sometimes to console
    /// </summary>
    public class Log
    {
        private const string stringLog = "[{0}] [{1}] {2}";
        private static bool _forceCloseOnError = false;

        private static string message = string.Empty;
        private static string dateTimeToLog { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

        public static LogType LogLevel { get; private set; } = LogType.Info;
        public static AppType AppType { get; private set; } = AppType.None;

        public static string FileName { get; private set; } = "log.txt";
        public static string FilePath { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;

        public static Log Er { get { return new Log(); } }

        /// <summary>
        /// Config how Logger will work
        /// </summary>
        /// <param name="fileName">log.txt</param>
        /// <param name="filePath">Environment.CurrentDirectory</param>
        /// <param name="logLevel">Warning</param>
        /// <param name="appType">None</param>
        public static void Setup(string fileName, string filePath, LogType logLevel, AppType appType, bool forceCloseOnError)
        {
            _forceCloseOnError = forceCloseOnError;
            Log.FilePath = filePath;
            Log.FileName = fileName;
            Log.LogLevel = logLevel;
            Log.AppType = appType;
            SetupWrite();
        }
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static void SetupWrite()
        {
            try { File.Delete(FileName); } catch { }
            StringBuilder sb = new StringBuilder().Append(Environment.NewLine);
            sb.Append("### LOGGER SETUP #####################").Append(Environment.NewLine);
            sb.AppendFormat("# FileName: {0}", Log.FileName).Append(Environment.NewLine);
            sb.AppendFormat("# LogLevel: {0}", Log.LogLevel).Append(Environment.NewLine);
            sb.AppendFormat("# AppType: {0}", Log.AppType).Append(Environment.NewLine);
            sb.AppendFormat("# Usuário atual {0}", (IsAdministrator() ? "é administrador" : "não é administrador")).Append(Environment.NewLine);
            sb.Append("######################################").Append(Environment.NewLine);
            Write(sb.ToString());
        }

        #region Log by Type
        public static void Critical(string message)
        {
            Write(LogType.Critical, message);
        }
        public static void Error(string message)
        {
            Write(LogType.Error, message);
        }
        public static void Info(string message)
        {
            Write(LogType.Info, message);
        }
        public static void Debug(string message)
        {
            Write(LogType.Debug, message);
        }
        public static void Warning(string message)
        {
            Write(LogType.Warning, message);
        }
        #endregion

        #region Read log
        public static string[] Tail(System.IO.StreamReader reader, int lineCount)
        {
            var buffer = new List<string>(lineCount);
            string line;
            for (int i = 0; i < lineCount; i++)
            {
                line = reader.ReadLine();
                if (line == null) return buffer.ToArray();
                buffer.Add(line);
            }

            int lastLine = lineCount - 1;

            while (null != (line = reader.ReadLine()))
            {
                lastLine++;
                if (lastLine == lineCount) lastLine = 0;
                buffer[lastLine] = line;
            }

            if (lastLine == lineCount - 1) return buffer.ToArray();
            var retVal = new string[lineCount];
            buffer.CopyTo(lastLine + 1, retVal, 0, lineCount - lastLine - 1);
            buffer.CopyTo(0, retVal, lineCount - lastLine - 1, lastLine + 1);
            return retVal;
        }
        public static string Read()
        {
            return Read(0);
        }
        public static string Read(int lines)
        {
            var result = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(FilePath + "\\" + FileName, Encoding.Default))
                {
                    if (lines == 0)
                    {
                        result = sr.ReadToEnd();
                    }
                    else
                    {
                        result = string.Join(Environment.NewLine, Tail(sr, lines));
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogType.Critical, ex.Message);
                return result;
            }
            return result;
        }
        #endregion

        #region Write in public mode
        /// <summary>
        /// Log simple message
        /// </summary>
        /// <param name="message"></param>
        public static void Write(string message)
        {
            Write(LogType.Info, message);
        }

        /// <summary>
        /// Log in custom message format
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteFormat(LogType type, string format, params string[] args)
        {
            Write(type, string.Format(format, args));
        }

        /// <summary>
        /// Log a exception
        /// </summary>
        /// <param name="exception"></param>
        public static void Write(Exception exception)
        {
            Write(LogType.Critical, exception.Message);
            Write(LogType.Debug, exception.StackTrace);
            if (_forceCloseOnError)
            {
                Write(LogType.Critical, "Application exit code 99");
                Environment.Exit(99);
            }
        }

        /// <summary>
        /// Log type and message
        /// </summary>
        /// <param name="type"></param>
        /// <param name="user_message"></param>
        public static void Write(LogType type, string user_message)
        {
            message = string.Format(stringLog, dateTimeToLog, type.ToString(), user_message);

            if (Convert.ToInt16(type) <= Convert.ToInt16(LogLevel)) { write(); }

            switch (type)
            {
                case LogType.Critical:
                case LogType.Error:
                case LogType.Debug:
                    writeToDebugConsole();
                    break;
            }
        }
        #endregion

        #region Write in private mode
        /// <summary>
        /// Log to file
        /// </summary>
        /// <param name="message"></param>
        private static void write()
        {
            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(FilePath + "\\" + FileName, true, Encoding.Default))
                {
                    sw.WriteLine(message);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                if (Log._forceCloseOnError) throw new Exception(string.Format("Falha ao acessar caminho {0}\\{1}. ({2})", FilePath, FileName, ex.Message));
            }
        }

        /// <summary>
        /// Log to Debug Console
        /// </summary>
        /// <param name="message"></param>
        private static void writeToDebugConsole()
        {
            System.Diagnostics.Debug.WriteLine(message);
            writeToConsole();
        }

        /// <summary>
        /// Log to console
        /// </summary>
        /// <param name="message"></param>
        private static void writeToConsole()
        {
            if (AppType == AppType.Console)
            {
                System.Console.WriteLine(message);
            }
        }
        #endregion
    }
}