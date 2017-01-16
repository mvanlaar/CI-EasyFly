using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace CI_EasyFly
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Parsing airport: {0}", Airport);
            BrowserSession b = new BrowserSession();
            Console.WriteLine("Getting Session and cookies...");
            b.Get("http://easyfly.com.co/");
            Console.WriteLine("Getting test flights....");
            string AirportResponse = b.Get("http://easyfly.com.co/flights?origins=24&originsText=Monter%C3%ADa+%28MTR%29&multi=&destinations=20&originsTextReturn=Medell%C3%ADn%2C+E+Olaya+H.+%28EOH%29&multiReturn=&flightType=0&departureDateEngine=25-01-2017&returnDateEngine=25-01-2017&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS");
            Console.WriteLine(AirportResponse);
        }
    }
}
