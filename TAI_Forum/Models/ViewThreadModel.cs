using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TAI_Forum.Models
{
    public class ViewThreadModel : MainViewModel
    {
        public int ThreadId { get; set; }
        public string ThreadTopic { get; set; }
        public string Tags { get; set; }
        
        public List<SingleMessage> Messages { get; set; }

        public MessageModel MessageModel { get; set; }

        public struct SingleMessage
        {
            public string Content { get; set; }
            public string Author { get; set; }
            public int Score { get; set; }
            public string PostDate { get; set; }
            public int OrdNum { get; set; }
        }
    }
}