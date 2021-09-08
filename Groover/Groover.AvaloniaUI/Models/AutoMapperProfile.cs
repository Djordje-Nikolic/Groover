using AutoMapper;
using DynamicData;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserViewModel, UserRequest>();
            CreateMap<GroupViewModel, GroupRequest>();

            CreateMap<UserGroup, UserGroupViewModel>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => Enum.Parse(typeof(GrooverGroupRole), s.GroupRole)));
            CreateMap<GroupUser, GroupUserViewModel>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => Enum.Parse(typeof(GrooverGroupRole), s.GroupRole)));

            CreateMap<UserGroupViewModel, UserGroup>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));
            CreateMap<GroupUserViewModel, GroupUser>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));

            CreateMap<User, UserViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.UserGroups != null)
                    {
                        foreach (var userGroup in src.UserGroups)
                        {
                            UserGroupViewModel userGroupViewModel = mapper.Map<UserGroupViewModel>(userGroup);
                            dest.UserGroupsCache.AddOrUpdate(userGroupViewModel);
                        }
                    }
                });
            CreateMap<Group, GroupViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.GroupUsers != null)
                    {
                        foreach (var groupUser in src.GroupUsers)
                        {
                            GroupUserViewModel groupUsersViewModel = mapper.Map<GroupUserViewModel>(groupUser);
                            dest.GroupUsersCache.AddOrUpdate(groupUsersViewModel);
                        }
                    }
                });

            CreateMap<UserViewModel, User>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.UserGroups != null)
                    {
                        dest.UserGroups = new List<UserGroup>();
                        foreach (var userGroupViewModel in src.UserGroups)
                        {
                            UserGroup userGroup = mapper.Map<UserGroup>(userGroupViewModel);
                            dest.UserGroups.Add(userGroup);
                        }
                    }
                });
            CreateMap<GroupViewModel, Group>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.SortedGroupUsers != null)
                    {
                        dest.GroupUsers = new List<GroupUser>();
                        foreach (var groupUserViewModel in src.SortedGroupUsers)
                        {
                            GroupUser groupUser = mapper.Map<GroupUser>(groupUserViewModel);
                            dest.GroupUsers.Add(groupUser);
                        }
                    }
                });

            CreateMap<UserResponse, UserViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.UserGroups != null)
                    {
                        foreach (var userGroup in src.UserGroups)
                        {
                            UserGroupViewModel userGroupViewModel = mapper.Map<UserGroupViewModel>(userGroup);
                            dest.UserGroupsCache.AddOrUpdate(userGroupViewModel);
                        }
                    }
                });
            CreateMap<GroupResponse, GroupViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.GroupUsers != null)
                    {
                        foreach (var groupUser in src.GroupUsers)
                        {
                            GroupUserViewModel groupUsersViewModel = mapper.Map<GroupUserViewModel>(groupUser);
                            dest.GroupUsersCache.AddOrUpdate(groupUsersViewModel);
                        }
                    }
                });
        }
    }
}
