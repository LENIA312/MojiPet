using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mojipet.Editor.MasterData
{
    public static class CsvReader
    {
        public static List<string[]> ReadRows(string filePath)
        {
            var rows = new List<string[]>();
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    rows.Add(ParseLine(line));
                }
            }

            return rows;
        }

        private static string[] ParseLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }

                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields.ToArray();
        }
    }
}
