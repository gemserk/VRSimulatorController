using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkController : MonoBehaviour
{
	public int listenPort;

	public bool isHost;
	
	public string ip;
	public int connectToPort;
	
	int connectionId;
	
	int myReliableChannelId;
	int socketId; // above Start()

	public int assignedListenPort;
	
	public Action<byte[], int> onMessageReceived;
	public Action onConnected;
	public Action onDisconnected;

	public void Init()
	{
		NetworkTransport.Init();
		ConnectionConfig config = new ConnectionConfig();
		myReliableChannelId = config.AddChannel(QosType.Reliable); // within Start()
		int maxConnections = 10;
		HostTopology topology = new HostTopology(config, maxConnections);
		socketId = NetworkTransport.AddHost(topology,listenPort);
		assignedListenPort = NetworkTransport.GetHostPort(socketId);
		Debug.Log(this.gameObject.name + ": - " + "Socket Open. SocketId is: " + socketId + " port: " + assignedListenPort);
	}


	[ContextMenu("Connect")]
	public void Connect() {
		byte error;
		connectionId = NetworkTransport.Connect(socketId, ip, connectToPort, 0, out error);
		Debug.Log(this.gameObject.name + ": - " + "Connected to server. ConnectionId: " + connectionId + " error: " + error);
	}
	
	[ContextMenu("Send")]
	public void SendSocketMessage() {
		byte error;
		byte[] buffer = new byte[1024];
		Stream stream = new MemoryStream(buffer);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(stream, "HelloServer from " + this.gameObject.name);

		int bufferSize = 1024;

		SendMessage(buffer, bufferSize);
	}

	public void SendMessage(byte[] buffer, int size)
	{
		byte error;
		NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, size, out error);
	}

	void Update()
	{
		int recHostId;
		int recConnectionId;
		int recChannelId;
		byte[] recBuffer = new byte[1024];
		int bufferSize = 1024;
		int dataSize;
		byte error;

		while (true)
		{
			NetworkEventType recNetworkEvent = NetworkTransport.ReceiveFromHost(socketId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);

			switch (recNetworkEvent)
			{
				case NetworkEventType.Nothing:
					return;
					break;
				case NetworkEventType.ConnectEvent:
					if (this.isHost)
					{
						connectionId = recConnectionId;
					}
					onConnected();
					break;
				case NetworkEventType.DataEvent:
					onMessageReceived(recBuffer, dataSize);
					break;
				case NetworkEventType.DisconnectEvent:
					onDisconnected();
					break;
			}
		}
	}
	
	
}
