// ============================================================================
// 
// サーバー側の API を呼ぶ機能を提供する基底クラス
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
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Client.Models.Services
{
	public abstract class ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ApiService(HttpClient httpClient, String baseUrl)
		{
			HttpClient = httpClient;
			BaseUrl = baseUrl;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// HTTP 通信用
		public HttpClient HttpClient { get; }

		// API URL
		public String BaseUrl { get; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// API の状態を取得
		// --------------------------------------------------------------------
		public async Task<String> StatusAsync()
		{
			String? status;
			try
			{
				status = await HttpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + BaseUrl + YbdConstants.URL_STATUS);
				if (status == null)
				{
					status = API_STATUS_ERROR_CANNOT_GET;
				}
			}
			catch (Exception)
			{
				status = API_STATUS_ERROR_CANNOT_CONNECT;
			}
			return status;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// API を呼びだした結果の配列を取得
		// --------------------------------------------------------------------
		protected async Task<(T[], Int32)> GetArrayAsync<T>(String leafUrl, String? query = null)
		{
			T[]? results = null;
			Int32 numResults = 0;
			using HttpResponseMessage response = await HttpClient.GetAsync(YbdConstants.URL_API + BaseUrl + leafUrl + query);
			Dictionary<String, String> parameters = ClientCommon.AnalyzeEntityTag(response.Headers.ETag);
			results = await response.Content.ReadFromJsonAsync<T[]>();
			if (parameters.TryGetValue(YbdConstants.RESULT_PARAM_NAME_COUNT, out String? value))
			{
				Int32.TryParse(value, out numResults);
			}
			else
			{
				numResults = results?.Length ?? 0;
			}
			if (results == null)
			{
				return (new T[0], 0);
			}
			return (results, numResults);
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// API 状態取得
		private const String API_STATUS_ERROR_CANNOT_GET = "状態を取得できません。";
		private const String API_STATUS_ERROR_CANNOT_CONNECT = "サーバーと通信できません。";


	}
}
