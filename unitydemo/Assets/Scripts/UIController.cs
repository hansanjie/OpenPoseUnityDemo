using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo
{
    public class UIController : MonoBehaviour
    {

        [SerializeField] Text IPText;

        [SerializeField] GameObject InterpolationToggle;
        [SerializeField] GameObject StepsNumber;

        // Use this for initialization
        void Start()
        {
            switch (Controller.Mode)
            {
                case PlayMode.Stream: InitStreamUI(); break;
                case PlayMode.FileJson: InitBvhJsonUI(); break;
                case PlayMode.FileBvh: InitBvhJsonUI(); break;
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
            StepsNumber.SetActive(true);
            InterpolationToggle.SetActive(false);
        }

        private void InitBvhJsonUI()
        {
            StepsNumber.SetActive(false);
            InterpolationToggle.SetActive(true);
        }
    }

}
