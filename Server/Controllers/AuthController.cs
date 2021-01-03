// ============================================================================
// 
// 認証 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Authorization;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[Authorize]
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_AUTH)]
	public class AuthController : ApiController
	{
		// ====================================================================
		// public static プロパティー
		// ====================================================================

		// ゲストのユーザー画像
		public static Thumbnail? DefaultGuestUserThumbnail { get; set; }

		// 登録ユーザーのデフォルトユーザー画像
		public static Thumbnail? DefaultRegisteredUserThumbnail { get; set; }

		// ====================================================================
		// API（ApiController）【認証不要】
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[AllowAnonymous]
		public override String ControllerStatus()
		{
			String status;
			try
			{
				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception("データベースにアクセスできません。");
				}

				// Where を使用すると列の不足を検出できる
				registeredUserContext.RegisteredUsers.Where(x => x.Id == 0).FirstOrDefault();

				status = "正常 / ユーザー数：" + registeredUserContext.RegisteredUsers.Count();
			}
			catch (Exception excep)
			{
				status = "エラー / " + excep.Message;
				Debug.WriteLine("認証 API 状態取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return status;
		}

		// ====================================================================
		// API（一般）【認証不要】
		// ====================================================================

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpGet, Route(YbdConstants.URL_IS_ADMIN_REGISTERED)]
		public Boolean? IsAdminRegistered()
		{
			Boolean? registered = null;
			try
			{
				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception("データベースにアクセスできません。");
				}

				registered = IsAdminRegistered(registeredUserContext.RegisteredUsers);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("認証 API 管理者登録確認サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return registered;
		}

		// --------------------------------------------------------------------
		// ユーザー登録
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpPost, Route(YbdConstants.URL_ADD)]
		public IActionResult AddUser([FromBody] LoginInfo registerInfo)
		{
			try
			{
				if (!registerInfo.IsValid())
				{
					throw new Exception();
				}

				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception();
				}
				RegisteredUser newUser = new();
				newUser.Name = registerInfo.Name;
				newUser.Password = registerInfo.Password;
				newUser.LastModified = ServerCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);

				if (!IsAdminRegistered(registeredUserContext.RegisteredUsers))
				{
					// 管理者未登録の場合は管理者登録でなければならない
					if (newUser.Name != YbdConstants.ADMIN_NAME)
					{
						throw new Exception();
					}
					newUser.IsAdmin = true;
				}

				// 登録
				registeredUserContext.RegisteredUsers.Add(newUser);
				registeredUserContext.SaveChanges();

				String idAndToken = GenerateIdAndTokenString(newUser.Id);
				Debug.WriteLine("AddUser() " + idAndToken);

				// 登録と同時にログインできるように ID とログイン用トークンを返す
				return Ok(idAndToken);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ユーザー登録サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// ログイン
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpPost, Route(YbdConstants.URL_LOGIN)]
		public IActionResult Login([FromBody] LoginInfo loginInfo)
		{
			try
			{
				if (!loginInfo.IsValid())
				{
					throw new Exception();
				}

				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception();
				}
				RegisteredUser loginUser = registeredUserContext.RegisteredUsers.Single(x => x.Name == loginInfo.Name && x.Password == loginInfo.Password);

				String idAndToken = GenerateIdAndTokenString(loginUser.Id);
				Debug.WriteLine("Login() " + idAndToken);

				// ID とログイン用トークンを返す
				return Ok(idAndToken);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ログインサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// 公開ユーザー情報を返す
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpGet, Route(YbdConstants.URL_PUBLIC_USER_INFO + "{id}")]
		public PublicUserInfo? GetPublicUserInfo(String? id)
		{
			PublicUserInfo? userInfo = null;
			try
			{
				if (!Int32.TryParse(id, out Int32 idNum))
				{
					throw new Exception("ID が指定されていません。");
				}

				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception();
				}

				RegisteredUser registeredUser = registeredUserContext.RegisteredUsers.Single(x => x.Id == idNum);
				userInfo = new PublicUserInfo();
				registeredUser.CopyPublicInfo(userInfo);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("公開ユーザー情報取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return userInfo;
		}

		// --------------------------------------------------------------------
		// 公開されているユーザーのサムネイルを返す
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpGet, Route(YbdConstants.URL_PUBLIC_USER_THUMBNAIL + "{*id}")]
		public IActionResult GetThumbnail(String? id)
		{
			try
			{
				if (!Int32.TryParse(id, out Int32 idNum))
				{
					throw new Exception();
				}
				using RegisteredUserContext registeredUserContext = new();
				if (registeredUserContext.RegisteredUsers == null)
				{
					throw new Exception();
				}
				RegisteredUser registeredUser = registeredUserContext.RegisteredUsers.Single(x => x.Id == idNum);
				if (DefaultRegisteredUserThumbnail == null)
				{
					throw new Exception();
				}
				return File(DefaultRegisteredUserThumbnail.Bitmap, DefaultRegisteredUserThumbnail.Mime, ServerConstants.INVALID_DATE_OFFSET, INVALID_ETAG);
			}
			catch (Exception excep)
			{
				if (DefaultGuestUserThumbnail != null)
				{
					return File(DefaultGuestUserThumbnail.Bitmap, DefaultGuestUserThumbnail.Mime, ServerConstants.INVALID_DATE_OFFSET, INVALID_ETAG);
				}

				Debug.WriteLine("ユーザーサムネイル取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// ====================================================================
		// API（一般）【要認証】
		// ====================================================================

		// --------------------------------------------------------------------
		// ログインしているか
		// クライアントは再起動後もトークンを保持しているが、この API を呼ぶことでそのトークンが引き続き有効かを確認できる
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_IS_LOGGED_IN)]
		public Boolean IsLoggedIn()
		{
			// ここに到達できているということはログインしているということ
			return true;
		}

		// --------------------------------------------------------------------
		// ログアウト時のサーバー側の処理
		// --------------------------------------------------------------------
		[HttpPut, Route(YbdConstants.URL_LOGOUT)]
		public IActionResult Logout([FromBody] Int32 dummy)
		{
			try
			{
				// 現状はサーバー側の処理は特に無い
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ログアウトサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

#if false
		// --------------------------------------------------------------------
		// 認証状態のユーザーのログイン情報を返す
		// ただしパスワードは返さない
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_LOGIN_INFO)]
		public LoginInfo? GetLoginInfo()
		{
			HttpContext.Request.Headers.TryGetValue("authorization", out StringValues authValues);
			if (authValues.Count == 0)
			{
				return null;
			}
#if false
			String[] authParams = authValues[0].Split(' ');
			if (split.Length > 1)
			{
				token = split[1];
			}
#endif
			return null;
		}
#endif

		// --------------------------------------------------------------------
		// テスト用
		// --------------------------------------------------------------------
		[HttpGet, Route("test/")]
		public String Test()
		{
			return "Test " + Environment.TickCount.ToString("#,0");
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// トークンの有効期間 [h]
		private const Int32 TOKEN_AVAILABLE_HOURS = 12;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// --------------------------------------------------------------------
		private Boolean IsAdminRegistered(DbSet<RegisteredUser> registeredUsers)
		{
			return registeredUsers.Where(x => x.IsAdmin).FirstOrDefault() != null;
		}

		// --------------------------------------------------------------------
		// 認証トークン文字列生成
		// --------------------------------------------------------------------
		private String GenerateToken(Int32 id)
		{
			SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServerConstants.TOKEN_SECRET_KEY));
			SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			Claim[] claims = new Claim[]
			{
				new Claim(ClaimTypes.NameIdentifier, id.ToString()),
			};
			JwtSecurityToken token = new JwtSecurityToken(ServerConstants.TOKEN_ISSUER, null, claims, null, DateTime.Now.AddHours(TOKEN_AVAILABLE_HOURS), creds);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		// --------------------------------------------------------------------
		// ユーザー ID と認証トークンセットの文字列生成
		// --------------------------------------------------------------------
		private String GenerateIdAndTokenString(Int32 id)
		{
			return id + YbdConstants.TOKEN_DELIM + GenerateToken(id);
		}



















		//[Authorize]
		[AllowAnonymous]
		[HttpGet, Route("test2/")]
		public String Test2()
		{
			String? token = null;
			HttpContext.Request.Headers.TryGetValue("authorization", out StringValues values);
			if (values.Count > 0)
			{
				Debug.WriteLine("Test2() header: " + values[0]);
				String[] split = values[0].Split('"');
				if (split.Length > 3)
				{
					token = split[3];
				}
				else
				{
					split = values[0].Split(' ');
					if (split.Length > 1)
					{
						token = split[1];
					}
				}
			}
			Debug.WriteLine("Test2() token: " + token);
			if (token == null)
			{
				return "Test 2 token null";
			}

			TokenValidationParameters p = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				//ValidIssuer = "MyIssuer",
				//ValidAudience = "MyIssuer",
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServerConstants.TOKEN_SECRET_KEY))
			};

			JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
			try
			{
				ClaimsPrincipal claims = jwtSecurityTokenHandler.ValidateToken(token, p, out SecurityToken validatedToken);
				var a = claims.Claims.Count();
			}
			catch (Exception excep)
			{
				Debug.WriteLine(excep.Message);
				return "Test 2 Exception: " + excep.Message;
			}

			return "Test 2 " + Environment.TickCount.ToString("#,0");
		}

	}
}
