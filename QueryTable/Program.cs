using System;
using Aws.AzureTools;
using System.Linq;

namespace QueryTable
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: QueryTable tablename");
                Console.WriteLine("Usage: QueryTable tablename \"PartitionKey eq '1'\"");
                return Settings.FAIL;
            }

            var tableHelper = new TableHelper();
            var query = (args.Length > 1) ? args[1] : "";
            var lastKeys = "";
            foreach (var entity in tableHelper.QueryTable(args[0], query))
            {
                var keys = string.Join(",", entity.Keys.Select(x => "\"" + x + "\"").ToArray());
                if (keys != lastKeys)
                {
                    // only write out the column headings if the columns have changed
                    Console.WriteLine(keys);
                    lastKeys = keys;
                }

                Console.WriteLine(string.Join(",", entity.Values.Select(Escape).ToArray()));
            }
            return Settings.SUCCESS;
        }

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = {',', '"', '\n'};

        public static string Escape(string s)
        {
            if (s.Contains(QUOTE))
            {
                s = s.Replace(QUOTE, ESCAPED_QUOTE);
            }

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
            {
                s = QUOTE + s + QUOTE;
            }

            return s;
        }

    }
}
