using AutoMapper;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
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
            CreateMap<Group, GroupRequest>();
            CreateMap<User, UserRequest>();
            CreateMap<UserRequest, User>();
            CreateMap<GroupRequest, Group>();

            CreateMap<UserResponse, User>();
        }
    }
}
