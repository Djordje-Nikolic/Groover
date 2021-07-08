using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Groover.AvaloniaUI.Views
{
    public partial class LoginView : ReactiveUserControl<LoginViewModel>
    {

        //public ReactiveCommand<Unit, Unit>? SuccessfulLoginCommand { get; set; }

        private ProgressBar _loginProgressBar; 

        public LoginView()
        {
            InitializeComponent();

            _loginProgressBar = this.FindControl<ProgressBar>("isLoggingIn");

            this.WhenActivated(disposables => 
            {
                //this.WhenAnyValue(v => v.ViewModel.LoggedInSuccessfully, logInSucc => logInSucc == true)
                //.Select(_ => System.Reactive.Unit.Default)
                //.InvokeCommand(SuccessfulLoginCommand)
                //.DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.Login.IsExecuting)
                .BindTo(this, x => x._loginProgressBar.IsVisible)
                .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
