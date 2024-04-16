using hChatAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace hChatAPI.Services {
	public class DataContext : DbContext {
		public DataContext(DbContextOptions<DataContext> options) : base(options) {
		}
		
		public DbSet<User> Users { get; set; }

	}
}
