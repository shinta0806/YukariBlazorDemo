// ============================================================================
// 
// クライエント側の雑多なクラス群
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
	public class TabItem
	{
		public TabItem(String label, String address)
		{
			Label = label;
			Address = address;
		}


		public String Label { get; private set; }

		public String Address { get; private set; }
	}
}
