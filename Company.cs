using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSearcher
{
    class Company
    {
        public string companyDesc
        {
            get;
            set;
        }
        public override string ToString()
        {
            return this.companyDesc;
        }
    }
}
