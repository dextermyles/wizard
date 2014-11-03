using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Wizard
{
    public class GameSessionHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}