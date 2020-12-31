﻿// ============================================================================
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
	public class ThumbnailService : ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ThumbnailService(HttpClient httpClient)
				: base(httpClient, YbdConstants.URL_THUMBNAIL)
		{
		}
	}
}
