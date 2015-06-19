using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Plugin.HttpAuth;

namespace TestingExe
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = HttpAccessor.getResult("spencer".ToString(), "hovno".ToString());

            System.Console.Write(t);
        }
    }
}
