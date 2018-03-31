using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BankingWebApp.Startup))]
namespace BankingWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
