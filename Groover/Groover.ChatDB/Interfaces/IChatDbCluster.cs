using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.ChatDB.Interfaces
{
    public interface IChatDbCluster : IDisposable, IAsyncDisposable
    {
        public Cassandra.ICluster Cluster { get; }
    }
}
