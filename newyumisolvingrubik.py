import time
import cv2
import os
import sys
import numpy as np
import kociemba
from pygrabber.dshow_graph import FilterGraph
import copy
import socket
import threading
import queue
import csv
import json
from datetime import datetime
from collections import Counter
from pathlib import Path
cap_queue = queue.Queue()
detected_case = None
case_ready_event = threading.Event()


# ==============================================================================
# APP_DIR - C# OUTPUT FOLDER
# Python luôn ghi solution.txt, rubik_state.txt, current_case_id.txt và logs vào đây.
# main.py sẽ set RUBIK_APP_DIR trước khi import file này.
# ==============================================================================
APP_DIR = Path(os.environ.get("RUBIK_APP_DIR", os.getcwd())).resolve()

def app_path(*parts):
    return APP_DIR.joinpath(*parts)

print(f"[PATH] APP_DIR={APP_DIR}", flush=True)
print(f"[PATH] module cwd={os.getcwd()}", flush=True)


# ==============================================================================
# EXPERIMENT LOGGING - 100 CASE RECORD
# ==============================================================================
CASE_ID = None
RECOGNITION_REACQ_TOTAL = 0
RECOGNITION_START_TIME = None
RECOGNITION_END_TIME = None
LOG_DIR = app_path("logs")
FACE_RAW_DIR = app_path("logs", "faces_raw")
FACE_ANN_DIR = app_path("logs", "faces_annotated")

def _now_iso():
    return datetime.now().isoformat(timespec="milliseconds")

def _safe_mkdirs():
    LOG_DIR.mkdir(parents=True, exist_ok=True)
    FACE_RAW_DIR.mkdir(parents=True, exist_ok=True)
    FACE_ANN_DIR.mkdir(parents=True, exist_ok=True)

def _make_case_id(index=None):
    """Tạo case_id duy nhất. Nếu không truyền index thì tự tăng logs/last_case_index.txt."""
    _safe_mkdirs()
    if index is None:
        idx_file = LOG_DIR / "last_case_index.txt"
        try:
            if idx_file.exists():
                last = int(idx_file.read_text(encoding="utf-8").strip() or "0")
            else:
                last = 0
            index = last + 1
            idx_file.write_text(str(index), encoding="utf-8")
        except Exception as e:
            print(f"[LOG][WARN] Không đọc/ghi được last_case_index.txt: {e}", flush=True)
            index = 1
    return f"CASE_{datetime.now().strftime('%Y%m%d_%H%M%S')}_{int(index):03d}"

def init_experiment_case(case_id=None):
    """Gọi ở đầu mỗi lần chạy Python. Python ghi current_case_id.txt để C# đọc lại."""
    global CASE_ID, RECOGNITION_REACQ_TOTAL, RECOGNITION_START_TIME, RECOGNITION_END_TIME
    _safe_mkdirs()
    CASE_ID = case_id or os.environ.get("RUBIK_CASE_ID") or _make_case_id()
    RECOGNITION_REACQ_TOTAL = 0
    RECOGNITION_START_TIME = _now_iso()
    RECOGNITION_END_TIME = ""
    current_case_path = app_path("current_case_id.txt")
    with open(current_case_path, "w", encoding="utf-8") as f:
        f.write(CASE_ID)
        f.flush()
        os.fsync(f.fileno())
    print(f"[LOG] case_id={CASE_ID}", flush=True)
    print(f"[LOG] current_case_id.txt={current_case_path}", flush=True)
    return CASE_ID

def _ensure_case_id():
    global CASE_ID
    if not CASE_ID:
        return init_experiment_case()
    return CASE_ID

def _json_compact(obj):
    return json.dumps(obj, ensure_ascii=False, separators=(",", ":"))

def _append_csv_row(csv_path, headers, row):
    """Append CSV an toàn, Excel đọc được tiếng Việt. File mới dùng UTF-8 BOM."""
    _safe_mkdirs()
    path = Path(csv_path)
    new_file = not path.exists() or path.stat().st_size == 0
    mode = "w" if new_file else "a"
    enc = "utf-8-sig" if new_file else "utf-8"
    try:
        with open(path, mode, newline="", encoding=enc) as f:
            writer = csv.DictWriter(f, fieldnames=headers, quoting=csv.QUOTE_MINIMAL)
            if new_file:
                writer.writeheader()
            writer.writerow({h: row.get(h, "") for h in headers})
            f.flush()
            os.fsync(f.fileno())
    except Exception as e:
        print(f"[LOG][ERROR] Cannot write CSV {path}: {e}", flush=True)

def _unknown_count(matrix):
    try:
        return sum(1 for row in matrix for c in row if c == "unknown" or c is None or str(c).strip() == "")
    except Exception:
        return 0

def _matrix_center(matrix):
    try:
        return matrix[1][1]
    except Exception:
        return ""

def _save_face_images(face_index, raw_frame=None, annotated_frame=None):
    cid = _ensure_case_id()
    raw_path = ""
    ann_path = ""
    try:
        if raw_frame is not None:
            raw_path = str(FACE_RAW_DIR / f"{cid}_face{face_index}_raw.png")
            cv2.imwrite(raw_path, raw_frame)
        if annotated_frame is not None:
            ann_path = str(FACE_ANN_DIR / f"{cid}_face{face_index}_annotated.png")
            cv2.imwrite(ann_path, annotated_frame)
    except Exception as e:
        print(f"[LOG][WARN] Cannot save face images face={face_index}: {e}", flush=True)
    return raw_path, ann_path

def log_recognition_face(face_index, detected_matrix, raw_frame=None, annotated_frame=None,
                         recognition_success=True, reacquisition_count=0,
                         ground_truth_matrix="", wrong_cell_count="", wrong_cell_positions="", note=""):
    cid = _ensure_case_id()
    raw_path, ann_path = _save_face_images(face_index, raw_frame, annotated_frame)
    unknowns = _unknown_count(detected_matrix)
    if not ground_truth_matrix:
        wrong_cell_count = ""
        wrong_cell_positions = ""
    headers = [
        "case_id", "face_index", "timestamp", "detected_matrix", "center_color",
        "unknown_count", "raw_image_path", "annotated_image_path", "recognition_success",
        "reacquisition_count", "ground_truth_matrix", "wrong_cell_count", "wrong_cell_positions", "note"
    ]
    row = {
        "case_id": cid,
        "face_index": face_index,
        "timestamp": _now_iso(),
        "detected_matrix": _json_compact(detected_matrix),
        "center_color": _matrix_center(detected_matrix),
        "unknown_count": unknowns,
        "raw_image_path": raw_path,
        "annotated_image_path": ann_path,
        "recognition_success": bool(recognition_success),
        "reacquisition_count": reacquisition_count,
        "ground_truth_matrix": _json_compact(ground_truth_matrix) if ground_truth_matrix else "",
        "wrong_cell_count": wrong_cell_count,
        "wrong_cell_positions": wrong_cell_positions,
        "note": note,
    }
    _append_csv_row(LOG_DIR / "recognition_faces.csv", headers, row)

def log_solution_row(rubik_state_string="", state_valid=False, solution_text="", solution_move_count=0,
                     solver_success=False, solver_error_message="", centers=None, capture_face_count=0,
                     total_unknown_count=0, recognition_reacquisition_total=0):
    cid = _ensure_case_id()
    headers = [
        "case_id", "timestamp", "rubik_state_string", "state_valid", "solution_text",
        "solution_move_count", "solver_success", "solver_error_message", "centers",
        "capture_face_count", "total_unknown_count", "recognition_reacquisition_total"
    ]
    row = {
        "case_id": cid,
        "timestamp": _now_iso(),
        "rubik_state_string": rubik_state_string,
        "state_valid": bool(state_valid),
        "solution_text": solution_text,
        "solution_move_count": int(solution_move_count) if str(solution_move_count).isdigit() else solution_move_count,
        "solver_success": bool(solver_success),
        "solver_error_message": solver_error_message,
        "centers": _json_compact(centers or []),
        "capture_face_count": capture_face_count,
        "total_unknown_count": total_unknown_count,
        "recognition_reacquisition_total": recognition_reacquisition_total,
    }
    _append_csv_row(LOG_DIR / "solutions.csv", headers, row)


def validate_kociemba_string(facelets):
    """Kiểm tra cube string trước solver: dài 54, chỉ U/R/F/D/L/B, mỗi mặt đúng 9 ô."""
    allowed = set("URFDLB")
    if not isinstance(facelets, str):
        return False, "INVALID_CUBE_STATE: facelets is not string"
    if len(facelets) != 54:
        return False, f"INVALID_CUBE_STATE: length={len(facelets)} != 54"
    bad = sorted(set(facelets) - allowed)
    if bad:
        return False, f"INVALID_CUBE_STATE: bad chars={bad}"
    counts = Counter(facelets)
    bad_counts = {k: counts.get(k, 0) for k in "URFDLB" if counts.get(k, 0) != 9}
    if bad_counts:
        return False, f"INVALID_CUBE_STATE: counts={dict(counts)}, bad_counts={bad_counts}"
    return True, ""

