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
using YukariBlazorDemo.Client.Models.Services;
using YukariBlazorDemo.Shared;

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

		public PublicUserInfo? LoginUserInfo { get; set; }

		public String? Token { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証状態を返す
		// --------------------------------------------------------------------
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			if (LoginUserInfo == null)
			{
				// 未認証
				HttpClient.DefaultRequestHeaders.Authorization = null;
				return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
			}
			else
			{
				// 認証
				AuthenticationHeaderValue authenticationHeader = new AuthenticationHeaderValue("Bearer", Token);
				HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, LoginUserInfo.Name) }, "apiauth");
				return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
			}
#if false
			// 未認証
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
#endif
#if false
			// 認証
			ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "id") }, "apiauth");
			return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
#endif
		}

		public async Task SetStateLoginAsync(String token, PublicUserInfo loginUserInfo)
		{
			Token = token;
			LoginUserInfo = loginUserInfo;

			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

#if false
		// --------------------------------------------------------------------
		// サーバーからのトークンを使ってログイン状態にする
		// --------------------------------------------------------------------
		public async Task<Boolean> SetStateLoginAsync(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				return false;
			}

			String? token = await response.Content.ReadAsStringAsync();
			if (String.IsNullOrEmpty(token))
			{
				return false;
			}

			// 要求ヘッダーに常にトークンを付与（サーバーからのトークンは '"' で囲まれているので除去する）
			AuthenticationHeaderValue authenticationHeader = new AuthenticationHeaderValue("Bearer", token.Trim('"'));
			HttpClient.DefaultRequestHeaders.Authorization = authenticationHeader;

			LoginInfo loginInfo = await AuthService.GetLoginInfoAsync();
			
			
			
			
			return false;
		}
#endif
	}
}
