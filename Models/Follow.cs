using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopTopServer.Models
{
    [Table("Follow")]
    public class Follow
    {
        public Follow(string follower, string following)
        {
            this.Follower = follower;
            this.Following = following;
        }
        [Key]
        public string Follower { get; set; }
        [Key]
        public string Following { get; set; }
    }
}
