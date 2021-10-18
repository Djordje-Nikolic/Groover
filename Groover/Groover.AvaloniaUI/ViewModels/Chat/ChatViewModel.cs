using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.Utils;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class ChatViewModel : ViewModelBase, IDisposable
    {
        private readonly IGroupChatService _groupChatService;
        private readonly IChatHubService _chatHubService;
        private readonly IVLCWrapper _vlcWrapper;
        private DispatcherTimer _newMessageTimer;
        private PageParams _pageParams;
        private bool _initialized;
        private bool _disposedValue;
        private bool _prioritiseHub;
        private SourceCache<Message, string> _messageCache;
        private ReadOnlyObservableCollection<MessageViewModel> _sortedMessages;
        public ReadOnlyObservableCollection<MessageViewModel> SortedMessages => _sortedMessages;

        [Reactive]
        public bool HasNewMessages { get; private set; }
        [Reactive]
        public string? DisplayError { get; private set; } //Simple error display
        [Reactive]
        public UserViewModel User { get; private set; }
        [Reactive]
        public UserGroupViewModel UserGroup { get; private set; }
        public InputViewModel InputViewModel { get; private set; }

        public ReactiveCommand<Unit, Unit> InitializeCommand { get; }
        public ReactiveCommand<Message, Unit> AddMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> GetMoreMessagesCommand { get; }
        public ReactiveCommand<Unit, Unit> ReadAllMessagesCommand { get; }

        public ChatViewModel(UserViewModel loggedInUser,
            UserGroupViewModel userGroup,
            IGroupChatService groupChatService,
            IChatHubService chatHubService,
            Interaction<ChooseImageDialogViewModel, string?> chooseImageInteraction,
            Interaction<ChooseTrackDialogViewModel, ChooseTrackResult?> chooseTrackInteraction,
            bool prioritiseSendingThroughHub = false)
        {
            _groupChatService = groupChatService;
            _chatHubService = chatHubService;
            _prioritiseHub = prioritiseSendingThroughHub;
            _vlcWrapper = DIContainer.GetRequiredService<IVLCWrapper>(Locator.Current);
            User = loggedInUser;
            UserGroup = userGroup;

            InitializeCommand = ReactiveCommand.CreateFromTask(Initialize);
            AddMessageCommand = ReactiveCommand.Create<Message>(AddNewMessage);
            GetMoreMessagesCommand = ReactiveCommand.CreateFromTask(GetMoreMessages);
            ReadAllMessagesCommand = ReactiveCommand.Create(ReadMessages);

            _messageCache = new SourceCache<Message, string>(msg => msg.Id);
            _messageCache.Connect()
                .TransformWithInlineUpdate(msg => GenerateMessageViewModel(msg), (prevViewModel, newMsg) =>
                {
                    //Do update
                })
                .Sort(SortExpressionComparer<MessageViewModel>.Ascending(mVm => mVm.CreatedAt))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _sortedMessages)
                .DisposeMany()
                .Subscribe();

            //This processes new messages and sets group flags depending on their position in the list
            _sortedMessages.ToObservableChangeSet()
                .WhereReasonsAre(ListChangeReason.Add, ListChangeReason.AddRange)
                .DeferUntilLoaded()
                .Do(_ => this.HasNewMessages = true)
                .DisposeMany()
                .Subscribe(chngSet =>
                {
                    foreach (var change in chngSet)
                    {
                        if (change.Type == ChangeType.Item)
                        {
                            var item = change.Item;
                            var currentViewModel = item.Current;
                            int currentIndex = item.CurrentIndex >= 0 ? item.CurrentIndex : _sortedMessages.IndexOf(currentViewModel); //FIGURE OUT HOW TO GET INDEXES TO LOAD
                            int prevInd = currentIndex - 1;
                            int nextInd = currentIndex + 1;

                            var prevViewModel = _sortedMessages.ElementAtOrDefault(prevInd);
                            var nextViewModel = _sortedMessages.ElementAtOrDefault(nextInd);
                            SetGroupFlags(prevViewModel, currentViewModel, nextViewModel);
                        }
                        else if (change.Type == ChangeType.Range)
                        {
                            var items = change.Range;
                            var currentViewModels = items.ToList();
                            int currentIndex = items.Index >= 0 ? items.Index : _sortedMessages.IndexOf(currentViewModels[0]); //FIGURE OUT HOW TO GET INDEXES TO LOAD
                            int prevInd = currentIndex - 1;
                            int nextInd = currentIndex + items.Count;

                            var prevViewModel = _sortedMessages.ElementAtOrDefault(prevInd);
                            var nextViewModel = _sortedMessages.ElementAtOrDefault(nextInd);
                            SetGroupFlags(prevViewModel, currentViewModels, nextViewModel);
                        }
                    }
                });

            _newMessageTimer = new DispatcherTimer(TimeSpan.FromMinutes(5),
                DispatcherPriority.Background,
                async (o, e) => await CheckForNewMessages());

            _pageParams = new PageParams()
            {
                PageSize = 30,
                PagingState = null
            };

            InputViewModel = new InputViewModel(SendMessage, SendMessage, SendMessage, 
                chooseImageInteraction, 
                chooseTrackInteraction);
        }

        public void ReadMessages()
        {
            if (HasNewMessages)
                HasNewMessages = false;
        }

        private async Task Initialize()
        {
            HasNewMessages = false;

            if (_initialized)
                return;

            await LoadMessages();
            _newMessageTimer.Start();
            _initialized = true;
        }

        private void AddNewMessage(Message message)
        {
            _messageCache.AddOrUpdate(message);
        }

        private async Task<BaseResponse> SendMessage(TextMessageRequest message)
        {
            message.GroupId = UserGroup.Group.Id;

            BaseResponse baseResponse;
            if (_prioritiseHub)
            {
                baseResponse = await _chatHubService.SendTextMessage(message);
            }
            else
            {
                baseResponse = await _groupChatService.SendTextMessageAsync(message);
            }

            return baseResponse;
        }

        private async Task<BaseResponse> SendMessage(ImageMessageRequest message)
        {
            message.GroupId = UserGroup.Group.Id;

            BaseResponse baseResponse;
            if (_prioritiseHub)
            {
                baseResponse = await _chatHubService.SendImageMessage(message);
            }
            else
            {
                baseResponse = await _groupChatService.SendImageMessageAsync(message);
            }

            return baseResponse;
        }

        private async Task<BaseResponse> SendMessage(TrackMessageRequest message, string filePath)
        {
            message.GroupId = UserGroup.Group.Id;

            return await _groupChatService.SendTrackMessageAsync(message, filePath);
        }

        private async Task CheckForNewMessages()
        {
            var lastMessageVm = _sortedMessages.LastOrDefault();
            if (lastMessageVm == null)
            {
                await LoadMessages();
            }
            else
            {
                var lastMsgDateTime = lastMessageVm.CreatedAt;
                var latestMsgs = await _groupChatService.GetMessagesAsync(UserGroup.Group.Id, lastMsgDateTime);

                if (latestMsgs.IsSuccessful)
                {
                    DisplayError = null;
                    AddMessages(latestMsgs.Items);
                }
                else
                {
                    //log 
                    //show error
                }
            }
        }

        private async Task GetMoreMessages()
        {
            if (!_initialized)
                return;

            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            PagedResponse<ICollection<Message>> pagedResponse = await _groupChatService.GetMessagesAsync(UserGroup.Group.Id, _pageParams);

            if (pagedResponse.IsSuccessful)
            {
                _pageParams = pagedResponse.PageParams;
                _pageParams.PagingState = _pageParams.NextPagingState;

                DisplayError = null;
                AddMessages(pagedResponse.Data);
            }
            else
            {
                //log
                //show error
            }
        }

        private void AddMessages(ICollection<Message> messages)
        {
            _messageCache.AddOrUpdate(messages);
        }

        private async Task<TrackResponse> LoadTrack(string trackId)
        {
            return await _groupChatService.GetLoadedTrackAsync(UserGroup.Group.Id, trackId);
        }

        private MessageViewModel GenerateMessageViewModel(Message message)
        {
            if (message.GroupId != UserGroup.Group.Id)
                return null;

            GroupUserViewModel? sender = UserGroup.Group.SortedGroupUsers.FirstOrDefault(guVm => guVm.User.Id == message.SenderId);
            if (sender == null)
                return null; //maybe throw?

            bool sentByLoggedInUser = User.Id == sender.User.Id;
            MessageViewModel messageViewModel = new MessageViewModel(message, sender, sentByLoggedInUser, _vlcWrapper, LoadTrack);
            return messageViewModel;
        }

        private void SetGroupFlags(
                MessageViewModel? previousViewModel, 
                MessageViewModel currentViewModel,
                MessageViewModel? nextViewModel)
        {
            if (previousViewModel != null)
            {
                MessageViewModel.SetGroupFlags(previousViewModel, currentViewModel);
            }
            else
            {
                MessageViewModel.SetGroupFlags(currentViewModel);
            }

            if (nextViewModel != null)
            {
                MessageViewModel.SetGroupFlags(currentViewModel, nextViewModel);
            }
        }

        private void SetGroupFlags(
                MessageViewModel? previousViewModel,
                IList<MessageViewModel> currentViewModels,
                MessageViewModel? nextViewModel)
        {
            if (currentViewModels.Count == 0)
                return;

            if (currentViewModels.Count == 1)
            {
                SetGroupFlags(previousViewModel, currentViewModels[0], nextViewModel);
                return;
            }

            SetGroupFlags(previousViewModel, currentViewModels[0], currentViewModels[1]);

            int count = currentViewModels.Count;
            for (int i = 1; i < count - 2; i += 2)
            {
                SetGroupFlags(currentViewModels[i], currentViewModels[i + 1], currentViewModels[i + 2]);
            }

            if (count % 2 == 0)
            {
                if (nextViewModel != null)
                {
                    SetGroupFlags(currentViewModels[count - 1], nextViewModel, null);
                }
            }
            else
            {
                SetGroupFlags(currentViewModels[count - 2], currentViewModels[count - 1], nextViewModel);
            }
        }

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_newMessageTimer != null)
                    {
                        _newMessageTimer.Stop();

                        foreach (var mVm in _sortedMessages)
                        {
                            mVm.Dispose();
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ChatViewModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
