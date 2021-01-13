using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpDemo.Model
{
    /* //DemoCust
    {
    "id": "3",
    "firstName": "Shalom",
    "lastName": "Westoff",
    "email": "swestoffd@about.me",
    "favColor": "Red"
    }
    */
    public class DemoCust
    {
        //id,first_name,last_name,email,favorite_color
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string favColor { get; set; }
    }

}