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
			return await mHttpClient.GetFromJsonAsync<RequestSong?>(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PLAYING);
		}

		// --------------------------------------------------------------------
		// 次の曲を再生
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> NextAsync()
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_NEXT, 0);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "次の曲を再生できませんでした。未再生の曲があるか確認してください。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 指定の曲を再生済にする
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> Played(Int32 requestSongId)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_REQUEST + requestSongId,
					YbdConstants.REQUEST_PARAM_VALUE_PLAYED);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "対象の曲がありません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 現在の曲を再生または一時停止する
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> PlayOrPauseAsync()
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PLAY_OR_PAUSE, 0);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "再生／一時停止できませんでした。曲が予約されているか確認してください。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// 前の曲を再生
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> PrevAsync()
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_PLAYER + YbdConstants.URL_PREV, 0);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "前の曲を再生できませんでした。再生済みの曲があるか確認してください。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}
	}
}
