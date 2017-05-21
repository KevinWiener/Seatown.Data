namespace Seatown.Data.Schemas
{   
    public class Column
    {

        public Column()
        {
            // Empty constructor
        }
        public Column(string schema, string name, string dataType)
        {
            this.Name = name;
            this.Schema = schema;
            this.DataType = dataType;
        }
        public string Name { get; set; }
        public string Schema { get; set; }
        public string DataType { get; set; }

    }
}
