using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ������������ ���������
builder.Logging.ClearProviders(); // �������� ��������� ���������� ����
builder.Logging.AddConsole();     // ������ ��������� � �������
builder.Logging.AddDebug();       // ������ ��������� ��� �������

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ��� ����������
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ������������ ���������� �� ���� ����� VotingDbContext
builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("VotingDbContext") ??
        throw new InvalidOperationException("Connection string 'VotingDbContext' not found."),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// ������������ ���������� �� ���� ����� UserDbContext
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("UserDbContext") ??
        throw new InvalidOperationException("Connection string 'UserDbContext' not found."),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// ��������� Identity � ��������� �����
builder.Services.AddIdentity<Account, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>() // ������ �������� �����
.AddEntityFrameworkStores<UserDbContext>() // ������������� UserDbContext ��� ���������� ������������ �� �����
.AddDefaultTokenProviders();

builder.Services.AddAuthorization();  // ������ �������� ������ �����������

// ��� ��� ������ ��������� ���������� � ���������������

var app = builder.Build();

// ����������� ������������ �������
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider serviceProvider = scope.ServiceProvider;

    // ��������� ��������� ���� ����� LibraryDbContext � ������������ EnsureCreated ��� ��������� ���� �����, ���� �� ����
    var votingDbContext = serviceProvider.GetRequiredService<VotingDbContext>();
    votingDbContext.Database.EnsureCreated();

    // ������������ ������� ���� EnsureCreated
    await votingDbContext.Database.MigrateAsync();

    // ��������� ��������� ���� ����� UserDbContext � ������������ EnsureCreated ��� ��������� ���� �����, ���� �� ����
    var userContext = serviceProvider.GetRequiredService<UserDbContext>();
    userContext.Database.EnsureCreated();

    // ������������ ������� ���� EnsureCreated
    await userContext.Database.MigrateAsync();

    // ��������� ���, ���� ���� �� �� �������
    await CreateRoles(serviceProvider);

    // ����������� ���������� ����� ��� ��������
    await SeedData.InitializeAsync(serviceProvider, app.Environment);
}

// ����� ��� ��������� �����
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // ��������� ��� � ������������� �������� �� ����� Roles
    // ������������� �������� ��� ��������� ��� ���� ����� Roles
    var roleNames = typeof(Roles)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly) // Գ������� ����� ���������
                    .Select(f => f.GetValue(null)?.ToString()) // �������� �������� ����
                    .ToArray(); // ������������� � �����

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // ���� ���� �� ����, ��������� ��
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// ������������ HTTP-������ � �������������
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ����������� �������������� �� �����������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
