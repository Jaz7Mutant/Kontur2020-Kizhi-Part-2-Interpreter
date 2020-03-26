using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace KizhiPart2
{
    class Program
    {
        public static void Main(string[] args)
        {
            var tw = new Class1();
            var intepretator = new Interpreter(tw);
            var test = new string[]
            {
                "set code",
                "set a 5\n" +
                "def test\n" +
                "    print a\n" +
                "set a 12", 
                "end set code",
                "run"
            };
            foreach (var s in test)
            {
                intepretator.ExecuteLine(s);
            }
        }
    }
}
