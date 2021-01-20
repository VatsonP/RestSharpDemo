using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Constraints;
using RestSharp;
using RestSharpDemo.Model;
using RestSharpDemo.Utilities;
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
        // for write log file on error
        protected string CurrentTestName { get; private set; }
        protected string CurrentTestFolderName { get; private set; }
        protected DirectoryInfo BaseLogFolder { get; set; }
        protected DirectoryInfo JsonFolder { get; private set; }
        protected string JsonFullFileName { get; private set; }

        protected LogWriter logWriter;

        [OneTimeSetUp]
        public static void OneTimeSetUp() { }

        [SetUp]
        public void SetUp()
        {
            initLogWriter(MethodInfo.GetCurrentMethod().Name);
        }

        [TearDown]
        public void TearDown()
        {
            stopTestInfo(MethodInfo.GetCurrentMethod().Name);
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown() { }

        protected void initLogWriter(string curMethodName)
        {
            // for write log file on error
            CurrentTestName = TestContext.CurrentContext.Test.Name;
            var CurrentFileName = CurrentTestName;
            CurrentTestFolderName = TestContext.CurrentContext.TestDirectory;

            if (CurrentTestName.Length > 30)
            {
                CurrentTestName = CurrentTestName.Substring(0, 30);
            }

            var startMsg      = "Start test: " + CurrentTestName;
            Console.WriteLine(curMethodName + " - " + startMsg);

            BaseLogFolder = createBaseLogDir(CurrentTestFolderName);
            logWriter = new LogWriter(BaseLogFolder, CurrentTestName, CurrentFileName);

            logWriter.LogWrite("// ------------------ START ---------------------------- //",
                               "// ----------------- NEW TEST -------------------------- //");
            logWriter.LogWrite(curMethodName, startMsg);

            // for write json file 
            JsonFolder = createBaseLogDir(currentTestFolder: CurrentTestFolderName, newSubFolder: "json");
            JsonFullFileName = Path.Combine(JsonFolder.FullName, "custdata.json");
        }
        protected DirectoryInfo createBaseLogDir(string currentTestFolder, string newSubFolder = "Log")
        {
            return Directory.CreateDirectory(Path.Combine(currentTestFolder, newSubFolder));
        }

        protected void stopTestInfo(string curMethodName)
        {
            var testResult = TestContext.CurrentContext.Result.Outcome;

            var finishMsg    = "Stop test - OK: " + CurrentTestName;

            if (Equals(testResult, ResultState.Failure) ||
                Equals(testResult == ResultState.Error))
            {
                finishMsg = "Stop test - ResultState.Failure or ResultState.Error in TestName: " + CurrentTestName;
            }
            Console.WriteLine(curMethodName + " - " + finishMsg);
            logWriter.LogWrite(curMethodName, finishMsg);
        }

        /* Можно настроить механизм сериализации/десериализации, используя свойства 
         * JsonSerializerOptions:
         * AllowTrailingCommas: устанавливает, надо ли добавлять после последнего элемента в json запятую. Если равно true, запятая добавляется
         * IgnoreNullValues: устанавливает, будут ли сериализоваться/десериализоваться в json объекты и их свойства со значением null
         * IgnoreReadOnlyProperties: аналогично устанавливает, будут ли сериализоваться свойства, предназначенные только для чтения
         * WriteIndented: устанавливает, будут ли добавляться в json пробелы (условно говоря, для красоты). 
         *                Если равно true устанавливаются дополнительные пробелы
         */
        protected static readonly JsonSerializerOptions jsonSerializerOptions
            = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

        protected T GetReqJsonDeserializeResult<T>(string _requestBaseLocation, string _pathWithParam, 
                                                 string _paramName, string _paramVal,
                                                 JsonSerializerOptions _jsonSerializerOptions)
        {
            //GET request to localhost:4545/customers/{custid}
            var client = new RestClient(_requestBaseLocation);

            var request = new RestRequest(_pathWithParam, Method.GET);
            if ((_paramName != null && _paramName != String.Empty) &&
                 (_paramVal != null && _paramVal != String.Empty))
            {
                request.AddUrlSegment(_paramName, _paramVal);
            }
            var response = client.Execute(request);

            //Deserialize<DemoCust> based response (System.Text.Json;)
            return JsonSerializer.Deserialize<T>(json: response.Content, options: _jsonSerializerOptions);
        }
        
        //----------------------------------------------------------------------------------------------------------------//

        [Test, TestCaseSource(typeof(DataProviders), nameof(DataProviders.ValidCustomers))]
        public async Task GetForDemoCustFromMockMB(CustData custData)
        {
            var curMethodName = MethodInfo.GetCurrentMethod().Name;

            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation  = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);
            Assert.IsNotNull(mbClient);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "Stub Example for: " + curMethodName);
            Assert.IsNotNull(mbImposter);

            try
            {
               //simple Imposter
                mbImposter.AddStub().OnPathAndMethodEqual("/customers/" + custid.ToString(), MbDotNet.Enums.Method.Get)
                          .ReturnsJson(HttpStatusCode.OK, custData);

                await mbClient.SubmitAsync(mbImposter);

                var httpImposter = mbClient.GetHttpImposterAsync(imposterPortNum);
                Assert.IsNotNull(httpImposter);
                logWriter.LogWrite(curMethodName, "httpImposter.IsCompleted= " + httpImposter.IsCompleted);

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
            var curMethodName = MethodInfo.GetCurrentMethod().Name;

            int imposterPortNum = 4545;
            int custid = custData.id;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);
            Assert.IsNotNull(mbClient);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "Stub Example for: " + curMethodName);
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

                var httpImposter = mbClient.GetHttpImposterAsync(imposterPortNum);
                Assert.IsNotNull(httpImposter);
                logWriter.LogWrite(curMethodName, "httpImposter.IsCompleted= " + httpImposter.IsCompleted);

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


        [Test]
        public async Task GetForDemoCustWithFileStreamSerialize()
        {
            var curMethodName = MethodInfo.GetCurrentMethod().Name;

            // Заполнение сгенрированными данными из DataProviders.ValidCustomers;
            IEnumerable<CustData> ieCustDataGenerated = DataProviders.ValidCustomers;
            // сохранение сгенирированных данных сериализации в файл json
            using (FileStream fs = new FileStream(JsonFullFileName, FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<IEnumerable<CustData>>(fs, ieCustDataGenerated, jsonSerializerOptions);
                var msg = "Serialized Data has been saved to file: " + JsonFullFileName;
                Console.WriteLine(msg);
                logWriter.LogWrite(curMethodName, msg);
            }

            // чтение данных и десериализация из файла json
            IEnumerable<CustData> ieCustDataRestored;
            using (FileStream fs = new FileStream(JsonFullFileName, FileMode.Open))
            {
                ieCustDataRestored = await JsonSerializer.DeserializeAsync<IEnumerable<CustData>>(fs);
                var msg = "Deserialized Data has been read from file: " + JsonFullFileName;
                Console.WriteLine(msg);
                logWriter.LogWrite(curMethodName, msg);
            }

            // --  MountebankClient  ------------------------------------------------------------------------------------ //
            int imposterPortNum = 4545;
            string mbClientBaseLocation = "http://localhost:2525";
            string requestBaseLocation = "http://localhost:" + imposterPortNum;

            MountebankClient mbClient = new MountebankClient(mbClientBaseLocation);
            Assert.IsNotNull(mbClient);

            var mbImposter = mbClient.CreateHttpImposter(imposterPortNum, "Stub Example for: "+ curMethodName);
            Assert.IsNotNull(mbImposter);

            try
            {
                //simple Imposter
                mbImposter.AddStub().OnPathAndMethodEqual("/customers", MbDotNet.Enums.Method.Get)
                          .ReturnsJson(HttpStatusCode.OK, ieCustDataRestored);

                await mbClient.SubmitAsync(mbImposter);

                var httpImposter = mbClient.GetHttpImposterAsync(imposterPortNum);
                Assert.IsNotNull(httpImposter);
                logWriter.LogWrite(curMethodName, "httpImposter.IsCompleted= " + httpImposter.IsCompleted);

                //GET Deserialized result of the request to localhost:4545/customers/{custid}
                IEnumerable<CustData> ieCustDataOutput = GetReqJsonDeserializeResult<IEnumerable<CustData>>(
                                                                        _requestBaseLocation: requestBaseLocation,
                                                                        _pathWithParam: "/customers",
                                                                        _paramName: "", _paramVal: "",
                                                                        _jsonSerializerOptions: jsonSerializerOptions);

                IEnumerator<CustData> rstIEnum = ieCustDataRestored.GetEnumerator();
                IEnumerator<CustData> outIEnum = ieCustDataOutput.GetEnumerator();

                foreach (var genItem in ieCustDataGenerated)
                {
                    Assert.IsTrue(rstIEnum.MoveNext());
                     Assert.IsTrue(outIEnum.MoveNext());
                    var rstItem = (CustData)rstIEnum.Current;     // берем элемент ieCustDataRestored на текущей позиции
                    var outItem = (CustData)outIEnum.Current;     // берем элемент outJsonICustData на текущей позиции

                    var msg1 = "genItem.ToString()= " + genItem.ToString();
                    var msg2 = "rstItem.ToString()= " + rstItem.ToString();
                    var msg3 = "outItem.ToString()= " + outItem.ToString();

                    Console.WriteLine(msg1); Console.WriteLine(msg2); Console.WriteLine(msg3);

                    logWriter.LogWrite(curMethodName, msg1);
                    logWriter.LogWrite(curMethodName, msg2);
                    logWriter.LogWrite(curMethodName, msg3);

                    Assert.That(genItem, Is.EqualTo(rstItem), "rstItem of <IEnumerable<CustData>> is not correct");
                    Assert.That(genItem, Is.EqualTo(outItem), "outItem of <IEnumerable<CustData>> is not correct");
                }

            }
            finally
            {
                await mbClient.DeleteImposterAsync(imposterPortNum);
            }
        }
    }
}
