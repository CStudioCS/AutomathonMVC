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
    }
}
