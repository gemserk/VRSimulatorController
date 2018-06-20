using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class GyroReplay : MonoBehaviour
    {
        public Transform targetTransform;
        public Text text;
        public GameObject pressIndicator;

        public NetworkController networkController;

        public void Start()
        {
            networkController.onMessageReceived += OnMessageReceived;
            
            networkController.onConnected += () =>
            {
                Debug.LogFormat("TransformSync:  connected");
            };
            networkController.onDisconnected += () =>
            {
                Debug.LogFormat("TransformSync: disconnected");
            };
            
            networkController.Init();
            networkController.Connect();
        }

        private void OnMessageReceived(byte[] bytes, int i)
        {
            Debug.LogFormat("Received transform message");
            Stream stream = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            var message = formatter.Deserialize(stream) as GyroCapture.ControllerStatus;

            var quaternion = message.quaternion.ToQuaternion();
            targetTransform.localRotation = quaternion;
            text.text = quaternion.ToString();
            pressIndicator.SetActive(message.triggerDown);
        }
    }
}