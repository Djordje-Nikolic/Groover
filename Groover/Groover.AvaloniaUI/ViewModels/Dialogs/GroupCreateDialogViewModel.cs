using AutoMapper;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class GroupCreateDialogViewModel : BaseGroupViewModel
    {
        public GroupCreateDialogViewModel(IGroupService groupService, IMapper mapper) 
            : base("Create group", groupService, mapper, new Group())
        {
            YesButtonText = "Create group";
        }

        public override async Task<GroupResponse?> ExecuteOperation()
        {
            GroupRequest groupRequest = this._mapper.Map<GroupRequest>(Group);
            var response = await this._groupService.CreateGroupAsync(groupRequest);

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
