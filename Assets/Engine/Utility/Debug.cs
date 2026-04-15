using System;

namespace Automathon
{
    public static class Debug
    {
        public static void Log(params object[] logs)
        {
            foreach (object log in logs)
            {
                Console.WriteLine(log.ToString());
                UnityEngine.Debug.Log(log.ToString());
            }
        }
    }
}
