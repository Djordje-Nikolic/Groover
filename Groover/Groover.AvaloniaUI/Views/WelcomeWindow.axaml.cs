using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.Services;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.Utils;
using Groover.AvaloniaUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Views
{
    public partial class WelcomeWindow : ReactiveWindow<WelcomeViewModel>
    {


        public WelcomeWindow()
        {
            AvaloniaXamlLoader.Load(this);

            LoginControl = this.FindControl<LoginView>("LoginControl");
            RegisterControl = this.FindControl<RegisterView>("RegisterControl");

            //LoginControl.SuccessfulLoginCommand = ReactiveCommand.CreateFromTask(execute: onSuccessfulLogin);
            //RegisterControl.SuccessfulRegisterCommand = ReactiveCommand.CreateFromTask(execute: onSuccessfulRegister);

            LoginControl.IsEnabled = true;

            this.WhenActivated(disposables => 
            {
                this.Closing += (s, e) => this.OnClosing(s, e);

                this.WhenAnyValue(v => v.ViewModel.CanClose)
                .Subscribe(x => OnCanClose(x))
                .DisposeWith(disposables);
            });
        }

        private readonly LoginView LoginControl;
        private readonly RegisterView RegisterControl;

        public bool LoginActivated { get; private set; }
        public bool RegisterActivated { get; private set; }

        private void onActivateLogin(object sender, RoutedEventArgs e)
        {
            LoginControl.IsEnabled = true;
            LoginActivated = true;
            RegisterActivated = false;
        }

        private void onActivateRegister(object sender, RoutedEventArgs e)
        {
            RegisterControl.IsEnabled = true;
            LoginActivated = false;
            RegisterActivated = true;
        }

        private void OnClosing(object? source, System.ComponentModel.CancelEventArgs e)
        {
            //Can't close if Login hasn't been completed. Change for this to prompt whether to close the whole application or smth.
            if (!ViewModel.CanClose)
            {
                e.Cancel = true;
            }
        }

        private void OnCanClose(bool canClose)
        {
            if (canClose)
            {
                WelcomeDialogResult res = new WelcomeDialogResult()
                {
                    AppViewModel = new AppViewModel(this.ViewModel.LoginViewModel?.Response, 
                                                    Locator.Current.GetRequiredService<IUserService>(),
                                                    Locator.Current.GetRequiredService<IGroupService>()),
                    ExitApp = !this.ViewModel.LoginViewModel?.LoggedInSuccessfully ?? true
                };
                this.Close(res);
            }
        }

        //private async Task onSuccessfulLogin()
        //{
            
        //}

        //private async Task onSuccessfulRegister()
        //{

        //}
    }
}
