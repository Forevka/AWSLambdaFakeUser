using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Newtonsoft.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using Amazon.DynamoDBv2.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambdaFakeUser
{
    public class Function
    {
        static string tableName;

        static BasicAWSCredentials credentials;
        static AmazonDynamoDBClient client;
        static DynamoDBContext worker;

        public async Task InitialiseSecrets()
        {

            string secretName = Environment.GetEnvironmentVariable("secretName");
            string region = Environment.GetEnvironmentVariable("region");
            SecretDict secret;

            MemoryStream memoryStream = new MemoryStream();

            IAmazonSecretsManager clientSecret = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = Environment.GetEnvironmentVariable("stageVersion")
            };
            GetSecretValueResponse response;
            try
            {
                response = await clientSecret.GetSecretValueAsync(request);
            }
            catch (Exception)
            {
                throw;
            }

            if (response.SecretString != null)
            {
                secret = new SecretDict(response.SecretString);

                tableName = secret.TableName;
                credentials = new BasicAWSCredentials(secret.accessKey, secret.secretKey);
                client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
                worker = new DynamoDBContext(client);
            }
            else
            {
                memoryStream = response.SecretBinary;
                StreamReader reader = new StreamReader(memoryStream);
                string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                secret = new SecretDict(decodedBinarySecret);

                tableName = secret.TableName;
                credentials = new BasicAWSCredentials(secret.accessKey, secret.secretKey);
                client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
                worker = new DynamoDBContext(client);
            }
        }
        public async Task<int> FunctionHandler(ILambdaContext context)
        {
            await InitialiseSecrets();
            FakeUser u = await new GenerateRandomUser().NewUser();
            int r = await SaveUser(u);

            return r;
        }

        private async Task<int> SaveUser(FakeUser u)
        {
            ListTablesResponse tableResponse = await client.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                FakeUserModel user = new FakeUserModel
                {
                    ID = u.ID,
                    Name = u.Name,
                    Gender = u.Gender,
                    Email = u.Email,
                    Nat = u.Nat
                };
                await worker.SaveAsync(user);
                return 200;
            }
            else
            {
                return 500;
            }
        }

        private string Dump(object o)
        {
            string json = JsonConvert.SerializeObject(o, Formatting.Indented);
            return json;
        }
    }
}
