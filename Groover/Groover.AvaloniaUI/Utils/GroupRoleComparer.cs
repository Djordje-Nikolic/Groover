using Groover.AvaloniaUI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class GroupRoleComparer : IComparer<GroupUser>
    {
        public int Compare(GroupUser? x, GroupUser? y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(x.GroupRole) && string.IsNullOrWhiteSpace(y.GroupRole))
                return 0;

            if (string.IsNullOrWhiteSpace(x.GroupRole))
                return -1;

            if (string.IsNullOrWhiteSpace(y.GroupRole))
                return 1;

            if (x.GroupRole == "Admin" && y.GroupRole != "Admin")
                return 10;

            if (y.GroupRole == "Admin" && x.GroupRole != "Admin")
                return -10;

            return 0;
        }
    }
}
