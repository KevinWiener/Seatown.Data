using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatown.Data.Scripting.Common;

namespace Seatown.Data.Scripting
{
    public class ScriptParser
    {

        #region Properties

        // Property default values match T-SQL syntax
        public string BatchSeparator { get; set; } = "GO";
        public bool CaseSensitive { get; set; } = false;
        public Dictionary<string, string> CommentDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "--", "\r\n" },
            { "/*", "*/" },
            { "{", "}" },
            { "[", "]" }
        };
        public Dictionary<string, string> StringDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "\'", "\'" },
            { "\"", "\"" }
        };

        #endregion

        #region Fluent Methods

        public ScriptParser WithBatchSeparator(string delimiter)
        {
            this.BatchSeparator = delimiter;
            this.CaseSensitive = false;
            return this;
        }

        public ScriptParser WithBatchSeparator(string delimiter, bool caseSensitive)
        {
            this.BatchSeparator = delimiter;
            this.CaseSensitive = caseSensitive;
            return this;
        }

        public ScriptParser WithCommentDelimiter(string openingDelimiter, string closingDelimiter)
        {
            if (this.CommentDelimiters.ContainsKey(openingDelimiter))
            {
                this.CommentDelimiters[openingDelimiter] = closingDelimiter;
            }
            else
            {
                this.CommentDelimiters.Add(openingDelimiter, closingDelimiter);
            }
            return this;
        }

        public ScriptParser WithStringDelimiter(string openingDelimiter, string closingDelimiter)
        {
            if (this.StringDelimiters.ContainsKey(openingDelimiter))
            {
                this.StringDelimiters[openingDelimiter] = closingDelimiter;
            }
            else
            {
                this.StringDelimiters.Add(openingDelimiter, closingDelimiter);
            }
            return this;
        }

        #endregion

        #region Parsing Methods

        // TODO: Add Parse(string content)
        // TODO: Add Parse(string content, Encoding encoding)
        // TODO: Add Parse(string fileName)
        // TODO: Add Parse(FileInfon fileInfo)

        public IEnumerable<string> Parse(Stream stream)
        {
            var result = new List<string>();

            var batchAccumulator = new StringBuilder();
            var commentLevel = new Stack<string>();
            var delimiterBuffer = new StringBuffer(this.CalculateBufferLength());
            var stringLevel = new Stack<string>();
            var stringComparer = this.GetStringComparison();

            using (var reader = new StreamReader(stream))
            {
                int characterCode = reader.Read();
                while (characterCode >= 0)
                {
                    char character = (char)characterCode;
                    batchAccumulator.Append(character);
                    delimiterBuffer.Append(character);

                    // Determine if we are in a delimiter block (strings, quoted identifer, comments).
                    // Comments can contain string delimiters, and strings can contain comment delimiters,
                    // so we cannot do level tracking for strings in comments, or comments in strings.
                    if (commentLevel.Count == 0 && stringLevel.Count == 0)
                    {
                        this.CalculateDelimiterLevel(delimiterBuffer.Content, this.CommentDelimiters, ref commentLevel);
                        this.CalculateDelimiterLevel(delimiterBuffer.Content, this.StringDelimiters, ref stringLevel);
                    }
                    else if (commentLevel.Count == 0 && stringLevel.Count > 0)
                    {
                        this.CalculateDelimiterLevel(delimiterBuffer.Content, this.StringDelimiters, ref stringLevel);
                    }
                    else if (commentLevel.Count > 0 && stringLevel.Count == 0)
                    {
                        this.CalculateDelimiterLevel(delimiterBuffer.Content, this.CommentDelimiters, ref commentLevel);
                    }


                    // If we find a batch separator not in a delimited block, split the batch and reset.
                    if (commentLevel.Count == 0 && stringLevel.Count == 0 && delimiterBuffer.Content.EndsWith(this.BatchSeparator, stringComparer))
                    {                       
                        if (char.IsWhiteSpace((char)reader.Peek()))
                        {
                            // Read to the next line separator not in a comment block, or to the end of the string.
                            // This prevents comments after the batch separator being returned in the next batch.
                            while (characterCode >= 0)
                            {
                                if (commentLevel.Count == 0 && delimiterBuffer.Content.EndsWith("\r\n"))
                                {
                                    break;
                                }
                                else
                                { 
                                    characterCode = reader.Read();
                                    delimiterBuffer.Append((char)characterCode);
                                    this.CalculateDelimiterLevel(delimiterBuffer.Content, this.CommentDelimiters, ref commentLevel);
                                }
                            }


                            // Remove the batch separator from the end of the current batch
                            batchAccumulator.Remove(batchAccumulator.Length - this.BatchSeparator.Length, this.BatchSeparator.Length);
                            //--------------------------------------------------------------------------------------
                            // TODO: Convert Parse method to use yield return for large scripts??
                            //--------------------------------------------------------------------------------------
                            //yield return batch.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(batchAccumulator.ToString()))
                            {
                                result.Add(batchAccumulator.ToString().Trim());
                            }
                            batchAccumulator.Clear();
                            delimiterBuffer.Clear();
                        }
                    }

                    characterCode = reader.Read();
                }

                if (!string.IsNullOrWhiteSpace(batchAccumulator.ToString()))
                {
                    result.Add(batchAccumulator.ToString().Trim());
                    //----------------------------------------------------------------------------------------------
                    // TODO: Convert Parse method to use yield return for large scripts??
                    //----------------------------------------------------------------------------------------------
                    //yield return batch.ToString().Trim();
                }
            }

            return result;
        }

        private int CalculateBufferLength()
        {
            int[] contentLengths = {
                this.BatchSeparator.Length,
                this.CommentDelimiters.Keys.Max((s) => s.Length),
                this.CommentDelimiters.Values.Max((s) => s.Length),
                this.StringDelimiters.Keys.Max((s) => s.Length),
                this.StringDelimiters.Values.Max((s) => s.Length)
            };
            // Allocate space for the character before our batch separater.
            return (contentLengths.Max() + 1);
        }

        private void CalculateDelimiterLevel(string content, Dictionary<string, string> delimiters, ref Stack<string> levelTracker)
        {
            if (levelTracker.Count == 0)
            {
                foreach (KeyValuePair<string, string> kvp in delimiters)
                {
                    if (levelTracker.Count == 0 && content.EndsWith(kvp.Key))
                    {
                        levelTracker.Push(kvp.Key);
                    }
                }
            }
            else
            {
                var key = levelTracker.Peek();
                var value = delimiters.Where((kvp) => kvp.Key.Equals(key)).FirstOrDefault().Value;
                if (content.EndsWith(key))
                {
                    levelTracker.Push(key);
                }
                else if (content.EndsWith(value))
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
                .WithBatchSeparator("GO", false)
                .WithCommentDelimiter("/*", "*/")
                .WithCommentDelimiter("--", "\r\n")
                .WithCommentDelimiter("{", "}")
                .WithCommentDelimiter("[", "]")
                .WithStringDelimiter("\'", "\'")
                .WithStringDelimiter("\"", "\"");
        }

        #endregion

    }
}
