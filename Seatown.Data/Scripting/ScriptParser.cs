using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatown.Data.Scripting.Common;

namespace Seatown.Data.Scripting
{
    // TODO: Maybe rename ScriptParser to BatchParser or CommandParser??

    public class ScriptParser
    {

        #region Properties

        // Property default values match T-SQL syntax
        public string BatchDelimiter { get; set; } = "GO";
        public bool CaseSensitive { get; set; } = false;
        public Dictionary<string, string> BlockDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "--", "\r\n" },
            { "/*", "*/" },
            { "{", "}" },
            { "[", "]" }
        };
        public Dictionary<string, string> TextDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "\'", "\'" },
            { "\"", "\"" }
        };

        #endregion

        #region Fluent Methods

        public ScriptParser WithBatchDelimiter(string delimiter)
        {
            this.BatchDelimiter = delimiter.Trim();
            return this;
        }

        public ScriptParser WithCaseSensitive(bool caseSensitive)
        {
            this.CaseSensitive = caseSensitive;
            return this;
        }

        public ScriptParser WithBlockDelimiter(string openingDelimiter, string closingDelimiter)
        {
            if (this.BlockDelimiters.ContainsKey(openingDelimiter))
            {
                this.BlockDelimiters[openingDelimiter] = closingDelimiter;
            }
            else
            {
                this.BlockDelimiters.Add(openingDelimiter, closingDelimiter);
            }
            return this;
        }

        public ScriptParser WithTextDelimiter(string delimiter)
        {
            if (this.TextDelimiters.ContainsKey(delimiter))
            {
                this.TextDelimiters[delimiter] = delimiter;
            }
            else
            {
                this.TextDelimiters.Add(delimiter, delimiter);
            }
            return this;
        }

        #endregion

        #region Parsing Methods

        public IEnumerable<string> Parse(Stream stream)
        {
            var result = new List<string>();
            var batch = new StringBuilder();
            var buffer = new StringBuffer(this.CalculateBufferLength());
            var commentLevel = new Stack<string>();
            var stringComparison = this.GetStringComparison();
            var textLevel = new Stack<string>();

            using (var reader = new StreamReader(stream))
            {
                int characterCode = reader.Read();
                while (characterCode >= 0)
                {
                    char character = (char)characterCode;

                    batch.Append(character);
                    buffer.Append(character);

                    // Determine if we are in a text block or quoted identifer block 
                    this.CalculateLevel(buffer.Content, this.TextDelimiters, ref textLevel);
                    if (textLevel.Count == 0)
                    {
                        // Determine if we are in a comment block
                        this.CalculateLevel(buffer.Content, this.BlockDelimiters, ref commentLevel);
                    }

                    // If we find a batch separator not in a text block, quoted identifier block, or comment block, split the batch and reset.
                    if (commentLevel.Count == 0 && textLevel.Count == 0 && buffer.Content.EndsWith(this.BatchDelimiter, stringComparison))
                    {
                        var nextCharacter = (char)reader.Peek();
                        if (string.IsNullOrWhiteSpace(new string(nextCharacter, 1)))
                        {
                            // Remove the batch separator from the end of the batch
                            batch.Remove(batch.Length - this.BatchDelimiter.Length, this.BatchDelimiter.Length);
                            //--------------------------------------------------------------------------------------
                            // TODO: Convert Parse method to use yield return for large scripts??
                            //--------------------------------------------------------------------------------------
                            //yield return batch.ToString().Trim();
                            result.Add(batch.ToString().Trim());
                            batch.Clear();
                            buffer.Clear();
                        }
                    }

                    characterCode = reader.Read();
                }

                if (!string.IsNullOrWhiteSpace(batch.ToString()))
                {
                    result.Add(batch.ToString().Trim());
                    //--------------------------------------------------------------------------------------
                    // TODO: Convert Parse method to use yield return for large scripts??
                    //--------------------------------------------------------------------------------------
                    //yield return batch.ToString().Trim();
                }
            }

            return result;
        }

        private int CalculateBufferLength()
        {
            int bufferLength = this.BatchDelimiter.Length;
            foreach (KeyValuePair<string, string> kvp in this.BlockDelimiters)
            {
                if (kvp.Key.Length > bufferLength) bufferLength = kvp.Key.Length;
                if (kvp.Value.Length > bufferLength) bufferLength = kvp.Value.Length;
            }
            // Allocate space for the character before our batch separater.
            return (bufferLength + 1);
        }

        private void CalculateLevel(string content, Dictionary<string, string> delimiters, ref Stack<string> levelTracker)
        {
            foreach (KeyValuePair<string, string> kvp in delimiters)
            {
                if (content.EndsWith(kvp.Key))
                {
                    levelTracker.Push(kvp.Key);
                }
                else if (levelTracker.Count > 0 && levelTracker.Peek().Equals(kvp.Key) && content.EndsWith(kvp.Value))
                {
                    levelTracker.Pop();
                }
            }
        }

        private StringComparison GetStringComparison()
        {
            var equalityComparer = StringComparison.CurrentCultureIgnoreCase;
            if (this.CaseSensitive)
            {
                equalityComparer = StringComparison.CurrentCulture;
            }
            return equalityComparer;
        }

        #endregion

        #region Static Methods

        public static ScriptParser FromSql()
        {
            return new ScriptParser()
                .WithBatchDelimiter("GO")
                .WithCaseSensitive(false)
                .WithBlockDelimiter("/*", "*/")
                .WithBlockDelimiter("--", "\r\n")
                .WithBlockDelimiter("{", "}")
                .WithBlockDelimiter("[", "]")
                .WithTextDelimiter("\'")
                .WithTextDelimiter("\"");
        }

        #endregion

    }
}
