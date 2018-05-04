using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferlesyl
{
    using Core;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ferlesyl.exe filename");
                Environment.Exit(1);
            }

            Emulator emulator = new Emulator();
            emulator.Read(args[0]);

            emulator.Run();
        }
    }
}
