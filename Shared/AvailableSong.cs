// ============================================================================
// 
// リクエスト可能な曲（ストレージに保存されている曲）の情報
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
	public class AvailableSong
	{
		public Int32 Id { get; set; }

		public String Path { get; set; } = String.Empty;

		public String? SongName { get; set; }

		public String? TieUpName { get; set; }

	}
}
