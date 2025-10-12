using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon.S3.Util;

namespace Rental.Management.App.Utils;

public static class InfraSetup
{
    public static async Task EnsureMotoTableExistsAsync(IAmazonDynamoDB client)
    {
        const string tableName = "Motos";

        var existingTables = await client.ListTablesAsync();

        if (!existingTables.TableNames.Contains(tableName))
        {
            Console.WriteLine($"Criando tabela {tableName}...");

            var request = new CreateTableRequest
            {
                TableName = tableName,

                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Identificador", ScalarAttributeType.S),
                    new AttributeDefinition("Placa", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Identificador", KeyType.HASH)
                },

                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "Placa-index",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("Placa", KeyType.HASH)
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                    }
                },

                ProvisionedThroughput = new ProvisionedThroughput(5, 5)
            };

            await client.CreateTableAsync(request);

            Console.WriteLine("Tabela criada com sucesso!");
        }
        else
        {
            Console.WriteLine("Tabela já existe.");
        }
    }

    public static async Task EnsureEntregadoresTableExistsAsync(IAmazonDynamoDB client)
    {
        const string tableName = "Entregadores";

        var existingTables = await client.ListTablesAsync();

        if (!existingTables.TableNames.Contains(tableName))
        {
            Console.WriteLine($"Criando tabela {tableName}...");

            var request = new CreateTableRequest
            {
                TableName = tableName,

                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Identificador", ScalarAttributeType.S),
                    new AttributeDefinition("Cnpj", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Identificador", KeyType.HASH)
                },

                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = "Cnpj-index",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("Cnpj", KeyType.HASH)
                        },
                        Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        ProvisionedThroughput = new ProvisionedThroughput(5, 5)
                    }
                },

                ProvisionedThroughput = new ProvisionedThroughput(5, 5)
            };

            await client.CreateTableAsync(request);

            Console.WriteLine("Tabela criada com sucesso!");
        }
        else
        {
            Console.WriteLine("Tabela já existe.");
        }
    }


    public static async Task EnsureBucketExistsAsync(IAmazonS3 client)
    {
        try
        {
            const string bucketName = "fotos-cnh";

            Console.WriteLine($"Verificando bucket '{bucketName}'...");

            // Verifica se o bucket já existe
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);

            if (!bucketExists)
            {
                Console.WriteLine($"Criando bucket '{bucketName}'...");

                var createBucketRequest = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                var response = await client.PutBucketAsync(createBucketRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    Console.WriteLine("Bucket criado com sucesso!");
                else
                    Console.WriteLine($"Falha ao criar bucket. StatusCode: {response.HttpStatusCode}");
            }
            else
            {
                Console.WriteLine("Bucket já existe.");
            }
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Erro AWS ao criar/verificar bucket: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
        }
    }

}

