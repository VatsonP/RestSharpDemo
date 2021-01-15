using System.Collections.Generic;


namespace RestSharpDemo.Model
{
    internal class DataProviders
    {
        public static IEnumerable<CustData> ValidCustomers
        {
            get
            {
                //1,Erda,Birkin,ebirkinb @google.com.hk,Aquamarine
                yield return new CustData(1, "Erda", "Birkin", "ebirkinb@google.com", "Aquamarine");
                //2,Cherey,Endacott,cendacottc @freewebs.com,Fuscia
                yield return new CustData(2, "Cherey", "Endacott", "cendacottc@freewebs.com", "Fuscia");
                //3,Shalom,Westoff,swestoffd @about.me,Red
                yield return new CustData(3, "Shalom", "Westoff", "swestoffd@about.me", "Red");
                //4,Jo,Goulborne,jgoulbornee @example.com,Red
                yield return new CustData(4, "Jo", "Goulborne", "jgoulbornee@example.com", "Red");
              
                /*
                 ... 
                */
            }
        }
    }
}