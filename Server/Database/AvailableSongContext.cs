using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Database
{
	public class AvailableSongContext : DbContext
	{
		public DbSet<AvailableSong>? AvailableSong { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			SqliteConnectionStringBuilder stringBuilder = new()
			{
				DataSource = "AvailableSong.sqlite3"
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}
	}
}
