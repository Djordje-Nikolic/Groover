using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Interfaces
{
    public interface IApiConfiguration
    {
        string BaseAddress { get; set; }
        string APIKey { get; set; }
        string GroupChatHubAddress { get; set; }
    }
}
