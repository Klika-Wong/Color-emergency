using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// UDP接收分发器：负责接收Python手势指令并指挥主角动作
/// </summary>
public class UDPReceiver : MonoBehaviour
{
    [Header("网络配置")]
    [Tooltip("必须与Python代码中的UDP_PORT一致")]
    public int port = 5005;

    [Header("引用关联")]
    public PlayerMovement playerMovement;
    public item_collector itemCollector;

    private UdpClient client;
    private Thread receiveThread;
    private string lastData = "";
    private string receiveError = "";
    private readonly object dataLock = new object();
    private volatile bool stopping;

    private void Start()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (itemCollector == null) itemCollector = GetComponent<item_collector>();

        stopping = false;
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log($"UDP receiver starting on port {port}.");
    }

    private void ReceiveData()
    {
        UdpClient receiver = null;

        try
        {
            receiver = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
            lock (dataLock)
            {
                if (stopping)
                {
                    receiver.Close();
                    return;
                }

                client = receiver;
            }

            while (!stopping)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = receiver.Receive(ref anyIP);
                string incomingData = Encoding.UTF8.GetString(dataByte);

                lock (dataLock)
                {
                    lastData = incomingData;
                }
            }
        }
        catch (Exception e)
        {
            if (!stopping)
            {
                lock (dataLock)
                {
                    receiveError = e.Message;
                }
            }
        }
        finally
        {
            if (receiver != null)
            {
                receiver.Close();
                lock (dataLock)
                {
                    if (ReferenceEquals(client, receiver))
                    {
                        client = null;
                    }
                }
            }
        }
    }

    private void Update()
    {
        string currentData;
        string currentError;

        lock (dataLock)
        {
            currentData = lastData;
            lastData = "";
            currentError = receiveError;
            receiveError = "";
        }

        if (!string.IsNullOrEmpty(currentError))
        {
            Debug.LogWarning("UDP receiver stopped: " + currentError);
        }

        if (string.IsNullOrWhiteSpace(currentData)) return;

        var commands = new HashSet<string>(currentData.Split('_'));
        var allowedCommands = new HashSet<string> { "None", "Up", "Down", "Left", "Right", "E" };
        foreach (string command in commands)
        {
            if (!allowedCommands.Contains(command))
            {
                Debug.LogWarning("UDP receiver ignored an unknown command.");
                return;
            }
        }

        if (commands.Contains("None") && commands.Count != 1)
        {
            return;
        }

        if (commands.Contains("E"))
        {
            if (itemCollector != null)
            {
                itemCollector.TryCollect();
            }
            else
            {
                Debug.LogError("UDPReceiver could not find item_collector.");
            }
        }

        if (playerMovement != null)
        {
            if (commands.Contains("Up"))
            {
                playerMovement.TriggerJump();
            }

            bool left = commands.Contains("Left");
            bool right = commands.Contains("Right");
            if (left && right)
            {
                return;
            }

            if (commands.Contains("None"))
            {
                playerMovement.SetHorizontalDirection(0f);
            }
            else if (left)
            {
                playerMovement.SetHorizontalDirection(-1f);
            }
            else if (right)
            {
                playerMovement.SetHorizontalDirection(1f);
            }
        }
    }

    private void OnDestroy()
    {
        CloseUDP();
    }

    private void OnApplicationQuit()
    {
        CloseUDP();
    }

    private void CloseUDP()
    {
        UdpClient receiver;
        Thread thread;

        lock (dataLock)
        {
            stopping = true;
            receiver = client;
            client = null;
            thread = receiveThread;
        }

        if (receiver != null)
        {
            receiver.Close();
        }

        if (thread != null && thread.IsAlive && Thread.CurrentThread != thread)
        {
            thread.Join(500);
        }
    }
}