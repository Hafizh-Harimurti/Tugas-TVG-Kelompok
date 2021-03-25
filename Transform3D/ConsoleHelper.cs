using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transform3D
{
    public static class ConsoleHelper
    {
        public static ConsoleColor DefaultConsoleForegroundColor { get; private set; }

        public static void Initialize()
        {
            DefaultConsoleForegroundColor = Console.ForegroundColor;
        }
    }
}
