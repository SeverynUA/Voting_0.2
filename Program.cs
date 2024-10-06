using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Data.Entities;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ��� ����������
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // ������������ ��� ���������������� ���
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;  // ���������� ����� 䳿 ��� ��� ���������

    // ��������� ���, ���� ������� "�����'����� ����"
    options.ExpireTimeSpan = TimeSpan.FromDays(14); // �� ��������� ��� RememberMe

    // ��������� ��� ���� �������� ��������, ���� "�����'����� ����" �� �������
    options.Cookie.Expiration = null;  // ��� ������ �������� � ���������� ���� �������� ��������
});


builder.Services.AddAuthorization();

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

// ��������� Identity
builder.Services.AddIdentity<Account, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // ��������� ��� � ������������� �������� �� ����� Roles
    string[] roleNames = { Roles.Admin, Roles.Organizator, Roles.Voter };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // ���� �� ����, ��������� ����
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // ��������� ���
    await CreateRoles(services);

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider serviceProvider = scope.ServiceProvider;

    var VotingDbContext = serviceProvider.GetRequiredService<VotingDbContext>();
    VotingDbContext.Database.EnsureCreated();

    await VotingDbContext.Database.MigrateAsync();

    var userContext = serviceProvider.GetRequiredService<UserDbContext>();
    userContext.Database.EnsureCreated();

    await userContext.Database.MigrateAsync();

    // ����������� ���������� ����� ��� ��������
    await SeedData.InitializeAsync(serviceProvider, app.Environment);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
