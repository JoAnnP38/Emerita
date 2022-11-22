using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Emerita
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Util.TraceSwitch.Level = Enum.Parse<TraceLevel>(config["AppSettings:TraceLevel"] ?? "Error");

            Perft perft = new();
            Stopwatch watch = new();

            for (int depth = 0; depth < 7; ++depth)
            {
                watch.Start();
                ulong actual = perft.Execute(depth);
                watch.Stop();
                Console.WriteLine($"{depth}: Elapsed = {watch.Elapsed}");
            }

        }
    }
}