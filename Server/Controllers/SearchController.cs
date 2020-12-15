using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces("application/json")]
	[Route("api/search")]
	public class SearchController : Controller
	{
		public SearchController()
		{

		}

		[HttpGet, Route("id/{id}")]
		public AvailableSong? SearchById(String? id)
		{
			return ServerCommon.AvailableSongById(id);
		}

		[HttpGet, Route("word/{query}")]
		public IEnumerable<AvailableSong> SearchByWord(String? query)
		{
			IEnumerable<AvailableSong>? results = null;
			try
			{
				SearchWord searchWord = new(query);
				if (!searchWord.IsValid(out String? errorMessage))
				{
					throw new Exception(errorMessage);
				}
				using AvailableSongContext availableSongContext = new();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception();
				}
				if (searchWord.Type == SearchWordType.AnyWord)
				{
					if (!String.IsNullOrEmpty(searchWord.AnyWord))
					{
#if false
						var a = availableSongContext.AvailableSong.Count();
						var b = availableSongContext.AvailableSong.Where(x => x.Id == 5);
						var b1 = b.ToList();
						var c = availableSongContext.AvailableSong.Where(x => x.Path.StartsWith("D"));
						var c1 = c.ToList();
						var d = availableSongContext.AvailableSong.Where(x => x.Path.Contains("E"));
						var d1 = d.ToList();
#endif

						// OK
						//results = availableSongContext.AvailableSong.Where(x => x.Path.Contains(searchWord.AnyWord)).ToList();

						// NG
						//results = availableSongContext.AvailableSong.Where(x => x.Path.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase)).ToList();	

						// NG
						//results = availableSongContext.AvailableSong.Where(x => x.Path.Contains(searchWord.AnyWord, StringComparison.InvariantCultureIgnoreCase)).ToList();

						// String.Contains() が StringComparison.OrdinalIgnoreCase 付きで動作しないため、EF.Functions.Like() を使う
						results = availableSongContext.AvailableSongs.Where(x => EF.Functions.Like(x.Path, $"%{searchWord.AnyWord}%")
								|| !String.IsNullOrEmpty(x.SongName) && EF.Functions.Like(x.SongName, $"%{searchWord.AnyWord}%")
								|| !String.IsNullOrEmpty(x.TieUpName) && EF.Functions.Like(x.TieUpName, $"%{searchWord.AnyWord}%")).ToList();

#if false
						results = availableSongContext.AvailableSong.Where(x => x.Path.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase)
								|| !String.IsNullOrEmpty(x.SongName) && x.SongName.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase)
								|| !String.IsNullOrEmpty(x.TieUpName) && x.TieUpName.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase)).ToList();
#endif
					}
				}
				else
				{
					results = availableSongContext.AvailableSongs.Where(x =>
							(String.IsNullOrEmpty(searchWord.FileName) || !String.IsNullOrEmpty(searchWord.FileName) && EF.Functions.Like(x.Path, $"%{searchWord.FileName}%"))
							&& (String.IsNullOrEmpty(searchWord.SongName) || !String.IsNullOrEmpty(searchWord.SongName) && EF.Functions.Like(x.SongName, $"%{searchWord.SongName}%"))
							&& (String.IsNullOrEmpty(searchWord.TieUpName) || !String.IsNullOrEmpty(searchWord.TieUpName) && EF.Functions.Like(x.TieUpName, $"%{searchWord.TieUpName}%"))).ToList();
#if false
					results = availableSongContext.AvailableSong.Where(x => (String.IsNullOrEmpty(searchWord.FileName) || !String.IsNullOrEmpty(searchWord.FileName) && x.Path.Contains(searchWord.FileName))
							&& (String.IsNullOrEmpty(searchWord.SongName) || !String.IsNullOrEmpty(searchWord.SongName) && !String.IsNullOrEmpty(x.SongName) && x.SongName.Contains(searchWord.SongName))
							&& (String.IsNullOrEmpty(searchWord.TieUpName) || !String.IsNullOrEmpty(searchWord.TieUpName) && !String.IsNullOrEmpty(x.TieUpName) && x.TieUpName.Contains(searchWord.TieUpName)));
#endif
				}
			}
			catch (Exception)
			{
			}
			if (results == null)
			{
				return new AvailableSong[0];
			}
			return results;
		}

	}
}
