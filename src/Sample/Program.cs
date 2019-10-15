using System;
using ArgsToClass;
using ArgsToClass.Exceptions;

namespace Sample
{
    public class Option
    {
        public bool Help { get; set; }
        public string Name { get; set; }
        public int Repeat { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // args == new string[]{ "--name", "test name", "--repeat", "1"};

            var parser = new ArgsParser<Option>();
            
            IArgsData<Option> data;
            try
            {
                // ここでコマンドライン引数を解析してクラスにマッピングする
                data = parser.Parse(args);
            }
            catch (ArgsAnalysisException ex)
            {
                Console.WriteLine(ex);
                return;
            }

            Console.WriteLine($"Help   == {data.Option.Help}");   // Help   == false
            Console.WriteLine($"Name   == {data.Option.Name}");   // Name   == "test name"
            Console.WriteLine($"Repeat == {data.Option.Repeat}"); // Repeat == 1

            Console.ReadKey();
        }
    }
}
