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

using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class SearchService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public SearchService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// HTTP 通信用
		public HttpClient HttpClient { get; }

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
		public async Task<IEnumerable<AvailableSong>> SearchByWordAsync(String? query)
		{
			AvailableSong[]? results = null;
			try
			{
				results = await HttpClient.GetFromJsonAsync<AvailableSong[]>(YbdConstants.URL_API + YbdConstants.URL_SEARCH + YbdConstants.URL_WORD + query);
			}
			catch (Exception)
			{
			}
			if (results == null)
			{
				return new AvailableSong[0];
			}
			return results;
		}
	}
}
