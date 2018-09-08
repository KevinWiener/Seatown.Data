using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Seatown.Data.Tests
{
    [TestClass]
    public class ScriptParser_Tests
    {
        //---------------------------------------------------------------------------------
        // TODO: add performance tests for FixedLengthQueue
        // TODO: add performance tests for StringBuffer
        // TODO: add performance tests for ScriptParser
        //---------------------------------------------------------------------------------

        #region Declarations & Properties

        public TestContext TestContext { get; set; }
        private const string TEST_CATEGORY = "ScriptParser Tests";

        public Scripting.IScriptParser GetParser()
        {
            return new Scripting.ScriptParser();
        }

        #endregion

        #region Simple Syntax Tests

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparator_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorSingleBatch_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorEmptyBatches_ReturnsZeroBatches()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("GO");
            sb.AppendLine("    ");
            sb.AppendLine("GO");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(0, batches.Count(), "Incorrect number of batches");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SemiColonCommandDelimiter_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1; SELECT 2;");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleQuoteString_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("INSERT INTO #SomeTable");
            sb.AppendLine("EXEC sp_HelpIndex 'dbo.SomeTable'");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_EmptyLinesInScript_IncludesEmptyLines()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine();
            sb.AppendLine("--Text");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1\r\n\r\n--Text", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        #endregion

        #region Complex Syntax Tests

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorFollowedByDashDashCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO -- This is a comment!");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorPreceededByNestedSlashStarCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* nested comment */");
            sb.AppendLine("*/");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                // This test returns two batches, one with the comment block, 
                // and the second with an actual command.  
                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorPrecededBySlashStarCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* Comment! */ GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1\r\n/* Comment! */", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleHalfQuoteInsideSlashStarCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/* This doesn't return three */");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("/* This doesn't return three */\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleHalfQuoteInsideDashDashCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("--This doesn't return three");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("--This doesn't return three\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_PartialSlashStarCommentInsideDashDashCommentBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- Why aren't /* comments cool?");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("-- Why aren't /* comments cool?\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_DashDashCommentInsideAnotherDashDashComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- --");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("-- --\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SlashStarCommentInsideSingleQuoteBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT '/* odd */'");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT '/* odd */'", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_EscapedSingleQuote_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 'This doesn''t fail!'");
            sb.AppendLine("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 'This doesn''t fail!'", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorMissingLineTerminator_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.Append("GO");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_CommandNewlineBatchSeparatorCommandSeparator_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO;");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_CommandNewlineBatchSeparatorSpaceCommandSeparator_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO ;");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BlockCommentFollowedImmediatelyByBatchSeparator_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* block comment */GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1\r\n/* block comment */", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BlockCommentFollowedByBatchSeparatorFollowedByLineComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* block comment */ GO -- line comment");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1\r\n/* block comment */", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_CommandFollowedByBatchSeparatorOnTheSameLine_DoesNotSeparateBatch()
        {
            // TODO: add tests for "SELECT 1 GO SELECT 2"
            //       May need several variations on this test, with \r\n, without, etc.
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1; GO SELECT 2;");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_ColumnAliasEqualToBatchSeparator_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1 AS DaysAgo");
            sb.AppendLine("FROM dbo.SomeTable s");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        #endregion

        #region Batch Separator Inside Exclusion Block Tests

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSingleLineComment_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("-- GO");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSlashStarCommentBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* GO */");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideNestedSlashStarCommentBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* nested comment */");
            sb.AppendLine("GO");
            sb.AppendLine("*/");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideCurlyBraceBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("{ GO }");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSquareBracketBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1 ");
            sb.AppendLine("[GO]");
            sb.AppendLine("SELECT 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideMultiLineSquareBracketBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1 [");
            sb.AppendLine("GO");
            sb.AppendLine("], 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideDoubleQuoteBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("\"GO\"");
            sb.AppendLine(", 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSingleQuoteBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("'GO'");
            sb.AppendLine(", 2");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideMultiLineSingleQuoteBlock_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PRINT '");
            sb.AppendLine("''");
            sb.AppendLine("GO");
            sb.AppendLine("/*");
            sb.AppendLine("'");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        #endregion

        #region Tests for possible implementation

        //[TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorFollowedByNumber_SeparatesAndRepeatsBatch()
        {
            // TODO: add tests for repeating batches? 
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO 10");

            var parser = this.GetParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(10, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        #endregion

    }
}


