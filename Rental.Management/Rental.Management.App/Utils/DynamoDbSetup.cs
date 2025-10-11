using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;

namespace Rental.Management.App.Utils;

public static class DynamoDbSetup
{
    public static async Task EnsureMotoTableExistsAsync(IAmazonDynamoDB client)
    {
        const string tableName = "Motos";

        var existingTables = await client.ListTablesAsync();

        if (!existingTables.TableNames.Contains(tableName))
        {
            Console.WriteLine($"🛠 Criando tabela {tableName}...");

            var request = new CreateTableRequest
            {
                TableName = tableName,

                // 🔑 Chave primária (Partition Key)
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Identificador", ScalarAttributeType.S),
                    new AttributeDefinition("Placa", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Identificador", KeyType.HASH)
                },

                // ⚙️ Índice secundário global (para buscas únicas por Placa)
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

            Console.WriteLine("✅ Tabela criada com sucesso!");
        }
        else
        {
            Console.WriteLine("✔️ Tabela já existe.");
        }
    }
}

