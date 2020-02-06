
# Maxee API
This demo application shows how to retrieve data by using REST API calls.
To limit the number of API calls we need to collect all channel ids for which we want to retrieve the data on a regular basis. Your application should keep track of the channelids and the latest timestamp for which data is retrieved. The next time your app will retrieve the data it will use the channelid and the latesttimestamp. This way you can limit the requests and get your data very fast.

The demo application shows a console menu:
1. Enter API key : here you can enter your api key.
2. Get authorization token (valid for 24 hours) : the application will request an authorization token which will be used for all other API calls. The token will expire within 24 hour. When the token is expired a new token must be requested.
3. Get all channel ids : will show all channels and ids for which your API token has permissions. It will loop through each company, division and devices and it will print out all channels with id's.
4. Get data for a channeldid : retrieve channeldata for a specific channel with id x starting at a certain timestamp.

Once you have listed all channel ids you can start retrieving data by following the steps 1,2 and 4.
1. Enter API key
2. Get authorization token (valid for 24 hours)
4. Get data for a channeldid

In the release map you can download the binaries and you can start exploring the API keys.

# **Example Requests** (using c# RestSharp)

## Request for retrieving data in your script !
For retrieving data in a script or batch job you only need to use the "Get data for channel" api request. In your database you keep track of all channel ids and the latest timestamp for which data is retrieved. Then you user the channelid in combination with the latest timestamp to start retrieving data.

Get data for channel
This sample shows how to retrieve the data for a specific channelid at a specific datetime. You have to loop through the pages until you don't receive any data anymore.
Replace "< enter a channelid>" with a channelid.

Get Data Request
```javascript
var client = new RestClient("https://my.maxee.eu/api/data?sort=timeStamp-desc&page=24&pageSize=20&filter=(channelId~eq~<enter a channelId<~and~timeStamp~gte~datetime'2020-01-05T10-00-00')");
client.Timeout = -1;
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer <enter your sessiontoken here>");
request.AddHeader("Content-Type", "application/json");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```
## Request for getting an authorization token.
This request returns a sessiontoken which is valid for 24 hours. You should use the token in all other requests in the header.
Replace "< enter your sessiontoken here>" with the retrieved sessiontoken.

Get Authorization Token 

```javascript
var client = new RestClient("https://my.maxee.eu/api/Auth/token");
client.Timeout = -1;
var request = new RestRequest(Method.POST);
request.AddHeader("Content-Type", "application/json");
request.AddParameter("application/json", "{\r\n  \"apiKey\": \"xxxx-xxxx-xxxx-xxxx-xxxx\"\r\n}",  ParameterType.RequestBody);
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```
## Requests for retrieving the channel ids.

Get all companies

```javascript
var client = new RestClient("https://my.maxee.eu/api/companies");
client.Timeout = -1;
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer <enter your sessiontoken here>");
request.AddHeader("Content-Type", "application/json");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```

Get all divisions for a company
Replace "< enter a companyid>" with a companyid.

```javascript
var client = new RestClient("https://my.maxee.eu/api/divisions?filter=(companyid~eq~<enter a companyid>)");
client.Timeout = -1;
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer <enter your sessiontoken here>");
request.AddHeader("Content-Type", "application/json");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```

Get all devices
Replace "< enter a divisionid>" with a divisionid.
```javascript
var client = new RestClient("https://my.maxee.eu/api/devices?filter=(divisionid~eq~<enter a divisionid>)");
client.Timeout = -1;
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer <enter your sessiontoken here>");
request.AddHeader("Content-Type", "application/json");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```

Get channels for device
Replace "< enter a deviceid>" with a divisionid.
```javascript
var client = new RestClient("https://my.maxee.eu/api/channels/<enter a deviceid>");
client.Timeout = -1;
var request = new RestRequest(Method.GET);
request.AddHeader("Authorization", "Bearer <enter your sessiontoken here>");
request.AddHeader("Content-Type", "application/json");
IRestResponse response = client.Execute(request);
Console.WriteLine(response.Content);
```


