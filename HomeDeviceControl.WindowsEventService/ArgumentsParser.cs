using System;

namespace HomeDeviceControl.WindowsEventService
{
    public class ArgumentsParser
    {
        public Settings ParseArguments(string[] args)
        {
            if (args.Length != 2)
                throw new InvalidOperationException($"Expected two arguments. {GetUsage()}");

            return new Settings
            {
                ComputerDeviceId = ParseGuid(args[0]),
                ServerUrl = args[1]
            };
        }

        private Guid ParseGuid(string arg)
        {
            if (Guid.TryParse(arg, out Guid result))
                return result;
            throw new InvalidOperationException($"Could not parse guid: {arg}");
        }

        private string GetUsage()
        {
            return "Example: 7d115c0c-6181-4965-bceb-449781ecd27a http://192.168.1.125:8084";
        }
    }
}
