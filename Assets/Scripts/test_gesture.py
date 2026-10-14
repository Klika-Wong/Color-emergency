"""Offline smoke checks for the local UDP transport and image dependencies.

These checks do not validate MediaPipe hand recognition or the Unity scene.
"""
import socket

import cv2
import numpy as np


def test_udp_round_trip():
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as receiver:
        receiver.bind(("127.0.0.1", 0))
        receiver.settimeout(1.0)
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as sender:
            sender.sendto(b"Right", receiver.getsockname())
        payload, _ = receiver.recvfrom(64)
        assert payload == b"Right"


def test_opencv_frame_setup():
    frame = np.zeros((480, 640, 3), dtype=np.uint8)
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    assert hsv.shape == frame.shape
    assert hsv.dtype == np.uint8


if __name__ == "__main__":
    test_udp_round_trip()
    test_opencv_frame_setup()
    print("Local UDP and OpenCV smoke checks passed.")
