using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Middleware
{
    public class Middleware
    {
        private readonly int _sensorId;
        private readonly VectorClock _localClock;
        private readonly VectorClock _deliveredClock;
        private readonly Dictionary<int, Queue<Message>> _messageBuffers;
        private int _localSequence;

        public event Action<Message> OnMessageReadyForDelivery;

        public Middleware(int sensorId)
        {
            _sensorId = sensorId;
            _localClock = new VectorClock();
            _deliveredClock = new VectorClock();
            _messageBuffers = new Dictionary<int, Queue<Message>>();
            _localSequence = 0;

            // Lokalni senzor inicijalno na 0
            _localClock.Update(_sensorId, 0);
            _deliveredClock.Update(_sensorId, 0);
        }

        public Message PrepareOutgoingMessage(double measurement)
        {
            _localSequence++;
            _localClock.Update(_sensorId, _localSequence);

            return new Message
            {
                SensorId = _sensorId,
                Measurement = measurement,
                Timestamp = DateTime.Now,
                Id = Guid.NewGuid(),
                VectorClock = _localClock.Clone(),
                SequenceNumber = _localSequence
            };
        }

        public string GetStatus()
        {
            int bufferedCount = _messageBuffers.Sum(b => b.Value.Count);
            return $"Sensor {_sensorId} - DeliveredClock: {_deliveredClock}, Buffers: {bufferedCount} messages";
        }
    }
}
