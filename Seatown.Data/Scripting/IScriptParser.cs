using System;
using System.Collections.Generic;
using System.IO;

namespace Seatown.Data.Scripting
{

    // TODO: Remove IScriptParser interface 
    // TODO: Remove CharacterScriptParser class
    // TODO: Remove FixedLengthQueue class
    // TODO: Remove Common namespace and move current classes under the Scripting root (these are internal classes anyway).
    // TODO: Review the LineReader class to add any missing comments and refactor the code for easier maintainability.

    public interface IScriptParser
    {
        IEnumerable<string> Parse(Stream stream);
    }
}
