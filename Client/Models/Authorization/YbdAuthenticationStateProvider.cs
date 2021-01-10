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
			mHttpClient = httpClient;
			mLocalStorage = localStorage;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

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
			if (mHttpClient.DefaultRequestHeaders.Authorization != null)
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
					new Claim(ClaimTypes.Role, loginUserInfo.IsAdmin ? ClientConstants.ROLE_NAME_ADMIN : ClientConstants.ROLE_NAME_GENERAL),
				};

				// AuthenticationType
				// https://docs.microsoft.com/ja-jp/dotnet/api/system.security.claims.authenticationtypes?view=netframework-4.8&viewFallbackFrom=net-5.0
				// 何かしらを指定しないと AuthorizeView がうまく動作しない
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Signature");
				return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
			}
		}

		// --------------------------------------------------------------------
		// 認証済にする
		// --------------------------------------------------------------------
		public async Task SetStateLoginAsync(String token, PublicUserInfo loginUserInfo)
		{
			// ローカルストレージに認証状態を保存
			await mLocalStorage.SetItemAsync(ITEM_NAME_TOKEN, token);
			await mLocalStorage.SetItemAsync(ITEM_NAME_LOGIN_INFO, loginUserInfo);

			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		// --------------------------------------------------------------------
		// 未認証にする
		// --------------------------------------------------------------------
		public async Task SetStateLogoutAsync()
		{
			// ローカルストレージから認証状態を削除
			await mLocalStorage.RemoveItemAsync(ITEM_NAME_TOKEN);
			await mLocalStorage.RemoveItemAsync(ITEM_NAME_LOGIN_INFO);

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
		// private メンバー変数
		// ====================================================================

		// HTTP クライアント
		private HttpClient mHttpClient;

		// ローカルストレージ
		private ILocalStorageService mLocalStorage;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ローカルストレージから認証状態を読み込む
		// --------------------------------------------------------------------
		private async Task<(String?, PublicUserInfo?)> LoadLocalStorageAsync()
		{
			// GetItemAsync() の返値は定義上 nullable になっていないが実際には nullable
			String? token = await mLocalStorage.GetItemAsync<String>(ITEM_NAME_TOKEN);
			PublicUserInfo? loginUserInfo = await mLocalStorage.GetItemAsync<PublicUserInfo>(ITEM_NAME_LOGIN_INFO);
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
				mHttpClient.DefaultRequestHeaders.Authorization = null;

				// 認証ヘッダー更新後に LoginUserInfo 設定
				LoginUserInfo = null;
				return (null, null);
			}

			// 認証ヘッダーを設定
			header = new AuthenticationHeaderValue("Bearer", token);
			mHttpClient.DefaultRequestHeaders.Authorization = header;
			ClientCommon.DebugWriteLine("SetAuthorizationHeaderAndPropertyAsync() header set: " + token);

			// 認証ヘッダー更新後に LoginUserInfo 設定
			LoginUserInfo = loginUserInfo;
			return (token, loginUserInfo);
		}
	}
}
