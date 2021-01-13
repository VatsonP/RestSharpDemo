using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpDemo.Utilities
{
    public static class Libraries
    {

        public static Dictionary<string, string> DeserializeResponseDict(this IRestResponse restResponse)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json: restResponse.Content);
        }

        public static JObject DeserializeResponseJObj(this IRestResponse restResponse)
        {
            return JObject.Parse(restResponse.Content);
        }

        public static string GetResponseJObjString(this IRestResponse response, string responseObject)
        {
            JObject obs = JObject.Parse(response.Content);
            return obs[responseObject].ToString();
        }

        /* depricated: "RestClientExtensions.ExecuteAsync(IRestClient, IRestRequest, Action)' is obsolete: 'Use ExecuteAsync that returns Task'"
         * public static async Task<IRestResponse<T>> ExecuteAsyncRequest<T>(this RestClient client, IRestRequest request) where T : class, new()
        {
            var taskCompletionSource = new TaskCompletionSource<IRestResponse<T>>();

            client.ExecuteAsync<T>(request, restResponse =>
            {
                if (restResponse.ErrorException != null)
                {
                    const string message = "Error retrieving response.";
                    throw new ApplicationException(message, restResponse.ErrorException);
                }

                taskCompletionSource.SetResult(restResponse);
            });

            return await taskCompletionSource.Task;
        }
        */

        public static async Task<IRestResponse<T>> ExecuteAsyncRequest<T>(this RestClient client, IRestRequest request) where T : class, new()
        {
            var restResponseTask = await client.ExecuteAsync<T>(request);

            if (restResponseTask.ErrorException != null)
            {
                const string message = "Error retrieving response.";
                throw new ApplicationException(message, restResponseTask.ErrorException);
            }

            return restResponseTask;
        }


    }
}
