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
using System.Net.Http.Headers;
using System.Threading.Tasks;

using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Misc
{
	public class ClientCommon
	{
		// ====================================================================
		// public static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ETag をパラメーター名と値に分離
		// --------------------------------------------------------------------
		public static Dictionary<String, String> AnalyzeEntityTag(EntityTagHeaderValue? eTag)
		{
			if (eTag == null)
			{
				return new();
			}
			return YbdCommon.AnalyzeQuery(eTag.Tag.Trim('"'));
		}

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
			dest.SongName = source.SongName;
			dest.TieUpName = source.TieUpName;
			dest.ArtistName = source.ArtistName;
			dest.Maker = source.Maker;
			dest.Worker = source.Worker;
			dest.Path = source.Path;
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
		// ページ切替 HTML 生成
		// currentPage は 0 ベース
		// --------------------------------------------------------------------
		public static String GeneratePageNavigation(Int32 numPages, Int32 currentPage, String baseUrl)
		{
			if (numPages < 1)
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
		// クライアント側のエラーページに遷移
		// --------------------------------------------------------------------
		public static void NavigateToClientError(NavigationManager navigationManager, Exception excep)
		{
			navigationManager.NavigateTo("/clienterror/" + ClientConstants.ERROR_PARAM_NAME_MESSAGE + "=" + Uri.EscapeDataString(excep.Message)
					+ "&" + ClientConstants.ERROR_PARAM_NAME_TRACE + "=" + Uri.EscapeDataString(excep.StackTrace ?? String.Empty));
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		private const String CLASS_NAME_PAGE_NAVI_ITEM = "page-navi-item";
		private const String CLASS_NAME_PAGE_NAVI_CURRENT_ITEM = "page-navi-current-item";

		private const Int32 NUM_NAVI_PAGE_BUTTONS = 3;

		// ====================================================================
		// public static メンバー関数
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
			if (baseUrl.IndexOf('?') >= 0 || baseUrl.IndexOf('=') >= 0)
			{
				baseUrl += '&';
			}
			else
			{
				baseUrl += '?';
			}
			baseUrl += YbdConstants.SEARCH_PARAM_NAME_PAGE + "=" + (page + 1).ToString();
			return baseUrl;
		}

	}
}
