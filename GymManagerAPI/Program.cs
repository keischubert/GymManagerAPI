
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using GymManagerAPI.Data.AutoMapperProfiles;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Repositories;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace GymManagerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddTransient<GenderService>();

            builder.Services.AddTransient<MemberService>();

            builder.Services.AddTransient<PlanService>();

            builder.Services.AddTransient<SubscriptionService>();

            builder.Services.AddTransient<IMemberRepository, MemberRepository>();

            builder.Services.AddTransient<IGenderRepository, GenderRepository>();

            builder.Services.AddTransient<IPlanRepository, PlanRepository>();

            builder.Services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();

            builder.Services.AddTransient<JwtService>();

            builder.Services.AddTransient<IUserService, UserService>();

            builder.Services.AddTransient<AuthService>();

            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.1",
                    Title = "GymManager API",
                    Description = "Un API desarrollado en ASP.NET Core para administrar un gimnasio."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,  // Validar el emisor
                        ValidateAudience = true, // Validar la audiencia
                        ValidateLifetime = true, // Verificar que el token no haya expirado
                        ValidateIssuerSigningKey = true, // Validar la clave de firma
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = "GymManagerAPI", // Emisor permitido
                        ValidAudience = "GymManagerClient", // Audiencia permitida
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))  // Clave secreta utilizada para firmar el token
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                // all roles
                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireRole("User", "Admin", "Developer"));

                // allow admin and developer and excludes user "role"
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin", "Developer"));

                // only developer
                options.AddPolicy("DeveloperPolicy", policy =>
                    policy.RequireRole("Developer"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
