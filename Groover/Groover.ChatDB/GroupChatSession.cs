using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Groover.ChatDB.Interfaces;

namespace Groover.ChatDB
{
    public class GroupChatSession : IGroupChatSession
    {
        private readonly IChatDbCluster _groupChatCluster;
        private readonly ISession _session;

        public ISession Session { get => _session; }
        public IChatDbConfiguration Configuration { get => _groupChatCluster.Configuration; }

        public GroupChatSession(IChatDbCluster cluster)
        {
            _groupChatCluster = cluster;
            _session = cluster.Cluster.Connect(cluster.Configuration.GroupChatKeySpace);
        }
    }
}
