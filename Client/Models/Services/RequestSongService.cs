// ============================================================================
// 
// リクエストされた曲の一覧を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> AddRequestAsync(RequestSong requestSong)
		{
			return await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_REQUEST, requestSong);
		}

		// --------------------------------------------------------------------
		// 予約をすべて削除
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> DeleteAllAsync()
		{
			return await HttpClient.PutAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_DELETE_ALL, 0);
		}

		// --------------------------------------------------------------------
		// 予約一覧を取得
		// --------------------------------------------------------------------
		public async Task<(RequestSong[], Int32)> GetRequestSongsAsync(String? query)
		{
			return await GetArrayAsync<RequestSong>(YbdConstants.URL_LIST, query);
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
