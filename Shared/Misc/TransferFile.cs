﻿// ============================================================================
// 
// 転送するファイルの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;

namespace YukariBlazorDemo.Shared.Misc
{
	public class TransferFile
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 転送するファイルのデータ
		public Byte[] Content { get; set; } = Array.Empty<Byte>();

		// 形式
		public String Mime { get; set; } = String.Empty;
	}
}
