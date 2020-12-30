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
		public async Task<(IEnumerable<AvailableSong>, Int32)> SearchByWordAsync(String? query)
		{
			AvailableSong[]? results = null;
			Int32 numResults = 0;
			try
			{
				using HttpResponseMessage response = await HttpClient.GetAsync(YbdConstants.URL_API + YbdConstants.URL_SEARCH + YbdConstants.URL_WORD + query);
				Dictionary<String, String> parameters = ClientCommon.AnalyzeEntityTag(response.Headers.ETag);
				results = await response.Content.ReadFromJsonAsync<AvailableSong[]>();
				if (parameters.TryGetValue(YbdConstants.RESULT_PARAM_NAME_COUNT, out String? value))
				{
					Int32.TryParse(value, out numResults);
				}
				else
				{
					numResults = results?.Length ?? 0;
				}
			}
			catch (Exception)
			{
			}
			if (results == null)
			{
				return (new AvailableSong[0], 0);
			}
			return (results, numResults);
		}

		// --------------------------------------------------------------------
		// 検索 API の状態を取得
		// --------------------------------------------------------------------
		public async Task<String> Status()
		{
			String? status;
			try
			{
				status = await HttpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + YbdConstants.URL_SEARCH + YbdConstants.URL_STATUS);
				if (status == null)
				{
					status = ClientConstants.API_STATUS_ERROR_CANNOT_GET;
				}
			}
			catch (Exception)
			{
				status = ClientConstants.API_STATUS_ERROR_CANNOT_CONNECT;
			}
			return status;
		}

	}
}
