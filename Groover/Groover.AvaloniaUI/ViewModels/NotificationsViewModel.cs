using DynamicData;
using DynamicData.Binding;
using Groover.AvaloniaUI.ViewModels.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        [ObservableAsProperty]
        public bool HasUnread { get; }

        private SourceCache<NotificationViewModel, Guid> _notificationsCache;
        private ReadOnlyObservableCollection<NotificationViewModel> _notifications;
        public ReadOnlyObservableCollection<NotificationViewModel> Notifications => _notifications;

        public ReactiveCommand<NotificationViewModel, Unit> ClickCommand { get; }

        [ObservableAsProperty]
        public Interaction<NotificationViewModel, NotificationViewModel?> ShowNotificationDialog { get; }

        public NotificationsViewModel()
        {
            _notificationsCache = new SourceCache<NotificationViewModel, Guid>(nVm => nVm.Id);
            _notificationsCache.Connect()
                .Sort(SortExpressionComparer<NotificationViewModel>.Ascending(vm => vm.CreatedAt))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _notifications)
                .Subscribe();

            Notifications.ToObservableChangeSet()
                .AutoRefresh(notif => notif.Read)
                .ToCollection()                      // Get the new collection of items
                .Select(x => !x.All(y => y.Read))
                .ToPropertyEx(this, nsvm => nsvm.HasUnread);

            ClickCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(OnClick);
        }

        public NotificationsViewModel(IEnumerable<NotificationViewModel> notificationViewModels) : this()
        {
            _notificationsCache.AddOrUpdate(notificationViewModels);
        }

        public void AddNotification(NotificationViewModel notificationViewModel)
        {
            _notificationsCache.AddOrUpdate(notificationViewModel);
        }

        private async Task OnClick(NotificationViewModel notificationViewModel)
        {
            var result = await ShowNotificationDialog.Handle(notificationViewModel);

            if (result != null)
                _notificationsCache.AddOrUpdate(result);

            notificationViewModel.Read = true;
        }
    }
}