def parse_solution_moves_from_text(solution_text):
    """Parse solution text, bỏ E nếu có. E không phải move robot."""
    if not solution_text:
        return []
    return [m.strip() for m in str(solution_text).replace("\n", " ").split() if m.strip() and m.strip() != "E"]


import heapq
from typing import List, Dict, Tuple
from functools import lru_cache


# ==============================================================================
# YOLO RUBIK COLOR DETECTION - ADAPTER THAY CHO KHỐI HSV/OpenCV
# ==============================================================================
YOLO_CONF = float(os.environ.get("RUBIK_YOLO_CONF", "0.3"))
YOLO_IMGSZ = int(os.environ.get("RUBIK_YOLO_IMGSZ", "640"))
YOLO_DEVICE_ENV = os.environ.get("RUBIK_YOLO_DEVICE", "auto")

# YOLO_COLOR_ALIASES = {
#     "white": "white", "w": "white", "trang": "white", "trắng": "white",
#     "mau_trang": "white", "màu_trắng": "white",
#     "yellow": "yellow", "y": "yellow", "vang": "yellow", "vàng": "yellow",
#     "mau_vang": "yellow", "màu_vàng": "yellow",
#     "red": "red", "r": "red", "do": "red", "đỏ": "red",
#     "mau_do": "red", "màu_đỏ": "red",
#     "orange": "orange", "o": "orange", "cam": "orange",
#     "mau_cam": "orange", "màu_cam": "orange",
#     "green": "green", "g": "green", "xanh": "green", "xanh_la": "green",
#     "xanh_lá": "green", "xanhla": "green", "mau_xanh": "green",
#     "mau_xanh_la": "green", "màu_xanh_lá": "green",
#     "blue": "blue", "b": "blue", "xanh_duong": "blue", "xanh_dương": "blue",
#     "xanhduong": "blue", "xanh_bien": "blue", "xanh_biển": "blue",
#     "mau_xanh_duong": "blue", "màu_xanh_dương": "blue",
#     # Trong model hiện tại, nếu ô xanh dương bị đặt label là tím/tim thì map về blue.
#     "tim": "blue", "tím": "blue", "purple": "blue", "mau_tim": "blue", "màu_tím": "blue",
# }
YOLO_COLOR_ALIASES = {
    "trang": "white",
    "vang": "yellow",
    "do": "red",
    "cam": "orange",
    "xanh": "green",
    "tim": "blue",
    "object": "unknown"
}
def normalize_yolo_class_name(name):
    s = str(name).strip().lower()
    return YOLO_COLOR_ALIASES.get(s, "unknown")


def resolve_yolo_device(device_arg="auto"):
    if device_arg and device_arg != "auto":
        return device_arg
    try:
        import torch
        return "0" if torch.cuda.is_available() else "cpu"
    except Exception:
        return "cpu"


def find_yolo_weights():
    """
    Tìm best.pt. Ưu tiên APP_DIR vì APP_DIR chính là folder C# output đang chạy.
    """
    candidates = [
        APP_DIR / "weights" / "best.pt",
        APP_DIR / "best.pt",
        Path.cwd() / "weights" / "best.pt",
        Path.cwd() / "best.pt",
    ]

    if getattr(sys, "frozen", False):
        exe_dir = Path(sys.executable).resolve().parent
        candidates.extend([
            exe_dir / "weights" / "best.pt",
            exe_dir / "best.pt",
        ])
    else:
        src_dir = Path(__file__).resolve().parent
        candidates.extend([
            src_dir / "weights" / "best.pt",
            src_dir / "best.pt",
        ])

    checked = []
    for p in candidates:
        p = Path(p).resolve()
        if p in checked:
            continue
        checked.append(p)
        if p.is_file():
            print(f"[YOLO] Tìm thấy weights: {p}", flush=True)
            return str(p)

    print("[YOLO] Không tìm thấy best.pt. Đã thử:", flush=True)
    for p in checked:
        print("   ", p, flush=True)
    raise FileNotFoundError("Không tìm thấy best.pt. Hãy đặt model tại APP_DIR\\weights\\best.pt hoặc cạnh file .py/.exe.")

def load_yolo_model_once():
    try:
        from ultralytics import YOLO
    except ImportError:
        print("[YOLO] Lỗi: chưa cài ultralytics. Chạy: pip install ultralytics")
        raise

    weights_path = find_yolo_weights()
    device = resolve_yolo_device(YOLO_DEVICE_ENV)

    print("===== YOLO Rubik Detection =====")
    print(f"Weights : {weights_path}")
    print(f"Img size: {YOLO_IMGSZ}")
    print(f"Conf    : {YOLO_CONF}")
    print(f"Device  : {device}")
    print("================================")

    model = YOLO(weights_path)
    print(f"[YOLO] Model classes: {model.names}")
    return model, device


def yolo_detections_to_3x3(detections):
    """
    Chuyển list detection YOLO thành đúng format OpenCV cũ:
        [[color, color, color],
         [color, color, color],
         [color, color, color]]

    Mỗi detection cần có:
        color, conf, cx, cy, box=(x1,y1,x2,y2)
    """
    face = [["unknown"] * 3 for _ in range(3)]
    score_map = [[-1.0] * 3 for _ in range(3)]

    if not detections:
        print("[YOLO] Cảnh báo: không detect được ô nào.")
        return face

    # Lấy tối đa 9 detection confidence cao nhất để tránh box rác.
    detections = sorted(detections, key=lambda d: d["conf"], reverse=True)[:9]

    if len(detections) < 9:
        print(f"[YOLO] Cảnh báo: chỉ detect được {len(detections)}/9 ô. Ô thiếu sẽ là 'unknown'.")

    # Bao quanh các sticker bằng một bounding box lớn rồi chia 3x3.
    x_min = min(d["box"][0] for d in detections)
    y_min = min(d["box"][1] for d in detections)
    x_max = max(d["box"][2] for d in detections)
    y_max = max(d["box"][3] for d in detections)

    big_w = max(1, x_max - x_min)
    big_h = max(1, y_max - y_min)
    cell_w = big_w / 3.0
    cell_h = big_h / 3.0

    for d in detections:
        col = int((d["cx"] - x_min) / cell_w)
        row = int((d["cy"] - y_min) / cell_h)
        col = max(0, min(2, col))
        row = max(0, min(2, row))

        # Nếu trùng ô thì giữ detection có confidence cao hơn.
        if d["conf"] > score_map[row][col]:
            face[row][col] = d["color"]
            score_map[row][col] = d["conf"]

    return face


def detect_colors_by_yolo(frame, model, device):
    """
    Adapter thay cho khối detect_colors_in_box() OpenCV cũ.

    Input:
        frame: ảnh BGR từ cv2.VideoCapture.read().

    Output:
        detected_colors: list 3x3 tên màu, giống output cũ của detect_colors_in_box().
        annotated_frame: frame đã vẽ bbox YOLO để debug/hiển thị.
    """
    try:
        results = model.predict(
            source=frame,
            imgsz=YOLO_IMGSZ,
            conf=YOLO_CONF,
            device=device,
            verbose=False,
        )
    except Exception as ex:
        print(f"[YOLO] Lỗi predict: {ex}")
        return [["unknown"] * 3 for _ in range(3)], frame

    result = results[0]
    annotated_frame = result.plot()
    detections = []

    if result.boxes is None or len(result.boxes) == 0:
        #print("[YOLO] Cảnh báo: result không có boxes.")
        return [["unknown"] * 3 for _ in range(3)], annotated_frame

    names = result.names
    for box in result.boxes:
        cls_id = int(box.cls[0])
        conf = float(box.conf[0])
        raw_name = names.get(cls_id, str(cls_id)) if isinstance(names, dict) else str(cls_id)
        color_name = normalize_yolo_class_name(raw_name)

        if color_name is None:
            print(f"[YOLO] Class chưa map màu Rubik: {raw_name}")
            continue

        x1, y1, x2, y2 = box.xyxy[0].tolist()
        cx = (x1 + x2) / 2.0
        cy = (y1 + y2) / 2.0

        detections.append({
            "color": color_name,
            "conf": conf,
            "cx": cx,
            "cy": cy,
            "box": (int(x1), int(y1), int(x2), int(y2)),
            "raw_name": raw_name,
        })

    detected_colors = yolo_detections_to_3x3(detections)

    # Vẽ ma trận mà chương trình hiểu lên frame để debug thứ tự row/col.
    h_img, w_img = annotated_frame.shape[:2]
    text_x = max(20, w_img - 430)
    text_y0 = 40
    for r in range(3):
        line = " | ".join(detected_colors[r])
        cv2.putText(
            annotated_frame,
            f"row {r}: {line}",
            (text_x, text_y0 + r * 30),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.7,
            (0, 0, 255),
            2,
        )

    return detected_colors, annotated_frame


