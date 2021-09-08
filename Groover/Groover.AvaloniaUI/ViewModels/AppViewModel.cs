using AutoMapper;
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
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using DynamicData.Binding;
using DynamicData;
using Groover.AvaloniaUI.ViewModels.Notifications;
using Groover.AvaloniaUI.Utils;

namespace Groover.AvaloniaUI.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        private IUserService _userService;
        private IGroupService _groupService;
        private IGroupChatService _groupChatService;
        private IMapper _mapper;
        private readonly UserConstants _userParams;

        public Interaction<YesNoDialogViewModel, bool> ShowYesNoDialog { get; set; }
        public Interaction<ChangeRoleDialogViewModel, GrooverGroupRole?> ShowGroupRoleDialog { get; set; }
        public Interaction<BaseGroupViewModel, GroupResponse?> ShowGroupEditDialog { get; set; }
        public Interaction<EditUserDialogViewModel, UserResponse?> ShowUserEditDialog { get; set; }
        public Interaction<ChooseUserDialogViewModel, int?> ShowUserSearchDialog { get; set; }

        [Reactive]
        public Interaction<NotificationViewModel, NotificationViewModel?> ShowNotificationDialog { get; set; } 
        
        [Reactive]
        public LoginResponse LoginResponse { get; set; }

        
        private SourceCache<UserGroup, int> _userGroupsCache;
        private ReadOnlyObservableCollection<UserGroup> _userGroups;
        public ReadOnlyObservableCollection<UserGroup> UserGroups => _userGroups;

        private ReadOnlyObservableCollection<ChatViewModel> _chatViewModels;
        public ReadOnlyObservableCollection<ChatViewModel> ChatViewModels => _chatViewModels;

        [ObservableAsProperty]
        public bool IsActiveGroupAdmin { get; set; }
        [ObservableAsProperty]
        public Group ActiveGroup { get; }
        [Reactive]
        public ChatViewModel ActiveChatViewModel { get; set; }

        [Reactive]
        public NotificationsViewModel NotificationsViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> SwitchToHomeCommand { get; }
        public ReactiveCommand<int, Unit> SwitchGroupDisplay { get; }
        public ReactiveCommand<User, Unit> ChangeRoleCommand { get; }
        public ReactiveCommand<User, Unit> KickUserCommand { get; }
        public ReactiveCommand<Group, Unit> InviteUserCommand { get; }
        public ReactiveCommand<Group, Unit> LeaveGroupCommand { get; }
        public ReactiveCommand<Group, Unit> DeleteGroupCommand { get; }
        public ReactiveCommand<Group, Unit> EditGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
        public ReactiveCommand<Unit, bool> LogoutCommand { get; }
        public ReactiveCommand<User, Unit> EditUserCommand { get; }

        public AppViewModel(LoginResponse logResp, 
                            IUserService userService, 
                            IGroupService groupService,
                            IGroupChatService groupChatService,
                            IMapper mapper,
                            UserConstants userParameters)
        {

            SwitchToHomeCommand = ReactiveCommand.CreateFromTask(SwitchToHome);
            SwitchGroupDisplay = ReactiveCommand.CreateFromTask<int>(SwitchGroup);
            ChangeRoleCommand = ReactiveCommand.CreateFromTask<User>(ChangeRole);
            KickUserCommand = ReactiveCommand.CreateFromTask<User>(KickUser);
            InviteUserCommand = ReactiveCommand.CreateFromTask<Group>(InviteUser);
            LeaveGroupCommand = ReactiveCommand.CreateFromTask<Group>(LeaveGroup);
            DeleteGroupCommand = ReactiveCommand.CreateFromTask<Group>(DeleteGroup);
            EditGroupCommand = ReactiveCommand.CreateFromTask<Group>(EditGroup);
            CreateGroupCommand = ReactiveCommand.CreateFromTask(CreateGroup);
            EditUserCommand = ReactiveCommand.CreateFromTask<User>(EditUser);
            LogoutCommand = ReactiveCommand.CreateFromTask<bool>(Logout);

            _userService = userService;
            _groupService = groupService;
            _groupChatService = groupChatService;
            _mapper = mapper;
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
                .Select(role => role == "Admin")
                .ToPropertyEx(this, x => x.IsActiveGroupAdmin);

            InitializeLoginResponse(logResp);
        }

        public async Task InitializeChatConnections()
        {
            var connection = await this._groupChatService.InitializeConnection();
            connection.On<UserGroup>("GroupCreated", async (ug) => 
            {
                _userGroupsCache.AddOrUpdate(ug);
                await this._groupChatService.JoinGroup(ug.Group.Id);

                NotificationsViewModel.AddNotification(new NotificationViewModel()
                {
                    TitleText = "New group created!",
                    BodyText = $"Group '{ug.Group.Name}' has been successfully created!"
                });
            });

            connection.On<string>("GroupDeleted", async (groupId) =>
            {
                if (int.TryParse(groupId, out int gId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        await this._groupChatService.LeaveGroup(ug.Group.Id);
                        _userGroupsCache.Remove(ug);

                        SwitchToHomeCommand.Execute().Subscribe();

                        NotificationsViewModel.AddNotification(new NotificationViewModel()
                        {
                            TitleText = "A group has been deleted",
                            BodyText = $"Group '{ug.Group.Name}' has been deleted."
                        });
                    }
                }
            });

            connection.On<Group>("GroupUpdated", (group) =>
            {
                if (group != null)
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == group.Id);
                    if (ug != null)
                    {
                        ug.Group.Name = group.Name;
                        ug.Group.Description = group.Description;
                        ug.Group.ImageBase64 = group.ImageBase64;
                        _userGroupsCache.AddOrUpdate(ug);

                        var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == group.Id);
                        //cVm.UpdateGroupData(group);
                    }
                }
            });

            connection.On<string, GroupUser>("UserJoined", (groupId, gu) =>
            {
                if (int.TryParse(groupId, out int gId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        ug.Group.GroupUsers.InsertIntoSorted(gu);

                        var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                        //cVm.UserJoined(gu);
                    }
                }
            });

            connection.On<UserGroup>("LoggedInUserJoined", async (userGroup) =>
            {
                if (userGroup != null)
                {
                    userGroup.Group.GroupUsers.SortByRole();
                    _userGroupsCache.AddOrUpdate(userGroup);
                    await this._groupChatService.JoinGroup(userGroup.Group.Id);


                    NotificationsViewModel.AddNotification(new NotificationViewModel()
                    {
                        TitleText = "You have joined a group!",
                        BodyText = $"You have joined '{userGroup.Group.Name}'!"
                    });
                }
            });

            connection.On<string, string>("UserLeft", async (groupId, userId) =>
            {
                if (int.TryParse(groupId, out int gId) &&
                    int.TryParse(userId, out int uId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        if (uId == LoginResponse.User.Id)
                        {
                            _userGroupsCache.Remove(ug);
                            await this._groupChatService.LeaveGroup(ug.Group.Id);
                        }
                        else
                        {
                            var gu = ug.Group.GroupUsers.FirstOrDefault(gu => gu.User.Id == uId);
                            if (gu != null)
                            {
                                ug.Group.GroupUsers.Remove(gu);

                                var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                                //cVm.UserLeft(gu);
                            }
                        }
                    }
                }
            });

            connection.On<string, string, string>("UserRoleUpdated", (groupId, userId, newRole) =>
            {
                if (int.TryParse(groupId, out int gId) &&
                    int.TryParse(userId, out int uId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        if (uId == LoginResponse.User.Id)
                        {
                            ug.GroupRole = newRole;
                        }

                        var gu = ug.Group.GroupUsers.FirstOrDefault(gu => gu.User.Id == uId);
                        if (gu != null)
                        {
                            gu.GroupRole = newRole;

                            ug.Group.GroupUsers.SortByRole();

                            var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                            //cVm.UserRoleUpdated(uId, newRole);
                        }
                    }
                }
            });

            connection.On<User>("LoggedInUserUpdated", (user) =>
            {
                if (user != null)
                {
                    LoginResponse.User.Username = user.Username;
                    LoginResponse.User.Email = user.Email;
                    LoginResponse.User.AvatarBase64 = user.AvatarBase64;
                }
            });

            connection.On<string, User>("UserUpdated", (groupId, user) =>
            {
                if (int.TryParse(groupId, out int gId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        var tempUser = ug.Group.GroupUsers.FirstOrDefault(gu => gu.User.Id == user.Id)?.User;
                        if (tempUser != null)
                        {
                            tempUser.Username = user.Username;
                            tempUser.Email = user.Email;
                            tempUser.AvatarBase64 = tempUser.AvatarBase64;

                            var cVm = ChatViewModels.First(vm => vm.UserGroup.Group.Id == gId);
                            //cVm.UserUpdated(user);
                        }
                    }
                }
            });

            connection.On<string, string>("ConnectedToGroup", async (groupId, userId) =>
            {
                if (int.TryParse(groupId, out int gId) &&
                    int.TryParse(userId, out int uId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        var user = ug.Group.GroupUsers.FirstOrDefault(gu => gu.User.Id == uId)?.User;
                        if (user != null)
                        {
                            user.IsOnline = true;
                            await _groupChatService.NotifyConnection(gId, uId);
                        }
                    }
                }
            });

            connection.On<string, string>("DisconnectedFromGroup", (groupId, userId) =>
            {
                if (int.TryParse(groupId, out int gId) &&
                    int.TryParse(userId, out int uId))
                {
                    var ug = UserGroups.FirstOrDefault(ug => ug.Group.Id == gId);
                    if (ug != null)
                    {
                        var user = ug.Group.GroupUsers.FirstOrDefault(gu => gu.User.Id == uId)?.User;
                        if (user != null)
                        {
                            user.IsOnline = false;
                        }
                    }
                }
            });

            connection.On<byte[], Group, string>("UserInvited", (tokenBytes, group, userId) =>
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
                        if (uId == LoginResponse.User.Id)
                        {
                            NotificationsViewModel.AddNotification(new InviteViewModel(group, token, uId, _groupService));
                        }
                    }
                }
            });

            //Do the rest of callbacks

            await this._groupChatService.StartConnection();
            
            foreach (var ug in this.UserGroups)
            {
                await this._groupChatService.JoinGroup(ug.Group.Id);
            }
        }

        private void InitializeLoginResponse(LoginResponse logResp)
        {
            //It is assumed that the application is closing
            if (logResp == null)
                return;

            LoginResponse = logResp;

            this._userGroupsCache = new SourceCache<UserGroup, int>(ug => ug.Group.Id);
            _userGroupsCache.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _userGroups)
                .DisposeMany()
                .Subscribe();

            _userGroupsCache.Connect()
                .TransformWithInlineUpdate(ug => GenerateChatViewModel(ug), (previousViewModel, updatedUserGroup) =>
                {
                    previousViewModel.UserGroupUpdated(updatedUserGroup);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _chatViewModels)
                .DisposeMany()
                .Subscribe();

            _userGroupsCache.AddOrUpdate(LoginResponse.User.UserGroups);
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
            var selectedUg = UserGroups.FirstOrDefault(ug => ug.Group.Id == groupIdToSelect);

            if (selectedUg == null)
                return;

            selectedUg.Group.GroupUsers.SortByRole();

            ActiveChatViewModel = this.ChatViewModels.FirstOrDefault(vm => vm.UserGroup.Group == selectedUg.Group);
        }

        public async Task SwitchToHome()
        {
            ActiveChatViewModel = null;
        }

        private List<ChatViewModel> GenerateChatViewModels()
        {
            List<ChatViewModel> chatViewModels = new List<ChatViewModel>();
            foreach (var userGroup in UserGroups)
            {
                var viewModel = GenerateChatViewModel(userGroup);

                chatViewModels.Add(viewModel);
            }

            return chatViewModels;
        }

        private ChatViewModel GenerateChatViewModel(UserGroup userGroup)
        {
            var viewModel = new ChatViewModel();

            //Set callbacks

            //Set data
            viewModel.InitializeData(LoginResponse.User, userGroup, _groupService);

            return viewModel;
        }

        private async Task EditGroup(Group group)
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

        private async Task EditUser(User user)
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
                _userGroupsCache.Clear();
                ActiveChatViewModel = null;
                LoginResponse = null;
                _userService.Logout();
                await _groupChatService.Reset();
                return true;
            }
            else
                return false;
        }

        private async Task KickUser(User user)
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

        private async Task InviteUser(Group group)
        {
            if (ShowUserSearchDialog == null)
                return;

            var currentUsers = group.GroupUsers.Select(gu => gu.User).ToList();
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

        private async Task LeaveGroup(Group group)
        {
            if (ShowYesNoDialog == null)
                return;

            var yesNoVm = new YesNoDialogViewModel($"Are you sure you want to leave '{group.Name}'?", "Leave group");
            var leaveAccepted = await ShowYesNoDialog.Handle(yesNoVm);

            if (leaveAccepted)
            {
                var response = await _groupService.RemoveUserAsync(group.Id, LoginResponse.User.Id);

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

        private async Task DeleteGroup(Group group)
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
