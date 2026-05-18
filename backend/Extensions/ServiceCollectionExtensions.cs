using FluentValidation;
using FluentValidation.AspNetCore;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.Events.Publisher;
using SPA_app.Queue;
using SPA_app.Services;
using SPA_app.Services.Interface;
using SPA_приложение.Queue;
using SPA_приложение.Services;
using SPA_приложение.Validators;

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
            services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedSignalRHandler>();

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();

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
