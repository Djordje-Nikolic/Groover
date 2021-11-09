using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public record UserGroupPair(int userId, int groupId);

    public class OnlineStatusManager : IOnlineStatusManager
    {
        private Dictionary<UserGroupPair, uint> UserConnectedDevicesCache;

        public OnlineStatusManager()
        {
            UserConnectedDevicesCache = new Dictionary<UserGroupPair, uint>();
        }

        public bool LoggedOn(int userId, int groupId)
        {
            UserGroupPair key = new UserGroupPair(userId, groupId);
            if (UserConnectedDevicesCache.TryGetValue(key, out uint connectedDevices))
            {
                UserConnectedDevicesCache[key] = connectedDevices + 1;
            }
            else
            {
                UserConnectedDevicesCache.Add(new UserGroupPair(userId, groupId), 1);
            }

            return true;
        }

        public bool Disconnected(int userId, int groupId)
        {
            UserGroupPair key = new UserGroupPair(userId, groupId);
            if (UserConnectedDevicesCache.TryGetValue(key, out uint connectedDevices))
            {
                var newVal = connectedDevices > 0 ? connectedDevices - 1 : 0;
                UserConnectedDevicesCache[key] = newVal;

                return newVal != 0;
            }
            else
            {
                return false;
            }
        }

        public bool GetUserStatuc(int userId, int groupId)
        {
            UserGroupPair key = new UserGroupPair(userId, groupId);
            if (UserConnectedDevicesCache.TryGetValue(key, out uint connectedDevices))
            {
                return connectedDevices != 0;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            UserConnectedDevicesCache.Clear();
        }
    }
}
