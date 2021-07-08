using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Utils;
using Groover.BL.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groover.API.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LoggedInDTO, LogInResponse>();
            CreateMap<RegisterRequest, RegisterDTO>();
            CreateMap<LogInRequest, LogInDTO>();
            CreateMap<UserDTO, UserResponse>();

            CreateMap<GroupUserDTO, GroupUserResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));
            CreateMap<GroupUserDTO, UserGroupResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));

            CreateMap<UserDTO, UserLiteResponse>();
            CreateMap<GroupDTO, GroupLiteResponse>();
            CreateMap<GroupUserDTO, GroupUserLiteResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));

            CreateMap<GroupDTO, GroupResponse>();
            CreateMap<CreateGroupRequest, GroupDTO>();
            CreateMap<UpdateGroupRequest, GroupDTO>();
            CreateMap<ConfirmEmailRequest, ConfirmEmailDTO>();
            CreateMap<RefreshTokenDTO, RefreshTokenResponse>();
        }
    }
}
