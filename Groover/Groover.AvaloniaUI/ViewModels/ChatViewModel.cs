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
        public UserViewModel User { get; set; }

        [Reactive]
        public UserGroupViewModel UserGroup { get; set; }

        public ChatViewModel()
        {

        }
        public void InitializeData(UserViewModel loggedInUser, UserGroupViewModel userGroup, IGroupService groupService)
        {
            _groupService = groupService;
            User = loggedInUser;
            UserGroup = userGroup;
        }

        internal void UpdateGroupData(GroupViewModel group)
        {
            throw new NotImplementedException();
        }

        internal void UserLeft(GroupUserViewModel gu)
        {
            throw new NotImplementedException();
        }

        internal void UserJoined(GroupUserViewModel gu)
        {
            throw new NotImplementedException();
        }

        internal void UserRoleUpdated(int uId, string newRole)
        {
            throw new NotImplementedException();
        }

        internal void UserUpdated(UserViewModel user)
        {
            throw new NotImplementedException();
        }

        internal void UserGroupUpdated(UserGroupViewModel userGroup)
        {
            throw new NotImplementedException();
        }
    }
}
