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
		public async Task<String> AddRequestAsync(RequestSong requestSong)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST, requestSong);
			if (response.IsSuccessStatusCode)
			{
				return String.Empty;
			}
			switch (response.StatusCode)
			{
				default:
					return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
			}
		}

		// --------------------------------------------------------------------
		// 予約をすべて削除
		// --------------------------------------------------------------------
		public async Task<String> DeleteAllAsync()
		{
			using HttpResponseMessage response = await mHttpClient.DeleteAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST + "-1/");
			if (response.IsSuccessStatusCode)
			{
				return String.Empty;
			}
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "予約がありません。";
				default:
					return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
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
	}
}
