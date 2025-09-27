using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Common;

namespace SensorService
{
    [ServiceContract(CallbackContract = typeof(ISensorCallback))]
    public interface ISensorService
    {
        [OperationContract(IsOneWay = true)]
        void RegisterSensor(int sensorId);

        [OperationContract(IsOneWay = true)]
        void BroadcastMeasurement(Message message);

        [OperationContract(IsOneWay = true)]
        void AcknowledgeMessage(Guid messageId, int sensorId);
    }
}