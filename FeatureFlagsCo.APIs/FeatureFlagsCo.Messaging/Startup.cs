using FeatureFlags.APIs.Services;
using FeatureFlagsCo.Messaging.Services;
using FeatureFlagsCo.Messaging.ViewModels;
using FeatureFlagsCo.MQ;
using FeatureFlagsCo.MQ.Export;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.Messaging
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            services.Configure<MySettings>(options => Configuration.GetSection("MySettings").Bind(options));
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));
            services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            // services.AddSingleton<IInsighstMqService, InsighstRabbitMqService>();
            // services.AddSingleton<IFeatureFlagMqService, FeatureFlagMqService>();
            // services.AddSingleton<IExperimentStartEndMqService, ExperimentStartEndMqService>();
            // services.AddSingleton<IExperimentMqService, ExperimentstRabbitMqService>();
            services.AddSingleton<ExperimentsService, ExperimentsService>();

            var serviceProvider = services.BuildServiceProvider();

            // service bus sender
            var q1SenderLogger = serviceProvider.GetService<ILogger<ServiceBusQ1Sender>>();
            var q4SenderLogger = serviceProvider.GetService<ILogger<ServiceBusQ4Sender>>();
            var q5SenderLogger = serviceProvider.GetService<ILogger<ServiceBusQ5Sender>>();

            services.AddSingleton<ServiceBusQ1Sender>(new ServiceBusQ1Sender(Configuration, q1SenderLogger));
            services.AddSingleton<ServiceBusQ4Sender>(new ServiceBusQ4Sender(Configuration, q4SenderLogger));
            services.AddSingleton<ServiceBusQ5Sender>(new ServiceBusQ5Sender(Configuration, q5SenderLogger));

            // service bus receiver
            // var experimentStartEndmqService = serviceProvider.GetService<IExperimentStartEndMqService>();
            // var ffMqService = serviceProvider.GetService<IFeatureFlagMqService>();
            // var experimentstRabbitMqService = serviceProvider.GetService<IExperimentMqService>();
            var experimentsService = serviceProvider.GetService<ExperimentsService>();
            // var q1ReceiverLogger = serviceProvider.GetService<ILogger<ServiceBusQ1Receiver>>();
            // var q4ReceiverLogger = serviceProvider.GetService<ILogger<ServiceBusQ4Receiver>>();
            // var q5ReceiverLogger = serviceProvider.GetService<ILogger<ServiceBusQ5Receiver>>();
            var q3ReceiverLogger = serviceProvider.GetService<ILogger<ServiceBusQ3Receiver>>();

            services.AddSingleton(new ServiceBusQ3Receiver(Configuration, experimentsService, q3ReceiverLogger));
            // services.AddSingleton<ServiceBusQ1Receiver>(new ServiceBusQ1Receiver(Configuration, experimentStartEndmqService, q1ReceiverLogger));
            // services.AddSingleton<ServiceBusQ4Receiver>(new ServiceBusQ4Receiver(Configuration, ffMqService, q4ReceiverLogger));
            // services.AddSingleton<ServiceBusQ5Receiver>(new ServiceBusQ5Receiver(Configuration, experimentstRabbitMqService, q5ReceiverLogger));

            var esHost = this.Configuration.GetSection("MySettings").GetSection("ElasticSearchHost").Value;

            var insightsRabbitMqUrl =
                this.Configuration.GetSection("MySettings").GetSection("InsightsRabbitMqUrl").Value;
/**/

            services.AddSingleton<IExportExperimentsDataToElasticSearchService>(
                new ExportExperimentsDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            //services.AddSingleton<IExportInsightsDataToElasticSearchService>(new ExportInsightsDataToElasticSearchService(insightsRabbitMqUrl, esHost));
            services.AddSingleton<IExportAuditLogDataToElasticSearchService>(
                new ExportAuditLogDataToElasticSearchService(insightsRabbitMqUrl, esHost));


            var exptsService = serviceProvider.GetService<ExperimentsService>();
            services.AddSingleton<IExperimentResultService>(new ExperimentResultService(insightsRabbitMqUrl,
                exptsService));

            var hostingType = this.Configuration.GetSection("MySettings").GetSection("HostingType").Value;
            if (hostingType == "Azure")
            {
                Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions aiOptions
                    = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
                aiOptions.InstrumentationKey = this.Configuration.GetSection("ApplicationInsights")
                    .GetSection("InstrumentationKey").Value;
                aiOptions.ConnectionString = this.Configuration.GetSection("ApplicationInsights")
                    .GetSection("ConnectionString").Value;
                aiOptions.EnableAdaptiveSampling = false;
                aiOptions.EnableDependencyTrackingTelemetryModule = false;
                aiOptions.EnableAppServicesHeartbeatTelemetryModule = false;
                aiOptions.EnablePerformanceCounterCollectionModule = false;
                aiOptions.EnableEventCounterCollectionModule = false;
                aiOptions.EnableRequestTrackingTelemetryModule = false;
                services.AddApplicationInsightsTelemetry(aiOptions);
            }
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FeatureFlags.AdminWebAPIs V0.1.3");
            });

            app.UseRouting();
            app.UseCors("AllowMyOrigin");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireCors("AllowMyOrigin");
            });
        }
    }
}