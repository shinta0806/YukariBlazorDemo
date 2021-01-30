// ============================================================================
// 
// 認証 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 認証状態は頻繁に更新されるため ShortCache 属性を付与
// ToDo: Firefox だと画像がうまくキャッシュされない模様
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;

using YukariBlazorDemo.Server.Attributes;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Authorization;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[ShortCache]
	[Authorize]
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_AUTH)]
	public class AuthController : ApiController
	{
		// ====================================================================
		// public static プロパティー
		// ====================================================================

		// ゲストのプロフィール画像
		public static Thumbnail? DefaultGuestUserThumbnail { get; set; }

		// 管理者のデフォルトプロフィール画像
		public static Thumbnail? DefaultAdminUserThumbnail { get; set; }

		// 登録ユーザーのデフォルトプロフィール画像
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
				if (DefaultGuestUserThumbnail == null)
				{
					throw new Exception("デフォルトゲストプロフィール画像が作成できませんでした。" + ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + " フォルダーがあるか確認してください。");
				}
				if (DefaultRegisteredUserThumbnail == null)
				{
					throw new Exception("デフォルト登録ユーザープロフィール画像が作成できませんでした。" + ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + " フォルダーがあるか確認してください。");
				}
				if (!ServerCommon.IsTokenSecretKeyValid())
				{
					throw new Exception("トークン生成用の秘密鍵の長さが足りません。" + ServerConstants.TOKEN_SECRET_KEY_LENGTH_MIN + " 文字以上にしてください。");
				}

				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);

				// FirstOrDefault を使用すると列の不足を検出できる
				registeredUsers.FirstOrDefault(x => x.Id == String.Empty);

				status = "正常 / ユーザー数：" + registeredUsers.Count();
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
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				registered = IsAdminRegistered(registeredUsers);
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
		[HttpPost, Route(YbdConstants.URL_USERS)]
		public IActionResult AddUser([FromBody] LoginInfo registerInfo)
		{
			try
			{
				if (!registerInfo.IsValid())
				{
					return BadRequest();
				}

				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				RegisteredUser newUser = new();
				newUser.Name = registerInfo.Name;
				newUser.Password = registerInfo.Password;
				newUser.LastModified = newUser.LastLogin = YbdCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);

				if (!IsAdminRegistered(registeredUsers))
				{
					// 管理者未登録の場合は管理者登録でなければならない
					if (newUser.Name != YbdConstants.ADMIN_NAME)
					{
						return BadRequest();
					}
					newUser.IsAdmin = true;
				}

				// 同じ名前のユーザーが既に存在している場合は登録できない
				if (registeredUsers.FirstOrDefault(x => x.Name == newUser.Name) != null)
				{
					return Conflict();
				}

				// 登録
				HashPassword(newUser);
				registeredUsers.Add(newUser);
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
				return InternalServerError();
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
					return BadRequest();
				}

#if DEBUG
				Thread.Sleep(1000);
