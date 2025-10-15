using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SQS;
using Rental.Management.App.Consumers;

namespace Rental.Management.App
{
    public static class DependencyInjection
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            var awsConfig = configuration.GetSection("AWS");

            // DynamoDB Local
            services.AddSingleton<IAmazonDynamoDB>(_ =>
                new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = awsConfig["DynamoDbLocal"],
                    UseHttp = true
                }));

            // S3 via LocalStack
            services.AddSingleton<IAmazonS3>(_ =>
                new AmazonS3Client(new AmazonS3Config
                {
                    ServiceURL = awsConfig["ServiceURL"],
                    ForcePathStyle = true
                }));

            // SQS via LocalStack
            services.AddSingleton<IAmazonSQS>(_ =>
                new AmazonSQSClient(new AmazonSQSConfig
                {
                    ServiceURL = awsConfig["ServiceURL"]
                }));

            services.AddSingleton(typeof(IDynamoDbRepository<>), typeof(DynamoDbRepository<>));
            services.AddSingleton<IMotorcycleService, MotorcycleService>();
            services.AddSingleton<IDeliveryMenService, DeliveryMenService>();
            services.AddSingleton<IRentalService, RentalService>();
            services.AddSingleton<IS3Repository, S3Repository>();
            services.AddSingleton<ISQSRepository, SQSRepository>();

            services.AddHostedService<MotorcycleQueueConsumerWorker>();
        }
    }
}
