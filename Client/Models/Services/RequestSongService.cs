// ============================================================================
// 
// リクエストされた曲の一覧を提供
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
	public class RequestSongService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public RequestSongService(HttpClient httpClient)
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
		public async Task<IEnumerable<RequestSong>> GetRequestSongsAsync()
		{
			RequestSong[]? songs = null;
			try
			{
				songs = await HttpClient.GetFromJsonAsync<RequestSong[]>(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS);
			}
			catch (Exception)
			{
			}
			if (songs == null)
			{
				return new RequestSong[0];
			}
			return songs;
		}

		// --------------------------------------------------------------------
		// 検索 API の状態を取得
		// --------------------------------------------------------------------
		public async Task<String> Status()
		{
			String? status;
			try
			{
				status = await HttpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS + YbdConstants.URL_STATUS);
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
