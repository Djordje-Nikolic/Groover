using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
	public static class AutoMigrator
	{
		public static void ApplyMigrations(string connectionString)
		{
			var contextFactory = new GrooverContextFactory();
			using (var context = contextFactory.CreateDbContext(connectionString))
			{
				context.Database.Migrate();
			}
		}
	}
}
