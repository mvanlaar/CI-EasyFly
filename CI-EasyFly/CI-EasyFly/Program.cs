using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using CsvHelper;
using Newtonsoft.Json;
using System.Configuration;
using System.IO.Compression;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CI_EasyFly
{
    public class Program
    {
        static void Main(string[] args)
        {
            string APIPathAirport = "airport/iata/";
            string APIPathAirline = "airline/iata/";
            const string ua = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            const string HeaderAccept = "text/html,application/xhtml+xml,application/xml;q=0.9,*;q=0.8";

            Regex rgxIATAAirport = new Regex(@"([A-Z]{3})");
            List<AirportDef> _Airports = new List<AirportDef> { };
            List<CIFLight> CIFLights = new List<CIFLight> { };
            //Console.WriteLine("Parsing airport: {0}", Airport);
            string Frontpage = String.Empty;
            using (var webClient1 = new System.Net.WebClient())
            {
                webClient1.Headers.Add("user-agent", ua);
                //webClient1.Headers.Add("Referer", "http://easyfly.com.co/");
                //string destinationsurl = "http://easyfly.com.co/home/destinations?originID={0}";
                //Frontpage = destinationsurl.Replace("{0}", AirportFrom.Value);
                Frontpage = webClient1.DownloadString("http://easyfly.com.co/");
            }            
            HtmlDocument doc = new HtmlDocument();
            
            doc.LoadHtml(Frontpage);
            var nodes = doc.DocumentNode.SelectNodes("//select[@id='origins']/option");
            foreach (var node in nodes)
            {
                string AirportName = node.NextSibling.InnerText;
                string AirportValue = node.Attributes["value"].Value;
                string IATA = rgxIATAAirport.Match(AirportName).Groups[0].Value;
                AirportName = AirportName.Trim();
                AirportValue = AirportValue.Trim();
                if (AirportValue != "0")
                {
                    _Airports.Add(new AirportDef { Name = HttpUtility.HtmlDecode(AirportName), IATA = IATA, Value = AirportValue });
                }
            }
            foreach (var AirportFrom in _Airports)
            {
                List<AirportDef> AirportToList = new List<AirportDef> { };
                
                string destinationsairportjson = String.Empty;
                using (var webClient1 = new System.Net.WebClient())
                {
                    webClient1.Headers.Add("user-agent", ua);
                    webClient1.Headers.Add("Referer", "http://easyfly.com.co/");
                    string destinationsurl = "http://easyfly.com.co/home/destinations?originID={0}";
                    destinationsurl = destinationsurl.Replace("{0}", AirportFrom.Value);
                    destinationsairportjson = webClient1.DownloadString(destinationsurl);
                }                
                // Parse the Response.
                dynamic FlightResponseJson = JObject.Parse(destinationsairportjson);
                foreach (var Destination in FlightResponseJson.data)
                {
                    AirportToList.Add(new AirportDef { Name = Destination.Value.Value, IATA = Destination.Value.Key, Value = Destination.Key.ToString() });
                }

                int FromDay = Convert.ToInt32(ConfigurationManager.AppSettings.Get("FromDay"));
                int ToDay = Convert.ToInt32(ConfigurationManager.AppSettings.Get("ToDay"));


                foreach (var AirportTo in AirportToList)
                {
                    Parallel.For(FromDay, ToDay, new ParallelOptions { MaxDegreeOfParallelism = 5 }, (Day) =>
                    {
                        DateTime dateAndTime = DateTime.Now;
                        dateAndTime = dateAndTime.AddDays(Day);
                        BrowserSession b = new BrowserSession();
                        // Console.WriteLine("Getting Session and cookies...");
                        b.Get("http://easyfly.com.co/");
                        Console.WriteLine("{0} - {1} - {2}", AirportFrom.IATA, AirportTo.IATA, dateAndTime.ToShortDateString());
                        //string AirportResponse = b.Get("http://easyfly.com.co/flights?origins=24&originsText=Monter%C3%ADa+%28MTR%29&multi=&destinations=20&originsTextReturn=Medell%C3%ADn%2C+E+Olaya+H.+%28EOH%29&multiReturn=&flightType=0&departureDateEngine=25-01-2017&returnDateEngine=25-01-2017&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS");
                        String RequestUrl = "http://easyfly.com.co/flights?origins={0}&originsText={1}&multi=&destinations={2}&originsTextReturn={3}&multiReturn=&flightType=0&departureDateEngine={4}&returnDateEngine={4}&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS";
                        RequestUrl = RequestUrl.Replace("{0}", AirportFrom.Value);
                        RequestUrl = RequestUrl.Replace("{1}", HttpUtility.UrlEncode(AirportFrom.Name, Encoding.UTF8));
                        RequestUrl = RequestUrl.Replace("{2}", AirportTo.Value);
                        RequestUrl = RequestUrl.Replace("{3}", HttpUtility.UrlEncode(AirportTo.Name, Encoding.UTF8));
                        RequestUrl = RequestUrl.Replace("{4}", String.Format("{0:dd-MM-yyyy}", dateAndTime));
                        string AirportResponse = b.Get(RequestUrl);
                        //Console.WriteLine("Parsing Response...");
                        string TEMP_FromIATA = null;
                        string TEMP_ToIATA = null;
                        DateTime TEMP_ValidFrom = new DateTime();
                        DateTime TEMP_ValidTo = new DateTime();

                        Boolean TEMP_FlightMonday = false;
                        Boolean TEMP_FlightTuesday = false;
                        Boolean TEMP_FlightWednesday = false;
                        Boolean TEMP_FlightThursday = false;
                        Boolean TEMP_FlightFriday = false;
                        Boolean TEMP_FlightSaterday = false;
                        Boolean TEMP_FlightSunday = false;
                        DateTime TEMP_DepartTime = new DateTime();
                        DateTime TEMP_ArrivalTime = new DateTime();
                        Boolean TEMP_FlightCodeShare = false;
                        string TEMP_FlightNumber = String.Empty;
                        string TEMP_Aircraftcode = String.Empty;
                        Boolean TEMP_FlightNextDayArrival = false;
                        int TEMP_FlightNextDays = 0;

                        HtmlDocument docresponse = new HtmlDocument();
                        docresponse.LoadHtml(AirportResponse);
                        var flights = docresponse.DocumentNode.SelectNodes("//div[@class='large-12 columns']//div[@class='large-6 columns']");
                        if (flights != null)
                        {
                            foreach (HtmlNode flightitem in flights)
                            {
                                var inDirectFlights = flightitem.Descendants("./a[@class='stepConnection']").Any();
                                var popUpClose = flightitem.SelectSingleNode("./a[@class='popClose']");
                                var ResumedeVuelo = flightitem.InnerHtml.Contains("Resumen del vuelo");
                                if (!inDirectFlights & !ResumedeVuelo & popUpClose == null)
                                {
                                    TEMP_FromIATA = flightitem.SelectSingleNode("./div[1]/div[1]/span[2]").InnerText.ToString();
                                    TEMP_ToIATA = flightitem.SelectSingleNode("./div[1]/div[3]/span[2]").InnerText.ToString();
                                    TEMP_DepartTime = Convert.ToDateTime(flightitem.SelectSingleNode("./div[1]/div[1]/span[1]").InnerText.ToString());
                                    TEMP_ArrivalTime = Convert.ToDateTime(flightitem.SelectSingleNode("./div[1]/div[3]/span[1]").InnerText.ToString());
                                    TEMP_FlightNumber = flightitem.SelectSingleNode("./div[1]/div[4]/span[2]").InnerText.ToString();
                                    TEMP_FlightNumber = TEMP_FlightNumber.Replace("EF ", "");
                                    TEMP_ValidFrom = dateAndTime.Date;
                                    TEMP_ValidTo = dateAndTime.Date;
                                    int dayofweek = Convert.ToInt32(dateAndTime.DayOfWeek);
                                    if (dayofweek == 0) { TEMP_FlightSunday = true; }
                                    if (dayofweek == 1) { TEMP_FlightMonday = true; }
                                    if (dayofweek == 2) { TEMP_FlightTuesday = true; }
                                    if (dayofweek == 3) { TEMP_FlightWednesday = true; }
                                    if (dayofweek == 4) { TEMP_FlightThursday = true; }
                                    if (dayofweek == 5) { TEMP_FlightFriday = true; }
                                    if (dayofweek == 6) { TEMP_FlightSaterday = true; }
                                    bool alreadyExists = CIFLights.Exists(x => x.FromIATA == TEMP_FromIATA
                                                    && x.ToIATA == TEMP_ToIATA
                                                    && x.FromDate == TEMP_ValidFrom
                                                    && x.ToDate == TEMP_ValidTo
                                                    && x.FlightNumber == TEMP_FlightNumber
                                                    && x.FlightAirline == "VE"
                                                    && x.FlightMonday == TEMP_FlightMonday
                                                    && x.FlightTuesday == TEMP_FlightTuesday
                                                    && x.FlightWednesday == TEMP_FlightWednesday
                                                    && x.FlightThursday == TEMP_FlightThursday
                                                    && x.FlightFriday == TEMP_FlightFriday
                                                    && x.FlightSaterday == TEMP_FlightSaterday
                                                    && x.FlightSunday == TEMP_FlightSunday);


                                    if (!alreadyExists)
                                    {
                                        CIFLights.Add(new CIFLight
                                        {
                                            FromIATA = TEMP_FromIATA,
                                            ToIATA = TEMP_ToIATA,
                                            FromDate = TEMP_ValidFrom,
                                            ToDate = TEMP_ValidTo,
                                            ArrivalTime = TEMP_ArrivalTime,
                                            DepartTime = TEMP_DepartTime,
                                            FlightAircraft = String.Empty,
                                            FlightAirline = "VE",
                                            FlightMonday = TEMP_FlightMonday,
                                            FlightTuesday = TEMP_FlightTuesday,
                                            FlightWednesday = TEMP_FlightWednesday,
                                            FlightThursday = TEMP_FlightThursday,
                                            FlightFriday = TEMP_FlightFriday,
                                            FlightSaterday = TEMP_FlightSaterday,
                                            FlightSunday = TEMP_FlightSunday,
                                            FlightNumber = TEMP_FlightNumber,
                                            FlightOperator = String.Empty,
                                            FlightDuration = String.Empty,
                                            FlightCodeShare = TEMP_FlightCodeShare,
                                            FlightNextDayArrival = TEMP_FlightNextDayArrival,
                                            FlightNextDays = TEMP_FlightNextDays,
                                            FlightNonStop = true,
                                            FlightVia = String.Empty
                                        });
                                        // End Duplicate checking
                                    }
                                    // End Onyl direct flights parsing
                                }
                                // End Flight item parsing
                            }
                            // End flight check
                        }
                    // End Parrallel parsing
                    });
                }
            }
            

            // You'll do something else with it, here I write it to a console window
            // Console.WriteLine(text.ToString());
            Console.WriteLine("Insert into XML...");
            // Write the list of objects to a file.
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(CIFLights.GetType());
            string myDir = AppDomain.CurrentDomain.BaseDirectory + "\\output";
            System.IO.Directory.CreateDirectory(myDir);

            System.IO.StreamWriter file =
                new System.IO.StreamWriter("output\\output.xml");

            writer.Serialize(file, CIFLights);
            file.Close();

            string gtfsDir = AppDomain.CurrentDomain.BaseDirectory + "\\gtfs";
            System.IO.Directory.CreateDirectory(gtfsDir);

            Console.WriteLine("Creating GTFS Files...");

            Console.WriteLine("Creating GTFS File agency.txt...");
            using (var gtfsagency = new StreamWriter(@"gtfs\\agency.txt"))
            {
                var csv = new CsvWriter(gtfsagency);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.Encoding = Encoding.UTF8;
                csv.Configuration.TrimFields = true;
                // header 
                csv.WriteField("agency_id");
                csv.WriteField("agency_name");
                csv.WriteField("agency_url");
                csv.WriteField("agency_timezone");
                csv.WriteField("agency_lang");
                csv.WriteField("agency_phone");
                csv.WriteField("agency_fare_url");
                csv.WriteField("agency_email");
                csv.NextRecord();

                var airlines = CIFLights.Select(m => new { m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < airlines.Count; i++) // Loop through List with for)
                {
                    string urlapi = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirline + airlines[0].FlightAirline.Trim();
                    string RequestAirlineJson = String.Empty;
                    HttpWebRequest requestAirline = (HttpWebRequest)WebRequest.Create(urlapi);
                    requestAirline.Method = "GET";
                    requestAirline.UserAgent = ua;
                    requestAirline.Accept = HeaderAccept;
                    requestAirline.Proxy = null;
                    requestAirline.KeepAlive = false;
                    using (HttpWebResponse Airlineresponse = (HttpWebResponse)requestAirline.GetResponse())
                    using (StreamReader reader = new StreamReader(Airlineresponse.GetResponseStream()))
                    {
                        RequestAirlineJson = reader.ReadToEnd();
                    }
                    dynamic AirlineResponseJson = JsonConvert.DeserializeObject(RequestAirlineJson);
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].code));
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].name));
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].website));
                    csv.WriteField("America/Bogota");
                    csv.WriteField("ES");
                    csv.WriteField(Convert.ToString(AirlineResponseJson[0].phone));
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.NextRecord();
                }
            }

            Console.WriteLine("Creating GTFS File routes.txt ...");

            using (var gtfsroutes = new StreamWriter(@"gtfs\\routes.txt"))
            {
                // Route record


                var csvroutes = new CsvWriter(gtfsroutes);
                csvroutes.Configuration.Delimiter = ",";
                csvroutes.Configuration.Encoding = Encoding.UTF8;
                csvroutes.Configuration.TrimFields = true;
                // header 
                csvroutes.WriteField("route_id");
                csvroutes.WriteField("agency_id");
                csvroutes.WriteField("route_short_name");
                csvroutes.WriteField("route_long_name");
                csvroutes.WriteField("route_desc");
                csvroutes.WriteField("route_type");
                csvroutes.WriteField("route_url");
                csvroutes.WriteField("route_color");
                csvroutes.WriteField("route_text_color");
                csvroutes.NextRecord();


                var routes = CIFLights.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline, m.FlightNumber }).Distinct().ToList();

                //for (int j = 0; j < routes.Count; j++)
                //{
                //    int FlightNumberOrg = Convert.ToInt16(routes[j].FlightNumber);
                //    if (IsEven(FlightNumberOrg))
                //    {
                //        // This is the flight from te base station
                //        // So the return flight in part of this route.
                //        // Return flight is flightnumber + 1
                //        int ReturnFlight = FlightNumberOrg + 1;
                //        routes.Remove(routes.Find(c => c.FromIATA == routes[j].ToIATA && c.ToIATA == routes[j].FromIATA && c.FlightAirline == routes[j].FlightAirline && c.FlightNumber == Convert.ToString(ReturnFlight)));                            
                //    }
                //    // Need to rework special cases like the ist - bog - pty - ist flight nr 800
                //}
                var routesdist = routes.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline }).Distinct().ToList();
                //var routes = CIFLights.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < routesdist.Count; i++) // Loop through List with for)
                {
                    string FromAirportName = null;
                    string ToAirportName = null;
                    string FromAirportCountry = null;
                    string FromAirportContinent = null;
                    string ToAirportCountry = null;
                    string ToAirportContinent = null;

                    using (var clientFrom = new WebClient())
                    {
                        clientFrom.Encoding = Encoding.UTF8;
                        clientFrom.Headers.Add("user-agent", ua);
                        string urlapiFrom = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + routesdist[i].FromIATA;
                        var jsonapiFrom = clientFrom.DownloadString(urlapiFrom);
                        dynamic AirportResponseJsonFrom = JsonConvert.DeserializeObject(jsonapiFrom);
                        FromAirportName = Convert.ToString(AirportResponseJsonFrom[0].name);
                        FromAirportCountry = Convert.ToString(AirportResponseJsonFrom[0].country_code);
                        FromAirportContinent = Convert.ToString(AirportResponseJsonFrom[0].continent);
                    }
                    using (var clientTo = new WebClient())
                    {
                        clientTo.Encoding = Encoding.UTF8;
                        clientTo.Headers.Add("user-agent", ua);
                        string urlapiTo = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + routesdist[i].ToIATA;
                        var jsonapiTo = clientTo.DownloadString(urlapiTo);
                        dynamic AirportResponseJsonTo = JsonConvert.DeserializeObject(jsonapiTo);
                        ToAirportName = Convert.ToString(AirportResponseJsonTo[0].name);
                        ToAirportCountry = Convert.ToString(AirportResponseJsonTo[0].country_code);
                        ToAirportContinent = Convert.ToString(AirportResponseJsonTo[0].continent);
                    }

                    csvroutes.WriteField(routesdist[i].FromIATA + routesdist[i].ToIATA);
                    csvroutes.WriteField(routesdist[i].FlightAirline);
                    csvroutes.WriteField(routesdist[i].FromIATA + routesdist[i].ToIATA);
                    csvroutes.WriteField(FromAirportName + " - " + ToAirportName);
                    csvroutes.WriteField(""); // routes[i].FlightAircraft + ";" + CIFLights[i].FlightAirline + ";" + CIFLights[i].FlightOperator + ";" + CIFLights[i].FlightCodeShare
                    if (FromAirportCountry == ToAirportCountry)
                    {
                        // Colombian internal flight domestic
                        csvroutes.WriteField(1102);
                    }
                    else
                    {
                        if (FromAirportContinent == ToAirportContinent)
                        {
                            // International Flight
                            csvroutes.WriteField(1101);
                        }
                        else
                        {
                            // Intercontinental Flight
                            csvroutes.WriteField(1103);
                        }
                    }
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.NextRecord();
                }
            }

            // stops.txt

            List<string> agencyairportsiata =
             CIFLights.SelectMany(m => new string[] { m.FromIATA, m.ToIATA })
                     .Distinct()
                     .ToList();

            using (var gtfsstops = new StreamWriter(@"gtfs\\stops.txt"))
            {
                // Route record
                var csvstops = new CsvWriter(gtfsstops);
                csvstops.Configuration.Delimiter = ",";
                csvstops.Configuration.Encoding = Encoding.UTF8;
                csvstops.Configuration.TrimFields = true;
                // header                                 
                csvstops.WriteField("stop_id");
                csvstops.WriteField("stop_name");
                csvstops.WriteField("stop_desc");
                csvstops.WriteField("stop_lat");
                csvstops.WriteField("stop_lon");
                csvstops.WriteField("zone_id");
                csvstops.WriteField("stop_url");
                csvstops.WriteField("stop_timezone");
                csvstops.NextRecord();

                for (int i = 0; i < agencyairportsiata.Count; i++) // Loop through List with for)
                {
                    // Using API for airport Data.
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        string urlapi = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + agencyairportsiata[i];
                        var jsonapi = client.DownloadString(urlapi);
                        dynamic AirportResponseJson = JsonConvert.DeserializeObject(jsonapi);

                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].code));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].name));
                        csvstops.WriteField("");
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].lat));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].lng));
                        csvstops.WriteField("");
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].website));
                        csvstops.WriteField(Convert.ToString(AirportResponseJson[0].timezone));
                        csvstops.NextRecord();
                    }
                }
            }


            Console.WriteLine("Creating GTFS File trips.txt, stop_times.txt, calendar.txt ...");

            using (var gtfscalendar = new StreamWriter(@"gtfs\\calendar.txt"))
            {
                using (var gtfstrips = new StreamWriter(@"gtfs\\trips.txt"))
                {
                    using (var gtfsstoptimes = new StreamWriter(@"gtfs\\stop_times.txt"))
                    {
                        // Headers 
                        var csvstoptimes = new CsvWriter(gtfsstoptimes);
                        csvstoptimes.Configuration.Delimiter = ",";
                        csvstoptimes.Configuration.Encoding = Encoding.UTF8;
                        csvstoptimes.Configuration.TrimFields = true;
                        // header 
                        csvstoptimes.WriteField("trip_id");
                        csvstoptimes.WriteField("arrival_time");
                        csvstoptimes.WriteField("departure_time");
                        csvstoptimes.WriteField("stop_id");
                        csvstoptimes.WriteField("stop_sequence");
                        csvstoptimes.WriteField("stop_headsign");
                        csvstoptimes.WriteField("pickup_type");
                        csvstoptimes.WriteField("drop_off_type");
                        csvstoptimes.WriteField("shape_dist_traveled");
                        csvstoptimes.WriteField("timepoint");
                        csvstoptimes.NextRecord();

                        var csvtrips = new CsvWriter(gtfstrips);
                        csvtrips.Configuration.Delimiter = ",";
                        csvtrips.Configuration.Encoding = Encoding.UTF8;
                        csvtrips.Configuration.TrimFields = true;
                        // header 
                        csvtrips.WriteField("route_id");
                        csvtrips.WriteField("service_id");
                        csvtrips.WriteField("trip_id");
                        csvtrips.WriteField("trip_headsign");
                        csvtrips.WriteField("trip_short_name");
                        csvtrips.WriteField("direction_id");
                        csvtrips.WriteField("block_id");
                        csvtrips.WriteField("shape_id");
                        csvtrips.WriteField("wheelchair_accessible");
                        csvtrips.WriteField("bikes_allowed ");
                        csvtrips.NextRecord();

                        var csvcalendar = new CsvWriter(gtfscalendar);
                        csvcalendar.Configuration.Delimiter = ",";
                        csvcalendar.Configuration.Encoding = Encoding.UTF8;
                        csvcalendar.Configuration.TrimFields = true;
                        // header 
                        csvcalendar.WriteField("service_id");
                        csvcalendar.WriteField("monday");
                        csvcalendar.WriteField("tuesday");
                        csvcalendar.WriteField("wednesday");
                        csvcalendar.WriteField("thursday");
                        csvcalendar.WriteField("friday");
                        csvcalendar.WriteField("saturday");
                        csvcalendar.WriteField("sunday");
                        csvcalendar.WriteField("start_date");
                        csvcalendar.WriteField("end_date");
                        csvcalendar.NextRecord();

                        //1101 International Air Service
                        //1102 Domestic Air Service
                        //1103 Intercontinental Air Service
                        //1104 Domestic Scheduled Air Service


                        for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
                        {

                            // Calender

                            csvcalendar.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightMonday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightTuesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightWednesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightThursday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightFriday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSaterday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvcalendar.NextRecord();

                            // Trips
                            string FromAirportName = null;
                            string ToAirportName = null;
                            using (var client = new WebClient())
                            {
                                client.Encoding = Encoding.UTF8;
                                string urlapi = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + CIFLights[i].FromIATA;
                                var jsonapi = client.DownloadString(urlapi);
                                dynamic AirportResponseJson = JsonConvert.DeserializeObject(jsonapi);
                                FromAirportName = Convert.ToString(AirportResponseJson[0].name);
                            }
                            using (var client = new WebClient())
                            {
                                client.Encoding = Encoding.UTF8;
                                string urlapi = ConfigurationManager.AppSettings.Get("APIUrl") + APIPathAirport + CIFLights[i].ToIATA;
                                var jsonapi = client.DownloadString(urlapi);
                                dynamic AirportResponseJson = JsonConvert.DeserializeObject(jsonapi);
                                ToAirportName = Convert.ToString(AirportResponseJson[0].name);
                            }
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA);
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvtrips.WriteField(ToAirportName);
                            csvtrips.WriteField(CIFLights[i].FlightNumber);
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("1");
                            csvtrips.WriteField("");
                            csvtrips.NextRecord();

                            // Depart Record
                            csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(CIFLights[i].FromIATA);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("");
                            csvstoptimes.NextRecord();
                            if (!CIFLights[i].FlightNonStop)
                            {
                                // Non Direct flight...
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField(CIFLights[i].FlightVia);
                                csvstoptimes.WriteField("1");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }

                            // Arrival Record
                            if (!CIFLights[i].FlightNextDayArrival)
                            {
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                            else
                            {
                                //add 24 hour for the gtfs time
                                int hour = CIFLights[i].ArrivalTime.Hour;
                                hour = hour + 24;
                                int minute = CIFLights[i].ArrivalTime.Minute;
                                string strminute = minute.ToString();
                                if (strminute.Length == 1) { strminute = "0" + strminute; }
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate) + Convert.ToInt32(CIFLights[i].FlightMonday) + Convert.ToInt32(CIFLights[i].FlightTuesday) + Convert.ToInt32(CIFLights[i].FlightWednesday) + Convert.ToInt32(CIFLights[i].FlightThursday) + Convert.ToInt32(CIFLights[i].FlightFriday) + Convert.ToInt32(CIFLights[i].FlightSaterday) + Convert.ToInt32(CIFLights[i].FlightSunday));
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                        }
                    }
                }
            }

            // Create Zip File
            string startPath = gtfsDir;
            string zipPath = myDir + "\\EasyFly.zip";
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, false);
        }

        public class AirportDef
        {
            // Auto-implemented properties.  
            public string Name { get; set; }
            public string IATA { get; set; }
            public string Value { get; set; }
        }

        [Serializable]
        public class CIFLight
        {
            // Auto-implemented properties. 

            public string FromIATA;
            public string ToIATA;
            public DateTime FromDate;
            public DateTime ToDate;
            public Boolean FlightMonday;
            public Boolean FlightTuesday;
            public Boolean FlightWednesday;
            public Boolean FlightThursday;
            public Boolean FlightFriday;
            public Boolean FlightSaterday;
            public Boolean FlightSunday;
            public DateTime DepartTime;
            public DateTime ArrivalTime;
            public String FlightNumber;
            public String FlightAirline;
            public String FlightOperator;
            public String FlightAircraft;
            public Boolean FlightCodeShare;
            public Boolean FlightNextDayArrival;
            public int FlightNextDays;
            public string FlightDuration;
            public Boolean FlightNonStop;
            public string FlightVia;
        }
    }
}
