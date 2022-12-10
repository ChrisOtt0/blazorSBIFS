using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class GroupUserDto : IJson
    {
        public GroupDto GroupRequest { get; set; } = new GroupDto();
        public UserDto UserRequest { get; set; } = new UserDto();
    }
}
