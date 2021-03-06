﻿// ============================================================================
// 
// キャッシュの有効期限を短くする属性
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

namespace YukariBlazorDemo.Server.Attributes
{
	public class ShortCacheAttribute : CachePeriodAttribute
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ShortCacheAttribute() : base(1)
		{
			// ブラウザに内容をキャッシュさせるが、1 秒後には有効期限が切れて変更確認が来る
			// max-age=0 でも大丈夫そうだが、キャッシュは保持して欲しいので念のため 1 にしておく
		}
	}
}
