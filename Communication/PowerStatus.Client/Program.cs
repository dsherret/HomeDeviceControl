﻿using System;
using System.Threading.Tasks;

namespace HomeDeviceControl.Communication.PowerStatus.Client
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Error: Expected two arguments.");
                PrintUsage();
                return 1;
            }

            if (!Guid.TryParse(args[0], out Guid deviceId))
            {
                Console.WriteLine("Error: Expected first argument to be a GUID.");
                PrintUsage();
                return 1;
            }

            if (!bool.TryParse(args[1], out bool isPoweredOn))
            {
                Console.WriteLine("Error: Expected second argument to be a boolean.");
                PrintUsage();
                return 1;
            }

            // todo: don't hardcode this url
            using (var client = new ClientApi.Client("http://192.168.1.125:8084"))
                await client.UpdateDevicePowerStatus(deviceId, isPoweredOn);

            return 0;
        }

        static void PrintUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("=====");
            Console.WriteLine("Usage");
            Console.WriteLine("=====");
            Console.WriteLine("The application requires one guid argument for the device id and one boolean parameter to say the power status.");
            Console.WriteLine("Example: HomeDeviceControl.Communication.PowerStatus.Client.exe 09E9E275-5C73-4FD0-B44B-D6890B176B75 true");
        }
    }
}