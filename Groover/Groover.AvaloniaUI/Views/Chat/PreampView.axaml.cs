using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class PreampView : ReactiveUserControl<PreampViewModel>
    {
        public PreampView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
