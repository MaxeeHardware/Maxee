using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Maxee.DemoAPIConsole
{
    class Program
    {
        static string _apiKeyUser = string.Empty;
        const int _indentSize = 4;
        private const string BaseUrl ="https://api.maxee.eu";
        static string _sessionToken;
        static bool _debugDump = false;

        static void Main(string[] args)
        {
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1) Enter API key. ");
            Console.WriteLine("2) Get authorization token (valid 24 hours). ");
            Console.WriteLine("3) Get all channelids. ");
            Console.WriteLine("4) Get data for channelid.");
            Console.WriteLine("5) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    GetAPIKey();
                    return true;
                case "2":
                    GetAuthorizationToken();
                    return true;
                case "3":
                    GetAllActiveInactiveChannels();
                    return true;
                case "4":
                    GetChannelDataOfDeviceStartingAtTimestamp();
                    return true;
                case "5":
                    return false;
                default:
                    return true;
            }
        }

        static void GetAPIKey()
        {
            Console.Clear();
            Console.WriteLine("Enter your API Key. This key will be used for requesting an authorization token.");
            var apiKey = Console.ReadLine();
            if (!string.IsNullOrEmpty(apiKey)) _apiKeyUser = apiKey;
            Console.WriteLine("Api key is stored.");
            Console.WriteLine("Press any key to return to menu.");
            Console.ReadLine();
        }
        static void GetAuthorizationToken()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("Trying to get an authorization token.");
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("api/Auth/token", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddJsonBody(new { apiKey = _apiKeyUser }); // uses JsonSerializer

                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string
                //if _debugDump is true it shows json content in console
                if (_debugDump) DumpJsonPrettyFormatted("api/Auth/token", content);

                dynamic input = JsonConvert.DeserializeObject(content);
                //if _debugDump is true it shows property information of objects
                if (_debugDump) DumpDynamicObjectInfo(input);

                //Token used for all other requests. Token is valid for 24 hours
                _sessionToken = input.auth_token;

                Console.WriteLine("Session token retrieved, press any key to return to menu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to return to menu.");
            }
            Console.ReadLine();
        }

        private static void GetChannelDataOfDeviceStartingAtTimestamp()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("Session token is valid for 24 hours. Be sure you have refreshed session token if needed.");
                Console.WriteLine("Enter the channelid:");
                int channelId = Convert.ToInt32(Console.ReadLine());


                var minStartDate = DateTime.Now.AddMonths(-2);
                DateTime startTimestamp;

                Console.WriteLine($"Enter startdate to retrieve data. Minimumdate is {minStartDate.ToString("yyyy-MM-dd HH:mm:ss")}");
                if (!DateTime.TryParse(Console.ReadLine(), out startTimestamp))
                {
                    Console.WriteLine("Invalid date, please enter a valid date (yyyy-MM-dd HH:mm:ss), fe 2019-10-30 10:15:00");
                }
                else
                {
                    if (startTimestamp < minStartDate)
                    {
                        Console.WriteLine($"The startdate must be greater than the minimumstardate {minStartDate.ToString("yyyy-MM-dd HH:mm:ss")}");
                    }
                    else
                    {
                        RestRequest request = null;
                        IRestResponse response = null;
                        int indentLevel = 1;
                        string indentString = string.Empty;

                        string timestampFilter = string.Empty;
                        timestampFilter = $"timeStamp~gte~datetime'{startTimestamp.ToString("yyyy-MM-ddTHH-mm-ss")}'";

                        Console.WriteLine($"\r\nRetrieving data for channel with id {channelId}, pagesize=20");

                        var client = new RestClient(BaseUrl);
                        long count = 0;
                        int pagenumber = 1;
                        //We should use a pagesize for retrieving data. Then we need to repeat the request until no data is retrieved
                        do
                        {
                            count = 0;
                            //get first page of data for channel
                            request = new RestRequest($"api/data", Method.GET)
                            {
                                RequestFormat = DataFormat.Json
                            };
                            request.AddHeader("Authorization", "Bearer " + _sessionToken);
                            request.AddQueryParameter("sort", "timeStamp-desc");
                            request.AddQueryParameter("page", pagenumber.ToString());
                            request.AddQueryParameter("pageSize", "20");
                            //            request.AddQueryParameter("filter", $"(channelId~eq~{channelId}~and~timestamp~gte~datetime'{urlStartDate}')", false);
                            request.AddParameter("filter", $"(channelId~eq~{channelId}{(string.IsNullOrWhiteSpace(timestampFilter) ? string.Empty : "~and~" + timestampFilter)})");

                            //if _debugDump is true it shows the REST API url
                            if (_debugDump) Console.WriteLine(client.BuildUri(request));
                            response = client.Execute(request);

                            if (response.IsSuccessful)
                            {
                                var jsonChannelData = response.Content;

                                var maxeeChannelDataList = MaxeeDeviceChannelDataQuickType.MaxeeDeviceChannelDataList.FromJson(jsonChannelData);
                                pagenumber++;
                                indentLevel = 4;
                                indentString = new string(' ', indentLevel * _indentSize);
                                int ii = 1;
                                foreach (var maxeeChannelData in maxeeChannelDataList.Data)
                                {
                                    Console.WriteLine($"{indentString}{pagenumber}/{ii} TimeStamp : {maxeeChannelData.TimeStamp}  Value : {maxeeChannelData.Value}");
                                    ii++;
                                    count++;
                                }
                            }
                            else
                            {
                                throw new ApplicationException($"Could not retrieve data, statuscode: {response.StatusCode.ToString()}");
                            }

                        } while (count > 0);
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key to return to menu.");
            Console.ReadLine();
        }

        private static void GetAllActiveInactiveChannels()
        {
            //Get all companies of users apikey
            try
            {
                Console.Clear();
                RestRequest request = null;
                IRestResponse response = null;
                int indentLevel;
                string indentString;

                var client = new RestClient(BaseUrl);

                //Get all companies of users apikey
                var jsonString = ExecuteAPIMethod(client, _sessionToken, "api/companies");
                var maxeeCompanies = MaxeeCompaniesQuickType.MaxeeCompanies.FromJson(jsonString);


                Console.WriteLine("Number of companies retrieved :" + maxeeCompanies.Total);
                int i = 1;
                foreach (var company in maxeeCompanies.Data)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{i} Company : {company.Name} (id={company.Id})");
                    Console.WriteLine("=============================================");
                    i++;
                    //get all divisions for company
                    request = new RestRequest("api/divisions", Method.GET)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader("Authorization", "Bearer " + _sessionToken);
                    request.AddQueryParameter("sort", "name-asc");
                    request.AddQueryParameter("page", "1");
                    request.AddQueryParameter("pageSize", "20");
                    request.AddQueryParameter("filter", $"companyId~eq~{company.Id}");
                    response = client.Execute(request);
                    var jsonDivisions = response.Content;
                    var maxeeDivisions = MaxeeDivisionsQuickType.MaxeeDivisions.FromJson(jsonDivisions);

                    indentLevel = 1;
                    indentString = new string(' ', indentLevel * _indentSize);
                    Console.WriteLine($"{indentString}Number of divisions for company {company.Name} retrieved : {maxeeDivisions.Total}");
                    int j = 1;
                    foreach (var division in maxeeDivisions.Data)
                    {
                        indentLevel = 1;
                        indentString = new string(' ', indentLevel * _indentSize);
                        Console.WriteLine($"{indentString}{j} DivisionName : {division.Name} (id={division.Id})");
                        j++;
                        //get all devices for division
                        request = new RestRequest("api/devices", Method.GET)
                        {
                            RequestFormat = DataFormat.Json
                        };
                        request.AddHeader("Authorization", "Bearer " + _sessionToken);
                        request.AddQueryParameter("sort", "name-asc");
                        request.AddQueryParameter("page", "1");
                        request.AddQueryParameter("pageSize", "20");
                        request.AddQueryParameter("filter", $"divisionId~eq~{division.Id}");
                        Console.WriteLine(client.BuildUri(request));
                        response = client.Execute(request);

                        var jsonDevices = response.Content;
                        var maxeeSensors = MaxeeDevicesQuickType.MaxeeDevices.FromJson(jsonDevices);
                        indentLevel = 2;
                        indentString = new string(' ', indentLevel * _indentSize);
                        Console.WriteLine($"{indentString}Number of sensors for division {division.Name} retrieved : {maxeeSensors.Total}");
                        int k = 1;
                        foreach (var maxeeSensor in maxeeSensors.Data)
                        {
                            indentLevel = 2;
                            indentString = new string(' ', indentLevel * _indentSize);

                            //get all active channels for device
                            Console.WriteLine($"{indentString}{k} SensorName : {maxeeSensor.Name} (id={maxeeSensor.Id}) ");
                            
                            indentLevel = 3;
                            indentString = new string(' ', indentLevel * _indentSize);

                            Console.WriteLine($"{indentString}ACTIVE CHANNELS");
                            request = new RestRequest($"api/channels/GetActiveChannels/{maxeeSensor.Id}", Method.GET)
                            {
                                RequestFormat = DataFormat.Json
                            };
                            request.AddHeader("Authorization", "Bearer " + _sessionToken);
                            request.AddQueryParameter("sort", "name-asc");
                            request.AddQueryParameter("page", "1");
                            request.AddQueryParameter("pageSize", "20");
                            response = client.Execute(request);
                            var jsonChannels = response.Content;
                            var maxeeDeviceChannels = MaxeeDeviceChannelsQuickType.MaxeeDeviceChannels.FromJson(jsonChannels);
                            int l = 1;
                            indentLevel = 4;
                            indentString = new string(' ', indentLevel * _indentSize);
                            if (maxeeDeviceChannels != null)
                            {
                                Console.WriteLine($"{indentString}Number of ACTIVE channels for device {maxeeSensor.Name} retrieved : {maxeeDeviceChannels.Total}");
                                l = 1;
                                foreach (var maxeeChannel in maxeeDeviceChannels.Data)
                                {
                                    Console.WriteLine($"{indentString}{l} ChannelName : {maxeeChannel.Name} (id={maxeeChannel.Id})");
                                    l++;
                                }
                            }
                            else
                                Console.WriteLine($"{indentString}Number of NO ACTIVE channels for device {maxeeSensor.Name}.");
                            //get all inactive channels for device
                            indentLevel = 3;
                            indentString = new string(' ', indentLevel * _indentSize);
                            Console.WriteLine($"{indentString}INACTIVE CHANNELS");
                            request = new RestRequest($"api/channels/getinactivechannels/{maxeeSensor.Id}", Method.GET)
                            {
                                RequestFormat = DataFormat.Json
                            };
                            request.AddHeader("Authorization", "Bearer " + _sessionToken);
                            request.AddQueryParameter("sort", "name-asc");
                            request.AddQueryParameter("page", "1");
                            request.AddQueryParameter("pageSize", "20");
                            response = client.Execute(request);
                            jsonChannels = response.Content;
                            maxeeDeviceChannels = MaxeeDeviceChannelsQuickType.MaxeeDeviceChannels.FromJson(jsonChannels);
                            indentLevel = 4;
                            indentString = new string(' ', indentLevel * _indentSize);
                            if (maxeeDeviceChannels != null)
                            {
                                Console.WriteLine($"{indentString}Number of INACTIVE channels for device {maxeeSensor.Name} retrieved : {maxeeDeviceChannels.Total}");
                                l = 1;
                                foreach (var maxeeChannel in maxeeDeviceChannels.Data)
                                {
                                    Console.WriteLine($"{indentString}{l} ChannelName : {maxeeChannel.Name} (id={maxeeChannel.Id})");
                                    l++;
                                }
                            }
                            else
                                Console.WriteLine($"{indentString}Number of NO INACTIVE channels for device {maxeeSensor.Name}.");
                            k++;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key to return to menu.");
            Console.ReadLine();
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
            if (response.IsSuccessful)
                return response.Content;
            else
            {
                throw new ApplicationException($"Could not retrieve data, statuscode: {response.StatusCode.ToString()}");
            }
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
                    Console.WriteLine("===================" );
                }
                catch (Exception ex)
                {

                    Console.WriteLine();
                    Console.WriteLine("===================" + titleAPI);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("===================");
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