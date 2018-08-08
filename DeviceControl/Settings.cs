using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DeviceControl
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        private Settings()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static Settings Default { get; } = new Settings();

        public double Latitude => double.Parse(_configuration.GetSection("General")["Latitude"]);
        public double Longitude => double.Parse(_configuration.GetSection("General")["Longitude"]);

        public string ServerHostname => _configuration.GetSection("General")["ServerHostname"];
        public int ServerPort => int.Parse(_configuration.GetSection("General")["ServerPort"]);
        public string ZWavePort => _configuration.GetSection("General")["ZWavePort"];

        public Guid SunroomComputerId => Guid.Parse(_configuration.GetSection("Sunroom")["ComputerId"]);
        public Guid SunroomLightId => Guid.Parse(_configuration.GetSection("Sunroom")["LightId"]);

        /// <summary>
        /// Sunroom's luminance level to be considered dark.
        /// </summary>
        public int SunroomMotionDarkLuminance => int.Parse(_configuration.GetSection("Sunroom")["DarkLuminance"]);

        /// <summary>
        /// Sun's max altitude for the room to be considered dark.
        /// </summary>
        public double SunroomMotionDarkSunAltitude => double.Parse(_configuration.GetSection("Sunroom")["DarkSunAltitude"]);

        /// <summary>
        /// Number of seconds to dim the sunroom for after detecting motion when it's dark and the computer is off.
        /// </summary>
        public int SunroomMotionDimmedSeconds => int.Parse(_configuration.GetSection("Sunroom")["MotionDimmedSeconds"]);

        /// <summary>
        /// Brightness to dim the sunroom with after detecting motion when it's dark and the computer is off.
        /// </summary>
        public int SunroomMotionDimmedBrightness => int.Parse(_configuration.GetSection("Sunroom")["MotionDimmedBrightness"]);

        /// <summary>
        /// Node id for the sunroom's luminance/motion/temperature/humidity sensor.
        /// </summary>
        public int SunroomZWaveZoozNodeId => int.Parse(_configuration.GetSection("Sunroom")["ZWaveZoozNodeId"]);
    }
}
