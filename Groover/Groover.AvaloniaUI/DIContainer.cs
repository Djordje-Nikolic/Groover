using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Interfaces;
using Groover.AvaloniaUI.Services;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.ViewModels;
using Splat;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using AutoMapper;

namespace Groover.AvaloniaUI
{
    internal static class DIContainer
    {

        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            var apiConfig = new ApiConfiguration(ConfigurationManager.GetSection("apiSettings") as NameValueCollection);
            services.RegisterConstant<IApiConfiguration>(apiConfig);

            var identityConfig = new UserConstants(ConfigurationManager.GetSection("identitySettings") as NameValueCollection);
            services.RegisterConstant<UserConstants>(identityConfig);

            var imageConfig = new ImageConstants(ConfigurationManager.GetSection("imageSettings") as NameValueCollection);
            services.RegisterConstant<ImageConstants>(imageConfig);

            var cacheConfig = new CacheConfiguration(ConfigurationManager.GetSection("cacheSettings") as NameValueCollection);
            services.RegisterConstant<ICacheConfiguration>(cacheConfig);

            services.RegisterLazySingleton<IApiService>(() => new ApiService(
                resolver.GetRequiredService<IApiConfiguration>()));

            services.RegisterLazySingleton<IChatHubService>(() => new ChatHubService(
                resolver.GetRequiredService<IApiService>()));

            services.RegisterLazySingleton<ICacheWrapper>(() => new CacheWrapper(
                resolver.GetRequiredService<ICacheConfiguration>()));

            //Make into singleton?
            services.Register<IGroupService>(() => new GroupService(
                resolver.GetRequiredService<IApiService>(),
                resolver.GetRequiredService<ICacheWrapper>()));

            services.Register<IGroupChatService>(() => new GroupChatService(
                resolver.GetRequiredService<IApiService>(),
                resolver.GetRequiredService<ICacheWrapper>()));

            //Make into singleton?
            services.Register<IUserService>(() => new UserService(
                resolver.GetRequiredService<IApiService>(),
                resolver.GetRequiredService<ICacheWrapper>()));

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            services.Register<IMapper>(() => new Mapper(config));

            //ViewModels
            services.Register<MainWindowViewModel>(() => new MainWindowViewModel());
            services.Register<WelcomeViewModel>(() => new WelcomeViewModel());
            services.Register<LoginViewModel>(() => new LoginViewModel(
                resolver.GetRequiredService<IUserService>(),
                resolver.GetRequiredService<UserConstants>()));
            services.Register<RegisterViewModel>(() => new RegisterViewModel(
                resolver.GetRequiredService<IUserService>(),
                resolver.GetRequiredService<UserConstants>()));
        }

        public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
        {
            var service = resolver.GetService<TService>();
            if (service is null) // Splat is not able to resolve type for us
            {
                throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}"); // throw error with detailed description
            }

            return service; // return instance if not null
        }
    }
}
