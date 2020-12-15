// ============================================================================
// 
// サーバー側で共通して使われる関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Misc
{
	public class ServerCommon
	{
		public static AvailableSong? AvailableSongById(String? id)
		{
			AvailableSong? result = null;
			try
			{
				if (!Int32.TryParse(id, out Int32 idNum))
				{
					throw new Exception();
				}
				using AvailableSongContext availableSongContext = new();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception();
				}
				result = availableSongContext.AvailableSongs.FirstOrDefault(x => x.Id == idNum);
			}
			catch (Exception)
			{
			}
			return result;
		}
	}
}
