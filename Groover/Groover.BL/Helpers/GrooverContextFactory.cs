using Groover.DB.MySqlDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
	public class GrooverContextFactory : IDesignTimeDbContextFactory<GrooverDbContext>
	{
		public GrooverDbContext CreateDbContext(string[] args)
		{
			var jsonBytes = File.ReadAllBytes(@"..\..\Groover.API\appsettings.Development.json");
			var jsonDoc = JsonDocument.Parse(jsonBytes);
			var root = jsonDoc.RootElement;

			string conString = root.GetProperty("ConnectionStrings").GetProperty("grooverMySql").GetString();

			return CreateDbContext(conString);
		}

		public GrooverDbContext CreateDbContext(string connectionString)
		{
			var optionsBuilder = new DbContextOptionsBuilder<GrooverDbContext>();
			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

			return new GrooverDbContext(optionsBuilder.Options);
		}
	}
}
