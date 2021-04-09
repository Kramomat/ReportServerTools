using System;
using System.Reflection;
using System.Threading.Tasks;

// TODO: Eventuell mehrere Parameter unterstützen beim Start?
// /c --> Cache Warming durchführen
// /l --> ExecutionLogs kopieren
// /s --> SubscriptionLogs kopieren

namespace ReportServerTools
{
    class Program
    {
        /// <summary>
        /// Main Method when starting App
        /// </summary>
        /// <param name="args">Commandline Arguments</param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            // traverse through all arugments found
            foreach (String arg in args)
            {
                ToolLibrary tl = new ToolLibrary();
                switch(arg.ToLower())
                { 
                    case "/c":
                        await tl.ExecuteCacheWarming();
                        break;
                    case "/e":
                        tl.CopyExecutionLog();
                        break;
                    case "/r":
                        tl.CopyCatalog();
                        break;
                    case "/s":
                        tl.CopySubscriptionLog();
                        break;
                    case "/h":
                    case "/help":
                    case "/?":
                        Console.WriteLine();
                        Console.WriteLine(String.Concat("Report Server Tools ", Assembly.GetExecutingAssembly().GetName().Version.ToString()," Copyright (c) 2020 Mark A. Kuschel"));
                        Console.WriteLine();
                        Console.WriteLine("ReportServerTools <Argument1> <Argument2>");
                        Console.WriteLine();
                        Console.WriteLine("Arguments:");
                        Console.WriteLine(" /c      Executes Cache Warming");
                        Console.WriteLine(" /e      Executes copying ExecutionLog");
                        Console.WriteLine(" /r      Executes copying Reporting Catalog");
                        Console.WriteLine(" /s      Executes copying SubscriptionLog");
                        Console.WriteLine(" /h      Shows this help");
                        Console.WriteLine();
                        break;
                }
            }           
        }

       
    }
}

/*
 * Backlog:
 *  - Übernahme des ExecutionLogs für längere Speicherfrist --> DSGVO Retention Policy
 *  - Übernahme des Datenaufbereitslogs für längere Speicherfrist
 */
