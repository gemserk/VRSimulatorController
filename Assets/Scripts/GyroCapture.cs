using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GyroCapture : MonoBehaviour {

	[Serializable]
	public class ControllerStatus
	{
		public QuaternionSerializable quaternion;
		public bool triggerPressed;
		public bool triggerDown;
	}
	
	
	[Serializable]
	public class QuaternionSerializable
	{
		public float x, y, z, w;

		public static QuaternionSerializable FromQuaternion(Quaternion q)
		{
			return new QuaternionSerializable {x = q.x, y = q.y, z = q.z, w = q.w};
		}

		public Quaternion ToQuaternion()
		{
			return new Quaternion(x,y,z,w);
		}
	}


	Gyroscope gyro;
	public Text text;
    private int connectedCant;
    public NetworkController networkController;
    private byte[] buffer = new byte[1000];

    public bool IsConnected
    {
        get { return connectedCant > 0; }
    }
    
	void Start()
	{
		//Set up and enable the gyroscope (check your device has one)
		gyro = Input.gyro;
		gyro.enabled = true;
	    
	    networkController.onConnected += () =>
	    {
	        Debug.LogFormat("TransformSync:  connected");
	        connectedCant++;
	    };
	    networkController.onDisconnected += () =>
	    {
	        Debug.LogFormat("TransformSync: disconnected");

	        connectedCant--;
	    };
		
		networkController.Init();
	}
	
	
	void Update () {
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Attitude: {0}", gyro.attitude);
		text.text = stringBuilder.ToString();
		
		if (!IsConnected)
			return;
            
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();

		var data = new ControllerStatus
		{
			quaternion = QuaternionSerializable.FromQuaternion(gyro.attitude),
			triggerDown = Input.GetMouseButton(0),
			triggerPressed = Input.GetMouseButtonDown(0)
		};
		
		
		formatter.Serialize(stream, data);
		int bufferSize = 1024;
		networkController.SendMessage(buffer, bufferSize);
	}
}
