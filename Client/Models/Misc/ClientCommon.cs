// ============================================================================
// 
// クライアント側で共通して使われる関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Misc
{
	public class ClientCommon
	{
		// --------------------------------------------------------------------
		// 確認用ダイアログを表示
		// --------------------------------------------------------------------
		public static async Task<Boolean> ConfirmAsync(IJSRuntime jsRuntime, String message)
		{
			return await jsRuntime.InvokeAsync<Boolean>("confirm", message);
		}

		// --------------------------------------------------------------------
		// ISongProperty コピー
		// --------------------------------------------------------------------
		public static void CopySongProperty(ISongProperty source, ISongProperty dest)
		{
			dest.Path = source.Path;
			dest.SongName = source.SongName;
			dest.TieUpName = source.TieUpName;
			dest.ArtistName = source.ArtistName;
			dest.Maker = source.Maker;
			dest.Worker = source.Worker;
		}

		// --------------------------------------------------------------------
		// 疑似タブ HTML 生成
		// --------------------------------------------------------------------
		public static String GenerateTabHeader(IEnumerable<TabItem> items, String activeItemLabel)
		{
			String header = "<div class='tab-header'>";
			foreach (TabItem item in items)
			{
				if (item.Label == activeItemLabel)
				{
					header += "<div class='tab-item-active'>" + activeItemLabel + "</div>";
				}
				else
				{
					header += "<div class='tab-item'><a class='tab-item-link' href='" + item.Address + "'>" + item.Label + "</a></div>";
				}
			}
			header += "</div>";

			return header;
		}

		// --------------------------------------------------------------------
		// クライアント側のエラーページに遷移
		// --------------------------------------------------------------------
		public static void NavigateToClientError(NavigationManager navigationManager, Exception excep)
		{
			navigationManager.NavigateTo("/clienterror/" + ClientConstants.ERROR_PARAM_NAME_MESSAGE + "=" + Uri.EscapeDataString(excep.Message)
					+ "&" + ClientConstants.ERROR_PARAM_NAME_TRACE + "=" + Uri.EscapeDataString(excep.StackTrace ?? String.Empty));
		}

#if false
		// --------------------------------------------------------------------
		// id 属性が id の HTML 要素にフォーカスを当てる
		// Microsoft.AspNetCore.Components.WebAssembly v5.0.1 時点で HTML autofocus 属性が効かないため JS で実装
		// → ElementReference.FocusAsync() を使う
		// --------------------------------------------------------------------
		public static async Task SetFocusAsync(IJSRuntime jsRuntime, String id)
		{
			await jsRuntime.InvokeVoidAsync("SetFocus", id);
		}
#endif

	}
}
