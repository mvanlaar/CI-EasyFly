using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CI_EasyFly
{
    class EasyFly
    {
        public const string TeletiqueteApi = "api/v1/";
        public const string ua = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
        public const string HeaderAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*;q=0.8";
        Regex rgxIATAAirport = new Regex(@"([A-Z]{3})");

        private static HttpClientHandler handler = new HttpClientHandler();
        private static HttpClient httpClient = new HttpClient(handler);
        //handler.CookieContainer = new CookieContainer();

        private static CookieContainer cookieContainer;
        private static HttpClientHandler clienthandler;
        private static HttpClient client;


        public List<Models.EasyFly.FlightList> GetFlightList(List<Models.EasyFly.Origins> Origens)
        {
            List<Models.EasyFly.FlightList> AirportFlightList = new List<Models.EasyFly.FlightList> { };

            foreach (var Origen in Origens)
            {
                string destinationsairportjson = String.Empty;
                using (var webClient2 = new System.Net.WebClient())
                {
                    webClient2.Headers.Add("user-agent", ua);
                    webClient2.Headers.Add("Referer", "http://easyfly.com.co/");
                    string destinationsurl = "https://easyfly.com.co/home/destinations?originID={0}";
                    destinationsurl = destinationsurl.Replace("{0}", Origen.Id);                    
                    destinationsairportjson = webClient2.DownloadString(destinationsurl);
                }
                // Parse the Response.
                dynamic FlightResponseJson = JObject.Parse(destinationsairportjson);
                foreach (var Destination in FlightResponseJson.data)
                {
                    if (Destination.Value.Key != "-1")
                    {
                        AirportFlightList.Add(new Models.EasyFly.FlightList
                        {   
                            FromName = Origen.Name,
                            FromIATA = Origen.IATA,
                            FromId = Origen.Id,
                            ToName = Destination.Value.Value,
                            ToIATA = Destination.Value.Key,
                            ToId = Destination.Key.ToString()
                        });
                    }
                }
            }

            return AirportFlightList;

            //try
            //{
            //    string urldest = CompagnySite + TeletiqueteApi + "get_ciudades_destino/" + id.ToString() + "/" + CompagnyID.ToString();

            //    var json = urldest.GetJsonFromUrl(req => req.UserAgent = ua);

            //    List<Models.Models.Destinations> Destinations = JsonObject.Parse(json)
            //    .ArrayObjects("content")
            //    .ConvertAll(x => new Models.Models.Destinations
            //    {
            //        Id = x.Get("value"),
            //        Value = x.Get("label")
            //    });

            //    return Destinations;
            //}
            //catch (Exception)
            //{
            //    return new List<Models.Models.Destinations>();
            //}
        }

        public List<Models.EasyFly.Origins> GetOrigens()
        {
            List<Models.EasyFly.Origins> ListOrigens = new List<Models.EasyFly.Origins>();

            cookieContainer = new CookieContainer();
            clienthandler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = true, CookieContainer = cookieContainer };
            client = new HttpClient(clienthandler);

            Uri uri = new Uri("https://www.easyfly.com.co/");
            
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://www.easyfly.com.co/");
            requestMessage.Headers.Add("User-Agent", ua);

            Task<HttpResponseMessage> task = Task.Run(async () => await client.SendAsync(requestMessage));
            task.Wait();
            HttpResponseMessage response = task.Result;

            //HttpResponseMessage response = await httpClient.GetAsync(uri);
            CookieCollection collection = clienthandler.CookieContainer.GetCookies(uri); // Retrieving a Cookie

            var contents = response.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(contents.Result.ToString());
            var nodes = doc.DocumentNode.SelectNodes("//select[@id='lstDestinationMobile']/option");
            foreach (var node in nodes)
            {
                string AirportName = node.InnerText;
                string AirportValue = node.Attributes["value"].Value;
                string IATA = rgxIATAAirport.Match(AirportName).Groups[0].Value;
                AirportName = AirportName.Trim();
                AirportValue = AirportValue.Trim();
                if (AirportValue != "0")
                {
                    ListOrigens.Add(new Models.EasyFly.Origins { Name = HttpUtility.HtmlDecode(AirportName), IATA = IATA, Id = AirportValue });
                }
            }
            return ListOrigens;
        }
        
        public static async Task ProcessUrls(List<Models.EasyFly.FlightList> FlightList)
        {
            var tasks = new List<Task>();
            // semaphore, allow to run 10 tasks in parallel
            using (var semaphore = new SemaphoreSlim(4))
            {
                foreach (var Flight in FlightList)
                {
                    // await here until there is a room for this task
                    await semaphore.WaitAsync();
                    tasks.Add(MakeRequest(semaphore, Flight.FromIATA, Flight.FromId, Flight.FromName, Flight.ToIATA, Flight.ToId, Flight.ToName));
                }
                // await for the rest of tasks to complete
                await Task.WhenAll(tasks);
            }
        }

        public static async Task MakeRequest(SemaphoreSlim semaphore, string FromIATA, string FromId, string FromName, string ToIATA, string ToId, string ToName)
        {

            //cookieContainer = new CookieContainer();
            clienthandler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = true, CookieContainer = cookieContainer };
            client = new HttpClient(clienthandler);


            DateTime dateAndTime = DateTime.Now;
            dateAndTime = dateAndTime.AddDays(7);







            //try
            //{
            DateTime DateTrip = DateTime.Now;
                DateTrip = DateTrip.AddDays(7);
            HttpRequestMessage requestMessageFrontpage = new HttpRequestMessage(HttpMethod.Get, "https://easyfly.com.co/");
            requestMessageFrontpage.Headers.Add("User-Agent", ua);
            requestMessageFrontpage.Headers.Add("Referer", "https://www.google.co/");


            HttpResponseMessage responseFrontpage = await client.SendAsync(requestMessageFrontpage);
            var contentsFrontpage = await responseFrontpage.Content.ReadAsStringAsync();

            // https://www.easyfly.com.co/flights?origins=26&originsText=Cartagena+%28CTG%29&multi=&destinations=24&originsTextReturn=Monter%C3%ADa+%28MTR%29&multiReturn=&flightType=0&departureDateEngine=30-10-2018&returnDateEngine=30-10-2018&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost

            String RequestUrl = "https://easyfly.com.co/flights?origins={0}&originsText={1}&multi=&destinations={2}&originsTextReturn={3}&multiReturn=&flightType=0&departureDateEngine={4}&returnDateEngine={4}&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost";
            RequestUrl = RequestUrl.Replace("{0}", FromId);
            RequestUrl = RequestUrl.Replace("{1}", HttpUtility.UrlEncode(FromName, Encoding.UTF8));
            RequestUrl = RequestUrl.Replace("{2}", ToId);
            RequestUrl = RequestUrl.Replace("{3}", HttpUtility.UrlEncode(ToName, Encoding.UTF8));
            RequestUrl = RequestUrl.Replace("{4}", String.Format("{0:dd-MM-yyyy}", dateAndTime));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, RequestUrl);
            requestMessage.Headers.Add("User-Agent", ua);
            requestMessage.Headers.Add("Referer", "https://easyfly.com.co/");



            //var response = await Policy
            //    .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
            //    .WaitAndRetryAsync(new[]
            //    {
            //        TimeSpan.FromSeconds(5),
            //        TimeSpan.FromSeconds(10),
            //        TimeSpan.FromSeconds(15)
            //    }, (result, timeSpan, retryCount, context) =>
            //    {
            //        ConsoleHelper.WriteLineInColor($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}", ConsoleColor.Yellow);
            //    })
            //    .ExecuteAsync(() => client.GetAsync(RequestUrl));

            //if (response.IsSuccessStatusCode)
            //    ConsoleHelper.WriteLineInColor("Response was successful.", ConsoleColor.Green);
            //else
            //    ConsoleHelper.WriteLineInColor($"Response failed. Status code {response.StatusCode}", ConsoleColor.Red);
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            var contents = await response.Content.ReadAsStringAsync();

            ////string json = await urltrips.GetJsonFromUrlAsync(req => req.UserAgent = ua);

            //// do something with response   
            //List<Models.Models.Teletiquete> basicObjectList = JsonObject.Parse(contents)
            //.Object("content")
            //.ArrayObjects("ida")
            //.ConvertAll(x => new Models.Models.Teletiquete
            //{
            //    internal_id = Guid.NewGuid(),
            //    travel_id = GentravelID(x.Get("operacion"), x.JsonTo<int>("id_empresa")),
            //    company_id = x.JsonTo<int>("id_empresa"),
            //    company_name = CompagnyName,
            //    service_type = x.Get("tipo_vehiculo"),
            //    origin_id = x.Get("id_ciudad_origen"),
            //    origin_name = from_name,
            //    destination_id = x.Get("id_ciudad_destino"),
            //    destination_name = to_name,
            //    departure_date = x.Get("fec_salida_estandar"),
            //    departure_time = x.Get("hora_salida_estandar"),
            //    departure = CreateDateTime(x.Get("fec_salida_estandar"), x.Get("hora_salida_estandar")),
            //    arrival_date = x.Get("fec_llegada_estandar"),
            //    arrival_time = x.Get("hora_llegada_estandar"),
            //    arrival = CreateDateTime(x.Get("fec_llegada_estandar"), x.Get("hora_llegada_estandar")),
            //    price = GenPrice(x.Get("tarifa_online")),
            //    operator_id = x.JsonTo<int>("id_empresa")
            //});

            //if (basicObjectList.Count == 0)
            //{
            //    Console.WriteLine("No route");
            //}
            //else
            //{
            //    Console.WriteLine("Found Route from: {0} to {1}", from_name, to_name);
            //    "Inserting {0} trips into database.".Print(basicObjectList.Count);

            //    System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            //    builder["Data Source"] = "(local)";
            //    builder["Trusted_Connection"] = true;
            //    builder["Initial Catalog"] = "CI-Import";

            //    var dbFactory = new OrmLiteConnectionFactory(builder.ToString(), SqlServerDialect.Provider);

            //    using (var db = dbFactory.Open())
            //    {
            //        db.InsertAll(basicObjectList);
            //    }
            //}

            //}
            //catch (WebException ex)
            //{
            //    //// do something
            //    //var knownError = ex.IsBadRequest()
            //    //|| ex.IsNotFound()
            //    //|| ex.IsUnauthorized()
            //    //|| ex.IsForbidden()
            //    //|| ex.IsInternalServerError();

            //    //var isAnyClientError = ex.IsAny400();
            //    //var isAnyServerError = ex.IsAny500();

            //    //HttpStatusCode? errorStatus = ex.GetStatus();
            //    //string errorBody = ex.GetResponseBody();
            //    //Console.WriteLine("Error: {0} Backup Method", errorStatus.ToString());
            //    //GetTrips(CompagnyID,CompagnyName,CompagnySite,from,to,from_name,to_name);

            //}
            //finally
            //{
            //    // don't forget to release
            semaphore.Release();
            //}
        }
    }
}
