using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Common;

namespace SensorService
{
    public interface ISensorCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(Message message);
    }
}