using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IO.Ports;

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
            // Display the program header
            Console.WriteLine("    ChaseMappr: UDP Listener v 1.1.0");

            // Get the configuration information from the appsettings.json file
            // located in the same folder as the executable program
            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration configuration = configBuilder.Build();
            string? udpP = configuration["udpPort"];
            string? cpuLat = configuration["cpuLatitude"];
            string? cpuLon = configuration["cpuLongitude"];
            string? cpuAlt = configuration["cpuAltitude"];
            string? serialPort = configuration["serialPort"];
            string? serialBaud = configuration["serialBaud"];
            string? serialData = configuration["serialData"];
            string? serialStop = configuration["serialStop"];
            string? serialParity = configuration["serialParity"];
            string? serialPacketPause = configuration["serialPacketPause"];
            int serialPacketPauseInt = (int)Convert.ToInt64(serialPacketPause);
            string? locationPause = configuration["locationPause"];
            int locationPauseInt = (int)Convert.ToInt64(locationPause);
            // Convert config info as required
            double cpuLatitude = Convert.ToDouble(cpuLat);
            double cpuLongitude = Convert.ToDouble(cpuLon);
            int udpPort = Convert.ToInt32(udpP);
            int cpuAltitude = Convert.ToInt32(cpuAlt);

            // Display IP/port number in use
            Console.WriteLine("    IP Address: " + GetLocalIPAddress());
            Console.WriteLine("      UDP Port: " + udpPort);
            Console.WriteLine("   Serial Port: " + serialPort + ": " +
                serialBaud + "," +
                serialData + "," +
                serialParity + "," +
                serialStop);
            Console.WriteLine("  Packet Pause: " + serialPacketPause + " seconds");
            Console.WriteLine("Location Pause: " + locationPause + " seconds");

            // Create the UDP socket
            UdpClient udpServer = new UdpClient(udpPort);

            // Open the Serial Port
            SerialPort? mySerialPort = new SerialPort();
            mySerialPort.PortName = serialPort;
            mySerialPort.BaudRate = (int)Convert.ToInt64(serialBaud);
            mySerialPort.DataBits = Convert.ToInt32(serialData);
            switch (serialParity?.ToLower())
            {
                case "even":
                    mySerialPort.Parity = Parity.Even;
                    break;
                case "odd":
                    mySerialPort.Parity = Parity.Odd;
                    break;
                case "mark":
                    mySerialPort.Parity = Parity.Mark;
                    break;
                case "space":
                    mySerialPort.Parity = Parity.Space;
                    break;
                default:
                    mySerialPort.Parity = Parity.None;
                    break;
            }
            switch (serialStop)
            {
                case "0":
                    mySerialPort.StopBits = StopBits.None;
                    break;
                case "2":
                    mySerialPort.StopBits = StopBits.Two;
                    break;
                default:
                    mySerialPort.StopBits = StopBits.One;
                    break;
            }
            try
            {
                mySerialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Serial Port was not found. No serial data will be sent.");
                mySerialPort.Close();
                mySerialPort = null;
            }

            // Calculate last send time for serial packet pause
            DateTime lastSerialPacket = Convert.ToDateTime("2023-12-1 20:52:00");

            Console.WriteLine("");

            // Send initial CPU Location
            sendScreen(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"));
            sendFile(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"));
            if (mySerialPort != null)
            {
                sendSerial(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"), mySerialPort);
            }
            DateTime lastLocation = DateTime.Now;

            // Loop forever
            while (true)
            {
                // open the port for receive, listen for the packet
                var remoteEP = new IPEndPoint(IPAddress.Any, udpPort);
                var data = udpServer.Receive(ref remoteEP);
                string someString = Encoding.ASCII.GetString(data);

                // Deserialize the JSON packet received into the skypacket class
                Packet? skypacket = JsonSerializer.Deserialize<Packet>(someString);

                // output the properly formatted balloon packet to the screen, log file, and serial port
                sendScreen(formatInfo(Convert.ToDouble(skypacket?.latitude),
                    Convert.ToDouble(skypacket?.longitude),
                    Convert.ToDouble(skypacket?.altitude), "$"));
                sendFile(formatInfo(Convert.ToDouble(skypacket?.latitude),
                    Convert.ToDouble(skypacket?.longitude),
                    Convert.ToDouble(skypacket?.altitude), "$"));
                if (DateTime.Now > lastSerialPacket.AddSeconds(serialPacketPauseInt))
                {
                    if (mySerialPort != null)
                    {
                        sendSerial(formatInfo(Convert.ToDouble(skypacket?.latitude),
                            Convert.ToDouble(skypacket?.longitude),
                            Convert.ToDouble(skypacket?.altitude), "$"), mySerialPort);
                    }
                    lastSerialPacket = DateTime.Now;
                }

                // send cpu location if ready
                if (DateTime.Now > lastLocation.AddSeconds(locationPauseInt))
                {
                    sendScreen(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"));
                    sendFile(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"));
                    if (mySerialPort != null)
                    {
                        sendSerial(formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L"), mySerialPort);
                    }
                    lastLocation = DateTime.Now;
                }
            }
        }

        // send the line to the screen
        private static void sendScreen(string formattedString)
        {
            Console.Write($"{formattedString}");
        }

        // send the line to the log file
        private static void sendFile(string formattedString)
        {
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "ChaseMappr.txt".
            using (StreamWriter outputFile = File.AppendText(Path.Combine(docPath, "ChaseMappr.txt")))
            {
                outputFile.Write($"{formattedString}");
            }
        }

        // send the line to the serial port
        public static void sendSerial(string outputData, SerialPort mySerialPort)
        {
            mySerialPort.Write(outputData);
        }

        // get the local machine's IP to display at the start of the program
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

        // format the output to match the GC requirements
        public static string formatInfo(double lat, double lon, double alt, string lev)
        {

            string linedata = $"{lat:+000.000000;-000.000000},";
            linedata += $"{lon:+000.000000;-000.000000},";
            linedata += $"{alt:00000},";
            linedata += $"{lev}";
            linedata += $"\n";
            return linedata;
        }
    }
}