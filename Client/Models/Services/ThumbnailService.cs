// ============================================================================
// 
// サムネイルを提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// サムネイル画像を取得する場合は本サービスを使わずに HTML から直接 URL を指定する
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class ThumbnailService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ThumbnailService(HttpClient httpClient)
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
		// 検索 API の状態を取得
		// --------------------------------------------------------------------
		public async Task<String> Status()
		{
			String? status;
			try
			{
				status = await HttpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + YbdConstants.URL_THUMBNAIL + YbdConstants.URL_STATUS);
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
