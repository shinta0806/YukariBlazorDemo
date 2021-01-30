﻿// ============================================================================
// 
// サーバー側の API を呼ぶ機能を提供する基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
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
			_httpClient = httpClient;
			_baseUrl = baseUrl;
		}

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
				status = await _httpClient.GetFromJsonAsync<String>(YbdConstants.URL_API + _baseUrl + YbdConstants.URL_STATUS);
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
		// protected メンバー変数
		// ====================================================================

		// HTTP 通信用
		protected HttpClient _httpClient;

		// ====================================================================
		// protected static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// statusCode に対応するデフォルトのエラーメッセージ
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		protected static String DefaultErrorMessage(HttpStatusCode statusCode)
		{
			if (IsSuccessStatusCode(statusCode))
			{
				return String.Empty;
			}
			return statusCode switch
			{
				HttpStatusCode.BadRequest => ClientConstants.ERROR_MESSAGE_BAD_REQUEST,
				HttpStatusCode.InternalServerError => ClientConstants.ERROR_MESSAGE_INTERNAL_SERVER_ERROR,
				HttpStatusCode.Unauthorized => ClientConstants.ERROR_MESSAGE_UNAUTHORIZED,
				_ => ClientConstants.ERROR_MESSAGE_UNEXPECTED + "（" + statusCode.ToString() + "）",
			};
		}

		// --------------------------------------------------------------------
		// HTTP 応答が成功したかどうか
		// --------------------------------------------------------------------
		protected static Boolean IsSuccessStatusCode(HttpStatusCode statusCode)
		{
			return HttpStatusCode.OK <= statusCode && statusCode < HttpStatusCode.MultipleChoices;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// API を呼びだした結果の配列（1 ページ分）と結果の総数を取得
		// --------------------------------------------------------------------
		protected async Task<(T[], Int32)> GetArrayAsync<T>(String leafUrl, String? query = null)
		{
			T[]? results = null;
			Int32 numResults = 0;
			using HttpResponseMessage response = await _httpClient.GetAsync(YbdConstants.URL_API + _baseUrl + leafUrl + query);
			if (response.IsSuccessStatusCode)
			{
				Dictionary<String, String> parameters = ClientCommon.AnalyzeEntityTag(response.Headers.ETag);
				results = await response.Content.ReadFromJsonAsync<T[]>();
				if (parameters.TryGetValue(YbdConstants.RESULT_PARAM_NAME_COUNT, out String? value))
				{
					_ = Int32.TryParse(value, out numResults);
				}
				else
				{
					numResults = results?.Length ?? 0;
				}
			}
			if (results == null)
			{
				return (Array.Empty<T>(), 0);
			}
			return (results, numResults);
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// API 状態取得
		private const String API_STATUS_ERROR_CANNOT_GET = "状態を取得できません。";
		private const String API_STATUS_ERROR_CANNOT_CONNECT = "サーバーと通信できません。";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// API URL
		private readonly String _baseUrl;
	}
}
