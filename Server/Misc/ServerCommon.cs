// ============================================================================
// 
// サーバー側で共通して使われる関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.IdentityModel.Tokens;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YukariBlazorDemo.Server.Misc
{
	public class ServerCommon
	{
		// ====================================================================
		// public static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// SymmetricSecurityKey 生成
		// --------------------------------------------------------------------
		public static SymmetricSecurityKey CreateSymmetricSecurityKey()
		{
			return new SymmetricSecurityKey(_tokenSecretKey);
		}

		// --------------------------------------------------------------------
		// サムネイル生成（アスペクト比維持）
		// square == true の場合、画像としては正方形で、アスペクト比維持のコンテンツが中に描画される
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static Byte[] CreateThumbnail(Stream sourceStream, String mime, Int32 maxWidth, Int32 maxHeight, Boolean square)
		{
			using Image sourceImage = Image.FromStream(sourceStream);
			if (sourceImage.Width <= maxWidth && sourceImage.Height <= maxHeight && !square)
			{
				// 縮小の必要無し
				Byte[] destBytes = new Byte[sourceStream.Length];
				sourceStream.Position = 0;
				sourceStream.Read(destBytes);
				return destBytes;
			}

			// サムネイルサイズ
			Single scale = Math.Min(Math.Min((Single)maxWidth / sourceImage.Width, (Single)maxHeight / sourceImage.Height), 1.0f);
			Int32 scaledWidth = (Int32)(sourceImage.Width * scale);
			Int32 scaledHeight = (Int32)(sourceImage.Height * scale);
			Int32 thumbWidth;
			Int32 thumbHeight;
			Int32 offsetX;
			Int32 offsetY;
			if (square)
			{
				thumbWidth = thumbHeight = Math.Max(scaledWidth, scaledHeight);
				offsetX = (thumbWidth - scaledWidth) / 2;
				offsetY = (thumbHeight - scaledHeight) / 2;
			}
			else
			{
				thumbWidth = scaledWidth;
				thumbHeight = scaledHeight;
				offsetX = offsetY = 0;
			}

			// サムネイル生成
			using Bitmap scaledImage = new Bitmap(thumbWidth, thumbHeight);
			using Graphics graphics = Graphics.FromImage(scaledImage);
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			if (square && mime == ServerConstants.MIME_TYPE_JPEG)
			{
				using Brush brush = new SolidBrush(USER_THUMBNAIL_BG_COLOR);
				graphics.FillRectangle(brush, 0, 0, thumbWidth, thumbHeight);
			}
			graphics.DrawImage(sourceImage, offsetX, offsetY, scaledWidth, scaledHeight);

			// シリアル化
			using MemoryStream destStream = new();
			switch (mime)
			{
				case ServerConstants.MIME_TYPE_GIF:
					scaledImage.Save(destStream, ImageFormat.Gif);
					break;
				case ServerConstants.MIME_TYPE_JPEG:
					scaledImage.Save(destStream, ImageFormat.Jpeg);
					break;
				case ServerConstants.MIME_TYPE_PNG:
					scaledImage.Save(destStream, ImageFormat.Png);
					break;
				default:
					throw new Exception();
			}
			destStream.Position = 0;
			return destStream.ToArray();
		}

		// --------------------------------------------------------------------
		// トークン生成用の秘密鍵が要件を満たしているか
		// --------------------------------------------------------------------
		public static Boolean IsTokenSecretKeyValid()
		{
			return _tokenSecretKey.Length >= ServerConstants.TOKEN_SECRET_KEY_LENGTH_MIN;
		}

		// --------------------------------------------------------------------
		// ファイルの更新日時（UTC）
		// --------------------------------------------------------------------
		public static DateTime LastModified(String path)
		{
			FileInfo fileInfo = new FileInfo(path);
			return fileInfo.LastWriteTimeUtc;
		}

		// --------------------------------------------------------------------
		// トークン生成用の秘密鍵を用意
		// --------------------------------------------------------------------
		public static void PrepareTokenSecretKey()
		{
			if (File.Exists(FILE_NAME_TOKEN_SECRET_KEY))
			{
				// 秘密鍵が存在しているので読み込む
				_tokenSecretKey = Encoding.UTF8.GetBytes(File.ReadAllText(FILE_NAME_TOKEN_SECRET_KEY, Encoding.UTF8));
				return;
			}

			// 生成
			Byte[] source = new Byte[ServerConstants.TOKEN_SECRET_KEY_LENGTH_MIN];
			using RandomNumberGenerator generator = RandomNumberGenerator.Create();
			generator.GetBytes(source);
			String keyString = Convert.ToBase64String(source);
			File.WriteAllText(FILE_NAME_TOKEN_SECRET_KEY, keyString, Encoding.UTF8);
			_tokenSecretKey = Encoding.UTF8.GetBytes(keyString);
		}

		// --------------------------------------------------------------------
		// トークン検証パラメーター
		// --------------------------------------------------------------------
		public static TokenValidationParameters TokenValidationParameters()
		{
			return new()
			{
				ValidateIssuer = true,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = ServerConstants.TOKEN_ISSUER,
				IssuerSigningKey = CreateSymmetricSecurityKey(),
			};
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// サムネイル作成時の背景色（クライアント側のヘッダー色と合わせる）
		private static readonly Color USER_THUMBNAIL_BG_COLOR = Color.FromArgb(0x22, 0x22, 0x22);

		// トークン生成用の秘密鍵のファイル
		private const String FILE_NAME_TOKEN_SECRET_KEY = "TokenSecretKey.txt";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// トークン生成用の秘密鍵
		private static Byte[] _tokenSecretKey = Array.Empty<Byte>();
	}
}
