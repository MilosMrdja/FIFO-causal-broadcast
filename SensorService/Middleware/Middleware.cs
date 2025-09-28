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
      
        private readonly VectorClock _deliveredClock;
        private readonly Dictionary<int, Queue<Message>> _messageBuffers;
        private int _localSequence;

        public event Action<Message> OnMessageReadyForDelivery;

        public Middleware(int sensorId)
        {
            _sensorId = sensorId;
            _deliveredClock = new VectorClock();
            _messageBuffers = new Dictionary<int, Queue<Message>>();
            _localSequence = 0;
            _deliveredClock.Update(_sensorId, 0);
        }

        public Message PrepareOutgoingMessage(double measurement)
        {
            _localSequence++;
            _deliveredClock.Update(_sensorId, _localSequence);

            return new Message
            {
                SensorId = _sensorId,
                Measurement = measurement,
                Timestamp = DateTime.Now,
                Id = Guid.NewGuid(),
                VectorClock = _deliveredClock.Clone(),
                SequenceNumber = _localSequence
            };
        }

        public void ProcessIncomingMessage(Message message)
        {
            try
            {

                EnsureSensorInitialized(message.SensorId, message.VectorClock, message.SequenceNumber);

                if (!IsFIFOReady(message))
                {
                    Console.WriteLine(
                        $"FIFO not ready for sensor {message.SensorId}. " +
                        $"Expected: {_deliveredClock.GetTime(message.SensorId) + 1}, Got: {message.SequenceNumber}");
                    _messageBuffers[message.SensorId].Enqueue(message);
                    return;
                }

                DeliverMessage(message);
                CheckAllBufferedMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Middleware error: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
            }
        }


        private void EnsureSensorInitialized(int sensorId, VectorClock incomingClock, int sequenceNumber)
        {
            // Ako nemamo buffer za senzor – dodaj ga
            if (!_messageBuffers.ContainsKey(sensorId))
            {
                _messageBuffers[sensorId] = new Queue<Message>();
            }

            // Ako clock nema entry za senzor – dodaj ga
            if (_deliveredClock.GetTime(sensorId) == -1)
            {
                _deliveredClock.Update(sensorId, sequenceNumber - 1);
            }
        }

        private void DeliverMessage(Message message)
        {
            _deliveredClock.Update(message.SensorId, message.SequenceNumber);
            OnMessageReadyForDelivery?.Invoke(message);
        }

        public string GetStatus()
        {
            int bufferedCount = _messageBuffers.Sum(b => b.Value.Count);
            return $"Sensor {_sensorId} - DeliveredClock: {_deliveredClock}, Buffers: {bufferedCount} messages";
        }

        private bool IsFIFOReady(Message message)
        {
            int lastDelivered = _deliveredClock.GetTime(message.SensorId);
            if (lastDelivered == -1)
            {
                _deliveredClock.Update(message.SensorId, message.SequenceNumber - 1);
                return true;
            }

            return message.SequenceNumber == lastDelivered + 1;
        }

        private void CheckAllBufferedMessages()
        {
            bool deliveredAny;
            do
            {
                deliveredAny = false;

                foreach (var sensorBuffer in _messageBuffers.ToList())
                {
                    if (sensorBuffer.Value.Count == 0) continue;

                    var message = sensorBuffer.Value.Peek();

                    if (IsFIFOReady(message))
                    {
                        sensorBuffer.Value.Dequeue();
                        DeliverMessage(message);
                        deliveredAny = true;
                        break;
                    }
                }
            } while (deliveredAny);
        }
    }
}
