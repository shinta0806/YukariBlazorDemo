using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllersWithViews();
			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});

			CreateDatabaseIfNeeded();
		}

		private void CreateDatabaseIfNeeded()
		{
			try
			{
				using AvailableSongContext availableSongContext = new();
				availableSongContext.Database.EnsureCreated();
				if (availableSongContext.AvailableSong == null)
				{
					throw new Exception();
				}
				if (availableSongContext.AvailableSong.Count() == 0)
				{
					// サンプルデータ作成
					AvailableSong[] availableSongs = new AvailableSong[]
					{
						new AvailableSong { Path = @"D:\Song\チューリップ.mp4", SongName = "チューリップ", TieUpName = "花花花花" },
						new AvailableSong { Path = @"D:\Song\ひまわり.mp4", SongName = "ひまわり", TieUpName = "花花花花" },
						new AvailableSong { Path = @"D:\Song\薔薇.mp4", SongName = "薔薇", TieUpName = "花花花花" },
						new AvailableSong { Path = @"D:\Song\ポインセチア.mp4", SongName = "ポインセチア", TieUpName = "花花花花" },
						new AvailableSong { Path = @"D:\Song\トイプードル.mp4", SongName = "トイプードル", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = @"D:\Song\チワワ.mp4", SongName = "チワワ", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = @"D:\Song\柴犬.mp4", SongName = "柴犬", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = @"D:\Song\ポメラニアン.mp4", SongName = "ポメラニアン", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = @"E:\AddSong\Beni.mp4", SongName = "ベニウチワ", TieUpName = "花花花花" },
					};
					availableSongContext.AvailableSong.AddRange(availableSongs);
					availableSongContext.SaveChanges();
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
