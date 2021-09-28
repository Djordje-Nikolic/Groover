using Cassandra;
using Groover.ChatDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB
{
    internal class ChatDbCluster : IChatDbCluster
    {
        private readonly IChatDbConfiguration _configuration;

        private bool disposedValue;
        private readonly ICluster _cluster;

        public ICluster Cluster { get => _cluster; }
        public IChatDbConfiguration Configuration { get => _configuration; } 

        internal ChatDbCluster(ICluster cluster, IChatDbConfiguration configuration)
        {
            _configuration = configuration;
            _cluster = cluster;
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue)
            {
                if (Cluster != null)
                    await Cluster.ShutdownAsync();

                disposedValue = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (Cluster != null)
                        Cluster.Shutdown();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GroupChatCluster()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
