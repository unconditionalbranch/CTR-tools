﻿using CTRFramework.Lang;
using CTRFramework.Shared;
using System;

namespace lng2txt
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(
                "{0}\r\n{1}\r\n\r\n{2}\r\n",
                $"CTR-Tools: lng2txt - {Meta.GetSignature()}",
                "Converts LNG localization files to TXT and back.",
                Meta.GetVersion());

            if (args.Length == 0)
            {
                Console.WriteLine("No filename!");
                return;
            }

            LNG lng = new LNG(args[0]);
            Console.WriteLine("Done!");
        }
    }
}