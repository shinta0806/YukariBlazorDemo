// ============================================================================
// 
// 予約可能な曲（ストレージに保存されている曲）の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Shared
{
	[Table("t_available_song")]
	public class AvailableSong
	{
		[Key]
		[Column("KeyId")]
		public Int32 Id { get; set; }

		[Required]
		public String Path { get; set; } = String.Empty;

		public String? SongName { get; set; }

		public String? TieUpName { get; set; }

	}
}
