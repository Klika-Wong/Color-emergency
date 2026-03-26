using System.Collections;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance;

    [Header("设置总数")]
    public int totalTriggers = 7; // A和B加起来的总数
    private int currentCount = 0;

    [Header("展示设置")]
    public CanvasGroup targetCanvasGroup; // 拖入 Canvas2 里的图片
    public float delayTime = 5f;        // 触发完后的等待秒数

    private void Awake()
    {
        Instance = this;
        if (targetCanvasGroup != null) targetCanvasGroup.alpha = 0;
    }

    // 只要 A 或 B 触发，都会调用这个方法
    public void RegisterTrigger()
    {
        currentCount++;
        Debug.Log("当前总进度：" + currentCount + " / " + totalTriggers);

        if (currentCount >= totalTriggers)
        {
            StartCoroutine(WaitAndShowImage());
        }
    }

    private IEnumerator WaitAndShowImage()
    {
        // 1. 固定等待 10 秒
        yield return new WaitForSeconds(delayTime);

        // 2. 执行 2 秒渐变
        float timer = 0;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            if (targetCanvasGroup != null) targetCanvasGroup.alpha = timer / 2f;
            yield return null;
        }
    }
}