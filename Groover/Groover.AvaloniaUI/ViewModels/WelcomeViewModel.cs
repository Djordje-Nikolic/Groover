using AutoMapper;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class WelcomeViewModel : ViewModelBase
    {
        private readonly IMapper _mapper;

        [Reactive]
        public LoginViewModel LoginViewModel { get; private set; }

        [Reactive]
        public RegisterViewModel RegisterViewModel { get; private set; }

        public ReactiveCommand<Unit,Unit> OverrideCloseCommand { get; }

        [ObservableAsProperty]
        public bool CanClose { get; }

        public WelcomeViewModel()
        {
            _mapper = DIContainer.GetRequiredService<IMapper>(Locator.Current);

            var loginViewModel = DIContainer.GetRequiredService<LoginViewModel>(Locator.Current);
            var registerViewModel = DIContainer.GetRequiredService<RegisterViewModel>(Locator.Current);
            LoginViewModel = loginViewModel;
            RegisterViewModel = registerViewModel;

            var helperCommand = ReactiveCommand.Create<Unit, bool>(x => true);
            helperCommand.ToPropertyEx(this, x => x.CanClose);

            OverrideCloseCommand = ReactiveCommand.Create<Unit, Unit>(x => x);
            OverrideCloseCommand.InvokeCommand(helperCommand);

            this.WhenAnyValue(x => x.LoginViewModel.LoggedInSuccessfully)
                .Where(val => val == true)
                .Select(val => new Unit())
                .Delay(TimeSpan.FromSeconds(3))
                .InvokeCommand(helperCommand);
        }

        public AppViewModel GenerateAppViewModel()
        {
            UserViewModel userViewModel = this._mapper.Map<UserViewModel>(LoginViewModel?.Response?.User);
            return new AppViewModel(userViewModel,
                                    DIContainer.GetRequiredService<IUserService>(Locator.Current),
                                    DIContainer.GetRequiredService<IGroupService>(Locator.Current),
                                    DIContainer.GetRequiredService<IGroupChatService>(Locator.Current),
                                    _mapper,
                                    DIContainer.GetRequiredService<UserConstants>(Locator.Current));

        }
    }
}
