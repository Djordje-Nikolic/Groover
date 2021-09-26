using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    internal interface ITrackHasher
    {
        string GenerateHash(byte[] trackBytes);
        bool ValidateHash(byte[] trackBytes, string hash);
    }
}
