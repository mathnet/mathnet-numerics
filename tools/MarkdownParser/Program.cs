using System;
using System.IO;
using System.Text.RegularExpressions;
using anrControls;
using HtmlAgilityPack.Samples;

namespace MarkdownParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("usage: MarkdownParser input_file html_output_file text_output_file");
                Environment.Exit(-1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("{0} does not exist.", args[0]);
                Environment.Exit(-1);
            }

            //convert markdown to html
            var markdownSource = File.ReadAllText(args[0]);
            var htmlSource = new Markdown().Transform(markdownSource);
            File.WriteAllText(args[1], htmlSource);

            //convert html to txt
            var html2txt = new HtmlToText();
            var text = html2txt.Convert(args[1]);
            //use OS newlines
            text = Regex.Replace(text, "(?<!\r)\n", Environment.NewLine);
            Console.WriteLine(text);
            File.WriteAllText(args[2], text);
        }
    }
}