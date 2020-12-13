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
					// �T���v���f�[�^�쐬
					AvailableSong[] availableSongs = new AvailableSong[]
					{
						new AvailableSong { Path = @"D:\Song\�`���[���b�v.mp4", SongName = "�`���[���b�v", TieUpName = "�ԉԉԉ�" },
						new AvailableSong { Path = @"D:\Song\�Ђ܂��.mp4", SongName = "�Ђ܂��", TieUpName = "�ԉԉԉ�" },
						new AvailableSong { Path = @"D:\Song\�K�N.mp4", SongName = "�K�N", TieUpName = "�ԉԉԉ�" },
						new AvailableSong { Path = @"D:\Song\�|�C���Z�`�A.mp4", SongName = "�|�C���Z�`�A", TieUpName = "�ԉԉԉ�" },
						new AvailableSong { Path = @"D:\Song\�g�C�v�[�h��.mp4", SongName = "�g�C�v�[�h��", TieUpName = "���������ς�" },
						new AvailableSong { Path = @"D:\Song\�`����.mp4", SongName = "�`����", TieUpName = "���������ς�" },
						new AvailableSong { Path = @"D:\Song\�Č�.mp4", SongName = "�Č�", TieUpName = "���������ς�" },
						new AvailableSong { Path = @"D:\Song\�|�����j�A��.mp4", SongName = "�|�����j�A��", TieUpName = "���������ς�" },
						new AvailableSong { Path = @"E:\AddSong\Beni.mp4", SongName = "�x�j�E�`��", TieUpName = "�ԉԉԉ�" },
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
