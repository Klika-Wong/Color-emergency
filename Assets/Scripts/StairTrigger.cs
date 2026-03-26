using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    [Header("目标层级名称")]
    public string targetSortingLayer = "Default";

    [Header("目标排序值 (可选)")]
    public int targetOrderInLayer = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查碰撞的是不是玩家
        if (other.CompareTag("Player"))
        {
            // 获取玩家身上的渲染器
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                // 核心动作：切换图层
                sr.sortingLayerName = targetSortingLayer;
                sr.sortingOrder = targetOrderInLayer;

                Debug.Log($"玩家已切换到层级: {targetSortingLayer}");
            }
        }
    }
}