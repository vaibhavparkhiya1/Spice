using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spice.Models;


namespace Spice.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { }

        public DbSet<Category> Category { get;  set; }
        public DbSet<SubCategory>  SubCategory { get; set; }

        public DbSet<MenuItems> MenuItems { get; set; }

        public DbSet<Coupons> Coupons { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<OrderHeader> OrderHeader { get; set; }


        public DbSet<OrderDetails> OrderDetails { get; set; }




    }
}
