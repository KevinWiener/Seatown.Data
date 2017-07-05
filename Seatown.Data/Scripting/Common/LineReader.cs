using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seatown.Data.Scripting.Common;

namespace Seatown.Data.Scripting.Common
{
    //-------------------------------------------------------------------------------------------------------
    // This class evaluates a script line and determines if it is a batch separator.
    //-------------------------------------------------------------------------------------------------------
    // Criteria to determine if a line is a batch separator:
    //   1.  Line must contain the batch separator outside a delimiter block
    //   2.  No characters are permitted prior to the batch separator that are outside a delimiter block
    //   3.  No characters are permitted after the batch separator that are outside a delimiter block
    //   4.  A trailing delimiter block cannot span beyond the current line (batch separator cannot be 
    //       followed by a block comment that does not terminate on the same line)
    //-------------------------------------------------------------------------------------------------------
    internal class LineReader
    {

        #region Declarations & Properties

        private Stack<string> m_DelimiterLevel = new Stack<string>();

        private StringBuilder m_Content = new StringBuilder();
        public string Content
        {
            get
            {
                return m_Content.ToString();
            }
        }

        public string BatchSeparator { get; private set; }
        public Dictionary<string, string> ExclusionDelimiters { get; private set; }
        public string LineTerminator { get; private set; }
        public StringComparison StringComparer { get; private set; }

        #endregion

        #region Evaluation Methods

        public LineReader(string batchSeparator, string lineTerminator, bool caseSensitive, Dictionary<string, string> exclusionDelimiters)
        {
            this.BatchSeparator = batchSeparator;
            this.LineTerminator = lineTerminator;
            this.ExclusionDelimiters = exclusionDelimiters;
            if (caseSensitive)
            {
                this.StringComparer = StringComparison.CurrentCulture;
            }
            else
            {
                this.StringComparer = StringComparison.CurrentCultureIgnoreCase;
            }
        }

        public bool IsSeparator(string s)
        {
            bool result = false;
            this.m_Content.Clear();
            var contentBuffer = new StringBuffer(this.CalculateBufferLength());
            var characters = s.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                char c = characters[i];
                contentBuffer.Append(c);
                this.m_Content.Append(c);

                // Determine if we are in a delimiter block (strings, quoted identifer, comments).
                this.CalculateDelimiterLevel(contentBuffer.Content, ref m_DelimiterLevel);

                // If we find a batch separator not in a delimited block, split the batch and reset.
                if (m_DelimiterLevel.Count == 0 && contentBuffer.Content.EndsWith(this.BatchSeparator, this.StringComparer))
                {
                    result = this.EndOfLineIsEmpty(s.Substring(i + 1));
                    if (result)
                    {
                        this.m_Content.Remove(this.m_Content.Length - this.BatchSeparator.Length, this.BatchSeparator.Length);
                        break;
                    }
                }
            }

            return result;
        }

        private bool EndOfLineIsEmpty(string s)
        {
            bool result = true;
            var contentBuffer = new StringBuffer(this.CalculateBufferLength());
            var delimiterLevel = new Stack<string>();

            foreach (char c in s.ToCharArray())
            {
                contentBuffer.Append(c);

                // Determine if we are in a delimiter block (strings, quoted identifer, comments).
                this.CalculateDelimiterLevel(contentBuffer.Content, ref delimiterLevel);

                // If there are any non-whitespace characters after the batch separator, disqualify the line.
                if (delimiterLevel.Count == 0 && !char.IsWhiteSpace(c) && !char.IsControl(c))
                {
                    result = false;
                }
                else if (delimiterLevel.Count > 0 && !contentBuffer.Content.EndsWith(delimiterLevel.Peek(), this.StringComparer))
                {
                    result = true;
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
            if (levelTracker.Count == 0)
            {
                this.MatchOpeningDelimiter(content, ref levelTracker);
            }
            else
            {
                this.MatchClosingDelimiter(content, ref levelTracker);
            }
        }

        private void MatchOpeningDelimiter(string content, ref Stack<string> levelTracker)
        {
            foreach (KeyValuePair<string, string> kvp in this.ExclusionDelimiters)
            {
                if (content.EndsWith(kvp.Key))
                {
                    levelTracker.Push(kvp.Key);
                    break;
                }
            }
        }

        private void MatchClosingDelimiter(string content, ref Stack<string> levelTracker)
        {
            // Any time we encounter a delimiter block, we will read to the ending delimiter, and
            // only accumulate levels for the same opening delimiter assuming the opening delimiter 
            // is not equal to the closing delimiter in case of nested exclusion delimiter blocks.
            var openingDelimiter = levelTracker.Peek();
            var closingDelimiter = this.ExclusionDelimiters.Where((kvp) => kvp.Key.Equals(openingDelimiter)).FirstOrDefault().Value;
            if (content.EndsWith(openingDelimiter) && !openingDelimiter.Equals(closingDelimiter))
            {
                levelTracker.Push(openingDelimiter);
            }
            else if (content.EndsWith(closingDelimiter))
            {
                // If this is a line comment, clear the stack, as all instances
                // of a line comment end with a single line terminator.
                if (closingDelimiter.Equals(this.LineTerminator))
                {
                    levelTracker.Clear();
                }
                else
                {
                    levelTracker.Pop();
                }
            }
        }

        #endregion

    }
}
