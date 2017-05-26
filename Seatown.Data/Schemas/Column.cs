namespace Seatown.Data.Schemas
{   
    public class Column
    {

        public Column()
        {
            // Empty constructor
        }
        public Column(string name, string dataType)
        {
            this.Name = name;
            this.DataType = dataType;
        }
        public string Name { get; set; }
        public string DataType { get; set; }

        public override string ToString()
        {
            return $"{this.Name} {this.DataType}";
        }
    }
}
