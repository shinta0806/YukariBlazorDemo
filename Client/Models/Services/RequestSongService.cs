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
		public HttpClient HttpClient { get; }

		public RequestSongService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public async Task<IEnumerable<RequestSong>> GetRequestSongsAsync()
		{
			RequestSong[]? songs = null;
			try
			{
				songs = await HttpClient.GetFromJsonAsync<RequestSong[]>("api/requestsongs");
			}
			catch (Exception)
			{
			}
			if (songs == null)
			{
				return new RequestSong[0];
			}
			return songs;
		}
	}
}
