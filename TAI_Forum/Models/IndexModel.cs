using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAI_Forum.Models
{
    public class IndexModel : MainViewModel
    {
        public string IndexMessage { get; set; }
        public List<SingleThread> ThreadsList { get; set; }

        public struct SingleThread
        {
            public int Id { get; set; }
            public string Topic { get; set; }
            public string ContentLead { get; set; }
            public string Tags { get; set; }
            public string Author { get; set; }
        }
    }
}