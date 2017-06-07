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
        //
        // TODO: add tests for escaped quotes (''')
        // TODO: add tests for quoted identifier blocks (select "GO")
        // TODO: add tests for text delimiters inside a comment block
        // TODO: add tests for comment blocks inside a text block
        // TODO: add tests for batch delimiter followed by command delimiter (GO;)
        //---------------------------------------------------------------------------------

        #region Declarations & Properties

        public TestContext TestContext { get; set; }
        private const string TEST_CATEGORY = "ScriptParser Tests";

        #endregion

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparator_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSingleLineComment_DoesNotSeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("-- GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSlashStarBlockComment_DoesNotSeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* GO */");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideCurlyBraceBlockComment_DoesNotSeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("{ GO }");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorQuotedIdentifierBlock_DoesNotSeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("[GO]");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorFollowedByLineComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO -- This is a comment!");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorNestedCommentBlocks_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* nested comment */");
            sb.AppendLine("*/");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
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
        public void Parse_NestedCommentBlocks_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* nested comment */");
            sb.AppendLine("GO");
            sb.AppendLine("*/");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorEmptyBatches_ReturnsZeroBatches()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("GO");
            sb.AppendLine("GO");
            sb.AppendLine("GO");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(0, batches.Count(), "Incorrect number of batches");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_CommandDelimiter_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1; SELECT 2;");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_QuotedString_DoesNotSeparateBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("INSERT INTO #SomeTable");
            sb.AppendLine("EXEC sp_HelpIndex 'dbo.SomeTable'");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorSingleBatch_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorPrecededByBlockComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SELECT 1");
            sb.AppendLine("/* Comment! */ GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("SELECT 1\r\n/* Comment! */", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_MultiLineString_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PRINT '");
            sb.AppendLine("''");
            sb.AppendLine("GO");
            sb.AppendLine("/*");
            sb.AppendLine("'");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleHalfQuoteInsideBlockComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/* This doesn't return three */");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("/* This doesn't return three */\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleHalfQuoteInsideSingleLineComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("--This doesn't return three");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");
            sb.AppendLine("SELECT 2");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("--This doesn't return three\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual("SELECT 2", batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_SingleExclusionDelimiterInsideAnotherDelimitedBlock_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- Why aren't /* comments cool?");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("-- Why aren't /* comments cool?\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_LineCommentInsideAnotherLineComment_SeparatesBatch()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- --");
            sb.AppendLine("SELECT 1");
            sb.AppendLine("GO");

            var parser = new Scripting.ScriptParser();
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual("-- --\r\nSELECT 1", batches.FirstOrDefault(), "Incorrect batch information");
            }
        }


    }
}


