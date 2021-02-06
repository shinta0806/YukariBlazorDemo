// ============================================================================
// 
// 検索結果を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Net;
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
		// 履歴で曲を検索
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 曲)
		// --------------------------------------------------------------------
		public async Task<(String, AvailableSong?)> SearchByHistoryAsync(HistorySong historySong)
		{
			(HttpStatusCode statusCode, AvailableSong? availableSong) = await PostAndGetFromJsonAsync<AvailableSong, HistorySong>(YbdConstants.URL_HISTORY, historySong);
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => ("指定された曲が見つかりません。", availableSong),
				_ => (DefaultErrorMessage(statusCode), availableSong),
			};
		}

		// --------------------------------------------------------------------
		// AvailableSong.Id で曲を検索
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 曲)
		// --------------------------------------------------------------------
		public async Task<(String, AvailableSong?)> SearchByIdAsync(String? id)
		{
			(HttpStatusCode statusCode, AvailableSong? availableSong) = await GetFromJsonAsync<AvailableSong>(YbdConstants.URL_ID, id);
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => ("指定された曲が見つかりません。", availableSong),
				_ => (DefaultErrorMessage(statusCode), availableSong),
			};
		}

		// --------------------------------------------------------------------
		// キーワードで曲を検索
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 検索結果 1 ページ分, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, AvailableSong[], Int32)> SearchByWordAsync(String? query)
		{
			(HttpStatusCode statusCode, AvailableSong[] availableSongs, Int32 totalCount) = await GetArrayFromJsonAsync<AvailableSong>(YbdConstants.URL_WORD, query);
			return (DefaultErrorMessage(statusCode), availableSongs, totalCount);
		}
	}
}
