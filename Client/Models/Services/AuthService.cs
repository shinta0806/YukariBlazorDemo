// ============================================================================
// 
// 認証制御を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Authorization;
using YukariBlazorDemo.Shared.Authorization;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class AuthService : ApiService
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
				: base(httpClient, YbdConstants.URL_AUTH)
		{
			AuthenticationStateProvider = authenticationStateProvider;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 認証状態
		public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ユーザー登録
		// 返却される HttpResponseMessage は呼びだし元で Dispose() が必要
		// --------------------------------------------------------------------
		public async Task<HttpResponseMessage> AddUserAsync(LoginInfo registerInfo)
		{
			HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_ADD, registerInfo);
			await SetStateLoginAsync(response);
			return response;
		}

		// --------------------------------------------------------------------
		// 認証が必要な API からデータを取得
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		public async Task<T?> GetFromJsonAsync<T>(String uri)
		{
			if(AuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				await stateProvider.AddAuthorizationHeaderIfCanAsync();
			}
			using HttpResponseMessage response = await HttpClient.GetAsync(uri);
			if (!response.IsSuccessStatusCode)
			{
				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					await SetStateLogoutAsync();
				}
				return default(T);
			}
			return await response.Content.ReadFromJsonAsync<T>();
		}

		// --------------------------------------------------------------------
		// 公開ユーザー情報を取得
		// --------------------------------------------------------------------
		public async Task<PublicUserInfo?> GetPublicUserInfoAsync(Int32 id)
		{
			PublicUserInfo? result = null;
			try
			{
				result = await HttpClient.GetFromJsonAsync<PublicUserInfo>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_PUBLIC_USER_INFO + id);
			}
			catch (JsonException)
			{
				// 存在しない ID が指定された場合（ユーザーが URL を書き換えた場合など）はサーバー側で null を返し、JSON 化できないため JsonException 例外となる
				// クライアント側には null を返す
			}
			return result;
		}

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// --------------------------------------------------------------------
		public async Task<Boolean> IsAdminRegisteredAsync()
		{
			return await HttpClient.GetFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_ADMIN_REGISTERED);
		}

		// --------------------------------------------------------------------
		// 現在ログインしているか
		// --------------------------------------------------------------------
		public async Task<Boolean> IsLoggedInAsync()
		{
			return await GetFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_LOGGED_IN);
		}

		// --------------------------------------------------------------------
		// ログアウト
		// --------------------------------------------------------------------
		public async Task<Boolean> LogoutAsync()
		{
			Boolean result = true;
			try
			{
				using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGOUT, 0);
			}
			catch (Exception)
			{
				result = false;
			}
			if (!await SetStateLogoutAsync())
			{
				result = false;
			}
			return result;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// サーバーからのトークンを使ってログイン状態にする
		// --------------------------------------------------------------------
		private async Task<Boolean> SetStateLoginAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				return false;
			}

			String? idAndToken = await response.Content.ReadAsStringAsync();
			if (String.IsNullOrEmpty(idAndToken))
			{
				return false;
			}

			// サーバーからのトークンは '"' で囲まれているので除去する
			idAndToken = idAndToken.Trim('"');

			// ID とトークンに分離
			String[] split = idAndToken.Split(YbdConstants.TOKEN_DELIM);
			if (split.Length != 2)
			{
				return false;
			}
			Int32.TryParse(split[0], out Int32 id);
			String token = split[1];

			// ユーザー情報を取得
			PublicUserInfo? userInfo = await GetPublicUserInfoAsync(id);
			if (userInfo == null)
			{
				return false;
			}

			// 状態設定
			YbdAuthenticationStateProvider? stateProvider = AuthenticationStateProvider as YbdAuthenticationStateProvider;
			if (stateProvider == null)
			{
				return false;
			}
			await stateProvider.SetStateLoginAsync(token, userInfo);
			return true;
		}

		// --------------------------------------------------------------------
		// ログアウト状態にする
		// --------------------------------------------------------------------
		private async Task<Boolean> SetStateLogoutAsync()
		{
			// 状態設定
			YbdAuthenticationStateProvider? stateProvider = AuthenticationStateProvider as YbdAuthenticationStateProvider;
			if (stateProvider == null)
			{
				return false;
			}
			await stateProvider.SetStateLogoutAsync();
			return true;
		}









		public async Task<String> LoginAsync(LoginInfo userInfo)
		{
			await Task.Delay(1000);
			return "DummyToken";
		}



	}
}
