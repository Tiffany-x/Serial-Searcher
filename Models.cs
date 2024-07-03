using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSearcher
{
    public class Models
    {
        public string modelDesc
        {
            get;
            set;
        }
        public override string ToString()
        {
            return this.modelDesc;
        }
    }
}
