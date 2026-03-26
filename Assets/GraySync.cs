using UnityEngine;
using UnityEngine.Tilemaps; // 必须加这一行，否则会报错找不到 Tilemap

public class GraySync : MonoBehaviour
{
    [Header("设置")]
    public string grayID = "Tilemap1"; // 确保这里和樱桃 TriggerB 里的 Name1 一致

    private TilemapRenderer tr;
    private Tilemap tm;

    void Awake()
    {
        tr = GetComponent<TilemapRenderer>();
        tm = GetComponent<Tilemap>();
    }

    // 这个方法供 GrayManager 调用
    public void SetGray(bool isGray)
    {
        // 方案 A：如果你的变色是通过修改 Tilemap 整体颜色实现的
        if (tm != null)
        {
            tm.color = isGray ? Color.gray : Color.white;
        }

        // 方案 B：如果你的变色是通过 Shader 材质球控制的
        if (tr != null && tr.material != null)
        {
            // 假设你的 Shader 里控制灰度的变量名叫 _UIGray
            // tr.material.SetFloat("_UIGray", isGray ? 1 : 0);
        }
    }
}