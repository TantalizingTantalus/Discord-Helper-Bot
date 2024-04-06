using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordHelperBot
{
    /******************************************************************** BEGIN LOGGING CLASS DEFINITIONS **************************************************************************/

    public static class Logger
    {
        private static ILogger Log;
        static Logger()
        {
            using ILoggerFactory Factory = LoggerFactory.Create(builder => builder.AddConsole());

            Log = Factory.CreateLogger<Program>();
        }

        public static void LogInformation(string Message)
        {
            Log.LogInformation(Message);
            return;
        }

        public static void LogInformation(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {
            Message = ProcessMessage(Message, Param1, Param2, Param3, Param4, Param5);
            Log.LogInformation(Message);
            return;
        }

        public static void LogError(string Message)
        {
            Log.LogError(Message);
            return;
        }

        public static void LogError(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {
            Message = ProcessMessage(Message, Param1, Param2, Param3, Param4, Param5);
            Log.LogError(Message);
            return;
        }

        public static string ProcessMessage(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {

            List<string> Parameters = new List<string>();

            if (Param1 != null)
            {
                Parameters.Add((string)Param1);
            }

            if (Param2 != null)
            {
                Parameters.Add((string)Param2);
            }

            if (Param3 != null)
            {
                Parameters.Add((string)Param3);
            }

            if (Param4 != null)
            {
                Parameters.Add((string)Param4);
            }

            if (Param5 != null)
            {
                Parameters.Add((string)Param5);
            }

            string ConcPattern = @"(\{([^}]+)\})";
            Match ConcMatch = Regex.Match(Message, ConcPattern);

            while (ConcMatch.Success)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    Message = Message.Replace(ConcMatch.Groups[0].Value, Parameters[i]);
                    ConcMatch = Regex.Match(Message, ConcPattern);
                }
            }

            return Message;
        }
    }

    /******************************************************************** END LOGGING CLASS DEFINITIONS **************************************************************************/
}
