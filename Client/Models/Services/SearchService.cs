// ============================================================================
// 
// 検索結果を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

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
			AvailableSong? result = await _httpClient.GetFromJsonAsync<AvailableSong>(YbdConstants.URL_API + YbdConstants.URL_SEARCH + YbdConstants.URL_ID + id);

			if (String.IsNullOrEmpty(result?.Id))
			{
				// 存在しない ID が指定された場合（ユーザーが URL を書き換えた場合など）はサーバー側で空の AvailableSong を返す
				// クライアント側には null を返す
				result = null;
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
