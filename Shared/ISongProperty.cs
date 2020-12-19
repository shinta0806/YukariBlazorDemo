using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Shared
{
	public interface ISongProperty
	{
		String Path { get; set; }

		String SongName { get; set; }

		String TieUpName { get; set; }

		String ArtistName { get; set; }

		String Maker { get; set; }

		String Worker { get; set; }
	}
}
