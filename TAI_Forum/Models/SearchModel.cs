using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAI_Forum.Models
{
    public class SearchModel : MainViewModel
    {
        public string Tag { get; set; }
        public List<SingleSearchThread> SearchThreadsList { get; set; }

        public struct SingleSearchThread
        {
            public int Id { get; set; }
            public string Topic { get; set; }
            public string ContentLead { get; set; }
            public string Tags { get; set; }
            public string Author { get; set; }
        }
    }
}