using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contentful.Core.Models;

namespace Replace.Me.NameSpace
{
    public class Product
    {
        public string ProductName { get; set; }
        public string Slug { get; set; }
        public string ProductDescription { get; set; }
        public string Sizetypecolor { get; set; }
        public List<Asset> Image { get; set; }
        public List<string> Tags { get; set; }
        public List<Category> Categories { get; set; }
        public float Price { get; set; }
        public object Brand { get; set; }
        public int Quantity { get; set; }
        public string Sku { get; set; }
        public string Website { get; set; }
        public string Test { get; set; }
    }
}

