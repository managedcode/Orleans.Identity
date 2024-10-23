using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Models;

public class TestUser : IdentityUser
{
    public TestUser()
    {
    }

    public TestUser(string login)
    {
        this.UserName = login;
    }
}

public class TestUserIdentityDbContext : IdentityDbContext<TestUser>
{
    public TestUserIdentityDbContext(DbContextOptions<TestUserIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Add custom configurations for TestUser
        builder.Entity<TestUser>().ToTable("TestUsers");
    }
}