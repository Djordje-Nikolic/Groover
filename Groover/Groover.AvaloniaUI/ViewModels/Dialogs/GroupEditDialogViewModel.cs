using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models.DTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class GroupEditDialogViewModel : ReactiveValidationObject
    {
        public Interaction<string[], string?> ShowChooseImageDialog { get; set; }

        [Reactive]
        public List<string> ImageErrors { get; set; }
        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }
        [Reactive]
        public string NoButtonText { get; set; }
        [Reactive]
        public Group Group { get; set; }
        [Reactive]
        public Bitmap? GroupImage { get; set; }

        public ReactiveCommand<Unit, Group?> YesCommand { get; }
        public ReactiveCommand<Unit, Group?> NoCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseImage { get; }
        public ReactiveCommand<Unit, Unit> ClearImage { get; }

        public GroupEditDialogViewModel(string titleText, 
            string yesButtonText = "Update group", 
            Group? groupToEdit = null)
        {
            TitleText = titleText;
            Group = groupToEdit ?? new Group();
            GroupImage = groupToEdit?.Image;
            YesButtonText = yesButtonText;
            NoButtonText = "Cancel";

            ShowChooseImageDialog = new Interaction<string[], string?>();
            this.ValidationRule(vm => vm.Group.Name, username => !string.IsNullOrWhiteSpace(username), "Name can't be empty.");

            YesCommand = ReactiveCommand.Create<Unit, Group?>(x => this.Group, this.IsValid());
            NoCommand = ReactiveCommand.Create<Unit, Group?>(x => null);
            ChooseImage = ReactiveCommand.CreateFromTask<Unit>(x => OnChooseImage());
            ClearImage = ReactiveCommand.Create<Unit>(x => { GroupImage = null; });

        }

        public async Task OnChooseImage()
        {
            ImageErrors = null;
            var resultPath = await ShowChooseImageDialog.Handle(new string[] { "jpg", "jpeg", "png", "gif" });

            if (resultPath == null)
                return;

            try
            {
                var errorList = new List<string>();
                //try to load image, generate errors if any and place them in errorList
                ImageErrors = errorList;
            }
            catch (Exception e)
            {
                ImageErrors = new List<string>() { "Couldn't load the image." };
            }
        }
    }
}
