using Microsoft.Owin;
using Owin;
using Shop.Global;

[assembly: OwinStartupAttribute(typeof(Shop.Startup))]
namespace Shop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
