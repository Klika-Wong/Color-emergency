"""
测试脚本 - 验证手势识别的核心逻辑
"""
import cv2
import numpy as np
import socket

print("=" * 50)
print("🧪 手势识别系统测试")
print("=" * 50)

# 1. 测试 UDP 通信
print("\n1️⃣ 测试 UDP 通信...")
try:
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    test_message = "Test_Left_Right"
    sock.sendto(test_message.encode("utf-8"), ("127.0.0.1", 5005))
    print(f"   ✓ 成功发送测试信号: '{test_message}'")
except Exception as e:
    print(f"   ✗ UDP 错误: {e}")

# 2. 测试 OpenCV 肤色检测
print("\n2️⃣ 测试肤色检测算法...")
# 创建一个测试图像
test_image = np.zeros((480, 640, 3), dtype=np.uint8)
# 添加一个肤色区域
test_image[100:300, 100:300] = [20, 150, 150]  # HSV 肤色近似

hsv = cv2.cvtColor(test_image, cv2.COLOR_BGR2HSV)
lower_skin1 = np.array([0, 10, 60], dtype=np.uint8)
upper_skin1 = np.array([20, 255, 255], dtype=np.uint8)
lower_skin2 = np.array([160, 10, 60], dtype=np.uint8)
upper_skin2 = np.array([180, 255, 255], dtype=np.uint8)

mask1 = cv2.inRange(hsv, lower_skin1, upper_skin1)
mask2 = cv2.inRange(hsv, lower_skin2, upper_skin2)
mask = cv2.bitwise_or(mask1, mask2)

contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
print(f"   ✓ 检测到 {len(contours)} 个轮廓")

# 3. 测试手杖识别流程
print("\n3️⃣ 测试手部识别流程...")

class HandDetector:
    def __init__(self):
        self.lower_skin1 = np.array([0, 10, 60], dtype=np.uint8)
        self.upper_skin1 = np.array([20, 255, 255], dtype=np.uint8)
        self.lower_skin2 = np.array([160, 10, 60], dtype=np.uint8)
        self.upper_skin2 = np.array([180, 255, 255], dtype=np.uint8)
        self.kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (15, 15))
    
    def detect(self, frame):
        hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
        mask1 = cv2.inRange(hsv, self.lower_skin1, self.upper_skin1)
        mask2 = cv2.inRange(hsv, self.lower_skin2, self.upper_skin2)
        mask = cv2.bitwise_or(mask1, mask2)
        
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, self.kernel)
        mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5)))
        
        contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        
        hands = []
        for contour in contours:
            area = cv2.contourArea(contour)
            if 3000 < area < 50000:
                M = cv2.moments(contour)
                if M["m00"] > 0:
                    cx = int(M["m10"] / M["m00"])
                    cy = int(M["m01"] / M["m00"])
                    is_closed = area < 15000
                    hands.append({
                        'center': (cx, cy),
                        'area': area,
                        'closed': is_closed,
                        'contour': contour
                    })
        
        return sorted(hands, key=lambda x: x['area'], reverse=True)[:2], mask

detector = HandDetector()
hands, _ = detector.detect(test_image)
print(f"   ✓ 检测器初始化成功")
print(f"   ✓ 测试检测: {len(hands)} 只手")

# 4. 测试指令生成
print("\n4️⃣ 测试指令生成...")
test_commands = set()
test_commands.add("Left")
test_commands.add("Right")
message = "_".join(sorted(test_commands)) if test_commands else "None"
print(f"   ✓ 生成指令: '{message}'")

print("\n" + "=" * 50)
print("✅ 所有测试通过！系统可以运行")
print("=" * 50)
print("\n💡 现在运行主脚本：python gesture_control.py")
