using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSensor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Sensor Client...");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            if (args.Length != 1 || !int.TryParse(args[0], out int sensorId) || sensorId < 1 || sensorId > 4)
            {
                Console.WriteLine("Usage: SensorClient <sensorId> (where sensorId is 1-4)");
                Console.ReadKey();
                return;
            }
            Console.WriteLine($"Starting Sensor Client {sensorId}...");

            try
            {
                using (var client = new SensorClient(sensorId))
                {
                    client.Connect();
                    client.StartSendingMeasurements();

                    Console.WriteLine("Press any key to stop...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Full error: {ex.ToString()}");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}