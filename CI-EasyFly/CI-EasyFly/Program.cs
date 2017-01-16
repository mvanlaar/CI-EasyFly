using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace CI_EasyFly
{
    class Program
    {
        static void Main(string[] args)
        {
            Regex rgxIATAAirport = new Regex(@"([A-Z]{3})");
            List<AirportDef> _Airports = new List<AirportDef> { };
            //Console.WriteLine("Parsing airport: {0}", Airport);
            BrowserSession b = new BrowserSession();
            Console.WriteLine("Getting Session and cookies...");
            string Frontpage = b.Get("http://easyfly.com.co/");
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
                    _Airports.Add(new AirportDef { Name = AirportName, IATA = IATA, Value = AirportValue });
                }
            }

            Console.WriteLine("Getting test flights....");
            string AirportResponse = b.Get("http://easyfly.com.co/flights?origins=24&originsText=Monter%C3%ADa+%28MTR%29&multi=&destinations=20&originsTextReturn=Medell%C3%ADn%2C+E+Olaya+H.+%28EOH%29&multiReturn=&flightType=0&departureDateEngine=25-01-2017&returnDateEngine=25-01-2017&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS");
            Console.WriteLine(AirportResponse);
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
