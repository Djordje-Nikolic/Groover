using AutoMapper;
using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace Groover.AvaloniaUI.ViewModels
{
    public abstract class GroupViewModelBase : ReactiveValidationObject
    {
        protected IGroupService _groupService;
        protected IMapper _mapper;
        protected ImageConstants _imageConstants;

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
        public GroupViewModel Group { get; set; }
        [Reactive]
        public string GroupName { get; set; }
        [Reactive]
        public string GroupDesc { get; set; }
        [Reactive]
        public Bitmap? GroupImage { get; set; }

        public ReactiveCommand<Unit, GroupResponse?> YesCommand { get; }
        public ReactiveCommand<Unit, GroupResponse?> NoCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseImage { get; }
        public ReactiveCommand<Unit, Unit> ClearImage { get; }

        public GroupViewModelBase(string titleText,
            IGroupService groupService,
            IMapper mapper,
            GroupViewModel group)
        {
            _groupService = groupService;
            _mapper = mapper;
            _imageConstants = Locator.Current.GetService<ImageConstants>();

            TitleText = titleText;
            Group = group;
            GroupName = group.Name;
            GroupDesc = group.Description;
            GroupImage = group.Image;
            YesButtonText = "Confirm";
            NoButtonText = "Cancel";

            ShowChooseImageDialog = new Interaction<string[], string?>();
            InitializeValidation();

            YesCommand = ReactiveCommand.CreateFromTask<Unit, GroupResponse?>(x => 
            {
                Group.Name = GroupName;
                Group.Description = GroupDesc;
                return ExecuteOperation();
            }, this.IsValid());

            NoCommand = ReactiveCommand.Create<Unit, GroupResponse?>(x => null);
            ChooseImage = ReactiveCommand.CreateFromTask<Unit>(x => OnChooseImage());
            ClearImage = ReactiveCommand.Create<Unit>(x => { GroupImage = null; Group.ImageBytes = null; });

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
                GroupImage = image;
                Group.ImageBytes = imageBytes;
            }
            catch (Exception)
            {
                Errors = new List<string> { "Couldn't load image." };
            }
        }

        public abstract Task<GroupResponse?> ExecuteOperation();

        protected virtual void InitializeValidation()
        {
            this.ValidationRule(vm => vm.GroupName, name => !string.IsNullOrWhiteSpace(name), "Name can't be empty.");
            this.ValidationRule(vm => vm.GroupImage, image => image == null || image.Size.Width < _imageConstants.MaxWidth, $"Image too wide. Max width: {_imageConstants.MaxWidth} px");
            this.ValidationRule(vm => vm.GroupImage, image => image == null || image.Size.Width > _imageConstants.MinWidth, $"Image too narrow. Min width: {_imageConstants.MinWidth} px");
            this.ValidationRule(vm => vm.GroupImage, image => image == null || image.Size.Height < _imageConstants.MaxHeight, $"Image too tall. Max height: {_imageConstants.MaxHeight} px");
            this.ValidationRule(vm => vm.GroupImage, image => image == null || image.Size.Height > _imageConstants.MinHeight, $"Image too short. Min height: {_imageConstants.MinHeight} px");

            IObservable<bool> imageSize = this.WhenAnyValue(
                    x => x.Group.ImageBytes,
                    x => x == null || (x.Length / (1024 * 1024)) < _imageConstants.MaxSizeInMb);
            this.ValidationRule(vm => vm.GroupImage, imageSize, $"Image too big. Max size: {_imageConstants.MaxSizeInMb} mb");
        }
    }
}
