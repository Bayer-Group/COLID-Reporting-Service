using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace COLID.ReportingService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            PrintStartMessage();

            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

            Console.WriteLine("- ASPNETCORE_URLS = " + urls);
            return WebHost
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(urls);
        }

        public static void PrintStartMessage()
        {
            Console.Write(@"
     __________  __    ________              ____                        __  _            
    / ____/ __ \/ /   /  _/ __ \            / __ \___  ____  ____  _____/ /_(_)___  ____ _
   / /   / / / / /    / // / / /  ______   / /_/ / _ \/ __ \/ __ \/ ___/ __/ / __ \/ __ `/
  / /___/ /_/ / /____/ // /_/ /  /_____/  / _, _/  __/ /_/ / /_/ / /  / /_/ / / / / /_/ / 
  \____/\____/_____/___/_____/           /_/ |_|\___/ .___/\____/_/   \__/_/_/ /_/\__, /  
                                                   /_/                           /____/              
");
        }
    }
}
