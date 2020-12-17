// ============================================================================
// 
// 予約された曲の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

		[Required(ErrorMessage = "リクエスト者を入力してください。")]
		public String User { get; set; } = String.Empty;

		public String? Comment { get; set; }

		public PlayStatus PlayStatus { get; set; }

		public void Import(AvailableSong availableSong)
		{
			Path = availableSong.Path;
			SongName = availableSong.SongName;
			TieUpName = availableSong.TieUpName;
		}

		public Boolean IsValid()
		{
			return !String.IsNullOrEmpty(Path) && !String.IsNullOrEmpty(User);
		}
	}
}
