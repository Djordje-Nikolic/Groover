﻿using Groover.BL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface IImageProcessor
    {
        Task<byte[]> CheckAsync(byte[] imageBytes, bool failOnNull = false);
        Task<byte[]> CheckAsync(MemoryStream ms);
    }
}
