using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private readonly IGroupChatService _groupChatService;
        private readonly IChatHubService _chatHubService;
        private bool _prioritiseHub;

        [Reactive]
        public string TextContent { get; set; }
        [Reactive]
        public string ImageFilePath { get; set; }
        [Reactive]
        public string TrackFilePath { get; set; }

        public ReactiveCommand<Unit,Unit> SendMessageCommand { get; }

        public InputViewModel(IGroupChatService groupChatService,
            IChatHubService chatHubService,
            bool prioritiseSendingThroughHub = true)
        {
            _groupChatService = groupChatService;
            _chatHubService = chatHubService;
            _prioritiseHub = prioritiseSendingThroughHub;

            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage);
        }

        public async Task SendMessage()
        {

        }
    }
}
