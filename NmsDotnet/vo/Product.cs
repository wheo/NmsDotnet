using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotNet.vo
{
    class Product
    {
        public string Name { get; set; }
        public String Status { get; set; }
        public string Image { get; set; }

        public Product(string Name, string Status, string Image )
        {
            this.Name = Name;
            this.Status = Status;
            this.Image = Image;
        }
    }
}
 