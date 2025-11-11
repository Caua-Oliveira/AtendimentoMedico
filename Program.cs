using ClinicaBemEstar.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClinicaBemEstar.Data;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Identity; 

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
builder.Services.AddAuthorization();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

        // Seed Admin User
        if (!context.Users.Any(u => u.Email == "admin@email.com"))
        {
            var adminUser = new User
            {
                Name = "Administrador",
                Email = "admin@email.com",
                Role = "Admin"
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin");
            context.Users.Add(adminUser);
        }

        // Seed Specialties and Doctors
        if (!context.Specialties.Any())
        {
            var hematologia = new Specialty { Name = "Hematologia", ImageUrl = "https://media.discordapp.net/attachments/784525631420629002/1437435741163622411/OIP.png?ex=69148d5a&is=69133bda&hm=73e7c05d317d79e71d5a6e1743aba2db802db5673d9eb22c25cdd632b88bcd63&=&format=webp&quality=lossless" };
            var urologia = new Specialty { Name = "Urologia", ImageUrl = "https://media.discordapp.net/attachments/784525631420629002/1437435979651616922/OIP.png?ex=69148d93&is=69133c13&hm=570a715c4975cd5bfdfea234d77c3fb60cb79f2b00f224c631585fd0e44a3be5&=&format=webp&quality=lossless" };
            var dermatologia = new Specialty { Name = "Dermatologia", ImageUrl = "https://media.discordapp.net/attachments/784525631420629002/1437436213098446878/jfxX5HJ1yXOoMtSdlb1pup2wNKYUamWXPT76VJaquYkH90lxiAfzJ1nQLG50wNsQAAAA.png?ex=69148dcb&is=69133c4b&hm=22aa16d0d09dff4f6c95c7e29ad29a4341eb1ee297cc9054512eb8d93326d361&=&format=webp&quality=lossless" };
            var ortopedia = new Specialty { Name = "Ortopedia", ImageUrl = "https://media.discordapp.net/attachments/784525631420629002/1437436503222517905/OIP.png?ex=69148e10&is=69133c90&hm=e4d4a05ac9d9502255235ffde4125e74a7a9bccc72cd171063b0bf9e6f4794ea&=&format=webp&quality=lossless" };
            var radiologia = new Specialty { Name = "Radiologia", ImageUrl = "https://cdn.discordapp.com/attachments/784525631420629002/1437437714743037992/OIP.png?ex=69148f31&is=69133db1&hm=b742008f6c1ae4064411df9b30377cb3af22ba908f53f01fadb2feaf12c32402&" };

            context.Specialties.AddRange(hematologia, urologia, dermatologia, ortopedia, radiologia);

            if (!context.Doctors.Any())
            {
                var doctors = new List<Doctor>
                {
                    // Hematologia
                    new Doctor { Name = "Dr. João da Silva", Crm = "CRM/SP 123456", Specialty = hematologia },
                    new Doctor { Name = "Dra. Maria da Costa", Crm = "CRM/SP 123457", Specialty = hematologia },
                    // Urologia
                    new Doctor { Name = "Dr. Carlos Alberto", Crm = "CRM/RJ 234567", Specialty = urologia },
                    new Doctor { Name = "Dra. Ana Pereira", Crm = "CRM/RJ 234568", Specialty = urologia },
                    // Dermatologia
                    new Doctor { Name = "Dr. Pedro Mendonça", Crm = "CRM/MG 345678", Specialty = dermatologia },
                    new Doctor { Name = "Dra. Beatriz Lima", Crm = "CRM/MG 345679", Specialty = dermatologia },
                    // Ortopedia
                    new Doctor { Name = "Dr. Fernando Martins", Crm = "CRM/BA 456789", Specialty = ortopedia },
                    new Doctor { Name = "Dra. Sofia Ferreira", Crm = "CRM/BA 456790", Specialty = ortopedia },
                    // Radiologia
                    new Doctor { Name = "Dr. Ricardo Azevedo", Crm = "CRM/RS 567890", Specialty = radiologia },
                    new Doctor { Name = "Dra. Helena Souza", Crm = "CRM/RS 567891", Specialty = radiologia }
                };
                context.Doctors.AddRange(doctors);
            }
        }

        context.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB or seeding data.");
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();