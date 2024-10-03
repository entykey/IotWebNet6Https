namespace IOTWeb.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    

    public class HttpControlService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://113.161.84.132:8081";  // Base URL of the server;
        private Blazored.Toast.ToastParameters _toastParameters = default!;


        public HttpControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Main method to handle control request and send it
        public async Task<int> SendControlRequest(string deviceId, string commandType, string actionType, string requestType, string httpRequestType)
        {
            // Construct the request URL using the provided parameters
            string requestUrl = ConstructUrl(deviceId, commandType, actionType, requestType);

            HttpResponseMessage responseMessage;

            // Perform the required HTTP operation (GET or POST) based on the request type
            switch (httpRequestType.ToUpper())
            {
                case "GET":
                    responseMessage = await SendGetRequest(requestUrl);
                    break;

                case "POST":
                    responseMessage = await SendPostRequest(requestUrl);
                    break;

                default:
                    throw new NotSupportedException($"HTTP request type '{httpRequestType}' is not supported.");
            }

            // Extract the status code from the response message
            int statusCode = (int)responseMessage.StatusCode;

            // Blazored.Toast is not able to trigger show toast from here !!
            // if(statusCode == 200) _toastService.ShowSuccess("Http request success code.");

            // Log the response topic and status code
            string responseTopic = $"/{deviceId}/{commandType}/{actionType}/res";
            // Console.WriteLine($"[HttpCOntrolService] Response Topic: {responseTopic}, Status Code: {statusCode}");

            // Return the HTTP status code
            return statusCode;
        }

        // Construct the URL based on DeviceID, CommandType, ActionType, and RequestType
        private string ConstructUrl(string deviceId, string commandType, string actionType, string requestType)
        {
            return $"{_baseUrl}/{deviceId}/{commandType}/{actionType}/{requestType}";
        }

        // Handle GET request and return HttpResponseMessage
        private async Task<HttpResponseMessage> SendGetRequest(string url)
        {
            return await _httpClient.GetAsync(url);
        }

        // Handle POST request and return HttpResponseMessage
        private async Task<HttpResponseMessage> SendPostRequest(string url)
        {
            var content = new StringContent(""); // Add actual data to be sent in POST request if needed
            return await _httpClient.PostAsync(url, content);
        }
    }
}