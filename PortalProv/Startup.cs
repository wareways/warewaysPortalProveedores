using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Wareways.PortalProv.Startup))]
namespace Wareways.PortalProv
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            
        }
    }
}
