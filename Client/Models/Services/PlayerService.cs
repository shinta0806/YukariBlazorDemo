// ============================================================================
// 
// 再生制御を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class PlayerService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public PlayerService(HttpClient httpClient)
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
		// 再生中（または一時停止中）の曲を取得
		// --------------------------------------------------------------------
		public async Task<RequestSong?> GetPlayingSongAsync()
		{
			return await HttpClient.GetFromJsonAsync<RequestSong?>(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PLAYING);
		}

		// --------------------------------------------------------------------
		// 次の曲を再生
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> NextAsync()
		{
			return await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_NEXT, 0);
		}

		// --------------------------------------------------------------------
		// 現在の曲を再生または一時停止する
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> PlayOrPauseAsync()
		{
			return await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PLAY_OR_PAUSE, 0);
		}

		// --------------------------------------------------------------------
		// 前の曲を再生
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> PrevAsync()
		{
			return await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PREV, 0);
		}

		// --------------------------------------------------------------------
		// 再生 API の状態を取得
		// --------------------------------------------------------------------
		public async Task<String> Status()
		{
			String? status;
			try
			{
				status = await HttpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_STATUS);
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
