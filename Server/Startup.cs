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
				Debug.WriteLine("�X�^�[�g�A�b�v�G���[�F\n" + excep.Message);
				Debug.WriteLine("�@�X�^�b�N�g���[�X�F\n" + excep.StackTrace);
			}
		}

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
				// �\��\�ȋȂ̃T���v���f�[�^�쐬
				AvailableSong[] availableSongs =
				{
						new AvailableSong { Path = FILE_NAME_TULIP, SongName = "�`���[���b�v", TieUpName = "�ԉԉԉ�", ArtistName="�̏��C�q", Maker="�A�j���X�^�W�IA", Worker="���쑾�Y",
								LastModified = 58970.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_SUNFLOWER, SongName = "�Ђ܂��", TieUpName = "�ԉԉԉ�", ArtistName="�̏��R�q", Maker="�A�j���X�^�W�IA", Worker="���쑾�Y",
								LastModified = 58969.0, FileSize = 120 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_ROSE, SongName = "�K�N", TieUpName = "�ԉԉԉ�", ArtistName="�̏��J�q", Maker="�A�j���X�^�W�IA", Worker="�����Y",
								LastModified = 58971.0, FileSize = 115 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_POINSETTIA, SongName = "�|�C���Z�`�A", TieUpName = "�ԉԉԉ�", ArtistName="�̏��C�q", Maker="�A�j���X�^�W�IA", Worker="�����Y",
								LastModified = 58960.0, FileSize = 118 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_TOY_POODLE, SongName = "�g�C�v�[�h��", TieUpName = "���������ς�", ArtistName="�̏���q", Maker="�Q�[���X�^�W�IB", Worker="����O�Y",
								LastModified = 58965.0, FileSize = 95 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_CHIHUAHUA, SongName = "�`����", TieUpName = "���������ς�", ArtistName="�̏��Ԏq", Maker="�Q�[���X�^�W�IB", Worker="����O�Y",
								LastModified = 58976.0, FileSize = 205 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_SHIBA, SongName = "�Č�", TieUpName = "���������ς�", ArtistName="�̏��R�q", Maker="�Q�[���X�^�W�IB", Worker="����l�Y",
								LastModified = 58970.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_POMERANIAN, SongName = "�|�����j�A��", TieUpName = "���������ς�", ArtistName="�̏���q", Maker="�Q�[���X�^�W�IB", Worker="����l�Y",
								LastModified = 58980.0, FileSize = 119 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_ANTHURIUM, SongName = "�x�j�E�`��", TieUpName = "�ԉԉԉ�", ArtistName="�̏��C�q", Maker="�A�j���X�^�W�IA", Worker="����ܘY",
								LastModified = 58950.0, FileSize = 103 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_IRIS, SongName = "�A����", TieUpName = "�ԉԉԉ�", ArtistName="�̏��R�q", Maker="�A�j���X�^�W�IA", Worker="����ܘY",
								LastModified = 58945.0, FileSize = 110 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_TEMPLE, SongName = "��", TieUpName = String.Empty, ArtistName = "���a�V��", Maker = String.Empty, Worker="����ܘY",
								LastModified = 58944.0, FileSize = 102 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_REMOTE, SongName = "�����R��", TieUpName = "�Ɠd", ArtistName = String.Empty, Maker = "�A�j���X�^�W�IC", Worker="����ܘY",
								LastModified = 58943.0, FileSize = 104 * 1024 * 1024 },
						new AvailableSong { Path = FILE_NAME_HANA, SongName = "�Ԃƕ@", TieUpName = String.Empty, ArtistName = String.Empty, Maker = String.Empty, Worker="����ܘY",
								LastModified = 58942.0, FileSize = 101 * 1024 * 1024 },
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
				try
				{
					// �T���l�C���̃T���v���f�[�^�쐬
					// ���ۂ̉^�p���̓I���f�}���h�ŃT���l�C���f�[�^���쐬���邱�Ƃ��z�肳��邪�A�f���Ȃ̂Ŏ��O�ɍ쐬���Ă��܂�
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
					Debug.WriteLine("�T���l�C���쐬�G���[�F\n" + excep.Message);
					Debug.WriteLine("�@�X�^�b�N�g���[�X�F\n" + excep.StackTrace);
				}
			}

			// �\��ꗗ
			using RequestSongContext requestSongContext = new();
			requestSongContext.Database.EnsureCreated();
		}

		private Thumbnail CreateThumbnail(String songPath, String imageFileName)
		{
			using Image sourceImage = Image.FromFile("SampleDataImages\\" + imageFileName);

			// �T���l�C���T�C�Y
			Single scale = Math.Min((Single)SCALE_WIDTH / sourceImage.Width, (Single)SCALE_HEIGHT / (float)sourceImage.Height);
			Int32 scaledWidth = (Int32)(sourceImage.Width * scale);
			Int32 scaledHeight = (Int32)(sourceImage.Height * scale);

			// �T���l�C������
			using Bitmap scaledImage = new Bitmap(scaledWidth, scaledHeight);
			using Graphics graphics = Graphics.FromImage(scaledImage);
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(sourceImage, 0, 0, scaledWidth, scaledHeight);

			// �V���A����
			using MemoryStream stream = new();
			scaledImage.Save(stream, ImageFormat.Png);
			stream.Position = 0;

			// ���R�[�h����
			return new Thumbnail
			{
				Path = songPath,
				Bitmap = stream.ToArray(),
				Mime = "image/png",
			};
		}

		private const Int32 SCALE_WIDTH = 160;
		private const Int32 SCALE_HEIGHT = 90;

		private const String FILE_NAME_TULIP = @"D:\Song\�`���[���b�v.mp4";
		private const String FILE_NAME_SUNFLOWER = @"D:\Song\�Ђ܂��.mp4";
		private const String FILE_NAME_ROSE = @"D:\Song\�K�N.mp4";
		private const String FILE_NAME_POINSETTIA = @"D:\Song\�|�C���Z�`�A.mp4";
		private const String FILE_NAME_TOY_POODLE = @"D:\Song\�g�C�v�[�h��.mp4";
		private const String FILE_NAME_CHIHUAHUA = @"D:\Song\�`����.mp4";
		private const String FILE_NAME_SHIBA = @"D:\Song\�Č�.mp4";
		private const String FILE_NAME_POMERANIAN = @"D:\Song\�|�����j�A��.mp4";
		private const String FILE_NAME_ANTHURIUM = @"E:\AddSong\Beni.mp4";
		private const String FILE_NAME_IRIS = @"E:\AddSong\Ayame.mp4";
		private const String FILE_NAME_TEMPLE = @"E:\AddSong\Temple.mp4";
		private const String FILE_NAME_REMOTE = @"E:\AddSong\Remote.mp4";
		private const String FILE_NAME_HANA = @"E:\AddSong\Hana.mp4";
	}
}
