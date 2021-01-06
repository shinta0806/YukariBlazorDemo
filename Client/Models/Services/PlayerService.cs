// ============================================================================
// 
// 再生制御を提供
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
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> NextAsync()
		{
			using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_NEXT, 0);
			if (response.IsSuccessStatusCode)
			{
				return String.Empty;
			}
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "次の曲を再生できませんでした。未再生の曲があるか確認してください。";
				default:
					return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
			}
		}

		// --------------------------------------------------------------------
		// 現在の曲を再生または一時停止する
		// --------------------------------------------------------------------
		public async Task<String> PlayOrPauseAsync()
		{
			using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PLAY_OR_PAUSE, 0);
			if (response.IsSuccessStatusCode)
			{
				return String.Empty;
			}
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "再生／一時停止できませんでした。曲が予約されているか確認してください。";
				default:
					return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
			}
		}

		// --------------------------------------------------------------------
		// 前の曲を再生
		// --------------------------------------------------------------------
		public async Task<String> PrevAsync()
		{
			using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PREV, 0);
			if (response.IsSuccessStatusCode)
			{
				return String.Empty;
			}
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "前の曲を再生できませんでした。再生済みの曲があるか確認してください。";
				default:
					return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
			}
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

	}
}
