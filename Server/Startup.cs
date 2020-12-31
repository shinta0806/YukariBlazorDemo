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

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// サムネイルサイズ
		private const Int32 THUMB_WIDTH_MAX = 160;
		private const Int32 THUMB_HEIGHT_MAX = 90;

		// 動画ファイル名（メイン）
		private const String FILE_NAME_TULIP = @"D:\Song\チューリップ.mp4";
		private const String FILE_NAME_SUNFLOWER = @"D:\Song\ひまわり.mp4";
		private const String FILE_NAME_ROSE = @"D:\Song\薔薇.mp4";
		private const String FILE_NAME_POINSETTIA = @"D:\Song\ポインセチア.mp4";
		private const String FILE_NAME_TOY_POODLE = @"D:\Song\トイプードル.mp4";
		private const String FILE_NAME_CHIHUAHUA = @"D:\Song\チワワ.mp4";
		private const String FILE_NAME_SHIBA = @"D:\Song\柴犬.mp4";
		private const String FILE_NAME_POMERANIAN = @"D:\Song\ポメラニアン.mp4";

		// 動画ファイル名（AddSong）
		private const String FILE_NAME_ANTHURIUM = @"E:\AddSong\Beni.mp4";
		private const String FILE_NAME_IRIS = @"E:\AddSong\Ayame.mp4";
		private const String FILE_NAME_TEMPLE = @"E:\AddSong\Temple.mp4";
		private const String FILE_NAME_REMOTE = @"E:\AddSong\Remote.mp4";
		private const String FILE_NAME_HANA = @"E:\AddSong\Hana.mp4";

		// 動画ファイル名（AddSong2）
		private const String FILE_NAME_FRENCH_BULLDOG = @"E:\AddSong2\FB.mp4";
		private const String FILE_NAME_SHIH_TZU = @"E:\AddSong2\ST.mp4";
		private const String FILE_NAME_MAMESHIBA = @"E:\AddSong2\Mame.mp4";
		private const String FILE_NAME_YORKSHIRE_TERRIER = @"E:\AddSong2\York.mp4";
		private const String FILE_NAME_CORGI = @"E:\AddSong2\Corgi.mp4";
		private const String FILE_NAME_GOLDEN_RETRIEVER = @"E:\AddSong2\GR.mp4";
		private const String FILE_NAME_IRIS2 = @"E:\AddSong2\Kaki.mp4";
		private const String FILE_NAME_COMMON_SNAPDRAGON = @"E:\AddSong2\CSD.mp4";

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 歌唱花子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKHanako()
		{
			return new NameAndRuby("歌唱花子", "カショウハナコ");
		}

		// --------------------------------------------------------------------
		// 歌唱川子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKKawako()
		{
			return new NameAndRuby("歌唱川子", "カショウカワコ");
		}

		// --------------------------------------------------------------------
		// 歌唱空子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKSorako()
		{
			return new NameAndRuby("歌唱空子", "カショウソラコ");
		}

		// --------------------------------------------------------------------
		// 歌唱谷子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKTaniko()
		{
			return new NameAndRuby("歌唱谷子", "カショウタニコ");
		}

		// --------------------------------------------------------------------
		// 歌唱海子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKUmiko()
		{
			return new NameAndRuby("歌唱海子", "カショウウミコ");
		}

		// --------------------------------------------------------------------
		// 歌唱山子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKYamako()
		{
			return new NameAndRuby("歌唱山子", "カショウヤマコ");
		}

		// --------------------------------------------------------------------
		// 中声出子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistNDasuko()
		{
			return new NameAndRuby("中声出子", "ナカゴエダスコ");
		}

		// --------------------------------------------------------------------
		// 中声出男
		// --------------------------------------------------------------------
		private NameAndRuby ArtistNDasuo()
		{
			return new NameAndRuby("中声出男", "ナカゴエダスオ");
		}

		// --------------------------------------------------------------------
		// 大声出子
		// --------------------------------------------------------------------
		private NameAndRuby ArtistODasuko()
		{
			return new NameAndRuby("大声出子", "オオゴエダスコ");
		}

		// --------------------------------------------------------------------
		// 大声出男
		// --------------------------------------------------------------------
		private NameAndRuby ArtistODasuo()
		{
			return new NameAndRuby("大声出男", "オオゴエダスオ");
		}

		// --------------------------------------------------------------------
		// 唱和坊主
		// --------------------------------------------------------------------
		private NameAndRuby ArtistSBouzu()
		{
			return new NameAndRuby("唱和坊主", "ショウワボウズ");
		}

		// --------------------------------------------------------------------
		// 必要に応じてデータベースを生成
		// --------------------------------------------------------------------
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
				String idPrefix = Environment.TickCount.ToString() + "-";
				Int32 idSuffix = 1;

				// 予約可能な曲のサンプルデータ作成
				AvailableSong[] availableSongs =
				{
					// メイン
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("チューリップ"), TieUpHanaHana(), ArtistKUmiko(), MakerAnimeA(), "製作太郎",
							FILE_NAME_TULIP, 58970.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ひまわり"), TieUpHanaHana(), ArtistKYamako(), MakerAnimeA(), "製作太郎",
							FILE_NAME_SUNFLOWER, 58969.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("薔薇", "バラ"), TieUpHanaHana(), ArtistKTaniko(), MakerAnimeA(), "製作太郎",
							FILE_NAME_ROSE, 58969.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ポインセチア"), TieUpHanaHana(), ArtistKUmiko(), MakerAnimeA(), "製作二郎",
							FILE_NAME_POINSETTIA, 58960.0, 118 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("トイプードル"), TieUpInugaIppai(), ArtistKSorako(), MakerGameB(), "製作三郎",
							FILE_NAME_TOY_POODLE, 58965.0, 95 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("チワワ"), TieUpInugaIppai(), ArtistKHanako(), MakerGameB(), "製作三郎",
							FILE_NAME_CHIHUAHUA, 58976.0, 205 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("柴犬", "シバイヌ"), TieUpInugaIppai(), ArtistKYamako(), MakerGameB(), "製作四郎",
							FILE_NAME_SHIBA, 58970.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ポメラニアン"), TieUpInugaIppai(), ArtistKKawako(), MakerGameB(), "製作四郎",
							FILE_NAME_POMERANIAN, 58980.0, 119 * 1024 * 1024),

					// AddSong
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ベニウチワ"), TieUpHanaHana(), ArtistKUmiko(), MakerGameB(), "製作五郎",
							FILE_NAME_ANTHURIUM, 58950.0, 103 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("アヤメ"), TieUpHanaHana(), ArtistKYamako(), MakerGameB(), "製作五郎",
							FILE_NAME_IRIS, 58945.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("寺", "テラ"), new NameAndRuby(String.Empty), ArtistSBouzu(), new NameAndRuby(String.Empty), "製作五郎",
							FILE_NAME_TEMPLE, 58944.0, 102 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("リモコン"), TieUpKaden(), new NameAndRuby(String.Empty), MakerAnimeC(), "製作五郎",
							FILE_NAME_REMOTE, 58943.0, 104 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("花と鼻", "はなとはな"), new NameAndRuby(String.Empty), new NameAndRuby(String.Empty), new NameAndRuby(String.Empty), "製作五郎",
							FILE_NAME_HANA, 58942.0, 101 * 1024 * 1024),

					// AddSong2
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("フレンチブルドッグ"), TieUpInugaTakusan(), ArtistODasuko(), MakerTakusanA(), "製作太郎",
							FILE_NAME_FRENCH_BULLDOG, 58930.0, 115 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("シーズー"), TieUpInugaTakusan(), ArtistODasuo(), MakerTakusanA(), "つくっ太郎",
							FILE_NAME_SHIH_TZU, 58931.0, 118 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("豆柴", "マメシバ"), TieUpInugaTakusan(), ArtistNDasuo(), MakerTakusanA(), "つく二郎",
							FILE_NAME_MAMESHIBA, 58971.0, 92 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ヨークシャーテリア"), TieUpInugaTakusan(), ArtistNDasuko(), MakerTakusanA(), "つく三郎",
							FILE_NAME_YORKSHIRE_TERRIER, 58931.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("コーギー"), TieUpInugaTakusan(), ArtistODasuko(), MakerTakusanA(), "つく三郎",
							FILE_NAME_CORGI, 58932.0, 121 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("ゴールデンレトリバー"), TieUpInugaTakusan(), ArtistODasuo(), MakerTakusanA(), "つく三郎",
							FILE_NAME_GOLDEN_RETRIEVER, 58933.0, 151 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("燕子花", "カキツバタ"), TieUpHanagaTakusan(), ArtistODasuo(), MakerTakusanA(), "つくっ太郎",
							FILE_NAME_IRIS2, 58934.0, 123 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("金魚草", "キンギョソウ"), TieUpHanagaTakusan(), ArtistNDasuo(), MakerTakusanC(), "つく二郎",
							FILE_NAME_COMMON_SNAPDRAGON, 58971.0, 101 * 1024 * 1024),
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
						CreateThumbnail(FILE_NAME_IRIS, "Iris.png"),
						CreateThumbnail(FILE_NAME_TEMPLE, "Temple.png"),
						CreateThumbnail(FILE_NAME_REMOTE, "Remote.png"),
						CreateThumbnail(FILE_NAME_SHIH_TZU, "ShihTzu.png"),
						CreateThumbnail(FILE_NAME_YORKSHIRE_TERRIER, "YorkshireTerrier.png"),
						CreateThumbnail(FILE_NAME_CORGI, "Corgi.png"),
						CreateThumbnail(FILE_NAME_GOLDEN_RETRIEVER, "GoldenRetriever.png"),
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

		// --------------------------------------------------------------------
		// サムネイル生成
		// --------------------------------------------------------------------
		private Thumbnail CreateThumbnail(String songPath, String imageFileName)
		{
			using Image sourceImage = Image.FromFile(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + imageFileName);

			// サムネイルサイズ
			Single scale = Math.Min((Single)THUMB_WIDTH_MAX / sourceImage.Width, (Single)THUMB_HEIGHT_MAX / (float)sourceImage.Height);
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
				Mime = ServerConstants.MIME_TYPE_PNG,
				LastModified = ServerCommon.DateTimeToModifiedJulianDate(ServerCommon.LastModified(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + imageFileName)),
			};
		}

		// --------------------------------------------------------------------
		// アニメスタジオ A
		// --------------------------------------------------------------------
		private NameAndRuby MakerAnimeA()
		{
			return new NameAndRuby("アニメスタジオA", "アニメスタジオエー");
		}

		// --------------------------------------------------------------------
		// アニメスタジオ C
		// --------------------------------------------------------------------
		private NameAndRuby MakerAnimeC()
		{
			return new NameAndRuby("アニメスタジオC", "アニメスタジオシー");
		}

		// --------------------------------------------------------------------
		// ゲームスタジオ B
		// --------------------------------------------------------------------
		private NameAndRuby MakerGameB()
		{
			return new NameAndRuby("ゲームスタジオB", "ゲームスタジオビー");
		}

		// --------------------------------------------------------------------
		// たくさんスタジオ A
		// --------------------------------------------------------------------
		private NameAndRuby MakerTakusanA()
		{
			return new NameAndRuby("たくさんスタジオA", "タクサンスタジオエー");
		}

		// --------------------------------------------------------------------
		// たくさんゲーム C
		// --------------------------------------------------------------------
		private NameAndRuby MakerTakusanC()
		{
			return new NameAndRuby("たくさんゲームC", "たくさんゲームシー");
		}

		// --------------------------------------------------------------------
		// 花がたくさん
		// --------------------------------------------------------------------
		private NameAndRuby TieUpHanagaTakusan()
		{
			return new NameAndRuby("花がたくさん", "ハナガタクサン");
		}

		// --------------------------------------------------------------------
		// 花花花花
		// --------------------------------------------------------------------
		private NameAndRuby TieUpHanaHana()
		{
			return new NameAndRuby("花花花花", "ハナハナハナハナ");
		}

		// --------------------------------------------------------------------
		// 犬がいっぱい
		// --------------------------------------------------------------------
		private NameAndRuby TieUpInugaIppai()
		{
			return new NameAndRuby("犬がいっぱい", "イヌガイッパイ");
		}

		// --------------------------------------------------------------------
		// 犬がたくさん
		// --------------------------------------------------------------------
		private NameAndRuby TieUpInugaTakusan()
		{
			return new NameAndRuby("犬がたくさん", "イヌガタクサン");
		}

		// --------------------------------------------------------------------
		// 家電
		// --------------------------------------------------------------------
		private NameAndRuby TieUpKaden()
		{
			return new NameAndRuby("家電", "カデン");
		}
	}
}
