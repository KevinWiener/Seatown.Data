using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatown.Data.Scripting.Common;

namespace Seatown.Data.Scripting
{
    public class ScriptParser : IScriptParser
    {

        // TODO: Convert project to .Net Standard 2.0
        // TODO: Merge feature branch back to main trunk as a starting point for major changes
        // TODO: Remove any Try/Catch blocks that are just rethrowing the exception
        // TODO: Finish building out StringBuilder script parser
        // TODO: Implement Async methods for SchemaBuilder and Factory classes?

        #region Properties

        public string BatchSeparator { get; set; } = "GO";
        public string LineTerminator { get; set; } = "\r\n";
        public bool CaseSensitive { get; set; } = false;
        public Dictionary<string, string> ExclusionDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "--", "\r\n" },
            { "/*", "*/" },
            { "{", "}" },
            { "[", "]" },
            { "\'", "\'" },
            { "\"", "\"" }
        };

        #endregion

        #region Parsing Methods

        public IEnumerable<string> Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return this.Parse(reader);
            }
        }

        public IEnumerable<string> Parse(StreamReader reader)
        {
            var result = new List<string>();
            var lineReader = new LineReader(this.BatchSeparator, this.LineTerminator, this.CaseSensitive, this.ExclusionDelimiters);
            var batchAccumulator = new StringBuilder();

            while (reader.Peek() >= 0)
            {
                if (lineReader.IsSeparator(reader.ReadLine() + "\r\n"))
                {
                    if (!string.IsNullOrWhiteSpace(lineReader.Content))
                    {
                        batchAccumulator.Append(lineReader.Content);
                    }

                    if (!string.IsNullOrWhiteSpace(batchAccumulator.ToString()))
                    {
                        result.Add(batchAccumulator.ToString().Trim());
                    }

                    batchAccumulator.Clear();
                }
                else
                {
                    batchAccumulator.Append(lineReader.Content);
                }
            }

            if (!string.IsNullOrWhiteSpace(batchAccumulator.ToString()))
            {
                result.Add(batchAccumulator.ToString().Trim());
            }

            return result.ToArray();
        }

        #endregion

    }
}
