using hChatAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace hChatAPI.Services {
	public class DataContext(DbContextOptions<DataContext> options) : DbContext(options) {
		public DbSet<User> Users { get; set; }

	}
}
