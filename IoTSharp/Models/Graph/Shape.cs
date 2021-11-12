namespace IoTSharp.Models.Graph
{
    public class Shape
    {

        public long id { get; set; }

        public long[] incomes { get; set; }
        public long[] outgoings { get; set; }


        public DeviceProp? prop { get; set; }



    }
}