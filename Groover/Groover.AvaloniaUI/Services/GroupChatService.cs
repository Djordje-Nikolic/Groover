using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public class GroupChatService : GrooverService, IGroupChatService
    {
        private readonly Controller _controller;
        private readonly FileExtensionContentTypeProvider _mimeContentTypeProvider;
        public GroupChatService(IApiService apiService, ICacheWrapper cacheWrapper) : base(apiService, cacheWrapper)
        {
            _mimeContentTypeProvider = new FileExtensionContentTypeProvider();
            _controller = Controller.GroupChat;
        }

        public async Task<TrackResponse> GetLoadedTrackAsync(int groupId, string trackId, bool getFromCacheIfAvailable)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("trackId", trackId);
            TrackResponse trackResponse = await this.SendRequestAsync<TrackResponse>(queryParams, HttpMethod.Get, _controller, "getTrack");

            if (trackResponse.IsSuccessful)
            {
                string uniqueFilename = $"{trackResponse.Id}.{trackResponse.Extension}";
                string? filePath = null;

                if (getFromCacheIfAvailable)
                {
                    filePath = _cacheWrapper.LocateCachedFile(uniqueFilename, FileType.Track);
                }

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    trackResponse.TrackFileResponse = new FileResponse()
                    {
                        FilePath = filePath,
                        IsSuccessful = true
                    };
                }
                else
                {
                    trackResponse.TrackFileResponse = await this.SendFileRequestAsync(trackResponse.TrackFileLink, uniqueFilename, FileType.Track);
                }
            }

            return trackResponse;
        }

        public async Task<PagedResponse<ICollection<Message>>> GetMessagesAsync(int groupId, PageParams pageParams)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("pageSize", pageParams.PageSize.ToString());
            if (pageParams.PagingState != null) queryParams.Add("pagingState", pageParams.PagingState);
            return await this.SendRequestAsync<PagedResponse<ICollection<Message>>>(queryParams, HttpMethod.Get, _controller, "getMessages");
        }

        public async Task<PagedResponse<ICollection<Message>>> GetMessagesAsync(int groupId, DateTime afterDateTime, PageParams pageParams)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("pageSize", pageParams.PageSize.ToString());
            queryParams.Add("createdAfter", afterDateTime.ToUniversalTime().ToString(Message.DateTimeFormat));
            if (pageParams.PagingState != null) queryParams.Add("pagingState", pageParams.PagingState.ToString());
            return await this.SendRequestAsync<PagedResponse<ICollection<Message>>>(queryParams, HttpMethod.Get, _controller, "getLatestMessages");
        }

        public async Task<CollectionResponse<Message>> GetMessagesAsync(int groupId, DateTime afterDateTime)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("createdAfter", afterDateTime.ToUniversalTime().ToString(Message.DateTimeFormat));
            return await this.SendRequestAsync<CollectionResponse<Message>>(queryParams, HttpMethod.Get, _controller, "getAllLatestMessages");
        }

        public async Task<CollectionResponse<Message>> GetMessagesAsync(int groupId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            return await this.SendRequestAsync<CollectionResponse<Message>>(queryParams, HttpMethod.Get, _controller, "getAllMessages");
        }

        public async Task<BaseResponse> SendTextMessageAsync(TextMessageRequest textMessageRequest)
        {
            return await this.SendRequestAsync<TextMessageRequest, BaseResponse>(textMessageRequest, HttpMethod.Post, _controller, "sendTextMessage");
        }

        public async Task<BaseResponse> SendImageMessageAsync(ImageMessageRequest imageMessageRequest)
        {
            return await this.SendRequestAsync<ImageMessageRequest, BaseResponse>(imageMessageRequest, HttpMethod.Post, _controller, "sendImageMessage");
        }

        public async Task<BaseResponse> SendTrackMessageAsync(TrackMessageRequest trackMessageRequest, string pathToFile)
        {
            using (var multipartContent = new MultipartFormDataContent())
            {
                var jsonRequest = JsonConvert.SerializeObject(trackMessageRequest);
                var messageDataContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                multipartContent.Add(messageDataContent, "messageData");

                var fileContent = new StreamContent(File.OpenRead(pathToFile));
                if (!_mimeContentTypeProvider.TryGetContentType(Path.GetFileName(pathToFile), out string contentType))
                {
                    contentType = "application/octet-stream";
                }
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                multipartContent.Add(fileContent, "trackFile");

                return await this.SendRequestAsync<BaseResponse>(multipartContent, HttpMethod.Post, _controller, "sendTrackMessage");
            }
        }
    }
}
