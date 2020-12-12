﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		private List<AvailableSong> AvailableSongs { get; } = new()
		{
			new AvailableSong { Id = 1, Path = @"D:\Song\チューリップ.mp4", SongName = "チューリップ", TieUpName = "花花花花" },
			new AvailableSong { Id = 2, Path = @"D:\Song\ひまわり.mp4", SongName = "ひまわり", TieUpName = "花花花花" },
			new AvailableSong { Id = 3, Path = @"D:\Song\薔薇.mp4", SongName = "薔薇", TieUpName = "花花花花" },
			new AvailableSong { Id = 4, Path = @"D:\Song\ポインセチア.mp4", SongName = "ポインセチア", TieUpName = "花花花花" },
			new AvailableSong { Id = 5, Path = @"D:\Song\トイプードル.mp4", SongName = "トイプードル", TieUpName = "犬がいっぱい" },
			new AvailableSong { Id = 6, Path = @"D:\Song\チワワ.mp4", SongName = "チワワ", TieUpName = "犬がいっぱい" },
			new AvailableSong { Id = 7, Path = @"D:\Song\柴犬.mp4", SongName = "柴犬", TieUpName = "犬がいっぱい" },
			new AvailableSong { Id = 8, Path = @"D:\Song\ポメラニアン.mp4", SongName = "ポメラニアン", TieUpName = "犬がいっぱい" },
		};

		[HttpGet, Route("{query}")]
		public IEnumerable<AvailableSong> GetSearchResults(String query)
		{
			IEnumerable<AvailableSong>? results = null;
			try
			{
				SearchWord searchWord = new(query);
				if (!searchWord.IsValid(out String? errorMessage))
				{
					throw new Exception(errorMessage);
				}
				if (searchWord.Type == SearchWordType.AnyWord)
				{
					if (!String.IsNullOrEmpty(searchWord.AnyWord))
					{
						results = AvailableSongs.Where(x => x.Path.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase)
								|| (x.SongName?.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase) ?? false)
								|| (x.TieUpName?.Contains(searchWord.AnyWord, StringComparison.OrdinalIgnoreCase) ?? false));
					}
				}
				else
				{
					results = AvailableSongs.Where(x => (String.IsNullOrEmpty(searchWord.FileName) || !String.IsNullOrEmpty(searchWord.FileName) && x.Path.Contains(searchWord.FileName))
							&& (String.IsNullOrEmpty(searchWord.SongName) || !String.IsNullOrEmpty(searchWord.SongName) && !String.IsNullOrEmpty(x.SongName) && x.SongName.Contains(searchWord.SongName))
							&& (String.IsNullOrEmpty(searchWord.TieUpName) || !String.IsNullOrEmpty(searchWord.TieUpName) && !String.IsNullOrEmpty(x.TieUpName) && x.TieUpName.Contains(searchWord.TieUpName)));
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
