using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public class GroupService : IGroupService
    {
        private IApiService _apiService;

        public GroupService(IApiService apiService)
        {
            _apiService = apiService;
        }
    }
}
