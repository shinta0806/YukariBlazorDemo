// ============================================================================
// 
// API 基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;

using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	public abstract class ApiController : Controller
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ApiController()
		{
			INVALID_ETAG = GenerateEntityTag(YbdConstants.INVALID_MJD);
		}

		// ====================================================================
		// API
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[Produces(ServerConstants.MIME_TYPE_JSON)]
		[HttpGet, Route(YbdConstants.URL_STATUS)]
		public abstract String ControllerStatus();

		// ====================================================================
		// protected メンバー定数
		// ====================================================================

		// 日付が指定されていない場合の ETAG
		protected readonly EntityTagHeaderValue INVALID_ETAG;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 応答ヘッダーにアイテム総数を追加
		// --------------------------------------------------------------------
		protected void AddTotalCountToHeader(Int32 totalCount)
		{
			Response.Headers.Add(YbdConstants.HEADER_NAME_TOTAL_COUNT, new StringValues(totalCount.ToString()));
		}

		// --------------------------------------------------------------------
		// データベースコンテキスト生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected AvailableSongContext CreateAvailableSongContext(out DbSet<AvailableSong> availableSongs)
		{
			AvailableSongContext availableSongContext = new();
			if (availableSongContext.AvailableSongs == null)
			{
				throw new Exception("予約可能曲データベースにアクセスできません。");
			}
			availableSongs = availableSongContext.AvailableSongs;
			return availableSongContext;
		}

		// --------------------------------------------------------------------
		// データベースコンテキスト生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected UserProfileContext CreateUserProfileContext(out DbSet<RegisteredUser> registeredUsers, out DbSet<StockSong> stockSongs, out DbSet<HistorySong> historySongs)
		{
			UserProfileContext userProfileContext = new();

			if (userProfileContext.RegisteredUsers == null)
			{
				throw new Exception("登録ユーザーデータベースにアクセスできません。");
			}
			registeredUsers = userProfileContext.RegisteredUsers;

			if (userProfileContext.StockSongs == null)
			{
				throw new Exception("後で歌う予定データベースにアクセスできません。");
			}
			stockSongs = userProfileContext.StockSongs;

			if (userProfileContext.HistorySongs == null)
			{
				throw new Exception("予約履歴データベースにアクセスできません。");
			}
			historySongs = userProfileContext.HistorySongs;

			return userProfileContext;
		}

		// --------------------------------------------------------------------
		// データベースコンテキスト生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected RequestSongContext CreateRequestSongContext(out DbSet<RequestSong> requestSongs)
		{
			RequestSongContext requestSongContext = new();
			if (requestSongContext.RequestSongs == null)
			{
				throw new Exception("予約データベースにアクセスできません。");
			}
			requestSongs = requestSongContext.RequestSongs;
			return requestSongContext;
		}

		// --------------------------------------------------------------------
		// データベースコンテキスト生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected ThumbnailContext CreateThumbnailContext(out DbSet<Thumbnail> thumbnails)
		{
			ThumbnailContext thumbnailContext = new();
			if (thumbnailContext.Thumbnails == null)
			{
				throw new Exception("動画サムネイルデータベースにアクセスできません。");
			}
			thumbnails = thumbnailContext.Thumbnails;
			return thumbnailContext;
		}

		// --------------------------------------------------------------------
		// ETag 生成
		// --------------------------------------------------------------------
		protected EntityTagHeaderValue GenerateEntityTag(Double lastModified)
		{
			return new EntityTagHeaderValue("\"" + lastModified.ToString() + "\"");
		}

		// --------------------------------------------------------------------
		// サーバー側で処理方法がわからない事態が発生した
		// --------------------------------------------------------------------
		protected StatusCodeResult InternalServerError()
		{
			return StatusCode((Int32)HttpStatusCode.InternalServerError);
		}

		// --------------------------------------------------------------------
		// クライアント側から送られてきた ETag が有効か
		// --------------------------------------------------------------------
		protected Boolean IsEntityTagValid(Double lastModified)
		{
			if (!HttpContext.Request.Headers.TryGetValue("If-None-Match", out StringValues values))
			{
				//Debug.WriteLine("IsEntityTagValid() If-None-Match ヘッダー無し");
				return false;
			}

			//Debug.WriteLine("IsEntityTagValid() If-None-Match ヘッダーあり");
			for (Int32 i = 0; i < values.Count; i++)
			{
				String str = values[i].Trim('"');
				if (str == lastModified.ToString())
				{
					return true;
				}
			}
			return false;
		}

		// --------------------------------------------------------------------
		// ヘッダーに記載されているトークンが有効かどうか
		// --------------------------------------------------------------------
		protected Boolean IsTokenValid(DbSet<RegisteredUser> registeredUsers, [NotNullWhen(true)] out RegisteredUser? loginUser)
		{
			loginUser = null;
			String? id = GetUserIdFromHeader();
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

		// --------------------------------------------------------------------
		// 与えられた条件に合うコンテンツが見つからない
		// --------------------------------------------------------------------
		protected StatusCodeResult NotAcceptable()
		{
			return StatusCode((Int32)HttpStatusCode.NotAcceptable);
		}

		// --------------------------------------------------------------------
		// クライアント側のキャッシュが有効
		// --------------------------------------------------------------------
		protected StatusCodeResult NotModified()
		{
			return StatusCode((Int32)HttpStatusCode.NotModified);
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// authorization ヘッダー
		private const String HEADER_NAME_AUTHORIZATION = "authorization";

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ヘッダーの認証トークンからユーザー Id を取得
		// --------------------------------------------------------------------
		private String? GetUserIdFromHeader()
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
	}
}
