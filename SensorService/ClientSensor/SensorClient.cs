using Common;
using SensorService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientSensor
{
    public class SensorClient: IDisposable
    {
        private ISensorService _service;
        private DuplexChannelFactory<ISensorService> _channelFactory;
        private int _sensorId;

        public SensorClient(int sensorId)
        {
            _sensorId = sensorId;
        }

        public void Connect()
        {
            try
            {
                var callbackInstance = new InstanceContext(this);
                _channelFactory = new DuplexChannelFactory<ISensorService>(
                    callbackInstance,
                    "SensorEndpoint"
                );

                _service = _channelFactory.CreateChannel();
                _service.RegisterSensor(_sensorId);
                Console.WriteLine($"✅ Sensor {_sensorId} connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection error: {ex.Message}");
                throw;
            }
        }
        public void Dispose()
        {
            _isRunning = false;
            _channelFactory?.Close();
        }
    }
}