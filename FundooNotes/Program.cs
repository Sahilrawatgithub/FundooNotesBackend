    using Microsoft.EntityFrameworkCore;
    using RepositoryLayer.Context;
    using RepositoryLayer.Interface;
    using RepositoryLayer.Service;
    using BusinessLayer.Interface;
    using BusinessLayer.Service;
    using RepositoryLayer.Helper;
    using Microsoft.OpenApi.Models;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;
    using ConsumerLayer;
    using RepositoryLayer.Middleware;
    using NLog.Config;
    using NLog.Web;
    using StackExchange.Redis;
    using Yarp.ReverseProxy; 

    namespace FundooNotes
    { 
        public class Program
        {
            public static void Main(string[] args)
            {
                var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
                try
                {
                    var builder = WebApplication.CreateBuilder(args);
                    logger.Info("Application Starting...");

                    var redisConfig=builder.Configuration.GetSection("Redis:ConnectionString").Value;

                    // Remove default logging providers & use NLog
                    builder.Logging.ClearProviders();
                    builder.Logging.SetMinimumLevel(LogLevel.Information);
                    builder.Host.UseNLog(); // Add NLog to .NET Core logging

                    // Add services to the container.
                    builder.Services.AddDbContext<UserContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("conn")));

                    builder.Services.AddReverseProxy()
                    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

                builder.Services.AddScoped<IUserRL, UserImplRL>();
                    builder.Services.AddScoped<IUserBL, UserImplBL>();

                    builder.Services.AddScoped<INotesRL, NotesImplRL>();
                    builder.Services.AddScoped<INotesBL, NotesImplBL>();

                    builder.Services.AddScoped<ILabelRL, LabelImplRL>();
                    builder.Services.AddScoped<ILabelBL, LabelImplBL>();

                    builder.Services.AddScoped<ICollabRL, CollabImplRL>();
                    builder.Services.AddScoped<ICollabBL, CollabImpleBL>();

                    builder.Services.AddScoped<AuthService>();
                    builder.Services.AddScoped<PasswordHasherRL>();
                    builder.Services.AddScoped<GeneratesOtp>();
                    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
                builder.Services.AddScoped(provider =>
                {
                    var connection = provider.GetRequiredService<IConnectionMultiplexer>();
                    return connection.GetDatabase();
                });

                builder.Services.AddHostedService<RabbitMqConsumerService>();

                    var jwtKey = builder.Configuration["Jwt:Key"];
                    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
                    var jwtAudience = builder.Configuration["Jwt:Audience"];

                    builder.Services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtIssuer,
                            ValidAudience = jwtAudience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                    });


                    builder.Services.AddControllers();
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });

                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            In = ParameterLocation.Header,
                            Description = "Enter JWT as: {your token}",
                            Name = "Authorization",
                            Type = SecuritySchemeType.Http,
                            Scheme = "Bearer"
                        });

                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                           {
                           {
                               new OpenApiSecurityScheme
                               {
                                   Reference = new OpenApiReference
                                   {
                                       Type = ReferenceType.SecurityScheme,
                                       Id = JwtBearerDefaults.AuthenticationScheme
                                   }
                               },
                               Array.Empty<string>()
                           }
                           });

                        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                    });


                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll",
                        policy =>
                        {
                            policy.WithOrigins("http://localhost:3000")
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                        });
                });

                var app = builder.Build();
                app.MapReverseProxy();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                app.UseCors("AllowAll");


                app.UseHttpsRedirection();

                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.UseMiddleware<ExceptionMiddleware>();

                    app.MapControllers();
                    app.Run();
            }
                catch (Exception ex)
                {
                    logger.Error(ex, "Application stopped because of exception");
                throw;
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }

            }
        }
    }
