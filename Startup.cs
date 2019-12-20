using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EastFive.Admin.UI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");

        }
    }
}
