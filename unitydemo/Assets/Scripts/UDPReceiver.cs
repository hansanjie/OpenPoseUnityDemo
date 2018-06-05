using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace opdemo
{
    public class UDPReceiver : MonoBehaviour
    {
        // Singleton
        private static UDPReceiver instance = null;

        // Threading
        private bool CloseThreadFlag = true;
        private Mutex ReceivingMutex = new Mutex();
        private Thread receiveThread;

        // UDP receiver
        private UdpClient client;
        public int port = 8051; // define > init

        // Data & timing
        public float FrameDelayCoef = 1.2f;
        private string receivedData = "";
        private float avgFrameTime = float.PositiveInfinity;
        private float currentFrameLength = 0f;
        private bool serverActive = false;
        private bool newDataFlag = false;

        // Interface
        public static int PortNumber { get { return instance.port; } }
        public static string ReceivedData { get { return instance.receivedData; } }
        public static float AvgFrameTime { get { return instance.avgFrameTime * instance.FrameDelayCoef; } }
        public static float CurrentFrameLength { get { return instance.currentFrameLength; } }
        public static float EstimatedRestFrameTime { get { return AvgFrameTime - CurrentFrameLength; } }
        public static bool IsDataNew()
        {
            return instance.newDataFlag;
        }
        public static void BeginReceiving()
        {
            instance.StartReceivingThread();
        }
        public static void StopReceiving()
        {
            instance.StopReceivingThread();
        }
        public static bool IsRunning { get { return !instance.CloseThreadFlag; } }

        // Private
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (Controller.Mode == PlayMode.Stream) BeginReceiving(); // Automatic start receiving
        }

        private void StartReceivingThread()
        {
            if (!CloseThreadFlag) return; // already started

            CloseThreadFlag = false;
            receiveThread = new Thread(new ThreadStart(Receiving));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void Receiving()
        {
            Debug.Log("Receive start");
            ReceivingMutex.WaitOne();

            client = new UdpClient(port);
            while (!CloseThreadFlag)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = client.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    InputText(text);
                }
                catch (Exception err)
                {
                    Debug.Log("Receive error: " + err.ToString());
                }
            }

            Debug.Log("Receive end");
            ReceivingMutex.ReleaseMutex();
        }

        private void StopReceivingThread()
        {
            if (CloseThreadFlag) return; // already stopped

            client.Close();
            CloseThreadFlag = true;

            ReceivingMutex.WaitOne();
            receiveThread.Abort();
            Debug.Log("thread aborted");
            ReceivingMutex.ReleaseMutex();
        }

        private void InputText(string text)
        {
            //Debug.Log(">> " + text);
            receivedData = text;
            newDataFlag = true;

            // timing
            float thisFrameTime = currentFrameLength;
            currentFrameLength = 0f;
            if (avgFrameTime == float.PositiveInfinity)
            {
                if (!serverActive) // first package
                {
                    serverActive = true;
                } else // second package
                {
                    avgFrameTime = thisFrameTime;
                }
            } else // continueous receiving
            {
                avgFrameTime = 0.75f * avgFrameTime + 0.25f * thisFrameTime;
            }
        }

        private void Update()
        {
            if (serverActive)
            {
                currentFrameLength += Time.deltaTime;
            }
            if (currentFrameLength > 5f) // threshold to inactive
            {
                serverActive = false;
                avgFrameTime = float.PositiveInfinity;
                currentFrameLength = 0f;
            }
        }

        private void LateUpdate()
        {
            newDataFlag = false;
        }

        // In case of abrupt exit of program
        private void OnDestroy()
        {
            StopReceiving();
        }
    }
}
