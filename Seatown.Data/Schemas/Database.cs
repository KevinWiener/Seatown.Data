using System;
using System.Collections.Generic;

namespace Seatown.Data.Schemas
{   
    public class Database
    {
        public Database()
        {
            // Empty constructor...
        }
        public Database(string name)
        {
            this.Name = name;
        }
        public Database(string name, Table[] tables, View[] views, Procedure[] procedures, Function[] functions)
        {
            this.Name = name;
            if (tables != null && tables.Length > 0) this.Tables.AddRange(tables);
            if (views != null && views.Length > 0) this.Views.AddRange(views);
            if (procedures != null && procedures.Length > 0) this.Procedures.AddRange(procedures);
            if (functions != null && functions.Length > 0) this.Functions.AddRange(functions);
        }

        public string Name { get; set; }
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<View> Views { get; set; } = new List<View>();
        public List<Procedure> Procedures { get; set; } = new List<Procedure>();
        public List<Function> Functions { get; set; } = new List<Function>();

        public override string ToString()
        {
            return this.Name;
        }
    }
}