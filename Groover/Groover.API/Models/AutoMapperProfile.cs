using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Utils;
using Groover.BL.Models.Chat.DTOs;
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
            CreateMap<UserDTO, UserResponse>()
                .ForMember(d => d.AvatarBase64, options =>
                    options.MapFrom(s => s.AvatarImage != null ? Convert.ToBase64String(s.AvatarImage) : null));
            CreateMap<UserDTO, UserDataResponse>()
                .ForMember(d => d.AvatarBase64, options =>
                    options.MapFrom(s => s.AvatarImage != null ? Convert.ToBase64String(s.AvatarImage) : null));
            CreateMap<UserResponse, UserDataResponse>();
            CreateMap<UpdateUserRequest, UserDTO>()
                .ForMember(d => d.AvatarImage, options =>
                    options.MapFrom(s => !string.IsNullOrWhiteSpace(s.AvatarBase64) ? Convert.FromBase64String(s.AvatarBase64) : null));

            CreateMap<GroupUserDTO, GroupUserResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));
            CreateMap<GroupUserDTO, UserGroupResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));

            CreateMap<UserDTO, UserLiteResponse>()
                .ForMember(d => d.AvatarBase64, options =>
                    options.MapFrom(s => s.AvatarImage != null ? Convert.ToBase64String(s.AvatarImage) : null));
            CreateMap<GroupDTO, GroupLiteResponse>()
                .ForMember(d => d.ImageBase64, options =>
                    options.MapFrom(s => s.Image != null ? Convert.ToBase64String(s.Image) : null));
            CreateMap<GroupUserDTO, GroupUserLiteResponse>()
                .ForMember(d => d.GroupRole, options => options.MapFrom(s => s.GroupRole.ToString()));

            CreateMap<GroupDTO, GroupResponse>()
                .ForMember(d => d.ImageBase64, options =>
                    options.MapFrom(s => s.Image != null ? Convert.ToBase64String(s.Image) : null));
            CreateMap<GroupDTO, GroupDataResponse>()
                .ForMember(d => d.ImageBase64, options =>
                    options.MapFrom(s => s.Image != null ? Convert.ToBase64String(s.Image) : null));
            CreateMap<GroupResponse, GroupDataResponse>();
            CreateMap<CreateGroupRequest, GroupDTO>()
                .ForMember(d => d.Image, options => 
                    options.MapFrom(s => !string.IsNullOrWhiteSpace(s.ImageBase64) ? Convert.FromBase64String(s.ImageBase64): null));
            CreateMap<UpdateGroupRequest, GroupDTO>()
                .ForMember(d => d.Image, options =>
                    options.MapFrom(s => !string.IsNullOrWhiteSpace(s.ImageBase64) ? Convert.FromBase64String(s.ImageBase64) : null)); ;
            CreateMap<ConfirmEmailRequest, ConfirmEmailDTO>();
            CreateMap<RefreshTokenDTO, RefreshTokenResponse>();

            CreateMap<ImageMessageRequest, ImageMessageDTO>()
                .ForMember(d => d.Image, options =>
                    options.MapFrom(s => !string.IsNullOrWhiteSpace(s.Image) ? Convert.FromBase64String(s.Image) : null));
            CreateMap<TextMessageRequest, TextMessageRequest>();
            CreateMap<TrackMessageRequest, TrackMessageDTO>();
            CreateMap<FullMessageDTO, FullMessageResponse>()
                .ForMember(d => d.Type, options =>
                    options.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.Image, options =>
                    options.MapFrom(s => s.Image != null ? Convert.ToBase64String(s.Image) : null));

            CreateMap(typeof(PagedDataDTO<>), typeof(PagedResponse<>));
            CreateMap<PageParamsDTO, PageParamsResponse>();
        }
    }
}
