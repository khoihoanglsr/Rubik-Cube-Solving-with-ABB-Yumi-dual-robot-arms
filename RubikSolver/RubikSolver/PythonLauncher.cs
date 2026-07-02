using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RubikSolver1
{
    public static class PythonLauncher
    {
        private const string ResourceName = "RubikSolver.Resource.Python.main.exe";

        /// <summary>
        /// Giải nén main.exe từ embedded resource trực tiếp vào Application.StartupPath
        /// </summary>
        public static string ExtractPythonExe()
        {
            var asm = Assembly.GetExecutingAssembly();
            string tempDir = Path.Combine(Path.GetTempPath(), "MyRubik_Python");
            Directory.CreateDirectory(tempDir);

            string targetPath = Path.Combine(tempDir, "main.exe");
            // Nếu đã tồn tại, xóa để ghi đè
            if (File.Exists(targetPath))
            {
                try { File.Delete(targetPath); }
                catch { }
            }

            // Lấy stream resource và ghi ra file
            using (Stream resStream = asm.GetManifestResourceStream(ResourceName))
            {
                if (resStream == null)
                    throw new Exception($"Không tìm thấy embedded resource: {ResourceName}");

                using (var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                {
                    resStream.CopyTo(fs);
                }
            }

            return targetPath;
        }

        /// <summary>
        /// Chạy Python‐exe (main.exe) mà không chờ nó kết thúc.
        /// </summary>
        public static Process RunSolver(string args = "")
        {
            // 1) Extract main.exe
            string pythonExePath = ExtractPythonExe();

            // 2) Cấu hình ProcessStartInfo
            var psi = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = false,    // true nếu muốn ẩn console Python, false để hiển thị
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            // 3) Start process và trả về Process object
            Process proc = Process.Start(psi);
            return proc;
        }

    }
}
