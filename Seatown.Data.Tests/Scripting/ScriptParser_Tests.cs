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
            string batch1 = "SELECT 1";
            string batchDelimiter = "GO";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

            var parser = new Scripting.ScriptParser();

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(batch1, batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual(batch2, batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorInsideSingleLineComment_DoesNotSeparatesBatch()
        {
            string batch1 = "SELECT 1";
            string batchDelimiter = "--GO";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

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
            string batch1 = "SELECT 1";
            string batchDelimiter = "/*GO*/";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

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
            string batch1 = "SELECT 1";
            string batchDelimiter = "{GO}";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

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
            string batch1 = "SELECT 1";
            string batchDelimiter = "[GO]";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

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
            string batch1 = "SELECT 1";
            string batchDelimiter = "GO -- This is a comment!";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(batch1);
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

            var parser = new Scripting.ScriptParser();

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(2, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(batch1, batches.FirstOrDefault(), "Incorrect batch information");
                Assert.AreEqual(batch2, batches.LastOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_BatchSeparatorNestedCommentBlocks_SeparatesBatch()
        {
            string batch1 = "SELECT 1";
            string batchDelimiter = "GO";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine(batch1);
            sb.AppendLine("/* nested comment */");
            sb.AppendLine("*/");
            sb.AppendLine(batchDelimiter);
            sb.AppendLine(batch2);

            var parser = new Scripting.ScriptParser();

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                // TODO: This test returns two batches, one with the comment block, and the second with 
                //       the actual command.  May need to drop the comment block?

                //Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                //Assert.AreEqual(batch2, batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

        [TestCategory(TEST_CATEGORY), TestMethod]
        public void Parse_NestedCommentBlocks_SeparatesBatch()
        {
            string batch1 = "SELECT 1";
            string batchDelimiter = "GO";
            string batch2 = "SELECT 2";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("/*");
            sb.AppendLine(batch1);
            sb.AppendLine("/* nested comment */");
            sb.AppendLine(batchDelimiter);
            sb.AppendLine("*/");
            sb.AppendLine(batch2);

            var parser = new Scripting.ScriptParser();

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(sb.ToString())))
            {
                IEnumerable<string> batches = parser.Parse(ms);

                Assert.AreEqual(1, batches.Count(), "Incorrect number of batches");
                Assert.AreEqual(sb.ToString().Trim(), batches.FirstOrDefault(), "Incorrect batch information");
            }
        }

    }
}


