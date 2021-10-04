using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlagsCo.MQ;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlagsCo.FeatureInsights;
using FeatureFlagsCo.FeatureInsights.ElasticSearch;
using FeatureFlagsCo.MQ.Export;

namespace FeatureFlags.AdminWebAPIs
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment appEnv)
        {
            Configuration = configuration;
            CurrentEnvironment = appEnv;
        }

        public IConfiguration Configuration { get; }
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin", p =>
                {
                    p.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.AddControllers();

            // For Entity Framework
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnStr")));

            #region Identity
            // For Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // Adding Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });
            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;
            });
            #endregion

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ToDo API",
                    Description = "A simple example ASP.NET Core Web API",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Shayne Boyer",
                        Email = string.Empty,
                        Url = new Uri("https://twitter.com/spboyer"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                        }
                });
            });

            services.AddScoped<IGenericRepository, GenericRepository<ApplicationDbContext>>();
            services.AddTransient<IEnvironmentUserPropertyService, EnvironmentUserPropertyService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IAccountUserService, AccountUserService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IProjectUserService, ProjectUserService>();
            services.AddTransient<IEnvironmentService, EnvironmentService>();
            services.AddTransient<IUserInvitationService, UserInvitationService>();

            services.AddTransient<IFeatureFlagsService, FeatureFlagsService>();
            services.AddScoped<ILaunchDarklyService, LaunchDarklyService>();
            services.AddScoped<IVariationService, VariationService>();
            services.AddTransient<IAppInsightsService, AppInsightsService>();

            services.AddScoped<IDataSyncService, DataSyncService>();

            services.Configure<JWTSettings>(options => Configuration.GetSection("JWT").Bind(options));
            services.Configure<MySettings>(options => Configuration.GetSection("MySettings").Bind(options));
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));
            
            services.AddSingleton<IJwtUtilsService, JwtUtilsService>();
            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
            services.AddSingleton<MongoDbFeatureFlagService>();
            services.AddSingleton<MongoDbEnvironmentUserService>();
            services.AddSingleton<MongoDbEnvironmentUserPropertyService>();
            services.AddSingleton<MongoDbFeatureTriggerService>();
            //services.AddSingleton<MongoDbFeatureFlagCommitService>();
            services.AddSingleton<INoSqlService, MongoDbService>();
            services.AddSingleton<IExperimentsService, ExperimentsService>();
            services.AddSingleton<MongoDbExperimentService>();

            var hostingType = this.Configuration.GetSection("MySettings").GetSection("HostingType").Value;
            var cacheType = this.Configuration.GetSection("MySettings").GetSection("CacheType").Value;

            if (cacheType == CacheTypeEnum.Memory.ToString())
            {
                services.AddDistributedMemoryCache();
            }
            else if (cacheType == CacheTypeEnum.Redis.ToString())
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = this.Configuration.GetConnectionString("RedisServerUrl");
                    options.InstanceName = "feature-flags-users";
                });
            }

            #region Telemetry/Insights
            if (hostingType == HostingTypeEnum.Azure.ToString())
            {
                Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
                        = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
                aiOptions.InstrumentationKey = this.Configuration.GetSection("ApplicationInsights").GetSection("InstrumentationKey").Value;
                aiOptions.ConnectionString = this.Configuration.GetSection("ApplicationInsights").GetSection("ConnectionString").Value;
                aiOptions.EnableAdaptiveSampling = false;
                aiOptions.EnableDependencyTrackingTelemetryModule = false;
                aiOptions.EnableAppServicesHeartbeatTelemetryModule = false;
                aiOptions.EnablePerformanceCounterCollectionModule = false;
                aiOptions.EnableEventCounterCollectionModule = false;
                aiOptions.EnableRequestTrackingTelemetryModule = false;
                services.AddApplicationInsightsTelemetry(aiOptions);
            }
            if (hostingType == HostingTypeEnum.Docker.ToString())
            {
                //var otlpEndpoint = this.Configuration.GetSection("OpenTelemetry").GetSection("Endpoint").Value;
                //if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                //{
                //    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                //    var serviceName = this.Configuration.GetSection("OpenTelemetry").GetSection("ServiceName").Value;
                //    services.AddOpenTelemetryTracing((builder) => builder
                //        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                //        .AddAspNetCoreInstrumentation()
                //        .AddHttpClientInstrumentation()
                //        .AddOtlpExporter(otlpOptions =>
                //        {
                //            otlpOptions.Endpoint = new Uri(otlpEndpoint);
                //    }));
                //}
            }

            var StartSleepTimeStr = this.Configuration.GetSection("MySettings").GetSection("StartSleepTime").Value;
            Thread.Sleep(Convert.ToInt32(StartSleepTimeStr) * 1000);

            var insightsRabbitMqUrl = this.Configuration.GetSection("MySettings").GetSection("InsightsRabbitMqUrl").Value;
            services.AddSingleton<IExperimentMqService, ExperimentstRabbitMqService>();
            services.AddSingleton<IInsighstMqService, InsighstRabbitMqService>();
            services.AddSingleton<IFeatureFlagMqService, FeatureFlagMqService>();
            services.AddSingleton<IExperimentStartEndMqService, ExperimentStartEndMqService>();
            services.AddSingleton<IExperimentResultService, ExperimentResultService>();
            services.AddSingleton<IAuditLogMqService, AuditLogMqService>();
            

            var esHost = this.Configuration.GetSection("MySettings").GetSection("ElasticSearchHost").Value;
            services.AddSingleton<IExportExperimentsDataToElasticSearchService>(new ExportExperimentsDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            services.AddSingleton<IExportInsightsDataToElasticSearchService>(new ExportInsightsDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            services.AddSingleton<IExportInsightsDataToElasticSearchService>(new ExportInsightsDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            services.AddSingleton<IExportAuditLogDataToElasticSearchService>(new ExportAuditLogDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            
            services.AddScoped<IFeatureFlagsUsageService, ElasticSearchFeatureFlagsUsageService>();
            services.AddScoped<IExperimentationService, ExperimentationService>();
            services.AddScoped<IAuditLogSearchService, AuditLogSearchService>();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/error-local-development");
            }
            else
            {
                app.UseExceptionHandler("/error");
            }


            //app.UseRequestBodyLogging();

            //app.UseStatusCodePages();

            app.UseRouting();

            app.UseSwagger(); 
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FeatureFlags.AdminWebAPIs V0.1.3");
            });


            app.UseCors("AllowMyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireCors("AllowMyOrigin");
            });


        }
    }

    public static class ApplicationInsightExtensions
    {
        public static IApplicationBuilder UseRequestBodyLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestBodyLoggingMiddleware>();
        }
    }

    public class RequestBodyLoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var method = context.Request.Method;

            // Ensure the request body can be read multiple times
            context.Request.EnableBuffering();

            // Only if we are dealing with POST or PUT, GET and others shouldn't have a body
            if (context.Request.Body.CanRead && method == HttpMethods.Post &&
                (context.Request.Path.Value.Contains("GetUserVariationResult") ||
                context.Request.Path.Value.Contains("redistest")))
            {
                // Leave stream open so next middleware can read it
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 512, leaveOpen: true);

                var requestBody = await reader.ReadToEndAsync();

                // Reset stream position, so next middleware can read it
                context.Request.Body.Position = 0;

                // Write request body to App Insights
                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                requestTelemetry?.Properties.Add("RequestBody", requestBody);
            }

            // Call next middleware in the pipeline
            await next(context);
        }
    }
}
