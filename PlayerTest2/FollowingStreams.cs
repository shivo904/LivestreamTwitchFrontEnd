using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;

namespace PlayerTest2
{
    public class FollowingStreams
    {
        public FollowingStreams(string username, string game, bool isOnline, bool isPartner)
        {
            this.UserName = username;
            this.Online = isOnline;
            this.Game = game;
            this.Partner = isPartner;
        }
        public string UserName { get; set; }
        public bool Online { get; set; }
        public string Game { get; set; }
        public bool Partner { get; set; }
    }
}
