using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using YukariBlazorDemo.Server.Controllers;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

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

			// �閧������
			ServerCommon.PrepareTokenSecretKey();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
			{
				// �閧��������
				options.TokenValidationParameters = ServerCommon.TokenValidationParameters();
			});
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

			// UseAuthentication() �� UseAuthorization() ����ɌĂ΂Ȃ��Ƃ����Ȃ��炵���i��ŌĂԂ� API �R�[������ 401 �ɂȂ�j
			app.UseAuthentication();

			// UseAuthorization() �� UseRouting() �� UseEndpoints() �̊Ԃɂ��Ȃ��Ƃ����Ȃ��i�͈͊O���ƃR���\�[���ɃT�W�F�X�g���o��j
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});

			try
			{
				CreateDatabaseIfNeeded();
				ThumbnailController.DefaultThumbnail = CreateMovieThumbnail(String.Empty, "DefaultThumbnail.png");
				AuthController.DefaultGuestUserThumbnail = CreateUserThumbnail("DefaultGuestUser.png");
				AuthController.DefaultAdminUserThumbnail = CreateUserThumbnail("DefaultAdminUser.png");
				AuthController.DefaultRegisteredUserThumbnail = CreateUserThumbnail("DefaultRegisteredUser.png");

				//TestCreateThumbnail();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("�X�^�[�g�A�b�v�G���[�F\n" + excep.Message);
				Debug.WriteLine("�@�X�^�b�N�g���[�X�F\n" + excep.StackTrace);
			}
		}

		// ====================================================================
		// private �����o�[�萔
		// ====================================================================

		// ����T���l�C���T�C�Y
		private const Int32 MOVIE_THUMB_WIDTH_MAX = 160;
		private const Int32 MOVIE_THUMB_HEIGHT_MAX = 90;

		// ����t�@�C�����i���C���j
		private const String FILE_NAME_TULIP = @"D:\Song\�`���[���b�v.mp4";
		private const String FILE_NAME_SUNFLOWER = @"D:\Song\�Ђ܂��.mp4";
		private const String FILE_NAME_ROSE = @"D:\Song\�K�N.mp4";
		private const String FILE_NAME_POINSETTIA = @"D:\Song\�|�C���Z�`�A.mp4";
		private const String FILE_NAME_TOY_POODLE = @"D:\Song\�g�C�v�[�h��.mp4";
		private const String FILE_NAME_CHIHUAHUA = @"D:\Song\�`����.mp4";
		private const String FILE_NAME_SHIBA = @"D:\Song\�Č�.mp4";
		private const String FILE_NAME_POMERANIAN = @"D:\Song\�|�����j�A��.mp4";

		// ����t�@�C�����iAddSong�j
		private const String FILE_NAME_ANTHURIUM = @"E:\AddSong\Beni.mp4";
		private const String FILE_NAME_IRIS = @"E:\AddSong\Ayame.mp4";
		private const String FILE_NAME_TEMPLE = @"E:\AddSong\Temple.mp4";
		private const String FILE_NAME_REMOTE = @"E:\AddSong\Remote.mp4";
		private const String FILE_NAME_HANA = @"E:\AddSong\Hana.mp4";

		// ����t�@�C�����iAddSong2�j
		private const String FILE_NAME_FRENCH_BULLDOG = @"E:\AddSong2\FB.mp4";
		private const String FILE_NAME_SHIH_TZU = @"E:\AddSong2\ST.mp4";
		private const String FILE_NAME_MAMESHIBA = @"E:\AddSong2\Mame.mp4";
		private const String FILE_NAME_YORKSHIRE_TERRIER = @"E:\AddSong2\York.mp4";
		private const String FILE_NAME_CORGI = @"E:\AddSong2\Corgi.mp4";
		private const String FILE_NAME_GOLDEN_RETRIEVER = @"E:\AddSong2\GR.mp4";
		private const String FILE_NAME_IRIS2 = @"E:\AddSong2\Kaki.mp4";
		private const String FILE_NAME_COMMON_SNAPDRAGON = @"E:\AddSong2\CSD.mp4";

		// ====================================================================
		// private �����o�[�֐�
		// ====================================================================

		// --------------------------------------------------------------------
		// �̏��Ԏq
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKHanako()
		{
			return new NameAndRuby("�̏��Ԏq", "�J�V���E�n�i�R");
		}

		// --------------------------------------------------------------------
		// �̏���q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKKawako()
		{
			return new NameAndRuby("�̏���q", "�J�V���E�J���R");
		}

		// --------------------------------------------------------------------
		// �̏���q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKSorako()
		{
			return new NameAndRuby("�̏���q", "�J�V���E�\���R");
		}

		// --------------------------------------------------------------------
		// �̏��J�q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKTaniko()
		{
			return new NameAndRuby("�̏��J�q", "�J�V���E�^�j�R");
		}

		// --------------------------------------------------------------------
		// �̏��C�q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKUmiko()
		{
			return new NameAndRuby("�̏��C�q", "�J�V���E�E�~�R");
		}

		// --------------------------------------------------------------------
		// �̏��R�q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistKYamako()
		{
			return new NameAndRuby("�̏��R�q", "�J�V���E���}�R");
		}

		// --------------------------------------------------------------------
		// �����o�q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistNDasuko()
		{
			return new NameAndRuby("�����o�q", "�i�J�S�G�_�X�R");
		}

		// --------------------------------------------------------------------
		// �����o�j
		// --------------------------------------------------------------------
		private NameAndRuby ArtistNDasuo()
		{
			return new NameAndRuby("�����o�j", "�i�J�S�G�_�X�I");
		}

		// --------------------------------------------------------------------
		// �吺�o�q
		// --------------------------------------------------------------------
		private NameAndRuby ArtistODasuko()
		{
			return new NameAndRuby("�吺�o�q", "�I�I�S�G�_�X�R");
		}

		// --------------------------------------------------------------------
		// �吺�o�j
		// --------------------------------------------------------------------
		private NameAndRuby ArtistODasuo()
		{
			return new NameAndRuby("�吺�o�j", "�I�I�S�G�_�X�I");
		}

		// --------------------------------------------------------------------
		// ���a�V��
		// --------------------------------------------------------------------
		private NameAndRuby ArtistSBouzu()
		{
			return new NameAndRuby("���a�V��", "�V���E���{�E�Y");
		}

		// --------------------------------------------------------------------
		// �K�v�ɉ����ăf�[�^�x�[�X�𐶐�
		// --------------------------------------------------------------------
		private void CreateDatabaseIfNeeded()
		{
			// �\��\�ȋȂ̈ꗗ
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

				// �\��\�ȋȂ̃T���v���f�[�^�쐬
				AvailableSong[] availableSongs =
				{
					// ���C��
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�`���[���b�v"), TieUpHanaHana(), ArtistKUmiko(), MakerAnimeA(), "���쑾�Y",
							FILE_NAME_TULIP, 58970.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�Ђ܂��"), TieUpHanaHana(), ArtistKYamako(), MakerAnimeA(), "���쑾�Y",
							FILE_NAME_SUNFLOWER, 58969.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�K�N", "�o��"), TieUpHanaHana(), ArtistKTaniko(), MakerAnimeA(), "���쑾�Y",
							FILE_NAME_ROSE, 58969.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�|�C���Z�`�A"), TieUpHanaHana(), ArtistKUmiko(), MakerAnimeA(), "�����Y",
							FILE_NAME_POINSETTIA, 58960.0, 118 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�g�C�v�[�h��"), TieUpInugaIppai(), ArtistKSorako(), MakerGameB(), "����O�Y",
							FILE_NAME_TOY_POODLE, 58965.0, 95 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�`����"), TieUpInugaIppai(), ArtistKHanako(), MakerGameB(), "����O�Y",
							FILE_NAME_CHIHUAHUA, 58976.0, 205 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�Č�", "�V�o�C�k"), TieUpInugaIppai(), ArtistKYamako(), MakerGameB(), "����l�Y",
							FILE_NAME_SHIBA, 58970.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�|�����j�A��"), TieUpInugaIppai(), ArtistKKawako(), MakerGameB(), "����l�Y",
							FILE_NAME_POMERANIAN, 58980.0, 119 * 1024 * 1024),

					// AddSong
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�x�j�E�`��"), TieUpHanaHana(), ArtistKUmiko(), MakerGameB(), "����ܘY",
							FILE_NAME_ANTHURIUM, 58950.0, 103 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�A����"), TieUpHanaHana(), ArtistKYamako(), MakerGameB(), "����ܘY",
							FILE_NAME_IRIS, 58945.0, 110 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("��", "�e��"), new NameAndRuby(String.Empty), ArtistSBouzu(), new NameAndRuby(String.Empty), "����ܘY",
							FILE_NAME_TEMPLE, 58944.0, 102 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�����R��"), TieUpKaden(), new NameAndRuby(String.Empty), MakerAnimeC(), "����ܘY",
							FILE_NAME_REMOTE, 58943.0, 104 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�Ԃƕ@", "�͂ȂƂ͂�"), new NameAndRuby(String.Empty), new NameAndRuby(String.Empty), new NameAndRuby(String.Empty), "����ܘY",
							FILE_NAME_HANA, 58942.0, 101 * 1024 * 1024),

					// AddSong2
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�t�����`�u���h�b�O"), TieUpInugaTakusan(), ArtistODasuko(), MakerTakusanA(), "���쑾�Y",
							FILE_NAME_FRENCH_BULLDOG, 58930.0, 115 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�V�[�Y�["), TieUpInugaTakusan(), ArtistODasuo(), MakerTakusanA(), "�������Y",
							FILE_NAME_SHIH_TZU, 58931.0, 118 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("����", "�}���V�o"), TieUpInugaTakusan(), ArtistNDasuo(), MakerTakusanA(), "����Y",
							FILE_NAME_MAMESHIBA, 58971.0, 92 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("���[�N�V���[�e���A"), TieUpInugaTakusan(), ArtistNDasuko(), MakerTakusanA(), "���O�Y",
							FILE_NAME_YORKSHIRE_TERRIER, 58931.0, 120 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�R�[�M�["), TieUpInugaTakusan(), ArtistODasuko(), MakerTakusanA(), "���O�Y",
							FILE_NAME_CORGI, 58932.0, 121 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("�S�[���f�����g���o�["), TieUpInugaTakusan(), ArtistODasuo(), MakerTakusanA(), "���O�Y",
							FILE_NAME_GOLDEN_RETRIEVER, 58933.0, 151 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("���q��", "�J�L�c�o�^"), TieUpHanagaTakusan(), ArtistODasuo(), MakerTakusanA(), "�������Y",
							FILE_NAME_IRIS2, 58934.0, 123 * 1024 * 1024),
					new AvailableSong(idPrefix + (idSuffix++).ToString(), new NameAndRuby("������", "�L���M���\�E"), TieUpHanagaTakusan(), ArtistNDasuo(), MakerTakusanC(), "����Y",
							FILE_NAME_COMMON_SNAPDRAGON, 58971.0, 101 * 1024 * 1024),
				};
				availableSongContext.AvailableSongs.AddRange(availableSongs);
				availableSongContext.SaveChanges();
			}

			// �\��\�ȋȂ̃T���l�C��
			using ThumbnailContext thumbnailContext = new();
			thumbnailContext.Database.EnsureCreated();
			if (thumbnailContext.Thumbnails == null)
			{
				throw new Exception();
			}
			if (thumbnailContext.Thumbnails.Count() == 0)
			{
				// �T���l�C���̃T���v���f�[�^�쐬
				// ���ۂ̉^�p���̓I���f�}���h�ŃT���l�C���f�[�^���쐬���邱�Ƃ��z�肳��邪�A�f���Ȃ̂Ŏ��O�ɍ쐬���Ă��܂�
				Thumbnail?[] thumbnails =
				{
					CreateMovieThumbnail(FILE_NAME_TULIP, "Tulip.png"),
					CreateMovieThumbnail(FILE_NAME_SUNFLOWER, "Sunflower.png"),
					CreateMovieThumbnail(FILE_NAME_ROSE, "Rose.png"),
					CreateMovieThumbnail(FILE_NAME_POINSETTIA, "Poinsettia.png"),
					CreateMovieThumbnail(FILE_NAME_TOY_POODLE, "ToyPoodle.png"),
					CreateMovieThumbnail(FILE_NAME_CHIHUAHUA, "Chihuahua.png"),
					CreateMovieThumbnail(FILE_NAME_SHIBA, "Shiba.png"),
					CreateMovieThumbnail(FILE_NAME_POMERANIAN, "Pomeranian.png"),
					CreateMovieThumbnail(FILE_NAME_ANTHURIUM, "Anthurium.png"),
					CreateMovieThumbnail(FILE_NAME_IRIS, "Iris.png"),
					CreateMovieThumbnail(FILE_NAME_TEMPLE, "Temple.png"),
					CreateMovieThumbnail(FILE_NAME_REMOTE, "Remote.png"),
					CreateMovieThumbnail(FILE_NAME_SHIH_TZU, "ShihTzu.png"),
					CreateMovieThumbnail(FILE_NAME_YORKSHIRE_TERRIER, "YorkshireTerrier.png"),
					CreateMovieThumbnail(FILE_NAME_CORGI, "Corgi.png"),
					CreateMovieThumbnail(FILE_NAME_GOLDEN_RETRIEVER, "GoldenRetriever.png"),
				};
				thumbnailContext.Thumbnails.AddRange(thumbnails.Where(x => x != null)!);
				thumbnailContext.SaveChanges();
			}

			// �\��ꗗ
			using RequestSongContext requestSongContext = new();
			requestSongContext.Database.EnsureCreated();

			// �o�^���[�U�[
			using RegisteredUserContext registeredUserContext = new();
			registeredUserContext.Database.EnsureCreated();
		}

		// --------------------------------------------------------------------
		// ����T���l�C������
		// --------------------------------------------------------------------
		private Thumbnail? CreateMovieThumbnail(String songPath, String imageFileName)
		{
			return CreateThumbnail(songPath, ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + imageFileName, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX);
		}

		// --------------------------------------------------------------------
		// �T���l�C������
		// --------------------------------------------------------------------
		private Thumbnail? CreateThumbnail(String moviePath, String imageFileName, Int32 maxWidth, Int32 maxHeight)
		{
			try
			{
				using FileStream sourceStream = new FileStream(imageFileName, FileMode.Open);
				return new Thumbnail
				{
					Path = moviePath,
					Bitmap = ServerCommon.CreateThumbnail(sourceStream, ServerConstants.MIME_TYPE_PNG, maxWidth, maxHeight, false),
					Mime = ServerConstants.MIME_TYPE_PNG,
					LastModified = YbdCommon.DateTimeToModifiedJulianDate(ServerCommon.LastModified(imageFileName)),
				};
			}
			catch (Exception excep)
			{
				Debug.WriteLine("�T���l�C���쐬�G���[�F\n" + excep.Message);
				Debug.WriteLine("�@�X�^�b�N�g���[�X�F\n" + excep.StackTrace);
				return null;
			}
		}

		// --------------------------------------------------------------------
		// ���[�U�[�摜�T���l�C������
		// --------------------------------------------------------------------
		private Thumbnail? CreateUserThumbnail(String imageFileName)
		{
			return CreateThumbnail(String.Empty, ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + imageFileName, YbdConstants.USER_THUMBNAIL_WIDTH_MAX, YbdConstants.USER_THUMBNAIL_HEIGHT_MAX);
		}

		// --------------------------------------------------------------------
		// �A�j���X�^�W�I A
		// --------------------------------------------------------------------
		private NameAndRuby MakerAnimeA()
		{
			return new NameAndRuby("�A�j���X�^�W�IA", "�A�j���X�^�W�I�G�[");
		}

		// --------------------------------------------------------------------
		// �A�j���X�^�W�I C
		// --------------------------------------------------------------------
		private NameAndRuby MakerAnimeC()
		{
			return new NameAndRuby("�A�j���X�^�W�IC", "�A�j���X�^�W�I�V�[");
		}

		// --------------------------------------------------------------------
		// �Q�[���X�^�W�I B
		// --------------------------------------------------------------------
		private NameAndRuby MakerGameB()
		{
			return new NameAndRuby("�Q�[���X�^�W�IB", "�Q�[���X�^�W�I�r�[");
		}

		// --------------------------------------------------------------------
		// ��������X�^�W�I A
		// --------------------------------------------------------------------
		private NameAndRuby MakerTakusanA()
		{
			return new NameAndRuby("��������X�^�W�IA", "�^�N�T���X�^�W�I�G�[");
		}

		// --------------------------------------------------------------------
		// ��������Q�[�� C
		// --------------------------------------------------------------------
		private NameAndRuby MakerTakusanC()
		{
			return new NameAndRuby("��������Q�[��C", "��������Q�[���V�[");
		}

		// --------------------------------------------------------------------
		// �Ԃ���������
		// --------------------------------------------------------------------
		private NameAndRuby TieUpHanagaTakusan()
		{
			return new NameAndRuby("�Ԃ���������", "�n�i�K�^�N�T��");
		}

		// --------------------------------------------------------------------
		// �ԉԉԉ�
		// --------------------------------------------------------------------
		private NameAndRuby TieUpHanaHana()
		{
			return new NameAndRuby("�ԉԉԉ�", "�n�i�n�i�n�i�n�i");
		}

		// --------------------------------------------------------------------
		// ���������ς�
		// --------------------------------------------------------------------
		private NameAndRuby TieUpInugaIppai()
		{
			return new NameAndRuby("���������ς�", "�C�k�K�C�b�p�C");
		}

		// --------------------------------------------------------------------
		// ������������
		// --------------------------------------------------------------------
		private NameAndRuby TieUpInugaTakusan()
		{
			return new NameAndRuby("������������", "�C�k�K�^�N�T��");
		}

		// --------------------------------------------------------------------
		// �Ɠd
		// --------------------------------------------------------------------
		private NameAndRuby TieUpKaden()
		{
			return new NameAndRuby("�Ɠd", "�J�f��");
		}

#if DEBUG
		// ====================================================================
		// �f�o�b�O��p
		// ====================================================================

		// --------------------------------------------------------------------
		// �T���l�C�������e�X�g
		// --------------------------------------------------------------------
		private void TestCreateThumbnail()
		{
			// �����E�k������
			using FileStream y1 = new FileStream(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + "Test_430x177.png", FileMode.Open);
			File.WriteAllBytes("TestGen_430x177_square.png", ServerCommon.CreateThumbnail(y1, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, true));
			File.WriteAllBytes("TestGen_430x177_rect.png", ServerCommon.CreateThumbnail(y1, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, false));

			// �����E�k���Ȃ�
			using FileStream y0 = new FileStream(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + "Test_150x62.png", FileMode.Open);
			File.WriteAllBytes("TestGen_150x62_square.png", ServerCommon.CreateThumbnail(y0, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, true));
			File.WriteAllBytes("TestGen_150x62_rect.png", ServerCommon.CreateThumbnail(y0, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, false));

			// �c���E�k������
			using FileStream t1 = new FileStream(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + "Test_250x468.png", FileMode.Open);
			File.WriteAllBytes("TestGen_250x468_square.png", ServerCommon.CreateThumbnail(t1, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, true));
			File.WriteAllBytes("TestGen_250x468_rect.png", ServerCommon.CreateThumbnail(t1, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, false));

			// �c���E�k���Ȃ�
			using FileStream t0 = new FileStream(ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + "Test_43x80.png", FileMode.Open);
			File.WriteAllBytes("TestGen_43x80_square.png", ServerCommon.CreateThumbnail(t0, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, true));
			File.WriteAllBytes("TestGen_43x80_rect.png", ServerCommon.CreateThumbnail(t0, ServerConstants.MIME_TYPE_PNG, MOVIE_THUMB_WIDTH_MAX, MOVIE_THUMB_HEIGHT_MAX, false));
		}
#endif
	}
}
