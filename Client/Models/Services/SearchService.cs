// ============================================================================
// 
// 検索結果を提供
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
	public class SearchService
	{
		public HttpClient HttpClient { get; }

		public SearchService(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public async Task<IEnumerable<AvailableSong>> GetSearchResultsAsync(SearchWord searchWord)
		{
			AvailableSong[]? results = null;
			try
			{
				results = await HttpClient.GetFromJsonAsync<AvailableSong[]>("api/search/ポ");
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
