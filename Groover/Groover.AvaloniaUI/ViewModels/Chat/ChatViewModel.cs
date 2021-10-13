using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
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
        private SourceCache<MessageViewModel, string> _messageCache;
        private ReadOnlyObservableCollection<MessageViewModel> _sortedMessages;
        public ReadOnlyObservableCollection<MessageViewModel> SortedMessages => _sortedMessages;

        [Reactive]
        public UserViewModel User { get; set; }
        [Reactive]
        public UserGroupViewModel UserGroup { get; set; }
        public InputViewModel InputViewModel { get; set; }

        public ChatViewModel(UserViewModel loggedInUser,
            UserGroupViewModel userGroup,
            IGroupChatService groupChatService,
            IChatHubService chatHubService,
            Interaction<ChooseImageDialogViewModel, string?> chooseImageInteraction,
            Interaction<ChooseTrackDialogViewModel, string?> chooseTrackInteraction,
            bool prioritiseSendingThroughHub = false)
        {
            _groupChatService = groupChatService;
            _chatHubService = chatHubService;
            _prioritiseHub = prioritiseSendingThroughHub;
            _vlcWrapper = DIContainer.GetRequiredService<IVLCWrapper>(Locator.Current);
            User = loggedInUser;
            UserGroup = userGroup;

            _messageCache = new SourceCache<MessageViewModel, string>(msg => msg.Id);
            _messageCache.Connect()
                .Sort(SortExpressionComparer<MessageViewModel>.Ascending(mVm => mVm.CreatedAt))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _sortedMessages)
                .Subscribe();

            _newMessageTimer = new DispatcherTimer(TimeSpan.FromMinutes(2),
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

        public async Task Initialize()
        {
            if (_initialized)
                return;

            await LoadMessages();
            _newMessageTimer.Start();
            _initialized = true;
        }

        public void AddNewMessage(Message message)
        {
            var messageViewModel = GenerateMessageViewModel(message);
            if (messageViewModel != null)
                _messageCache.AddOrUpdate(messageViewModel);
        }

        private async Task<BaseResponse> SendMessage(TextMessageRequest message)
        {
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
                    AddMessages(latestMsgs.Items);
                }
                else
                {
                    //log 
                    //show error
                }
            }
        }

        private async Task LoadMessages()
        {
            PagedResponse<ICollection<Message>> pagedResponse = await _groupChatService.GetMessagesAsync(UserGroup.Group.Id, _pageParams);

            if (pagedResponse.IsSuccessful)
            {
                _pageParams = pagedResponse.PageParams;
                _pageParams.PagingState = _pageParams.NextPagingState;

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
            if (messages.Count > 0)
            {
                List<MessageViewModel> msgVms = new List<MessageViewModel>();
                foreach (var msg in messages)
                {
                    var msgVm = GenerateMessageViewModel(msg);
                    if (msgVm != null)
                        msgVms.Add(msgVm);
                }
                _messageCache.AddOrUpdate(msgVms);
            }
        }

        private async Task<TrackResponse> LoadTrack(string trackId)
        {
            return await _groupChatService.GetLoadedTrackAsync(UserGroup.Group.Id, trackId);
        }

        private MessageViewModel? GenerateMessageViewModel(Message message)
        {
            if (message.GroupId != UserGroup.Group.Id)
                throw new ArgumentException("Message is not for this group.", nameof(message));

            GroupUserViewModel? sender = UserGroup.Group.SortedGroupUsers.FirstOrDefault(guVm => guVm.User.Id == message.SenderId);
            if (sender == null)
                return null; //maybe throw?

            bool sentByLoggedInUser = User.Id == sender.User.Id;
            MessageViewModel messageViewModel = new MessageViewModel(message, sender, _vlcWrapper, sentByLoggedInUser, LoadTrack);
            return messageViewModel;
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

                        var kvplist = _messageCache.KeyValues.ToList();
                        foreach (var kvp in kvplist)
                        {
                            _messageCache.Remove(kvp.Value);
                            kvp.Value.Dispose();
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

        //internal void UpdateGroupData(GroupViewModel group)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UserLeft(GroupUserViewModel gu)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UserJoined(GroupUserViewModel gu)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UserRoleUpdated(int uId, string newRole)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UserUpdated(UserViewModel user)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UserGroupUpdated(UserGroupViewModel userGroup)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
