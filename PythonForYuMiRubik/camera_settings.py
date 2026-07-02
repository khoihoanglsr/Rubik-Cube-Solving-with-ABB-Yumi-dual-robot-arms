import cv2
import time
# Cần thư viện này để đọc tên camera (bạn đã có trong project chính)
from pygrabber.dshow_graph import FilterGraph 

def open_camera_settings():
    # 1. Quét danh sách thiết bị để tìm "C270"
    print("Đang quét danh sách thiết bị...")
    graph = FilterGraph()
    devices = graph.get_input_devices()
    
    cam_index = -1
    
    for i, device_name in enumerate(devices):
        print(f"  - Tìm thấy tại index {i}: {device_name}")
        # Kiểm tra xem tên có chứa "C270" không (viết hoa hết để so sánh cho chắc)
        if "C270" in device_name.upper():
            print(f">>> ĐÃ TÌM THẤY 'C270' tại Index {i}!")
            cam_index = i
            break
    
    if cam_index == -1:
        print("❌ Lỗi: Không tìm thấy camera nào có tên chứa 'C270'!")
        print("Hãy kiểm tra lại dây cáp kết nối.")
        return

    # 2. Mở Camera đúng index vừa tìm được
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)
    
    if not cap.isOpened():
        print(f"Không thể mở camera index {cam_index} dù đã tìm thấy tên.")
        return

    # Set độ phân giải giống project chính
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 720)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 560)

    # 3. Mở bảng cài đặt
    print(">>> ĐANG MỞ BẢNG CÀI ĐẶT...")
    # Lệnh này sẽ mở cửa sổ cấu hình của Logitech C270
    cap.set(cv2.CAP_PROP_SETTINGS, 1) 

    # 4. Vòng lặp hiển thị
    print(">>> Nhấn SPACE (Dấu cách) để LƯU và THOÁT.")
    while True:
        ret, frame = cap.read()
        if not ret:
            print("Mất kết nối camera.")
            break

        # Vẽ hướng dẫn
        cv2.putText(frame, "Logitech C270 Settings Mode", (10, 30), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 255), 2)
        cv2.putText(frame, "1. Bo tich 'Auto' o White Balance", (10, 60), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        cv2.putText(frame, "2. Keo thanh truot den khi het xanh", (10, 90), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        cv2.putText(frame, "Nhan 'SPACE' de Thoat", (10, 130), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

        cv2.imshow("C270 Preview - Chinh xong nho bam Apply", frame)

        key = cv2.waitKey(1)
        if key == 32: # Space key
            break

    cap.release()
    cv2.destroyAllWindows()
    print("Đã lưu cài đặt cho phiên làm việc này.")

if __name__ == "__main__":
    open_camera_settings()