using System.Text;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Authentication.Scheme;
using FeatureFlags.APIs.Middlewares;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.Utils.ExtensionMethods;
using FeatureFlagsCo.FeatureInsights;
using FeatureFlagsCo.FeatureInsights.ElasticSearch;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeatureFlags.APIs
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

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
            
            // public api authentication
            services.AddPublicApiAuthentication();
            #endregion

            // add api versioning and swagger
            services.AddApiVersion();
            services.AddSwagger();
            
            // add elasticsearch
            services.AddElasticsearch(Configuration);
            
            // auto register assembly services
            services.AddAssembly(typeof(Startup).Assembly);
            
            // add named service provider
            services.AddNamedServiceProvider();
            
            // add auto mapper
            services.AddAutoMapper(typeof(Startup));
            
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
            services.AddSingleton<MongoDbFeatureFlagZeroCodeSettingService>();
            services.AddSingleton<MetricService>();
            services.AddSingleton<MongoDbAnalyticBoardService>();
            
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
                    options.Configuration = Configuration.GetConnectionString("RedisServerUrl");
                    options.InstanceName = "feature-flags-users";
                });
            }

            #region Telemetry/Insights
            if (hostingType == HostingTypeEnum.Azure.ToString())
            {
                var applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions
                {
                    InstrumentationKey = Configuration.GetSection("ApplicationInsights:InstrumentationKey").Value,
                    ConnectionString = Configuration.GetSection("ApplicationInsights:ConnectionString").Value,
                    EnableAdaptiveSampling = false,
                    EnableDependencyTrackingTelemetryModule = false,
                    EnableAppServicesHeartbeatTelemetryModule = false,
                    EnablePerformanceCounterCollectionModule = false,
                    EnableEventCounterCollectionModule = false,
                    EnableRequestTrackingTelemetryModule = false
                };
                
                services.AddApplicationInsightsTelemetry(applicationInsightsServiceOptions);
            }
            
            services.AddSingleton<MessagingService, MessagingService>();
            services.AddScoped<IFeatureFlagsUsageService, ElasticSearchFeatureFlagsUsageService>();
            services.AddScoped<IExperimentationService, ExperimentationService>();
            services.AddScoped<IAuditLogSearchService, AuditLogSearchService>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseException();
            app.UseVersionedSwagger();
            app.UseRouting();
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
}
