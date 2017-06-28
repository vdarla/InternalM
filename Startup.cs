using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Medical.Startup))]
namespace Medical
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
