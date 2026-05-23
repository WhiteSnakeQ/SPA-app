using Elastic.Clients.Elasticsearch;
using FluentValidation;
using FluentValidation.AspNetCore;
using RabbitMQ.Client;
using SPA_app.BackGroundChannel.Comments;
using SPA_app.BackGroundChannel.Files;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.FileUploaded;
using SPA_app.Events.Interface;
using SPA_app.Events.Publisher;
using SPA_app.Extensions.Init;
using SPA_app.GraphQL;
using SPA_app.RabbitMQ.HostedService.CommentCreate;
using SPA_app.RabbitMQ.HostedService.FileCreate;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using SPA_app.Services.CacheS;
using SPA_app.Services.CaptchaS;
using SPA_app.Services.CommentsS;
using SPA_app.Services.ElasticIndexIntialazer;
using SPA_app.Services.ElasticSearch;
using SPA_app.Services.FileS;
using SPA_app.Services.ImageS;
using SPA_app.Services.SeedS;
using SPA_приложение.Validators;
using System.Threading.Channels;

namespace SPA_приложение.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<ElasticSearchInitializer>();
			services.AddScoped<ICaptchaService, CaptchaService>();
			services.AddScoped<ICommentFileService, CommentFileService>();
			services.AddScoped<ICommentService, CommentService>();
			services.AddScoped<ISeedService, SeedService>();
			services.AddScoped<IImageService, ImageService>();
			services.AddScoped<ICommentSearchService, CommentSearchService>();
			services.AddScoped<ICacheService, CacheService>();

			services.AddScoped<IEventPublisher, EventPublisher>();
			services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedCacheCleanHandler>();
			services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedSignalRHandler>();
			services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedInsertIndexSearch>();

			services.AddScoped<IEventHandler<FileUploadedEvent>, FileUploadedHandler>();

			services.AddSingleton<RabbitMQInit>();

			services.AddSingleton<IConnection>(sp =>
			{
				var factory = new ConnectionFactory
				{
					HostName = "rabbitmq",
					DispatchConsumersAsync = true
				};

				return factory.CreateConnection();
			});

			services.AddSingleton<IMessagePublisher, MessagePublisher>();

			services.AddHostedService<ImageResizeConsumer>();
            services.AddHostedService<FileCreateConsumer>();
            services.AddHostedService<CommentCreateSignalRConsumer>();
            services.AddHostedService<CommentCreateCacheCleanConsumer>();
            services.AddHostedService<CommentCreateIndexSearchConsumer>();

            services.AddHostedService<CommentWorker>();
            services.AddHostedService<FileWorker>();

            services.AddSingleton(Channel.CreateBounded<CommentCreatedMessage>(
				new BoundedChannelOptions(5000)
				{
					FullMode = BoundedChannelFullMode.Wait
				}));
			services.AddSingleton(Channel.CreateBounded<FileCreatedMessage>(
				new BoundedChannelOptions(5000)
				{
					FullMode = BoundedChannelFullMode.Wait
				}));

			services.AddGraphQLServer().AddQueryType<CommentGQL>();

			services.AddSingleton(new ElasticsearchClient(new Uri("http://elasticsearch:9200")));

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
