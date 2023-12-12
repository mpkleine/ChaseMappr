# ChaseMappr

Executive Summary
This program will receive ChaseMapper data via UDP from the RDZSonde devices, store the JSON data to a text file, deserialize the JSON data, display the data to the console, and send the balloon lat, lon, and altitude data out a serial port (USB) and store the data in the ChaseMappr.txt file in the Documents folder. This program feeds external devices like the Gordon Cooper High Altitude Balloon Pointing Device.<br>

<b>Installation</b><br>
This program is supplied as a ZIP file. Make a folder somewhere on your computer and unzip the ChaseMappr.zip file to that folder. Since this is a "server" program, the first time this program is executed, Windows asks your permission to let packets through the firewall system. You must check all three networks, domain, public, and local to let all packets through to the computer. This was compiled using .NET 8 and should run on Windows, Linux, and Mac's which have the .NET 8 system loaded. Windows should have this all ready to go, but if you receive an error about the .NET 8 version in your machine, it can be downloaded from this link https://dotnet.microsoft.com/en-us/download/dotnet/8.0

<b>Configuration</b><br>
Located in the installation folder is a configuration file that loads when the ChaseMappr program starts. This file is called appsettings.json with the following information.<br>
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
  "locationPause": "60",<br>
  "eolChar": "\n",<br>
  "serialReplayPause": "5"<br>
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
<b>eolChar</b> is an end-of-line character parameter for the serial output. You can specify newline - "\n", carriage return - "\r", or newline and carriage return - "\n\r" as the end-of-line character.<br>
<b>serialReplayPause</b> is the minimum number of seconds between the balloon packets on the serial output during a replay run. This slows the typical 1 packet per second rate to a speed the location pointer can handle.<br>

<b>LilyGO Setup</b><br>
From the Configuration panel, you need to enable the ChaseMapper function, insert the IP address of the computer running ChaseMappr, and enter the port number used for Chasemappr. 

<b>Input Data</b><br>
The LilyGO ChaseMappr feature sends JSON packets like these when they are received from the balloons:<br>
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18246,"longitude": -97.43690,"altitude": 443,"speed": 26,"heading": 50,"time": "11:08:17","model": "RS41","freq": "404.200 MHz"}<br>
{ "type": "PAYLOAD_SUMMARY","callsign": "V2821133","latitude": 35.18256,"longitude": -97.43678,"altitude": 448,"speed": 30,"heading": 39,"time": "11:08:18","model": "RS41","freq": "404.200 MHz"}<br>
These can come as frequently as 1 per second.

<b>Output Data</b><br>
The Gordon Cooper project requires data in the following fixed text format. Characters 1 through 11 are the latitude, 13 through 13 are the longitude, 25 through 29 is the altitude (in meters), and character 31 is the data source. "L" for the LilyGO location (The LilyGO location is sent at the start of the program and every yy seconds (as specified in the configuration file), "$" is for the balloon location. The timestamp is when the transaction is received. Then the distance in km is listed, and finally, the angle from the LilyGO to the balloon is shown in degrees (0 degrees is east).<br>

+035.379917,-096.907845,00325,L,2023-12-04 19:28:38.058<br>
+034.806380,-096.087100,03593,$,2023-12-04 19:28:43.077,098.1991,+139.599927<br>
+034.806230,-096.086640,03553,$,2023-12-04 19:28:46.029,098.2418,+139.608428<br>
+034.806070,-096.086160,03515,$,2023-12-04 19:28:49.066,098.2866,+139.617121<br>
+034.806020,-096.086000,03503,$,2023-12-04 19:28:50.027,098.3013,+139.620180<br>
+034.805800,-096.085160,03438,$,2023-12-04 19:28:55.044,098.3754,+139.638303<br>
+034.805650,-096.084500,03385,$,2023-12-04 19:28:59.036,098.4320,+139.653641<br>
+034.805550,-096.083990,03349,$,2023-12-04 19:29:02.042,098.4745,+139.666258<br>
+034.805480,-096.083640,03324,$,2023-12-04 19:29:04.066,098.5038,+139.674844<br>
+034.805420,-096.083270,03298,$,2023-12-04 19:29:06.041,098.5338,+139.684599<br>
+034.805390,-096.083090,03285,$,2023-12-04 19:29:07.054,098.5484,+139.689303<br>

<b>Operation</b><br>
To move the data through the system, configure the program and run ChaseMappr.exe from the command line. The program must stay in operation while the balloons are in flight. An Arduino sketch is available in the file section to show how to read a serial port and a video at this link [https://vimeo.com/893751948?share=copy#t=0] shows ChaseMappr and the sketch in operation.

<b>Calibration Routine</b><br>
If you add a command line parameter of "calibrate" this program runs through a calibration scheme to test the data connection of the pointing device. It will output:<br>
a point north of this location and pause for 10 seconds<br>
a point east and pause for 10 seconds<br>
a point south and pause for 10 seconds<br>
a point west and pause for 10 seconds<br>
and finally point straight up. The program will then exit.<br>

<b>Replay Routine</b><br>
If you add a command line parameter of "replay" this program runs through a replay routine to send a past balloon flight to the serial connection of the pointing device. It will output all of the LilyGO and balloon locations for the run stored in the ChaseMapprReplay.txt file located in the documents folder. Note that there is a separate replay pause variable so you can change the pause between packets sent to the serial port on replay.<br>

<b>Version History</b><br>
<b>Version 1.5.0</b> - 12/4/2023 - Added the distance in km and direction in degrees (east is 0 degrees) from the LilyGO location to the balloon.<br>
<b>Version 1.4.0</b> - 12/3/2023 - Added a replay feature that lets a user replay a past balloon flight.<br>
<b>Version 1.3.0</b> - 12/03/2023 - Added the date/time to the output streams, screen/file/serial. Added an end-of-line character parameter to the configuration file. The user can specify newline - "\n", carriage return - "\r", or newline and carriage return - "\n\r" end of line character.<br>
<b>Version 1.2.0</b> - 12/01/2023 - Added the calibrate routine.<br>
<b>Version 1.1.0</b> - 12/01/2023 - Added the pause routines on serial data for both the packets and the location lines. Also added error trapping for serial port failures. A message states the serial port failed and all serial transmissions are canceled. Also added a display of all input parameters as the program starts.<br>
<b>Version 1.0.0</b> - 11/30/2023 - Initial functional release.
