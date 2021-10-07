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
                        ICollection<UserGroupViewModel> userGroupViewModels = mapper.Map<ICollection<UserGroupViewModel>>(src.UserGroups);
                        dest.UserGroupsCache.AddOrUpdate(userGroupViewModels);
                    }
                });
            CreateMap<Group, GroupViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.GroupUsers != null)
                    {
                        ICollection<GroupUserViewModel> groupUserViewModels = mapper.Map<ICollection<GroupUserViewModel>>(src.GroupUsers);
                        dest.GroupUsersCache.AddOrUpdate(groupUserViewModels);
                    }
                });

            CreateMap<UserViewModel, User>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.UserGroups != null)
                    {
                        dest.UserGroups = mapper.Map<ICollection<UserGroup>>(src.UserGroups);
                    }
                });
            CreateMap<GroupViewModel, Group>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.SortedGroupUsers != null)
                    {
                        dest.GroupUsers = mapper.Map<ICollection<GroupUser>>(src.SortedGroupUsers);
                    }
                });

            CreateMap<UserResponse, UserViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.UserGroups != null)
                    {
                        ICollection<UserGroupViewModel> userGroupViewModels = mapper.Map<ICollection<UserGroupViewModel>>(src.UserGroups);
                        dest.UserGroupsCache.AddOrUpdate(userGroupViewModels);
                    }
                });
            CreateMap<GroupResponse, GroupViewModel>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .AfterMap((src, dest, context) =>
                {
                    var mapper = context.Mapper;
                    if (src.GroupUsers != null)
                    {
                        ICollection<GroupUserViewModel> groupUserViewModels = mapper.Map<ICollection<GroupUserViewModel>>(src.GroupUsers);
                        dest.GroupUsersCache.AddOrUpdate(groupUserViewModels);
                    }
                });

            CreateMap<TrackResponse, Track>();
        }
    }
}
