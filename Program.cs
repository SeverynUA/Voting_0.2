using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voting_0._2.Data.Entities.Users;
using Voting_0._2.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Налаштування логування
builder.Logging.ClearProviders(); // Очищення попередніх провайдерів логів
builder.Logging.AddConsole();     // Додаємо логування в консоль
builder.Logging.AddDebug();       // Додаємо логування для відладки

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Час очікування
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Налаштування підключення до бази даних VotingDbContext
builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("VotingDbContext") ??
        throw new InvalidOperationException("Connection string 'VotingDbContext' not found."),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// Налаштування підключення до бази даних UserDbContext
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("UserDbContext") ??
        throw new InvalidOperationException("Connection string 'UserDbContext' not found."),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// Реєстрація Identity з підтримкою ролей
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
.AddRoles<IdentityRole>() // Додаємо підтримку ролей
.AddEntityFrameworkStores<UserDbContext>() // Використовуємо UserDbContext для збереження користувачів та ролей
.AddDefaultTokenProviders();

builder.Services.AddAuthorization();  // Додаємо необхідні служби авторизації

// Ось тут додаємо реєстрацію контролерів з представленнями

var app = builder.Build();

// Автоматичне застосування міграцій
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider serviceProvider = scope.ServiceProvider;

    // Створення контексту бази даних LibraryDbContext і застосування EnsureCreated для створення бази даних, якщо її немає
    var votingDbContext = serviceProvider.GetRequiredService<VotingDbContext>();
    votingDbContext.Database.EnsureCreated();

    // Застосування міграцій після EnsureCreated
    await votingDbContext.Database.MigrateAsync();

    // Створення контексту бази даних UserDbContext і застосування EnsureCreated для створення бази даних, якщо її немає
    var userContext = serviceProvider.GetRequiredService<UserDbContext>();
    userContext.Database.EnsureCreated();

    // Застосування міграцій після EnsureCreated
    await userContext.Database.MigrateAsync();

    // Створюємо ролі, якщо вони ще не існують
    await CreateRoles(serviceProvider);

    // Ініціалізація початкових даних для бібліотеки
    await SeedData.InitializeAsync(serviceProvider, app.Environment);
}

// Метод для створення ролей
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Створюємо ролі з використанням констант із класу Roles
    // Використовуємо рефлексію для отримання всіх полів класу Roles
    var roleNames = typeof(Roles)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly) // Фільтруємо тільки константи
                    .Select(f => f.GetValue(null)?.ToString()) // Отримуємо значення полів
                    .ToArray(); // Преобразовуємо в масив

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Якщо роль не існує, створюємо її
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// Налаштування HTTP-запитів і маршрутизації
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Налаштовуємо аутентифікацію та авторизацію
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
