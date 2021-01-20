using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpDemo.Model
{
    public class Posts
    {
        public static int sid { get; set; }
        public static string stitle { get; set; }
        public static string sauthor { get; set; }

        public int id { get; set; }
        public string title { get; set; }
        public string author { get; set; }

        public Posts()
        {
            this.id = sid;
            this.title = stitle;
            this.author = sauthor;
        }

        public Posts(int id, string title, string author) 
        {
            this.id = id;
            this.title = title;
            this.author = author;
        }

    }
}
