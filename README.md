# Color Emergency

基于 Unity 的 2D/2.5D 交互式游戏原型。项目将角色移动、跳跃、动画、收集物、场景机关与外部视觉输入组合在同一关卡中，用于验证计算机视觉输入控制 Unity 游戏角色的可行性。

## 当前可验证状态

| 项目 | 当前状态 |
| --- | --- |
| Unity 版本 | `2021.3.33f1` |
| 项目入口 | 仓库根目录 |
| 主场景 | `Assets/Scenes/Level1.unity` |
| Build Settings | 仅启用 `Level1.unity` |
| 游戏脚本 | C#，位于 `Assets/Scripts/` 及 `Assets/` |
| 外部输入辅助程序 | Python，位于 `Assets/Scripts/gesture_control.py` |
| 开发性质 | 可运行的交互原型，不代表完整商业游戏 |

> 请直接使用 Unity `2021.3.33f1` 打开仓库根目录。不要将资源包中的 `Demo.unity` 作为游戏入口。

## 项目结构

```text
Color-emergency/
├── Assets/
│   ├── Scenes/
│   │   └── Level1.unity              # 当前主场景与唯一 Build Scene
│   ├── Scripts/
│   │   ├── PlayerMovement.cs         # 角色移动、跳跃和动画状态
│   │   ├── PlayerLife.cs             # 死亡检测与关卡重载
│   │   ├── item_collector.cs         # 收集物与附近机关交互
│   │   ├── TriggerA.cs               # A 类机关逻辑
│   │   ├── TriggerB.cs               # B 类机关与状态切换
│   │   ├── GrayManager.cs            # 场景灰度状态管理
│   │   ├── WaypointFollower.cs       # 移动平台路径点逻辑
│   │   ├── StickyPlatform.cs         # 角色与移动平台附着
│   │   ├── gesture_control.py        # 外部视觉输入辅助程序
│   │   └── test_gesture.py           # 输入测试脚本
│   ├── UDPReceiver.cs                # Unity 外部输入接收组件
│   ├── LevelProgressManager.cs       # 关卡触发进度管理
│   ├── GraySync.cs                   # 灰度状态同步（保留脚本）
│   ├── Animation/                    # 角色、物品和场景动画
│   ├── Profab/                       # 当前项目 Prefab（历史命名）
│   ├── Pixel Adventure 1/            # 导入的像素美术资源
│   ├── BGM/                          # 背景音乐
│   └── CasualGameSounds/             # 游戏音效与授权文件
├── Packages/                         # Unity 包配置
├── ProjectSettings/                  # Unity 项目设置
├── .gitignore
└── README.md
```

### 场景边界

- `Assets/Scenes/Level1.unity`：当前正式游戏场景，也是唯一启用的 Build Scene。
- `Assets/Pixel Adventure 1/Scenes/Demo.unity`：导入资源附带的展示场景，不属于本项目正式关卡。
- 当前仓库没有已启用的开始菜单、结束菜单或多关卡场景，因此不把相关保留脚本描述为已完成流程。

## 关键代码与启用状态

### 已由 `Level1.unity` 直接引用

| 脚本 | 主要职责 |
| --- | --- |
| `Assets/UDPReceiver.cs` | 在 Unity 端接收外部输入，并调用已绑定的角色与交互组件 |
| `Assets/Scripts/PlayerMovement.cs` | 水平移动、跳跃、落地检测和动画状态更新 |
| `Assets/Scripts/item_collector.cs` | 检测附近收集物或机关并执行单次交互 |
| `Assets/Scripts/PlayerLife.cs` | 处理危险碰撞、死亡动画和当前场景重载 |
| `Assets/Scripts/Cemara.cs` | 摄像机跟随逻辑；文件名为早期历史命名 |
| `Assets/Scripts/TriggerA.cs` | 控制 A 类机关对象 |
| `Assets/Scripts/TriggerB.cs` | 控制 B 类机关及目标灰度状态 |
| `Assets/Scripts/GrayManager.cs` | 统一管理场景对象灰度切换 |
| `Assets/LevelProgressManager.cs` | 记录机关触发数量并显示完成反馈 |
| `Assets/Scripts/WaypointFollower.cs` | 控制平台沿路径点移动 |
| `Assets/Scripts/StickyPlatform.cs` | 保持角色随移动平台运动 |
| `Assets/Scripts/Rotate.cs` | 控制旋转障碍物 |

### 间接依赖或当前保留

| 脚本 | 状态说明 |
| --- | --- |
| `Assets/Scripts/AllControl.cs` | 未挂载到场景，但被 `item_collector.cs` 用于保存收集计数，不能直接删除 |
| `Assets/GraySync.cs` | 当前未由 `Level1.unity` 直接引用，作为灰度同步方案保留 |
| `Assets/Scripts/StairTrigger.cs` | 当前主场景未直接引用，作为楼梯层级切换逻辑保留 |
| `Assets/Scripts/StartMenu.cs` | 当前没有对应的已启用菜单场景 |
| `Assets/Scripts/EndMenu.cs` | 当前没有对应的已启用结束场景 |
| `Assets/Scripts/Finish.cs` | 包含按 Build Index 切换场景的逻辑，但当前 Build Settings 只有一个场景 |

上述“保留”表示代码存在于公开仓库，不表示对应流程已经在当前 Build 中完成验证。

## 打开项目

1. 安装 Unity Hub 和 Unity `2021.3.33f1`。
2. 在 Unity Hub 中选择 **Add project from disk**，打开本仓库根目录。
3. 打开 `Assets/Scenes/Level1.unity`。
4. 检查 Console 是否存在资源导入或脚本编译错误。
5. 在需要验证外部输入时，再根据本机环境配置 Python 依赖和输入设备。

## 技术栈

- Unity `2021.3.33f1`
- C# / Unity 2D Physics
- Python / OpenCV / MediaPipe（外部输入侧）
- UDP 本地通信
- Animator、Tilemap、Collider2D、Rigidbody2D

## 仓库整理记录

早期 `main` 曾包含 `My project/`、`My project (1)/`、`My project (2)/` 和 `My project (3)/` 四套 Unity 初始化或测试工程。其中包含重复的 Unity 6 模板场景、不完整工程和空白 Unity 2021 工程，均不是当前游戏入口。

为避免误导项目入口，这些目录已从 `main` 移除。清理前的完整状态保存在分支：

[`archive/legacy-unity-projects`](https://github.com/Klika-Wong/Color-emergency/tree/archive/legacy-unity-projects)

该归档仅用于追溯历史，不建议作为运行版本使用。

## 已知限制

- 当前仅有一个正式关卡进入 Build Settings。
- 尚未提供正式发布包和跨设备测试结果。
- 外部视觉输入效果会受到摄像头、光照、识别稳定性和本机网络配置影响。
- 部分资源和脚本保留早期命名，后续重命名时需要连同 Unity `.meta` 文件一起迁移，以保持 GUID 引用稳定。
- 本仓库不声明未经公开验证的准确率、延迟或帧率指标。

## 简历关键词

`Unity` · `C#` · `Python` · `OpenCV` · `MediaPipe` · `UDP` · `Computer Vision Interaction` · `Game Prototyping` · `Real-time Control`
