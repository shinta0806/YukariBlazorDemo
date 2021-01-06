// ============================================================================
// 
// AuthorizeView と HttpClient に認証状態を通知する
// 
// ============================================================================

// ----------------------------------------------------------------------------
// クライアント側でログイン情報を知りたい場合は AuthenticationState を使うのが本筋とは思うが、
// 回りくどいので LoginUserInfo プロパティーでも提供する
// ----------------------------------------------------------------------------

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Misc;
using YukariBlazorDemo.Shared.Authorization;

namespace YukariBlazorDemo.Client.Models.Authorization
{
	public class YbdAuthenticationStateProvider : AuthenticationStateProvider
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YbdAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage /*AuthService authService*/)
		{
			HttpClient = httpClient;
			LocalStorage = localStorage;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// HTTP クライアント
		public HttpClient HttpClient { get; set; }

		// ローカルストレージ
		public ILocalStorageService LocalStorage { get; set; }

		// ログインしているユーザーの情報
		private PublicUserInfo? mLoginUserInfo;
		public PublicUserInfo? LoginUserInfo
		{
			get => mLoginUserInfo;
			private set
			{
				mLoginUserInfo = value;
				ClientCommon.DebugWriteLine("LoginUserInfo setter() id: " + mLoginUserInfo?.Id);
				if (StateChanged != null)
				{
					StateChanged();
				}
			}
		}

		// ログイン状態が変化した
		public NotifyDelegate? StateChanged { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証ヘッダーが無い場合は、ローカルストレージの情報に応じて認証ヘッダーを設定する
		// ＜返値＞ true: 既に付与済み、または、付与した, false: 付与できなかった
		// --------------------------------------------------------------------
		public async Task<Boolean> AddAuthorizationHeaderIfCanAsync()
		{
			if (HttpClient.DefaultRequestHeaders.Authorization != null)
			{
				// 既に認証ヘッダーがある
				return true;
			}

			(String? token, PublicUserInfo? loginUserInfo) = await SetAuthorizationHeaderAndPropertyAsync();
			return !String.IsNullOrEmpty(token) && loginUserInfo != null;
		}

		// --------------------------------------------------------------------
		// 認証状態を返す
		// --------------------------------------------------------------------
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			(String? token, PublicUserInfo? loginUserInfo) = await SetAuthorizationHeaderAndPropertyAsync();
			if (String.IsNullOrEmpty(token) || loginUserInfo == null)
			{
				// 未認証
				return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
			}
			else
			{
				// 認証
				Claim[] claims = new Claim[]
				{
					new Claim(ClaimTypes.NameIdentifier, loginUserInfo.Id.ToString()),
					new Claim(ClaimTypes.Name, loginUserInfo.Name),
				};
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "apiauth");
				return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
			}
		}

		// --------------------------------------------------------------------
		// 認証済にする
		// --------------------------------------------------------------------
		public async Task SetStateLoginAsync(String token, PublicUserInfo loginUserInfo)
		{
			// ローカルストレージに認証状態を保存
			await LocalStorage.SetItemAsync(ITEM_NAME_TOKEN, token);
			await LocalStorage.SetItemAsync(ITEM_NAME_LOGIN_INFO, loginUserInfo);

			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		// --------------------------------------------------------------------
		// 未認証にする
		// --------------------------------------------------------------------
		public async Task SetStateLogoutAsync()
		{
			// ローカルストレージから認証状態を削除
			await LocalStorage.RemoveItemAsync(ITEM_NAME_TOKEN);
			await LocalStorage.RemoveItemAsync(ITEM_NAME_LOGIN_INFO);

			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// トークン保存用
		private const String ITEM_NAME_TOKEN = "token";

		// ログイン情報用
		private const String ITEM_NAME_LOGIN_INFO = "logininfo2";

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ローカルストレージから認証状態を読み込む
		// --------------------------------------------------------------------
		private async Task<(String?, PublicUserInfo?)> LoadLocalStorageAsync()
		{
			// GetItemAsync() の返値は定義上 nullable になっていないが実際には nullable
			String? token = await LocalStorage.GetItemAsync<String>(ITEM_NAME_TOKEN);
			PublicUserInfo? loginUserInfo = await LocalStorage.GetItemAsync<PublicUserInfo>(ITEM_NAME_LOGIN_INFO);
			return (token, loginUserInfo);
		}

		// --------------------------------------------------------------------
		// ローカルストレージの情報に応じて認証ヘッダーと LoginUserInfo を設定する
		// --------------------------------------------------------------------
		private async Task<(String? token, PublicUserInfo? loginUserInfo)> SetAuthorizationHeaderAndPropertyAsync()
		{
			AuthenticationHeaderValue? header = null;
			(String? token, PublicUserInfo? loginUserInfo) = await LoadLocalStorageAsync();
			if (String.IsNullOrEmpty(token) || loginUserInfo == null)
			{
				// 認証ヘッダーをクリア
				HttpClient.DefaultRequestHeaders.Authorization = null;

				// 認証ヘッダー更新後に LoginUserInfo 設定
				LoginUserInfo = null;
				return (null, null);
			}

			// 認証ヘッダーを設定
			header = new AuthenticationHeaderValue("Bearer", token);
			HttpClient.DefaultRequestHeaders.Authorization = header;
			ClientCommon.DebugWriteLine("SetAuthorizationHeaderAndPropertyAsync() header set: " + token);

			// 認証ヘッダー更新後に LoginUserInfo 設定
			LoginUserInfo = loginUserInfo;
			return (token, loginUserInfo);
		}


	}
}
