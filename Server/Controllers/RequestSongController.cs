using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces("application/json")]
	[Route("api/requestsongs")]
	public class RequestSongController : Controller
	{
		public RequestSongController()
		{

		}

		private List<RequestSong> RequestSongs { get; } = new()
		{
			new RequestSong { Id = 1, Sort = 1, Path = @"D:\Song\hoge.mp4", SongName = "HogeS", TieUpName = "HogeAnime", User = "Taro" },
			new RequestSong { Id = 2, Sort = 2, Path = @"D:\Song\fuga.mp4", SongName = "FugaS", TieUpName = "FugaAnime", User = "Hanako" },
			new RequestSong { Id = 3, Sort = 3, Path = @"D:\Song\foo.mp4", SongName = "FooS", TieUpName = "FooAnime", User = "Miyuki" },
		};

		[HttpGet]
		public IEnumerable<RequestSong> GetDevices()
		{
			return RequestSongs;
		}

#if false
		public IActionResult Index()
		{
			return View();
		}
#endif
	}
}
