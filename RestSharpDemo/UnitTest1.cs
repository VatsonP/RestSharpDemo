using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharpDemo.Model;
using RestSharpDemo.Utilities;
using MbDotNet;
using MbDotNet.Models.Stubs;
using MbDotNet.Models.Responses;
using MbDotNet.Models.Responses.Fields;
using MbDotNet.Models.Predicates;
using MbDotNet.Models.Predicates.Fields;


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

        [Test, TestCaseSource(typeof(DataProviders), nameof(DataProviders.ValidCustomers))]
        public async Task GetForDemoCustFromMockMB(CustData custData)
        {
            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation  = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "StubExample");
            Assert.IsNotNull(mbImposter);

            try
            {
               //simple Imposter
                mbImposter.AddStub().OnPathAndMethodEqual("/customers/" + custid.ToString(), MbDotNet.Enums.Method.Get)
                          .ReturnsJson(HttpStatusCode.OK, custData);

                await mbClient.SubmitAsync(mbImposter);


                Assert.IsNotNull(mbClient.GetHttpImposterAsync(imposterPortNum));

                //GET request to localhost:4545/customers/{custid}
                var client = new RestClient(requestBaseLocation);

                var request = new RestRequest("/customers/{custid}", RestSharp.Method.GET);
                request.AddUrlSegment("custid", custid.ToString());

                var response = client.Execute(request);

                //Lib 1 - Deserialize<DemoCust> {or Deserialize<IList<DemoCust>>} based response (System.Text.Json;)
                var outJsonDemoCust = JsonSerializer.Deserialize<CustData>(json: response.Content,
                                                                        options: jsonSerializerOptions);

                Console.WriteLine(outJsonDemoCust.ToString());
                Assert.That(outJsonDemoCust, Is.EqualTo(custData), "custData is not correct");
            }
            finally
            {
                if (mbClient != null && mbImposter != null)
                {
                    await mbClient.DeleteImposterAsync(imposterPortNum);
                }
            }
        }


        [Test, TestCaseSource(typeof(DataProviders), nameof(DataProviders.ValidCustomers))]
        public async Task GetForDemoCustFromMockMBComplex(CustData custData)
        {
            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation  = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "StubExample");
            Assert.IsNotNull(mbImposter);

            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Post);
            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Put);
            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Delete);

            try
            {
                var mbResponseFields = new HttpResponseFields
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Headers = new Dictionary<string, Object> { { "Location", requestBaseLocation + "/customers/" + custid.ToString() } },
                    ResponseObject = custData
                };
                var mbResponse = new IsResponse<HttpResponseFields>(mbResponseFields);
                
                var mbComplexPredicateFields = new HttpPredicateFields
                {
                    Method = MbDotNet.Enums.Method.Get,
                    Path = "/customers/" + custid.ToString()
                    //QueryParameters = new Dictionary<string, Object> { { "custid", custid.ToString() } },
                    //Headers = new Dictionary<string, Object> { { "Accept", "application/json" } }
                };
                var mbComplexPredicate = new EqualsPredicate<HttpPredicateFields>(mbComplexPredicateFields);

                mbImposter.AddStub().On(mbComplexPredicate).Returns(mbResponse).ReturnsStatus(HttpStatusCode.OK);

                await mbClient.SubmitAsync(mbImposter);

                Assert.IsNotNull(mbClient.GetHttpImposterAsync(imposterPortNum));
                
                //GET request to localhost:4545/customers/{custid}
                var client = new RestClient(requestBaseLocation);

                var request = new RestRequest("/customers/{custid}", Method.GET);
                request.AddUrlSegment("custid", custid.ToString());

                var response = client.Execute(request);

                //Lib 1 - Deserialize<DemoCust> {or Deserialize<IList<DemoCust>>} based response (System.Text.Json;)
                var outJsonDemoCust = JsonSerializer.Deserialize<CustData>(json: response.Content,
                                                                        options: jsonSerializerOptions);
                Console.WriteLine(outJsonDemoCust.ToString());
                Assert.That(outJsonDemoCust, Is.EqualTo(custData), "custData is not correct");
            }
            finally
            {
                if (mbClient != null && mbImposter != null)
                {
                    await mbClient.DeleteImposterAsync(imposterPortNum);
                }
            }
        }


    }
}
