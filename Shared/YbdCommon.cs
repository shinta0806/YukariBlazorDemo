// ============================================================================
// 
// YukariBlazorDemo 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace YukariBlazorDemo.Shared
{
	public class YbdCommon
	{
		// --------------------------------------------------------------------
		// URL のクエリ部分をパラメーター名と値に分離
		// --------------------------------------------------------------------
		public static Dictionary<String, String> AnalyzeQuery(String? query)
		{
			Dictionary<String, String> result = new();
			if (!String.IsNullOrEmpty(query))
			{
				String[] parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
				foreach (String param in parameters)
				{
					String[] elements = param.Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (elements.Length < 2)
					{
						continue;
					}
					String paramName = elements[0];
					String paramValue = Uri.UnescapeDataString(elements[1]);
					result[paramName] = paramValue;
				}
			}
			return result;
		}

	}
}
