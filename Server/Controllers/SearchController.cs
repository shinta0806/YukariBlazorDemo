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
				SearchWord searchWord = new SearchWord(query);
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
						// String.Contains() が StringComparison.OrdinalIgnoreCase 付きで動作しないため、EF.Functions.Like() を使う
						// 検索結果は AvailableSongContext の寿命と共に尽きるようなので、ToList() で新しいコンテナに格納する
						results = SortSearchResult(availableSongContext.AvailableSongs.Where(x => EF.Functions.Like(x.Path, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.SongName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.TieUpName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.ArtistName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.Maker, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.Worker, $"%{searchWord.AnyWord}%")), searchWord.Sort).ToList();
					}
				}
				else
				{
					results = SortSearchResult(availableSongContext.AvailableSongs.Where(x =>
							(String.IsNullOrEmpty(searchWord.Path) || !String.IsNullOrEmpty(searchWord.Path) && EF.Functions.Like(x.Path, $"%{searchWord.Path}%"))
							&& (String.IsNullOrEmpty(searchWord.SongName) || !String.IsNullOrEmpty(searchWord.SongName) && EF.Functions.Like(x.SongName, $"%{searchWord.SongName}%"))
							&& (String.IsNullOrEmpty(searchWord.TieUpName) || !String.IsNullOrEmpty(searchWord.TieUpName) && EF.Functions.Like(x.TieUpName, $"%{searchWord.TieUpName}%"))
							&& (String.IsNullOrEmpty(searchWord.ArtistName) || !String.IsNullOrEmpty(searchWord.ArtistName) && EF.Functions.Like(x.ArtistName, $"%{searchWord.ArtistName}%"))
							&& (String.IsNullOrEmpty(searchWord.Maker) || !String.IsNullOrEmpty(searchWord.Maker) && EF.Functions.Like(x.Maker, $"%{searchWord.Maker}%"))
							&& (String.IsNullOrEmpty(searchWord.Worker) || !String.IsNullOrEmpty(searchWord.Worker) && EF.Functions.Like(x.Worker, $"%{searchWord.Worker}%"))), searchWord.Sort).ToList();
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

		private IQueryable<AvailableSong> SortSearchResult(IQueryable<AvailableSong> result, SearchResultSort sort)
		{
			switch (sort)
			{
				case SearchResultSort.SongName:
					return result.OrderBy(x => x.SongName);
				case SearchResultSort.ArtistName:
					return result.OrderBy(x => x.ArtistName);
				case SearchResultSort.FileSize:
					return result.OrderByDescending(x => x.FileSize);
				default:
					return result.OrderByDescending(x => x.LastModified);
			}
		}

	}
}
