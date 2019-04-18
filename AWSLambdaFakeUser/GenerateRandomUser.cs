using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace AWSLambdaFakeUser
{
    class GenerateRandomUser
    {
        private readonly HttpClient client = new HttpClient();
        readonly string ApiUrl = "https://randomuser.me/api/?inc=gender,name,nat,email";
        async Task<string> ApiCall()
        {
            return await client.GetStringAsync(ApiUrl);
        }

        public async Task<FakeUser> NewUser()
        {
            return new FakeUser(await ApiCall());
        }
    }

    
    class FakeUser
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Email { get; set; }
        public string Nat { get; set; }
        public string Gender { get; set; }
        
        public FakeUser(string json)
        {
            JObject jObject = JObject.Parse(json);
            Name = JToken(jObject["results"][0]["name"].ToObject<Dictionary<string, string>>(), " ");
            Email = (string)jObject["results"][0]["email"];
            Nat = (string)jObject["results"][0]["nat"];
            Gender = (string)jObject["results"][0]["gender"];
            Console.WriteLine(ID, Name, Gender, Nat, Email);
        }

        private string JToken(Dictionary<string, string> source, string sequenceSeparator)
        {
            if (source == null)
                throw new ArgumentException("Parameter source can`t be null.");

            var pairs = source.Select(x => string.Format("{0}", x.Value));

            return string.Join(sequenceSeparator, pairs);
        }
    }

    [DynamoDBTable("FakePeopleEveryMinute")]
    class FakeUserModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Nat { get; set; }
        public string Gender { get; set; }
    }

}
