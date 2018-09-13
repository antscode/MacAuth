using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MacAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((webHostBuilderContext, configurationbuilder) =>
                {
                    var environment = webHostBuilderContext.HostingEnvironment;
                    configurationbuilder
                        .AddJsonFile("appsettings.json");
                    configurationbuilder.AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
