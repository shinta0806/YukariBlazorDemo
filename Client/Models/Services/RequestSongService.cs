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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Client.Models.Services
{
	public class RequestSongService
	{
#if false
		private List<RequestSong> RequestSongs { get; } = new()
		{
			new RequestSong { Id = 1, Sort = 1, Path = @"D:\Song\hoge.mp4", SongName = "Hoge", TieUpName = "HogeAnime", User = "Taro" },
			new RequestSong { Id = 2, Sort = 2, Path = @"D:\Song\fuga.mp4", SongName = "Fuga", TieUpName = "FugaAnime", User = "Hanako" },
			new RequestSong { Id = 3, Sort = 3, Path = @"D:\Song\foo.mp4", SongName = "Foo", TieUpName = "FooAnime", User = "Miyuki" },
		};
#endif

		public HttpClient HttpClient { get; }

		public RequestSongService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public async Task<IEnumerable<RequestSong>> GetRequestSongsAsync()
		{
			//return await Task.FromResult(RequestSongs);

			return await HttpClient.GetFromJsonAsync<RequestSong[]>("api/requestsongs");
		}
	}
}
