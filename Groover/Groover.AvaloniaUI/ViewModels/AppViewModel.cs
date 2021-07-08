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
        public Group ActiveGroup { get; set; }

        [Reactive]
        public bool IsActiveGroupAdmin { get; set; }

        public ReactiveCommand<int, Unit> SwitchGroupDisplay { get; }
        public ReactiveCommand<User, Unit> ChangeRoleCommand { get; }
        public ReactiveCommand<User, Unit> KickUserCommand { get; }
        public ReactiveCommand<Group, Unit> InviteUserCommand { get; }
        public ReactiveCommand<Group, Unit> LeaveGroupCommand { get; }

        public AppViewModel(LoginResponse logResp, IUserService userService, IGroupService groupService)
        {
            SwitchGroupDisplay = ReactiveCommand.CreateFromTask<int>(SwitchGroup);
            ChangeRoleCommand = ReactiveCommand.CreateFromTask<User>(ChangeRole);
            KickUserCommand = ReactiveCommand.CreateFromTask<User>(KickUser);
            InviteUserCommand = ReactiveCommand.CreateFromTask<Group>(InviteUser);
            LeaveGroupCommand = ReactiveCommand.CreateFromTask<Group>(LeaveGroup);

            _userService = userService;
            _groupService = groupService;

            LoginResponse = logResp;

            //It is assumed that the application is closing
            if (logResp == null)
                return;

            UserGroups = logResp.User.UserGroups.ToList();
        }

        public async Task SwitchGroup(int groupIdToSelect)
        {
            var selectedUg = UserGroups.FirstOrDefault(ug => ug.Group.Id == groupIdToSelect);

            if (selectedUg == null)
                return;

            var group = selectedUg.Group;
            group.GroupUsers = group.GroupUsers.OrderByDescending(x => x, _groupRoleComparer).ToList();

            IsActiveGroupAdmin = selectedUg.GroupRole == "Admin";
            ActiveGroup = group;
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
                //Send request through service
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
                //Send request through service
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
                //Send request through service
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
                //Send request through service
            }
        }
    }
}
