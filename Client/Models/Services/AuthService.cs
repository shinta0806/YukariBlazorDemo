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
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_USERS, registerInfo);
			if (!response.IsSuccessStatusCode)
			{
				switch (response.StatusCode)
				{
					case HttpStatusCode.Conflict:
						return "そのお名前は既に登録されています。";
					default:
						return DefaultErrorMessage(response.StatusCode);
				}
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(await response.Content.ReadAsStringAsync());
		}

		// --------------------------------------------------------------------
		// ユーザー削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteUserAsync(String? id)
		{
			using HttpResponseMessage response = await mHttpClient.DeleteAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_USERS + id);
			switch (response.StatusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "指定された ID が見つかりません。";
				default:
					return DefaultErrorMessage(response.StatusCode);
			}
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーのトークンの有効性確認と延長
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> ExtendAsync()
		{
			(HttpStatusCode statusCode, String content)
					= await PostAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_EXTEND, 0);
			if (!IsSuccessStatusCode(statusCode))
			{
				switch (statusCode)
				{
					default:
						return DefaultErrorMessage(statusCode);
				}
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(content);
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
				result = await mHttpClient.GetFromJsonAsync<PublicUserInfo>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_PUBLIC + YbdConstants.URL_INFO + id);
			}
			catch (JsonException)
			{
				// 存在しない ID が指定された場合（ユーザーが URL を書き換えた場合など）はサーバー側で null を返し、JSON 化できないため JsonException 例外となる
				// クライアント側には null を返す
			}
			return result;
		}

		// --------------------------------------------------------------------
		// ユーザー一覧を取得
		// --------------------------------------------------------------------
		public async Task<(PublicUserInfo[], Int32)> GetUsersAsync()
		{
			return await GetArrayAsync<PublicUserInfo>(YbdConstants.URL_USERS);
		}

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// --------------------------------------------------------------------
		public async Task<Boolean> IsAdminRegisteredAsync()
		{
			return await mHttpClient.GetFromJsonAsync<Boolean>(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_IS_ADMIN_REGISTERED);
		}

		// --------------------------------------------------------------------
		// ログイン
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LoginAsync(LoginInfo loginInfo)
		{
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGIN, loginInfo);
			if (!response.IsSuccessStatusCode)
			{
				switch (response.StatusCode)
				{
					case HttpStatusCode.NotAcceptable:
						return "お名前またはパスワードが違います。";
					default:
						return DefaultErrorMessage(response.StatusCode);
				}
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(await response.Content.ReadAsStringAsync());
		}

		// --------------------------------------------------------------------
		// ログアウト
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LogoutAsync()
		{
			using HttpResponseMessage response = await mHttpClient.PutAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_LOGOUT, 0);
			Boolean result = await SetStateLogoutAsync();
			if (!response.IsSuccessStatusCode || !result)
			{
				return "ログアウトできませんでした。";
			}
			return String.Empty;
		}

		// --------------------------------------------------------------------
		// 名前設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetNameAsync(String name)
		{
			(HttpStatusCode statusCode, String content)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_NAME, name);
			switch (statusCode)
			{
				case HttpStatusCode.Conflict:
					return "そのお名前は既に登録されています。";
				default:
					return DefaultErrorMessage(statusCode);
			}
		}

		// --------------------------------------------------------------------
		// パスワード設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetPasswordAsync(String currentPassword, String newPassword)
		{
			(HttpStatusCode statusCode, String content)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_PASSWORD, new String[] { currentPassword, newPassword });
			switch (statusCode)
			{
				case HttpStatusCode.NotAcceptable:
					return "現在のパスワードが間違っています。";
				default:
					return DefaultErrorMessage(statusCode);
			}
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
			(HttpStatusCode statusCode, String content)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_THUMBNAIL, transferFile);
			switch (statusCode)
			{
				case HttpStatusCode.InternalServerError:
					// RequestSizeLimit を越えた場合も InternalServerError になる模様
					return "プロフィール画像を変更できませんでした。データ容量が大きすぎないか確認してください。";
				default:
					return DefaultErrorMessage(statusCode);
			}
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
		// 認証ヘッダーが無い場合は、ローカルストレージの情報に応じて認証ヘッダーを設定する
		// ＜返値＞ true: 既に付与済み、または、付与した, false: 付与できなかった
		// --------------------------------------------------------------------
		private async ValueTask<Boolean> AddAuthorizationHeaderIfCanAsync()
		{
			if (mAuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				return await stateProvider.AddAuthorizationHeaderIfCanAsync();
			}
			else
			{
				return false;
			}
		}

		// --------------------------------------------------------------------
		// HTTP 応答の本文
		// --------------------------------------------------------------------
		private async ValueTask<String> GetResponseContent(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				return String.Empty;
			}
		}

		// --------------------------------------------------------------------
		// 認証が必要な API へデータを送信
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, String)> PostAuthorizedAsJsonAsync<T>(String uri, T obj)
		{
			await AddAuthorizationHeaderIfCanAsync();
			using HttpResponseMessage response = await mHttpClient.PostAsJsonAsync(uri, obj);
			await SetStateLogoutIfUnauthorizedAsync(response);
			return (response.StatusCode, await GetResponseContent(response));
		}

		// --------------------------------------------------------------------
		// 認証が必要な API へデータを送信
		// 401 が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, String)> PutAuthorizedAsJsonAsync<T>(String uri, T obj)
		{
			await AddAuthorizationHeaderIfCanAsync();
			using HttpResponseMessage response = await mHttpClient.PutAsJsonAsync(uri, obj);
			await SetStateLogoutIfUnauthorizedAsync(response);
			return (response.StatusCode, await GetResponseContent(response));
		}

		// --------------------------------------------------------------------
		// サーバーからのトークンを使ってログイン状態にする
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		private async Task<String> SetStateLoginAsync(String idAndToken)
		{

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
		private async ValueTask<Boolean> SetStateLogoutAsync()
		{
			if (mAuthenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				// 状態設定
				await stateProvider.SetStateLogoutAsync();
				return true;
			}
			else
			{
				return false;
			}
		}

		// --------------------------------------------------------------------
		// HTTP 応答が Unauthorized ならログアウト状態にする
		// ＜返値＞ ログアウト状態にしたら true
		// --------------------------------------------------------------------
		private async ValueTask<Boolean> SetStateLogoutIfUnauthorizedAsync(HttpResponseMessage response)
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
