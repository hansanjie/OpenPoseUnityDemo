# Instructions for opUnityDemo : Windows

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

## Windows firewall setting to enable UDP
1. Open the application and choose "Streaming" mode. 
2. If the "Windows Security Alert" pops up, check both the checkboxes (Private networks & Public networks) and click "Allow access" (see instruction_p1). 
3. Run OpenPose program with UDP: "--udp_host <your_IP> --udp_port <your_port>" which could be found in the Unity application.
4. Test if it works. If not, do following steps. 
5. Open "wf.msc" in Windows search, which is supposed to be "Windows Firewall with Advanced Security". 
6. Select "Inbound Rules" tab, find the application (you may see two same names). 
7. Doubleclick to open "Properties" window (or Right click -> Properties). 
8. In "General" tab, select "Allow the connection" in "Action" field. Click "OK". 
9. Restart and try again.

## Application operations guide
	Key	/ Mouse	|		Function
------------------------------------------------------------------
	MouseDrag	|		Rotate camera view angle
	MouseScroll	|		Zoom in / out camera
-------------------------------------------------------------------
	Esc			|		Back to the menu / Quit the application
	C 			|		Reset the character position to center
	V 			|		Reset the character rotation to vertical
	I 			|		Enable / disable interpolation
	< >			|		Change the character model (not in use)
	M /			|		Change the scene model (not in use)
----------------------------------------------------------------
	Space		|		Start / pause / Resume animation play (only in BVH and JSON mode)
	R 			|		Reset animation to start position (only in BVH and JSON mode)
	LeftArrow	|		Last frame (only in BVH and JSON mode)
	RightArrow	|		Next frame (only in BVH and JSON mode)
	UpArrow		|		Speed up animation (only in BVH and JSON mode)
	DownArrow	|		Slow down animation (only in BVH and JSON mode)

## Project setup instructions - if you want to open in Unity
1. Clone the whole repository.
2. Open the project with Unity.
4. Do the Firewall Seting for "Unity Edotr" in "wf.msc" (you may refer to the previous section). 

## Software
Developed in Unity 2018.1.5f1 Personal.

## 3rd Party Packages:
TriLib (not in use so far): http://ricardoreis.net/?p=14
RockVR video capture: https://assetstore.unity.com/packages/tools/video/video-capture-75653
File browser: https://assetstore.unity.com/packages/tools/gui/file-browser-windows-macos-98716