using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Groover.AvaloniaUI.Views
{
    public partial class RegisterView : ReactiveUserControl<RegisterViewModel>
    {
        //public ReactiveCommand<Unit, Unit>? SuccessfulRegisterCommand { get; set; }

        private ProgressBar _registerProgressBar;

        public RegisterView()
        {
            InitializeComponent();

            _registerProgressBar = this.FindControl<ProgressBar>("isRegister");

            this.WhenActivated(disposables =>
            {
                //this.WhenAnyValue(v => v.ViewModel.RegisteredSuccessfully, regSucc => regSucc == true)
                //.Select(_ => Unit.Default)
                //.InvokeCommand(SuccessfulRegisterCommand)
                //.DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.Register.IsExecuting)
                .BindTo(this, x => x._registerProgressBar.IsVisible)
                .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
