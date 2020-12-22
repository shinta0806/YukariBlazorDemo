// ============================================================================
// 
// サムネイルの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukariBlazorDemo.Server.Database
{
	[Table("t_thumbnail")]
	public class Thumbnail
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID
		[Key]
		public Int32 Id { get; set; }

		// 動画ファイルのパス
		[Required]
		public String Path { get; set; } = String.Empty;

		// サムネイル画像データ
		[Required]
		public Byte[] Bitmap { get; set; } = new Byte[0];

		// サムネイル画像形式
		[Required]
		public String Mime { get; set; } = String.Empty;

	}
}
