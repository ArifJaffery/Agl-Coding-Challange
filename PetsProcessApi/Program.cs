using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;
using System;

namespace PetsProcessApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start");
                Console.WriteLine(ex);
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args).ConfigureAppConfiguration((context, builder) =>
                {
                    var config = builder
                        .AddJsonFile($"appSettings.json",optional:true,reloadOnChange:true)
                        .AddEnvironmentVariables()
                        .Build();

                })
                .UseStartup<Startup>();
        }
    }
}
