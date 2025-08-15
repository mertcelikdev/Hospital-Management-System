using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
// Service registrations - Sadece Service katmanı
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<INurseTaskService, NurseTaskService>();
builder.Services.AddScoped<IPatientNoteService, PatientNoteService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// DataSeeder kaldırıldı (artık başlangıç seed verisi oluşturulmuyor)

// Session kaldırıldı (JWT kullanılıyor). Eğer geçici UI state gerekirse tekrar eklenebilir.

// --- JWT Authentication ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? jwtSection["Secret"] ?? "dev-fallback-key-123456";
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
        ValidIssuer = jwtIssuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    // Cookie içinden token çek (Authorization header yoksa)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                if (context.Request.Cookies.TryGetValue("HMS.AuthToken", out var token))
                {
                    context.Token = token;
                }
            }
            return Task.CompletedTask;
        }
    };
});
// --- JWT Authentication Sonu ---
// --- MongoDB Yapılandırması Sonu ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Sadece index kontrolü (seed yok)
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var userCollection = database.GetCollection<HospitalManagementSystem.Models.User>("Users");

    try
    {
        var existingCursor = await userCollection.Indexes.ListAsync();
        var existing = await existingCursor.ToListAsync();
        var existingNames = existing.Select(d => d.GetValue("name", BsonValue.Create(string.Empty)).AsString).ToHashSet();

        async Task EnsureIndexAsync(string name, IndexKeysDefinition<HospitalManagementSystem.Models.User> keys, CreateIndexOptions options)
        {
            if (existingNames.Contains(name)) return;
            try
            {
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(keys, options));
                Console.WriteLine($"Index created: {name}");
            }
            catch (MongoCommandException mce) when (mce.Message.Contains("existing index", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Index skipped (exists): {name}");
            }
        }

        await EnsureIndexAsync("TcNo_Index", Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.TcNo), new CreateIndexOptions { Unique = true, Name = "TcNo_Index" });
        await EnsureIndexAsync("Email_Index", Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true, Name = "Email_Index" });
        Console.WriteLine("MongoDB index kontrolü tamamlandı.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Index kontrolü hatası: {ex.Message}");
    }

    // Departman seed (eksik olan standart departmanları ekle)
    try
    {
        await HospitalManagementSystem.Data.DepartmentSeeder.EnsureSeededAsync(database, msg => Console.WriteLine(msg));
    }
    catch(Exception depEx)
    {
        Console.WriteLine($"Department seed hatası: {depEx.Message}");
    }

    // İlaç seed (120 hedef)
    try
    {
        await HospitalManagementSystem.Data.MedicineSeeder.EnsureSeededAsync(database, 120, msg => Console.WriteLine(msg));
    }
    catch(Exception medEx)
    {
        Console.WriteLine($"Medicine seed hatası: {medEx.Message}");
    }

    // Doktor ve Hasta seed (39 departman * 6 doktor, 60 hasta)
    try
    {
        await HospitalManagementSystem.Data.DoctorAndPatientSeeder.EnsureDoctorsAndPatientsAsync(database, 6, 60, msg => Console.WriteLine(msg));
    }
    catch(Exception docPatEx)
    {
        Console.WriteLine($"Doctor/Patient seed hatası: {docPatEx.Message}");
    }
}

app.Run();

