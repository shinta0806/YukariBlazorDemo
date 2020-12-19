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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Shared
{
	public class RequestSong : ISongProperty
	{


		public Int32 Id { get; set; }

		public Int32 Sort { get; set; }

		public String SongName { get; set; } = String.Empty;

		public String TieUpName { get; set; } = String.Empty;

		public String ArtistName { get; set; } = String.Empty;

		public String Maker { get; set; } = String.Empty;

		public String Worker { get; set; } = String.Empty;

		public String Path { get; set; } = String.Empty;

		[Required(ErrorMessage = "リクエスト者を入力してください。")]
		public String User { get; set; } = String.Empty;

		public String? Comment { get; set; }

		public PlayStatus PlayStatus { get; set; }

#if false
		public void Import(AvailableSong availableSong)
		{
			Path = availableSong.Path;
			SongName = availableSong.SongName;
			TieUpName = availableSong.TieUpName;
			ArtistName = availableSong.ArtistName;
			Maker = availableSong.Maker;
			Worker = availableSong.Worker;
		}
#endif

		public Boolean IsValid()
		{
			return !String.IsNullOrEmpty(Path) && !String.IsNullOrEmpty(User);
		}
	}
}
