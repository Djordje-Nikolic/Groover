using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;
using ReactiveUI;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class InputView : ReactiveUserControl<InputViewModel>
    {
        private TextBox _textControl;
        private ReactiveCommand<int, Unit> _moveInputCaretCommand { get; }

        public InputView()
        {
            InitializeComponent();

            _moveInputCaretCommand = ReactiveCommand.Create<int>(MoveTextInputCaret);
            _textControl = this.FindControl<TextBox>("textControl");

            this.WhenActivated(disposables =>
            {
                ViewModel?.NewLineCommand.InvokeCommand(_moveInputCaretCommand);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MoveTextInputCaret(int newIndex)
        {
            if (_textControl != null)
                _textControl.CaretIndex = newIndex;
        }
    }
}
