using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Potato.Dashboard.Startup))]
namespace Potato.Dashboard
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
