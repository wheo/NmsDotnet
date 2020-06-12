using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmsDotNet.vo
{
    class Product
    {
        public string GroupName { get; set; }
        public string Name { get; set; }        
        public string Image { get; set; }

        public Product(string Name, string GroupName, string Image )
        {
            this.Name = Name;
            this.GroupName = GroupName;
            this.Image = Image;
        }
    }
}
 