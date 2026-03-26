using UnityEngine;

public class TriggerB : MonoBehaviour
{
    private bool isEnter = false;
    public string name1;
    public string name2;
    public string name3; // 这是新增的第3个 ID

    public bool GetIsEnter() { return isEnter; }

    public void Set()
    {
        // 保持原版的判断逻辑
        if (isEnter)
        {
            if (GrayManager.Instance != null)
            {
                // 处理原有的两个变色
                GrayManager.Instance.SetGrayByID(name1, false);
                GrayManager.Instance.SetGrayByID(name2, false);

                // 处理新增的第3个背景变色
                if (!string.IsNullOrEmpty(name3))
                {
                    GrayManager.Instance.SetGrayByID(name3, false);
                }
            }

            if (LevelProgressManager.Instance != null)
                LevelProgressManager.Instance.RegisterTrigger();

            Destroy(gameObject);
        }
    }

    // 恢复原版的感应逻辑
    private void OnTriggerEnter2D(Collider2D collision) { isEnter = true; }
    private void OnTriggerExit2D(Collider2D collision) { isEnter = false; }
}