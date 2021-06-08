using Groover.DB.MySqlDb.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public class TokenProviderAccessor<TUser> : ITokenProviderAccessor<TUser> where TUser : class
    {
        private readonly IdentityOptions _options;
        private readonly IServiceProvider _services;
        private readonly ILogger<ITokenProviderAccessor<TUser>> _logger;
        private readonly Dictionary<string, IUserTwoFactorTokenProvider<TUser>> _tokenProviders =
              new Dictionary<string, IUserTwoFactorTokenProvider<TUser>>();

        public TokenProviderAccessor(IOptions<IdentityOptions> optionsAccessor,
            IServiceProvider services,
            ILogger<ITokenProviderAccessor<TUser>> logger)
        {
            _options = optionsAccessor.Value;
            _services = services;
            _logger = logger;

            foreach (var providerName in _options.Tokens.ProviderMap.Keys)
            {
                var description = _options.Tokens.ProviderMap[providerName];

                var provider = (description.ProviderInstance ?? _services.GetRequiredService(description.ProviderType))
                    as IUserTwoFactorTokenProvider<TUser>;
                if (provider != null)
                {
                    RegisterTokenProvider(providerName, provider);
                }
            }
        }

        public IUserTwoFactorTokenProvider<TUser> GetTokenProvider(string providerName)
        {
            return _tokenProviders.GetValueOrDefault(providerName);
        }

        private void RegisterTokenProvider(string providerName, IUserTwoFactorTokenProvider<TUser> provider)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentNullException("Provider name is null or whitespace.");

            _tokenProviders.Add(providerName, provider);
        }
    }
}
