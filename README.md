# ChaseMappr

Executive Summary
This program will receive ChaseMapper data via UDP from the RDZSonde devices, store the JSON data to a text file, deserialize the JSON data, display the data to the console, and send the balloon lat, lon, and altitude data out a serial port (USB) and store the data in the ChaseMappr.txt file in the Documents folder. This program feeds external devices like the Gordon Cooper High Altitude Balloon Pointing Device.<br>

<b>Installation</b><br>
This program is supplied as a ZIP file. Make a folder somewhere on your computer and unzip the ChaseMappr.zip file to that folder. Since this is a "server" program, the first time this program is executed, Windows asks your permission to let packets through the firewall system. You must check all three networks, domain, public, and local to let all packets through to the computer. This was compiled using .NET 8 and should run on Windows, Linux, and Mac's which have the .NET 8 system loaded. Windows should have this all ready to go, but if you receive an error about the .NET 8 version in your machine, it can be downloaded from this link https://dotnet.microsoft.com/en-us/download/dotnet/8.0

<b>Configuration</b><br>
Located in the installation folder is a configuration file that loads when the ChaseMappr program starts. This file is called appsetting.json with the following information.<br>
{<br>
  "udpPort": "11000",<br>
  "cpuLatitude": "35.379917",<br>
  "cpuLongitude": "-96.907845",<br>
  "cpuAltitude": "325",<br>
  "serialPort": "COM6",<br>
  "serialBaud": "9600",<br>
  "serialData": "8",<br>
  "serialParity": "None",<br>
  "serialStop": "1",<br>
  "serialPacketPause": "10",<br>
  "locationPause": "60"<br>
}<br>
<b>udpPort</b> is the port that this program listens to.<br>
<b>cpuLatitude</b> is the latitude of the LilyGO computer. Be sure to change this when you move around.<br>
<b>cpuLongitude</b> is the longitude of the LilyGO computer. Be sure to change this when you move around.<br>
<b>cpuAltitude</b> is the altitude (in meters) of the LilyGo computer. Be sure to change this when you move around.<br>
<b>serialPort</b> is the port this system uses<br>
<b>serialBaud</b> is the baud rate this system uses<br>
<b>serialData</b> is the number of databits this system uses<br>
<b>serialParity</b> is the parity this system uses. Options are Mark, Space, Even, Odd, and None. The default is None if an invalid entry is provided.<br>
<b>serialStop</b> is the number of stop bits this system uses. Options are 1 or 2. The default is 1 if an invalid entry is provided.<br>
<b>serialPacketPause</b> is the minimum number of seconds between the balloon packets on the serial output. This slows the typical 1 packet per second rate to a speed the location pointer can handle.<br>
<b>locationPause</b> is the minimum number of seconds between the location packets on the output. This varies the frequency the LilyGO location is sent.<br>

<b>LilyGO Setup</b><br>
From the Configuration panel, you need to enable the ChaseMapper function, insert the IP address of the computer running ChaseMappr, and enter the port number used for Chasemappr. 

<b>Input Data</b><br>
The LilyGO ChaseMappr feature sends JSON packets like these when they are received from the balloons:<br>
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18246,"longitude": -97.43690,"altitude": 443,"speed": 26,"heading": 50,"time": "11:08:17","model": "RS41","freq": "404.200 MHz"}<br>
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18256,"longitude": -97.43678,"altitude": 448,"speed": 30,"heading": 39,"time": "11:08:18","model": "RS41","freq": "404.200 MHz"}<br>
These can come as frequently as 1 per second.

<b>Output Data</b><br>
The Gordon Cooper project requires data in the following fixed text format. Characters 1 through 11 are the latitude, 13 through 13 are the longitude, 25 through 29 is the altitude (in meters), and character 31 is the data source. "L" for the LilyGO location (The LilyGO location is sent at the start of the program and every 10 balloon packets. "$" is for the balloon location. <br>

+035.379917,-096.907845,00325,L<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.379917,-096.907845,00325,L<br>
+035.210820,-097.421100,02141,$<br>
+035.379917,-096.907845,00325,L<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>
+035.210820,-097.421100,02141,$<br>

<b>Operation</b><br>
To move the data through the system, configure the program and run ChaseMappr.exe from the command line. The program must stay in operation while the balloons are in flight.

<b>Calibration Routine</b><br>
If you add a command line parameter of "calibrate" this program runs through a calibration scheme to test the data connection to the pointing device. It will output:<br>
a point north of this location and pause for 10 seconds<br>
a point east and pause for 10 seconds<br>
a point south and pause for 10 seconds<br>
a point west and pause for 10 seconds<br>
and finally point straight up. The program will then exit.<br>

<b>Version History</b><br>
Version 1.2.0 - 12/01/2023 - Added the calibrate routine.
Version 1.1.0 - 12/01/2023 - Added the pause routines on serial data for both the packets and the location lines. Also added error trapping for serial port failures. A message states the serial port failed and all serial transmissions are canceled. Also added a display of all input parameters as the program starts.
Version 1.0.0 - 11/30/2023 - Initial functional release.
