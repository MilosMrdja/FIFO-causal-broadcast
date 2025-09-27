using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class Message
    {
        public Guid Id { get; set; }
        public int SensorId { get; set; }
        public double Measurement { get; set; }
        public DateTime Timestamp { get; set; }
        public VectorClock VectorClock { get; set; }
        public int SequenceNumber { get; set; }

        public Message()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now;
            VectorClock = new VectorClock();
        }

        public Message(int sensorId, double measurement) : this()
        {
            SensorId = sensorId;
            Measurement = measurement;
        }

        public override string ToString()
        {
            return $"Message {Id} from Sensor {SensorId}: {Measurement} (Seq: {SequenceNumber}, Time: {Timestamp:HH:mm:ss.fff})";
        }
    }
}
