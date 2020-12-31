// ============================================================================
// 
// 再生制御を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class PlayerService : ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public PlayerService(HttpClient httpClient)
				: base(httpClient, YbdConstants.URL_PLAYER)
		{
		}

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


	}
}
