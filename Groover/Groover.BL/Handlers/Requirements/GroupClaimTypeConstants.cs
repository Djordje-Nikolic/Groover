using Groover.IdentityDB.MySqlDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Handlers.Requirements
{
    public static class GroupClaimTypeConstants
    {
        public const string Admin = "group_admin";
        public const string Member = "group_member";

        public static string GetConstant(GroupRole groupRole)
        {
            switch (groupRole)
            {
                case GroupRole.Admin:
                    return Admin;
                case GroupRole.Member:
                    return Member;
                default:
                    return null;
            }
        }
    }
}
