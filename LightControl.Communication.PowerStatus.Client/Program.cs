using System;

namespace LightControl.Communication.PowerStatus.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("Requires one argument boolean indicating the power status.");

            if (!bool.TryParse(args[0], out bool result))
                throw new InvalidOperationException($"Could not parse argument to bool: {args[0]}");


        }
    }
}
