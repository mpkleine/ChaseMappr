using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

// Program to listen on a UDP port for CHASEMAPPER UDP packets from the LilyGO RDZSonde device
// and write them to a file.
// mpk - 11/28/2023

namespace ClassMappr
{
    public class Packet
    {
        public string? type { get; set; }
        public string? callsign { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public int? altitude { get; set; }
        public int? speed { get; set; }
        public int? heading { get; set; }
        public string? time { get; set; }
        public string? model { get; set; }
        public string? freq { get; set; }
        public double? temp { get; set; }

    }

    public class Program
    {

        public static void Main()
        {
            // Display the header
            Console.WriteLine("ChaseMappr: UDP Listener");

            int sendCounter = 0;

            // Get the configuration information from the appsettings.json file
            // located in the same folder as the executable program
            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration configuration = configBuilder.Build();
            string? udpP = configuration["udpPort"];
            string? cpuLat = configuration["cpuLatitude"];
            string? cpuLon = configuration["cpuLongitude"];
            string? cpuAlt = configuration["cpuAltitude"];
            // Convert config info as required
            double cpuLatitude = Convert.ToDouble(cpuLat);
            double cpuLongitude = Convert.ToDouble(cpuLon);
            int udpPort = Convert.ToInt32(udpP);
            int cpuAltitude = Convert.ToInt32(cpuAlt);

            // Display IP/port number in use
            Console.WriteLine("IP Address: " + GetLocalIPAddress());
            Console.WriteLine("  UDP Port: " + udpPort);
            Console.WriteLine("");

            // Create the UDP socket
            UdpClient udpServer = new UdpClient(udpPort);

            // Send initial CPU Location
            sendCpuLocation(cpuLatitude, cpuLongitude, cpuAltitude);
            fileCpu(cpuLatitude, cpuLongitude, cpuAltitude);

            // Loop forever
            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, udpPort);
                var data = udpServer.Receive(ref remoteEP);
                string someString = Encoding.ASCII.GetString(data);

                // Deserialize the JSON packet received into the skypacket class
                Packet? skypacket = JsonSerializer.Deserialize<Packet>(someString);

                // output the proper format
                sendBalloonLocation(skypacket?.latitude, skypacket?.longitude, skypacket?.altitude);
                filePackets(skypacket?.latitude, skypacket?.longitude, skypacket?.altitude);

                // send a cpu location every 10 packets
                if (sendCounter++ > 8)
                {
                    sendCpuLocation(cpuLatitude, cpuLongitude, cpuAltitude);
                    fileCpu(cpuLatitude, cpuLongitude, cpuAltitude);
                    sendCounter = 0;
                }
            }
        }

        private static void sendBalloonLocation(double? cpuLatitude, double? cpuLongitude, int? cpuAltitude)
        {
            Console.Write($"{cpuLatitude:+000.000000;-000.000000},");
            Console.Write($"{cpuLongitude:+000.000000;-000.000000},");
            Console.WriteLine($"{cpuAltitude:00000},$");
        }

        private static void sendCpuLocation(double? cpuLatitude, double? cpuLongitude, int? cpuAltitude)
        {
            Console.Write($"{cpuLatitude:+000.000000;-000.000000},");
            Console.Write($"{cpuLongitude:+000.000000;-000.000000},");
            Console.WriteLine($"{cpuAltitude:00000},L");
        }

        private static void filePackets(double? latitude, double? longitude, int? altitude)
        {
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = File.AppendText(Path.Combine(docPath, "WriteLines.txt")))
            {
                outputFile.Write($"{latitude:+000.000000;-000.000000},");
                outputFile.Write($"{longitude:+000.000000;-000.000000},");
                outputFile.WriteLine($"{altitude:00000},$");
            }
        }

        private static void fileCpu(double? cpuLatitude, double? cpuLongitude, int? cpuAltitude)
        {
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = File.AppendText(Path.Combine(docPath, "WriteLines.txt")))
            {
                outputFile.Write($"{cpuLatitude:+000.000000;-000.000000},");
                outputFile.Write($"{cpuLongitude:+000.000000;-000.000000},");
                outputFile.WriteLine($"{cpuAltitude:00000},L");
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

    }
}
