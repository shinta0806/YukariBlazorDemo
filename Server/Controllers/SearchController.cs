// ============================================================================
// 
// 検索 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_SEARCH)]
	public class SearchController : Controller
	{
		// ====================================================================
		// API
		// ====================================================================

		// --------------------------------------------------------------------
		// AvailableSong.Id で曲を検索
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_ID + "{id}")]
		public AvailableSong? SearchById(String? id)
		{
			AvailableSong? result = null;
			try
			{
				using AvailableSongContext availableSongContext = new();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception();
				}
				result = availableSongContext.AvailableSongs.FirstOrDefault(x => x.Id == id);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("曲取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return result;
		}

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_STATUS)]
		public String SearchControllerStatus()
		{
			String status;
			try
			{
				using AvailableSongContext availableSongContext = new();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception("データベースにアクセスできません。");
				}
#if DEBUG
				Thread.Sleep(1000);
#endif

				// Where を使用すると列の不足を検出できる
				availableSongContext.AvailableSongs.Where(x => x.Id == String.Empty).FirstOrDefault();

				status = "正常 / 曲数：" + availableSongContext.AvailableSongs.Count();
			}
			catch (Exception excep)
			{
				status = "エラー / " + excep.Message;
				Debug.WriteLine("検索 API 状態取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return status;
		}

		// --------------------------------------------------------------------
		// キーワードで曲を検索
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_WORD + "{query}")]
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
						// 検索結果は AvailableSongContext の寿命と共に尽きるようなので、ToArray() で新しいコンテナに格納する
						results = SortSearchResult(availableSongContext.AvailableSongs.Where(x => EF.Functions.Like(x.Path, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.SongName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.TieUpName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.ArtistName, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.Maker, $"%{searchWord.AnyWord}%")
								|| EF.Functions.Like(x.Worker, $"%{searchWord.AnyWord}%")), searchWord.Sort).Take(RESULT_MAX).ToArray();
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
							&& (String.IsNullOrEmpty(searchWord.Worker) || !String.IsNullOrEmpty(searchWord.Worker) && EF.Functions.Like(x.Worker, $"%{searchWord.Worker}%"))), searchWord.Sort).
							Take(RESULT_MAX).ToArray();
				}
			}
			catch (Exception excep)
			{
				Debug.WriteLine("キーワード検索サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			if (results == null)
			{
				return new AvailableSong[0];
			}
			return results;
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// 検索結果を返す最大数
		private Int32 RESULT_MAX = 100;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 検索結果のソート
		// --------------------------------------------------------------------
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
