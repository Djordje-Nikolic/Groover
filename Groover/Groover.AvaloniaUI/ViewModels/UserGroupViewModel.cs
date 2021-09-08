using Groover.AvaloniaUI.Models;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class UserGroupViewModel : ViewModelBase, IComparable<UserGroupViewModel>
    {
        [Reactive]
        public GroupViewModel Group { get; set; }
        [Reactive]
        public GrooverGroupRole GroupRole { get; set; }

        public int CompareTo(UserGroupViewModel? other)
        {
            if (other == null)
                return 10;

            if (GroupRole == other.GroupRole)
                return 0;

            if (GroupRole == GrooverGroupRole.Admin && other.GroupRole == GrooverGroupRole.Member)
                return 5;

            if (GroupRole == GrooverGroupRole.Member && other.GroupRole == GrooverGroupRole.Admin)
                return -5;

            return 0;
        }
    }
}
