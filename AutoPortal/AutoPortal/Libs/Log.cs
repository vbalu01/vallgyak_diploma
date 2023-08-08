namespace AutoPortal.Libs
{
    public enum eLogType
    {
        NORMAL,
        ERROR,
        DEBUG,
        LOGIN,
    }
    public class Log
    {
        private static List<string> LogList = new List<string>();
        public Log()
        {
            Thread t = new Thread(ProcessVoid);
            t.Start();
        }
        public static void LogMessageAsync(string row, HttpContext obj, string caller = "", string file = "")
        {
            string baseText = "";
            if (file != null)
                baseText += file;
            if (caller != null)
                baseText += "::" + caller;
            if (obj != null)
                row += "\nIP:" + obj.Connection.RemoteIpAddress;
            string text = "\n~~~~~~~~~~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "::" + baseText + "~~~~~~~~~~\n" + row + "\n~~~~~~~~~~" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "~~~~~~~~~~\n";
            LogList.Add(text);
        }

        private static void ProcessVoid()
        {
            while (true)
            {
                string logItem = LogList.FirstOrDefault();
                if (string.IsNullOrEmpty(logItem))
                {
                    if (LogList.Capacity > 100)
                        GC.Collect();
                    Thread.Sleep(1000);
                }
                else
                {
                    File.AppendAllText("Log.txt", logItem);
                    LogList.Remove(logItem);
                    if (LogList.Count * 2 <= LogList.Capacity)
                        GC.Collect();
                }
            }
        }
    }

    public class ActionLog : Attribute
    {
        public bool LogParams { get; }
        public ActionLog(bool LogParams = false)
        {
            this.LogParams = LogParams;
        }
    }
}
