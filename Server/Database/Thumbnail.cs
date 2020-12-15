// ============================================================================
// 
// サムネイルの情報
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
using System.Threading.Tasks;

namespace YukariBlazorDemo.Server.Database
{
	[Table("t_thumbnail")]
	public class Thumbnail
	{
		[Key]
		public Int32 Id { get; set; }

		[Required]
		public String Path { get; set; } = String.Empty;

		[Required]
		public Byte[] Bitmap { get; set; } = new Byte[0];

		[Required]
		public String Mime { get; set; } = String.Empty;

	}
}
