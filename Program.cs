using Microsoft.Extensions.Options;
using MongoDB.Driver;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Services.Implementations;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// JWT Config binding
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

// --- MongoDB Yapılandırması ---
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<MongoDbSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<MongoDbSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

// Repository registrations - Artık gerek yok, direkt Service kullanıyoruz
// Service registrations - Interface bazlı
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<INurseTaskService, NurseTaskService>();
builder.Services.AddScoped<IPatientNoteService, PatientNoteService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
// Auth & JWT
builder.Services.AddScoped<IAuthService, AuthService>();
// Data Seeder
builder.Services.AddScoped<DataSeeder>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});
// Seed mekanizmasi kaldirildi

// Session support for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// --- MongoDB Yapılandırması Sonu ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/RoleDispatch"); // Home kaldırıldı
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session (geçici - uzun vadede kaldırılabilir)

app.UseAuthentication();
app.UseAuthorization();

// Eski aliskanlikla /auth gelen talepleri /login'e yonlendir
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/auth", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/login");
        return;
    }
    await next();
});

// /login kisa yolu (Account/Login aksiyonuna gider)
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Login olmuş kullanıcı /Account/Login veya /login'e gelirse role dashboard'a gönder
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
        context.Request.Path.Equals("/login", StringComparison.OrdinalIgnoreCase))
    {
        var token = context.Session.GetString("AuthToken");
        var role = context.Session.GetString("UserRole");
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(role))
        {
            context.Response.Redirect("/Dashboard/RoleDispatch");
            return;
        }
    }
    await next();
});

// Sadece index olusturma (seed yok)
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    // Seed işlemi (idempotent)
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        Console.WriteLine("[Seed] Örnek veriler kontrol edildi/oluşturuldu.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed] Hata: {ex.Message}");
    }
    var userCollection = database.GetCollection<HospitalManagementSystem.Models.User>("Users");
    try
    {
        var tcIndexKeys = Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.TcNo);
        var tcIndexOptions = new CreateIndexOptions { Unique = true, Sparse = true, Name = "TcNo_Index" };
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(tcIndexKeys, tcIndexOptions));

        // Email index (önceden yoksa oluştur)
        try
        {
            var emailIdxKeys2 = Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.Email);
            var emailIdxOptions2 = new CreateIndexOptions { Unique = true, Sparse = true, Name = "Email_Index" };
            await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(emailIdxKeys2, emailIdxOptions2));
        }
        catch { /* index varsa yoksay */ }
        var emailIndexKeys = Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true, Sparse = true, Name = "Email_Index" };
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(emailIndexKeys, emailIndexOptions));

        // Basit admin seed (yoksa)
        var adminExists = await userCollection.Find(u => u.Email == "admin@hms.local").AnyAsync();
        if (!adminExists)
        {
            var adminDoc = new HospitalManagementSystem.Models.User
            {
                Id = null,
                FirstName = "Sistem",
                LastName = "Admin",
                Email = "admin@hms.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                TcNo = "99999999999"
            };
            await userCollection.InsertOneAsync(adminDoc);
            Console.WriteLine("[Seed] Admin kullanıcı oluşturuldu: admin@hms.local / Admin123!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Index / seed hatasi: {ex.Message}");
    }
}

app.Run();

