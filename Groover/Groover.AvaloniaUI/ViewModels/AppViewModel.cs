using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.Utils;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        private GroupRoleComparer _groupRoleComparer = new GroupRoleComparer();
        private IUserService _userService;
        private IGroupService _groupService;

        public Interaction<YesNoDialogViewModel, bool> ShowYesNoDialog { get; set; }
        public Interaction<ChangeRoleDialogViewModel, GrooverGroupRole?> ShowGroupRoleDialog { get; set; }
        public Interaction<ChooseUserDialogViewModel, int?> ShowUserSearchDialog { get; set; }

        [Reactive]
        public LoginResponse LoginResponse { get; set; }

        //Represent logged in state?
        [Reactive]
        public List<UserGroup> UserGroups { get; set; }

        [Reactive]
        public List<ChatViewModel> ChatViewModels { get; set; }

        [ObservableAsProperty]
        public bool IsActiveGroupAdmin { get; set; }
        [ObservableAsProperty]
        public Group ActiveGroup { get; }
        [Reactive]
        public ChatViewModel ActiveChatViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> SwitchToHomeCommand { get; }
        public ReactiveCommand<int, Unit> SwitchGroupDisplay { get; }
        public ReactiveCommand<User, Unit> ChangeRoleCommand { get; }
        public ReactiveCommand<User, Unit> KickUserCommand { get; }
        public ReactiveCommand<Group, Unit> InviteUserCommand { get; }
        public ReactiveCommand<Group, Unit> LeaveGroupCommand { get; }
        public ReactiveCommand<Group, Unit> DeleteGroupCommand { get; }

        public AppViewModel(LoginResponse logResp, IUserService userService, IGroupService groupService)
        {
            SwitchToHomeCommand = ReactiveCommand.CreateFromTask(SwitchToHome);
            SwitchGroupDisplay = ReactiveCommand.CreateFromTask<int>(SwitchGroup);
            ChangeRoleCommand = ReactiveCommand.CreateFromTask<User>(ChangeRole);
            KickUserCommand = ReactiveCommand.CreateFromTask<User>(KickUser);
            InviteUserCommand = ReactiveCommand.CreateFromTask<Group>(InviteUser);
            LeaveGroupCommand = ReactiveCommand.CreateFromTask<Group>(LeaveGroup);
            DeleteGroupCommand = ReactiveCommand.CreateFromTask<Group>(DeleteGroup);

            _userService = userService;
            _groupService = groupService;

            LoginResponse = logResp;

            //It is assumed that the application is closing
            if (logResp == null)
                return;

            //Set active group when chatviewmodel changes
            this.WhenAnyValue(vm => vm.ActiveChatViewModel)
                .Select(cvm => cvm?.UserGroup.Group)
                .ToPropertyEx(this, x => x.ActiveGroup);

            //Set active group isadmin
            this.WhenAnyValue(vm => vm.ActiveChatViewModel)
                .Where(cvm => cvm != null)
                .Select(cvm => cvm.UserGroup.GroupRole == "Admin")
                .ToPropertyEx(this, x => x.IsActiveGroupAdmin);

            UserGroups = logResp.User.UserGroups.ToList();
            ChatViewModels = GenerateChatViewModels();
        }

        public async Task SwitchGroup(int groupIdToSelect)
        {
            var selectedUg = UserGroups.FirstOrDefault(ug => ug.Group.Id == groupIdToSelect);

            if (selectedUg == null)
                return;

            var group = selectedUg.Group;
            group.GroupUsers = group.GroupUsers.OrderByDescending(x => x, _groupRoleComparer).ToList();

            //IsActiveGroupAdmin = selectedUg.GroupRole == "Admin";
            //ActiveGroup = group;
            ActiveChatViewModel = this.ChatViewModels.Find(vm => vm.UserGroup.Group == group);
        }

        public async Task SwitchToHome()
        {
            //ActiveGroup = null;
            ActiveChatViewModel = null;
        }

        private List<ChatViewModel> GenerateChatViewModels()
        {
            List<ChatViewModel> chatViewModels = new List<ChatViewModel>();
            foreach (var userGroup in UserGroups)
            {
                var viewModel = new ChatViewModel();

                //Set callbacks

                //Set data
                viewModel.InitializeData(LoginResponse.User, userGroup, _groupService);

                chatViewModels.Add(viewModel);
            }

            return chatViewModels;
        }

        private async Task ChangeRole(User user)
        {
            if (ShowGroupRoleDialog == null)
                return;

            var currentGroupRoleString = ActiveGroup.GroupUsers.Where(gu => gu.User.Id == user.Id).First().GroupRole;
            var currentGroupRole = (GrooverGroupRole) Enum.Parse(typeof(GrooverGroupRole), currentGroupRoleString);
            var changeRoleVm = new ChangeRoleDialogViewModel(currentGroupRole);
            var chosenNewRole = await ShowGroupRoleDialog.Handle(changeRoleVm);

            if (chosenNewRole != null)
            {
                var response = await _groupService.UpdateUserRoleAsync(ActiveGroup.Id, user.Id, chosenNewRole.Value);

                if (response.IsSuccessful)
                {
                    //Display confirmation message
                    //Wait for notification to update or update right away?
                }
                else
                {
                    string errorMessage;
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            errorMessage = "One of the ID's is in an invalid format.";
                            break;
                        case "bad_role":
                            errorMessage = "New role unrecognized.";
                            break;
                        case "not_found_group":
                            errorMessage = "Chosen group does not exist.";
                            break;
                        case "not_found":
                            errorMessage = "User is not a member of the group.";
                            break;
                        case "last_admin":
                            errorMessage = "User is the last admin of the group, and can't be demoted.";
                            break;
                        default:
                            errorMessage = "Unknown error occured.";
                            break;
                    }

                    //Display error message
                }
            }
        }

        private async Task KickUser(User user)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to kick user '{user.Username}' from the group?", "Kick user?");
            var kickAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (kickAccepted)
            {
                var response = await _groupService.RemoveUserAsync(ActiveGroup.Id, user.Id);

                if (response.IsSuccessful)
                {
                    //Display confirmation message
                    //Wait for notification to update or update right away?
                }
                else
                {
                    string errorMessage;
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            errorMessage = "One of the ID's is in an invalid format.";
                            break;
                        case "not_found_group":
                            errorMessage = "Chosen group does not exist.";
                            break;
                        case "not_found":
                            errorMessage = "User is not a member of the group.";
                            break;
                        default:
                            errorMessage = "Unknown error occured.";
                            break;
                    }

                    //Display error message
                }
            }
        }

        private async Task InviteUser(Group group)
        {
            if (ShowUserSearchDialog == null)
                return;

            var currentUsers = group.GroupUsers.Select(gu => gu.User).ToList();
            var chooseUserVm = new ChooseUserDialogViewModel(currentUsers, this._userService);
            var chosenUserId = await ShowUserSearchDialog.Handle(chooseUserVm);

            if (chosenUserId != null)
            {
                var response = await _groupService.InviteUserAsync(ActiveGroup.Id, chosenUserId.Value);

                if (response.IsSuccessful)
                {
                    //Display confirmation message
                    //Wait for notification to update or update right away?
                }
                else
                {
                    string errorMessage;
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            errorMessage = "One of the ID's is in an invalid format.";
                            break;
                        case "not_found_group":
                            errorMessage = "Chosen group does not exist.";
                            break;
                        case "not_found":
                            errorMessage = "User is not a member of the group.";
                            break;
                        case "already_member":
                            errorMessage = "User is already a member of the group.";
                            break;
                        default:
                            errorMessage = "Unknown error occured.";
                            break;
                    }

                    //Display error message
                }
            }
        }

        private async Task LeaveGroup(Group group)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to leave '{group.Name}'?", "Leave group?");
            var leaveAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (leaveAccepted)
            {
                var response = await _groupService.RemoveUserAsync(group.Id, LoginResponse.User.Id);

                if (response.IsSuccessful)
                {
                    //Display confirmation message
                    //Wait for notification to update or update right away?
                }
                else
                {
                    string errorMessage;
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            errorMessage = "One of the ID's is in an invalid format.";
                            break;
                        case "not_found_group":
                            errorMessage = "Chosen group does not exist.";
                            break;
                        case "not_found":
                            errorMessage = "User is not the member of the group.";
                            break;
                        default:
                            errorMessage = "Unknown error occured.";
                            break;
                    }

                    //Display error message
                }
            }
        }

        private async Task DeleteGroup(Group group)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to delete '{group.Name}'? This will delete all data related to this group.", "Delete group?");
            var deleteAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (deleteAccepted)
            {
                var response = await _groupService.DeleteGroupAsync(group.Id);

                if (response.IsSuccessful)
                {
                    //Display confirmation message
                    //Wait for notification to update or update right away?
                }
                else
                {
                    string errorMessage;
                    switch (response.ErrorResponse.ErrorCode)
                    {
                        case "bad_id":
                            errorMessage = "Group ID is in an invalid format.";
                            break;
                        case "not_found":
                            errorMessage = "Chosen group does not exist.";
                            break;
                        default:
                            errorMessage = "Unknown error occured.";
                            break;
                    }

                    //Display error message
                }
            }
        }
    }
}
