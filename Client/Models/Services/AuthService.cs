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
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Authorization;
using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared.Authorization;
using YukariBlazorDemo.Shared.Database;
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
			_authenticationStateProvider = authenticationStateProvider;
		}

		// ====================================================================
		// public メンバー関数【認証不要】
		// ====================================================================

		// --------------------------------------------------------------------
		// ユーザー登録
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> AddUserAsync(LoginInfo registerInfo)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_USERS, registerInfo);
			if (!response.IsSuccessStatusCode)
			{
				return response.StatusCode switch
				{
					HttpStatusCode.Conflict => "そのお名前は既に登録されています。",
					_ => DefaultErrorMessage(response.StatusCode),
				};
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(await response.Content.ReadAsStringAsync());
		}

		// --------------------------------------------------------------------
		// 公開ユーザー情報を取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 公開ユーザー情報)
		// --------------------------------------------------------------------
		public async Task<(String, PublicUserInfo?)> GetPublicUserInfoAsync(String id)
		{
			(HttpStatusCode statusCode, PublicUserInfo? publicUserInfo) = await GetFromJsonAsync<PublicUserInfo>(YbdConstants.URL_PUBLIC + YbdConstants.URL_INFO, id);
			return (DefaultErrorMessage(statusCode), publicUserInfo);
		}

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 管理者が登録されているか)
		// --------------------------------------------------------------------
		public async Task<(String, Boolean)> IsAdminRegisteredAsync()
		{
			(HttpStatusCode statusCode, Boolean isAdminregistered) = await GetFromJsonAsync<Boolean>(YbdConstants.URL_IS_ADMIN_REGISTERED);
			return (DefaultErrorMessage(statusCode), isAdminregistered);
		}

		// --------------------------------------------------------------------
		// ログイン
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LoginAsync(LoginInfo loginInfo)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_LOGIN, loginInfo);
			if (!response.IsSuccessStatusCode)
			{
				return response.StatusCode switch
				{
					HttpStatusCode.NotAcceptable => "お名前またはパスワードが違います。",
					_ => DefaultErrorMessage(response.StatusCode),
				};
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(await response.Content.ReadAsStringAsync());
		}

		// --------------------------------------------------------------------
		// イベントハンドラー設定
		// --------------------------------------------------------------------
		public Boolean SetStateChangedHandler(NotifyDelegate notifyDelegate)
		{
			if (_authenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				stateProvider.StateChanged = notifyDelegate;
				return true;
			}
			return false;
		}

		// ====================================================================
		// public メンバー関数【要認証（一般ユーザー）】
		// ====================================================================

		// --------------------------------------------------------------------
		// 後で歌う予定リストに追加
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> AddStockAsync(AvailableSong availableSong)
		{
			(HttpStatusCode statusCode, _)
					= await PostAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_STOCKS, availableSong);
			return DefaultErrorMessage(statusCode);
		}

		// --------------------------------------------------------------------
		// マイ履歴をすべて削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteHistoriesAllAsync()
		{
			(HttpStatusCode statusCode, _) = await DeleteAsync<String>(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_HISTORIES + YbdConstants.URL_ALL);
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => "マイ履歴がありません。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーのトークンの有効性確認と延長
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> ExtendAsync()
		{
			// ローカルストレージの情報に応じて認証ヘッダーを設定する
			await AddAuthorizationHeaderIfCanAsync();

			// 確認
			(HttpStatusCode statusCode, String content)
					= await PostAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_EXTEND, 0);
			if (!IsSuccessStatusCode(statusCode))
			{
				return statusCode switch
				{
					_ => DefaultErrorMessage(statusCode),
				};
			}

			// 成功した場合はログイン状態にする
			return await SetStateLoginAsync(content);
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーの予約履歴を取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 予約履歴, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, HistorySong[], Int32)> GetLoginUserHistories()
		{
			(HttpStatusCode statusCode, HistorySong[] historySongs, Int32 totalCount) = await GetAuthorizedArrayAsync<HistorySong>(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_HISTORIES);
			return (DefaultErrorMessage(statusCode), historySongs, totalCount);
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーの情報を取得
		// --------------------------------------------------------------------
		public PublicUserInfo? GetLoginUserInfo()
		{
			if (_authenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				return stateProvider.LoginUserInfo;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// ログインしているユーザーの後で歌う予定リストを取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 後で歌う予定リスト, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, StockSong[], Int32)> GetLoginUserStocks()
		{
			(HttpStatusCode statusCode, StockSong[] stockSongs, Int32 totalCount) = await GetAuthorizedArrayAsync<StockSong>(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_STOCKS);
			return (DefaultErrorMessage(statusCode), stockSongs, totalCount);
		}

		// --------------------------------------------------------------------
		// ログアウト
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> LogoutAsync()
		{
			(HttpStatusCode statusCode, String _)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_LOGOUT, 0);
			await SetStateLogoutAsync();
			return statusCode switch
			{
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// 名前設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetNameAsync(String name)
		{
			(HttpStatusCode statusCode, String _)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_NAME, name);
			return statusCode switch
			{
				HttpStatusCode.Conflict => "そのお名前は既に登録されています。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// パスワード設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetPasswordAsync(String currentPassword, String newPassword)
		{
			(HttpStatusCode statusCode, String _)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_PASSWORD, new String[] { currentPassword, newPassword });
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => "現在のパスワードが間違っています。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// プロフィール画像設定
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> SetThumbnailAsync(TransferFile transferFile)
		{
			(HttpStatusCode statusCode, String _)
					= await PutAuthorizedAsJsonAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_CURRENT_USER + YbdConstants.URL_THUMBNAIL, transferFile);
			return statusCode switch
			{
				// RequestSizeLimit を越えた場合も InternalServerError になる模様
				HttpStatusCode.InternalServerError => "プロフィール画像を変更できませんでした。データ容量が大きすぎないか確認してください。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// ====================================================================
		// public メンバー関数【要認証（管理者用）】
		// ====================================================================

		// --------------------------------------------------------------------
		// ユーザー削除
		// ＜返値＞ 成功した場合は空文字列、エラーの場合はエラーメッセージ
		// --------------------------------------------------------------------
		public async Task<String> DeleteUserAsync(String? id)
		{
			(HttpStatusCode statusCode, String _)
					= await DeleteAuthorizedAsync(YbdConstants.URL_API + YbdConstants.URL_AUTH + YbdConstants.URL_USERS + id);
			return statusCode switch
			{
				HttpStatusCode.NotAcceptable => "指定された ID が見つかりません。",
				_ => DefaultErrorMessage(statusCode),
			};
		}

		// --------------------------------------------------------------------
		// ユーザー一覧を取得
		// ＜返値＞ (成功した場合は空文字列、エラーの場合はエラーメッセージ, 予約履歴, 総数)
		// --------------------------------------------------------------------
		public async Task<(String, PublicUserInfo[], Int32)> GetUsersAsync()
		{
			(HttpStatusCode statusCode, PublicUserInfo[] publicUserInfos, Int32 totalCount) = await GetAuthorizedArrayAsync<PublicUserInfo>(YbdConstants.URL_USERS);
			return (DefaultErrorMessage(statusCode), publicUserInfos, totalCount);
		}

		// ====================================================================
		// DI
		// ====================================================================

		// 認証状態
		private readonly AuthenticationStateProvider _authenticationStateProvider;

		// ====================================================================
		// private static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// HTTP 応答の本文
		// --------------------------------------------------------------------
		private static async ValueTask<String> GetResponseContent(HttpResponseMessage response)
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

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証ヘッダーが無い場合は、ローカルストレージの情報に応じて認証ヘッダーを設定する
		// ＜返値＞ true: 既に付与済み、または、付与した, false: 付与できなかった
		// --------------------------------------------------------------------
		private async ValueTask<Boolean> AddAuthorizationHeaderIfCanAsync()
		{
			if (_authenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
			{
				return await stateProvider.AddAuthorizationHeaderIfCanAsync();
			}
			else
			{
				return false;
			}
		}

		// --------------------------------------------------------------------
		// 認証が必要な API で削除
		// HttpStatusCode.Unauthorized が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, String)> DeleteAuthorizedAsync(String uri)
		{
			using HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
			await SetStateLogoutIfUnauthorizedAsync(response.StatusCode);
			return (response.StatusCode, await GetResponseContent(response));
		}

		// --------------------------------------------------------------------
		// 認証が必要な API で配列（1 ページ分）と結果の総数を取得
		// HttpStatusCode.Unauthorized が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, T[], Int32)> GetAuthorizedArrayAsync<T>(String leafUrl, String? query = null)
		{
			(HttpStatusCode statusCode, T[] array, Int32 totalCount) = await GetArrayFromJsonAsync<T>(leafUrl, query);
			await SetStateLogoutIfUnauthorizedAsync(statusCode);
			return (statusCode, array, totalCount);
		}

		// --------------------------------------------------------------------
		// 認証が必要な API へデータを送信
		// HttpStatusCode.Unauthorized が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, String)> PostAuthorizedAsJsonAsync<T>(String uri, T obj)
		{
			using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uri, obj);
			await SetStateLogoutIfUnauthorizedAsync(response.StatusCode);
			return (response.StatusCode, await GetResponseContent(response));
		}

		// --------------------------------------------------------------------
		// 認証が必要な API へデータを送信
		// HttpStatusCode.Unauthorized が返ってきたらログアウトする
		// --------------------------------------------------------------------
		private async Task<(HttpStatusCode, String)> PutAuthorizedAsJsonAsync<T>(String uri, T obj)
		{
			using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uri, obj);
			await SetStateLogoutIfUnauthorizedAsync(response.StatusCode);
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
			(String errorMessage, PublicUserInfo? userInfo) = await GetPublicUserInfoAsync(id);
			if (userInfo == null)
			{
				return "サーバーからユーザー情報を取得できませんでした。" + errorMessage;
			}

			// 状態設定
			if (_authenticationStateProvider is not YbdAuthenticationStateProvider stateProvider)
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
			if (_authenticationStateProvider is YbdAuthenticationStateProvider stateProvider)
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
		private async ValueTask<Boolean> SetStateLogoutIfUnauthorizedAsync(HttpStatusCode statusCode)
		{
			if (statusCode == HttpStatusCode.Unauthorized)
			{
				return await SetStateLogoutAsync();
			}
			return false;
		}
	}
}
