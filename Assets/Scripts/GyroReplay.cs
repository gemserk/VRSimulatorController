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


        private Quaternion referenceQuaternion;
        private bool hasReference = false;
        

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

            if (!hasReference)
            {
                referenceQuaternion = quaternion;
                hasReference = true;
            }

            if (message.extra3)
            {
                referenceQuaternion = quaternion;
            }
            //referenceQuaternion.ToEulerAngles();

            text.text = string.Format("Attitude: {0}\nTrigger: {1}\nTrackButton: {2}\nExtra1: {3}\nExtra2: {4}\nExtra3: {5}\nTrackpad: {6}\nTrackpadTouched: {7}", quaternion,message.trigger, message.trackpadButton, message.extra1, message.extra2, message.extra3, message.trackpad.ToVector2(), message.trackpadTouched);
            
            var referenceTransformed = Quaternion.Inverse(GyroToUnity(referenceQuaternion));
            targetTransform.localRotation = referenceTransformed * GyroToUnity(quaternion);
            
            
            //targetTransform.localRotation = relative;
            //text.text = relative.ToString();
            pressIndicator.SetActive(message.trigger);
        }
        
        
        private static Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }
    }
}