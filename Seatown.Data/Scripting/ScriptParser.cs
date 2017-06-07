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
        public Dictionary<string, string> ExclusionDelimiters { get; private set; } = new Dictionary<string, string>() {
            { "--", "\r\n" },
            { "/*", "*/" },
            { "{", "}" },
            { "[", "]" },
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

        public ScriptParser WithExclusionDelimiter(string openingDelimiter, string closingDelimiter)
        {
            if (this.ExclusionDelimiters.ContainsKey(openingDelimiter))
            {
                this.ExclusionDelimiters[openingDelimiter] = closingDelimiter;
            }
            else
            {
                this.ExclusionDelimiters.Add(openingDelimiter, closingDelimiter);
            }
            return this;
        }

        #endregion

        #region Parsing Methods

        // TODO: Add Parse(string content)
        // TODO: Add Parse(string content, Encoding encoding)
        // TODO: Add Parse(string fileName)
        // TODO: Add Parse(FileInfon fileInfo)
        // TODO: Refactor Parse method to raise maintainability index?
        // TODO: Add class description comment and usage examples?
        // TODO: Test stream availability after parse (StreamReader may close/dispose the underlying stream when it is disposed?)

        public IEnumerable<string> Parse(Stream stream)
        {
            var result = new List<string>();
            var batchAccumulator = new StringBuilder();
            var contentBuffer = new StringBuffer(this.CalculateBufferLength());
            var delimiterLevel = new Stack<string>();
            var stringComparer = this.GetStringComparison();

            using (var reader = new StreamReader(stream))
            {
                int characterCode = reader.Read();
                while (characterCode >= 0)
                {
                    char character = (char)characterCode;
                    batchAccumulator.Append(character);
                    contentBuffer.Append(character);

                    // Determine if we are in a delimiter block (strings, quoted identifer, comments).
                    this.CalculateDelimiterLevel(contentBuffer.Content, ref delimiterLevel);

                    // If we find a batch separator not in a delimited block, split the batch and reset.
                    if (delimiterLevel.Count == 0 && contentBuffer.Content.EndsWith(this.BatchSeparator, stringComparer))
                    {                       
                        if (char.IsWhiteSpace((char)reader.Peek()))
                        {
                            // Read to the next line separator not in a comment block, or to the end of the string.
                            // This prevents comments after the batch separator being returned in the next batch.
                            while (characterCode >= 0)
                            {
                                if (delimiterLevel.Count == 0 && contentBuffer.Content.EndsWith("\r\n"))
                                {
                                    break;
                                }
                                else
                                { 
                                    characterCode = reader.Read();
                                    contentBuffer.Append((char)characterCode);
                                    this.CalculateDelimiterLevel(contentBuffer.Content, ref delimiterLevel);
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
                            contentBuffer.Clear();
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
                this.ExclusionDelimiters.Keys.Max((s) => s.Length),
                this.ExclusionDelimiters.Values.Max((s) => s.Length)
            };
            // Allocate space for the character before our batch separater.
            return (contentLengths.Max() + 1);
        }

        private void CalculateDelimiterLevel(string content, ref Stack<string> levelTracker)
        {
            // Comments can contain string delimiters, and strings can contain comment delimiters,
            // so we cannot do level tracking for strings in comments, or comments in strings.
            // Any time we encounter a delimiter block, we will read to the ending delimiter, and
            // only accumulate levels for the same opening delimiter in case of nested comments.
            if (levelTracker.Count == 0)
            {
                foreach (KeyValuePair<string, string> kvp in this.ExclusionDelimiters)
                {
                    if (levelTracker.Count == 0 && content.EndsWith(kvp.Key))
                    {
                        levelTracker.Push(kvp.Key);
                    }
                }
            }
            else
            {
                var openingDelimiter = levelTracker.Peek();
                var closingDelimiter = this.ExclusionDelimiters.Where((kvp) => kvp.Key.Equals(openingDelimiter)).FirstOrDefault().Value;
                if (content.EndsWith(openingDelimiter))
                {
                    levelTracker.Push(openingDelimiter);
                }
                else if (content.EndsWith(closingDelimiter))
                {
                    // If this is a line comment, clear the stack, as all 
                    // instances of a line comment end with a single \r\n.
                    if (closingDelimiter.Equals("\r\n"))
                    {
                        levelTracker.Clear();
                    }
                    else
                    {
                        levelTracker.Pop();
                    }
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
                .WithExclusionDelimiter("/*", "*/")
                .WithExclusionDelimiter("--", "\r\n")
                .WithExclusionDelimiter("{", "}")
                .WithExclusionDelimiter("[", "]")
                .WithExclusionDelimiter("\'", "\'")
                .WithExclusionDelimiter("\"", "\"");
        }

        #endregion

    }
}
