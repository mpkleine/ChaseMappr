# ChaseMappr

Executive Summary
This program will receive ChaseMapper data via UDP from the RDZSonde devices, store the JSON data to a text file, deserialize the JSON data, display the data to the console, and send the balloon lat, lon, and altitude data out a serial port (USB) and store the data in the ChaseMappr.txt file in the Documents folder. This program feeds external devices like the Gordon Cooper High Altitude Balloon Pointing Device.

Installation
This program is supplied as a ZIP file. Make a folder somewhere on your computer and unzip the ChaseMappr.zip file to that folder. Since this is a "server" program, the first time this program is executed, Windows asks your permission to let packets through the firewall system. You must check all three networks, domain, public, and local to let all packets through to the computer.

Configuration
Located in the installation folder is a configuration file that loads when the ChaseMappr program starts. This file is called appsetting.json with the following information.
{
  "udpPort": "11000",
  "cpuLatitude": "35.379917",
  "cpuLongitude": "-96.907845",
  "cpuAltitude": "325",
  "serialPort": "COM6",
  "serialBaud": "9600",
  "serialData": "8",
  "serialParity": "None",
  "serialStop": "1"
}
udpPort is the port that this program listens to.
cpuLatitude is the latitude of the LilyGO computer. Be sure to change this when you move around.
cpuLongitude is the longitude of the LilyGO computer. Be sure to change this when you move around.
cpuAltitude is the altitude (in meters) of the LilyGo computer. Be sure to change this when you move around.
serialPort is the port this system uses
serialBaud is the baud rate this system uses
serialData is the number of databits this system uses
serialParity is the parity this system uses. Options are Mark, Space, Even, Odd, and None. The default is None if an invalid entry is provided.
serialStop is the number of stop bits this system uses. Options are 1 or 2. The default is 1 if an invalid entry is provided.

LilyGO Setup
From the Configuration panel, you need to enable the ChaseMapper function, insert the IP address of the computer running ChaseMappr, and enter the port number used for Chasemappr. 

Input Data
The LilyGO ChaseMappr feature sends JSON packets like these when they are received from the balloons:
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18246,"longitude": -97.43690,"altitude": 443,"speed": 26,"heading": 50,"time": "11:08:17","model": "RS41","freq": "404.200 MHz"}
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18256,"longitude": -97.43678,"altitude": 448,"speed": 30,"heading": 39,"time": "11:08:18","model": "RS41","freq": "404.200 MHz"}
These can come as frequently as 1 per second.

Output Data
The Gordon Cooper project requires data in the following fixed text format. Characters 1 through 11 are the latitude, 13 through 13 are the longitude, 25 through 29 is the altitude (in meters), and character 31 is the data source. "L" for the LilyGO location. "$" is for the balloon location. 

+035.379917,-096.907845,00325,L
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.379917,-096.907845,00325,L
+035.210820,-097.421100,02141,$
+035.379917,-096.907845,00325,L
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$
+035.210820,-097.421100,02141,$

Operation
To move the data through the system, configure the program and run ChaseMappr.exe from the command line. The program must stay in operation while the balloons are in flight.
