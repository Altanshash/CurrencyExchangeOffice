using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CurrencyExchangeOffice.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IExchangeServic" in both code and config file together.
    [ServiceContract]
    public interface IExchangeServic
    {
        [OperationContract]
        void DoWork();
    }
}
