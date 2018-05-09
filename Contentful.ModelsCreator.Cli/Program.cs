using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace Contentful.ModelsCreator.Cli
{
    class Program
    {
        public const int EXCEPTION = 2;
        public const int ERROR = 1;
        public const int OK = 0;

        static async Task<int> Main(string[] args)
        {
            try
            {
                return await CommandLineApplication.ExecuteAsync<ModelsCreator>(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unexpected error: " + ex.ToString());
                Console.ResetColor();
                return EXCEPTION;
            }
        }
    }
}
