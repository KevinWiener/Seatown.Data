using System;
using System.Collections.Generic;

namespace Seatown.Data.Schemas
{   
    public class Procedure
    {

        public Procedure()
        {
            // Empty constructor
        }
        public Procedure(string schema, string name, Parameter[] parameters)
        {
            this.Name = name;
            this.Schema = schema;
            if (parameters != null && parameters.Length > 0)
            {
                this.Parameters.AddRange(parameters);
            }
        }
        public string Name { get; set; }
        public string Schema { get; set; }
        public List<Parameter> Parameters { get; set; }
        
    }
}