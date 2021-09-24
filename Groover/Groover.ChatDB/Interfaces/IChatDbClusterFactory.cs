using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IChatDbClusterFactory
    {
        void AddLoggerProvider(ILoggerProvider loggerProvider);
        IChatDbCluster CreateInstance(IChatDbConfiguration configuration);
    }
}
