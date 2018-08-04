using System;

namespace LightControl.Core
{
    // todo: improve this
    public static class Logger
    {
        public static void Log(Exception ex)
        {
            Console.WriteLine("EXCEPTION: " + ex.Message);
        }

        public static void Log(string message)
        {

        }
    }
}