# ==============================================================================
# BẮT ĐẦU: KHỐI LOGIC TỪ UPGRADE.PY (Dán vào sau phần Import)
# ==============================================================================

# CẤU HÌNH TỐI ƯU
ASTAR_MAX_NODES = 5000      
PREMOVE_DEPTH_MAX = 2       
FORCED_ITERATIONS = 4       
CANDIDATE_POOL_SIZE = 50    

MOVE_COST: Dict[str, float] = {
    "U": 4.30, "U'": 4.30, "U2": 5.51,
    "D": 3.05, "D'": 3.05, "D2": 4.35,
    "L": 2.90, "L'": 2.90, "L2": 3.91,
    "R": 3.44, "R'": 3.44, "R2": 4.51,
    "F": 3.44, "F'": 3.44, "F2": 5.71,
    "B": 4.21, "B'": 4.21, "B2": 5.66,
}
HAND_SWITCH_COST = 2.00
HAND_OF = {"U":"L","D":"L","L":"L","R":"R","F":"R","B":"R"}
USE_MOVES = ("U","U'","U2","D","D'","D2","L","L'","L2","R","R'","R2","F","F'","F2","B","B'","B2")

@lru_cache(maxsize=20000)
def cached_kociemba_solve(facelets):
    try:
        return kociemba.solve(facelets)
    except:
        return ""

# --- HÀM HỖ TRỢ ---
def parse_moves(x) -> List[str]:
    if isinstance(x, str):
        return x.replace("\n"," ").strip().split()
    return [m for m in x if m]

def face_of(m: str) -> str: return m[0]

def exp_of(m: str) -> int:
    if m.endswith("2"): return 2
    if m.endswith("'"): return 3
    return 1

def move_from(face: str, e: int) -> str:
    e %= 4
    if e == 0: return ""
    if e == 1: return face
    if e == 2: return face + "2"
    return face + "'"

def reduce_sequence(seq: List[str]) -> List[str]:
    if not seq: return []
    out = []
    f0, e0 = face_of(seq[0]), exp_of(seq[0])
    for m in seq[1:]:
        f, e = face_of(m), exp_of(m)
        if f == f0:
            e0 = (e0 + e) % 4
        else:
            s = move_from(f0, e0)
            if s: out.append(s)
            f0, e0 = f, e
    s = move_from(f0, e0)
    if s: out.append(s)
    return out

def seq_time(seq: List[str]) -> float:
    if not seq: return 0.0
    t = MOVE_COST[seq[0]]
    prev = HAND_OF[face_of(seq[0])]
    for m in seq[1:]:
        h = HAND_OF[face_of(m)]
        if h != prev: t += HAND_SWITCH_COST
        t += MOVE_COST[m]
        prev = h
    return t

# --- OPTIMIZER ---
def neighbors_rl(seq: List[str]) -> List[List[str]]:
    outs = []
    for i in range(len(seq)-1):
        a, b = seq[i], seq[i+1]
        if {face_of(a), face_of(b)} == {"R","L"}:
            cand = seq[:]
            cand[i], cand[i+1] = cand[i+1], cand[i]
            cand = reduce_sequence(cand)
            outs.append(cand)
    return outs

def greedy_rl(seq: List[str]) -> List[str]:
    s = seq[:]
    improved = True
    while improved:
        improved = False
        i = 0
        while i < len(s)-1:
            a, b = s[i], s[i+1]
            if {face_of(a), face_of(b)} == {"R","L"}:
                base = seq_time(s)
                cand = s[:]
                cand[i], cand[i+1] = cand[i+1], cand[i]
                cand = reduce_sequence(cand)
                if seq_time(cand) + 1e-9 < base:
                    s = cand
                    improved = True
                    i = 0
                    continue
            i += 1
    return s

def astar_rl(seq: List[str], max_nodes: int) -> List[str]:
    start = greedy_rl(seq)
    start_c = seq_time(start)
    pq = []
    seen = set([tuple(start)])
    heapq.heappush(pq, (start_c, 0, start))
    best = start
    best_c = start_c
    push_id = 1
    while pq and push_id < max_nodes:
        c, _, cur = heapq.heappop(pq)
        if c + 1e-9 < best_c:
            best_c, best = c, cur
        for nb in neighbors_rl(cur):
            key = tuple(nb)
            if key in seen: continue
            seen.add(key)
            cn = seq_time(nb)
            heapq.heappush(pq, (cn, push_id, nb))
            push_id += 1
    return best

def rl_opt(seq: List[str]) -> List[str]:
    s = greedy_rl(seq)
    if len(s) > 2:
        s = astar_rl(s, max_nodes=ASTAR_MAX_NODES)
    return s

# --- SIMULATOR ---
CYCLES_U = [(0,2,8,6), (1,5,7,3), (18,36,45,9), (19,37,46,10), (20,38,47,11)]
CYCLES_D = [(27,29,35,33), (28,32,34,30), (24,15,51,42), (25,16,52,43), (26,17,53,44)]
CYCLES_F = [(18,20,26,24), (19,23,25,21), (6,9,29,44), (7,12,28,41), (8,15,27,38)]
CYCLES_B = [(45,47,53,51), (46,50,52,48), (2,36,33,17), (1,39,34,14), (0,42,35,11)]
CYCLES_L = [(36,38,44,42), (37,41,43,39), (0,18,27,53), (3,21,30,50), (6,24,33,47)]
CYCLES_R = [(9,11,17,15), (10,14,16,12), (20,2,51,29), (23,5,48,32), (26,8,45,35)]

def apply_cycles(state_list: List[str], cycles: List[Tuple[int, ...]]) -> List[str]:
    new_state = state_list[:]
    for cyc in cycles:
        for i in range(len(cyc)-1):
            new_state[cyc[i+1]] = state_list[cyc[i]]
        new_state[cyc[0]] = state_list[cyc[-1]]
    return new_state

def apply_move_on_facelets(state_str: str, m: str) -> str:
    s_list = list(state_str)
    face = face_of(m)
    times = exp_of(m)
    cycles = []
    if face == 'U': cycles = CYCLES_U
    elif face == 'D': cycles = CYCLES_D
    elif face == 'L': cycles = CYCLES_L
    elif face == 'R': cycles = CYCLES_R
    elif face == 'F': cycles = CYCLES_F
    elif face == 'B': cycles = CYCLES_B
    
    for _ in range(times):
        s_list = apply_cycles(s_list, cycles)
    return "".join(s_list)

def apply_sequence_on_facelets(state: str, seq: List[str]) -> str:
    s = state
    for m in seq:
        s = apply_move_on_facelets(s, m)
    return s

def is_inverse(a: str, b: str) -> bool:
    fa, fb = face_of(a), face_of(b)
    if fa != fb: return False
    ea, eb = exp_of(a), exp_of(b)
    return (ea + eb) % 4 == 0

def generate_premoves(depth_max: int) -> List[List[str]]:
    res = [[]]
    for _ in range(depth_max):
        cur = []
        for seq in res:
            last = seq[-1] if seq else ""
            for m in USE_MOVES:
                if last and face_of(last) == face_of(m): continue
                if seq and is_inverse(seq[-1], m): continue
                cur.append(seq + [m])
        res += cur
    return [s for s in res if s]

def find_best_premove_step(current_facelets: str) -> Tuple[List[str], float, List[str]]:
    try:
        base_raw = cached_kociemba_solve(current_facelets)
    except:
        return [], 9999, []

    if not base_raw or not base_raw.strip():
        return [], 0.0, []

    base_seq = rl_opt(parse_moves(base_raw))
    base_cost = seq_time(base_seq)
    
    if len(base_seq) <= PREMOVE_DEPTH_MAX:
        return [], base_cost, base_seq

    prem_list = generate_premoves(PREMOVE_DEPTH_MAX)
    candidates_rough = [] 

    for P in prem_list:
        try:
            S_next = apply_sequence_on_facelets(current_facelets, P)
            Q_str = cached_kociemba_solve(S_next)
        except:
            continue
            
        Q = parse_moves(Q_str)
        full_seq = reduce_sequence(P + Q)
        if not full_seq: continue 
        
        greedy_seq = greedy_rl(full_seq)
        c_greedy = seq_time(greedy_seq)
        candidates_rough.append((c_greedy, P, greedy_seq))

    candidates_rough.sort(key=lambda x: x[0])
    top_candidates = candidates_rough[:CANDIDATE_POOL_SIZE]

    real_best_cost = base_cost
    real_best_P = []
    real_best_seq = base_seq

    for _, P, seq in top_candidates:
        opt_seq = astar_rl(seq, max_nodes=ASTAR_MAX_NODES)
        c_opt = seq_time(opt_seq)
        if c_opt < real_best_cost:
            real_best_cost = c_opt
            real_best_P = P
            real_best_seq = opt_seq

    return real_best_P, real_best_cost, real_best_seq

