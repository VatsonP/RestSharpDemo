using System;
using System.Collections;

namespace RestSharpDemo.Model
{
    internal class DataProviders
    {
        public static IEnumerable ValidCustomers
        {
            get
            {   
                /*
                //1,Erda,Birkin,ebirkinb @google.com.hk,Aquamarine
                yield return new DemoCust()
                {
                    id = "1",
                    firstName = "Erda",
                    lastName = "Birkin",
                    email = "ebirkinb@google.com",
                    favColor = "Aquamarine"
                };
                //2,Cherey,Endacott,cendacottc @freewebs.com,Fuscia
                yield return new DemoCust()
                {
                    id = "2",
                    firstName = "Cherey",
                    lastName = "Endacott",
                    email = "cendacottc@freewebs.com",
                    favColor = "Fuscia"
                };
                */
                //3,Shalom,Westoff,swestoffd @about.me,Red
                yield return new DemoCust()
                {
                    id = "3",
                    firstName = "Shalom",
                    lastName = "Westoff",
                    email = "swestoffd@about.me",
                    favColor = "Red"
                };
                /*
                //4,Jo,Goulborne,jgoulbornee @example.com,Red
                yield return new DemoCust()
                {
                    id = "4",
                    firstName = "Jo",
                    lastName = "Goulborne",
                    email = "jgoulbornee@example.com",
                    favColor = "Red"
                };
                */
                /*
                 ... 
                */
            }
        }
    }
}