using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AutoPortal.Libs
{
    public static class Functions
    {
        public static string ReplaceSpecials(string text)
        {
            return Regex.Replace(text, @"[^0-9a-zA-Z]+", GetRnadomChar().ToString());
        }

        private static char GetRnadomChar()
        {
            string charset = "abcdefghjklmnopqrstuvwxyz";
            charset += charset.ToUpper();
            charset += "0123456789";
            Random r = new Random();
            return charset[r.Next(0, charset.Length)];
        }

        public static void WriteLog(string text, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        {
            Log.LogMessageAsync(text, null, caller, file);
        }

        public static void WriteErrorLog(string text, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        {
            Log.ErrorLog(text, null, caller, file);
        }
    }
}
