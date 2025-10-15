using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SQS;
using Rental.Management.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

DependencyInjection.AddServices(builder.Services, builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dynamoClient = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
    var sqsClient = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
    await InfraSetup.EnsureMotoTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureEntregadoresTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureLocacoesTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureMotosNotificationTableExistsAsync(dynamoClient);
    await InfraSetup.EnsureBucketExistsAsync(s3Client);
    await InfraSetup.EnsureMotosQueueExistsAsync(sqsClient);
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
