using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo
{
    public class UIController : MonoBehaviour
    {

        [SerializeField] Text IPText;

        // Use this for initialization
        void Start()
        {
            switch (Controller.Mode)
            {
                case PlayMode.Stream: InitStreamUI(); break;
            }
        }

        private void InitStreamUI()
        {
            // IP & port
            string ipAddress = "No network adapters with an IPv4 address in the system!";
            string port = UDPReceiver.PortNumber.ToString();
            System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                }
            }
            IPText.text = "IP: " + ipAddress + "\nPort: " + port;
        }
    }

}