# --- HÀM CHÍNH ĐỂ GỌI TỪ YUMI ---
def run_upgrade_solver(facelets: str) -> str:
    print(f"\n[UPGRADE] Bắt đầu tối ưu hóa: {facelets}")
    t_start = time.time()
    
    current_state = facelets
    final_committed_moves = []

    for i in range(1, FORCED_ITERATIONS + 1):
        best_P, predicted_cost, _ = find_best_premove_step(current_state)
        
        if not best_P:
            check = cached_kociemba_solve(current_state)
            if not check or not check.strip():
                break
            current_base = parse_moves(check)
            best_P = current_base[:PREMOVE_DEPTH_MAX] if len(current_base) > PREMOVE_DEPTH_MAX else current_base
            print(f"[Stage {i}] Fallback: Lấy {len(best_P)} moves")
        else:
            print(f"[Stage {i}] Tìm thấy Premove (Cost: {predicted_cost:.2f})")

        final_committed_moves.extend(best_P)
        current_state = apply_sequence_on_facelets(current_state, best_P)

    try:
        remainder_raw = cached_kociemba_solve(current_state)
        remainder_seq = parse_moves(remainder_raw)
        if remainder_seq:
            final_committed_moves.extend(remainder_seq)
    except:
        pass

    final_optimized = reduce_sequence(final_committed_moves)
    final_optimized = rl_opt(final_optimized)
    
    print(f"[UPGRADE] Xong. Robot Time: {seq_time(final_optimized):.2f}s")
    return " ".join(final_optimized)

# ==============================================================================
# KẾT THÚC: KHỐI LOGIC UPGRADE
# ==============================================================================

HOST = '127.0.0.1'
PORT = 1027
cv_color_dict = {
    'white': (255, 255, 255),
    'yellow': (0, 255, 255),
    'red': (0, 0, 255),
    'orange': (0, 165, 255),
    'green': (0, 255, 0),
    'blue': (255, 0, 0),
    'unknown': (50, 50, 50)
}

# ---------------------- VẼ LƯỚI ----------------------
GRID_SIZE = 350
GRID_COLOR = (255, 255, 255)  # Màu đường lưới (trắng)

def draw_fixed_grid(frame):
   
    h, w, _ = frame.shape
    x = (w - GRID_SIZE) // 2
    y = (h - GRID_SIZE) // 2
    cell_size = GRID_SIZE // 3

    # Khung ngoài
    cv2.rectangle(frame, (x, y), (x + GRID_SIZE, y + GRID_SIZE), (255, 0, 0), 1)

    # Đường lưới
    for i in range(1, 3):
        cv2.line(frame, (x + i * cell_size, y), (x + i * cell_size, y + GRID_SIZE), GRID_COLOR, 2)
        cv2.line(frame, (x, y + i * cell_size), (x + GRID_SIZE, y + i * cell_size), GRID_COLOR, 2)

    # Điểm tâm mỗi ô
    for row in range(3):
        for col in range(3):
            cx = int(x + col * cell_size + cell_size / 2)
            cy = int(y + row * cell_size + cell_size / 2)
            cv2.circle(frame, (cx, cy), 5, (0, 0, 255), -1)

    bbox = (x, y, GRID_SIZE, GRID_SIZE)
    return frame, bbox, cell_size

print("Working directory:", os.getcwd())
print(f"[PATH] All output files will be written under APP_DIR={APP_DIR}", flush=True)
# ---------------------- CẤU HÌNH ----------------------
output_folder = app_path("rubik_images")
if not output_folder.exists():
    output_folder.mkdir(parents=True, exist_ok=True)
else:
    for filename in os.listdir(output_folder):
        file_path = output_folder / filename
        try:
            if file_path.is_file() or file_path.is_symlink():
                file_path.unlink()
        except Exception as e:
            print(f'Không thể xóa {file_path}. Lý do: {e}', flush=True)

# -------------------- THREAD LẮNG NGHE 6 GÓI CAP -------------------- 
def cap_server():
    """Nhận CAP từ Form1, gửi ACK sau mỗi CAP, và gửi detected_case sau khi solver sẵn sàng."""
    global detected_case, case_ready_event

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((HOST, PORT))
        s.listen(1)
        print(f"[CAP-SERVER] Listening at {HOST}:{PORT} ...", flush=True)

        conn, addr = s.accept()
        with conn:
            print(f"[CAP-SERVER] C# connected from {addr}", flush=True)
            cap_cnt = 0

            while cap_cnt < 6:
                data = conn.recv(16)
                print(f"[CAP-SERVER] raw recv = {data!r}", flush=True)

                if not data:
                    print("[CAP-SERVER] C# closed socket before 6 CAP.", flush=True)
                    break

                if data.strip() == b"CAP":
                    cap_cnt += 1
                    cap_queue.put(1)
                    conn.sendall(b"ACK\n")
                    print(f"[CAP] {cap_cnt}/6 nhận, ACK sent", flush=True)
                else:
                    print(f"[CAP-SERVER] Unknown command: {data!r}", flush=True)

            if cap_cnt < 6:
                print(f"[CAP-SERVER] Not enough CAP: {cap_cnt}/6. Stop server thread.", flush=True)
                return

            print("[CAP-SERVER] 6 CAP received. Waiting case_ready_event...", flush=True)
            case_ready = case_ready_event.wait(timeout=60.0)
            print(f"[CAP-SERVER] case_ready_event={case_ready}, detected_case={detected_case}", flush=True)

            if case_ready and detected_case is not None:
                msg = f"{detected_case}\n".encode("ascii")
                try:
                    conn.sendall(msg)
                    print(f"[CAP-SERVER] SEND CASE SUCCESS: detected_case={detected_case}", flush=True)
                except Exception as e:
                    print(f"[CAP-SERVER][ERROR] send case failed: {e}", flush=True)
            else:
                print("[CAP-SERVER][ERROR] Timeout or detected_case is None. No case sent to C#.", flush=True)

            time.sleep(0.5)

def get_dshow_device_names():
    
    # Trả về danh sách tên DirectShow video devices theo thứ tự FilterGraph trả về.
    
    graph = FilterGraph()
    return graph.get_input_devices()  # ['USB Camera', 'e2eSoft iVCam', 'DroidCam Source 3', ...]

def find_usbcam_index(max_index=10):
    """
    Liệt kê toàn bộ camera, ưu tiên chọn camera ngoài (chứa chữ USB).
    Nếu không có, tự động chọn camera khả dụng cuối cùng (thường là thiết bị cắm thêm).
    """
    names = get_dshow_device_names()
    print("\n=== DANH SÁCH CAMERA ĐANG KẾT NỐI ===")
    for i, name in enumerate(names):
        print(f" [{i}] {name}")
    print("=====================================\n")

    if not names:
        return None

    # Bước 1: Ưu tiên tìm camera có chữ "USB" trong tên
    for i in range(min(len(names), max_index)):
        if "USB" in names[i].upper():
            cap = cv2.VideoCapture(i, cv2.CAP_DSHOW)
            if cap.isOpened():
                ret, _ = cap.read()
                cap.release()
                if ret:
                    print(f"[INFO] Tự động chọn camera USB: [{i}] {names[i]}")
                    return i

    # Bước 2: Nếu không có chữ USB, duyệt ngược từ dưới lên (ưu tiên thiết bị cắm vào sau)
    for i in reversed(range(min(len(names), max_index))):
        cap = cv2.VideoCapture(i, cv2.CAP_DSHOW)
        if cap.isOpened():
            ret, _ = cap.read()
            cap.release()
            if ret:
                print(f"[INFO] Tự động chọn camera khả dụng: [{i}] {names[i]}")
                return i

    return None

