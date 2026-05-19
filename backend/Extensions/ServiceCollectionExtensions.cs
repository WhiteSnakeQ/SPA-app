using FluentValidation;
using FluentValidation.AspNetCore;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.Events.Publisher;
using SPA_app.Queue;
using SPA_app.Services.CacheS;
using SPA_app.Services.CaptchaS;
using SPA_app.Services.FileS;
using SPA_app.Services.CommentsS;
using SPA_app.Services.ImageS;
using SPA_app.Services.SeedS;
using SPA_приложение.Queue;
using SPA_приложение.Validators;
using SPA_app.GraphQL;

namespace SPA_приложение.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICaptchaService, CaptchaService>();
            services.AddScoped<ICommentFileService, CommentFileService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ISeedService, SeedService>();
            services.AddScoped<IImageService, ImageService>();

            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedCacheClean>();
            services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedSignalRHandler>();

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();

            services.AddScoped<ICacheService, CacheService>();

            services.AddGraphQLServer().AddQueryType<CommentGQL>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "redis:6379";
                options.InstanceName = "SPA-app";
            });

            return services;
        }

        public static IServiceCollection AddApplicationValidators(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<CreateCommentValidator>();

            services.AddFluentValidationAutoValidation();

            return services;
        }

        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("cors", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("Captcha-Id");
                });
            });

            return services;
        }
    }
}
