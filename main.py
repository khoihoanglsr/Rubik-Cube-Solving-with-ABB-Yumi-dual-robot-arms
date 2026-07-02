import os
import argparse
from pathlib import Path


def resolve_app_dir(app_dir_arg):
    """Folder C# output. Python always writes solution.txt/rubik_state.txt/logs here."""
    app_dir = app_dir_arg or os.environ.get("RUBIK_APP_DIR") or os.getcwd()
    app_dir_path = Path(app_dir).expanduser().resolve()
    app_dir_path.mkdir(parents=True, exist_ok=True)
    return str(app_dir_path)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--case-id", default=None, help="Case ID dùng chung Python/C#, ví dụ CASE_20260626_145501_001")
    parser.add_argument("--app-dir", default=None, help="Folder C# output chứa RubikSolver.exe, solution.txt, rubik_state.txt, logs")
    args = parser.parse_args()

    print("[MAIN] Đang khởi động hệ thống...")
    print("[MAIN] Python file:", __file__)
    print("[MAIN] Original working directory:", os.getcwd())

    app_dir = resolve_app_dir(args.app_dir)
    os.environ["RUBIK_APP_DIR"] = app_dir
    os.chdir(app_dir)

    print("[MAIN] APP_DIR =", app_dir)
    print("[MAIN] Working directory after chdir =", os.getcwd())
    print("[MAIN] solution.txt will be written to =", os.path.abspath("solution.txt"))
    print("[MAIN] rubik_state.txt will be written to =", os.path.abspath("rubik_state.txt"))
    print("[MAIN] current_case_id.txt will be written to =", os.path.abspath("current_case_id.txt"))
    print("[MAIN] logs dir =", os.path.abspath("logs"))

    # Xóa solution.txt cũ trong APP_DIR để C# không đọc nhầm kết quả case trước.
    if os.path.exists("solution.txt"):
        try:
            os.remove("solution.txt")
            print("[MAIN] Đã xóa solution.txt cũ trong APP_DIR")
        except Exception as e:
            print("[MAIN][WARN] Không xóa được solution.txt cũ trong APP_DIR:", e)

    # Import sau khi đã set RUBIK_APP_DIR và chdir(APP_DIR).
    from newyumisolvingrubik import run_recognition, init_experiment_case, APP_DIR as MODULE_APP_DIR

    print("[MAIN] newyumisolvingrubik.APP_DIR =", MODULE_APP_DIR)

    init_experiment_case(args.case_id)

    print("[MAIN] Bắt đầu nhận diện Rubik...")
    ordered_faces, moves, unfold_img = run_recognition()

    if moves:
        print(f"Giải pháp tìm thấy: {' '.join(moves)}")
    else:
        print("Không tìm thấy giải pháp hoặc chưa đủ 6 mặt.")


if __name__ == "__main__":
    main()
