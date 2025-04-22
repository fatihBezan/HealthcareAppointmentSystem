using FluentValidation;
using FluentValidation.AspNetCore;
using Healthcare.API.Middleware;
using Healthcare.Application.DTOs;
using Healthcare.Application.Interfaces;
using Healthcare.Application.Services;
using Healthcare.Application.Validators;
using Healthcare.Core.Security;
using Healthcare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("JwtSettings:Key").Value ?? "DefaultSecretKeyWithAtLeast32Characters!")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Register services
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Add validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDoctorValidator>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Healthcare API", Version = "v1" });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandlingMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Create admin role and user if they don't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    
    // Ensure database is created
    dbContext.Database.EnsureCreated();
    
    // Check if admin role exists
    var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole == null)
    {
        // Create admin role
        adminRole = new Healthcare.Domain.Entities.Role
        {
            Name = "Admin",
            Description = "Administrator"
        };
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();
    }
    
    // Check if admin user exists
    var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "admin");
    if (adminUser == null)
    {
        // Create admin user
        passwordHasher.CreatePasswordHash("Admin123!", out byte[] passwordHash, out byte[] passwordSalt);
        adminUser = new Healthcare.Domain.Entities.User
        {
            Username = "admin",
            Email = "admin@healthcare.com",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };
        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();
        
        // Link admin user to admin role
        var userRole = new Healthcare.Domain.Entities.UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        };
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
