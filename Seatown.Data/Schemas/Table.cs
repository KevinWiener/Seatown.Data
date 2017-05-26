using System;
using System.Collections.Generic;

namespace Seatown.Data.Schemas
{   
    public class Table
    {

        public Table()
        {
            // Empty constructor
        }
        public Table(string schema, string name, params Column[] columns)
        {
            this.Name = name;
            this.Schema = schema;
            if (columns != null && columns.Length > 0)
            {
                this.Columns.AddRange(columns);
            }
        }
        public string Name { get; set; }
        public string Schema { get; set; }
        public List<Column> Columns { get; set; }
        
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(this.Schema))
            {
                return this.Name;
            }
            else
            {
                return $"{this.Schema}.{this.Name}";
            }               
        }
    }
}