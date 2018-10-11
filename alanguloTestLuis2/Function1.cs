
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Data.SqlClient;
using System.Data;

namespace alanguloTestLuis2
{
    public static class Function1
    {
        static HttpClient httpClient = new HttpClient();

        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {


            // The Application ID from any published app in luis.ai, found in Manage > Application Information 

            var LUISappID = "";

            // The above LUIS app's authoring/starter key found in Manage > Keys and Endpoints 

            var LUISsubscriptionKey = "";

            var LUISendpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/7ff37942-98d9-4e78-a1a5-75caad8f0886?subscription-key=59ed7a93381840ab8bb2df3f94da6093&timezoneOffset=-360&q=";



            // Copy/paste your connection string from your SQL database resource in the Azure portal, found on the Overview page

            // Substitute your username and password, to your SQL database, where indicated

            var SQLconnectionString = "";



           // log.LogInformation("Get LUIS query from HTTP Request");



            // Query string

            string query = req.Query["query"];



            // POST Body

            dynamic data = await new StreamReader(req.Body).ReadToEndAsync();



            // Final LUIS Query

            query = query ?? data?.query;



            // If no query, return 204

            if (String.IsNullOrEmpty(query))
            {

                return new HttpResponseMessage(HttpStatusCode.NoContent);

            }



           // log.LogInformation("LUIS QUERY:" + query);



            // LUIS HTTP CALL

            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", LUISsubscriptionKey);

            var response = await httpClient.GetAsync(LUISendpoint + LUISappID + "/?verbose=true&q=" + query);



            // If LUIS error, return 204 - YOU SHOULD COMMENT THIS OUT! Then build/run again.

            if (!response.IsSuccessStatusCode)
            {

                return new HttpResponseMessage(HttpStatusCode.NoContent);

            }



            // Get LUIS response content as string

            var contents = await response.Content.ReadAsStringAsync();

           // log.LogInformation(contents);



            try

            {

                // SQL DATABASE INSERT

                using (SqlConnection con = new SqlConnection(SQLconnectionString))

                {

                    // build up insert statement

                    var insert = "insert into LUIS (Endpoint,Subscription,Application,Query) values " +

                    "('" + LUISendpoint + "'," +

                    "'" + LUISsubscriptionKey + "'," +

                    "'" + LUISappID + "'," +

                    "'" + contents + "')";



                    //log.LogInformation(insert);



                    using (SqlCommand cmd = new SqlCommand(insert, con))

                    {

                        cmd.CommandType = CommandType.Text;

                        con.Open();



                        var countRowsAffected = cmd.ExecuteNonQuery();



                        //log.LogInformation($"processed SQL command successfully; uploaded {countRowsAffected} rows");

                    }

                    return response;

                }

            }

            catch (Exception ex)

            {

                //log.LogInformation(ex.Message);

                return response;

            }

        }
    }
}
