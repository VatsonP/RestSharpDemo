using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharpDemo.Model;
using MbDotNet;
using MbDotNet.Models.Responses;
using MbDotNet.Models.Responses.Fields;
using MbDotNet.Models.Predicates;
using MbDotNet.Models.Predicates.Fields;


namespace RestSharpDemo
{
    [TestFixture]
    public class UnitTest2
    {

        private static readonly JsonSerializerOptions jsonSerializerOptions
            = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        private T GetReqJsonDeserializeResult<T>(string _requestBaseLocation, string _pathWithParam, 
                                                 string _paramName, string _paramVal,
                                                 JsonSerializerOptions _jsonSerializerOptions)
        {
            //GET request to localhost:4545/customers/{custid}
            var client = new RestClient(_requestBaseLocation);

            var request = new RestRequest(_pathWithParam, Method.GET);
            request.AddUrlSegment(_paramName, _paramVal);

            var response = client.Execute(request);

            //Deserialize<DemoCust> based response (System.Text.Json;)
            return JsonSerializer.Deserialize<T>(json: response.Content, options: _jsonSerializerOptions);
        }
        
        //----------------------------------------------------------------------------------------------------------------//

        [Test, TestCaseSource(typeof(DataProviders), nameof(DataProviders.ValidCustomers))]
        public async Task GetForDemoCustFromMockMB(CustData custData)
        {
            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation  = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);
            Assert.IsNotNull(mbClient);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "StubExample");
            Assert.IsNotNull(mbImposter);

            try
            {
               //simple Imposter
                mbImposter.AddStub().OnPathAndMethodEqual("/customers/" + custid.ToString(), MbDotNet.Enums.Method.Get)
                          .ReturnsJson(HttpStatusCode.OK, custData);

                await mbClient.SubmitAsync(mbImposter);

                Assert.IsNotNull(mbClient.GetHttpImposterAsync(imposterPortNum));

                //GET Deserialized result of the request to localhost:4545/customers/{custid}
                CustData outJsonDemoCust = GetReqJsonDeserializeResult<CustData>(_requestBaseLocation: requestBaseLocation,
                                                                                 _pathWithParam: "/customers/{custid}",
                                                                                 _paramName: "custid", _paramVal: custid.ToString(),
                                                                                 _jsonSerializerOptions: jsonSerializerOptions);
                Console.WriteLine(outJsonDemoCust.ToString());
                Assert.That(outJsonDemoCust, Is.EqualTo(custData), "custData is not correct");
            }
            finally
            {
                await mbClient.DeleteImposterAsync(imposterPortNum);
            }
        }


        [Test, TestCaseSource(typeof(DataProviders), nameof(DataProviders.ValidCustomers))]
        public async Task GetForDemoCustFromMockMBComplex(CustData custData)
        {
            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);
            Assert.IsNotNull(mbClient);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "StubExample");
            Assert.IsNotNull(mbImposter);

            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Post);
            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Put);
            mbImposter.AddStub().ReturnsStatus(HttpStatusCode.MethodNotAllowed).OnMethodEquals(MbDotNet.Enums.Method.Delete);

            try
            {
                var mbResponseFields = new HttpResponseFields
                {
                    StatusCode = HttpStatusCode.OK,
                    Headers = new Dictionary<string, Object> {
                                                               { "Content-Type", "application/json" },
                                                               { "Location", requestBaseLocation + "/customers/" + custid.ToString() }
                                                             },
                    ResponseObject = custData,
                    Mode = "text"
                };
                var mbResponse = new IsResponse<HttpResponseFields>(mbResponseFields);

                var mbComplexPredicateFields = new HttpPredicateFields
                {
                    Method = MbDotNet.Enums.Method.Get,
                    Path = "/customers/" + custid.ToString()
                };
                var mbComplexPredicate = new EqualsPredicate<HttpPredicateFields>(mbComplexPredicateFields);

                mbImposter.AddStub().On(mbComplexPredicate).Returns(mbResponse);

                await mbClient.SubmitAsync(mbImposter);

                Assert.IsNotNull(mbClient.GetHttpImposterAsync(imposterPortNum));

                //GET Deserialized result of the request to localhost:4545/customers/{custid}
                CustData outJsonDemoCust = GetReqJsonDeserializeResult<CustData>(_requestBaseLocation: requestBaseLocation,
                                                                                 _pathWithParam: "/customers/{custid}",
                                                                                 _paramName: "custid", _paramVal: custid.ToString(),
                                                                                 _jsonSerializerOptions: jsonSerializerOptions);
                Console.WriteLine(outJsonDemoCust.ToString());
                Assert.That(outJsonDemoCust, Is.EqualTo(custData), "custData is not correct");
            }
            finally
            {
                await mbClient.DeleteImposterAsync(imposterPortNum);
            }
        }


    }
}
