using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IO.Ports;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices;

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

        public static void Main(string[] args)
        {
            // Display the program header
            Console.WriteLine("    ChaseMappr: UDP Listener v 1.5.0");

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
            string? eolChar = configuration["eolChar"];
            string? serialReplayPause = configuration["serialReplayPause"];
            int serialReplayPauseInt = (int)Convert.ToInt64(serialReplayPause);

            // Convert config info as required
            double cpuLatitude = Convert.ToDouble(cpuLat);
            double cpuLongitude = Convert.ToDouble(cpuLon);
            int udpPort = Convert.ToInt32(udpP);
            int cpuAltitude = Convert.ToInt32(cpuAlt);

            // Sanity check end of line character
            if (!eolChar.Equals("\n") &&
                !eolChar.Equals("\r") &&
                !eolChar.Equals("\n\r") &&
                !eolChar.Equals("\r\n"))
            {
                eolChar = "\n";
            }

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
            Console.WriteLine("  Replay Pause: " + serialReplayPauseInt + " seconds");

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
            catch (Exception)
            {
                Console.WriteLine("Serial Port was not found. No serial data will be sent.");
                mySerialPort.Close();
                mySerialPort = null;
            }

            // Calculate last send time for serial packet pause
            DateTime lastSerialPacket = Convert.ToDateTime("2023-12-1 20:52:00");

            Console.WriteLine("");

            // check command line for calibrate
            for (int j = 0; j < args.Length; j++)
            {
                if (args[j].ToLower().Equals("calibrate"))
                {
                    Calibrate(cpuLatitude, cpuLongitude, cpuAltitude, mySerialPort, eolChar);
                }

                if (args[j].ToLower().Equals("replay"))
                {
                    Replay(mySerialPort, eolChar, serialReplayPauseInt);
                }
            }

            // Send initial CPU Location
            string outputLineL = formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L");
            sendScreen(outputLineL);
            sendFile(outputLineL);
            if (mySerialPort != null)
            {
                sendSerial(outputLineL, mySerialPort, eolChar);
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
                string outputLineD = formatInfo(Convert.ToDouble(skypacket?.latitude),
                    Convert.ToDouble(skypacket?.longitude),
                    Convert.ToDouble(skypacket?.altitude), "$");
                sendScreen(outputLineD);
                sendFile(outputLineD);
                if (DateTime.Now > lastSerialPacket.AddSeconds(serialPacketPauseInt))
                {
                    if (mySerialPort != null)
                    {
                        sendSerial(outputLineD, mySerialPort, eolChar);
                    }
                    lastSerialPacket = DateTime.Now;
                }

                // send cpu location if ready
                if (DateTime.Now > lastLocation.AddSeconds(locationPauseInt))
                {
                    outputLineL = formatInfo(cpuLatitude, cpuLongitude, cpuAltitude, "L");
                    sendScreen(outputLineL);
                    sendFile(outputLineL);
                    if (mySerialPort != null)
                    {
                        sendSerial(outputLineL, mySerialPort, eolChar);
                    }
                    lastLocation = DateTime.Now;
                }
            }
        }

        // Send the contents of ChaseMapprReplay.txt to the screen and serial outputs
        private static void Replay(SerialPort? mySerialPort, string eolChar, int serialReplayPauseInt)
        {
            // Display header
            Console.WriteLine("File Replay -- Documents\\ChaseMapprReplay.txt");

            string? line;
            int ms = 0;
            DateTime lastSerialReplay = Convert.ToDateTime("2023-12-1 20:52:00");

            try
            {
                // Set a variable to the Documents path.
                string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(Path.Combine(docPath, "ChaseMapprReplay.txt"));

                // Previous Time
                DateTime previousSend = DateTime.Now;
                double locationLat = 0;
                double locationLon = 0;
                double balloonLat = 0;
                double balloonLon = 0;
                double distance = 0;
                double angle = 0;
                
                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    // calculate the time of the current packet
                    DateTime dt1 = DateTime.ParseExact(line.Substring(32, 23), "yyyy-MM-dd HH:mm:ss.fff", null);
                    TimeSpan span = dt1 - previousSend;

                    // calculate the milliseconds 
                    ms = (int)span.TotalMilliseconds;

                    // send out location to the serial port
                    string lineType = line.Substring(30, 1);

                    if (lineType.Equals("L"))
                    {
                        locationLat = Convert.ToDouble(line.Substring(0, 11));
                        locationLon = Convert.ToDouble(line.Substring(12, 11));
                        // Send this out LilyGO location out the serial line if it's open
                        if (mySerialPort != null)
                        {
                            sendSerial(line, mySerialPort, eolChar);
                        }
                    }

                    // handle the replay pause for the $ packets
                    if (lineType.Equals("$"))
                    {
                        balloonLat = Convert.ToDouble(line.Substring(0, 11));
                        balloonLon = Convert.ToDouble(line.Substring(12, 11));
                        angle = angleBetweenEarthCoordinates(locationLat, locationLon, balloonLat, balloonLon);
                        distance = distanceInKmBetweenEarthCoordinates(locationLat, locationLon, balloonLat, balloonLon);
                        if (dt1 > lastSerialReplay.AddSeconds(serialReplayPauseInt))
                        {
                            // Send this out the serial line if it's open
                            if (mySerialPort != null)
                            {
                                sendSerial(line, mySerialPort, eolChar);
                            }
                            lastSerialReplay = dt1;
                        }
                    }

                    // zero out the negative times (first line)
                    if (ms < 0)
                    {
                        ms = 0;
                    }

                    // write the line to the screen
                    Console.WriteLine(line + " " + $"{distance:000.0000}" + " " + $"{angle:+000.000000;-000.000000}");

                    // Pause the packets to match previous timing
                    System.Threading.Thread.Sleep(ms);
                    previousSend = dt1;

                    //Read the next line
                    line = sr.ReadLine();
                }

                //close the file
                sr.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Ending Replay Flight.");
            }

            // Exit the program
            Environment.Exit(0);
        }

        // Send the calibration routine to the screen/file/serial
        private static void Calibrate(double cpuLat, double cpuLon, double cpuAlt, SerialPort? mySerialPort, string eolChar)
        {
            Double delta = .001;
            Console.WriteLine("Calibrate Routine");
            sendScreen("Sending LilyGO location: " + formatInfo(cpuLat, cpuLon, cpuAlt, "L"));
            if (mySerialPort != null)
            {
                sendSerial(formatInfo(cpuLat, cpuLon, cpuAlt, "L"), mySerialPort, eolChar);
            }

            string outputLineD = formatInfo(cpuLat + delta, cpuLon, cpuAlt, "$");
            sendScreen("Pointing North - Waiting 10 seconds - " + outputLineD);
            if (mySerialPort != null)
            {
                sendSerial(outputLineD, mySerialPort, eolChar);
            }
            System.Threading.Thread.Sleep(10000);
            outputLineD = formatInfo(cpuLat, cpuLon + delta, cpuAlt, "$");
            sendScreen("Pointing East - Waiting 10 seconds - " + outputLineD);
            if (mySerialPort != null)
            {
                sendSerial(outputLineD, mySerialPort, eolChar);
            }
            System.Threading.Thread.Sleep(10000);

            outputLineD = formatInfo(cpuLat - delta, cpuLon, cpuAlt, "$");
            sendScreen("Pointing South - Waiting 10 seconds - " + outputLineD);
            if (mySerialPort != null)
            {
                sendSerial(outputLineD, mySerialPort, eolChar);
            }
            System.Threading.Thread.Sleep(10000);

            outputLineD = formatInfo(cpuLat, cpuLon - delta, cpuAlt, "$");
            sendScreen("Pointing West - Waiting 10 seconds - " + outputLineD);
            if (mySerialPort != null)
            {
                sendSerial(outputLineD, mySerialPort, eolChar);
            }
            System.Threading.Thread.Sleep(10000);

            outputLineD = formatInfo(cpuLat, cpuLon, cpuAlt + 1000, "$");
            sendScreen("Pointing Up - " + outputLineD);
            if (mySerialPort != null)
            {
                sendSerial(outputLineD, mySerialPort, eolChar);
            }

            // Exit the program
            Environment.Exit(0);
        }

        // send the line to the screen
        private static void sendScreen(string formattedString)
        {
            Console.WriteLine($"{formattedString}");
        }

        // send the line to the log file
        private static void sendFile(string formattedString)
        {
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "ChaseMappr.txt".
            using (StreamWriter outputFile = File.AppendText(Path.Combine(docPath, "ChaseMappr.txt")))
            {
                outputFile.WriteLine($"{formattedString}");
            }
        }

        // send the line to the serial port
        public static void sendSerial(string outputData, SerialPort mySerialPort, string eolChar)
        {
            byte[] bufferData = System.Text.Encoding.UTF8.GetBytes(outputData + eolChar);
            mySerialPort.Write(bufferData, 0, bufferData.Length);
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

        // format the output to match the Gordon Cooper requirements
        public static string formatInfo(double lat, double lon, double alt, string lev)
        {
            string linedata = $"{lat:+000.000000;-000.000000},";
            linedata += $"{lon:+000.000000;-000.000000},";
            linedata += $"{alt:00000},";
            linedata += $"{lev},";
            linedata += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
            return linedata;
        }

        public static double degreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double radiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double distanceInKmBetweenEarthCoordinates(double lat1, double lon1, double lat2, double lon2)
        {
            double earthRadiusKm = 6371;

            double dLat = degreesToRadians(lat2 - lat1);
            double dLon = degreesToRadians(lon2 - lon1);

            lat1 = degreesToRadians(lat1);
            lat2 = degreesToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        public static double angleBetweenEarthCoordinates(double lat1, double lon1, double lat2, double lon2)
        {
            double dy = lat2 - lat1;
            double dx = Math.Cos(degreesToRadians(lat1)) * (lon2 - lon1);
            double angle = Math.Atan2(dy, dx);

            return radiansToDegrees(angle);
        }

    }
}