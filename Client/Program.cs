using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.Http;
using System.Threading.Tasks;

using YukariBlazorDemo.Client.Models.Authorization;
using YukariBlazorDemo.Client.Models.Services;

namespace YukariBlazorDemo.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddBlazoredLocalStorage();
			builder.Services.AddScoped<AuthenticationStateProvider, YbdAuthenticationStateProvider>();
			builder.Services.AddScoped<AuthService>();
			builder.Services.AddScoped<PlayerService>();
			builder.Services.AddScoped<RequestSongService>();
			builder.Services.AddScoped<SearchService>();
			builder.Services.AddScoped<ThumbnailService>();

			builder.Services.AddOptions();
			builder.Services.AddAuthorizationCore();
			
			await builder.Build().RunAsync();
		}
	}
}
