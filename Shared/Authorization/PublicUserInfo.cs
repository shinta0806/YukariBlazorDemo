﻿// ============================================================================
// 
// ユーザーの公開情報（他人も取得可能な情報）
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;

namespace YukariBlazorDemo.Shared.Authorization
{
	public class PublicUserInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID
		public Int32 Id { get; set; }

		// 管理者権限かどうか
		public Boolean IsAdmin { get; set; }

		// 名前
		public String Name { get; set; } = String.Empty;
	}
}