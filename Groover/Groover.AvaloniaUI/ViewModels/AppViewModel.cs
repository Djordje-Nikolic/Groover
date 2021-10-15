using AutoMapper;
using Groover.AvaloniaUI.Models;
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
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using DynamicData.Binding;
using DynamicData;
using Groover.AvaloniaUI.ViewModels.Notifications;
using Avalonia.Threading;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.ViewModels.Chat;

namespace Groover.AvaloniaUI.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        private IUserService _userService;
        private IGroupService _groupService;
        private IGroupChatService _groupChatService;
        private IChatHubService _chatHubService;
        private IMapper _mapper;
        private IVLCWrapper _vlcWrapper;
        private readonly UserConstants _userParams;

        public Interaction<YesNoDialogViewModel, bool> ShowYesNoDialog { get; set; }
        public Interaction<ChangeRoleDialogViewModel, GrooverGroupRole?> ShowGroupRoleDialog { get; set; }
        public Interaction<GroupViewModelBase, GroupResponse?> ShowGroupEditDialog { get; set; }
        public Interaction<EditUserDialogViewModel, UserResponse?> ShowUserEditDialog { get; set; }
        public Interaction<ChooseUserDialogViewModel, int?> ShowUserSearchDialog { get; set; }
        public Interaction<ChooseImageDialogViewModel, string?> ShowChooseImageDialog { get; set; }
        public Interaction<ChooseTrackDialogViewModel, ChooseTrackResult?> ShowChooseTrackDialog { get; set; }

        [Reactive]
        public Interaction<NotificationViewModel, NotificationViewModel?> ShowNotificationDialog { get; set; } 
        
        [Reactive]
        public UserViewModel LoggedInUser { get; private set; }

        private ReadOnlyObservableCollection<ChatViewModel> _chatViewModels;
        public ReadOnlyObservableCollection<ChatViewModel> ChatViewModels => _chatViewModels;

        [ObservableAsProperty]
        public bool IsActiveGroupAdmin { get; set; }
        [ObservableAsProperty]
        public GroupViewModel ActiveGroup { get; }
        [Reactive]
        public ChatViewModel ActiveChatViewModel { get; set; }

        [Reactive]
        public NotificationsViewModel NotificationsViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> SwitchToHomeCommand { get; }
        public ReactiveCommand<int, Unit> SwitchGroupDisplay { get; }
        public ReactiveCommand<UserViewModel, Unit> ChangeRoleCommand { get; }
        public ReactiveCommand<UserViewModel, Unit> KickUserCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> InviteUserCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> LeaveGroupCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> DeleteGroupCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> EditGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
        public ReactiveCommand<Unit, bool> LogoutCommand { get; }
        public ReactiveCommand<UserViewModel, Unit> EditUserCommand { get; }

        public AppViewModel(UserViewModel loggedInUser, 
                            IUserService userService, 
                            IGroupService groupService,
                            IChatHubService chatHubService,
                            IGroupChatService groupChatService,
                            IMapper mapper,
                            IVLCWrapper vlcWrapper,
                            UserConstants userParameters)
        {

            SwitchToHomeCommand = ReactiveCommand.CreateFromTask(SwitchToHome);
            SwitchGroupDisplay = ReactiveCommand.CreateFromTask<int>(SwitchGroup);
            ChangeRoleCommand = ReactiveCommand.CreateFromTask<UserViewModel>(ChangeRole);
            KickUserCommand = ReactiveCommand.CreateFromTask<UserViewModel>(KickUser);
            InviteUserCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(InviteUser);
            LeaveGroupCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(LeaveGroup);
            DeleteGroupCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(DeleteGroup);
            EditGroupCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(EditGroup);
            CreateGroupCommand = ReactiveCommand.CreateFromTask(CreateGroup);
            EditUserCommand = ReactiveCommand.CreateFromTask<UserViewModel>(EditUser);
            LogoutCommand = ReactiveCommand.CreateFromTask<bool>(Logout);

            _userService = userService;
            _groupService = groupService;
            _chatHubService = chatHubService;
            _groupChatService = groupChatService;
            _mapper = mapper;
            _vlcWrapper = vlcWrapper;
            _userParams = userParameters;

            NotificationsViewModel = new NotificationsViewModel();
            this.WhenAnyValue(vm => vm.ShowNotificationDialog)
                .ToPropertyEx(NotificationsViewModel, x => x.ShowNotificationDialog);

            //Set active group when chatviewmodel changes
            this.WhenAnyValue(vm => vm.ActiveChatViewModel)
                .Select(cvm => cvm?.UserGroup.Group)
                .ToPropertyEx(this, x => x.ActiveGroup);

            //Set active group isadmin
            this.WhenAnyValue(vm => vm.ActiveChatViewModel.UserGroup.GroupRole)
                .Select(role => role == GrooverGroupRole.Admin)
                .ToPropertyEx(this, x => x.IsActiveGroupAdmin);

            if (loggedInUser == null)
                return;

            LoggedInUser = loggedInUser;
        }

        public async Task InitializeChatConnections()
        {
            await this._chatHubService.InitializeConnection();
            var handlersWrapper = _chatHubService.HandlersWrapper;
            handlersWrapper.GroupCreated(OnGroupCreated);
            handlersWrapper.GroupDeleted(OnGroupDeleted);
            handlersWrapper.GroupUpdated(OnGroupUpdated);
            handlersWrapper.UserJoined(OnUserJoined);
            handlersWrapper.LoggedInUserJoined(OnLoggedInUserJoined);
            handlersWrapper.UserLeft(OnUserLeft);
            handlersWrapper.UserRoleUpdated(OnUserRoleUpdated);
            handlersWrapper.LoggedInUserUpdated(OnLoggedInUserUpdated);
            handlersWrapper.UserUpdated(OnUserUpdated);
            handlersWrapper.ConnectedToGroup(OnConnectedToGroup);
            handlersWrapper.DisconnectedFromGroup(OnDisconnectedFromGroup);
            handlersWrapper.UserInvited(OnUserInvited);
            handlersWrapper.GroupMessageAdded(OnGroupMessageAdded);

            await this._chatHubService.StartConnection();
            
            foreach (var ug in LoggedInUser.UserGroups)
            {
                await this._chatHubService.ConnectToGroup(ug.Group.Id);
            }
        }

        public async Task Cleanup()
        {
            await _chatHubService.Reset();
            foreach (var cVm in ChatViewModels)
            {
                cVm.Dispose();
            }
        }

        #region Chat Service Callbacks
        private void OnGroupMessageAdded(Message message)
        {
            var cvm = ChatViewModels.FirstOrDefault(cvm => cvm.UserGroup.Group.Id == message.GroupId);

            if (cvm != null)
                cvm.AddNewMessage(message);
        }
        private async Task OnGroupCreated(UserGroup userGroup)
        {
            UserGroupViewModel userGroupViewModel = this._mapper.Map<UserGroupViewModel>(userGroup);
            LoggedInUser.UserGroupsCache.AddOrUpdate(userGroupViewModel);
            await this._chatHubService.ConnectToGroup(userGroup.Group.Id);

            NotificationsViewModel.AddNotification(new NotificationViewModel()
            {
                TitleText = "New group created!",
                BodyText = $"Group '{userGroup.Group.Name}' has been successfully created!"
            });
        }
        private async Task OnGroupDeleted(string groupId)
        {
            if (int.TryParse(groupId, out int gId))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    await this._chatHubService.DisconnectFromGroup(ug.Group.Id);
                    LoggedInUser.UserGroupsCache.Remove(ug);

                    SwitchToHomeCommand.Execute().Subscribe();

                    NotificationsViewModel.AddNotification(new NotificationViewModel()
                    {
                        TitleText = "A group has been deleted",
                        BodyText = $"Group '{ug.Group.Name}' has been deleted."
                    });
                }
            }
        }
        private void OnGroupUpdated(Group group)
        {
            if (group != null)
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == group.Id);
                if (ug != null)
                {
                    ug.Group.Name = group.Name;
                    ug.Group.Description = group.Description;
                    ug.Group.ImageBase64 = group.ImageBase64;
                    LoggedInUser.UserGroupsCache.AddOrUpdate(ug);

                    var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == group.Id);
                    //cVm.UpdateGroupData(group);
                }
            }
        }
        private void OnUserJoined(string groupId, GroupUser groupUser)
        {
            if (int.TryParse(groupId, out int gId))
            {
                var userGroup = LoggedInUser.UserGroups.FirstOrDefault(userGroup => userGroup.Group.Id == gId);
                if (userGroup != null)
                {
                    GroupUserViewModel groupUserViewModel = this._mapper.Map<GroupUserViewModel>(groupUser);
                    userGroup.Group.GroupUsersCache.AddOrUpdate(groupUserViewModel);

                    var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                    //cVm.UserJoined(gu);
                }
            }
        }
        private async Task OnLoggedInUserJoined(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                UserGroupViewModel userGroupViewModel = this._mapper.Map<UserGroupViewModel>(userGroup);
                LoggedInUser.UserGroupsCache.AddOrUpdate(userGroupViewModel);
                await this._chatHubService.ConnectToGroup(userGroup.Group.Id);

                NotificationsViewModel.AddNotification(new NotificationViewModel()
                {
                    TitleText = "You have joined a group!",
                    BodyText = $"You have joined '{userGroup.Group.Name}'!"
                });
            }
        }
        private async Task OnUserLeft(string groupId, string userId)
        {
            if (int.TryParse(groupId, out int gId) &&
                int.TryParse(userId, out int uId))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    if (uId == LoggedInUser.Id)
                    {
                        LoggedInUser.UserGroupsCache.Remove(ug);
                        await this._chatHubService.DisconnectFromGroup(ug.Group.Id);
                    }
                    else
                    {
                        var gu = ug.Group.SortedGroupUsers.FirstOrDefault(gu => gu.User.Id == uId);
                        if (gu != null)
                        {
                            ug.Group.GroupUsersCache.Remove(gu);

                            var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                            //cVm.UserLeft(gu);
                        }
                    }
                }
            }
        }
        private void OnUserRoleUpdated(string groupId, string userId, string newRole)
        {
            if (int.TryParse(groupId, out int gId) &&
                int.TryParse(userId, out int uId) &&
                Enum.TryParse(newRole, out GrooverGroupRole newGroupRole))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    if (uId == LoggedInUser.Id)
                    {
                        ug.GroupRole = newGroupRole;
                        LoggedInUser.UserGroupsCache.AddOrUpdate(ug);
                    }

                    var gu = ug.Group.SortedGroupUsers.FirstOrDefault(gu => gu.User.Id == uId);
                    if (gu != null)
                    {
                        gu.GroupRole = newGroupRole;
                        ug.Group.GroupUsersCache.AddOrUpdate(gu);

                        var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                        //cVm.UserRoleUpdated(uId, newRole);
                    }
                }
            }
        }
        private void OnLoggedInUserUpdated(User user)
        {
            if (user != null)
            {
                LoggedInUser.Username = user.Username;
                LoggedInUser.Email = user.Email;
                LoggedInUser.AvatarBase64 = user.AvatarBase64;
            }
        }
        private void OnUserUpdated(string groupId, User user)
        {
            if (int.TryParse(groupId, out int gId))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    var groupUser = ug.Group.SortedGroupUsers.FirstOrDefault(gu => gu.User.Id == user.Id);
                    if (groupUser != null)
                    {
                        var tempUser = groupUser.User;

                        tempUser.Username = user.Username;
                        tempUser.Email = user.Email;
                        tempUser.AvatarBase64 = user.AvatarBase64;

                        ug.Group.GroupUsersCache.AddOrUpdate(groupUser);

                        var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                        //cVm.UserUpdated(user);
                    }
                }
            }
        }
        private async Task OnConnectedToGroup(string groupId, string userId)
        {
            if (int.TryParse(groupId, out int gId) &&
                int.TryParse(userId, out int uId))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    var user = ug.Group.SortedGroupUsers.FirstOrDefault(gu => gu.User.Id == uId)?.User;
                    if (user != null)
                    {
                        user.IsOnline = true;
                        await _chatHubService.NotifyConnection(gId, uId);
                    }
                }
            }
        }
        private void OnDisconnectedFromGroup(string groupId, string userId)
        {
            if (int.TryParse(groupId, out int gId) &&
                int.TryParse(userId, out int uId))
            {
                var ug = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                if (ug != null)
                {
                    var user = ug.Group.SortedGroupUsers.FirstOrDefault(gu => gu.User.Id == uId)?.User;
                    if (user != null)
                    {
                        user.IsOnline = false;
                    }
                }
            }
        }
        private void OnUserInvited(byte[] tokenBytes, Group group, string userId)
        {
            if (tokenBytes != null)
            {
                string token = null;
                try
                {
                    token = Encoding.UTF8.GetString(tokenBytes);
                }
                catch (Exception e)
                {
                    NotificationsViewModel.AddNotification(new ErrorViewModel(e.GetType().ToString())
                    {
                        TitleText = "Bad invite token received",
                        BodyText = e.Message
                    });
                }

                if (int.TryParse(userId, out int uId) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    if (uId == LoggedInUser.Id)
                    {
                        GroupViewModel groupViewModel = this._mapper.Map<GroupViewModel>(group);
                        NotificationsViewModel.AddNotification(new InviteViewModel(groupViewModel, token, uId, _groupService));
                    }
                }
            }
        }
        #endregion

        public void Initialize()
        {
            LoggedInUser.UserGroupsCache.Connect()
                .TransformWithInlineUpdate(ug => GenerateChatViewModel(ug), (previousViewModel, updatedUserGroup) =>
                {
                    //If all objects are linked properly, this shouldnt be necessary
                    //previousViewModel.UserGroup = updatedUserGroup;
                    //previousViewModel.UserGroupUpdated(updatedUserGroup);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _chatViewModels)
                .DisposeMany()
                .Subscribe();
        }

        //private async Task UpdateLoginResponse()
        //{
        //    var newUserResp = await _userService.GetByIdAsync(LoginResponse.User.Id);
        //    if (newUserResp.IsSuccessful)
        //    {
        //        LoginResponse.User = _mapper.Map<User>(newUserResp);
        //        InitializeLoginResponse(LoginResponse);
        //    }
        //    else
        //    {
        //        //Display errors
        //    }
        //}

        public async Task SwitchGroup(int groupIdToSelect)
        {
            var selectedUg = LoggedInUser.UserGroups.FirstOrDefault(ug => ug.Group.Id == groupIdToSelect);

            if (selectedUg == null)
                return;

            //selectedUg.Group.GroupUsers.SortByRole();

            var cvm = this.ChatViewModels.FirstOrDefault(vm => vm.UserGroup.Group == selectedUg.Group);

            if (cvm != null)
                cvm.InitializeCommand.Execute().Subscribe();

            ActiveChatViewModel = cvm;
        }

        public async Task SwitchToHome()
        {
            ActiveChatViewModel = null;
        }

        private ChatViewModel GenerateChatViewModel(UserGroupViewModel userGroup)
        {
            var viewModel = new ChatViewModel(LoggedInUser, userGroup, _groupChatService, _chatHubService,
                ShowChooseImageDialog, ShowChooseTrackDialog);

            //Set callbacks

            return viewModel;
        }

        private async Task EditGroup(GroupViewModel group)
        {
            if (ShowGroupEditDialog == null)
                return;

            var deepCopiedGroup = group.DeepCopy(this._mapper);
            var vm = new GroupEditDialogViewModel(_groupService, _mapper, groupToEdit: deepCopiedGroup);
            var groupResponse = await ShowGroupEditDialog.Handle(vm);

            if (groupResponse != null)
            {
                if (!groupResponse.IsSuccessful)
                {
                    string titleText = $"Error updating group '{group.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (groupResponse.ErrorResponse != null)
                    {
                        switch (groupResponse.ErrorResponse.ErrorCode)
                        {
                            case "not_admin":
                                bodyText = "User is not the admin of the group.";
                                break;
                            case "undefined":
                                bodyText = "Group data is undefined.";
                                break;
                            case "not_found":
                                bodyText = "Couldn't find the group.";
                                break;
                            case "bad_format":
                                bodyText = "Format of the image is invalid.";
                                break;
                            case "too_wide":
                                bodyText = $"Image is too wide. Maximum width is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_narrow":
                                bodyText = $"Image is too narrow. Minimum width is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_tall":
                                bodyText = $"Image is too tall. Maximum height is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_short":
                                bodyText = $"Image is too short. Minimum height is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_big":
                                bodyText = $"Image is too big. Maximum size is '{groupResponse.ErrorResponse.ErrorValue}' bytes.";
                                break;
                            case "failed_validation":
                                bodyText = "One of the fields was invalid.";
                                break;
                            case "invalid_extension":
                                bodyText = $"File has invalid extension. Allowed extensions: {groupResponse.ErrorResponse.ErrorValue}";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(groupResponse.ErrorResponse?.ErrorCode ?? groupResponse.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task CreateGroup()
        {
            if (ShowGroupEditDialog == null)
                return;

            var vm = new GroupCreateDialogViewModel(_groupService, _mapper);
            var groupResponse = await ShowGroupEditDialog.Handle(vm);

            if (groupResponse != null)
            {
                if (!groupResponse.IsSuccessful)
                {
                    string titleText = $"Error creating a new group";
                    string bodyText = "Unknown error occured.";

                    if (groupResponse.ErrorResponse != null)
                    {
                        switch (groupResponse.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "Id of the user is invalid.";
                                break;
                            case "undefined":
                                bodyText = "Group data is undefined.";
                                break;
                            case "not_found":
                                bodyText = "Couldn't find the user.";
                                break;
                            case "duplicate_name":
                                bodyText = "A group with an identical name already exists.";
                                break;
                            case "bad_format":
                                bodyText = "Format of the image is invalid.";
                                break;
                            case "too_wide":
                                bodyText = $"Image is too wide. Maximum width is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_narrow":
                                bodyText = $"Image is too narrow. Minimum width is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_tall":
                                bodyText = $"Image is too tall. Maximum height is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_short":
                                bodyText = $"Image is too short. Minimum height is '{groupResponse.ErrorResponse.ErrorValue}' px.";
                                break;
                            case "too_big":
                                bodyText = $"Image is too big. Maximum size is '{groupResponse.ErrorResponse.ErrorValue}' bytes.";
                                break;
                            case "failed_validation":
                                bodyText = "One of the fields was invalid.";
                                break;
                            case "invalid_extension":
                                bodyText = $"File has invalid extension. Allowed extensions: {groupResponse.ErrorResponse.ErrorValue}";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(groupResponse.ErrorResponse?.ErrorCode ?? groupResponse.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task ChangeRole(UserViewModel user)
        {
            if (ShowGroupRoleDialog == null)
                return;

            var currentGroupRole = ActiveGroup.SortedGroupUsers.Where(gu => gu.User.Id == user.Id).First().GroupRole;
            var changeRoleVm = new ChangeRoleDialogViewModel(currentGroupRole);
            var chosenNewRole = await ShowGroupRoleDialog.Handle(changeRoleVm);

            if (chosenNewRole != null)
            {
                var response = await _groupService.UpdateUserRoleAsync(ActiveGroup.Id, user.Id, chosenNewRole.Value);

                if (!response.IsSuccessful)
                {
                    string titleText = $"Error changing the group role: User '{user.Username}' in group '{ActiveGroup.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (response.ErrorResponse != null)
                    {

                        switch (response.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "One of the ID's is in an invalid format.";
                                break;
                            case "bad_role":
                                bodyText = "New role unrecognized.";
                                break;
                            case "not_found_group":
                                bodyText = "Chosen group does not exist.";
                                break;
                            case "not_found":
                                bodyText = "User is not a member of the group.";
                                break;
                            case "last_admin":
                                bodyText = "User is the last admin of the group, and can't be demoted.";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(response.ErrorResponse?.ErrorCode ?? response.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task EditUser(UserViewModel user)
        {
            if (ShowUserEditDialog == null)
                return;

            var deepCopiedUser = user.DeepCopy(this._mapper);
            var vm = new EditUserDialogViewModel("Edit User", _userService, _mapper, deepCopiedUser);
            var userResponse = await ShowUserEditDialog.Handle(vm);

            if (userResponse != null)
            {
                if (userResponse.IsSuccessful)
                {
                    string titleText = $"Error updating the logged-in user";
                    string bodyText = "Unknown error occured.";
                    string errorCode = userResponse.StatusCode.ToString();

                    if (userResponse.ErrorResponse != null)
                    {
                        if (userResponse.ErrorCodes.Count > 1)
                        {
                            foreach (var code in userResponse.ErrorCodes)
                            {
                                StringBuilder bodyBuilder = new StringBuilder();
                                switch (code)
                                {
                                    case "InvalidUserName":
                                        bodyBuilder.Append("Invalid username format. ");
                                        break;
                                    case "InvalidEmail":
                                        bodyBuilder.Append("Invalid email format. ");
                                        break;
                                    case "DuplicateUserName":
                                        bodyBuilder.Append("User name is already taken. ");
                                        break;
                                    case "DuplicateEmail":
                                        bodyBuilder.Append("Email address is already taken. ");
                                        break;
                                    case "PasswordTooShort":
                                        bodyBuilder.Append($"Passwords must be at least {_userParams.PasswordMinLength} characters. ");
                                        break;
                                    case "PasswordRequiresUniqueChars":
                                        bodyBuilder.Append($"Passwords must use at least {_userParams.PasswordMinUnique} different characters. ");
                                        break;
                                    case "PasswordRequiresNonAlphanumeric":
                                        bodyBuilder.Append("Passwords must have at least one non alphanumeric character. ");
                                        break;
                                    case "PasswordRequiresLower":
                                        bodyBuilder.Append("Passwords must have at least one lowercase ('a'-'z'). ");
                                        break;
                                    case "PasswordRequiresUpper":
                                        bodyBuilder.Append("Passwords must have at least one uppercase ('A'-'Z'). ");
                                        break;
                                    case "PasswordRequiredDigit":
                                        bodyBuilder.Append("Passwords must have at least one digit ('0'-'9'). ");
                                        break;
                                    case "DefaultError":
                                        bodyBuilder.Append("An error has occured. ");
                                        break;
                                    default:
                                        bodyText = $"Unknown error '{code}' occured. ";
                                        break;
                                }
                                bodyText = bodyBuilder.ToString();
                                errorCode = "failed_validation";
                            }
                        }
                        else
                        {
                            switch (userResponse.ErrorResponse.ErrorCode)
                            {
                                case "undefined":
                                    bodyText = "User data is undefined.";
                                    break;
                                case "not_found":
                                    bodyText = "Couldn't find the user.";
                                    break;
                                case "DuplicateUserName":
                                    bodyText = "A user with an identical name already exists.";
                                    break;
                                case "bad_format":
                                    bodyText = "Format of the image is invalid.";
                                    break;
                                case "too_wide":
                                    bodyText = $"Image is too wide. Maximum width is '{userResponse.ErrorResponse.ErrorValue}' px.";
                                    break;
                                case "too_narrow":
                                    bodyText = $"Image is too narrow. Minimum width is '{userResponse.ErrorResponse.ErrorValue}' px.";
                                    break;
                                case "too_tall":
                                    bodyText = $"Image is too tall. Maximum height is '{userResponse.ErrorResponse.ErrorValue}' px.";
                                    break;
                                case "too_short":
                                    bodyText = $"Image is too short. Minimum height is '{userResponse.ErrorResponse.ErrorValue}' px.";
                                    break;
                                case "too_big":
                                    bodyText = $"Image is too big. Maximum size is '{userResponse.ErrorResponse.ErrorValue}' bytes.";
                                    break;
                                case "failed_validation":
                                    bodyText = "One of the fields was invalid.";
                                    break;
                                default:
                                    bodyText = "Unknown error occured.";
                                    break;
                            }
                            errorCode = userResponse.ErrorResponse.ErrorCode;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(errorCode)
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task<bool> Logout()
        {
            if (ShowYesNoDialog == null)
                return false;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to logout?", "Logout");
            var logoutAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (logoutAccepted)
            {
                ActiveChatViewModel = null;
                LoggedInUser = null;
                _userService.Logout();
                await Cleanup();
                return true;
            }
            else
                return false;
        }

        private async Task KickUser(UserViewModel user)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to kick user '{user.Username}' from the group?", "Kick user");
            var kickAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (kickAccepted)
            {
                var response = await _groupService.RemoveUserAsync(ActiveGroup.Id, user.Id);

                if (!response.IsSuccessful)
                {
                    string titleText = $"Error removing a user: User '{user.Username}' in group '{ActiveGroup.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (response.ErrorResponse != null)
                    {
                        switch (response.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "Group or user id are invalid.";
                                break;
                            case "not_admin":
                                bodyText = "User is not an admin of the group.";
                                break;
                            case "not_found_group":
                                bodyText = "Chosen group does not exist.";
                                break;
                            case "not_found":
                                bodyText = "User is not a member of the group.";
                                break;
                            case "last_admin":
                                bodyText = "User is the last admin of a non-empty group and cannot leave.";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(response.ErrorResponse?.ErrorCode ?? response.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task InviteUser(GroupViewModel group)
        {
            if (ShowUserSearchDialog == null)
                return;

            var currentUsers = group.SortedGroupUsers.Select(gu => gu.User).ToList();
            var chooseUserVm = new ChooseUserDialogViewModel(currentUsers, this._userService);
            var chosenUserId = await ShowUserSearchDialog.Handle(chooseUserVm);

            if (chosenUserId != null)
            {
                var response = await _groupService.InviteUserAsync(group.Id, chosenUserId.Value);

                if (!response.IsSuccessful)
                {
                    string titleText = $"Error inviting a user to group '{group.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (response.ErrorResponse != null)
                    {
                        switch (response.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "User or group id is invalid.";
                                break;
                            case "not_found_group":
                                bodyText = "Group does not exist.";
                                break;
                            case "not_found":
                                bodyText = "User does not exist.";
                                break;
                            case "not_admin":
                                bodyText = "User is not an admin of the group and can't send invites.";
                                break;
                            case "already_member":
                                bodyText = "Invited user is already a member of the group.";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(response.ErrorResponse?.ErrorCode ?? response.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }

        private async Task LeaveGroup(GroupViewModel group)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to leave '{group.Name}'?", "Leave group");
            var leaveAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (leaveAccepted)
            {
                var response = await _groupService.RemoveUserAsync(group.Id, LoggedInUser.Id);

                if (!response.IsSuccessful)
                {
                    string titleText = $"Error leaving group '{group.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (response.ErrorResponse != null)
                    {
                        switch (response.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "Group or user id are invalid.";
                                break;
                            case "not_admin":
                                bodyText = "User is not an admin of the group.";
                                break;
                            case "not_found_group":
                                bodyText = "Chosen group does not exist.";
                                break;
                            case "not_found":
                                bodyText = "User is not a member of the group.";
                                break;
                            case "last_admin":
                                bodyText = "User is the last admin of a non-empty group and cannot leave.";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(response.ErrorResponse?.ErrorCode ?? response.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }          
            }
        }

        private async Task DeleteGroup(GroupViewModel group)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to delete '{group.Name}'? This will delete all data related to this group.", "Delete group");
            var deleteAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (deleteAccepted)
            {
                var response = await _groupService.DeleteGroupAsync(group.Id);

                if (!response.IsSuccessful)
                {
                    string titleText = $"Error deleting group '{group.Name}'";
                    string bodyText = "Unknown error occured.";

                    if (response.ErrorResponse != null)
                    {
                        switch (response.ErrorResponse.ErrorCode)
                        {
                            case "bad_id":
                                bodyText = "Group id is invalid.";
                                break;
                            case "not_admin":
                                bodyText = "User is not an admin of the group.";
                                break;
                            case "not_found":
                                bodyText = "Group does not exist.";
                                break;
                            default:
                                bodyText = "Unknown error occured.";
                                break;
                        }
                    }

                    NotificationsViewModel.AddNotification(new ErrorViewModel(response.ErrorResponse?.ErrorCode ?? response.StatusCode.ToString())
                    {
                        TitleText = titleText,
                        BodyText = bodyText
                    });
                }
            }
        }
    }
}
