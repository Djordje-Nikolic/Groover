using AutoMapper;
using Groover.BL.Models.DTOs;
using Groover.IdentityDB.MySqlDb.Entities;
using Groover.ChatDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.BL.Models.Chat.DTOs;
using Groover.ChatDB;

namespace Groover.BL.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateIdentityDBMaps();
            CreateChatDBMaps();
        }

        public void CreateIdentityDBMaps()
        {
            CreateMap<RegisterDTO, User>();
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
            CreateMap<Group, GroupDTO>();
            CreateMap<GroupDTO, Group>();
            CreateMap<GroupUser, GroupUserDTO>();
            CreateMap<RefreshToken, RefreshTokenDTO>();
        }

        public void CreateChatDBMaps()
        {
            CreateMap<Message, FullMessageDTO>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dst => dst.Type, opt => opt.MapFrom(src => Enum.Parse(typeof(Chat.MessageType), src.Type.ToString())));
            CreateMap<Message, TextMessageDTO>()
                .BeforeMap((src, dst) => 
                {
                    if (src.Type != ChatDB.Models.MessageType.Text)
                        throw new InvalidCastException("Source message is not a text message.");
                })
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dst => dst.Type, opt => opt.Ignore());
            CreateMap<Message, ImageMessageDTO>()
                .BeforeMap((src, dst) =>
                {
                    if (src.Type != ChatDB.Models.MessageType.Image)
                        throw new InvalidCastException("Source message is not an image message.");
                })
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dst => dst.Type, opt => opt.Ignore());
            CreateMap<Message, TrackMessageDTO>()
                .BeforeMap((src, dst) =>
                {
                    if (src.Type != ChatDB.Models.MessageType.Track)
                        throw new InvalidCastException("Source message is not a track message.");

                    if (src.TrackDuration == null)
                        throw new InvalidCastException("Track duration not defined in the source message.");
                })
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dst => dst.Type, opt => opt.Ignore());

            CreateMap<TextMessageDTO, Message>()
                .ForMember(dst => dst.Id, opt => opt.Ignore());
            CreateMap<ImageMessageDTO, Message>()
                .ForMember(dst => dst.Id, opt => opt.Ignore());
            CreateMap<TrackMessageDTO, Message>()
                .ForMember(dst => dst.Id, opt => opt.Ignore());

            CreateMap<Track, TrackDTO>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()));
            CreateMap<TrackDTO, Track>()
                .ForMember(dst => dst.Id, opt => opt.Ignore());

            CreateMap<PageParamsDTO, PageParams>();
            CreateMap<PageParams, PageParamsDTO>();
        }
    }
}
