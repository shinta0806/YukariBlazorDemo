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
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST, requestSong);
			switch (response.StatusCode)
			{
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 予約を削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await mHttpClient.DeleteAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "予約がありません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 予約を一括削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteRequestSongAtOnceAsync(String param)
		{
			using HttpResponseMessage response = await mHttpClient.DeleteAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + YbdConstants.URL_AT_ONCE + param);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "予約がありません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 予約一覧を取得
		// --------------------------------------------------------------------
		public async Task<(RequestSong[], Int32)> GetRequestSongsAsync(String? query)
		{
			return await GetArrayAsync<RequestSong>(YbdConstants.URL_REQUEST, query);
		}

		// --------------------------------------------------------------------
		// 予約者名一覧を取得
		// --------------------------------------------------------------------
		public async Task<(String[], Int32)> GetUserNamesAsync()
		{
			return await GetArrayAsync<String>(YbdConstants.URL_GUEST_USER_NAMES);
		}

		// --------------------------------------------------------------------
		// 予約を下へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveDownRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_DOWN);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "下へ移動できません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 予約を次の再生位置へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveNextRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_NEXT);
			switch (response.StatusCode)
			{
				case HttpStatusCode.Conflict:
					return "再生中です。";
				case HttpStatusCode.NotAcceptable:
					return "移動できません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 予約を上へ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> MoveUpRequestSongAsync(Int32 requestSongId)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_UP);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "上へ移動できません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}
	}
}
