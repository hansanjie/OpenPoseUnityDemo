# Instructions for opUnityDemo streaming: Windows

## Local cable network setting: 
### Windows: connect the cable and disable other networks. 
### Ubuntu: 
1. Connect the cable. 
2. Get to Network Manager -> Connections (or Network Connections)
3. Create new wired connection. 
4. under iPv4, use settings: 
	Method: Manual;
	Address: 169.254.123.105;
	Netmask: 255.255.0.0;
	Gateway: leave blank
5. Save and run to test.

Reference: https://unix.stackexchange.com/questions/251057/can-i-connect-a-ubuntu-linux-laptop-to-a-windows-10-laptop-via-ethernet-cable?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa

## Application setup instructions
1. Open opUnityDemo.exe
2. If the "Windows Security Alert" pops up, saying "Windows Firewall has blocked some features of this app", check both the checkboxes (Private networks & Public networks) and click "Allow access" (see instruction_p1)
3. Choose "Streaming" in the demo app. 
4. Copy the IP address and port number on screen, and add input arguments when starting OP: "--udp_host <your_IP> --udp_port <your_port>"
5. Run and test if the character in opUnityDemo moves along with the character in OP. 
6. If it works, everything is good so far and you can skip the rest. If it still doesn't work, try the following steps:
7. Open Windows start menu (or Windows search) and type "wf.msc", choose the first one, which is supposed to be "Windows Firewall with Advanced Security"
8. Select "Inbound Rules" in the left side bar, and find opUnityDemo.exe in the middle. There may have two opUnityDemo.exe as one for UDP and the other for TCP. You can do the same operations for both. 
9. Doubleclick the rule for opUnityDemo.exe (or right click -> Properties). In "General" tab, select "Allow the connection" in "Action" field. Then click OK. 
10. Restart the application and test again. 

## Application operations guide
	Key		|		Function
-----------------------------------------------------
	Esc		|		Back to the menu / Quit the application
	C 		|		Reset the character position to center
	I 		|		Enable / disable interpolation
	< >		|		Change the character model
	M /		|		Change the scene model
			|		.....Whatever else

## Project setup instructions
1. Clone the whole repository.
2. Obtain the "Models" (NOT available yet) and extracted into folder "unitydemo/Assets/Models".
3. Import the "Standard Assets/Environment" for the water effect. 
3. Open the project with Unity.

## Software
Developed in Unity 2018.1.1f1 Personal.

## 3rd Party Packages:
TriLib: http://ricardoreis.net/?p=14