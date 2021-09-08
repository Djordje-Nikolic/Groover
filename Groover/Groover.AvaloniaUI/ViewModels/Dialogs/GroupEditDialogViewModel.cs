using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using AutoMapper;
using Groover.AvaloniaUI.Models.Requests;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class GroupEditDialogViewModel : BaseGroupViewModel
    {
        public GroupEditDialogViewModel(IGroupService groupService, 
            IMapper mapper,
            GroupViewModel groupToEdit) : base("Edit group", groupService, mapper, groupToEdit)
        {
            YesButtonText = "Update group";
        }
        
        public override async Task<GroupResponse?> ExecuteOperation()
        {
            GroupRequest groupRequest = this._mapper.Map<GroupRequest>(Group);
            var response = await this._groupService.UpdateGroupAsync(groupRequest);

            if (response.IsSuccessful)
                return response;

            Errors = ProcessErrors(response);

            return response;
        }

        private List<string> ProcessErrors(GroupResponse response)
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
                        matchMessage = "Invalid group id.";
                        break;
                    case "not_found":
                        matchMessage = "Group with this id doesn't exist.";
                        break;
                    case "undefined":
                        matchMessage = "Group update data undefined.";
                        break;
                    case "duplicate_name":
                        matchMessage = "A group with an identical name already exists.";
                        break;
                    case "bad_format":
                        matchMessage = "Image format is invalid.";
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
