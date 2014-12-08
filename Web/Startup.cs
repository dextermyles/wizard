using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(WizardGame.Startup))]
namespace WizardGame
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // manage configs
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(30);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
