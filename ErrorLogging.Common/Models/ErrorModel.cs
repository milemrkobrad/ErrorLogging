using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogging.Common.Models
{
    public class ErrorModel
    {
        public List<Item> ServerVariables { get; set; }
        public List<Item> Cookies { get; set; }
        public List<Item> Form { get; set; }
    }

    public class Item
    {
        public string K { get; set; }
        public string StringValues { get; set; }
       
        public Item()
        {

        }

        public Item(string k, string stringValues)
        {
            K = k;
            StringValues = stringValues;
        }
    }
}
