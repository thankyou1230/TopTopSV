using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TopTopServer.Models;

namespace TopTopServer
{
    public class TopTopDBContext: DbContext
    {
        // Thuộc tính products kiểu DbSet<Product> cho biết CSDL có bảng mà
        // thông tin về bảng dữ liệu biểu diễn bởi model Product
        public DbSet<User> Users { set; get; }
        public DbSet<Video> Videos { set; get; }
        public DbSet<Comment> Comments { set; get; }
        public DbSet<Like> Likes { set; get; }
        public DbSet<Follow> Follows { set; get; }
        public TopTopDBContext(DbContextOptions<TopTopDBContext> options) : base(options)
        {
            // Phương thức khởi tạo này chứa options để kết nối đến MS SQL Server
            // Thực hiện điều này khi Inject trong dịch vụ hệ thống
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follow>().HasKey(u => new
            {
                u.Follower,
                u.Following
            });
            modelBuilder.Entity<Like>().HasKey(u => new
            {
                u.User,
                u.Video
            });
            modelBuilder.Entity<Comment>().HasKey(u => new
            {
                u.User,
                u.Video,
                u.CommentTime
            });
            modelBuilder.Entity<Comment>().Property<string>("CommentContent").IsUnicode(true);
            modelBuilder.Entity<Video>().Property<string>("Title").IsUnicode(true);
        }
    }
}
