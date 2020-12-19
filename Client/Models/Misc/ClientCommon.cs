// ============================================================================
// 
// クライアント側で共通して使われる関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Client.Models.Misc
{
	public class ClientCommon
	{
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
	}
}
