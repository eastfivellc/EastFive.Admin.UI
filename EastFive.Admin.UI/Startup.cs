using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

using Blazored.LocalStorage;
using Radzen.Blazor;
using Radzen;

namespace EastFive.Admin.UI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazoredLocalStorage();
            services.AddScoped<DialogService>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
