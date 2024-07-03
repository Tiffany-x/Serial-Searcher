using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialSearcher
{
    class Devices
    {
        public string serialNumber
        {
            get;
            set;
        }
        public string invoiceDate
        {
            get;
            set;
        }
        public string serial()
        {
            return this.serialNumber;
        }
        public string invoice()
        {
            return this.invoiceDate;
        }
    }
}
