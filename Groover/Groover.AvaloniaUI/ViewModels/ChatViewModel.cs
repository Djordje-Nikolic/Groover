using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private IGroupService _groupService;

        [Reactive]
        public User User { get; set; }

        [Reactive]
        public UserGroup UserGroup { get; set; }

        public ChatViewModel()
        {

        }
        public void InitializeData(User loggedInUser, UserGroup userGroup, IGroupService groupService)
        {
            _groupService = groupService;
            User = loggedInUser;
            UserGroup = userGroup;
        }

        internal void UpdateGroupData(Group group)
        {
            throw new NotImplementedException();
        }

        internal void UserLeft(GroupUser gu)
        {
            throw new NotImplementedException();
        }

        internal void UserJoined(GroupUser gu)
        {
            throw new NotImplementedException();
        }

        internal void UserRoleUpdated(int uId, string newRole)
        {
            throw new NotImplementedException();
        }

        internal void UserUpdated(User user)
        {
            throw new NotImplementedException();
        }

        internal void UserGroupUpdated(UserGroup userGroup)
        {
            throw new NotImplementedException();
        }
    }
}
