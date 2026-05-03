using System;

namespace Automathon
{
    public static class Debug
    {
        public static void Log(params object[] logs)
        {
            string message = string.Join(";; ", logs);
            Console.WriteLine(message.ToString());
            UnityEngine.Debug.Log(message);
        }

        public static void LogError(params object[] logs)
        {
            string message = string.Join(";; ", logs);

            ConsoleColor previousForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message.ToString());

            Console.ForegroundColor = previousForegroundColor;

            UnityEngine.Debug.LogError(message);
        }
    }
}
