using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Common;

namespace SensorService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SensorService : ISensorService
    {
        private Dictionary<int, ISensorCallback> _sensorCallbacks;
        private object _lockObject = new object();

        public SensorService()
        {
            _sensorCallbacks = new Dictionary<int, ISensorCallback>();
            Console.WriteLine("Sensor Service initialized");
        }

        public void RegisterSensor(int sensorId)
        {
            lock (_lockObject)
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISensorCallback>();
                _sensorCallbacks[sensorId] = callback;
                Console.WriteLine($"Sensor {sensorId} registered successfully");
            }
        }

        public void BroadcastMeasurement(Message message)
        {
            lock (_lockObject)
            {
                var sensorsToRemove = new List<int>();

                foreach (var sensor in _sensorCallbacks)
                {
                    if (sensor.Key == message.SensorId)
                        continue;

                    try
                    {
                        sensor.Value.OnMessageReceived(message);
                        Console.WriteLine($"Sent to Sensor {sensor.Key}");
                    }
                    catch (CommunicationException)
                    {
                        Console.WriteLine($"Sensor {sensor.Key} is not reachable");
                        sensorsToRemove.Add(sensor.Key);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine($"Timeout sending to Sensor {sensor.Key}");
                        sensorsToRemove.Add(sensor.Key);
                    }
                }

                // Remove disconnected sensors
                foreach (var sensorId in sensorsToRemove)
                {
                    _sensorCallbacks.Remove(sensorId);
                }
            }
        }
    }
}
