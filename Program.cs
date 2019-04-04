using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Maxee.DemoAPIConsole
{
    class Program
    {
        const string apiKeyUser = "enter your api key here";
        const int IndentSize = 4;
        static void Main(string[] args)
        {
            if (apiKeyUser == "enter your api key here")
            {
                Console.WriteLine("Please fill in your api key in the constant apiKeyUser.");
                Console.ReadLine();
                return;
            }
            bool debugDumpJson = false;
            int indentLevel = 1;
            string indentString = string.Empty;


            var client = new RestClient("https://api.maxee.eu");
            var request = new RestRequest("api/Auth/token", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddBody(new { apiKey = apiKeyUser }); // uses JsonSerializer
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            //DumpJsonPrettyFormatted("api/Auth/token", content);

            dynamic input = JsonConvert.DeserializeObject(content);
            //DumpDynamicObjectInfo(input);

            //Token used for all other requests.
            //Token is valid for 24 hours
            string token = input.auth_token;


            //Get data of a channel starting at a certain time
            GetChannelDataOfDeviceStartingAtTimestamp(debugDumpJson, ref indentLevel, ref indentString, client, ref request, ref response, token);

            //Loop through all data
            ShowAllInformation(debugDumpJson, ref indentLevel, ref indentString, client, ref request, ref response, token);

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
        private static void GetChannelDataOfDeviceStartingAtTimestamp(bool debugDumpJson, ref int indentLevel, ref string indentString, RestClient client, ref RestRequest request, ref IRestResponse response, string token)
        {
            int channelId = 572;
            var startDate = DateTime.Now.AddHours(-1);
            var urlStartDate = startDate.ToString("yyyy-MM-ddTHH-mm-ss");
            Console.WriteLine($" ChannelId :{channelId}");

            //get first page of data for channel
            request = new RestRequest($"api/data", Method.GET)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddQueryParameter("sort", "timeStamp-desc");
            request.AddQueryParameter("page", "1");
            request.AddQueryParameter("pageSize", "20");
            request.AddQueryParameter("filter", $"(channelId~eq~{channelId}~and~timestamp~gte~datetime'{urlStartDate}')", false);
            Console.WriteLine(client.BuildUri(request));
            response = client.Execute(request);
            var jsonChannelData = response.Content;
            var maxeeChannelDataList = MaxeeDeviceChannelDataQuickType.MaxeeDeviceChannelDataList.FromJson(jsonChannelData);
            indentLevel = 4;
            indentString = new string(' ', indentLevel * IndentSize);
            int ii = 1;
            foreach (var maxeeChannelData in maxeeChannelDataList.Data)
            {
                Console.WriteLine($"{indentString}{ii} TimeStamp : {maxeeChannelData.TimeStamp}  Value : {maxeeChannelData.Value}");
                ii++;
            }


        }

        private static void ShowAllInformation(bool debugDumpJson, ref int indentLevel, ref string indentString, RestClient client, ref RestRequest request, ref IRestResponse response, string token)
        {
            //Get all companies of users apikey
            var jsonString = ExecuteAPIMethod(client, token, "api/companies", debugDumpJson);
            var maxeeCompanies = MaxeeCompaniesQuickType.MaxeeCompanies.FromJson(jsonString);

            Console.WriteLine("Number of companies retrieved :" + maxeeCompanies.Total);
            int i = 1;
            foreach (var company in maxeeCompanies.Data)
            {
                Console.WriteLine();
                Console.WriteLine($"{i} Company : {company.Name} ({company.Id})");
                Console.WriteLine("=============================================");
                i++;
                //get all divisions for company
                request = new RestRequest("api/divisions", Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddQueryParameter("sort", "name-asc");
                request.AddQueryParameter("page", "1");
                request.AddQueryParameter("pageSize", "20");
                request.AddQueryParameter("filter", $"companyId~eq~{company.Id}");
                response = client.Execute(request);
                var jsonDivisions = response.Content;
                var maxeeDivisions = MaxeeDivisionsQuickType.MaxeeDivisions.FromJson(jsonDivisions);

                indentLevel = 1;
                indentString = new string(' ', indentLevel * IndentSize);
                Console.WriteLine($"{indentString}Number of divisions for company {company.Name} retrieved : {maxeeDivisions.Total}");
                int j = 1;
                foreach (var division in maxeeDivisions.Data)
                {
                    indentLevel = 1;
                    indentString = new string(' ', indentLevel * IndentSize);
                    Console.WriteLine($"{indentString}{j} DivisionName : {division.Name} ({division.Id})");
                    j++;
                    //get all devices for division
                    request = new RestRequest("api/devices", Method.GET)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.AddQueryParameter("sort", "name-asc");
                    request.AddQueryParameter("page", "1");
                    request.AddQueryParameter("pageSize", "20");
                    request.AddQueryParameter("filter", $"divisionId~eq~{division.Id}");
                    response = client.Execute(request);
                    var jsonDevices = response.Content;
                    var maxeeSensors = MaxeeDevicesQuickType.MaxeeDevices.FromJson(jsonDevices);
                    indentLevel = 2;
                    indentString = new string(' ', indentLevel * IndentSize);
                    Console.WriteLine($"{indentString}Number of sensors for division {division.Name} retrieved : {maxeeSensors.Total}");
                    int k = 1;
                    foreach (var maxeeSensor in maxeeSensors.Data)
                    {
                        indentLevel = 2;
                        indentString = new string(' ', indentLevel * IndentSize);
                        Console.WriteLine($"{indentString}{k} SensorName : {maxeeSensor.Name} ({maxeeSensor.Id})");
                        k++;
                        //get all channels for device
                        request = new RestRequest($"api/channels/{maxeeSensor.Id}", Method.GET)
                        {
                            RequestFormat = DataFormat.Json
                        };
                        request.AddHeader("Authorization", "Bearer " + token);
                        request.AddQueryParameter("sort", "name-asc");
                        request.AddQueryParameter("page", "1");
                        request.AddQueryParameter("pageSize", "20");
                        response = client.Execute(request);
                        var jsonChannels = response.Content;
                        var maxeeDeviceChannels = MaxeeDeviceChannelsQuickType.MaxeeDeviceChannels.FromJson(jsonChannels);
                        indentLevel = 3;
                        indentString = new string(' ', indentLevel * IndentSize);
                        Console.WriteLine($"{indentString}Number of channels for device {maxeeSensor.Name} retrieved : {maxeeDeviceChannels.Total}");
                        int l = 1;
                        foreach (var maxeeChannel in maxeeDeviceChannels.Data)
                        {
                            Console.WriteLine($"{indentString}{l} ChannelName : {maxeeChannel.Name} ({maxeeChannel.Id})");
                            l++;
                            //get first page of data for channel
                            request = new RestRequest($"api/data", Method.GET)
                            {
                                RequestFormat = DataFormat.Json
                            };
                            request.AddHeader("Authorization", "Bearer " + token);
                            request.AddQueryParameter("sort", "timeStamp-desc");
                            request.AddQueryParameter("page", "1");
                            request.AddQueryParameter("pageSize", "20");
                            request.AddQueryParameter("filter", $"channelId~eq~{maxeeChannel.Id}");
                            response = client.Execute(request);
                            var jsonChannelData = response.Content;
                            var maxeeChannelDataList = MaxeeDeviceChannelDataQuickType.MaxeeDeviceChannelDataList.FromJson(jsonChannelData);
                            indentLevel = 4;
                            indentString = new string(' ', indentLevel * IndentSize);
                            Console.WriteLine($"{indentString}Number of channels for device {maxeeSensor.Name} retrieved : {maxeeDeviceChannels.Total}");
                            int ii = 1;
                            foreach (var maxeeChannelData in maxeeChannelDataList.Data)
                            {
                                Console.WriteLine($"{indentString}{ii} TimeStamp : {maxeeChannelData.TimeStamp}  Value : {maxeeChannelData.Value}");
                                ii++;
                            }

                        }

                    }

                }
            }
        }

        private static string ExecuteAPIMethod(RestClient client, string token, string apiMethod, bool dumpJson = false)
        {
            var request = new RestRequest(apiMethod, Method.GET)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Authorization", "Bearer " + token);
            IRestResponse response = client.Execute(request);
            if (dumpJson)
                DumpJsonPrettyFormatted(apiMethod, response.Content);
            return response.Content;
        }

        private static void DumpJsonPrettyFormatted(string titleAPI, string jsonContent)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);
                var f = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine();
                Console.WriteLine("===================" + titleAPI);
                Console.WriteLine(f);
            }
            catch (Exception ex)
            {

                Console.WriteLine();
                Console.WriteLine("===================" + titleAPI);
                Console.WriteLine(ex.Message);
            }

        }

        private static void DumpDynamicObjectInfo(dynamic input)
        {
            foreach (string propertyName in GetPropertyKeysForDynamic(input))
            {
                var propertyValue = input[propertyName];
                Console.WriteLine(propertyName + "         " + propertyValue);
            }
        }

        public static List<string> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
        {
            Newtonsoft.Json.Linq.JObject attributesAsJObject = dynamicToGetPropertiesFor;
            Dictionary<string, object> values = attributesAsJObject.ToObject<Dictionary<string, object>>();
            List<string> toReturn = new List<string>();
            foreach (string key in values.Keys)
            {
                toReturn.Add(key);
            }
            return toReturn;
        }



    }
}