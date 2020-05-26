using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NnmDotnet.vo
{
    class Product
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Image { get; set; }

        public Product(string Name, double Value, string Image )
        {
            this.Name = Name;
            this.Value = Value;
            this.Image = Image;
        }
    }
}
 