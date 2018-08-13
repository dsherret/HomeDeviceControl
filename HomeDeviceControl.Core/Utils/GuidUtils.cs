using System;
using System.Security.Cryptography;
using System.Text;

namespace HomeDeviceControl.Core.Utils
{
    public class GuidUtils
    {
        /// <summary>
        /// Converts an input string into a deterministic GUID.
        /// </summary>
        /// <param name="input">Input string.</param>
        public static Guid StringToGuid(string input)
        {
            // Source: https://gist.github.com/TonyMilton/b6b7f45b571d2ecc3eaacae8e4f6046c
            var provider = new MD5CryptoServiceProvider();
            return new Guid(provider.ComputeHash(Encoding.Default.GetBytes(input)));
        }
    }
}
