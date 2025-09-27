using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class VectorClock
    {
        private Dictionary<int, int> _clock;

        public VectorClock()
        {
            _clock = new Dictionary<int, int>();
        }

        public VectorClock(Dictionary<int, int> clock)
        {
            _clock = new Dictionary<int, int>(clock);
        }

        public void Increment(int sensorId)
        {
            if (_clock.ContainsKey(sensorId))
                _clock[sensorId]++;
            else
                _clock[sensorId] = 1;
        }

        public int GetTime(int sensorId)
        {
            return _clock.ContainsKey(sensorId) ? _clock[sensorId] : 0;
        }

        public void Update(int sensorId, int time)
        {
            _clock[sensorId] = time;
        }
        public VectorClock Clone()
        {
            return new VectorClock(_clock);
        }

        // Vraca listu svih sensor ID-jeva
        public IEnumerable<int> GetSensorIds()
        {
            return _clock.Keys.ToList();
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", _clock.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}")) + "]";
        }
    }
}
