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
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
		public IActionResult SearchByWord(String? query)
		{
			IEnumerable<AvailableSong>? results = null;
			Int32 numResults = 0;
			DateTime lastModified = ServerConstants.INVALID_DATE;
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

				IQueryable<AvailableSong> searchResults = availableSongContext.AvailableSongs;
				if (searchWord.Type == SearchWordType.AnyWord)
				{
					// なんでも検索
					String[] anyWords = SplitKeyword(searchWord.AnyWord);
					for (Int32 i = 0; i < anyWords.Length; i++)
					{
						searchResults = SearchByAnyWord(searchResults, anyWords[i]);
					}
				}
				else
				{
					// 曲名
					// ToDo: SearchBySongName() を関数化すると実行できるが、地の文にすると例外が発生する
					String[] songNames = SplitKeyword(searchWord.SongName);
					for (Int32 i = 0; i < songNames.Length; i++)
					{
						searchResults = SearchBySongName(searchResults, songNames[i]);
					}

					// タイアップ名
					String[] tieUpNames = SplitKeyword(searchWord.TieUpName);
					for (Int32 i = 0; i < tieUpNames.Length; i++)
					{
						searchResults = SearchByTieUpName(searchResults, tieUpNames[i]);
					}

					// 歌手名
					String[] artistNames = SplitKeyword(searchWord.ArtistName);
					for (Int32 i = 0; i < artistNames.Length; i++)
					{
						searchResults = SearchByArtistName(searchResults, artistNames[i]);
					}

					// 制作会社
					String[] makers = SplitKeyword(searchWord.Maker);
					for (Int32 i = 0; i < makers.Length; i++)
					{
						searchResults = SearchByMaker(searchResults, makers[i]);
					}

					// カラオケ動画制作者
					String[] workers = SplitKeyword(searchWord.Worker);
					for (Int32 i = 0; i < workers.Length; i++)
					{
						searchResults = SearchByWorker(searchResults, workers[i]);
					}

					// ファイル名
					String[] pathes = SplitKeyword(searchWord.Path);
					for (Int32 i = 0; i < pathes.Length; i++)
					{
						searchResults = SearchByPath(searchResults, pathes[i]);
					}
				}
				numResults = searchResults.Count();
				results = SortSearchResult(searchResults, searchWord.Sort).Skip(YbdConstants.PAGE_SIZE * searchWord.Page).Take(YbdConstants.PAGE_SIZE).ToArray();
				lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_AVAILABLE_SONGS);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("キーワード検索サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			if (results == null)
			{
				results = new AvailableSong[0];
			}
			EntityTagHeaderValue eTag = ServerCommon.GenerateEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
			return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// 半角スペース
		private const Char HANKAKU_SPACE = ' ';

		// 全角スペース
		private const Char ZENKAKU_SPACE = '　';

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// AnyWord 検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByAnyWord(IQueryable<AvailableSong> records, String word)
		{
			// String.Contains() が StringComparison.OrdinalIgnoreCase 付きで動作しないため、EF.Functions.Like() を使う
			// 検索結果は AvailableSongContext の寿命と共に尽きるようなので、ToArray() で新しいコンテナに格納する
			return records.Where(x => EF.Functions.Like(x.Path, $"%{word}%")
					|| EF.Functions.Like(x.SongName, $"%{word}%")
					|| EF.Functions.Like(x.TieUpName, $"%{word}%")
					|| EF.Functions.Like(x.ArtistName, $"%{word}%")
					|| EF.Functions.Like(x.Maker, $"%{word}%")
					|| EF.Functions.Like(x.Worker, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// 歌手名検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByArtistName(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.ArtistName, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// 制作会社検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByMaker(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.Maker, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// パス検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByPath(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.Path, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// 曲名検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchBySongName(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.SongName, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// タイアップ名検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByTieUpName(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.TieUpName, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// カラオケ動画制作者検索
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SearchByWorker(IQueryable<AvailableSong> records, String word)
		{
			return records.Where(x => EF.Functions.Like(x.Worker, $"%{word}%"));
		}

		// --------------------------------------------------------------------
		// 検索結果のソート
		// --------------------------------------------------------------------
		private IQueryable<AvailableSong> SortSearchResult(IQueryable<AvailableSong> result, SearchResultSort sort)
		{
			Debug.WriteLine("SortSearchResult() " + result.Count());
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

		// --------------------------------------------------------------------
		// 検索キーワードを全角・半角スペースで分割
		// --------------------------------------------------------------------
		private String[] SplitKeyword(String keyword)
		{
			keyword = keyword.Replace(ZENKAKU_SPACE, HANKAKU_SPACE);
			return keyword.Split(HANKAKU_SPACE, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
