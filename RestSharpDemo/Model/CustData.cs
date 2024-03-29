﻿using System;
using System.Text;


namespace RestSharpDemo.Model
{
    public class CustData
    {
        //id,first_name,last_name,email,favorite_color
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string favColor { get; set; }

        public CustData(int id, string firstName, string lastName, string email, string favColor)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.favColor = favColor;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                CustData p = (CustData)obj;
                return (id == p.id) && (firstName == p.firstName) && (lastName == p.lastName)
                                    && (email == p.email) && (favColor == p.favColor); 
            }
        }
        public override int GetHashCode()
        {
            return ASCIIEncoding.Unicode.GetByteCount(id.ToString()) ^
                   ASCIIEncoding.Unicode.GetByteCount(firstName) *
                   ASCIIEncoding.Unicode.GetByteCount(lastName) ^
                   ASCIIEncoding.Unicode.GetByteCount(email) *
                   ASCIIEncoding.Unicode.GetByteCount(favColor);
        }

        public override string ToString()
        {
            return String.Format("CustData(id= {0}, firstName= {1}, lastName= {2}, email= {3}, favColor= {4} )", 
                                 id, firstName, lastName, email, favColor);
        }
    }
}

