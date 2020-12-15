using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using YukariBlazorDemo.Server.Controllers;
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

			Debug.WriteLine("calling CreateDatabaseIfNeeded()");
			CreateDatabaseIfNeeded();
			ThumbnailController.DefaultThumbnail = CreateThumbnail(String.Empty, "NoImage.png");
		}

		private void CreateDatabaseIfNeeded()
		{
			try
			{
				// 予約可能な曲の一覧
				using AvailableSongContext availableSongContext = new();
				availableSongContext.Database.EnsureCreated();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception();
				}
				if (availableSongContext.AvailableSongs.Count() == 0)
				{
					// サンプルデータ作成
					AvailableSong[] availableSongs = new AvailableSong[]
					{
						new AvailableSong { Path = FILE_NAME_TULIP, SongName = "チューリップ", TieUpName = "花花花花" },
						new AvailableSong { Path = FILE_NAME_SUNFLOWER, SongName = "ひまわり", TieUpName = "花花花花" },
						new AvailableSong { Path = FILE_NAME_ROSE, SongName = "薔薇", TieUpName = "花花花花" },
						new AvailableSong { Path = FILE_NAME_POINSETTIA, SongName = "ポインセチア", TieUpName = "花花花花" },
						new AvailableSong { Path = FILE_NAME_TOY_POODLE, SongName = "トイプードル", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = FILE_NAME_CHIHUAHUA, SongName = "チワワ", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = FILE_NAME_SHIBA, SongName = "柴犬", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = FILE_NAME_POMERANIAN, SongName = "ポメラニアン", TieUpName = "犬がいっぱい" },
						new AvailableSong { Path = FILE_NAME_ANTHURIUM, SongName = "ベニウチワ", TieUpName = "花花花花" },
						new AvailableSong { Path = FILE_NAME_IRIS, SongName = "アヤメ", TieUpName = "花花花花" },
					};
					availableSongContext.AvailableSongs.AddRange(availableSongs);
					availableSongContext.SaveChanges();
				}

				// 予約可能な曲のサムネイル
				using ThumbnailContext thumbnailContext = new();
				thumbnailContext.Database.EnsureCreated();
				if (thumbnailContext.Thumbnails == null)
				{
					throw new Exception();
				}
				if (thumbnailContext.Thumbnails.Count() == 0)
				{
					// サンプルデータ作成
					Thumbnail[] thumbnails = new Thumbnail[]
					{
						CreateThumbnail(FILE_NAME_TULIP, "Tulip.png"),
						CreateThumbnail(FILE_NAME_SUNFLOWER, "Sunflower.png"),
						CreateThumbnail(FILE_NAME_ROSE, "Rose.png"),
						CreateThumbnail(FILE_NAME_POINSETTIA, "Poinsettia.png"),
						CreateThumbnail(FILE_NAME_TOY_POODLE, "ToyPoodle.png"),
						CreateThumbnail(FILE_NAME_CHIHUAHUA, "Chihuahua.png"),
						CreateThumbnail(FILE_NAME_SHIBA, "Shiba.png"),
						CreateThumbnail(FILE_NAME_POMERANIAN, "Pomeranian.png"),
						CreateThumbnail(FILE_NAME_ANTHURIUM, "Anthurium.png"),
					};
					thumbnailContext.Thumbnails.AddRange(thumbnails);
					thumbnailContext.SaveChanges();
				}

				// 予約一覧
				using RequestSongContext requestSongContext = new();
				requestSongContext.Database.EnsureCreated();
			}
			catch (Exception)
			{
			}
		}

		private Thumbnail CreateThumbnail(String songPath, String imageFileName)
		{
			using Image sourceImage = Image.FromFile("SampleDataImage\\" + imageFileName);

			// サムネイルサイズ
			Single scale = Math.Min((Single)SCALE_WIDTH / sourceImage.Width, (Single)SCALE_HEIGHT / (float)sourceImage.Height);
			Int32 scaledWidth = (Int32)(sourceImage.Width * scale);
			Int32 scaledHeight = (Int32)(sourceImage.Height * scale);

			// サムネイル生成
			using Bitmap scaledImage = new Bitmap(scaledWidth, scaledHeight);
			using Graphics graphics = Graphics.FromImage(scaledImage);
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(sourceImage, 0, 0, scaledWidth, scaledHeight);

			// シリアル化
			using MemoryStream stream = new();
			scaledImage.Save(stream, ImageFormat.Png);
			stream.Position = 0;

			// レコード生成
			return new Thumbnail
			{
				Path = songPath,
				Bitmap = stream.ToArray(),
				Mime = "image/png",
			};
		}

		private const Int32 SCALE_WIDTH = 160;
		private const Int32 SCALE_HEIGHT = 90;

		private const String FILE_NAME_TULIP = @"D:\Song\チューリップ.mp4";
		private const String FILE_NAME_SUNFLOWER = @"D:\Song\ひまわり.mp4";
		private const String FILE_NAME_ROSE = @"D:\Song\薔薇.mp4";
		private const String FILE_NAME_POINSETTIA = @"D:\Song\ポインセチア.mp4";
		private const String FILE_NAME_TOY_POODLE = @"D:\Song\トイプードル.mp4";
		private const String FILE_NAME_CHIHUAHUA = @"D:\Song\チワワ.mp4";
		private const String FILE_NAME_SHIBA = @"D:\Song\柴犬.mp4";
		private const String FILE_NAME_POMERANIAN = @"D:\Song\ポメラニアン.mp4";
		private const String FILE_NAME_ANTHURIUM = @"E:\AddSong\Beni.mp4";
		private const String FILE_NAME_IRIS = @"E:\AddSong\Ayame.mp4";
	}
}
