using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSearcher
{
    public class Devices
    {
        public string SerialNo
        {
            get;
            set;
        }
        public string InvoiceNo
        {
            get;
            set;
        }
        public string Serial()
        {
            return this.SerialNo;
        }
        public string Invoice()
        {
            return this.InvoiceNo;
        }
    }
}
