// ============================================================================
// 
// 検索結果を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class SearchService : ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public SearchService(HttpClient httpClient)
				: base(httpClient, YbdConstants.URL_SEARCH)
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// AvailableSong.Id で曲を検索
		// --------------------------------------------------------------------
		public async Task<AvailableSong?> SearchByIdAsync(String? id)
		{
			AvailableSong? result = null;
			try
			{
				result = await HttpClient.GetFromJsonAsync<AvailableSong>(YbdConstants.URL_API + YbdConstants.URL_SEARCH + YbdConstants.URL_ID + id);
			}
			catch (Exception)
			{
			}
			return result;
		}

		// --------------------------------------------------------------------
		// キーワードで曲を検索
		// --------------------------------------------------------------------
		public async Task<(AvailableSong[], Int32)> SearchByWordAsync(String? query)
		{
			return await GetArrayAsync<AvailableSong>(YbdConstants.URL_WORD, query);
		}

	}
}
