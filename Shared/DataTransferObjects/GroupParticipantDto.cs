using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
    public class GroupParticipantDto : IJson
    {
        public GroupDto? GroupRequest { get; set; }
        public EmailDto? ParticipantRequest { get; set; }
    }
}
