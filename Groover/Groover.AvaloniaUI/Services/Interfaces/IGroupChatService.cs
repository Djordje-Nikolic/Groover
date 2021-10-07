using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IGroupChatService
    {
        Task<PagedResponse<ICollection<Message>>> GetMessagesAsync(int groupId, DateTime afterDateTime, PageParams pageParams);
        Task<PagedResponse<ICollection<Message>>> GetMessagesAsync(int groupId, PageParams pageParams);
        Task<BaseResponse> SendImageMessageAsync(ImageMessageRequest imageMessageRequest);
        Task<BaseResponse> SendTextMessageAsync(TextMessageRequest textMessageRequest);
        Task<BaseResponse> SendTrackMessageAsync(TrackMessageRequest trackMessageRequest, string pathToFile);
    }
}
