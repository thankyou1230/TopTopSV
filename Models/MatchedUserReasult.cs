using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TopTopServer.Models
{
    public class MatchedUserReasult
    {
        public MatchedUserReasult(User user, int count)
        {
            this.Profile = user;
            this.FollowerCount = count;
        }
        public User Profile { get; set; }
        public int FollowerCount { get; set; }
    }
}
