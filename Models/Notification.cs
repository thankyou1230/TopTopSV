using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopTopServer.Models
{
    public class Notification
    {
        public string NotiTo { get; set; }
        public string NotiFrom { get; set; }
        public string NotiContent { get; set; }
        public DateTime NotiTime { get; set; }
    }
}
