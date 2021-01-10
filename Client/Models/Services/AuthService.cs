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
			mAuthenticationStateProvider = authenticationStateProvider;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ユーザー登録
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> AddUserAsync(LoginInfo registerInfo)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_ADD, registerInfo);
			return await SetStateLoginAsync(response);
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーの情報を取得
		// --------------------------------------------------------------------
		public PublicUserInfo? GetLoginUserInfo()
		{
			if (mAuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				return stateProvider.LoginUserInfo;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 公開ユーザー情報を取得
		// --------------------------------------------------------------------
		public async Task<PublicUserInfo?> GetPublicUserInfoAsync(String id)
		{
			PublicUserInfo? result = null;
			try
			{
				result = await mHttpClient.GetFromJsonAsync<PublicUserInfo>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_PUBLIC_USER_INFO + id);
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
			return await mHttpClient.GetFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_ADMIN_REGISTERED);
		}

		// --------------------------------------------------------------------
		// 現在ログインしているか
		// --------------------------------------------------------------------
		public async Task<Boolean> IsLoggedInAsync()
		{
			Boolean isLoggedIn = await GetAuthorizedFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_LOGGED_IN);
			if (!isLoggedIn)
			{
				await SetStateLogoutAsync();
			}
			return isLoggedIn;
		}

		// --------------------------------------------------------------------
		// ログイン
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LoginAsync(LoginInfo registerInfo)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGIN, registerInfo);
			return await SetStateLoginAsync(response);
		}

		// --------------------------------------------------------------------
		// ログアウト
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LogoutAsync()
		{
			using HttpResponseMessage response = await mHttpClient.PutAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGOUT, 0);
			Boolean result = await SetStateLogoutAsync();
			if (!response.IsSuccessStatusCode || !result)
			{
				return "ログアウトできませんでした。";
			}
			return String.Empty;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー設定
		// --------------------------------------------------------------------
		public Boolean SetStateChangedHandler(NotifyDelegate notifyDelegate)
		{
			if (mAuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				stateProvider.StateChanged = notifyDelegate;
				return true;
			}
			return false;
		}

		// --------------------------------------------------------------------
		// プロフィール画像設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetThumbnailAsync(TransferFile transferFile)
		{
			HttpStatusCode statusCode = await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_SET_USER_THUMBNAIL, transferFile);
			if (!IsSuccessStatusCode(statusCode))
			{
				switch (statusCode)
				{
					case HttpStatusCode.InternalServerError:
						// RequestSizeLimit を越えた場合も InternalServerError になる模様
						return "プロフィール画像を変更できませんでした。データ容量が大きすぎないか確認してください。";
					case HttpStatusCode.Unauthorized:
						return ClientConstants.ERROR_MESSAGE_UNAUTHORIZED;
					default:
						return ClientConstants.ERROR_MESSAGE_UNEXPECTED;
				}
			}
			return String.Empty;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 認証状態
		private AuthenticationStateProvider mAuthenticationStateProvider;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証が必要な API からデータを取得
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<T?> GetAuthorizedFromJsonAsync<T>(String uri)
		{
			if (mAuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				await stateProvider.AddAuthorizationHeaderIfCanAsync();
			}
			using HttpResponseMessage response = await mHttpClient.GetAsync(uri);
			if (await SetStateLogoutIfUnauthorizedAsync(response))
			{
				return default(T);
			}
			if (!response.IsSuccessStatusCode)
			{
				return default(T);
			}
			return await response.Content.ReadFromJsonAsync<T>();
		}

		// --------------------------------------------------------------------
		// HTTP 応答が成功したかどうか
		// --------------------------------------------------------------------
		private Boolean IsSuccessStatusCode(HttpStatusCode statusCode)
		{
			return HttpStatusCode.OK <= statusCode && statusCode < HttpStatusCode.MultipleChoices;
		}

		// --------------------------------------------------------------------
		// 認証が必要な API へデータを送信
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<HttpStatusCode> PutAuthorizedAsJsonAsync<T>(String uri, T obj)
		{
			using HttpResponseMessage response = await mHttpClient.PutAsJsonAsync(uri, obj);
			await SetStateLogoutIfUnauthorizedAsync(response);
			return response.StatusCode;
		}

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
						return ClientConstants.ERROR_MESSAGE_INTERNAL_SERVER_ERROR;
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
			String id = split[0];
			String token = split[1];

			// ユーザー情報を取得
			PublicUserInfo? userInfo = await GetPublicUserInfoAsync(id);
			if (userInfo == null)
			{
				return "サーバーからユーザー情報を取得できませんでした。";
			}

			// 状態設定
			YbdAuthenticationStateProvider? stateProvider = mAuthenticationStateProvider as YbdAuthenticationStateProvider;
			if (stateProvider == null)
			{
				return "内部エラー。";
			}
			await stateProvider.SetStateLoginAsync(token, userInfo);
			return String.Empty;
		}

		// --------------------------------------------------------------------
		// ログアウト状態にする
		// ＜返値＞ ログアウト状態にしたら true
		// --------------------------------------------------------------------
		private async Task<Boolean> SetStateLogoutAsync()
		{
			// 状態設定
			YbdAuthenticationStateProvider? stateProvider = mAuthenticationStateProvider as YbdAuthenticationStateProvider;
			if (stateProvider == null)
			{
				return false;
			}
			await stateProvider.SetStateLogoutAsync();
			return true;
		}

		// --------------------------------------------------------------------
		// HTTP 応答が Unauthorized ならログアウト状態にする
		// ＜返値＞ ログアウト状態にしたら true
		// --------------------------------------------------------------------
		private async Task<Boolean> SetStateLogoutIfUnauthorizedAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					return await SetStateLogoutAsync();
				}
			}
			return false;
		}
	}
}
