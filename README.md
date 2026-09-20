# Color-emergency

基于 Unity 的交互式游戏演示项目，结合三视角场景呈现、角色动画与外部手势输入，实现通过手势控制角色移动、跳跃和场景交互。

> 当前仓库为开发演示版本，部分目录仍保留多个阶段性 Unity 工程副本。

## 项目亮点

- **Unity 交互场景**：构建包含平台、建筑、树木和可收集物的 2D/2.5D 游戏场景。
- **三视角展示**：支持从不同观察角度展示场景结构与交互效果。
- **手势控制**：通过外部手势识别程序输出指令，控制 Unity 中角色的移动、跳跃和交互。
- **UDP 通信**：Unity 端监听 UDP 端口，接收 Python 或其他外部程序发送的控制指令。
- **角色动画**：包含待机、移动、跳跃、跌落、死亡等动画资源和状态机控制器。
- **交互机制**：支持角色收集物品，并通过特定指令触发场景交互逻辑。

## 控制流程

```text
摄像头 / 手势识别程序
          |
          | UDP 指令
          v
Unity UDPReceiver
          |
          +--> PlayerMovement：移动与跳跃
          |
          +--> item_collector：物品收集与交互
          |
          v
Unity 场景与角色动画反馈
```

## 手势指令

Unity 端默认监听 `5005` 端口，主要指令包括：

| 指令 | 功能 |
| --- | --- |
| `Left` | 向左移动 |
| `Right` | 向右移动 |
| `Left_Up` | 向左移动并触发跳跃 |
| `Down_Left` | 根据外部识别逻辑执行组合方向控制 |
| `Up` | 触发跳跃 |
| `E` | 触发交互或收集物品 |
| `None` | 停止水平移动 |

## 技术栈

- **引擎**：Unity `2021.3.33f1`
- **开发语言**：C#
- **通信方式**：UDP
- **交互输入**：外部手势识别程序
- **项目类型**：Unity 游戏交互演示

## 运行方式

1. 使用 Unity `2021.3.33f1` 或兼容版本打开仓库根目录。
2. 在 Unity Inspector 中确认 `UDPReceiver`、`PlayerMovement` 和 `item_collector` 的引用关系。
3. 启动外部手势识别程序，并确认其 UDP 目标端口与 Unity 中的 `5005` 一致。
4. 运行场景，观察手势指令对角色移动、跳跃和交互行为的影响。

## 目录说明

```text
Color-emergency/
├── Assets/            # Unity 资源、脚本、动画、音频、场景与图片
├── Packages/          # Unity 包配置
├── ProjectSettings/   # Unity 项目设置
├── .gitignore         # Git 忽略规则
├── .gitattributes     # Git 属性配置
└── README.md          # 项目说明
```

### Assets 重点内容

- `Animation/`：角色、物品和场景动画控制器及动画片段
- `Scenes/`：Unity 场景文件
- `Scripts/`：角色控制、物品交互和网络通信脚本
- `BGM/`、`CasualGameSounds/`：背景音乐与音效资源
- `UDPReceiver.cs`：接收外部 UDP 指令并分发给角色控制和交互逻辑

## 开发状态

- 已完成 Unity 基础场景和角色动画搭建。
- 已完成 UDP 指令接收与角色控制联动。
- 已完成手势识别结果到 Unity 行为的初步映射。
- 后续可继续完善输入稳定性、指令去抖、场景切换、跨平台打包和演示素材归档。

## 演示素材

项目演示素材包括：

- 三视角场景展示视频
- 游戏场景截图
- 手势识别与 Unity 指令映射截图

如需将本地视频和图片上传到仓库，建议统一放入 `Documentation/media/`，并使用规范化文件名，避免将个人设备自动生成的文件名直接作为项目资源名称。

## 简历关键词

`Unity` · `C#` · `UDP` · `Computer Vision Interaction` · `Hand Gesture Recognition` · `Game Prototyping` · `Real-time Control` · `Multimodal Interaction`
