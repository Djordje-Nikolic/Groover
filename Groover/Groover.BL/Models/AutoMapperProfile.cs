using AutoMapper;
using Groover.BL.Models.DTOs;
using Groover.DB.MySqlDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterDTO, User>();
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
            CreateMap<Group, GroupDTO>();
            CreateMap<GroupDTO, Group>();
            CreateMap<GroupUser, GroupUserDTO>();
            CreateMap<RefreshToken, RefreshTokenDTO>();
        }
    }
}
