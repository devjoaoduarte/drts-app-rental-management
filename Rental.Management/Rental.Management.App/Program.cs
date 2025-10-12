using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

var awsConfig = builder.Configuration.GetSection("AWS");

// DynamoDB Local
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
{
    return new AmazonDynamoDBClient(new AmazonDynamoDBConfig
    {
        ServiceURL = awsConfig["DynamoDbLocal"],
        UseHttp = true
    });
});

// S3 e SQS via LocalStack
builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    return new AmazonS3Client(new AmazonS3Config
    {
        ServiceURL = awsConfig["ServiceURL"],
        ForcePathStyle = true
    });
});

builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    return new AmazonSQSClient(new AmazonSQSConfig
    {
        ServiceURL = awsConfig["ServiceURL"]
    });
});

builder.Services.AddSingleton(typeof(IDynamoDbRepository<>), typeof(DynamoDbRepository<>));
builder.Services.AddScoped<IMotorcycleServices, MotorcycleService>();
builder.Services.AddScoped<IEntregadoresService, EntregadoresService>();
builder.Services.AddScoped<IS3Repository, S3Repository>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dynamoClient = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
    await InfraSetup.EnsureMotoTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureEntregadoresTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureBucketExistsAsync(s3Client);
}

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
