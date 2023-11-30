// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System.Text;

// Program to listen on port 11000 for CHASEMAPPER UDP packets from the LilyGO RDZSonde device
// and write them to a file.
// mpk - 11/28/2023

// Display the header
Console.WriteLine("ChaseMappr: UDP Listener");

// Create the socket
UdpClient udpServer = new UdpClient(11000);

// Loop forever
while (true)
{
    var remoteEP = new IPEndPoint(IPAddress.Any, 11000);
    var data = udpServer.Receive(ref remoteEP); // listen on port 11000
    string someString = Encoding.ASCII.GetString(data);
    outputJSONLine(someString);

    Console.WriteLine("receive data from " + remoteEP.ToString() + " at " + DateTime.Now.ToString("yyyy/M/d h:mm:ss.fff tt"));
    Console.WriteLine(someString);
    Console.WriteLine("");
//    udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
}

void outputJSONLine(string someString)
{
    // Set a variable to the Documents path.
    string docPath =
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    // Write the string array to a new file named "WriteLines.txt".
    using (StreamWriter outputFile = File.AppendText(Path.Combine(docPath, "WriteLines.txt")))
    {
            outputFile.WriteLine(DateTime.Now.ToString("yyyy/M/d h:mm:ss.fff tt"));
            outputFile.WriteLine(someString);
    }
}