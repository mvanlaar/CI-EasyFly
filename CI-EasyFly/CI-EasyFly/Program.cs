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
            b.Get("http://easyfly.com.co/");           
            string AirportResponse = b.Get("http://easyfly.com.co/flights?origins=24&originsText=Monter%C3%ADa+%28MTR%29&multi=&destinations=20&originsTextReturn=Medell%C3%ADn%2C+E+Olaya+H.+%28EOH%29&multiReturn=&flightType=0&departureDateEngine=25-01-2017&returnDateEngine=25-01-2017&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS");
            Console.WriteLine(AirportResponse);
            //Stream s = null;
            //StreamReader sr = null;
            //HttpWebResponse Res = null;
            //CookieContainer CC = new CookieContainer();
            //try
            //{
            //    //—————————————————-
            //    //FIRST REQUEST
            //    //—————————————————-     
            //    HttpWebRequest Req = (HttpWebRequest)WebRequest.Create("http://easyfly.com.co/");
            //    Req.Proxy = null;
            //    Req.UseDefaultCredentials = true;

            //    //YOU MUST ASSIGN A COOKIE CONTAINER FOR THE REQUEST TO PULL THE COOKIES
            //    Req.CookieContainer = CC;

            //    Res = (HttpWebResponse)Req.GetResponse();

            //    //DUMP THE COOKIES
            //    Console.WriteLine("—– COOKIES —–");
            //    if (Res.Cookies != null && Res.Cookies.Count != 0)
            //    {
            //        foreach (Cookie c in Res.Cookies)
            //        {
            //            Console.WriteLine("\t" +c.ToString());
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("No Cookies present");
            //    }


            //    s = Res.GetResponseStream();
            //    sr = new StreamReader(s, Encoding.ASCII);
            //    Console.WriteLine("—– RESPONSE —–");
            //    Console.WriteLine("\t" +sr.ReadToEnd());


            //    //—————————————————-
            //    //SECOND REQUEST
            //    //—————————————————-   

            //    Req = (HttpWebRequest)WebRequest.Create("http://easyfly.com.co/flights?origins=24&originsText=Monter%C3%ADa+%28MTR%29&multi=&destinations=20&originsTextReturn=Medell%C3%ADn%2C+E+Olaya+H.+%28EOH%29&multiReturn=&flightType=0&departureDateEngine=25-01-2017&returnDateEngine=25-01-2017&adt=1&chd=0&inf=0&promotionID=&tstPost=tstPost&=BUSCAR+VUELOS");
            //    Req.Proxy = null;
            //    Req.UseDefaultCredentials = true;
            //   // CC.Add(new Cookie("UserSettings", "tracking=nYk1q9O1a2xmxJhxfsl90QDO6WfDbJvXAxQAzeu8bTbt+buKwxdpv3Loeup5f/y4"));
            //    //TO TRANSFER COOKIES TO THE NEXT PAGE
            //    Req.CookieContainer = CC;

            //    Res = (HttpWebResponse)Req.GetResponse();

            //    //DUMP THE COOKIES
            //    Console.WriteLine("—– COOKIES —–");
            //    if (Res.Cookies != null && Res.Cookies.Count != 0)
            //    {
            //        foreach (Cookie c in Res.Cookies)
            //        {
            //            Console.WriteLine("\t" +c.ToString());
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("No Cookies present");
            //    }

            //    s = Res.GetResponseStream();
            //    sr = new StreamReader(s, Encoding.ASCII);
            //    Console.WriteLine("—– RESPONSE —–");
            //    Console.WriteLine("\t" +sr.ReadToEnd());
            //    Console.WriteLine("—– RESPONSE —–");

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            //finally
            //{
            //    if (sr != null) sr.Close();
            //    if (s != null) s.Close();
            //}

        }
    }
}
