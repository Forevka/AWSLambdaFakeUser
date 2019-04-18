using Newtonsoft.Json.Linq;

namespace AWSLambdaFakeUser
{
    class SecretDict
    {
        public string accessKey { get; set; }
        public string secretKey { get; set; }
        public string TableName { get; set; }

        public SecretDict(string json)
        {
            JObject jObject = JObject.Parse(json);
            accessKey = (string)jObject["AccountAccessKey"];
            secretKey = (string)jObject["AccountSecretKey"];
            TableName = (string)jObject["TableNameFakeUser"];
        }

    }
}
