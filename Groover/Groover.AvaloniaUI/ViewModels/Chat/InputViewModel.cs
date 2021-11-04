using Avalonia.Media.Imaging;
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
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class InputViewModel : ViewModelBase
    {
        private Func<TextMessageRequest, Task<BaseResponse>> _sendTxtMsgDelegate;
        private Func<ImageMessageRequest, Task<BaseResponse>> _sendImgMsgDelegate;
        private Func<TrackMessageRequest, string, Task<BaseResponse>> _sendTrackMsgDelegate;
        private Interaction<ChooseImageDialogViewModel, string?> _chooseImageInteraction;
        private Interaction<ChooseTrackDialogViewModel, ChooseTrackResult?> _chooseTrackInteraction;
        private ImageConfiguration _imageConfig;
        private TrackConfiguration _trackConfig;

        [Reactive]
        public string? TextContent { get; set; }
        [Reactive]
        public string? ImageFilePath { get; set; }
        [Reactive]
        public string? TrackFilePath { get; set; }
        [Reactive]
        public string? TrackName { get; set; }
        [Reactive]
        public string? Error { get; set; }
        [ObservableAsProperty]
        public Bitmap? Image { get; }
        [ObservableAsProperty]
        public string? TrackDisplayText { get; }

        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearImageCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearTrackCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseImageCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseTrackCommand { get; }
        public ReactiveCommand<int, int> NewLineCommand { get; }

        public InputViewModel(Func<TextMessageRequest, Task<BaseResponse>> txtMsgDelegate,
            Func<ImageMessageRequest, Task<BaseResponse>> imgMsgDelegate,
            Func<TrackMessageRequest, string, Task<BaseResponse>> trackMsgDelegate,
            Interaction<ChooseImageDialogViewModel, string?> chooseImageInteraction,
            Interaction<ChooseTrackDialogViewModel, ChooseTrackResult?> chooseTrackInteraction)
        {
            _sendTxtMsgDelegate = txtMsgDelegate;
            _sendImgMsgDelegate = imgMsgDelegate;
            _sendTrackMsgDelegate = trackMsgDelegate;
            _chooseImageInteraction = chooseImageInteraction;
            _chooseTrackInteraction = chooseTrackInteraction;
            _imageConfig = DIContainer.GetRequiredService<ImageConfiguration>(Locator.Current);
            _trackConfig = DIContainer.GetRequiredService<TrackConfiguration>(Locator.Current);

            var canSend = this.WhenAnyValue(iVm => iVm.TextContent, iVm => iVm.ImageFilePath, iVm => iVm.TrackFilePath,
                (txtContent, imgFilePath, trackFilePath) =>
                !string.IsNullOrWhiteSpace(txtContent) ||
                !string.IsNullOrWhiteSpace(imgFilePath) ||
                !string.IsNullOrWhiteSpace(trackFilePath));
            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage, canSend);

            var canClearImage = this.WhenAnyValue(iVm => iVm.ImageFilePath, filepath => !string.IsNullOrWhiteSpace(filepath));
            ClearImageCommand = ReactiveCommand.Create(ClearImage, canClearImage);

            var canClearTrack = this.WhenAnyValue(iVm => iVm.TrackFilePath, filepath => !string.IsNullOrWhiteSpace(filepath));
            ClearTrackCommand = ReactiveCommand.Create(ClearTrack, canClearTrack);

            var canChooseImage = this.WhenAnyValue(iVm => iVm.ImageFilePath, filepath => string.IsNullOrWhiteSpace(filepath));
            ChooseImageCommand = ReactiveCommand.CreateFromTask(ChooseImage, canChooseImage);

            var canChooseTrack = this.WhenAnyValue(iVm => iVm.TrackFilePath, filepath => string.IsNullOrWhiteSpace(filepath));
            ChooseTrackCommand = ReactiveCommand.CreateFromTask(ChooseTrack, canChooseTrack);

            NewLineCommand = ReactiveCommand.Create<int, int>(AddNewLineToTextContent);

            this.WhenAnyValue(iVm => iVm.ImageFilePath)
                .Do(_ => { if (this.Image != null) this.Image.Dispose(); })
                .Select(val =>
                {
                    if (val != null && File.Exists(val))
                        return new Bitmap(val);
                    else
                        return null;
                })
                .ToPropertyEx(this, iVm => iVm.Image);

            this.WhenAnyValue(iVm => iVm.TrackFilePath, iVm => iVm.TrackName)
                .Where(vals =>
                !string.IsNullOrWhiteSpace(vals.Item1) &&
                !string.IsNullOrWhiteSpace(vals.Item2))
                .Select(vals =>
                {
                    string? filename = Path.GetFileName(vals.Item1);
                    if (filename != null)
                        return string.Join(' ', filename, "as", vals.Item2);
                    else
                        return string.Join(' ', vals.Item1, "as", vals.Item2);
                })
                .ToPropertyEx(this, iVm => iVm.TrackDisplayText);

            TextContent = null;
            ImageFilePath = null;
            TrackFilePath = null;
            TrackName = null;
        }

        private async Task ChooseTrack()
        {
            var viewModel = new ChooseTrackDialogViewModel(_trackConfig);
            ChooseTrackResult? trackResult = await _chooseTrackInteraction.Handle(viewModel);
            if (trackResult != null)
            {
                TrackFilePath = trackResult.Value.FilePath;
                TrackName = trackResult.Value.FileName;
            }
        }

        private async Task ChooseImage()
        {
            var viewModel = new ChooseImageDialogViewModel(_imageConfig);
            string? imageFilePath = await _chooseImageInteraction.Handle(viewModel);
            if (!string.IsNullOrWhiteSpace(imageFilePath))
            {
                ImageFilePath = imageFilePath;
            }
        }

        private void ClearTrack()
        {
            TrackFilePath = null;
        }

        private void ClearImage()
        {
            ImageFilePath = null;
        }

        private int AddNewLineToTextContent(int index)
        {
            if (!string.IsNullOrWhiteSpace(TextContent))
            {
                TextContent = TextContent.Insert(index, Environment.NewLine);
                return index + 1;
            }
            else
            {
                return index;
            }
        }

        private async Task SendMessage()
        {
            BaseResponse? response = null;
            MessageType? determineCurrentType = DetermineCurrentType();
            switch (determineCurrentType)
            {
                case MessageType.Text:
                    var trimmedContent = TextContent?.Trim();
                    TextMessageRequest textMessageRequest = new TextMessageRequest(trimmedContent);
                    response = await _sendTxtMsgDelegate.Invoke(textMessageRequest);
                    break;
                case MessageType.Image:
                    ImageMessageRequest imageMessageRequest = new ImageMessageRequest(ImageFilePath);
                    imageMessageRequest.Content = TextContent?.Trim();
                    response = await _sendImgMsgDelegate.Invoke(imageMessageRequest);
                    break;
                case MessageType.Track:
                    var trimmedName = TrackName?.Trim();
                    TrackMessageRequest trackMessageRequest = new TrackMessageRequest(trimmedName);
                    response = await _sendTrackMsgDelegate.Invoke(trackMessageRequest, TrackFilePath);
                    break;
                default:
                    Error = "Couldn't determine message type.";
                    break;
            }

            if (response != null)
                ProcessMessageResponse(response);
        }

        private void ProcessMessageResponse(BaseResponse response)
        {
            if (response.IsSuccessful)
            {
                TextContent = null;
                ImageFilePath = null;
                TrackFilePath = null;
                TrackName = null;
                Error = null;
            }
            else
            {
                if (response.ErrorCodes == null)
                {
                    Error = "There was an error while retrieving new messages.";
                }
                else
                {
                    List<string> errorCodes = new();

                    foreach (var code in response.ErrorCodes.Distinct())
                    {
                        switch (code)
                        {
                            case "not_member":
                                errorCodes.Add("User is not a member of the group.");
                                break;
                            case "not_found_group":
                                errorCodes.Add("Group not found.");
                                break;
                            case "not_found_user":
                                errorCodes.Add("Sender not found.");
                                break;
                            case "bad_message_format":
                                errorCodes.Add("Message format is invalid.");
                                break;
                            case "bad_extension":
                                errorCodes.Add("Track file extension is not supported.");
                                break;
                            case "bad_track_format":
                                errorCodes.Add("Track file format is not supported.");
                                break;
                            case "undefined":
                                errorCodes.Add("Bad message data or paging parameters.");
                                break;
                            case "bad_id":
                                errorCodes.Add("Bad group id.");
                                break;
                            case "internal":
                            default:
                                errorCodes.Add("Unknown error occured.");
                                break;
                        }
                    }

                    Error = string.Join(Environment.NewLine, errorCodes);
                }
            }
        }

        private MessageType? DetermineCurrentType()
        {
            if (!string.IsNullOrWhiteSpace(TrackFilePath) && !string.IsNullOrWhiteSpace(TrackName))
                return MessageType.Track;

            if (!string.IsNullOrWhiteSpace(ImageFilePath))
                return MessageType.Image;

            if (!string.IsNullOrWhiteSpace(TextContent))
                return MessageType.Text;

            return null;
        }
    }
}
