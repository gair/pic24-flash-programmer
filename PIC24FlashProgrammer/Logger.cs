using System.Diagnostics;
using System.Text;

namespace PIC24FlashProgrammer
{
    public class Logger
    {
        private const int SkipFrames = 2;
        private const int MaxLogCount = 20;
        private const int MaxLogSize = 0x800000;
        private const string LogFile = "Programmer.log";
        private static readonly Mutex mutex = new(false, "{66CA6C6F-0D44-434B-8154-11B88DC43C5E}");

        private enum LogType
        {
            Debug,
            Error,
            Info
        }

        public static string LogPath => GetPath(LogFile);

        public static string Directory => Application.LocalUserAppDataPath;

        public static void DebugEx(int skipFrames, string format, params object[] args)
        {
            Log(LogType.Debug, skipFrames, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            Log(LogType.Debug, SkipFrames, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Log(LogType.Error, SkipFrames, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Log(LogType.Info, SkipFrames, format, args);
        }

        public static string? Store(string data, string fileName)
        {
            mutex.WaitOne();
            try
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                var logPath = GetPath(fileName);
                var count = 0;
                while (File.Exists(logPath))
                {
                    fileName = string.Format("{0}-{1}{2}", name, ++count, ext);
                    logPath = GetPath(fileName);
                }

                File.WriteAllText(logPath, data, Encoding.UTF8);
                return logPath;
            }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, e.ToString());
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return null;
        }

        private static string GetPath(string fileName)
        {
            return Path.Combine(Directory, fileName);
        }

        private static void Log(LogType logType, int skipFrames, string format, params object[] args)
        {
            mutex.WaitOne();
            try
            {
                if (string.IsNullOrEmpty(format))
                {
                    return;
                }
                var message = format;
                if (args != null && args.Length > 0)
                {
                    message = string.Format(format, args);
                }
                var sf = new StackFrame(skipFrames, true);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var methodName = sf.GetMethod()?.Name ?? "Unknown";

                var content = $"{timestamp} {logType} [{Thread.CurrentThread.Name}] {methodName}, {Path.GetFileName(sf.GetFileName())} {sf.GetFileLineNumber()}: {message}";
                RotateLogs();
                File.AppendAllText(LogPath, content + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, e.ToString() + Environment.NewLine);
                File.AppendAllText(LogFile, $"Format: '{format}'{Environment.NewLine}");
                if (args != null && args.Length > 0)
                {
                    File.AppendAllText(LogFile, $"{args.Length} arg(s):{Environment.NewLine}");
                    foreach (var arg in args)
                    {
                        File.AppendAllText(LogFile, $"Arg: '{arg}'{Environment.NewLine}");
                    }
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static void RotateLogs()
        {
            try
            {
                var info = new FileInfo(LogPath);
                if (info.Exists && info.Length > MaxLogSize)
                {
                    for (var i = MaxLogCount - 1; i > 0; i--)
                    {
                        var srcPath = GetLogPath(i);
                        if (File.Exists(srcPath))
                        {
                            var destPath = GetLogPath(i + 1);
                            if (File.Exists(destPath))
                            {
                                File.Delete(destPath);
                            }

                            File.Move(srcPath, destPath);
                        }
                    }

                    File.Move(LogPath, GetLogPath(1));
                }
            }
            catch (Exception e)
            {
                File.AppendAllText(LogPath, e.ToString(), Encoding.UTF8);
            }
        }

        private static string GetLogPath(int num)
        {
            return GetPath(string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(LogFile), num, Path.GetExtension(LogFile)));
        }
    }
}
