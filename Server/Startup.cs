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
using YukariBlazorDemo.Server.Misc;
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

			try
			{
				CreateDatabaseIfNeeded();
				ThumbnailController.DefaultThumbnail = CreateThumbnail(String.Empty, "NoImage.png");
			}
			catch (Exception excep)
			{
				Debug.WriteLine("スタートアップエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
		}

		private void CreateDatabaseIfNeeded()
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
				// 予約可能な曲のサンプルデータ作成
				AvailableSong[] availableSongs =
				{
						new AvailableSong { Path = FILE_NAME_TULIP, SongName = "チューリップ", TieUpName = "花花花花", ArtistName="歌唱海子", Maker="アニメスタジオA", Worker="製作太郎",
								LastModified = 58970.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_SUNFLOWER, SongName = "ひまわり", TieUpName = "花花花花", ArtistName="歌唱山子", Maker="アニメスタジオA", Worker="製作太郎",
								LastModified = 58969.0, FileSize = 120 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_ROSE, SongName = "薔薇", TieUpName = "花花花花", ArtistName="歌唱谷子", Maker="アニメスタジオA", Worker="製作二郎",
								LastModified = 58971.0, FileSize = 115 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_POINSETTIA, SongName = "ポインセチア", TieUpName = "花花花花", ArtistName="歌唱海子", Maker="アニメスタジオA", Worker="製作二郎",
								LastModified = 58960.0, FileSize = 118 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_TOY_POODLE, SongName = "トイプードル", TieUpName = "犬がいっぱい", ArtistName="歌唱空子", Maker="ゲームスタジオB", Worker="製作三郎",
								LastModified = 58965.0, FileSize = 95 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_CHIHUAHUA, SongName = "チワワ", TieUpName = "犬がいっぱい", ArtistName="歌唱花子", Maker="ゲームスタジオB", Worker="製作三郎",
								LastModified = 58976.0, FileSize = 205 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_SHIBA, SongName = "柴犬", TieUpName = "犬がいっぱい", ArtistName="歌唱山子", Maker="ゲームスタジオB", Worker="製作四郎",
								LastModified = 58970.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_POMERANIAN, SongName = "ポメラニアン", TieUpName = "犬がいっぱい", ArtistName="歌唱川子", Maker="ゲームスタジオB", Worker="製作四郎",
								LastModified = 58980.0, FileSize = 119 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_ANTHURIUM, SongName = "ベニウチワ", TieUpName = "花花花花", ArtistName="歌唱海子", Maker="アニメスタジオA", Worker="製作五郎",
								LastModified = 58950.0, FileSize = 103 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_IRIS, SongName = "アヤメ", TieUpName = "花花花花", ArtistName="歌唱山子", Maker="アニメスタジオA", Worker="製作五郎",
								LastModified = 58945.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_TEMPLE, SongName = "寺", TieUpName = String.Empty, ArtistName = "唱和坊主", Maker = String.Empty, Worker="製作五郎",
								LastModified = 58944.0, FileSize = 102 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_REMOTE, SongName = "リモコン", TieUpName = "家電", ArtistName = String.Empty, Maker = "アニメスタジオC", Worker="製作五郎",
								LastModified = 58943.0, FileSize = 104 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_HANA, SongName = "花と鼻", TieUpName = String.Empty, ArtistName = String.Empty, Maker = String.Empty, Worker="製作五郎",
								LastModified = 58942.0, FileSize = 101 * 1024 * 1024 },
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
				try
				{
					// サムネイルのサンプルデータ作成
					// 実際の運用時はオンデマンドでサムネイルデータを作成することが想定されるが、デモなので事前に作成してしまう
					Thumbnail[] thumbnails =
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
						CreateThumbnail(FILE_NAME_TEMPLE, "Temple.png"),
					};
					thumbnailContext.Thumbnails.AddRange(thumbnails);
					thumbnailContext.SaveChanges();
				}
				catch (Exception excep)
				{
					Debug.WriteLine("サムネイル作成エラー：\n" + excep.Message);
					Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				}
			}

			// 予約一覧
			using RequestSongContext requestSongContext = new();
			requestSongContext.Database.EnsureCreated();
		}

		private Thumbnail CreateThumbnail(String songPath, String imageFileName)
		{
			using Image sourceImage = Image.FromFile("SampleDataImages\\" + imageFileName);

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
		private const String FILE_NAME_TEMPLE = @"E:\AddSong\Temple.mp4";
		private const String FILE_NAME_REMOTE = @"E:\AddSong\Remote.mp4";
		private const String FILE_NAME_HANA = @"E:\AddSong\Hana.mp4";
	}
}
