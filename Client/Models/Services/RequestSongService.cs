// ============================================================================
// 
// リクエストされた曲の一覧を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class RequestSongService : ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public RequestSongService(HttpClient httpClient)
				: base(httpClient, YbdConstants.URL_REQUEST_SONGS)
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 予約を追加
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> AddRequestSongAsync(RequestSong requestSong)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST, requestSong);
			return response.StatusCode switch
			{
				_ => DefaultErrorMessage(response.StatusCode),
			};
		}

		// --------------------------------------------------------------------
		// 予約を削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteRequestSongAsync(Int32 requestSongId)
		{
			(HttpStatusCode statusCode, _) = await DeleteAsync<String>(YbdConstants.URL_REQUEST, requestSongId.ToString());
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => "予約がありません。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// 予約をすべて削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteRequestSongAllAsync()
		{
			(HttpStatusCode statusCode, _) = await DeleteAsync<String>(YbdConstants.URL_REQUEST + YbdConstants.URL_ALL);
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => "予約がありません。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// 予約一覧を取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 予約一ページ分, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, RequestSong[], Int32)> GetRequestSongsAsync(String? query)
		{
			(HttpStatusCode statusCode, RequestSong[] reqestSongs, Int32 totalCount) = await GetArrayAsync<RequestSong>(YbdConstants.URL_REQUEST, query);
			return (DefaultErrorMessage(statusCode), reqestSongs, totalCount);
		}

		// --------------------------------------------------------------------
		// 予約者名一覧を取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 予約者一ページ分, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, String[], Int32)> GetUserNamesAsync()
		{
			(HttpStatusCode statusCode, String[] names, Int32 totalCount) = await GetArrayAsync<String>(YbdConstants.URL_GUEST_USER_NAMES);
			return (DefaultErrorMessage(statusCode), names, totalCount);
		}

		// --------------------------------------------------------------------
		// 予約を下へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveDownRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_DOWN);
			return response.StatusCode switch
			{
				HttpStatusCode.NotAcceptable => "下へ移動できません。",
				_ => DefaultErrorMessage(response.StatusCode),
			};
		}

		// --------------------------------------------------------------------
		// 予約を次の再生位置へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveNextRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_NEXT);
			return response.StatusCode switch
			{
				HttpStatusCode.Conflict => "再生中です。",
				HttpStatusCode.NotAcceptable => "移動できません。",
				_ => DefaultErrorMessage(response.StatusCode),
			};
		}

		// --------------------------------------------------------------------
		// 予約を上へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveUpRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_UP);
			return response.StatusCode switch
			{
				HttpStatusCode.NotAcceptable => "上へ移動できません。",
				_ => DefaultErrorMessage(response.StatusCode),
			};
		}
	}
}
