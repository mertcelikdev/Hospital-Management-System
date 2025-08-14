using Microsoft.Extensions.Options;
using MongoDB.Driver;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Services;

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
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<MedicineService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<TreatmentService>();
builder.Services.AddScoped<NurseTaskService>();
builder.Services.AddScoped<PatientNoteService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<DataSeeder>();

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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session middleware'ini ekle

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Seed data and create indexes
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
    
    // MongoDB Indexes oluştur
    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var userCollection = database.GetCollection<HospitalManagementSystem.Models.User>("Users");
    
    // TC kimlik numarası için index
    var tcIndexKeys = Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.TcNo);
    var tcIndexOptions = new CreateIndexOptions { Unique = true, Name = "TcNo_Index" };
    await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(tcIndexKeys, tcIndexOptions));
    
    // Email için index (varsa)
    var emailIndexKeys = Builders<HospitalManagementSystem.Models.User>.IndexKeys.Ascending(u => u.Email);
    var emailIndexOptions = new CreateIndexOptions { Unique = true, Name = "Email_Index" };
    await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<HospitalManagementSystem.Models.User>(emailIndexKeys, emailIndexOptions));
    
    Console.WriteLine("MongoDB indexes created successfully!");
}

app.Run();

