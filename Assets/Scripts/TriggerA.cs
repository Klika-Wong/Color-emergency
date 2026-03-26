using UnityEngine;

public class TriggerA : MonoBehaviour
{
    private bool isEnter = false;
    public GameObject obj;

    // 供 item_collector 检查玩家是否在范围内
    public bool GetIsEnter() { return isEnter; }

    public void Set()
    {
        // 保持你最初的逻辑
        if (isEnter)
        {
            if (obj != null) obj.SetActive(true);

            // 仅增加计数器
            if (LevelProgressManager.Instance != null)
                LevelProgressManager.Instance.RegisterTrigger();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) { isEnter = true; }
    private void OnTriggerExit2D(Collider2D collision) { isEnter = false; }
}