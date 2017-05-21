using System;
using System.Collections.Generic;

namespace Seatown.Data.Schemas
{   
    public class Database
    {

        public string Name { get; set; }
        public List<Table> Tables { get; set; }
        public List<View> Views { get; set; }
        public List<Procedure> Procedures { get; set; }
        public List<Function> Functions { get; set; }

    }
}