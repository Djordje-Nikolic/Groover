using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class TrackView : ReactiveUserControl<TrackViewModel>
    {
        public TrackView()
        {
            InitializeComponent();

            //this.PropertyChanged += (s, e) =>
            //{
            //    if (e.Property == Control.DataContextProperty)
            //    {
            //        if (e.NewValue != null)
            //            this.IsVisible = true;
            //        else
            //            this.IsVisible = false;
            //    }
            //};
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
