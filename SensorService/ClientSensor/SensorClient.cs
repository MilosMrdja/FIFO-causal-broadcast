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
    public class SensorClient: ISensorCallback, IDisposable
    {
        private ISensorService _service;
        private DuplexChannelFactory<ISensorService> _channelFactory;
        private Middleware.Middleware _middleware;
        private int _sensorId;
        private Random _random;
        private bool _isRunning;

        public SensorClient(int sensorId)
        {
            _sensorId = sensorId;
            _random = new Random(sensorId);
            _middleware = new Middleware.Middleware(sensorId);
            _middleware.OnMessageReadyForDelivery += OnMessageReadyForDelivery;
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

        public void StartSendingMeasurements()
        {
            _isRunning = true;
            Task.Run(() => SendMeasurementsLoop());
        }

        private async Task SendMeasurementsLoop()
        {
            while (_isRunning)
            {
                try
                {
                    double measurement = _random.NextDouble() * 100;
                    var message = _middleware.PrepareOutgoingMessage(measurement);

                    if (((ICommunicationObject)_service).State == CommunicationState.Opened)
                    {
                        _service.BroadcastMeasurement(message);
                        Console.WriteLine($"[Sensor {_sensorId}] 📤 Sending: {measurement:F2}");
                    }

                    await Task.Delay(_random.Next(2000, 4000));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Sensor {_sensorId}] ❌ Error: {ex.Message}");
                }
            }
            Console.WriteLine($"[Sensor {_sensorId}] ✅ Finished sending");
        }


        public void OnMessageReceived(Message message)
        {
            try
            {
                if (message != null)
                {
                    _middleware.ProcessIncomingMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Middleware error: {ex.Message}");
            }
        }


        private void OnMessageReadyForDelivery(Message message)
        {
            Console.WriteLine($"[Sensor {_sensorId}] 🎯 DELIVERED from {message.SensorId}: {message.Measurement:F2}, time:{message.Timestamp}, sequence number:{message.SequenceNumber}");
        }

        public void Dispose()
        {
            _isRunning = false;
            _channelFactory?.Close();
        }
    }
}