#endif

				// ユーザーを検索
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				RegisteredUser? loginUser = registeredUsers.SingleOrDefault(x => x.Name == loginInfo.Name);
				if (loginUser == null)
				{
					return NotAcceptable();
				}

				// パスワードハッシュの一致を確認
				if (loginUser.Password != HashPassword(loginInfo.Password, loginUser.Salt))
				{
					return NotAcceptable();
				}

				String idAndToken = GenerateIdAndTokenString(loginUser.Id);
				Debug.WriteLine("Login() " + idAndToken);

				loginUser.LastLogin = YbdCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);
				registeredUserContext.SaveChanges();

				// ID とログイン用トークンを返す
				return Ok(idAndToken);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ログインサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 公開ユーザー情報を返す
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpGet, Route(YbdConstants.URL_PUBLIC + YbdConstants.URL_INFO + "{id}")]
		public IActionResult GetPublicUserInfo(String? id)
		{
			try
			{
				// キャッシュチェック
				DateTime lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REGISTERED_USERS);
				if (IsValidEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetPublicUserInfo() キャッシュ有効: " + id);
					return NotModified();
				}

				if (String.IsNullOrEmpty(id))
				{
					return BadRequest();
				}

				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				RegisteredUser registeredUser = registeredUsers.Single(x => x.Id == id);
				PublicUserInfo userInfo = new PublicUserInfo();
				registeredUser.CopyPublicInfo(userInfo, false);

				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified));
				return File(JsonSerializer.SerializeToUtf8Bytes(userInfo), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("公開ユーザー情報取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 公開されているプロフィール画像を返す
		// --------------------------------------------------------------------
		[AllowAnonymous]
		[HttpGet, Route(YbdConstants.URL_PUBLIC + YbdConstants.URL_THUMBNAIL + "{id?}")]
		public IActionResult GetThumbnail(String? id)
		{
			try
			{
				RegisteredUser? registeredUser = null;
				if (String.IsNullOrEmpty(id))
				{
					// 引数が空の場合は、ゲストのプロフィール画像を返す
					if (DefaultGuestUserThumbnail == null)
					{
						throw new Exception();
					}
					registeredUser = new()
					{
						Bitmap = DefaultGuestUserThumbnail.Bitmap,
						Mime = DefaultGuestUserThumbnail.Mime,
						LastModified = ServerConstants.INVALID_MJD,
					};
				}
				else
				{
					using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
					registeredUser = registeredUsers.SingleOrDefault(x => x.Id == id);
					if (registeredUser == null)
					{
						return NotAcceptable();
					}

					// 指定されたユーザーにプロフィール画像が設定されていない場合
					if (registeredUser.Bitmap.Length == 0)
					{
						Thumbnail? defaultThumbnail;
						if (registeredUser.IsAdmin)
						{
							defaultThumbnail = DefaultAdminUserThumbnail;
						}
						else
						{
							defaultThumbnail = DefaultRegisteredUserThumbnail;
						}
						if (defaultThumbnail == null)
						{
							throw new Exception();
						}
						registeredUser = new()
						{
							Bitmap = defaultThumbnail.Bitmap,
							Mime = defaultThumbnail.Mime,
							LastModified = ServerConstants.INVALID_MJD,
						};
					}
				}

				// キャッシュチェック
				DateTime lastModified = YbdCommon.ModifiedJulianDateToDateTime(registeredUser.LastModified);
				if (IsValidEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetThumbnail() プロフィール画像キャッシュ有効: " + id);
					return NotModified();
				}

				// プロフィール画像を返す
				Debug.WriteLine("GetThumbnail() プロフィール画像キャッシュ無効: " + id);
				EntityTagHeaderValue eTag = GenerateEntityTag(registeredUser.LastModified);
				return File(registeredUser.Bitmap, registeredUser.Mime, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("プロフィール画像取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// ====================================================================
		// API（一般）【要認証（一般ユーザー）】
		// ====================================================================

		// --------------------------------------------------------------------
		// トークンの有効期限を延長
		// クライアントは再起動後もトークンを保持しているが、この API を呼ぶことでそのトークンが引き続き有効かを確認でき、有効な場合は有効期限を延長できる
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_EXTEND)]
		public IActionResult Extend([FromBody] Int32 _)
		{
			try
			{
				// ここに到達できているということはトークン自体は正規のものである
				// しかし有効かどうかはまた別問題のため、有効性を確認する
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser))
				{
					return Unauthorized();
				}

				// 新しい有効期限のトークン
				String idAndToken = GenerateIdAndTokenString(loginUser.Id);

				// ID とログイン用トークンを返す
				return Ok(idAndToken);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("延長サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// ログアウト時のサーバー側の処理
		// --------------------------------------------------------------------
		[HttpPut, Route(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_LOGOUT)]
		public IActionResult Logout([FromBody] Int32 _)
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
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 名前を設定
		// --------------------------------------------------------------------
		[HttpPut, Route(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_NAME)]
		public IActionResult SetName([FromBody] String? newName)
		{
			try
			{
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser))
				{
					return Unauthorized();
				}
				if (String.IsNullOrEmpty(newName))
				{
					return BadRequest();
				}

				// 同じ名前のユーザーが既に存在している場合は登録できない
				if (registeredUsers.FirstOrDefault(x => x.Name == newName) != null)
				{
					return Conflict();
				}

				// 設定
				loginUser.Name = newName;
				loginUser.LastModified = YbdCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);
				registeredUserContext.SaveChanges();

				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("名前設定サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// パスワードを設定
		// passwords[0]: 現在のパスワード, passwords[1]: 新しいパスワード
		// --------------------------------------------------------------------
		[HttpPut, Route(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_PASSWORD)]
		public IActionResult SetPassword([FromBody] String?[] passwords)
		{
			try
			{
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser))
				{
					return Unauthorized();
				}
				if (passwords.Length < 2)
				{
					return BadRequest();
				}
				String? currentPassword = passwords[0];
				String? newPassword = passwords[1];
				if (String.IsNullOrEmpty(currentPassword) || String.IsNullOrEmpty(newPassword))
				{
					return BadRequest();
				}

				// 現在のパスワードハッシュの一致を確認
				if (loginUser.Password != HashPassword(currentPassword, loginUser.Salt))
				{
					return NotAcceptable();
				}

				// 設定
				loginUser.Password = newPassword;
				loginUser.LastModified = YbdCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);
				HashPassword(loginUser);
				registeredUserContext.SaveChanges();

				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("パスワード設定サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// プロフィール画像を設定
		// --------------------------------------------------------------------
		[RequestSizeLimit(YbdConstants.USER_THUMBNAIL_LENGTH_MAX)]
		[HttpPut, Route(YbdConstants.URL_CURRENT_USER + YbdConstants.URL_THUMBNAIL)]
		public IActionResult SetThumbnail([FromBody] TransferFile? transferFile)
		{
			try
			{
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser))
				{
					return Unauthorized();
				}
				if (transferFile == null)
				{
					return BadRequest();
				}

				// 設定
				using MemoryStream memoryStream = new MemoryStream(transferFile.Content);
				loginUser.Bitmap = ServerCommon.CreateThumbnail(memoryStream, transferFile.Mime, YbdConstants.USER_THUMBNAIL_WIDTH_MAX, YbdConstants.USER_THUMBNAIL_HEIGHT_MAX, true);
				loginUser.Mime = transferFile.Mime;
				loginUser.LastModified = YbdCommon.DateTimeToModifiedJulianDate(DateTime.UtcNow);
				registeredUserContext.SaveChanges();

				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("プロフィール画像設定サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// ====================================================================
		// API（一般）【要認証（管理者用）】
		// ====================================================================

		// --------------------------------------------------------------------
		// ユーザー一覧
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_USERS)]
		public IActionResult GetUsers()
		{
			try
			{
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser) || !loginUser.IsAdmin)
				{
					return Unauthorized();
				}

				// キャッシュチェック
				DateTime lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REGISTERED_USERS);
				if (IsValidEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetUsers() キャッシュ有効: ");
					return NotModified();
				}

				RegisteredUser[] registeredUsersArray = registeredUsers.Where(x => !x.IsAdmin).OrderBy(x => x.Name).ToArray();
				PublicUserInfo[] results = new PublicUserInfo[registeredUsersArray.Length];
				for (Int32 i = 0; i < registeredUsersArray.Length; i++)
				{
					PublicUserInfo publicUserInfo = new();
					registeredUsersArray[i].CopyPublicInfo(publicUserInfo, true);
					results[i] = publicUserInfo;
				}
				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified));
				return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ユーザー一覧取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// ユーザー削除
		// --------------------------------------------------------------------
		[HttpDelete, Route(YbdConstants.URL_USERS + "{id?}")]
		public IActionResult DeleteUser(String? id)
		{
			try
			{
				using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser) || !loginUser.IsAdmin)
				{
					return Unauthorized();
				}
				if (String.IsNullOrEmpty(id))
				{
					return BadRequest();
				}

				RegisteredUser? deleteUser = registeredUsers.SingleOrDefault(x => x.Id == id);
				if (deleteUser == null)
				{
					return NotAcceptable();
				}
				if (deleteUser.IsAdmin)
				{
					// 管理者は削除できない
					return NotAcceptable();
				}

				registeredUsers.Remove(deleteUser);
				registeredUserContext.SaveChanges();
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("ユーザー削除サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// ====================================================================
		// テスト用
		// ====================================================================

		// --------------------------------------------------------------------
		// テスト用
		// --------------------------------------------------------------------
		[HttpGet, Route("test/")]
		public String Test()
		{
			using RegisteredUserContext registeredUserContext = CreateRegisteredUserContext(out DbSet<RegisteredUser> registeredUsers);
			return "Test isTokenValid: " + IsTokenValid(registeredUsers, out RegisteredUser? registeredUser) + " / " + Environment.TickCount.ToString("#,0");
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// 1 バイトは 8 ビット
		private const Int32 BITS_PER_BYTE = 8;

		// ソルト長 [bit]
		private const Int32 SALT_LENGTH = 128;

		// ハッシュ時の反復回数
		private const Int32 ITERATION_COUNT = 10000;

		// ハッシュ長 [bit]
		private const Int32 HASH_LENGTH = 512;

		// トークンの有効期間 [h]
		private const Int32 TOKEN_AVAILABLE_HOURS = 12;

		// authorization ヘッダー
		private const String HEADER_NAME_AUTHORIZATION = "authorization";

		// ====================================================================
		// private static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 認証トークン文字列生成
		// --------------------------------------------------------------------
		private static String GenerateToken(String id)
		{
			SymmetricSecurityKey key = ServerCommon.CreateSymmetricSecurityKey();
			SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			Claim[] claims = new Claim[]
			{
				new Claim(ClaimTypes.NameIdentifier, id),
			};
			JwtSecurityToken token = new JwtSecurityToken(ServerConstants.TOKEN_ISSUER, null, claims, null, DateTime.UtcNow.AddHours(TOKEN_AVAILABLE_HOURS), creds);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		// --------------------------------------------------------------------
		// ユーザー ID と認証トークンセットの文字列生成
		// --------------------------------------------------------------------
		private static String GenerateIdAndTokenString(String id)
		{
			return id + YbdConstants.TOKEN_DELIM + GenerateToken(id);
		}

		// --------------------------------------------------------------------
		// 登録情報のパスワードをハッシュ化
		// --------------------------------------------------------------------
		private static void HashPassword(RegisteredUser user)
		{
			// ソルトの作成
			user.Salt = new Byte[SALT_LENGTH / BITS_PER_BYTE];
			using RandomNumberGenerator generator = RandomNumberGenerator.Create();
			generator.GetBytes(user.Salt);

			// ハッシュ化
			user.Password = HashPassword(user.Password, user.Salt);
		}

		// --------------------------------------------------------------------
		// 指定されたソルトでパスワードをハッシュ化
		// --------------------------------------------------------------------
		private static String HashPassword(String password, Byte[] salt)
		{
			Byte[] hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, ITERATION_COUNT, HASH_LENGTH / BITS_PER_BYTE);
			return Convert.ToBase64String(hash);
		}

		// --------------------------------------------------------------------
		// 管理者が登録されているか
		// --------------------------------------------------------------------
		private static Boolean IsAdminRegistered(DbSet<RegisteredUser> registeredUsers)
		{
			return registeredUsers.FirstOrDefault(x => x.IsAdmin) != null;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ヘッダーの認証トークンから Id を取得
		// --------------------------------------------------------------------
		private String? GetIdFromHeader()
		{
			// Authorization ヘッダーは "Bearer Token" の形式になっている
			HttpContext.Request.Headers.TryGetValue(HEADER_NAME_AUTHORIZATION, out StringValues values);
			if (values.Count == 0)
			{
				return null;
			}
			String[] split = values[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (split.Length <= 1)
			{
				return null;
			}
			String token = split[1];

			// トークン検証
			try
			{
				TokenValidationParameters parameters = ServerCommon.TokenValidationParameters();
				JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
				ClaimsPrincipal claims = jwtSecurityTokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
				return claims.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
			}
			catch (Exception)
			{
				// トークンの有効期限切れ等の場合は例外となる
				return null;
			}
		}

		// --------------------------------------------------------------------
		// ヘッダーに記載されているトークンが有効かどうか
		// --------------------------------------------------------------------
		private Boolean IsTokenValid(DbSet<RegisteredUser> registeredUsers, [NotNullWhen(true)] out RegisteredUser? loginUser)
		{
			loginUser = null;
			String? id = GetIdFromHeader();
			if (String.IsNullOrEmpty(id))
			{
				return false;
			}

			// トークンに埋め込まれている ID が引き続き有効か（該当ユーザーが削除されていないか）確認する
			loginUser = registeredUsers.SingleOrDefault(x => x.Id == id);
			if (loginUser == null)
			{
				return false;
			}

			return true;
		}
	}
}
