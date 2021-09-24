using Cassandra;
using Groover.ChatDB.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    public class ChatDbClusterFactory : IChatDbClusterFactory
    {
        public void AddLoggerProvider(ILoggerProvider loggerProvider)
        {
            if (loggerProvider != null)
            {
                Diagnostics.AddLoggerProvider(loggerProvider);
            }
        }

        public IChatDbCluster CreateInstance(IChatDbConfiguration configuration)
        {
            ICluster cluster = Cluster.Builder()
                .AddContactPoint(configuration.ContactPointAddress)
                .Build();

            return new ChatDbCluster(cluster);
        }
    }
}
