// ============================================================================
// 
// AuthorizeView と HttpClient に認証状態を通知する
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

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

		//public PublicUserInfo? LoginUserInfo { get; set; }

		//public String? Token { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証状態を返す
		// --------------------------------------------------------------------
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			// ローカルストレージから認証状態を読み込む
			// GetItemAsync() の返値は定義上 nullable になっていないが実際には nullable
			String? token = await LocalStorage.GetItemAsync<String>(ITEM_NAME_TOKEN);
			PublicUserInfo? loginUserInfo = await LocalStorage.GetItemAsync<PublicUserInfo>(ITEM_NAME_LOGIN_INFO);

			if (token == null || loginUserInfo == null)
			{
				// 認証ヘッダーをクリア
				HttpClient.DefaultRequestHeaders.Authorization = null;

				// 未認証
				return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
			}
			else
			{
				// 認証ヘッダーを設定
				AuthenticationHeaderValue authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
				HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;

				// 認証
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, loginUserInfo.Name) }, "apiauth");
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
		// public メンバー関数
		// ====================================================================

		// トークン保存用
		private const String ITEM_NAME_TOKEN = "token";

		// ログイン情報用
		private const String ITEM_NAME_LOGIN_INFO = "logininfo";
	}
}
