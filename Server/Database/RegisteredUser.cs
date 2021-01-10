// ============================================================================
// 
// 登録ユーザーの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using YukariBlazorDemo.Shared.Authorization;

namespace YukariBlazorDemo.Server.Database
{
	[Table("t_registered_user")]
	public class RegisteredUser
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public RegisteredUser()
		{
			Id = Guid.NewGuid().ToString();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID（削除後の登録でも重複しないよう、GUID とする）
		[Key]
		public String Id { get; set; }

		// 管理者権限かどうか
		[Required]
		public Boolean IsAdmin { get; set; }

		// 名前
		[Required]
		public String Name { get; set; } = String.Empty;

		// パスワード（保存時はハッシュ化）
		[Required]
		public String Password { get; set; } = String.Empty;

		// ソルト
		[Required]
		public Byte[] Salt { get; set; } = new Byte[0];

		// サムネイル画像
		public Byte[] Bitmap { get; set; } = new Byte[0];

		// サムネイル画像形式
		public String Mime { get; set; } = String.Empty;

		// 更新日時 UTC（修正ユリウス日）
		[Required]
		public Double LastModified { get; set; }

		// 前回ログイン日時 UTC（修正ユリウス日）
		[Required]
		public Double LastLogin { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 公開情報のみをコピー
		// --------------------------------------------------------------------
		public void CopyPublicInfo(PublicUserInfo publicUserInfo)
		{
			publicUserInfo.Id = Id;
			publicUserInfo.IsAdmin = IsAdmin;
			publicUserInfo.Name = Name;
		}

	}
}
