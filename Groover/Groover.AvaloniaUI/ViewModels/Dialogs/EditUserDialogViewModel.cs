using AutoMapper;
using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class EditUserDialogViewModel : ReactiveValidationObject
    {
        protected IUserService _userService;
        protected IMapper _mapper;
        protected ImageConstants _imageConstants;
        protected UserConstants _userConstants;

        public Interaction<string[], string?> ShowChooseImageDialog { get; set; }

        [Reactive]
        public List<string> Errors { get; set; }
        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }
        [Reactive]
        public string NoButtonText { get; set; }
        [Reactive]
        public User User { get; set; }
        [Reactive]
        public string Username { get; set; }
        [Reactive]
        public string Password { get; set; }
        [Reactive]
        public string ConfirmPassword { get; set; }
        [Reactive]
        public Bitmap? AvatarImage { get; set; }

        public ReactiveCommand<Unit, UserResponse?> YesCommand { get; }
        public ReactiveCommand<Unit, UserResponse?> NoCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseImage { get; }
        public ReactiveCommand<Unit, Unit> ClearImage { get; }

        public EditUserDialogViewModel(string titleText,
            IUserService userService,
            IMapper mapper,
            User user)
        {
            _userService = userService;
            _mapper = mapper;
            _imageConstants = Locator.Current.GetService<ImageConstants>();
            _userConstants = Locator.Current.GetService<UserConstants>();

            TitleText = titleText;
            User = user;
            Username = user.Username;
            AvatarImage = user.AvatarImage;
            YesButtonText = "Confirm";
            NoButtonText = "Cancel";

            ShowChooseImageDialog = new Interaction<string[], string?>();
            InitializeValidation();

            YesCommand = ReactiveCommand.CreateFromTask<Unit, UserResponse?>(x =>
            {
                User.Username = Username;            
                return UpdateUser();
            }, this.IsValid());

            NoCommand = ReactiveCommand.Create<Unit, UserResponse?>(x => null);
            ChooseImage = ReactiveCommand.CreateFromTask<Unit>(x => OnChooseImage());
            ClearImage = ReactiveCommand.Create<Unit>(x => { AvatarImage = null; User.AvatarBytes = null; });

        }

        public async Task OnChooseImage()
        {
            Errors = null;
            var resultPath = await ShowChooseImageDialog.Handle(new string[] { "jpg", "jpeg", "png", "gif" });

            if (resultPath == null)
                return;

            //try to load image, generate errors if any and place them in errorList
            if (!Path.IsPathFullyQualified(resultPath))
            {
                Errors = new List<string>() { "Image file path invalid." };
                return;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = File.ReadAllBytes(resultPath);
            }
            catch (Exception)
            {
                Errors = new List<string>() { "Couldn't read file bytes." };
                return;
            }

            try
            {
                Bitmap image;
                using (var ms = new MemoryStream(imageBytes))
                {
                    image = new Bitmap(ms);
                }
                AvatarImage = image;
                User.AvatarBytes = imageBytes;
            }
            catch (Exception)
            {
                Errors = new List<string> { "Couldn't load image." };
            }
        }

        private async Task<UserResponse?> UpdateUser()
        {
            UserRequest userRequest = new UserRequest();
            userRequest.Id = User.Id;
            userRequest.Username = Username;
            userRequest.Password = Password;
            userRequest.AvatarBase64 = User.AvatarBase64;

            var response = await this._userService.UpdateUserAsync(userRequest);

            if (response.IsSuccessful)
                return response;

            Errors = ProcessErrors(response);

            return response;
        }

        protected virtual void InitializeValidation()
        {
            this.ValidationRule(vm => vm.Username, name => !string.IsNullOrWhiteSpace(name), "Name can't be empty.");
            this.ValidationRule(vm => vm.AvatarImage, image => image == null || image.Size.Width < _imageConstants.MaxWidth, $"Image too wide. Max width: {_imageConstants.MaxWidth} px");
            this.ValidationRule(vm => vm.AvatarImage, image => image == null || image.Size.Width > _imageConstants.MinWidth, $"Image too narrow. Min width: {_imageConstants.MinWidth} px");
            this.ValidationRule(vm => vm.AvatarImage, image => image == null || image.Size.Height < _imageConstants.MaxHeight, $"Image too tall. Max height: {_imageConstants.MaxHeight} px");
            this.ValidationRule(vm => vm.AvatarImage, image => image == null || image.Size.Height > _imageConstants.MinHeight, $"Image too short. Min height: {_imageConstants.MinHeight} px");

            IObservable<bool> passwordsObservable = this.WhenAnyValue(
                    x => x.Password,
                    x => x.ConfirmPassword,
                    (password, confirmation) => password == confirmation);
            this.ValidationRule(vm => vm.ConfirmPassword, passwordsObservable, "Passwords do not match!");

            IObservable<bool> imageSize = this.WhenAnyValue(
                    x => x.User.AvatarBytes,
                    x => x == null || (x.Length / (1024 * 1024)) < _imageConstants.MaxSizeInMb);
            this.ValidationRule(vm => vm.AvatarImage, imageSize, $"Image too big. Max size: {_imageConstants.MaxSizeInMb} mb");
        }

        private List<string> ProcessErrors(UserResponse response)
        {
            List<string> messages = new List<string>();

            if (response.ErrorCodes == null)
                return messages;

            foreach (var code in response.ErrorCodes.Distinct())
            {
                string matchMessage;
                switch (code)
                {
                    case "bad_id":
                        matchMessage = "Invalid user id.";
                        break;
                    case "not_found":
                        matchMessage = "User with this id doesn't exist.";
                        break;
                    case "undefined":
                        matchMessage = "User update data undefined.";
                        break;
                    case "InvalidUserName":
                        matchMessage = "Invalid username format.";
                        break;
                    case "DuplicateUserName":
                        matchMessage = "User name is already taken.";
                        break;
                    case "PasswordTooShort":
                        matchMessage = $"Passwords must be at least {_userConstants.PasswordMinLength} characters.";
                        break;
                    case "PasswordRequiresUniqueChars":
                        matchMessage = $"Passwords must use at least {_userConstants.PasswordMinUnique} different characters.";
                        break;
                    case "PasswordRequiresNonAlphanumeric":
                        matchMessage = "Passwords must have at least one non alphanumeric character.";
                        break;
                    case "PasswordRequiresLower":
                        matchMessage = "Passwords must have at least one lowercase ('a'-'z').";
                        break;
                    case "PasswordRequiresUpper":
                        matchMessage = "Passwords must have at least one uppercase ('A'-'Z').";
                        break;
                    case "PasswordRequiredDigit":
                        matchMessage = "Passwords must have at least one digit ('0'-'9').";
                        break;
                    case "DefaultError":
                        matchMessage = "An error has occured.";
                        break;
                    case "too_wide":
                        matchMessage = $"Image is too wide. Max width: {int.Parse(response.ErrorResponse.ErrorValue)} px";
                        break;
                    case "too_narrow":
                        matchMessage = $"Image is too narrow. Min width: {int.Parse(response.ErrorResponse.ErrorValue)} px";
                        break;
                    case "too_tall":
                        matchMessage = $"Image is too tall. Max height: {int.Parse(response.ErrorResponse.ErrorValue)} px";
                        break;
                    case "too_short":
                        matchMessage = $"Image is too short. Min height: {int.Parse(response.ErrorResponse.ErrorValue)} px";
                        break;
                    case "too_big":
                        matchMessage = $"Image is too big. Max size: {double.Parse(response.ErrorResponse.ErrorValue)} mb";
                        break;
                    case "invalid_extension":
                        matchMessage = $"File has invalid extension. Allowed extensions: {response.ErrorResponse.ErrorValue}";
                        break;
                    case "failed_validation":
                        matchMessage = $"One of the fields is invalid.";
                        break;
                    default:
                        matchMessage = "Error updating the group.";
                        break;
                }
                messages.Add(matchMessage);
            }

            return messages;
        }
    }
}
