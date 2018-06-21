using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GyroCapture : MonoBehaviour {

	[Serializable]
	public class ControllerStatus
	{
		public QuaternionSerializable quaternion;
		public bool trigger;
		public bool trackpadButton;
		public bool back;
		public Vector2Serializable trackpad;
		public bool trackpadTouched;
		public bool extra1;
		public bool extra2;
		public bool extra3;
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
	
	[Serializable]
	public class Vector2Serializable
	{
		public float x, y;

		public static Vector2Serializable FromVector2(Vector2 v)
		{
			return new Vector2Serializable {x = v.x, y = v.y};
		}

		public Vector2 ToVector2()
		{
			return new Vector2(x,y);
		}
	}


	Gyroscope gyro;
	private Quaternion referenceQuaternion;
	public Text text;
    private int connectedCant;
    public NetworkController networkController;
    private byte[] buffer = new byte[1000];

	private ControllerStatus controllerStatus = new ControllerStatus();

	public Image trackpadImage;
	
	private PointerEventData pointerEventData;

	public EventSystem eventSystem;

    public bool IsConnected
    {
        get { return connectedCant > 0; }
    }
    
	void Start()
	{
		pointerEventData = new PointerEventData(eventSystem);
		//Set up and enable the gyroscope (check your device has one)
		gyro = Input.gyro;
		gyro.enabled = true;
		referenceQuaternion = gyro.attitude;
		
	    
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
		var current = gyro.attitude;
		controllerStatus.quaternion = QuaternionSerializable.FromQuaternion(current);
		CheckTrackpad();
		
		stringBuilder.AppendFormat("Attitude: {0}\nTrigger: {1}\nTrackButton: {2}\nExtra1: {3}\nExtra2: {4}\nExtra3: {5}\nTrackpad: {6}\nTrackpadTouched: {7}\nConnected: {8}", current,controllerStatus.trigger, controllerStatus.trackpadButton, controllerStatus.extra1, controllerStatus.extra2, controllerStatus.extra3, controllerStatus.trackpad.ToVector2(), controllerStatus.trackpadTouched, connectedCant);
		text.text = stringBuilder.ToString();
		
		if (!IsConnected)
			return;
            
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();
		
		
		formatter.Serialize(stream, controllerStatus);
		int bufferSize = 1024;
		networkController.SendMessage(buffer, bufferSize);
	}

	public void Recenter()
	{
		referenceQuaternion = gyro.attitude;
	}

	public void SetTrigger(bool pressed)
	{
		this.controllerStatus.trigger = pressed;
	}
	
	public void SetTrackpadButton(bool pressed)
	{
		this.controllerStatus.trackpadButton = pressed;
	}
	
	public void SetExtraButton1(bool pressed)
	{
		this.controllerStatus.extra1 = pressed;
	}
	
	public void SetExtraButton2(bool pressed)
	{
		this.controllerStatus.extra2 = pressed;
	}
	
	public void SetExtraButton3(bool pressed)
	{
		this.controllerStatus.extra3 = pressed;
	}
	
	public List<Vector2> touchPositions = new List<Vector2>();
	
	
	public void CheckTrackpad()
	{
		touchPositions.Clear();

		for (int i = 0; i < Input.touchCount; i++)
		{
			var touch = Input.GetTouch(i);
			touchPositions.Add(touch.position);
		}

		if (touchPositions.Count == 0)
		{
			if (Input.GetMouseButton(0))
			{
				touchPositions.Add(Input.mousePosition);
			}
		}
		
		bool trackpadTouched = false;
		List<RaycastResult> results = new List<RaycastResult>();
		for (int i = 0; i < touchPositions.Count; i++)
		{
			results.Clear();
			var touchPosition = touchPositions[i];

			pointerEventData.position = touchPosition;

			eventSystem.RaycastAll(pointerEventData, results);
			
			foreach (RaycastResult result in results)
			{
				if (result.gameObject == trackpadImage.gameObject)
				{
					

					var trackpadRectTransform = trackpadImage.rectTransform;
					Vector2 localCursor;
					RectTransformUtility.ScreenPointToLocalPointInRectangle(trackpadRectTransform, pointerEventData.position, pointerEventData.enterEventCamera, out localCursor);
					localCursor.x = localCursor.x / (trackpadRectTransform.rect.width / 2.0f);
					localCursor.y = localCursor.y / (trackpadRectTransform.rect.height / 2.0f);

					var magnitude = localCursor.magnitude;
					
					localCursor.Normalize();

					if (magnitude <= 1)
					{
						controllerStatus.trackpadTouched = true;
						controllerStatus.trackpad = Vector2Serializable.FromVector2(localCursor);
						trackpadImage.color = Color.green;
						
						return;
					}
				}
			}
		}

		trackpadImage.color = Color.red;
		controllerStatus.trackpad = Vector2Serializable.FromVector2(Vector2.zero);
		controllerStatus.trackpadTouched = false;

	}
}
