import cv2
import socket
import time
import math
import numpy as np
from cvzone.HandTrackingModule import HandDetector

# --- 1. 自动寻找摄像头 (解决外接摄像头识别问题) ---
def get_camera():
    # 通常外接摄像头是 1 或 2，内置是 0
    for index in [1, 0, 2]:
        cap = cv2.VideoCapture(index)
        if cap.isOpened():
            success, _ = cap.read()
            if success:
                print(f"✅ 成功连接到摄像头，索引号: {index}")
                return cap
            cap.release()
    return None

# --- 2. 硬件参数初始化 ---
W, H = 640, 480


def configure_camera(camera):
    if camera is None:
        return None
    camera.set(cv2.CAP_PROP_FRAME_WIDTH, W)
    camera.set(cv2.CAP_PROP_FRAME_HEIGHT, H)
    # These exposure values vary by camera driver; remove them if the image is dark.
    camera.set(cv2.CAP_PROP_AUTO_EXPOSURE, 0.25)
    camera.set(cv2.CAP_PROP_EXPOSURE, -5)
    return camera


cap = configure_camera(get_camera()) 

# --- 3. 通信与算法配置 ---
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# modelComplexity=1 提升暗光下的深度识别能力
detector = HandDetector(staticMode=False, maxHands=1, 
                        modelComplexity=1, 
                        detectionCon=0.4, minTrackCon=0.4)

CENTER_POINT = (W // 2, H // 2)
DEAD_ZONE = 65
LOST_THRESHOLD = 0.2

last_sent_time = 0
send_interval = 0.02 
last_message = "None"
hand_lost_time = 0
buffered_commands = set()

def enhance_low_light(frame):
    """
    高级暗光增强：CLAHE 算法
    """
    lab = cv2.cvtColor(frame, cv2.COLOR_BGR2LAB)
    l, a, b = cv2.split(lab)
    # 限制对比度自适应直方图均衡化
    clahe = cv2.createCLAHE(clipLimit=3.0, tileGridSize=(8, 8))
    cl = clahe.apply(l)
    limg = cv2.merge((cl, a, b))
    enhanced = cv2.cvtColor(limg, cv2.COLOR_LAB2BGR)
    # 降噪处理
    return cv2.medianBlur(enhanced, 3)

print("🎮 强化版控制器已就绪 (支持外接摄像头 & 暗光优化)")

while True:
    if cap is None:
        print("⚠️ 未找到摄像头，1 秒后重试...")
        time.sleep(1)
        cap = configure_camera(get_camera())
        continue

    success, img = cap.read()
    if not success:
        print("⚠️ 无法获取摄像头画面，正在尝试重连...")
        cap.release()
        time.sleep(1)
        cap = configure_camera(get_camera())
        continue
    
    img = cv2.flip(img, 1)

    # --- 核心：软件补光 ---
    processed_img = enhance_low_light(img)

    # 识别手势
    hands, _ = detector.findHands(processed_img, draw=False)
    
    current_commands = set()
    now = time.time()

    if hands:
        hand_lost_time = now
        hand = hands[0]
        cx, cy = hand["center"]
        
        # 1. 移动逻辑
        dx, dy = cx - CENTER_POINT[0], cy - CENTER_POINT[1]
        dist = math.sqrt(dx**2 + dy**2)

        if dist > DEAD_ZONE:
            angle = math.degrees(math.atan2(-dy, dx))
            if angle < 0: angle += 360
            
            # 8方向映射
            if 22.5 <= angle < 67.5:    current_commands.update(["Right", "Up"])
            elif 67.5 <= angle < 112.5: current_commands.add("Up")
            elif 112.5 <= angle < 157.5:current_commands.update(["Left", "Up"])
            elif 157.5 <= angle < 202.5:current_commands.add("Left")
            elif 202.5 <= angle < 247.5:current_commands.update(["Left", "Down"])
            elif 247.5 <= angle < 292.5:current_commands.add("Down")
            elif 292.5 <= angle < 337.5:current_commands.update(["Right", "Down"])
            else:                       current_commands.add("Right")

        # 2. 交互逻辑 (握拳 E)
        try:
            fingers = detector.fingersUp(hand)
            if fingers[1] == 0 and fingers[2] == 0: 
                current_commands.add("E")
        except Exception:
            pass # 忽略关键点计算异常
            
        buffered_commands = current_commands 
    else:
        # 抗掉帧补偿
        if now - hand_lost_time < LOST_THRESHOLD:
            current_commands = buffered_commands
        else:
            current_commands = set()

    # --- 3. 指令发送 ---
    message = "_".join(sorted(current_commands)) if current_commands else "None"
    if message != last_message or (now - last_sent_time) > send_interval:
        try:
            sock.sendto(message.encode("utf-8"), (UDP_IP, UDP_PORT))
            last_message = message
            last_sent_time = now
        except Exception as e:
            print(f"网络发送错误: {e}")

    # --- 4. 视觉 UI ---
    display_img = processed_img.copy()
    cv2.circle(display_img, CENTER_POINT, DEAD_ZONE, (0, 0, 255), 2)
    if hands:
        cv2.line(display_img, CENTER_POINT, (cx, cy), (0, 255, 0), 2)
        cv2.circle(display_img, (cx, cy), 8, (0, 255, 0), cv2.FILLED)

    # 状态文字
    status_color = (0, 255, 0) if message != "None" else (0, 0, 255)
    cv2.putText(display_img, f"Unity: {message}", (20, 40), 
                cv2.FONT_HERSHEY_SIMPLEX, 0.8, status_color, 2)
    
    if not hands and (now - hand_lost_time < LOST_THRESHOLD):
        cv2.putText(display_img, "BUFFERING...", (20, 75), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)

    cv2.imshow("MOBA Controller (Enhanced)", display_img)
    if cv2.waitKey(1) & 0xFF == 27: break

if cap is not None:
    cap.release()
sock.close()
cv2.destroyAllWindows()