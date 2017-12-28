﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum LogType
{
    Critical = 1,
    Error = 2,
    Info = 3,
    Debug = 4,
    Warning = 5
}

public enum AppType
{
    Console,
    None
}

/// <summary>
/// Log
/// * Ever to file
/// * Sometimes to debug console
/// * Sometimes to console
/// </summary>
public class Log
{
    private static LogType logLevel = LogType.Info;
    private static AppType appType = AppType.None;
    private static string message = string.Empty;
    private const string stringLog = "[{0}] [{1}] {2}";
    private const string fileName = "log.txt";
    private static string dateTimeToLog { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

    public static LogType LogLevel { get { return logLevel; } set { logLevel = value; } }

    public static void Critical(String message)
    {
        Write(LogType.Critical, message);
    }
    public static void Debug(String message)
    {
        Write(LogType.Debug, message);
    }

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
    public static string Load()
    {
        return Load(0);
    }
    public static string Load(int lines)
    {
        var result = string.Empty;
        try
        {
            var path = Environment.CurrentDirectory;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path + "\\" + fileName, Encoding.Default))
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

    /// <summary>
    /// Log simple message
    /// </summary>
    /// <param name="message"></param>
    public static void Write(String message)
    {
        Write(LogType.Info, message);
    }

    /// <summary>
    /// Log a exception
    /// </summary>
    /// <param name="exception"></param>
    public static void Write(Exception exception)
    {
        Write(LogType.Critical, exception.Message);
        Write(LogType.Debug, exception.StackTrace);
        Environment.Exit(0);
    }

    /// <summary>
    /// Log type and message
    /// </summary>
    /// <param name="type"></param>
    /// <param name="user_message"></param>
    public static void Write(LogType type, String user_message)
    {
        message = String.Format(stringLog, dateTimeToLog, type.ToString(), user_message);

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

    /// <summary>
    /// Log to file
    /// </summary>
    /// <param name="message"></param>
    private static void write()
    {
        var path = Environment.CurrentDirectory;
        try
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path + "\\" + fileName, true, Encoding.Default))
            {
                sw.WriteLine(message);
                sw.Flush();
                sw.Close();
            }
        }
        catch (Exception ex)
        {
            System.Threading.Thread.Sleep(2);
            write();
            Write(LogType.Critical, ex.Message);
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
        if (appType == AppType.Console)
        {
            System.Console.WriteLine(message);
        }
    }
}
