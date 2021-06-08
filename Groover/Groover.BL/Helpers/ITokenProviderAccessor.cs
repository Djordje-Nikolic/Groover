using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface ITokenProviderAccessor<TUser> where TUser : class
    {
        IUserTwoFactorTokenProvider<TUser> GetTokenProvider(string providerName);
    }
}
