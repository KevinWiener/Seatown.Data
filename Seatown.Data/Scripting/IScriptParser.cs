using System;
using System.Collections.Generic;
using System.IO;

namespace Seatown.Data.Scripting
{
    public interface IScriptParser
    {
        IEnumerable<string> Parse(Stream stream);
    }
}
