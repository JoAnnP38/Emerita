using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Emerita
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Util.TraceSwitch.Level = Enum.Parse<TraceLevel>(config["AppSettings:TraceLevel"] ?? "Error");

            RunPerft();
        }

        static void RunPerft()
        {
            Perft perft = new();
            Stopwatch watch = new();

            Console.WriteLine("Single threaded results:");
            for (int depth = 1; depth < 7; ++depth)
            {
                watch.Restart();
                ulong actual = perft.Execute(depth);
                watch.Stop();

                double Mnps = (double)actual / (watch.Elapsed.TotalSeconds * 1000000.0D);
                Console.WriteLine($"{depth}: Elapsed = {watch.Elapsed}, Mnps: {Mnps,7:N2}, nodes = {actual}");
            }

            Perft.NumTasks = 2;
            Console.WriteLine($"\nMulti-threaded results ({Perft.NumTasks} threads):");
            for (int depth = 1; depth < 7; ++depth)
            {
                watch.Restart();
                ulong actual = perft.ExecuteMt(depth);
                watch.Stop();
                double Mnps = (double)actual / (watch.Elapsed.TotalSeconds * 1000000.0D);
                Console.WriteLine($"{depth}: Elapsed = {watch.Elapsed}, Mnps: {Mnps,7:N2}, Nodes = {actual}");
            }

            Perft.NumTasks = 4;
            Console.WriteLine($"\nMulti-threaded results ({Perft.NumTasks} threads):");
            for (int depth = 1; depth < 7; ++depth)
            {
                watch.Restart();
                ulong actual = perft.ExecuteMt(depth);
                watch.Stop();
                double Mnps = (double)actual / (watch.Elapsed.TotalSeconds * 1000000.0D);
                Console.WriteLine($"{depth}: Elapsed = {watch.Elapsed}, Mnps: {Mnps,7:N2}, Nodes = {actual}");
            }

            Perft.NumTasks = 6;
            Console.WriteLine($"\nMulti-threaded results ({Perft.NumTasks} threads):");
            for (int depth = 1; depth < 7; ++depth)
            {
                watch.Restart();
                ulong actual = perft.ExecuteMt(depth);
                watch.Stop();
                double Mnps = (double)actual / (watch.Elapsed.TotalSeconds * 1000000.0D);
                Console.WriteLine($"{depth}: Elapsed = {watch.Elapsed}, Mnps: {Mnps,7:N2}, Nodes = {actual}");
            }
        }
    }
}