def run_recognition():
    global detected_case, case_ready_event, RECOGNITION_START_TIME, RECOGNITION_END_TIME
    init_experiment_case(CASE_ID)
    RECOGNITION_START_TIME = _now_iso()
    detected_case = None
    case_ready_event.clear()

    # Khởi động CAP-server ở thread nền
    # Cái này dùng cho socket
    #threading.Thread(target=cap_server, daemon=True).start()

    # Cái này dùng cho PCSDK
    cap_thread = threading.Thread(target=cap_server, name="Thread-cap_server")  # không đặt daemon
    cap_thread.start()
    #---------------------------------
    print("[MAIN] CAP-server đã khởi chạy")

    # Load YOLO một lần, không load lại trong từng frame để tránh chậm/socket bị block.
    yolo_model, yolo_device = load_yolo_model_once()

    # Tìm index camera USB
    cam_index = find_usbcam_index()
    if cam_index is None:
        raise RuntimeError("Không tìm thấy camera USB mở được!")
    print(f"[INFO] Đang mở camera USB với index = {cam_index}")

    # Mở camera theo index
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)
    if not cap.isOpened():
        raise RuntimeError(f"Không thể mở camera {cam_index}!")
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 720)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 560)
    # cap.set(cv2.CAP_PROP_AUTO_WB, 0)
    # cap.set(cv2.CAP_PROP_WB_TEMPERATURE, 5000)  # ánh sáng tự nhiên

    image_count = 0
    captured_faces = []
    face_order = ['U', 'R', 'F', 'D', 'L', 'B']

    color_mappings = {
        'white': 'U', 'yellow': 'D', 'red': 'R', 'orange': 'L',
        'green': 'F', 'blue': 'B', 'unknown': '?'
    }

    # Khoảng màu HSV
    color_ranges = {
    'yellow': ([21, 30, 100], [35, 255, 255]),
    'orange': ([10, 50, 50], [20, 255, 255]),
    'green': ([36, 50, 50], [80, 255, 255]),
    'white': ([0, 0, 100], [180, 50, 255]),
    'blue': ([81, 100, 50], [120, 255, 255])
    }
    red_ranges = [([0, 60, 50], [9, 255, 255]), ([121, 60, 50], [180, 255, 255])]

    # ---------------------- XỬ LÝ ẢNH ----------------------
    def preprocess_image(img):
        lab = cv2.cvtColor(img, cv2.COLOR_BGR2LAB)
        l, a, b = cv2.split(lab)
        clahe = cv2.createCLAHE(clipLimit=3.0, tileGridSize=(8,8))
        l = clahe.apply(l)
        enhanced_lab = cv2.merge((l, a, b))
        return cv2.cvtColor(enhanced_lab, cv2.COLOR_LAB2BGR)

    def enhance_dark_image(img, gamma=1.5):
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        hsv[:,:,2] = np.clip(hsv[:,:,2] * 1.2, 0, 255).astype(np.uint8)
        hsv[:,:,1] = np.clip(hsv[:,:,1] * 1.1, 0, 255).astype(np.uint8)
        img = cv2.cvtColor(hsv, cv2.COLOR_HSV2BGR)
        invGamma = 1.0 / gamma
        table = np.array([(i/255.0)**invGamma * 255 for i in np.arange(256)]).astype("uint8")
        return cv2.LUT(img, table)

    def adjust_hsv_for_darkness(hsv_image):
        return color_ranges

    # ---------------------- NHẬN DIỆN MÀU TRONG BOUNDING BOX ----------------------
    def detect_colors_in_box(frame, bbox, cell_size):
        
        #---------------------------
        # x, y, w, h = bbox
        # roi = frame[y:y+h, x:x+w].copy()
        # #processed = preprocess_image(roi)
        # #processed = enhance_dark_image(processed)
        # #processed = cv2.GaussianBlur(processed, (5,5), 0)
        # #processed = cv2.medianBlur(processed, 5)
        # #processed = cv2.bilateralFilter(processed, 9, 75, 75)
        # #hsv_roi = cv2.cvtColor(processed, cv2.COLOR_BGR2HSV)
        # #current_ranges = adjust_hsv_for_darkness(hsv_roi)
        # # Lưu ý: Biến dùng để vẽ hình chữ nhật (debug) cũng phải đổi thành 'roi'
        # processed = roi 
        
        # # Chuyển đổi trực tiếp ảnh gốc (roi) sang HSV
        # hsv_roi = cv2.cvtColor(roi, cv2.COLOR_BGR2HSV)
        
        # current_ranges = adjust_hsv_for_darkness(hsv_roi)

        #----------------------

        x, y, w, h = bbox
        roi = frame[y:y+h, x:x+w].copy()
        
        # --- BƯỚC 1: KÍCH HOẠT TĂNG SÁNG ---
        # Bỏ comment dòng này và set gamma = 2.0 (số càng lớn càng sáng)
        processed = enhance_dark_image(roi, gamma=1.5) 
        
        # Các bộ lọc làm mịn (blur) có thể giữ nguyên comment nếu không cần thiết
        # processed = cv2.GaussianBlur(processed, (5,5), 0)
        # processed = cv2.medianBlur(processed, 5)
        # processed = cv2.bilateralFilter(processed, 9, 75, 75)
        
        # --- BƯỚC 2: CHUYỂN ĐỔI ẢNH ĐÃ TĂNG SÁNG SANG HSV ---
        # Quan trọng: Phải dùng biến 'processed' (ảnh sáng), KHÔNG dùng 'roi' (ảnh gốc tối)
        hsv_roi = cv2.cvtColor(processed, cv2.COLOR_BGR2HSV)
        
        # current_ranges = adjust_hsv_for_darkness(hsv_roi) # Có thể giữ comment dòng này

        detected_colors = []
        color_dict = {
            'white': (255, 255, 255),
            'yellow': (0, 255, 255),
            'red': (0, 0, 255),
            'orange': (0, 165, 255),
            'green': (0, 255, 0),
            'blue': (255, 0, 0),
            'unknown': (50, 50, 50)
        }
        
        for i in range(3):           # i = 0,1,2 (3 hàng)
            row_colors = []
            for j in range(3):       # j = 0,1,2 (3 cột)
                # Góc trên trái của ô con trong hsv_roi
                x1 = j * cell_size
                y1 = i * cell_size

                # Kích thước ô vuông mẫu = 40% của ô con
                small_square_size = int(cell_size * 0.4)
                offset = (cell_size - small_square_size) // 2

                # --------- CHỈ LẤY pixel TRONG Ô VUÔNG NHỎ ---------
                roi_cell = hsv_roi[
                    y1 + offset : y1 + offset + small_square_size,
                    x1 + offset : x1 + offset + small_square_size
                ]

                # Tính giá trị HSV trung bình trên roi_cell
                avg_hsv = np.mean(roi_cell, axis=(0,1)).astype(int)

                # Bắt đầu nhận diện: ưu tiên kiểm tra màu đỏ trước
                color_name = 'unknown'
                for lower, upper in red_ranges:
                    if all(lower[k] <= avg_hsv[k] <= upper[k] for k in range(3)):
                        color_name = 'red'
                        break

                # Nếu chưa là đỏ, kiểm tra các màu còn lại
                if color_name == 'unknown':
                    for color, (lower, upper) in color_ranges.items():
                        if all(lower[k] <= avg_hsv[k] <= upper[k] for k in range(3)):
                            color_name = color
                            break

                row_colors.append(color_name)

                # VẼ một ô vuông nhỏ (fill color) ngay đúng vị trí roi_cell để debug/hiển thị
                cv2.rectangle(
                    processed, 
                    (x1 + offset, y1 + offset), 
                    (x1 + offset + small_square_size, y1 + offset + small_square_size),
                    color_dict[color_name], 
                    -1
                )

            detected_colors.append(row_colors)

        return detected_colors, processed

    # ---------------------- HIỂN THỊ PREVIEW ----------------------
    def draw_preview(frame, current_colors):
        if current_colors is None:
            return frame
        preview_size = 150
        preview = np.zeros((preview_size, preview_size, 3), dtype=np.uint8) + 50
        face_size = preview_size // 3
        for i, row in enumerate(current_colors):
            for j, color in enumerate(row):
                color_bgr = cv_color_dict.get(color, cv_color_dict["unknown"])
                x1, y1 = j * face_size, i * face_size
                cv2.rectangle(preview, (x1, y1), (x1 + face_size, y1 + face_size), color_bgr, -1)
                cv2.rectangle(preview, (x1, y1), (x1 + face_size, y1 + face_size), (0, 0, 0), 1)
        frame[10:10 + preview_size, 10:10 + preview_size] = preview
        return frame

    # ---------------------- HÀM XOAY ----------------------
    def rotate_90_ccw(matrix):
        N = len(matrix)
        new_mat = [[None]*N for _ in range(N)]
        for i in range(N):
            for j in range(N):
                new_mat[i][j] = matrix[j][N-1-i]
        return new_mat

    def rotate_270_ccw(matrix):
        N = len(matrix)
        new_mat = [[None]*N for _ in range(N)]
        for i in range(N):
            for j in range(N):
                new_mat[i][j] = matrix[N-1-j][i]
        return new_mat

    def rotate_90_cw(matrix):
        return rotate_270_ccw(matrix)

    def rotate_180(matrix):
        N = len(matrix)
        new_mat = [[None]*N for _ in range(N)]
        for i in range(N):
            for j in range(N):
                new_mat[i][j] = matrix[N-1-i][N-1-j]
        return new_mat

    # ---------------------- HÀM CHUYỂN ĐỔI ----------------------
    def determine_face_order(captured_faces):
        center_colors = [face[1][1] for face in captured_faces]
        ordered_faces = {}
        for i, color in enumerate(center_colors):
            if color in color_mappings:
                ordered_faces[color_mappings[color]] = captured_faces[i]
        return [ordered_faces[face] for face in face_order]

    def convert_to_kociemba(ordered_faces):
        face_string = ''.join([color_mappings[color] for face in ordered_faces for row in face for color in row])
        return face_string

    def solve_rubik(cube_string):
        """Gọi solver và ghi solution.txt đúng format cũ. Không ghi solution thành công nếu rỗng."""
        try:
            solution = run_upgrade_solver(cube_string)
            moves = parse_solution_moves_from_text(solution)

            if not moves:
                print("[SOLVER][ERROR] Solver trả solution rỗng. Không ghi solution.txt thành công.", flush=True)
                return None

            print("Giải pháp Rubik:")
            solution_path = app_path("solution.txt")
            with open(solution_path, 'w', encoding='utf-8') as f:
                for i, move in enumerate(moves, 1):
                    print(f"Bước {i}: {move}")
                    f.write(move + "\n")
                # Giữ nguyên format cũ: E là ký hiệu kết thúc, C# sẽ bỏ qua khi log/gửi robot.
                f.write("E\n")
                f.flush()
                os.fsync(f.fileno())
            print(f"[SOLVER] WRITE solution.txt SUCCESS: {solution_path}", flush=True)
            return " ".join(moves)
        except Exception as e:
            print("Lỗi khi giải Rubik:", e)
            return None
        
    def create_unfold_image(ordered_faces, cell_size=40):
        """
        Tạo ảnh unfolded của Rubik theo layout:
        - Top (U) ở vị trí (3,0)
        - Left (L) ở vị trí (0,3)
        - Front (F) ở vị trí (3,3)
        - Right (R) ở vị trí (6,3)
        - Back (B) ở vị trí (9,3)
        - Bottom (D) ở vị trí (3,6)
        Mỗi ô có kích thước cell_size x cell_size pixel.
        """
        rows = 9
        cols = 12
        height = rows * cell_size
        width = cols * cell_size
        # Tạo ảnh nền với màu light gray (giá trị 200)
        img = np.full((height, width, 3), 200, dtype=np.uint8)
        
        # Hàm vẽ một mặt (3x3) vào ảnh tại vị trí grid (grid_x, grid_y) (tính theo số ô)
        def draw_face(face, grid_x, grid_y):
            for r in range(3):
                for c in range(3):
                    token = face[r][c]
                    color = cv_color_dict.get(token, cv_color_dict["unknown"])
                    x = (grid_x + c) * cell_size
                    y = (grid_y + r) * cell_size
                    cv2.rectangle(img, (x, y), (x+cell_size, y+cell_size), color, -1)
                    cv2.rectangle(img, (x, y), (x+cell_size, y+cell_size), (0,0,0), 1)
        
        # Mapping theo nhãn: face_order = ['U','R','F','D','L','B']
        # Chúng ta hiển thị theo:
        # Top (U) tại (3,0); Left (L) tại (0,3); Front (F) tại (3,3);
        # Right (R) tại (6,3); Back (B) tại (9,3); Bottom (D) tại (3,6).
        mapping = {
            'U': (3, 0),
            'R': (6, 3),
            'F': (3, 3),
            'D': (3, 6),
            'L': (0, 3),
            'B': (9, 3)
        }
        for token, pos in mapping.items():
            # Lấy vị trí (index) trong face_order
            idx = face_order.index(token)
            face = ordered_faces[idx]
            draw_face(face, pos[0], pos[1])
        return img

    # ---------------------- HÀM ĐIỀU CHỈNH CHO SOLVER ----------------------
    def adjust_faces_for_solver(raw_faces):
        global detected_case
        solver_faces = copy.deepcopy(raw_faces)
        centers = [face[1][1] for face in solver_faces[:3]]
        print("Center của 3 mặt đầu tiên (theo thứ tự chụp):", centers)
        
        if centers == ['green', 'blue', 'yellow']:
            # Th1
            detected_case = 1
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'yellow':
                    solver_faces[i] = rotate_180(face)
                    print("Th1: Mặt có center 'yellow' được xoay 180°.")
                else:
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th1: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
        elif centers == ['red', 'orange', 'yellow']:
            # Th2
            detected_case = 2
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white':
                    solver_faces[i] = rotate_180(face)
                    print("Th2: Mặt có center 'white' được xoay 180°.")
                else:
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th2: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
        elif centers == ['blue', 'green', 'yellow']:
            # Th3
            detected_case = 3
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'yellow':
                    print("Th3: Mặt có center 'yellow' không xoay.")
                elif c == 'white':
                    solver_faces[i] = rotate_90_cw(face)
                    print("Th3: Mặt có center 'white' được xoay 90° theo chiều kim đồng hồ.")
                else:
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th3: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
        elif centers == ['orange', 'red', 'yellow']:
            # Th4
            detected_case = 4
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'yellow':
                    solver_faces[i] = rotate_90_cw(face)
                    print("Th4: Mặt có center 'yellow' được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'white':
                    print("Th4: Mặt có center 'white' không xoay.")
                else:
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th4: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
        elif centers == ['white', 'yellow', 'blue']:
            # Th5
            detected_case = 5
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white' or c == 'green':
                    solver_faces[i] = rotate_90_cw(face)
                    print("Th5: Mặt có center 'white' được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'yellow':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th5: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'blue' or c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th5: Mặt có center {c} được xoay 180°.")
                elif c == 'orange':
                    print("Th5: Mặt có center 'orange' giữ nguyên không xoay.")
        elif centers == ['orange', 'red', 'blue']:
            # Th6
            detected_case = 6
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white':
                    solver_faces[i] = rotate_90_cw(face)
                    print("Th6: Mặt có center 'white' được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'yellow' or c == 'blue':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th6: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print("Th6: Mặt có center 'red' được xoay 180°.")
                elif c == 'orange' or c == 'green':
                    print(f"Th6: Mặt có center {c} giữ nguyên không xoay.")
        elif centers == ['yellow', 'white', 'blue']:
            # Th7
            detected_case = 7
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white':
                    solver_faces[i] = rotate_90_cw(face)
                    print("Th7: Mặt có center 'white' được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'yellow' or c == 'green':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th7: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print("Th7: Mặt có center 'red' được xoay 180°.")
                elif c == 'orange' or c == 'blue':
                    print(f"Th7: Mặt có center {c} giữ nguyên không xoay.")
        elif centers == ['red', 'orange', 'blue']:
            # Th8
            detected_case = 8
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white' or c == 'blue':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th8: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'yellow':
                    solver_faces[i] = rotate_90_ccw(face)
                    print("Th8: Mặt có center 'yellow' được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'green' or c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th8: Mặt có center {c} được xoay 180°.")
                elif c == 'orange':
                    print("Th8: Mặt có center 'orange' giữ nguyên không xoay.")
        elif centers == ['green', 'blue', 'white']:
            # Th9
            detected_case = 9
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white':
                    print("Th9: Mặt có center 'white' giữ nguyên không xoay.")
                elif c == 'green' or c == 'blue' or c == 'orange' or c == 'red' or c == 'yellow':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th9: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
        elif centers == ['orange', 'red', 'white']:
            #Th 10
            detected_case = 10
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'yellow':
                    print("Th10: Mặt có center 'yellow' giữ nguyên không xoay.")
                elif c == 'green' or c == 'blue' or c == 'orange' or c == 'red' or c == 'white':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th10: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
        elif centers == ['blue', 'green', 'white']:
            # Th11
            detected_case = 11
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'white':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th11: Mặt có center {c} được xoay 180°.")
                elif c == 'yellow':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th11: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'green' or c == 'blue' or c == 'orange' or c == 'red':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th11: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
        elif centers == ['red', 'orange', 'white']:
            # Th12
            detected_case = 12
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'yellow':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th12: Mặt có center {c} được xoay 180°.")
                elif c == 'white':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th12: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'green' or c == 'blue' or c == 'orange' or c == 'red':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th12: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
        elif centers == ['blue', 'green', 'orange']:
            # Th13
            detected_case = 13
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'orange':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th12: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'blue' or c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th13: Mặt có center {c} được xoay 180°.")
                elif c == 'green' or c == 'yellow' or c == 'white':
                    print("Th13: Giữ nguyên không xoay.")
        elif centers == ['white', 'yellow', 'orange']:
            # Th14
            detected_case = 14
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'red':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th14: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'blue' or c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th14: Mặt có center {c} được xoay 180°.")
                elif c == 'green' or c == 'yellow' or c == 'white':
                    print("Th14: Giữ nguyên không xoay.")
        elif centers == ['green', 'blue', 'orange']:
            # Th15
            detected_case = 15
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'blue':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th15: Mặt có center {c} được xoay 180°.")
                elif c == 'orange':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th15: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'green' or c == 'yellow' or c == 'white' or c == 'red':
                    print("Th15: Giữ nguyên không xoay.")
        elif centers == ['yellow', 'white', 'orange']:
            # Th16
            detected_case = 16
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'blue':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th16: Mặt có center {c} được xoay 180°.")
                elif c == 'red':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th16: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'green' or c == 'yellow' or c == 'white' or c == 'orange':
                    print("Th16: Giữ nguyên không xoay.")
        elif centers == ['yellow', 'white', 'green']:
            # Th17
            detected_case = 17
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th17: Mặt có center {c} được xoay 180°.")
                elif c == 'white' or c == 'blue':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th17: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'yellow':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th17: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'green' or c == 'red':
                    print("Th17: Giữ nguyên không xoay.")
        elif centers == ['orange', 'red', 'green']:
            # Th18
            detected_case = 18
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'blue' or c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th18: Mặt có center {c} được xoay 180°.")
                elif c == 'white':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th18: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'yellow' or c == 'green':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th18: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'red':
                    print("Th18: Giữ nguyên không xoay.")
        elif centers == ['white', 'yellow', 'green']:
            # Th19
            detected_case = 19
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'green' or c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th19: Mặt có center {c} được xoay 180°.")
                elif c == 'yellow' or c == 'blue':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th19: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'white':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th19: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'red':
                    print("Th19: Giữ nguyên không xoay.")
        elif centers == ['red', 'orange', 'green']:
            # Th20
            detected_case = 20
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th20: Mặt có center {c} được xoay 180°.")
                elif c == 'yellow':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th20: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'green' or c == 'white':
                    solver_faces[i] = rotate_90_ccw(face)
                    print(f"Th20: Mặt có center {c} được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'red' or c == 'blue':
                    print("Th20: Giữ nguyên không xoay.")
        elif centers == ['green', 'blue', 'red']:
            # Th21
            detected_case = 21
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'red':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th21: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'blue':
                    print("Th21: Giữ nguyên không xoay.")
                elif c == 'green' or c == 'white' or c == 'yellow' or c == 'orange':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th20: Mặt có center {c} được xoay 180°.")
        elif centers == ['white', 'yellow', 'red']:
            # Th22
            detected_case = 22
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'orange':
                    solver_faces[i] = rotate_90_cw(face)
                    print(f"Th22: Mặt có center {c} được xoay 90° theo chiều kim đồng hồ.")
                elif c == 'blue':
                    print("Th22: Giữ nguyên không xoay.")
                elif c == 'green' or c == 'white' or c == 'yellow' or c == 'red':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th22: Mặt có center {c} được xoay 180°.")
        elif centers == ['blue', 'green', 'red']:
            # Th23
            detected_case = 23
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'green' or c == 'yellow' or c == 'white':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th23: Mặt có center {c} được xoay 180°.")
                elif c == 'red':
                    solver_faces[i] = rotate_90_ccw(face)
                    print("Th23: Mặt có center 'red' được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'blue' or c == 'orange':
                    print("Th23: Giữ nguyên không xoay.")
        elif centers == ['yellow', 'white', 'red']:
            # Th24
            detected_case = 24
            for i, face in enumerate(solver_faces):
                c = face[1][1]
                if c == 'orange':
                    solver_faces[i] = rotate_90_ccw(face)
                    print("Th24: Mặt có center 'red' được xoay 90° ngược chiều kim đồng hồ.")
                elif c == 'blue' or c == 'red':
                    print("Th24: Giữ nguyên không xoay.")
                elif c == 'green' or c == 'white' or c == 'yellow':
                    solver_faces[i] = rotate_180(face)
                    print(f"Th23: Mặt có center {c} được xoay 180°.")
        else:
            detected_case = None
            print("Không xác định được trường hợp, không xoay.")
        
        return solver_faces
        

    # ---------------------- QUY TRÌNH XỬ LÝ TRẠNG THÁI RUBIK ----------------------
    def process_rubik_state(captured_faces):
        """Xử lý 6 mặt Rubik đúng 1 lần, ghi file, solve, tạo unfold, bật case_ready_event khi có case hợp lệ.

        Bản an toàn: nếu detect lỗi/unknown/case không xác định thì KHÔNG crash KeyError.
        Khi lỗi, hàm trả (None, [], None), cap_server sẽ không gửi case rác lên C#.
        """
        global detected_case, case_ready_event, RECOGNITION_END_TIME

        def _fail_solution(reason, centers=None):
            log_solution_row(
                rubik_state_string="",
                state_valid=False,
                solution_text="",
                solution_move_count=0,
                solver_success=False,
                solver_error_message=reason,
                centers=centers or [],
                capture_face_count=len(captured_faces),
                total_unknown_count=sum(_unknown_count(face) for face in captured_faces),
                recognition_reacquisition_total=RECOGNITION_REACQ_TOTAL,
            )
            return None, [], None

        print(f"[SOLVER] ENTER process_rubik_state(), len(captured_faces)={len(captured_faces)}", flush=True)

        if len(captured_faces) != 6:
            print(f"[SOLVER][ERROR] Chưa đủ 6 mặt: {len(captured_faces)}/6. Không solve.", flush=True)
            return _fail_solution(f"Chưa đủ 6 mặt: {len(captured_faces)}/6")

        # Log toàn bộ 6 mặt trước khi xử lý để debug nhanh.
        for idx, face in enumerate(captured_faces, start=1):
            print(f"[SOLVER] raw face {idx}: {face}", flush=True)

        raw_centers = [face[1][1] for face in captured_faces]
        print(f"[SOLVER] raw centers 6 faces={raw_centers}", flush=True)

        # Chặn lỗi rõ ràng: center unknown thì không thể xác định case 24 trường hợp.
        if any(c == 'unknown' or c is None or str(c).strip() == '' for c in raw_centers):
            print("[SOLVER][ERROR] Có center bị unknown/rỗng. Không thể xác định case.", flush=True)
            print("[SOLVER][ERROR] Nguyên nhân thường gặp: C# gửi CAP quá sớm hoặc YOLO chưa thấy đủ Rubik.", flush=True)
            print("[SOLVER][ERROR] Hãy sửa C# để prime lastcapValue trước khi bật timer, và chụp lại.", flush=True)
            detected_case = None
            return _fail_solution("Có center bị unknown/rỗng", raw_centers)

        # 6 center phải là 6 màu khác nhau. Nếu trùng/mất màu thì trạng thái không hợp lệ.
        if len(set(raw_centers)) != 6:
            print(f"[SOLVER][ERROR] 6 center không đủ 6 màu khác nhau: {raw_centers}. Không solve.", flush=True)
            detected_case = None
            return _fail_solution("6 center không đủ 6 màu khác nhau", raw_centers)

        solver_faces = adjust_faces_for_solver(captured_faces)
        print(f"[SOLVER] After adjust_faces_for_solver(), detected_case={detected_case}", flush=True)

        if detected_case is None:
            print("[SOLVER][ERROR] detected_case is None. Không gửi case lên C#.", flush=True)
            print("[SOLVER][ERROR] 3 center đầu không khớp 24 case trong adjust_faces_for_solver().", flush=True)
            return _fail_solution("detected_case is None; 3 center đầu không khớp 24 case", raw_centers)

        # Chưa set case_ready_event ở đây.
        # Chỉ gửi case sang C# sau khi cube state hợp lệ và solution.txt đã tạo thành công.

        try:
            ordered_faces = determine_face_order(solver_faces)
        except KeyError as e:
            print(f"[SOLVER][ERROR] determine_face_order() thiếu mặt {e}. Không solve.", flush=True)
            print("[SOLVER][ERROR] Thường do center sai/unknown/trùng màu.", flush=True)
            detected_case = None
            return _fail_solution(f"determine_face_order thiếu mặt {e}", raw_centers)
        except Exception as e:
            print(f"[SOLVER][ERROR] determine_face_order() lỗi: {e}", flush=True)
            detected_case = None
            return _fail_solution(f"determine_face_order lỗi: {e}", raw_centers)

        counts = [
            sum(1 for row in face for c in row if c != 'unknown')
            for face in ordered_faces
        ]
        print(f"[SOLVER] non-unknown counts per ordered face={counts}", flush=True)

        if counts.count(9) == 5 and any(cnt < 9 for cnt in counts):
            missing_idx = next(i for i, cnt in enumerate(counts) if cnt < 9)
            flat_all = [
                c
                for face in ordered_faces
                for row in face
                for c in row
                if c != 'unknown'
            ]
            color_counts = Counter(flat_all)
            missing_color = next((color for color, cnt in color_counts.items() if cnt < 9), None)

            if missing_color is not None:
                replaced = 0
                for r in range(3):
                    for c in range(3):
                        if ordered_faces[missing_idx][r][c] == 'unknown':
                            ordered_faces[missing_idx][r][c] = missing_color
                            replaced += 1

                print(f"[INFO] Face #{missing_idx+1} thiếu {9 - counts[missing_idx]} ô → gán {replaced} ô 'unknown' thành '{missing_color}'", flush=True)

        # Nếu vẫn còn unknown nhiều thì không solve, vì kociemba sẽ lỗi hoặc ra sai.
        remain_unknown = sum(1 for face in ordered_faces for row in face for c in row if c == 'unknown')
        if remain_unknown > 0:
            print(f"[SOLVER][ERROR] Vẫn còn {remain_unknown} ô unknown sau sửa thiếu 1 mặt. Không solve.", flush=True)
            detected_case = None
            return _fail_solution(f"Vẫn còn {remain_unknown} ô unknown sau sửa thiếu 1 mặt", raw_centers)

        state_path = app_path("rubik_state.txt")
        try:
            with open(state_path, 'w', encoding='utf-8') as f:
                for face in ordered_faces:
                    flat_face = []
                    for row in face:
                        flat_face.extend(row)
                    f.write(" ".join(flat_face) + "\n")
                f.flush()
                os.fsync(f.fileno())
            print(f"[SOLVER] WRITE rubik_state.txt SUCCESS: {state_path}", flush=True)
        except Exception as e:
            print(f"[SOLVER][ERROR] write rubik_state.txt failed: {e}", flush=True)
            detected_case = None
            return _fail_solution(f"write rubik_state.txt failed: {e}", raw_centers)

        try:
            kociemba_string = convert_to_kociemba(ordered_faces)
            print("Chuỗi nhập vào kociemba:", kociemba_string, flush=True)

            state_valid, state_error = validate_kociemba_string(kociemba_string)
            if not state_valid:
                print(f"[SOLVER][ERROR] {state_error}. Không gọi solver, không gửi robot chạy.", flush=True)
                detected_case = None
                log_solution_row(
                    rubik_state_string=kociemba_string,
                    state_valid=False,
                    solution_text="",
                    solution_move_count=0,
                    solver_success=False,
                    solver_error_message=state_error,
                    centers=raw_centers,
                    capture_face_count=len(captured_faces),
                    total_unknown_count=sum(_unknown_count(face) for face in captured_faces),
                    recognition_reacquisition_total=RECOGNITION_REACQ_TOTAL,
                )
                return None, [], None

            solution = solve_rubik(kociemba_string)
            print(f"[SOLVER] solution={solution}", flush=True)
            print(f"[SOLVER] solution.txt path={app_path('solution.txt')}", flush=True)

            moves = parse_solution_moves_from_text(solution)
            if not moves:
                print("[SOLVER][ERROR] SOLVER_FAILED: solution rỗng. Không gửi case sang C#.", flush=True)
                detected_case = None
                log_solution_row(
                    rubik_state_string=kociemba_string,
                    state_valid=True,
                    solution_text="",
                    solution_move_count=0,
                    solver_success=False,
                    solver_error_message="SOLVER_FAILED: solution rỗng",
                    centers=raw_centers,
                    capture_face_count=len(captured_faces),
                    total_unknown_count=sum(_unknown_count(face) for face in captured_faces),
                    recognition_reacquisition_total=RECOGNITION_REACQ_TOTAL,
                )
                return None, [], None

            unfold_img = create_unfold_image(ordered_faces, cell_size=40)
            cv2.imwrite(str(output_folder / 'unfold.png'), unfold_img)

            RECOGNITION_END_TIME = _now_iso()
            log_solution_row(
                rubik_state_string=kociemba_string,
                state_valid=True,
                solution_text=" ".join(moves),
                solution_move_count=len(moves),
                solver_success=True,
                solver_error_message="",
                centers=raw_centers,
                capture_face_count=len(captured_faces),
                total_unknown_count=sum(_unknown_count(face) for face in captured_faces),
                recognition_reacquisition_total=RECOGNITION_REACQ_TOTAL,
            )

            # Chỉ lúc này mới gửi case sang C#: case hợp lệ + solution.txt đã ghi thành công.
            case_ready_event.set()
            print("[SOLVER] case_ready_event.set() AFTER solution success", flush=True)

            print(f"[SOLVER] EXIT process_rubik_state(), detected_case={detected_case}", flush=True)
            return ordered_faces, moves, unfold_img
        except Exception as e:
            print(f"[SOLVER][ERROR] solve/write unfold failed: {e}", flush=True)
            detected_case = None
            return _fail_solution(f"solve/write unfold failed: {e}", raw_centers)

    # ---------------------- VÒNG LẶP CHÍNH ----------------------
    result_ordered_faces = None
    result_moves = []
    result_unfold_img = None

    while True:
        ret, frame = cap.read()
        if not ret:
            print("Lỗi khi đọc dữ liệu từ camera!")
            break

        # 1) Nhận diện màu bằng YOLO.
        raw_frame_for_log = frame.copy()
        # Output detected_colors vẫn là list 3x3 giống OpenCV cũ:
        # [["yellow", "blue", ...], [...], [...]]
        detected_colors, frame = detect_colors_by_yolo(frame, yolo_model, yolo_device)

        # Preview 3x3 góc trái giữ nguyên để xem chương trình hiểu mặt hiện tại ra sao.
        frame = draw_preview(frame, detected_colors)

        # ───────── 2) XỬ LÝ TÍN HIỆU CAP (từ Form1) ─────────
        while not cap_queue.empty():
            cap_queue.get()                             # lấy 1 CAP
            if detected_colors is None:                 # chưa thấy Rubik
                print("[CAP] Đến nhưng chưa detect màu – bỏ qua")
                continue

            captured_faces.append(detected_colors)
            image_count += 1
            print(f"[CAP] Đã chụp mặt {image_count}: {detected_colors}")
            log_recognition_face(image_count, detected_colors, raw_frame_for_log, frame, recognition_success=True, reacquisition_count=0, note="CAP")

            if image_count == 6:
                print("[CAP] Đủ 6 mặt – tự động giải Rubik…")
                result_ordered_faces, result_moves, result_unfold_img = process_rubik_state(captured_faces)
                ret = False                             # để thoát vòng lặp
                break                                   # thoát CAP-while
        if not ret or image_count == 6:                 # đã giải xong
            break
        # ────────────────────────────────────────────────────

        # 3) Hiển thị khung & đọc phím
        cv2.imshow('Rubik Color Detection', frame)
        key = cv2.waitKey(1)

        # --- Nhấn Enter (giữ nguyên logic cũ) ---
        if key == 13:
            if detected_colors is not None:
                captured_faces.append(detected_colors)
                image_count += 1
                print(f'Mặt {image_count}:', detected_colors)
                log_recognition_face(image_count, detected_colors, raw_frame_for_log, frame, recognition_success=True, reacquisition_count=0, note="MANUAL_ENTER")
                if image_count == 6:
                    print("Đã chụp đủ 6 mặt – tự động giải Rubik…")
                    result_ordered_faces, result_moves, result_unfold_img = process_rubik_state(captured_faces)
                    break
            else:
                print("Không nhận diện được màu!")

        # --- Nhấn r để chụp lại ---
        elif key == ord('r'):
            if image_count > 0:
                print("Chụp lại mặt vừa chụp…")
                globals()["RECOGNITION_REACQ_TOTAL"] = globals().get("RECOGNITION_REACQ_TOTAL", 0) + 1
                captured_faces.pop()
                image_count -= 1

        # --- Nhấn Esc để thoát ---
        elif key == 27:
            print("Thoát chương trình.")
            break

        # Có thể bỏ qua hiển thị cửa sổ phụ nếu không cần:
        # cv2.imshow('Colored ROI', colored_roi)

    cap.release()
    cv2.destroyAllWindows()

    if result_ordered_faces is None:
        print("[MAIN][WARN] Chưa có đủ 6 mặt hoặc chưa solve được.", flush=True)
        cap_thread.join(timeout=3.0)
        print("[MAIN] CAP-server join timeout done", flush=True)
        return [], [], None

    cap_thread.join(timeout=3.0)
    print("[MAIN] CAP-server đã kết thúc hoặc hết timeout join", flush=True)
    return result_ordered_faces, result_moves, result_unfold_img