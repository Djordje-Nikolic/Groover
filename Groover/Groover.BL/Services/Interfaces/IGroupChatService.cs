using Groover.BL.Models.Chat.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Services.Interfaces
{
    public interface IGroupChatService
    {
        Task<FullMessageDTO> AddImageMessageAsync(ImageMessageDTO imageMessageDTO);
        Task<FullMessageDTO> AddTextMessageAsync(TextMessageDTO textMessageDTO);
        Task<FullMessageDTO> AddTrackMessageAsync(TrackMessageDTO trackMessageDTO, IFormFile trackFile);
        Task<PagedDataDTO<ICollection<FullMessageDTO>>> GetAllMessagesAsync(int groupId, PageParamsDTO pageParamsDTO);
        Task<PagedDataDTO<ICollection<FullMessageDTO>>> GetMessagesAsync(int groupId, DateTime dateTimeAfter, PageParamsDTO pageParamsDTO);
        Task<TrackDTO> GetLoadedTrackAsync(int groupId, string trackUuId);
        Task<TrackDTO> GetTrackMetadataAsync(int groupId, string trackUuId);
        Task<ICollection<FullMessageDTO>> GetAllMessagesAsync(int groupId);
        Task<ICollection<FullMessageDTO>> GetMessagesAsync(int groupId, DateTime createdAfterDT);
    }
}
