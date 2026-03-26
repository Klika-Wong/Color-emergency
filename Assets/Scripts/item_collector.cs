using UnityEngine;
using UnityEngine.UI;

public class item_collector : MonoBehaviour
{
    [SerializeField] private Text cherriesText;
    [SerializeField] private AudioSource collectSoundEffect;
    public float detectRadius = 4.0f; // 探测半径
    private int cherries = 0;

    private float nextInteractTime = 0f; // 冷却时间控制
    private float cooldown = 0.3f;       // 0.3秒内只能触发一次

    private void Start()
    {
        if (AllControl.GameManager.Instance != null)
        {
            cherries = AllControl.GameManager.Instance.score;
            UpdateUI();
        }
    }

    private void Update()
    {
        // --- 修复1：统一键盘检测 ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCollect();
        }
    }

    public void TryCollect()
    {
        // 防止一帧或连续几帧内处理多个道具
        if (Time.time < nextInteractTime) return;

        Collider2D[] hitItems = Physics2D.OverlapCircleAll(transform.position, detectRadius);

        foreach (var item in hitItems)
        {
            // --- 修复2：优先级判定 + 单次处理 ---

            // 1. 如果是樱桃
            if (item.CompareTag("Cherry"))
            {
                ExecutePickup(item.gameObject);
                nextInteractTime = Time.time + cooldown; // 设置冷却
                return; // 【关键】处理完一个，立刻退出循环，防止“一吃多”
            }

            // 2. 如果是机关A
            TriggerA ta = item.GetComponent<TriggerA>();
            if (ta != null && ta.GetIsEnter())
            { // 配合最初的靠近逻辑
                ta.Set();
                nextInteractTime = Time.time + cooldown;
                return; // 【关键】触发完一个，立刻退出
            }

            // 3. 如果是机关B
            TriggerB tb = item.GetComponent<TriggerB>();
            if (tb != null && tb.GetIsEnter())
            {
                tb.Set();
                nextInteractTime = Time.time + cooldown;
                return; // 【关键】触发完一个，立刻退出
            }
        }
    }

    private void ExecutePickup(GameObject obj)
    {
        if (collectSoundEffect != null) collectSoundEffect.Play();
        Destroy(obj);
        cherries++;
        if (AllControl.GameManager.Instance != null) AllControl.GameManager.Instance.score = cherries;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cherriesText != null) cherriesText.text = "Cherries:" + cherries;
    }
}