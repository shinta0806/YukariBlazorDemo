// ============================================================================
// 
// リクエストされた曲の一覧を提供
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class RequestSongService
	{
		private List<RequestSong> RequestSongs { get; } = new()
		{
			new RequestSong { Id = 1, Sort = 1, Path = @"D:\Song\hoge.mp4", SongName = "Hoge", TieUpName = "HogeAnime", User = "Taro" },
			new RequestSong { Id = 2, Sort = 2, Path = @"D:\Song\fuga.mp4", SongName = "Fuga", TieUpName = "FugaAnime", User = "Hanako" },
			new RequestSong { Id = 3, Sort = 3, Path = @"D:\Song\foo.mp4", SongName = "Foo", TieUpName = "FooAnime", User = "Miyuki" },
		};

		public IEnumerable<RequestSong> GetRequestSongs()
		{
			return RequestSongs;
		}
	}
}
