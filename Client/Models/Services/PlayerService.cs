// ============================================================================
// 
// 再生制御を提供
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
	public class PlayerService
	{
		public HttpClient HttpClient { get; }

		public PlayerService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public async Task<RequestSong?> GetPlayingSongAsync()
		{
			RequestSong? playingSong = null;
			try
			{
				playingSong = await HttpClient.GetFromJsonAsync<RequestSong?>("api/player/playing");
			}
			catch (Exception)
			{
			}
			return playingSong;
		}

		public async Task<HttpResponseMessage> PlayOrPauseAsync()
		{
			return await HttpClient.PostAsJsonAsync("api/player/playorpause", 0);
		}

		public async Task<HttpResponseMessage> NextAsync()
		{
			return await HttpClient.PostAsJsonAsync("api/player/next", 0);
		}

		public async Task<HttpResponseMessage> PrevAsync()
		{
			return await HttpClient.PostAsJsonAsync("api/player/prev", 0);
		}

	}
}
