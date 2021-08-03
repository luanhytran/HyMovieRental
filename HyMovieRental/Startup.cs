using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HyMovieRental.Startup))]
namespace HyMovieRental
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
