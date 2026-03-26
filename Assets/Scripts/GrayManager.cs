using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局灰度控制管理器：单例模式，外部仅需通过 GrayManager.Instance 调用
/// </summary>
public class GrayManager: MonoBehaviour
{
    // 全局单例入口
    public static GrayManager Instance { get; private set; }

    // 内部数据类：存储每个受控对象的信息
    [System.Serializable]
    public class GrayTarget
    {
        [Header("对象唯一ID（用于外部调用识别）")]
        public string targetID; // 比如 "Layer1_Stone"、"Player"、"Prop_Carrot"
        [Header("受控对象的Renderer")]
        public Renderer targetRenderer;
        [Header("初始是否为灰度")]
        public bool isGrayOnStart = true;
        // 内部缓存的材质实例
        internal Material materialInstance;
    }

    [Header("所有需要控制的对象列表（在Inspector里添加）")]
    public List<GrayTarget> allGrayTargets = new List<GrayTarget>();

    // 内部字典：快速通过ID查找对象
    private Dictionary<string, GrayTarget> _targetDict = new Dictionary<string, GrayTarget>();

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            // 可选：切换场景不销毁管理器
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化所有受控对象
        InitAllTargets();
    }

    // 初始化：创建材质实例、设置初始状态
    private void InitAllTargets()
    {
        // 加载灰度Shader（确保你的Shader名为 "Custom/SpriteGraySwitch"）
        Shader grayShader = Shader.Find("Custom/SpriteGraySwitch");
        if (grayShader == null)
        {
            Debug.LogError("【GrayManager错误】找不到Shader，请确保Shader名为 Custom/SpriteGraySwitch");
            return;
        }

        foreach (var target in allGrayTargets)
        {
            if (target.targetRenderer == null || string.IsNullOrEmpty(target.targetID))
            {
                Debug.LogWarning($"【GrayManager警告】跳过无效对象，请检查ID和Renderer是否赋值");
                continue;
            }

            // 1. 创建独立材质实例，避免修改全局材质
            target.materialInstance = new Material(grayShader);
            target.targetRenderer.material = target.materialInstance;

            // 2. 设置初始灰度状态
            target.materialInstance.SetFloat("_IsGray", target.isGrayOnStart ? 1 : 0);

            // 3. 加入字典，方便快速查找
            if (!_targetDict.ContainsKey(target.targetID))
            {
                _targetDict.Add(target.targetID, target);
            }
        }
    }

    // ==================== 【外部唯一需要调用的公共方法】 ====================

    /// <summary>
    /// 【外部调用1】通过ID设置指定对象的灰度状态
    /// </summary>
    /// <param name="targetID">对象的唯一ID（在Inspector里填的）</param>
    /// <param name="isGray">true=变灰，false=恢复原色</param>
    public void SetGrayByID(string targetID, bool isGray)
    {
        if (_targetDict.TryGetValue(targetID, out GrayTarget target))
        {
            target.materialInstance.SetFloat("_IsGray", isGray ? 1 : 0);
        }
        else
        {
            Debug.LogWarning($"【GrayManager警告】找不到ID为 {targetID} 的对象");
        }
    }

    /// <summary>
    /// 【外部调用2】通过ID切换指定对象的状态（灰→原色 / 原色→灰）
    /// </summary>
    /// <param name="targetID">对象的唯一ID</param>
    public void ToggleGrayByID(string targetID)
    {
        if (_targetDict.TryGetValue(targetID, out GrayTarget target))
        {
            bool currentState = target.materialInstance.GetFloat("_IsGray") > 0;
            target.materialInstance.SetFloat("_IsGray", currentState ? 0 : 1);
        }
    }

    /// <summary>
    /// 【外部调用3】批量设置所有对象的灰度状态
    /// </summary>
    /// <param name="isGray">true=全部变灰，false=全部恢复原色</param>
    public void SetAllGray(bool isGray)
    {
        foreach (var target in allGrayTargets)
        {
            if (target.materialInstance != null)
            {
                target.materialInstance.SetFloat("_IsGray", isGray ? 1 : 0);
            }
        }
    }
}