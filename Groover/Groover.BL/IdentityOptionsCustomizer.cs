using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL
{
	public class IdentityOptionsCustomizer
	{//https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-3.1

		public static IdentityOptions Customize(IdentityOptions options)
		{
			CustomizePasswordOptions(options);
			CustomizeSignInOptions(options);
			CustomizeStoresOptions(options);
			CustomizeTokensOptions(options);
			CustomizeUserOptions(options);
			CustomizeClaimsOptions(options);
			CustomizeLockoutOptions(options);

			return options;
		}

		private static IdentityOptions CustomizePasswordOptions(IdentityOptions options)
		{
			var passwordOptions = options.Password;

			passwordOptions.RequireDigit = false;
			passwordOptions.RequiredLength = 8;
			passwordOptions.RequireLowercase = false;
			passwordOptions.RequireNonAlphanumeric = false;
			passwordOptions.RequireUppercase = false;
			passwordOptions.RequiredUniqueChars = 1;

			return options;
		}

		private static IdentityOptions CustomizeSignInOptions(IdentityOptions options)
		{
			var signInOptions = options.SignIn;

			signInOptions.RequireConfirmedAccount = false;
			signInOptions.RequireConfirmedEmail = true;
			signInOptions.RequireConfirmedPhoneNumber = false;

			return options;
		}

		private static IdentityOptions CustomizeStoresOptions(IdentityOptions options)
		{
			var storesOptions = options.Stores;

			storesOptions.MaxLengthForKeys = -1;
			storesOptions.ProtectPersonalData = false;

			return options;
		}

		private static IdentityOptions CustomizeTokensOptions(IdentityOptions options)
		{
			var tokensOptions = options.Tokens;

			tokensOptions.AuthenticatorIssuer = tokensOptions.AuthenticatorIssuer;
			tokensOptions.AuthenticatorTokenProvider = tokensOptions.AuthenticatorTokenProvider;
			tokensOptions.ChangeEmailTokenProvider = tokensOptions.ChangeEmailTokenProvider;
			tokensOptions.ChangePhoneNumberTokenProvider = tokensOptions.ChangePhoneNumberTokenProvider;
			tokensOptions.EmailConfirmationTokenProvider = "emailconfirmation";
			tokensOptions.PasswordResetTokenProvider = tokensOptions.PasswordResetTokenProvider;
			tokensOptions.ProviderMap = tokensOptions.ProviderMap;

			return options;
		}

		private static IdentityOptions CustomizeUserOptions(IdentityOptions options)
		{
			var userOptions = options.User;

			userOptions.AllowedUserNameCharacters = userOptions.AllowedUserNameCharacters;
			userOptions.RequireUniqueEmail = true;

			return options;
		}

		private static IdentityOptions CustomizeLockoutOptions(IdentityOptions options)
		{
			var lockoutOptions = options.Lockout;

			lockoutOptions.AllowedForNewUsers = true;
			lockoutOptions.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			lockoutOptions.MaxFailedAccessAttempts = 5;

			return options;
		}

		private static IdentityOptions CustomizeClaimsOptions(IdentityOptions options)
		{
			var claimsOptions = options.ClaimsIdentity;

			claimsOptions.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
			claimsOptions.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";
			claimsOptions.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier;
			claimsOptions.UserNameClaimType = System.Security.Claims.ClaimTypes.Name;

			return options;
		}
	}
}
