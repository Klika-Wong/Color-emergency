using System;
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

    // --- 内部变量 ---
    private UdpClient client;
    private Thread receiveThread;
    private string lastData = "";
    private readonly object dataLock = new object(); // 线程锁，确保数据安全

    // ==================== 初始化 ====================
    private void Start()
    {
        // 【自动修复】如果你忘了在Inspector里拖拽，脚本会尝试在自己身上找
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (itemCollector == null) itemCollector = GetComponent<item_collector>();

        // 启动后台接收线程
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log($"<color=green>🚀 UDP 接收器已启动，正在监听端口: {port}</color>");
    }

    // ==================== 后台线程：持续接收数据 ====================
    private void ReceiveData()
    {
        try
        {
            client = new UdpClient(port);
            while (true)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                string incomingData = Encoding.UTF8.GetString(dataByte);

                // 加锁写入数据，防止主线程读取时发生冲突
                lock (dataLock)
                {
                    lastData = incomingData;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("UDP线程关闭或异常: " + e.Message);
        }
    }

    // ==================== 主线程：解析指令并执行 ====================
    private void Update()
    {
        string currentData = "";

        // 加锁读取最新数据
        lock (dataLock)
        {
            currentData = lastData;
            // 如果你希望“E”只触发一次，可以在这里处理完后重置 lastData
        }

        if (string.IsNullOrEmpty(currentData)) return;

        // --- 核心逻辑 A：处理交互指令 (E) ---
        if (currentData.Contains("E"))
        {
            // 打印黄色调试日志，方便在控制台确认信号
            Debug.Log($"<color=yellow>📡 [网络层] 收到 E 指令! 原始数据: {currentData}</color>");

            if (itemCollector != null)
            {
                itemCollector.TryCollect(); // 调用全能交互器（吃樱桃+触发TriggerB）
            }
            else
            {
                Debug.LogError("❌ [错误] UDPReceiver 找不到 item_collector 脚本，请检查挂载！");
            }

            // 为了防止一瞬间触发太多次，处理完 E 后清除标志位
            lock (dataLock) { lastData = lastData.Replace("E", ""); }
        }

        // --- 核心逻辑 B：处理移动指令 (Left, Right, Up, None) ---
        if (playerMovement != null)
        {
            // 1. 处理跳跃
            if (currentData.Contains("Up"))
            {
                playerMovement.TriggerJump();
            }

            // 2. 处理左右移动
            if (currentData == "None")
            {
                playerMovement.SetHorizontalDirection(0f);
            }
            else if (currentData.Contains("Left"))
            {
                playerMovement.SetHorizontalDirection(-1f);
            }
            else if (currentData.Contains("Right"))
            {
                playerMovement.SetHorizontalDirection(1f);
            }
        }
    }

    // ==================== 退出清理 ====================
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
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
    }
}