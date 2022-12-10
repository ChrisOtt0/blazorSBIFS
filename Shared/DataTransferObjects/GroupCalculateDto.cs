using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazorSBIFS.Shared.DataTransferObjects
{
	public enum OutputForm
	{
		str,
		html,
		pdf
	}

	public class GroupCalculateDto : IJson
	{
		public int GroupID { get; set; }
		public OutputForm OutputForm { get; set; }
	}
}
