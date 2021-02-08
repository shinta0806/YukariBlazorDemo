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
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Client.Models.Misc
{
	public class ClientCommon
	{
		// ====================================================================
		// public static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// アラートダイアログを表示
		// --------------------------------------------------------------------
		public static async Task AlertAsync(IJSRuntime jsRuntime, String message)
		{
			await jsRuntime.InvokeVoidAsync("alert", message);
		}

		// --------------------------------------------------------------------
		// 確認用ダイアログを表示
		// --------------------------------------------------------------------
		public static async Task<Boolean> ConfirmAsync(IJSRuntime jsRuntime, String message)
		{
			return await jsRuntime.InvokeAsync<Boolean>("confirm", message);
		}

		// --------------------------------------------------------------------
		// ページ切替 HTML 生成
		// currentPage は 0 ベース
		// --------------------------------------------------------------------
		public static String GeneratePageNavigation(Int32 numPages, Int32 currentPage, String baseUrl)
		{
			if (numPages <= 1)
			{
				return String.Empty;
			}

			String navi = "<div class='page-navi'>";

			// 前のページへ
			AddPageNavigation(ref navi, "&#9665;", currentPage != 0, baseUrl, currentPage - 1);

			// ページ
			Int32 minPage = Math.Max(0, currentPage - NUM_NAVI_PAGE_BUTTONS);
			Int32 maxPage = Math.Min(numPages, currentPage + NUM_NAVI_PAGE_BUTTONS + 1);
			for (Int32 i = minPage; i < maxPage; i++)
			{
				AddPageNavigation(ref navi, (i + 1).ToString(), i != currentPage, baseUrl, i);
			}

			// 次のページへ
			AddPageNavigation(ref navi, "&#9655;", currentPage != numPages - 1, baseUrl, currentPage + 1);

			navi += "</div>";
			return navi;
		}

		// --------------------------------------------------------------------
		// 曲名セルに表示する曲情報の HTML 生成
		// --------------------------------------------------------------------
		public static String GenerateSongInfo(ISongProperty songProperty)
		{
			String cell = "<div class='search-result-song'>" + songProperty.SongName + "</div>";
			String misc = songProperty.ArtistName;
			if (!String.IsNullOrEmpty(songProperty.TieUpName))
			{
				misc += " / " + songProperty.TieUpName;
			}
			cell += "<div class='req-list-misc'>" + misc + "</div>";
			cell += "<div class='req-list-path'>" + Path.GetFileName(songProperty.Path.Replace('\\', '/')) + "</div>";
			return cell;
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
		// 致命的なエラーの場合に使用する想定
		// 致命的でないエラーの場合は本関数は呼ばず、各ページ内に想定されているエラーメッセージを表示する
		// --------------------------------------------------------------------
		public static void NavigateToClientFatalError(NavigationManager navigationManager, Exception excep)
		{
			navigationManager.NavigateTo("/clienterror/" + ClientConstants.ERROR_PARAM_NAME_TYPE + "=" + excep.GetType().Name
					+ "&" + ClientConstants.ERROR_PARAM_NAME_MESSAGE + "=" + Uri.EscapeDataString(excep.Message)
					+ "&" + ClientConstants.ERROR_PARAM_NAME_TRACE + "=" + Uri.EscapeDataString(excep.StackTrace ?? String.Empty));
		}

		// --------------------------------------------------------------------
		// 結果表示用 HTML
		// --------------------------------------------------------------------
		public static String ResultHtml(String okMessage, String errorMessage)
		{
			if (String.IsNullOrEmpty(errorMessage))
			{
				// OK
				return "<div class='ok-message'>" + okMessage + "</div>";
			}
			else
			{
				// エラー
				return "<div class='error-message'>" + errorMessage + "</div>";
			}
		}

		// --------------------------------------------------------------------
		// メッセージが空でなければ例外にする
		// --------------------------------------------------------------------
		public static void ThrowIfError(String message)
		{
			if (!String.IsNullOrEmpty(message))
			{
				throw new Exception(message);
			}
		}

		// ====================================================================
		// public static メンバー関数（デバッグ専用）
		// ====================================================================

		[Conditional("DEBUG")]
		public static void DebugWriteLine(String message)
		{
			Console.WriteLine(message);
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		private const String CLASS_NAME_PAGE_NAVI_ITEM = "page-navi-item";
		private const String CLASS_NAME_PAGE_NAVI_CURRENT_ITEM = "page-navi-current-item";

		private const Int32 NUM_NAVI_PAGE_BUTTONS = 3;

		// ====================================================================
		// private static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ページ切替 HTML にボタン 1 つ追加
		// --------------------------------------------------------------------
		private static void AddPageNavigation(ref String navi, String label, Boolean enabled, String baseUrl, Int32 page)
		{
			if (enabled)
			{
				navi += "<a class='" + CLASS_NAME_PAGE_NAVI_ITEM + "' href='" + AddPageParameter(baseUrl, page) + "'>" + label + "</a>";
			}
			else
			{
				navi += "<span class='" + CLASS_NAME_PAGE_NAVI_CURRENT_ITEM + "'>" + label + "</span>";
			}
		}

		// --------------------------------------------------------------------
		// baseUrl に page パラメーターを追加
		// --------------------------------------------------------------------
		private static String AddPageParameter(String baseUrl, Int32 page)
		{
			if (page != 0)
			{
				if (baseUrl.Contains('?') || baseUrl.Contains('='))
				{
					baseUrl += '&';
				}
				baseUrl += YbdConstants.SEARCH_PARAM_NAME_PAGE + "=" + (page + 1).ToString();
			}
			return baseUrl;
		}

	}
}
