using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI_EasyFly.Models
{
    class EasyFly
    {
        public class Origins
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string IATA { get; set; }
        }

        public class FlightList
        {
            public string FromName { get; set; }
            public string FromIATA { get; set; }
            public string FromId { get; set; }
            public string ToName { get; set; }
            public string ToIATA{ get; set; }
            public string ToId { get; set; }
        }
    }
}
