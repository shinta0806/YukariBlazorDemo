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
using YukariBlazorDemo.Client.Models.Misc;
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
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> AddUserAsync(LoginInfo registerInfo)
		{
			using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_ADD, registerInfo);
			return await SetStateLoginAsync(response);
		}

		// --------------------------------------------------------------------
		// 認証が必要な API からデータを取得
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		public async Task<T?> GetAuthorizedFromJsonAsync<T>(String uri)
		{
			if (AuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
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
		// ログインしているユーザーの情報を取得
		// --------------------------------------------------------------------
		public PublicUserInfo? GetLoginUserInfo()
		{
			if (AuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				return stateProvider.LoginUserInfo;
			}
			return null;
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
			return await GetAuthorizedFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_LOGGED_IN);
		}

		// --------------------------------------------------------------------
		// ログイン
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LoginAsync(LoginInfo registerInfo)
		{
			using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGIN, registerInfo);
			return await SetStateLoginAsync(response);
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

		// --------------------------------------------------------------------
		// イベントハンドラー設定
		// --------------------------------------------------------------------
		public Boolean SetStateChangedHandler(NotifyDelegate notifyDelegate)
		{
			if (AuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				stateProvider.StateChanged = notifyDelegate;
				return true;
			}
			return false;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// サーバーからのトークンを使ってログイン状態にする
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		private async Task<String> SetStateLoginAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				switch (response.StatusCode)
				{
					case HttpStatusCode.BadRequest:
						return "入力内容が不正です。";
					case HttpStatusCode.Conflict:
						return "そのお名前は既に登録されています。";
					case HttpStatusCode.InternalServerError:
						return "サーバー内部でエラーが発生しました。";
					case HttpStatusCode.NotAcceptable:
						return "お名前またはパスワードが違います。";
					default:
						return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
				}
			}

			String? idAndToken = await response.Content.ReadAsStringAsync();
			if (String.IsNullOrEmpty(idAndToken))
			{
				return "サーバーから認証情報を取得できませんでした。";
			}

			// サーバーからのトークンは '"' で囲まれているので除去する
			idAndToken = idAndToken.Trim('"');

			// ID とトークンに分離
			String[] split = idAndToken.Split(YbdConstants.TOKEN_DELIM);
			if (split.Length != 2)
			{
				return "サーバーから認証情報を取得できませんでした。";
			}
			Int32.TryParse(split[0], out Int32 id);
			String token = split[1];

			// ユーザー情報を取得
			PublicUserInfo? userInfo = await GetPublicUserInfoAsync(id);
			if (userInfo == null)
			{
				return "サーバーからユーザー情報を取得できませんでした。";
			}

			// 状態設定
			YbdAuthenticationStateProvider? stateProvider = AuthenticationStateProvider as YbdAuthenticationStateProvider;
			if (stateProvider == null)
			{
				return "内部エラー。";
			}
			await stateProvider.SetStateLoginAsync(token, userInfo);
			return String.Empty;
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

	}
}
