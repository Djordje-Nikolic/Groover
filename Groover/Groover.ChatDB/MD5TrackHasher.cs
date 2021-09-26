using Groover.ChatDB.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    internal class MD5TrackHasher : ITrackHasher
    {
        [ThreadStatic]
        private static MD5CryptoServiceProvider _md5;

        private static MD5CryptoServiceProvider MD5Instance
        {
            get
            {
                if (_md5 == null)
                    _md5 = new MD5CryptoServiceProvider();

                return _md5;
            }
        }

        public string GenerateHash(byte[] trackBytes)
        {
            var hash = MD5Instance.ComputeHash(trackBytes);
            var hashString = HexStringFromBytes(hash);

            return hashString;
        }

        public bool ValidateHash(byte[] trackBytes, string hash)
        {
            var calcHash = GenerateHash(trackBytes);

            return hash == calcHash;
        }

        private string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }

            return sb.ToString();
        }
    }
}
