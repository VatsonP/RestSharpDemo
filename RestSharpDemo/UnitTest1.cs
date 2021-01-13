using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharpDemo.Model;
using RestSharpDemo.Utilities;

namespace RestSharpDemo
{
    [TestFixture]
    public class UnitTest1
    {

        private static readonly JsonSerializerOptions jsonSerializerOptions
            = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        [Test]
        public void GetTestMethod()
        {
            var client = new RestClient("http://localhost:3000/");

            var request = new RestRequest("posts/{postid}", Method.GET); 
            request.AddUrlSegment("postid", 1);

            var response = client.Execute(request);

            //Lib 1 - Deserialize<Posts> {or Deserialize<IList<Posts>>} based response (System.Text.Json;)
            var outJsonDeserializedContent = JsonSerializer.Deserialize<Posts>(json   : response.Content, 
                                                                               options: jsonSerializerOptions);
            string resultAuthor = outJsonDeserializedContent.author;
            Assert.That(resultAuthor, Is.EqualTo("Karthik KK"), "Author is not correct");


            //Lib 2 - JSON based response (Newtonsoft.Json.Linq;)
            JObject obs = JObject.Parse(response.Content);
            Assert.That(obs["author"].ToString(), Is.EqualTo("Karthik KK"), "Author is not correct");
        }

        [Test]
        public void PostWithAnonymousBody()
        {
            var client = new RestClient("http://localhost:3000/");

            var request = new RestRequest("posts/{postid}/profile", Method.POST);

            request.RequestFormat = DataFormat.Json;
            // AddJsonBody serializes the object automatically
            request.AddJsonBody( new { name = "Raj" } ); 

            request.AddUrlSegment("postid", 1);

            var response = client.Execute(request);

            //Lib 1 - Deserialize<Dictionary<string, string>> based response (System.Text.Json;)
            var result1 = response.DeserializeResponseDict()["name"];
            Assert.That(result1, Is.EqualTo("Raj"), "Author is not correct");

            //Lib 2 - JSON based response
            var result2 = response.DeserializeResponseJObj()["name"].ToString();
            Assert.That(result2, Is.EqualTo("Raj"), "Author is not correct");

        }


        [Test]
        public void PostWithTypeClassBody()
        {
            var client = new RestClient("http://localhost:3000/");

            var request = new RestRequest("posts", Method.POST);

            request.RequestFormat = DataFormat.Json;
            // AddJsonBody serializes the object automatically
            request.AddJsonBody(new Posts() { id = 17, author="Execute Automation", title = "RestSharp demo course" });

            var response = client.Execute<Posts>(request);

            //Lib 1 - JsonSerializer.Deserialize<Posts>
            //var outJsonDeserializedContent = JsonSerializer.Deserialize<Posts>(json: response.Content);
                                                                          //, options: jsonSerializerOptions);
            //string resultAuthor = outJsonDeserializedContent.author;
            //Assert.That(resultAuthor, Is.EqualTo("Execute Automation"), "Author is not correct");

            Assert.That(response.Data.author, Is.EqualTo("Execute Automation"), "Author is not correct");

        }

        [Test]
        public void PostWithAsync()
        {
            var client = new RestClient("http://localhost:3000/");

            var request = new RestRequest("posts", Method.POST);

            request.RequestFormat = DataFormat.Json;
            // AddJsonBody serializes the object automatically
            request.AddJsonBody(new Posts() { id = 18, author = "Execute Automation", title = "RestSharp demo course" });

            //var response = client.Execute<Posts>(request);

            //Lib 3 - Using Async
            var response = client.ExecutePostAsync<Posts>(request).GetAwaiter().GetResult();

            //Lib 1 - JsonSerializer.Deserialize<Posts>
            //var outJsonDeserializedContent = JsonSerializer.Deserialize<Posts>(json: response.Content);
                                                                          //, options: jsonSerializerOptions);
            //string resultAuthor = outJsonDeserializedContent.author;
            //Assert.That(resultAuthor, Is.EqualTo("Execute Automation"), "Author is not correct");

            Assert.That(response.Data.author, Is.EqualTo("Execute Automation"), "Author is not correct");

        }

      }
}
