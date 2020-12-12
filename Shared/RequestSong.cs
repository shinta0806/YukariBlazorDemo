// ============================================================================
// 
// リクエストされた曲の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Shared
{
	public class RequestSong
	{
		public Int32 Id { get; set; }

		public Int32 Sort { get; set; }

		public String Path { get; set; } = String.Empty;

		public String? SongName { get; set; }

		public String? TieUpName { get; set; }

		public String? User { get; set; }

		public String? Comment { get; set; }
	}
}
