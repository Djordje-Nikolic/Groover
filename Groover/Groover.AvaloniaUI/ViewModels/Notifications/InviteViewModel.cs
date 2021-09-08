using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Notifications
{
    public class InviteViewModel : NotificationViewModel
    {
        private readonly string _inviteToken;
        private readonly IGroupService _groupService;
        private int _invitedUserId;

        [Reactive]
        public GroupViewModel Group { get; set; }
        [Reactive]
        public string GroupName { get; set; }
        [Reactive]
        public string GroupDesc { get; set; }
        [Reactive]
        public Bitmap? GroupImage { get; set; }

        [Reactive]
        public string NoButtonText { get; set; }

        public ReactiveCommand<Unit, NotificationViewModel?> NoCommand { get; }

        public InviteViewModel(GroupViewModel group, 
                               string inviteToken,
                               int invitedUserId,
                               IGroupService groupService) : base()
        {
            Group = group;
            GroupName = group.Name;
            GroupDesc = group.Description;
            GroupImage = group.Image;

            _inviteToken = inviteToken;
            _groupService = groupService;
            _invitedUserId = invitedUserId;

            TitleText = $"Invite to {GroupName}";
            BodyText = $"You have been invited to join {GroupName}. Do you want to accept the invite?";

            YesButtonText = "Accept";
            NoButtonText = "Decline";

            NoCommand = ReactiveCommand.Create<Unit, NotificationViewModel?>(x => null);
        }

        protected override async Task<NotificationViewModel?> YesOperationAsync()
        {
            try
            {
                var response = await _groupService.AcceptInviteAsync(_inviteToken, Group.Id, _invitedUserId);
                if (response.IsSuccessful)
                {
                    return new NotificationViewModel()
                    {
                        TitleText = "Invitation accepted!",
                        BodyText = $"Invitation to {GroupName} successfully accepted!"
                    };
                }
                else
                {
                    if (response.ErrorResponse?.ErrorCode == null)
                    {
                        return new ErrorViewModel("unknown")
                        {
                            TitleText = "Group invite: Unknown error",
                            BodyText = $"Unknown error occured while accepting the invitation to {GroupName}."
                        };
                    }

                    string titleText = "Group invite: Unknown error";
                    string bodyText = $"Unknown error occured while accepting the invitation to {GroupName}.";
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            //log
                            break;
                        case "not_found":
                            titleText = "Group invite: User or group not found";
                            bodyText = $"Group ({GroupName}) or user do not exist.";
                            break;
                        case "already_member":
                            titleText = "Group invite: User already a member!";
                            bodyText = $"User is already a member of the group ({GroupName}).";
                            break;
                        case "token_invalid":
                            titleText = "Group invite: Token no longer valid";
                            bodyText = "The invitation token is no longer valid.";
                            break;
                        default:
                            break;
                    }

                    return new ErrorViewModel(response.ErrorResponse.ErrorCode)
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    };
                }

            }
            catch (Exception e)
            {
                return new ErrorViewModel(e.GetType().ToString())
                {
                    TitleText = "Group invite: Exception occured",
                    BodyText = e.Message
                };
            }
        }
    }
}
