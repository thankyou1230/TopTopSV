using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopTopServer.Models
{
    [Table("Like")]
    public class Like
    {
        public Like(string user, string video)
        {
            this.User = user;
            this.Video = video;
        }
        [Key]
        public string User { get; set; }
        [Key]
        public string Video { get; set; }
    }
}
