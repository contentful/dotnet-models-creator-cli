using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contentful.Core.Models;

namespace Replace.Me.NameSpace
{
    public class Brand
    {
        public string CompanyName { get; set; }
        public Asset Logo { get; set; }
        public string CompanyDescription { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string Email { get; set; }
        public List<string> Phone { get; set; }
    }
}

