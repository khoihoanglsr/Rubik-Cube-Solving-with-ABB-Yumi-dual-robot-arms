using System;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.Configuration;
using ABB.Robotics.Controllers.MotionDomain;
using System.Globalization;
using System.Text.Json;
using RubikSolver1;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;           // for Process  
using System.Net.Sockets;           // for TcpClient
using System.IO;                    // for StreamReader
using System.Linq;
using System.Collections.Generic;
using System.Threading;             // for Thread.Sleep
using DotNetTask = System.Threading.Tasks.Task;
using BgTask = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.WinForms;




namespace RubikSolver
{
    public partial class Form1 : Form
    {
        NetworkScanner? Netscaner;      //this variable is used to scan the network to look for controllers
        NetworkWatcher networkwatcher = null;  //this variable is used to watch over the network to discover new controller
        public ControllerInfoCollection? Controllers;  //Info about the scanned controllers will be saved here
        public Controller? SelectedController;    //this variable is used to connect with the chosen controller

        ABB.Robotics.Controllers.RapidDomain.Task currentSelectedTask;   //this variable is used to choose a task in Rapid
        ABB.Robotics.Controllers.RapidDomain.Task[] allTasks;
        private ABB.Robotics.Controllers.RapidDomain.Task currentTask1;
        private ABB.Robotics.Controllers.RapidDomain.Task currentTask2;

        private System.Timers.Timer dataMonitoringTimer;
        // Chống timer chồng lặp khi tick liên tiếp
        private int _isPolling = 0; // 0=rảnh, 1=đang polling

        // —— Python-socket vars ——
        private Process pythonProcess;
        private TcpClient pythonClient;
        private bool caseListenerStarted = false;
        private int startPythonListenerCallCount = 0;
        private readonly object _dbgLock = new object();// Tạo kết nối TCP với Python
        private NetworkStream pythonStream; // Tạo stream để giao tiếp với Python qua kết nối TCP
        private int capCount = 0;           // 0?6 CAP messages sent
        private bool capPhase = true;       // true until 6×CAP are sent
        private bool solutionPrinted = false;

        private string lastcapValue;

        private string lastend1;
        private string lastend2;
        private string lastfn1;
        private string lastfn2;
        private string lastcs1;
        private string lastcs2;

        private bool f2 = false;
        private bool lastSend1 = false;
        private bool lastSend2 = false;
        private bool lastABC1 = false;
        private bool lastABC2 = false;
        private bool abcHandlerCalled = false;
        private bool sendHandlerCalled = false;
        private DisplayForm displayForm;
        private Dictionary<string, string[,]> cubeState;

        // —— Experiment logging vars ——
        private string currentCaseId = "";
        private int caseIndexCounter = 0;
        private DateTime caseStartTime;
        private DateTime caseEndTime;
        private DateTime robotStartTime;
        private DateTime robotEndTime;
        private int handExchangeCount = 0;
        private bool robotExecutionSuccess = false;
        private bool endToEndSuccess = false; // Người vận hành có thể xác nhận sau, mặc định false nếu chưa có xác nhận.
        private string operatorNote = "";

        public Form1()
        {
            InitializeComponent();
            //this.Load += Form1_Load;
            this.FormClosed += (s, e) => { try { wmpBackground.Ctlcontrols.stop(); } catch { } };
            NetScan();   //scan the controller right after and save into the comboBox
            this.networkwatcher = new NetworkWatcher(Netscaner.Controllers);
            this.networkwatcher.Found += new EventHandler<NetworkWatcherEventArgs>(HandleFoundEvent);
            this.networkwatcher.Lost += new EventHandler<NetworkWatcherEventArgs>(HandleLostEvent);
            this.networkwatcher.EnableRaisingEvents = true;

            btnReset.Click += btnReset_Click;

            dataMonitoringTimer = new System.Timers.Timer(100); // 100ms
            dataMonitoringTimer.Elapsed += DataMonitoringTimer_Elapsed;
            dataMonitoringTimer.AutoReset = true;
            dataMonitoringTimer.Stop();
            rtbLog.AppendText("[C#] Timer Ready. Timer will start after Python is connected." + Environment.NewLine);



        }

        private Dictionary<string, string[,]> LoadState(string path)
        {
            var faces = new[] { "U", "R", "F", "D", "L", "B" };
            var dict = faces.ToDictionary(f => f, f => new string[3, 3]);
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < 6; i++)
            {
                var tok = lines[i].Split(' ');
                for (int j = 0; j < 9; j++)
                    dict[faces[i]][j / 3, j % 3] = tok[j];
            }
            return dict;
        }


        private void Dbg(string tag, string msg)
        {
            try
            {
                string line = $"[{DateTime.Now:HH:mm:ss.fff}][T{System.Threading.Thread.CurrentThread.ManagedThreadId}][{tag}] {msg}";

                lock (_dbgLock)
                {
                    string logPath = Path.Combine(Application.StartupPath, "csharp_debug.log");
                    File.AppendAllText(logPath, line + Environment.NewLine, Encoding.UTF8);
                }

                if (this.IsDisposed) return;

                if (this.InvokeRequired)
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        if (!this.IsDisposed && rtbLog != null && !rtbLog.IsDisposed)
                            rtbLog.AppendText(line + Environment.NewLine);
                    }));
                }
                else
                {
                    if (rtbLog != null && !rtbLog.IsDisposed)
                        rtbLog.AppendText(line + Environment.NewLine);
                }
            }
            catch
            {
                // Không để debug làm crash chương trình chính.
            }

        }

        private string NowIso()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private string CsvEscape(object value)
        {
            string s = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            if (s.Contains(",") || s.Contains("\"") || s.Contains(";"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private void EnsureLogDir()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "logs"));
            }
            catch (Exception ex)
            {
                Dbg("CSV", "EnsureLogDir failed: " + ex.Message);
            }
        }

        private void AppendCsvRow(string fileName, string[] headers, Dictionary<string, object> row)
        {
            try
            {
                EnsureLogDir();
                string path = Path.Combine(Application.StartupPath, "logs", fileName);
                bool newFile = !File.Exists(path) || new FileInfo(path).Length == 0;

                // File mới dùng UTF-8 BOM để Excel đọc tiếng Việt đúng hơn. File cũ append UTF-8 không BOM.
                var enc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: newFile);
                using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs, enc))
                {
                    if (newFile)
                        sw.WriteLine(string.Join(",", headers.Select(CsvEscape)));

                    sw.WriteLine(string.Join(",", headers.Select(h => CsvEscape(row.ContainsKey(h) ? row[h] : ""))));
                    sw.Flush();
                    fs.Flush(true);
                }
            }
            catch (Exception ex)
            {
                Dbg("CSV", $"AppendCsvRow failed file={fileName}: {ex.Message}");
            }
        }

        private string ReadCurrentCaseIdFromPython()
        {
            string path = Path.Combine(Application.StartupPath, "current_case_id.txt");

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string id = File.ReadAllText(path, Encoding.UTF8).Trim();
                        if (!string.IsNullOrWhiteSpace(id))
                            return id;
                    }
                }
                catch { }

                Thread.Sleep(100);
            }

            // Fallback chỉ để không crash. Khi chạy chuẩn, Python phải tạo file này trước.
            int safeIndex = caseIndexCounter > 0 ? caseIndexCounter : 1;
            return $"CASE_{DateTime.Now:yyyyMMdd_HHmmss}_{safeIndex:000}";
        }


        private int ParseCaseIndexFromCaseId(string caseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caseId)) return 0;
                string[] parts = caseId.Trim().Split('_');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int idx))
                    return idx;
            }
            catch { }
            return 0;
        }

        private List<string> ReadSolutionMovesFromFile(string solutionPath, out bool hasEndMarker)
        {
            var validMoves = new HashSet<string>
            {
                "U", "U'", "U2", "D", "D'", "D2", "L", "L'", "L2",
                "R", "R'", "R2", "F", "F'", "F2", "B", "B'", "B2"
            };

            var rawLines = File.ReadAllLines(solutionPath, Encoding.UTF8)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            // E là marker kết thúc trong solution.txt. Không log E thành robot step,
            // nhưng phải nhớ nó để gửi next="E" ở move cuối và gửi final sol="E" xuống RAPID.
            hasEndMarker = rawLines.Any(l => l == "E");

            var moves = rawLines
                .Where(l => l != "E")
                .ToList();

            var invalid = moves.Where(m => !validMoves.Contains(m)).ToList();
            if (invalid.Count > 0)
                throw new InvalidDataException("solution.txt có move không hợp lệ: " + string.Join(" ", invalid));

            return moves;
        }

        private sealed class RobotMovePlan
        {
            public int StepIndex { get; set; }
            public string MoveSymbol { get; set; } = "";
            public string Face { get; set; } = "";
            public string MoveType { get; set; } = "";
            public string RequiredHoldingTask { get; set; } = "";
            public string RequiredRotatingTask { get; set; } = "";
            public bool HandExchangeRequired { get; set; }
            public string ExchangeFrom { get; set; } = "";
            public string ExchangeTo { get; set; } = "";
        }

        private string GetMoveType(string move)
        {
            if (move.EndsWith("2")) return "180";
            if (move.EndsWith("'")) return "CCW";
            return "CW";
        }

        private (string holding, string rotating) GetTaskMappingForMove(string face)
        {
            // Mapping tạm theo rule thí nghiệm/bài báo.
            // Nếu RAPID/C# hiện tại có mapping thật khác, thay duy nhất hàm này.
            if (face == "B" || face == "F" || face == "R")
                return ("T_ROB_R", "T_ROB_L");

            if (face == "U" || face == "D" || face == "L")
                return ("T_ROB_L", "T_ROB_R");

            return ("UNKNOWN", "UNKNOWN");
        }

        private List<RobotMovePlan> AnalyzeSolutionMoves(List<string> moves)
        {
            var result = new List<RobotMovePlan>();
            string currentHoldingTask = "";
            handExchangeCount = 0;

            for (int i = 0; i < moves.Count; i++)
            {
                string mv = moves[i].Trim();
                string face = mv.Length > 0 ? mv.Substring(0, 1) : "";
                var mapping = GetTaskMappingForMove(face);

                bool exchange = false;
                string exchangeFrom = "";
                string exchangeTo = "";

                if (!string.IsNullOrWhiteSpace(currentHoldingTask) && currentHoldingTask != mapping.holding)
                {
                    exchange = true;
                    exchangeFrom = currentHoldingTask;
                    exchangeTo = mapping.holding;
                    handExchangeCount++;
                }

                currentHoldingTask = mapping.holding;

                result.Add(new RobotMovePlan
                {
                    StepIndex = i + 1,
                    MoveSymbol = mv,
                    Face = face,
                    MoveType = GetMoveType(mv),
                    RequiredHoldingTask = mapping.holding,
                    RequiredRotatingTask = mapping.rotating,
                    HandExchangeRequired = exchange,
                    ExchangeFrom = exchangeFrom,
                    ExchangeTo = exchangeTo
                });
            }

            Dbg("ANALYZE", $"moves={moves.Count}, handExchangeCount={handExchangeCount}");
            return result;
        }

        private void LogRobotStep(RobotMovePlan plan, DateTime commandSendTime, DateTime commandDoneTime,
                                  bool robotAckReceived, string robotError = "", string robotErrorMessage = "",
                                  bool mechanicalFailureFlag = false, string stepOperatorNote = "")
        {
            string[] headers = {
                "case_id", "step_index", "move_symbol", "face", "move_type",
                "required_holding_task", "required_rotating_task", "hand_exchange_required",
                "exchange_from", "exchange_to", "command_send_time", "command_done_time",
                "step_duration_ms", "robot_ack_received", "robot_error", "robot_error_message",
                "mechanical_failure_flag", "operator_note"
            };

            var row = new Dictionary<string, object>
            {
                ["case_id"] = currentCaseId,
                ["step_index"] = plan.StepIndex,
                ["move_symbol"] = plan.MoveSymbol,
                ["face"] = plan.Face,
                ["move_type"] = plan.MoveType,
                ["required_holding_task"] = plan.RequiredHoldingTask,
                ["required_rotating_task"] = plan.RequiredRotatingTask,
                ["hand_exchange_required"] = plan.HandExchangeRequired,
                ["exchange_from"] = plan.ExchangeFrom,
                ["exchange_to"] = plan.ExchangeTo,
                ["command_send_time"] = commandSendTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["command_done_time"] = commandDoneTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["step_duration_ms"] = (long)(commandDoneTime - commandSendTime).TotalMilliseconds,
                ["robot_ack_received"] = robotAckReceived,
                ["robot_error"] = robotError,
                ["robot_error_message"] = robotErrorMessage,
                ["mechanical_failure_flag"] = mechanicalFailureFlag,
                ["operator_note"] = stepOperatorNote
            };

            AppendCsvRow("robot_steps.csv", headers, row);
        }

        private void LogFailureEvent(string stage, int stepIndex, string moveSymbol, string failureType,
                                     string failureDescription, string recoveryAction = "", string note = "")
        {
            string[] headers = {
                "case_id", "timestamp", "stage", "step_index", "move_symbol",
                "failure_type", "failure_description", "recovery_action", "operator_note"
            };

            var row = new Dictionary<string, object>
            {
                ["case_id"] = currentCaseId,
                ["timestamp"] = NowIso(),
                ["stage"] = stage,
                ["step_index"] = stepIndex,
                ["move_symbol"] = moveSymbol,
                ["failure_type"] = failureType,
                ["failure_description"] = failureDescription,
                ["recovery_action"] = recoveryAction,
                ["operator_note"] = note
            };

            AppendCsvRow("failure_events.csv", headers, row);
        }

        private void LogCaseSummary(List<string> moves, string solutionText, bool recognitionSuccess,
                                    bool solverSuccess, bool robotSuccess, string failureReason = "")
        {
            caseEndTime = DateTime.Now;
            endToEndSuccess = recognitionSuccess && solverSuccess && robotSuccess;

            string[] headers = {
                "case_id", "case_index", "start_time", "end_time", "total_duration_ms",
                "recognition_start_time", "recognition_end_time", "robot_start_time", "robot_end_time",
                "capture_face_count", "solution_move_count", "solution_text", "hand_exchange_count",
                "recognition_success", "solver_success", "robot_execution_success", "end_to_end_success",
                "full_cube_recognition_success", "total_wrong_cells", "wrong_cells_face_1", "wrong_cells_face_2",
                "wrong_cells_face_3", "wrong_cells_face_4", "wrong_cells_face_5", "wrong_cells_face_6",
                "total_unknown_cells", "reacquisition_total", "face_rotation_failure_count",
                "handover_failure_count", "mechanical_failure_count", "failure_reason", "operator_note"
            };

            var row = new Dictionary<string, object>
            {
                ["case_id"] = currentCaseId,
                ["case_index"] = caseIndexCounter,
                ["start_time"] = caseStartTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["end_time"] = caseEndTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["total_duration_ms"] = (long)(caseEndTime - caseStartTime).TotalMilliseconds,
                ["recognition_start_time"] = "",   // Python ghi chi tiết ở logs/solutions.csv; ghép bằng case_id.
                ["recognition_end_time"] = "",
                ["robot_start_time"] = robotStartTime == default ? "" : robotStartTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["robot_end_time"] = robotEndTime == default ? "" : robotEndTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                ["capture_face_count"] = 6,
                ["solution_move_count"] = moves.Count,
                ["solution_text"] = solutionText,
                ["hand_exchange_count"] = handExchangeCount,
                ["recognition_success"] = recognitionSuccess,
                ["solver_success"] = solverSuccess,
                ["robot_execution_success"] = robotSuccess,
                ["end_to_end_success"] = endToEndSuccess,
                ["full_cube_recognition_success"] = "", // Chỉ tính khi có ground truth.
                ["total_wrong_cells"] = "",
                ["wrong_cells_face_1"] = "",
                ["wrong_cells_face_2"] = "",
                ["wrong_cells_face_3"] = "",
                ["wrong_cells_face_4"] = "",
                ["wrong_cells_face_5"] = "",
                ["wrong_cells_face_6"] = "",
                ["total_unknown_cells"] = "",       // Python ghi ở logs/solutions.csv.
                ["reacquisition_total"] = "",       // Python ghi ở logs/solutions.csv.
                ["face_rotation_failure_count"] = 0,
                ["handover_failure_count"] = 0,
                ["mechanical_failure_count"] = 0,
                ["failure_reason"] = failureReason,
                ["operator_note"] = operatorNote
            };

            AppendCsvRow("case_summary.csv", headers, row);
        }


        private void ApplyMove(string mv)
        {
            switch (mv)
            {
                case "U": RotateU(); break;
                case "U'": RotateUPrime(); break;
                case "U2": RotateU(); RotateU(); break;

                case "R": RotateR(); break;
                case "R'": RotateRPrime(); break;
                case "R2": RotateR(); RotateR(); break;

                case "F": RotateF(); break;
                case "F'": RotateFPrime(); break;
                case "F2": RotateF(); RotateF(); break;

                case "D": RotateD(); break;
                case "D'": RotateDPrime(); break;
                case "D2": RotateD(); RotateD(); break;

                case "L": RotateL(); break;
                case "L'": RotateLPrime(); break;
                case "L2": RotateL(); RotateL(); break;

                case "B": RotateB(); break;
                case "B'": RotateBPrime(); break;
                case "B2": RotateB(); RotateB(); break;
            }
        }

        private void RotateU()
        {
            // 1) Xoay mặt U
            cubeState["U"] = RotateFaceClockwise(cubeState["U"]);

            // 2) Lưu hàng 0 của F, L, B, R
            string f1 = cubeState["F"][0, 0], f2 = cubeState["F"][0, 1], f3 = cubeState["F"][0, 2];
            string l1 = cubeState["L"][0, 0], l2 = cubeState["L"][0, 1], l3 = cubeState["L"][0, 2];
            string b1 = cubeState["B"][0, 0], b2 = cubeState["B"][0, 1], b3 = cubeState["B"][0, 2];
            string r1 = cubeState["R"][0, 0], r2 = cubeState["R"][0, 1], r3 = cubeState["R"][0, 2];

            // 3) F → L → B → R → F
            cubeState["L"][0, 0] = f1; cubeState["L"][0, 1] = f2; cubeState["L"][0, 2] = f3;
            cubeState["B"][0, 0] = l1; cubeState["B"][0, 1] = l2; cubeState["B"][0, 2] = l3;
            cubeState["R"][0, 0] = b1; cubeState["R"][0, 1] = b2; cubeState["R"][0, 2] = b3;
            cubeState["F"][0, 0] = r1; cubeState["F"][0, 1] = r2; cubeState["F"][0, 2] = r3;
        }

        // ——— U′ (xoay mặt U ngược chiều kim đồng hồ) ———
        private void RotateUPrime()
        {
            // 1) Xoay mặt U
            cubeState["U"] = RotateFaceCounterClockwise(cubeState["U"]);

            // 2) Lưu hàng 0 của F, L, B, R
            string f1 = cubeState["F"][0, 0], f2 = cubeState["F"][0, 1], f3 = cubeState["F"][0, 2];
            string l1 = cubeState["L"][0, 0], l2 = cubeState["L"][0, 1], l3 = cubeState["L"][0, 2];
            string b1 = cubeState["B"][0, 0], b2 = cubeState["B"][0, 1], b3 = cubeState["B"][0, 2];
            string r1 = cubeState["R"][0, 0], r2 = cubeState["R"][0, 1], r3 = cubeState["R"][0, 2];

            // 3) Ngược chu trình F ← R ← B ← L ← F
            cubeState["L"][0, 0] = b1; cubeState["L"][0, 1] = b2; cubeState["L"][0, 2] = b3;
            cubeState["B"][0, 0] = r1; cubeState["B"][0, 1] = r2; cubeState["B"][0, 2] = r3;
            cubeState["R"][0, 0] = f1; cubeState["R"][0, 1] = f2; cubeState["R"][0, 2] = f3;
            cubeState["F"][0, 0] = l1; cubeState["F"][0, 1] = l2; cubeState["F"][0, 2] = l3;
        }

        // ——— R (xoay mặt R theo chiều kim đồng hồ) ———
        private void RotateR()
        {
            // 1) Xoay mặt R
            cubeState["R"] = RotateFaceClockwise(cubeState["R"]);

            // 2) Lưu các ô biên
            string f9 = cubeState["F"][2, 2], f6 = cubeState["F"][1, 2], f3 = cubeState["F"][0, 2];
            string u3 = cubeState["U"][2, 2], u6 = cubeState["U"][1, 2], u9 = cubeState["U"][0, 2];
            string b7 = cubeState["B"][2, 0], b4 = cubeState["B"][1, 0], b1 = cubeState["B"][0, 0];
            string d3 = cubeState["D"][2, 2], d6 = cubeState["D"][1, 2], d9 = cubeState["D"][0, 2];

            // 3) F → U → B → D → F
            cubeState["D"][0, 2] = b7; cubeState["D"][1, 2] = b4; cubeState["D"][2, 2] = b1;
            cubeState["F"][0, 2] = d9; cubeState["F"][1, 2] = d6; cubeState["F"][2, 2] = d3;
            cubeState["U"][0, 2] = f3; cubeState["U"][1, 2] = f6; cubeState["U"][2, 2] = f9;
            cubeState["B"][0, 0] = u3; cubeState["B"][1, 0] = u6; cubeState["B"][2, 0] = u9;
        }

        // ——— R′ (xoay mặt R ngược chiều kim đồng hồ) ———
        private void RotateRPrime()
        {
            // 1) Xoay mặt R
            cubeState["R"] = RotateFaceCounterClockwise(cubeState["R"]);

            // 2) Lưu các ô biên
            string f3 = cubeState["F"][0, 2], f6 = cubeState["F"][1, 2], f9 = cubeState["F"][2, 2];
            string u9 = cubeState["U"][0, 2], u6 = cubeState["U"][1, 2], u3 = cubeState["U"][2, 2];
            string b1 = cubeState["B"][0, 0], b4 = cubeState["B"][1, 0], b7 = cubeState["B"][2, 0];
            string d9 = cubeState["D"][0, 2], d6 = cubeState["D"][1, 2], d3 = cubeState["D"][2, 2];

            // 3) Ngược chu trình F ← D ← B ← U ← F
            cubeState["D"][0, 2] = f3; cubeState["D"][1, 2] = f6; cubeState["D"][2, 2] = f9;
            cubeState["F"][0, 2] = u9; cubeState["F"][1, 2] = u6; cubeState["F"][2, 2] = u3;
            cubeState["U"][0, 2] = b7; cubeState["U"][1, 2] = b4; cubeState["U"][2, 2] = b1;
            cubeState["B"][0, 0] = d3; cubeState["B"][1, 0] = d6; cubeState["B"][2, 0] = d9;
        }

        // ——— F (xoay mặt F theo chiều kim đồng hồ) ———
        private void RotateF()
        {
            // 1) Xoay mặt F
            cubeState["F"] = RotateFaceClockwise(cubeState["F"]);

            // 2) Lưu cạnh
            string u7 = cubeState["U"][2, 2], u8 = cubeState["U"][2, 1], u9 = cubeState["U"][2, 0];
            string r1 = cubeState["R"][0, 0], r4 = cubeState["R"][1, 0], r7 = cubeState["R"][2, 0];
            string d1 = cubeState["D"][0, 2], d2 = cubeState["D"][0, 1], d3 = cubeState["D"][0, 0];
            string l3 = cubeState["L"][0, 2], l6 = cubeState["L"][1, 2], l9 = cubeState["L"][2, 2];

            // 3) Gán theo chu trình U → R → D → L → U
            cubeState["L"][0, 2] = d3; cubeState["L"][1, 2] = d2; cubeState["L"][2, 2] = d1;
            cubeState["D"][0, 2] = r1; cubeState["D"][0, 1] = r4; cubeState["D"][0, 0] = r7;
            cubeState["R"][0, 0] = u9; cubeState["R"][1, 0] = u8; cubeState["R"][2, 0] = u7;
            cubeState["U"][2, 0] = l9; cubeState["U"][2, 1] = l6; cubeState["U"][2, 2] = l3;
        }

        // ——— F′ (xoay mặt F ngược chiều kim đồng hồ) ———
        private void RotateFPrime()
        {
            // 1) Xoay mặt F
            cubeState["F"] = RotateFaceCounterClockwise(cubeState["F"]);

            // 2) Lưu cạnh
            string u7 = cubeState["U"][2, 2], u8 = cubeState["U"][2, 1], u9 = cubeState["U"][2, 0];
            string r1 = cubeState["R"][0, 0], r4 = cubeState["R"][1, 0], r7 = cubeState["R"][2, 0];
            string d1 = cubeState["D"][0, 2], d2 = cubeState["D"][0, 1], d3 = cubeState["D"][0, 0];
            string l3 = cubeState["L"][0, 2], l6 = cubeState["L"][1, 2], l9 = cubeState["L"][2, 2];

            // 3) Ngược chu trình F ← U ← L ← D ← R ← F
            cubeState["U"][2, 0] = r1; cubeState["U"][2, 1] = r4; cubeState["U"][2, 2] = r7;
            cubeState["R"][0, 0] = d1; cubeState["R"][1, 0] = d2; cubeState["R"][2, 0] = d3;
            cubeState["D"][0, 2] = l9; cubeState["D"][0, 1] = l6; cubeState["D"][0, 0] = l3;
            cubeState["L"][0, 2] = u7; cubeState["L"][1, 2] = u8; cubeState["L"][2, 2] = u9;
        }

        // ——— D (xoay mặt D theo chiều kim đồng hồ) ———
        private void RotateD()
        {
            // 1) Xoay mặt D
            cubeState["D"] = RotateFaceClockwise(cubeState["D"]);

            // 2) Lấy hàng 2 của F, R, B, L
            string f7 = cubeState["F"][2, 0], f8 = cubeState["F"][2, 1], f9 = cubeState["F"][2, 2];
            string l7 = cubeState["L"][2, 0], l8 = cubeState["L"][2, 1], l9 = cubeState["L"][2, 2];
            string b7 = cubeState["B"][2, 0], b8 = cubeState["B"][2, 1], b9 = cubeState["B"][2, 2];
            string r7 = cubeState["R"][2, 0], r8 = cubeState["R"][2, 1], r9 = cubeState["R"][2, 2];

            // 3) F → L → B → R → F
            cubeState["L"][2, 0] = b7; cubeState["L"][2, 1] = b8; cubeState["L"][2, 2] = b9;
            cubeState["B"][2, 0] = r7; cubeState["B"][2, 1] = r8; cubeState["B"][2, 2] = r9;
            cubeState["R"][2, 0] = f7; cubeState["R"][2, 1] = f8; cubeState["R"][2, 2] = f9;
            cubeState["F"][2, 0] = l7; cubeState["F"][2, 1] = l8; cubeState["F"][2, 2] = l9;
        }

        // ——— D′ (xoay mặt D ngược chiều kim đồng hồ) ———
        private void RotateDPrime()
        {
            // 1) Xoay mặt D
            cubeState["D"] = RotateFaceCounterClockwise(cubeState["D"]);

            // 2) Lấy hàng 2 của F, L, B, R
            string f7 = cubeState["F"][2, 0], f8 = cubeState["F"][2, 1], f9 = cubeState["F"][2, 2];
            string l7 = cubeState["L"][2, 0], l8 = cubeState["L"][2, 1], l9 = cubeState["L"][2, 2];
            string b7 = cubeState["B"][2, 0], b8 = cubeState["B"][2, 1], b9 = cubeState["B"][2, 2];
            string r7 = cubeState["R"][2, 0], r8 = cubeState["R"][2, 1], r9 = cubeState["R"][2, 2];

            // 3) F → L → B → R → F (ngược so với RotateD())
            cubeState["L"][2, 0] = f7; cubeState["L"][2, 1] = f8; cubeState["L"][2, 2] = f9;
            cubeState["B"][2, 0] = l7; cubeState["B"][2, 1] = l8; cubeState["B"][2, 2] = l9;
            cubeState["R"][2, 0] = b7; cubeState["R"][2, 1] = b8; cubeState["R"][2, 2] = b9;
            cubeState["F"][2, 0] = r7; cubeState["F"][2, 1] = r8; cubeState["F"][2, 2] = r9;
        }

        // ——— L (xoay mặt L theo chiều kim đồng hồ) ———
        private void RotateL()
        {
            // 1) Xoay mặt L
            cubeState["L"] = RotateFaceClockwise(cubeState["L"]);

            // 2) Lấy cột 0 của F, U, B, D
            string f1 = cubeState["F"][0, 0], f4 = cubeState["F"][1, 0], f7 = cubeState["F"][2, 0];
            string u1 = cubeState["U"][2, 0], u4 = cubeState["U"][1, 0], u7 = cubeState["U"][0, 0];
            string b3 = cubeState["B"][0, 2], b6 = cubeState["B"][1, 2], b9 = cubeState["B"][2, 2];
            string d1 = cubeState["D"][2, 0], d4 = cubeState["D"][1, 0], d7 = cubeState["D"][0, 0];

            // 3) F → U → B → D → F
            cubeState["U"][0, 0] = b9; cubeState["U"][1, 0] = b6; cubeState["U"][2, 0] = b3;
            cubeState["B"][0, 2] = d1; cubeState["B"][1, 2] = d4; cubeState["B"][2, 2] = d7;
            cubeState["D"][0, 0] = f1; cubeState["D"][1, 0] = f4; cubeState["D"][2, 0] = f7;
            cubeState["F"][0, 0] = u7; cubeState["F"][1, 0] = u4; cubeState["F"][2, 0] = u1;
        }

        // ——— L′ (xoay mặt L ngược chiều kim đồng hồ) ———
        private void RotateLPrime()
        {
            // 1) Xoay mặt L
            cubeState["L"] = RotateFaceCounterClockwise(cubeState["L"]);

            // 2) Lưu cột 0 của F, U, B, D
            string f1 = cubeState["F"][0, 0], f4 = cubeState["F"][1, 0], f7 = cubeState["F"][2, 0];
            string u1 = cubeState["U"][2, 0], u4 = cubeState["U"][1, 0], u7 = cubeState["U"][0, 0];
            string b3 = cubeState["B"][0, 2], b6 = cubeState["B"][1, 2], b9 = cubeState["B"][2, 2];
            string d1 = cubeState["D"][2, 0], d4 = cubeState["D"][1, 0], d7 = cubeState["D"][0, 0];

            // 3) Gán theo chu trình F → U → B → D → F
            cubeState["U"][0, 0] = f1; cubeState["U"][1, 0] = f4; cubeState["U"][2, 0] = f7;
            cubeState["B"][0, 2] = u1; cubeState["B"][1, 2] = u4; cubeState["B"][2, 2] = u7;
            cubeState["D"][0, 0] = b9; cubeState["D"][1, 0] = b6; cubeState["D"][2, 0] = b3;
            cubeState["F"][0, 0] = d7; cubeState["F"][1, 0] = d4; cubeState["F"][2, 0] = d1;
        }

        // ——— B (xoay mặt B theo chiều kim đồng hồ) ———
        private void RotateB()
        {
            // 1) Xoay mặt B
            cubeState["B"] = RotateFaceClockwise(cubeState["B"]);

            // 2) Lưu các ô biên
            string u3 = cubeState["U"][0, 0], u2 = cubeState["U"][0, 1], u1 = cubeState["U"][0, 2];
            string r9 = cubeState["R"][0, 2], r6 = cubeState["R"][1, 2], r3 = cubeState["R"][2, 2];
            string d9 = cubeState["D"][2, 0], d8 = cubeState["D"][2, 1], d7 = cubeState["D"][2, 2];
            string l7 = cubeState["L"][0, 0], l4 = cubeState["L"][1, 0], l1 = cubeState["L"][2, 0];

            // 3) U → R → D → L → U
            cubeState["R"][0, 2] = d7; cubeState["R"][1, 2] = d8; cubeState["R"][2, 2] = d9;
            cubeState["D"][2, 0] = l7; cubeState["D"][2, 1] = l4; cubeState["D"][2, 2] = l1;
            cubeState["L"][0, 0] = u1; cubeState["L"][1, 0] = u2; cubeState["L"][2, 0] = u3;
            cubeState["U"][0, 0] = r9; cubeState["U"][0, 1] = r6; cubeState["U"][0, 2] = r3;
        }

        // ——— B′ (xoay mặt B ngược chiều kim đồng hồ) ———
        private void RotateBPrime()
        {
            // 1) Xoay mặt B
            cubeState["B"] = RotateFaceCounterClockwise(cubeState["B"]);

            // 2) Lưu các ô biên
            string u3 = cubeState["U"][0, 0], u2 = cubeState["U"][0, 1], u1 = cubeState["U"][0, 2];
            string r9 = cubeState["R"][0, 2], r6 = cubeState["R"][1, 2], r3 = cubeState["R"][2, 2];
            string d9 = cubeState["D"][2, 0], d8 = cubeState["D"][2, 1], d7 = cubeState["D"][2, 2];
            string l7 = cubeState["L"][0, 0], l4 = cubeState["L"][1, 0], l1 = cubeState["L"][2, 0];

            // 3) Ngược chu trình U ← R ← D ← L ← U
            cubeState["R"][0, 2] = u3; cubeState["R"][1, 2] = u2; cubeState["R"][2, 2] = u1;
            cubeState["D"][2, 0] = r3; cubeState["D"][2, 1] = r6; cubeState["D"][2, 2] = r9;
            cubeState["L"][0, 0] = d9; cubeState["L"][1, 0] = d8; cubeState["L"][2, 0] = d7;
            cubeState["U"][0, 0] = l1; cubeState["U"][0, 1] = l4; cubeState["U"][0, 2] = l7;
        }

        // Xoay 3×3 face theo chiều kim đồng hồ.
        private string[,] RotateFaceClockwise(string[,] face)
        {
            var N = 3;
            var result = new string[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    // result[i,j] = face[2-j, i]
                    result[i, j] = face[N - j - 1, i];
            return result;
        }
        // Xoay 3×3 face ngược chiều kim đồng hồ.
        private string[,] RotateFaceCounterClockwise(string[,] face)
        {
            var N = 3;
            var result = new string[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    // result[i,j] = face[j, 2-i]
                    result[i, j] = face[j, N - i - 1];
            return result;
        }

        private void DataMonitoringTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (SelectedController == null || !SelectedController.Connected) return;

            if (System.Threading.Interlocked.Exchange(ref _isPolling, 1) == 1)
                return;

            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    string module = "Module1";
                    string capmodule = "Check_Face_DUL_L";

                    // --- Task 1 status ---
                    try
                    {
                        var data1 = currentTask1.GetRapidData(module, "recvMsg");
                        labelTask1.Text = $"Task 1: {data1.StringValue}";
                    }
                    catch
                    {
                        labelTask1.Text = "[Not Readable]";
                    }

                    // --- Task 2 status ---
                    try
                    {
                        var data2 = currentTask2.GetRapidData(module, "recvMsg");
                        labelTask2.Text = $"Task 2: {data2.StringValue}";
                    }
                    catch
                    {
                        labelTask2.Text = "[Not Readable]";
                    }

                    // --- Poll biến cap từ RAPID ---
                    try
                    {
                        if (currentTask2 != null)
                        {
                            var capData = currentTask2.GetRapidData(capmodule, "cap");
                            string capValue = capData.StringValue ?? string.Empty;

                            // Nếu vì lý do nào đó chưa prime được lastcapValue, chỉ khởi tạo rồi bỏ qua tick đầu.
                            // Không được gửi CAP ở tick đầu vì rất dễ chụp lúc camera chưa thấy Rubik.
                            if (lastcapValue == null)
                            {
                                lastcapValue = capValue;
                                Dbg("RAPID-CAP", $"First poll only primes cap='{capValue}'. NO CAP sent.");
                                return;
                            }

                            if (capValue != lastcapValue)
                            {
                                string oldCapValue = lastcapValue;
                                lastcapValue = capValue;

                                Dbg("RAPID-CAP", $"cap changed: old='{oldCapValue}', new='{capValue}', capPhase={capPhase}, capCount={capCount}, pythonStreamNull={pythonStream == null}");

                                // Không gửi CAP nếu chưa connect Python hoặc đã hết pha CAP.
                                if (capPhase && pythonStream != null && capCount < 6)
                                {
                                    SendCapToPython();
                                }
                                else
                                {
                                    Dbg("RAPID-CAP", "Không gửi CAP vì capPhase=false, capCount>=6 hoặc pythonStream=null.");
                                }
                            }
                        }

                        // ABC chỉ để log robot ready. KHÔNG gọi StartPythonListener ở đây.
                        if (!abcHandlerCalled && currentTask1 != null && currentTask2 != null)
                        {
                            bool ABC1 = ((ABB.Robotics.Controllers.RapidDomain.Bool)
                                          currentTask1.GetRapidData(module, "ABC").Value).Value;
                            bool ABC2 = ((ABB.Robotics.Controllers.RapidDomain.Bool)
                                          currentTask2.GetRapidData(module, "ABC").Value).Value;

                            if (ABC1 && ABC2)
                            {
                                abcHandlerCalled = true;
                                Dbg("ABC", $"ABC1 && ABC2 true. KHÔNG gọi StartPythonListener ở đây. capCount={capCount}");
                                rtbLog.AppendText("[Both ABC==true] Python ready, waiting for 6 CAP..." + Environment.NewLine);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Dbg("TIMER", "Error cap polling: " + ex.ToString());
                    }
                }));
            }
            catch (Exception ex)
            {
                Dbg("TIMER", "DataMonitoringTimer_Elapsed exception: " + ex.ToString());
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _isPolling, 0);
            }
        }


        private void NetScan()    //this function is used to scan all the existing controllers in the same network
        {
            Netscaner = new NetworkScanner();   //initialize the variable
            Netscaner.Scan();   // start to scan
            Controllers = Netscaner.Controllers;   //store the scanned controllers into this variable

            foreach (ControllerInfo info in Controllers)
            {
                comboBoxControllers.Items.Add(info);  //load info into the comboBox
            }
        }

        void HandleFoundEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new
            EventHandler<NetworkWatcherEventArgs>(AddControllerToListView),
            new System.Object[] { this, e });
        }

        void HandleLostEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new
            EventHandler<NetworkWatcherEventArgs>(RemoveControllerFromListView),
            new System.Object[] { this, e });
        }

        private void AddControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerInfo controllerInfo = e.Controller;
            this.comboBoxControllers.Items.Add(controllerInfo);
        }

        private void RemoveControllerFromListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerInfo controllerInfo = e.Controller;
            this.comboBoxControllers.Items.Remove(controllerInfo);
        }

        private void comboBoxControllers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxControllers.SelectedItem == null) return;

            ControllerInfo selectedInfo = (ControllerInfo)comboBoxControllers.SelectedItem;   //sotr the info of the selected controller from the box

            if (selectedInfo is null) return;


            try
            {
                SelectedController = Controller.Connect(selectedInfo, ConnectionType.Standalone);  //Connect() is a basic function of PC SDK to connect to a specific controller
                SelectedController.Logon(UserInfo.DefaultUser);   //choose the user as default

                labelStatus.Text = $"Connected to {SelectedController.SystemName}";  //notify when connected
                LoadTasksIntoComboBox();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to controller: " + ex.Message);   //notify when having error

            }
        }

        private void LoadTasksIntoComboBox()
        {
            comboBoxSelectedTask.Items.Clear();

            if (SelectedController != null && SelectedController.Connected)
            {
                try
                {
                    // L?y t?t c? task và n?p tên vào ComboBox
                    allTasks = SelectedController.Rapid.GetTasks();
                    foreach (var task in allTasks)
                        comboBoxSelectedTask.Items.Add(task.Name);

                    // N?u có đúng (ho?c ít nh?t) 2 task th? gán luôn 2 bi?n
                    if (comboBoxSelectedTask.Items.Count >= 2)
                    {
                        string name1 = comboBoxSelectedTask.Items[0].ToString()!;
                        string name2 = comboBoxSelectedTask.Items[1].ToString()!;

                        currentTask1 = SelectedController.Rapid.GetTask(name1);
                        currentTask2 = SelectedController.Rapid.GetTask(name2);

                        // (Tu? ch?n) show tên lên 2 Label ho?c TextBox đ? b?n bi?t
                        labelTask1.Text = "Task1: " + name1;
                        labelTask2.Text = "Task2: " + name2;
                    }

                    // Gi? ch?n m?c đ?nh đ? UI không l?i
                    if (comboBoxSelectedTask.Items.Count > 0)
                        comboBoxSelectedTask.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load tasks: " + ex.Message);
                }
            }
        }

        private void comboBoxSelectedTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            listModulefromTask.Items.Clear();  // Clear previous modules
            if (SelectedController == null || !SelectedController.Connected) return;

            try
            {
                string selectedTaskName = comboBoxSelectedTask.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(selectedTaskName))
                {
                    currentSelectedTask = SelectedController.Rapid.GetTask(selectedTaskName);

                    foreach (Module module in currentSelectedTask.GetModules())
                    {
                        listModulefromTask.Items.Add(module.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading modules: " + ex.Message);
            }
        }

        private void buttonToResetPPToMain_Click(object sender, EventArgs e)
        {
            if (SelectedController == null || !SelectedController.Connected)
            {
                MessageBox.Show("Please select a controller first!");
                return;
            }
            using (Mastership m = Mastership.Request(SelectedController))
            {
                foreach (var task in allTasks)
                {
                    task.ResetProgramPointer();
                }
            }
        }

        private void PrimeLastCapValue()
        {
            try
            {
                string capmodule = "Check_Face_DUL_L";

                if (currentTask2 == null)
                {
                    lastcapValue = string.Empty;
                    Dbg("RAPID-CAP", "PrimeLastCapValue: currentTask2=null, lastcapValue set to empty.");
                    return;
                }

                var capData = currentTask2.GetRapidData(capmodule, "cap");
                lastcapValue = capData.StringValue ?? string.Empty;

                Dbg("RAPID-CAP", $"PrimeLastCapValue DONE. initial cap='{lastcapValue}'. First poll will NOT send fake CAP.");
                rtbLog.AppendText($"[C#] Initial RAPID cap value primed: '{lastcapValue}'.\r\n");
            }
            catch (Exception ex)
            {
                lastcapValue = string.Empty;
                Dbg("RAPID-CAP", "PrimeLastCapValue ERROR: " + ex.ToString());
                rtbLog.AppendText("[C#][WARN] Không đọc được cap ban đầu, dùng lastcapValue rỗng. " + ex.Message + "\r\n");
            }
        }

        private async void buttonstartRAPID_Click(object sender, EventArgs e)
        {
            Dbg("START", "buttonstartRAPID_Click ENTER");

            capCount = 0;
            capPhase = true;
            caseListenerStarted = false;
            startPythonListenerCallCount = 0;
            solutionPrinted = false;
            sendHandlerCalled = false;
            abcHandlerCalled = false;
            lastcapValue = string.Empty;

            // 1) Start RAPID như cũ.
            bool rapidStarted = startRAPID();
            if (!rapidStarted)
            {
                Dbg("START", "RAPID không start được. Dừng connect Python.");
                return;
            }

            rtbLog.AppendText("[C#] RAPID started\r\n");

            // 2) Connect tới Python server đang chạy sẵn bằng: python main.py
            try
            {
                try
                {
                    pythonStream?.Close();
                    pythonClient?.Close();
                }
                catch { }

                pythonClient = new TcpClient();

                Dbg("SOCKET", "Connecting to Python server 127.0.0.1:1027 ...");
                await pythonClient.ConnectAsync("127.0.0.1", 1027);

                pythonStream = pythonClient.GetStream();

                Dbg("SOCKET", $"Connected to Python. pythonClientConnected={pythonClient.Connected}, pythonStreamNull={pythonStream == null}");
                rtbLog.AppendText("[C#] Connected to Python\r\n");

                currentCaseId = ReadCurrentCaseIdFromPython();
                int parsedCaseIndex = ParseCaseIndexFromCaseId(currentCaseId);
                if (parsedCaseIndex > 0) caseIndexCounter = parsedCaseIndex;
                else caseIndexCounter++;
                caseStartTime = DateTime.Now;
                robotStartTime = default;
                robotEndTime = default;
                robotExecutionSuccess = false;
                endToEndSuccess = false;
                handExchangeCount = 0;

                Dbg("CASE", $"currentCaseId={currentCaseId}, caseIndex={caseIndexCounter}");
                rtbLog.AppendText($"[C#] Current case_id: {currentCaseId}\r\n");

                // CỰC QUAN TRỌNG:
                // Lấy giá trị cap hiện tại làm mốc trước khi bật timer.
                // Nếu không làm bước này, lần poll đầu tiên có thể bị hiểu nhầm là CAP mới
                // và C# sẽ gửi CAP 1/6 quá sớm khi camera chưa thấy Rubik.
                PrimeLastCapValue();

                if (!dataMonitoringTimer.Enabled)
                    dataMonitoringTimer.Start();

                rtbLog.AppendText("[C#] Timer Started after priming RAPID cap value\r\n");
            }
            catch (Exception ex)
            {
                Dbg("SOCKET", "Connect Python FAILED: " + ex.ToString());

                rtbLog.AppendText("[C#][ERROR] Python server chưa chạy. Hãy chạy python main.py trước.\r\n");
                rtbLog.AppendText("[C#][ERROR] Chi tiết: " + ex.Message + "\r\n");

                MessageBox.Show(
                    "Python server chưa chạy.\n\nHãy mở terminal và chạy:\npython main.py\n\nSau đó bấm Start lại trong C#.",
                    "Không kết nối được Python",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private bool startRAPID()   //this function used to run the RAPID program
        {
            if (SelectedController == null)
            {
                MessageBox.Show("Please select a controller first!");
                return false;
            }

            try
            {
                if (SelectedController.OperatingMode == ControllerOperatingMode.Auto &&
                    SelectedController.State == ControllerState.MotorsOn)
                {
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        foreach (var task in allTasks)
                        {
                            task.ResetProgramPointer();
                        }

                        StartResult result = SelectedController.Rapid.Start(true);

                        buttonstartRAPID.Enabled = false;
                        buttonstopRAPID.Enabled = true;

                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode and MotorsOn are required to start execution from a remote client.");
                    return false;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client. " + ex.Message);
                return false;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
                return false;
            }
        }

        private void buttonstopRAPID_Click(object sender, EventArgs e)
        {
            stopRAPID();

        }

        private void stopRAPID()
        {
            if (SelectedController == null) return;

            try
            {
                if (SelectedController.OperatingMode == ControllerOperatingMode.Auto && SelectedController.State == ControllerState.MotorsOn)
                {
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        SelectedController.Rapid.Stop(StopMode.Immediate);
                        buttonstopRAPID.Enabled = false;
                        buttonstartRAPID.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (SelectedController == null || !SelectedController.Connected) return;

            try
            {
                using (Mastership m = Mastership.Request(SelectedController))
                {
                    string module = "Module1";

                    // --- Task 1: gán recvMsg = "CHECK" ---
                    RapidData rd1 = currentTask1.GetRapidData(module, "recvMsg");
                    var str1 = new ABB.Robotics.Controllers.RapidDomain.String();
                    str1.FillFromString("CHECK");
                    rd1.Value = str1;

                    // --- Task 2: gán recvMsg = "CHECK" ---
                    RapidData rd2 = currentTask2.GetRapidData(module, "recvMsg");
                    var str2 = new ABB.Robotics.Controllers.RapidDomain.String();
                    str2.FillFromString("CHECK");
                    rd2.Value = str2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing to RAPID: " + ex.Message);
            }

        }
        private void SendCapToPython()
        {
            Dbg("CAP", $"ENTER SendCapToPython(). capPhase={capPhase}, capCount={capCount}, pythonStreamNull={pythonStream == null}, pythonClientNull={pythonClient == null}, pythonClientConnected={(pythonClient != null ? pythonClient.Connected.ToString() : "null")}");

            if (!capPhase)
            {
                Dbg("CAP", "RETURN: capPhase=false, không gửi CAP.");
                return;
            }

            if (capCount >= 6)
            {
                Dbg("CAP", $"RETURN: capCount >= 6. capCount={capCount}");
                return;
            }

            if (pythonStream == null)
            {
                Dbg("CAP", "ERROR: pythonStream == null. Chưa connect Python.");
                return;
            }

            try
            {
                byte[] capBytes = Encoding.ASCII.GetBytes("CAP\n");

                pythonStream.Write(capBytes, 0, capBytes.Length);
                pythonStream.Flush();

                capCount++;

                Dbg("CAP", $"CAP sent SUCCESS. capCount={capCount}/6");

                rtbLog.AppendText($"[CAP {capCount}/6] Sent to python." + Environment.NewLine);

                if (capCount == 6)
                {
                    capPhase = false;

                    Dbg("CAP", "Đã gửi đủ 6 CAP. Bắt đầu chờ case từ Python.");
                    rtbLog.AppendText("[C#] Đã gửi đủ 6 CAP, bắt đầu chờ case từ Python...\r\n");

                    // Chỉ gọi listener đúng 1 lần sau CAP 6/6.
                    StartPythonListener();
                }
            }
            catch (Exception ex)
            {
                Dbg("CAP", "EXCEPTION SendCapToPython(): " + ex.ToString());
                rtbLog.AppendText("[C#][ERROR] SendCapToPython lỗi: " + ex.Message + Environment.NewLine);
            }
        }

        private void SendCaseAndSolution(string caseStr)
        {
            string module = "Module1";
            string libL = "Lib_Rotate_HandL";
            string libR = "Lib_Rotate_HandR";
            string lib24R = "Lib_24_case_R";
            string lib24L = "Lib_24_case_L";

            string solutionTextForSummary = "";
            var movesForSummary = new List<string>();       // Chỉ move Rubik thật, KHÔNG gồm E.
            var robotCommands = new List<string>();         // Lệnh gửi RAPID, GIỮ NGUYÊN solution.txt, gồm cả E nếu có.

            if (string.IsNullOrWhiteSpace(currentCaseId))
            {
                currentCaseId = ReadCurrentCaseIdFromPython();
                int parsedCaseIndex = ParseCaseIndexFromCaseId(currentCaseId);
                if (parsedCaseIndex > 0) caseIndexCounter = parsedCaseIndex;
                if (caseStartTime == default) caseStartTime = DateTime.Now;
                Dbg("CASE", $"SendCaseAndSolution fallback currentCaseId={currentCaseId}, caseIndex={caseIndexCounter}");
            }

            Dbg("SEND", $"ENTER SendCaseAndSolution(). caseStr='{caseStr}', case_id='{currentCaseId}'");

            if (!f2)
            {
                this.Invoke((Action)(() => OpenDisplayForm()));
                f2 = true;
            }

            try
            {
                // Thời gian robot bắt đầu: ngay trước command đầu tiên gửi xuống RAPID.
                robotStartTime = DateTime.Now;

                // --- Gửi case number xuống RAPID ---
                using (Mastership m = Mastership.Request(SelectedController))
                {
                    RapidData rd1 = currentTask1.GetRapidData(libR, "number");
                    var str1 = new ABB.Robotics.Controllers.RapidDomain.String();
                    str1.FillFromString(caseStr);
                    rd1.Value = str1;

                    RapidData rd2 = currentTask2.GetRapidData(libL, "number");
                    var str2 = new ABB.Robotics.Controllers.RapidDomain.String();
                    str2.FillFromString(caseStr);
                    rd2.Value = str2;
                }

                Invoke((Action)(() =>
                    rtbLog.AppendText($"Sent case {caseStr} to robots.{Environment.NewLine}")
                ));

                // --- Chờ ACK case orientation/24-case từ robot: GIỮ LOGIC CŨ, cả hai end phải đổi ---
                string oldEnd1 = lastend1;
                string oldEnd2 = lastend2;

                while (true)
                {
                    string endValue1 = currentTask1.GetRapidData(lib24R, "end").StringValue;
                    string endValue2 = currentTask2.GetRapidData(lib24L, "end").StringValue;

                    if (endValue1 != oldEnd1 && endValue2 != oldEnd2)
                    {
                        lastend1 = endValue1;
                        lastend2 = endValue2;
                        Invoke((Action)(() =>
                            rtbLog.AppendText($"ACK received for case: {caseStr}\r\n")
                        ));
                        break;
                    }
                    Thread.Sleep(50);
                }

                // --- Đọc solution.txt ---
                string exeDir = Application.StartupPath;
                string solutionPath = Path.Combine(exeDir, "solution.txt");
                int retries = 0;
                int maxRetries = 30;

                Invoke((Action)(() =>
                {
                    rtbLog.AppendText("Waiting for Python to generate solution.txt...\r\n");
                    rtbLog.AppendText($"[C#] Application.StartupPath = {Application.StartupPath}\r\n");
                    rtbLog.AppendText($"[C#] Waiting solution.txt at = {solutionPath}\r\n");
                    rtbLog.AppendText($"[C#] Waiting rubik_state.txt at = {Path.Combine(exeDir, "rubik_state.txt")}\r\n");
                    rtbLog.AppendText($"[C#] Reading current_case_id.txt at = {Path.Combine(exeDir, "current_case_id.txt")}\r\n");
                }));

                while (!File.Exists(solutionPath) && retries < maxRetries)
                {
                    Thread.Sleep(500);
                    retries++;
                }

                if (!File.Exists(solutionPath))
                {
                    string msg = $"Timeout: Python tính quá lâu hoặc chưa tạo file!\n{solutionPath}\n\n" +
                                 "Hãy chạy Python với --app-dir trỏ đúng folder C# output, ví dụ:\n" +
                                 "& \"D:\\robotCatchingSimulationUsingEGM\\.venv\\Scripts\\python.exe\" \"D:\\robotCatchingSimulationUsingEGM\\main.py\" --app-dir \"" + Application.StartupPath + "\"";
                    LogFailureEvent("solver", 0, "", "SOLVER_FAILED", msg, "Stop case");
                    LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: false, solverSuccess: false, robotSuccess: false, failureReason: msg);
                    MessageBox.Show(msg);
                    return;
                }

                // CỰC QUAN TRỌNG:
                // robotCommands GIỮ NGUYÊN solution.txt như code chạy ổn định cũ, bao gồm cả dòng E.
                // Không lọc E khỏi luồng gửi RAPID. Chỉ loại E khỏi thống kê/log robot_steps.
                robotCommands = File.ReadAllLines(solutionPath, Encoding.UTF8)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToList();

                var validRobotCommands = new HashSet<string>
                {
                    "U", "U'", "U2", "D", "D'", "D2", "L", "L'", "L2",
                    "R", "R'", "R2", "F", "F'", "F2", "B", "B'", "B2", "E"
                };

                var invalid = robotCommands.Where(c => !validRobotCommands.Contains(c)).ToList();
                if (invalid.Count > 0)
                {
                    string msg = "solution.txt có lệnh không hợp lệ: " + string.Join(" ", invalid);
                    LogFailureEvent("solver", 0, "", "SOLVER_FAILED", msg, "Stop case");
                    LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: true, solverSuccess: false, robotSuccess: false, failureReason: msg);
                    MessageBox.Show(msg);
                    return;
                }

                movesForSummary = robotCommands.Where(c => c != "E").ToList();
                solutionTextForSummary = string.Join(" ", movesForSummary);

                if (movesForSummary.Count == 0)
                {
                    string msg = "solution.txt rỗng hoặc chỉ có E. Không gửi robot chạy.";
                    LogFailureEvent("solver", 0, "", "SOLVER_FAILED", msg, "Stop case");
                    LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: true, solverSuccess: false, robotSuccess: false, failureReason: msg);
                    MessageBox.Show(msg);
                    return;
                }

                if (!solutionPrinted)
                {
                    // In ra UI giống code cũ: có thể in cả E để dễ đối chiếu solution.txt.
                    string solutionStringForUi = string.Join(" ", robotCommands);
                    Invoke((Action)(() =>
                        rtbLog.AppendText($"Solution: {solutionStringForUi}{Environment.NewLine}")
                    ));
                    solutionPrinted = true;
                }

                var movePlans = AnalyzeSolutionMoves(movesForSummary);

                // --- Lấy giá trị fn ban đầu trước khi gửi move: GIỮ LOGIC CŨ ---
                if (currentTask1 != null && currentTask2 != null)
                {
                    lastfn1 = currentTask1.GetRapidData(libR, "fn").StringValue;
                    lastfn2 = currentTask2.GetRapidData(libL, "fn").StringValue;
                }

                string prevMove = null;
                int realMoveIndex = 0;

                for (int idx = 0; idx < robotCommands.Count; idx++)
                {
                    string command = robotCommands[idx];
                    string nextCommand = (idx + 1 < robotCommands.Count) ? robotCommands[idx + 1] : ""; // GIỮ ĐÚNG CODE CŨ
                    bool isEndMarker = command == "E";

                    RobotMovePlan plan = null;
                    if (!isEndMarker)
                    {
                        plan = movePlans[realMoveIndex];
                        realMoveIndex++;
                    }

                    DateTime commandSendTime = DateTime.Now;

                    // 1) Gửi command vào RAPID: GIỮ NGUYÊN GIAO THỨC CŨ
                    // - current command ghi vào sol
                    // - command kế tiếp ghi vào next
                    // - E vẫn được gửi nếu có trong solution.txt
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        RapidData n1 = currentTask1.GetRapidData(libR, "next");
                        var strn1 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strn1.FillFromString(nextCommand);
                        n1.Value = strn1;

                        RapidData n2 = currentTask2.GetRapidData(libL, "next");
                        var strn2 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strn2.FillFromString(nextCommand);
                        n2.Value = strn2;

                        RapidData s1 = currentTask1.GetRapidData(libR, "sol");
                        var strs1 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strs1.FillFromString(command);
                        s1.Value = strs1;

                        RapidData s2 = currentTask2.GetRapidData(libL, "sol");
                        var strs2 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strs2.FillFromString(command);
                        s2.Value = strs2;
                    }

                    Invoke((Action)(() =>
                    {
                        if (isEndMarker)
                            rtbLog.AppendText($"Sent final E to robots.{Environment.NewLine}");
                        else
                        {
                            rtbLog.AppendText($"Sent move: {command}{Environment.NewLine}");
                            if (plan != null && plan.HandExchangeRequired)
                                rtbLog.AppendText($"[EXCHANGE] {plan.ExchangeFrom} -> {plan.ExchangeTo} at step {plan.StepIndex}, move {command}{Environment.NewLine}");
                        }

                        // GIỮ ĐÚNG CODE CŨ: sau khi gửi command hiện tại mới apply prevMove lên display.
                        // Khi command = E, prevMove là move Rubik cuối, nên display được cập nhật move cuối.
                        if (prevMove != null && prevMove != "E")
                        {
                            if (cubeState == null)
                                cubeState = LoadState(Path.Combine(Application.StartupPath, "rubik_state.txt"));
                            ApplyMove(prevMove);
                            displayForm.ShowUnfold();
                            displayForm.openGLControl1.Refresh();
                        }
                    }));

                    // 2) Đợi robot xoay/kết thúc: GIỮ ĐÚNG ACK CŨ, cả hai fn phải đổi.
                    DateTime commandDoneTime = DateTime.Now;
                    bool ackReceived = false;
                    DateTime ackWaitStart = DateTime.Now;
                    int maxMoveAckWaitMs = 300000; // safety timeout 5 phút; không đổi handshake, chỉ tránh treo vô hạn.

                    while ((DateTime.Now - ackWaitStart).TotalMilliseconds < maxMoveAckWaitMs)
                    {
                        var fnData1 = currentTask1.GetRapidData(libR, "fn");
                        string fnValue1 = fnData1.StringValue;
                        var fnData2 = currentTask2.GetRapidData(libL, "fn");
                        string fnValue2 = fnData2.StringValue;

                        if (fnValue1 != lastfn1 && fnValue2 != lastfn2)
                        {
                            lastfn1 = fnValue1;
                            lastfn2 = fnValue2;
                            ackReceived = true;
                            commandDoneTime = DateTime.Now;

                            Invoke((Action)(() =>
                            {
                                if (isEndMarker)
                                    rtbLog.AppendText($"ACK received for final E/place-down{Environment.NewLine}");
                                else
                                    rtbLog.AppendText($"ACK received for move: {command}{Environment.NewLine}");
                            }));
                            break;
                        }

                        Thread.Sleep(50);
                    }

                    if (!ackReceived)
                    {
                        commandDoneTime = DateTime.Now;
                        string curFn1Timeout = currentTask1.GetRapidData(libR, "fn").StringValue;
                        string curFn2Timeout = currentTask2.GetRapidData(libL, "fn").StringValue;
                        string curSol1Timeout = currentTask1.GetRapidData(libR, "sol").StringValue;
                        string curSol2Timeout = currentTask2.GetRapidData(libL, "sol").StringValue;
                        string curNext1Timeout = currentTask1.GetRapidData(libR, "next").StringValue;
                        string curNext2Timeout = currentTask2.GetRapidData(libL, "next").StringValue;

                        string msg = isEndMarker
                            ? $"Timeout waiting ACK for final E/place-down. lastfn1={lastfn1}, lastfn2={lastfn2}, curFn1={curFn1Timeout}, curFn2={curFn2Timeout}, solR={curSol1Timeout}, solL={curSol2Timeout}, nextR={curNext1Timeout}, nextL={curNext2Timeout}"
                            : $"Timeout waiting fn ACK for step {plan.StepIndex}, move {command}. lastfn1={lastfn1}, lastfn2={lastfn2}, curFn1={curFn1Timeout}, curFn2={curFn2Timeout}, solR={curSol1Timeout}, solL={curSol2Timeout}, nextR={curNext1Timeout}, nextL={curNext2Timeout}";

                        if (!isEndMarker && plan != null)
                            LogRobotStep(plan, commandSendTime, commandDoneTime, false, "ROBOT_TIMEOUT", msg);

                        LogFailureEvent("robot_execution",
                            isEndMarker ? movesForSummary.Count + 1 : plan.StepIndex,
                            command,
                            "ROBOT_TIMEOUT",
                            msg,
                            "Stop case and inspect RAPID fn/end update");

                        LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: true, solverSuccess: true, robotSuccess: false, failureReason: msg);
                        MessageBox.Show(msg);
                        return;
                    }

                    if (!isEndMarker && plan != null)
                        LogRobotStep(plan, commandSendTime, commandDoneTime, true);

                    prevMove = command;
                    Thread.Sleep(500);
                }

                robotEndTime = DateTime.Now;
                robotExecutionSuccess = true;
                LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: true, solverSuccess: true, robotSuccess: true);

                Dbg("SEND", $"CASE DONE. case_id={currentCaseId}, moves={movesForSummary.Count}, hand_exchange={handExchangeCount}, robotCommands={robotCommands.Count}");
            }
            catch (Exception ex)
            {
                robotEndTime = DateTime.Now;
                robotExecutionSuccess = false;
                string msg = ex.Message;
                LogFailureEvent("robot_execution", 0, "", "RAPID_ERROR", msg, "Check C#/RAPID log");
                LogCaseSummary(movesForSummary, solutionTextForSummary, recognitionSuccess: true, solverSuccess: movesForSummary.Count > 0, robotSuccess: false, failureReason: msg);
                MessageBox.Show("Error sending case and solution: " + ex.Message);
            }
        }

        private void StartPythonListener()
        {
            int callNo = System.Threading.Interlocked.Increment(ref startPythonListenerCallCount);

            Dbg("LISTENER", $"ENTER StartPythonListener(). callNo={callNo}, capCount={capCount}, caseListenerStarted={caseListenerStarted}, pythonStreamNull={pythonStream == null}, pythonClientConnected={(pythonClient != null ? pythonClient.Connected.ToString() : "null")}");

            if (pythonStream == null)
            {
                Dbg("LISTENER", "RETURN: pythonStream == null.");
                rtbLog.AppendText("[C#][WARN] pythonStream chưa kết nối, chưa đọc case được.\r\n");
                return;
            }

            if (caseListenerStarted)
            {
                Dbg("LISTENER", "RETURN: caseListenerStarted=true, không chạy listener lần 2.");
                return;
            }

            if (capCount < 6)
            {
                Dbg("LISTENER", $"RETURN: Chưa đủ 6 CAP. capCount={capCount}");
                return;
            }

            caseListenerStarted = true;

            System.Threading.Tasks.Task.Run(() =>
            {
                string caseFromPy = null;

                try
                {
                    using (var reader = new StreamReader(pythonStream, Encoding.ASCII, false, 1024, leaveOpen: true))
                    {
                        while (true)
                        {
                            Dbg("LISTENER", "Before ReadLine()");
                            string line = reader.ReadLine();

                            if (line == null)
                            {
                                Dbg("LISTENER", "ReadLine returned NULL. Socket có thể đã đóng.");
                                break;
                            }

                            Dbg("LISTENER", $"ReadLine got: '{line}'");

                            if (line == "ACK")
                            {
                                Dbg("LISTENER", "Skip ACK");
                                continue;
                            }

                            caseFromPy = line.Trim();
                            Dbg("LISTENER", $"caseFromPy='{caseFromPy}'");
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(caseFromPy))
                    {
                        Dbg("LISTENER", "NO CASE RECEIVED. In ra No case number received from Python.");
                        this.BeginInvoke((Action)(() =>
                            rtbLog.AppendText("No case number received from Python." + Environment.NewLine)));
                        return;
                    }

                    Dbg("LISTENER", $"Received case '{caseFromPy}'. Waiting SEND1/SEND2 before SendCaseAndSolution.");

                    string module = "Module1";

                    // Chờ SEND của RAPID tối đa 30 giây.
                    bool send1 = false;
                    bool send2 = false;

                    for (int i = 0; i < 600; i++)
                    {
                        if (currentTask1 == null || currentTask2 == null)
                        {
                            Dbg("LISTENER", "currentTask1/currentTask2 null while waiting SEND.");
                            Thread.Sleep(50);
                            continue;
                        }

                        send1 = ((ABB.Robotics.Controllers.RapidDomain.Bool)
                                  currentTask1.GetRapidData(module, "SEND").Value).Value;

                        send2 = ((ABB.Robotics.Controllers.RapidDomain.Bool)
                                  currentTask2.GetRapidData(module, "SEND").Value).Value;

                        if (send1 && send2)
                            break;

                        if (i % 20 == 0)
                            Dbg("LISTENER", $"Waiting SEND... send1={send1}, send2={send2}");

                        Thread.Sleep(50);
                    }

                    Dbg("LISTENER", $"Final SEND values: send1={send1}, send2={send2}, sendHandlerCalled={sendHandlerCalled}");

                    if (!sendHandlerCalled && send1 && send2)
                    {
                        sendHandlerCalled = true;
                        Dbg("LISTENER", $"Ready SendCaseAndSolution('{caseFromPy}')");

                        BgTask.Run(() =>
                        {
                            Dbg("LISTENER", $"Before SendCaseAndSolution('{caseFromPy}')");
                            SendCaseAndSolution(caseFromPy);
                            Dbg("LISTENER", $"After SendCaseAndSolution('{caseFromPy}')");
                        });
                    }
                    else
                    {
                        Dbg("LISTENER", $"NOT calling SendCaseAndSolution. sendHandlerCalled={sendHandlerCalled}, send1={send1}, send2={send2}");
                        this.BeginInvoke((Action)(() =>
                            rtbLog.AppendText("[C#][WARN] Đã nhận case nhưng SEND1/SEND2 chưa sẵn sàng nên chưa gửi RAPID.\r\n")));
                    }
                }
                catch (Exception ex)
                {
                    Dbg("LISTENER", "EXCEPTION StartPythonListener(): " + ex.ToString());

                    try
                    {
                        this.BeginInvoke((Action)(() =>
                            MessageBox.Show("Error sending case & solution: " + ex.Message)));
                    }
                    catch { }
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Không chạy Python main.exe nữa.
            // Bây giờ bạn tự mở terminal chạy: python main.py
            try
            {
                if (wmpBackground != null)
                {
                    wmpBackground.Ctlcontrols.stop();
                    wmpBackground.URL = "";
                    wmpBackground.Visible = false;
                    wmpBackground.Enabled = false;
                    wmpBackground.SendToBack();
                }
            }
            catch { }

            rtbLog.AppendText("[C#] Python auto-launch disabled. Please run python main.py manually first.\r\n");
            Dbg("FORM", "Form1_Load: disabled PythonLauncher.RunSolver() and background video.");
        }

        private void OpenDisplayForm()
        {
            if (displayForm == null || displayForm.IsDisposed)
            {
                string sp = Path.Combine(Application.StartupPath, "rubik_state.txt");
                cubeState = LoadState(sp);

                // 2) Tạo DisplayForm
                displayForm = new DisplayForm(cubeState);
                displayForm.StartPosition = FormStartPosition.CenterScreen;

                // 3) Khi bên DisplayForm nhấn Reset, Form1 sẽ chạy ResetAll()
                //displayForm.ResetClicked += (s, ev) => ResetAll();

                // 4) Hiện vùng hiển thị và ẩn Form1
                displayForm.Show();
                //this.Hide();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetAll();
        }

        private async void ResetAll()
        {
            cubeState = null;

            if (displayForm != null && !displayForm.IsDisposed)
            {
                displayForm.Close();
                displayForm = null;
            }

            if (SelectedController != null && SelectedController.Connected)
            {
                try
                {
                    using (var m = Mastership.Request(SelectedController))
                    {
                        foreach (var task in allTasks)
                            task.ResetProgramPointer();
                    }

                    buttonstopRAPID.Enabled = false;
                    buttonstartRAPID.Enabled = true;
                }
                catch (System.InvalidOperationException)
                {
                    rtbLog.AppendText("Mastership đã có sẵn, bỏ qua reset pointer.\r\n");
                }
                catch (Exception ex)
                {
                    rtbLog.AppendText("[RESET][WARN] Reset pointer lỗi: " + ex.Message + "\r\n");
                }
            }

            rtbLog.Clear();

            capCount = 0;
            capPhase = true;
            solutionPrinted = false;
            caseListenerStarted = false;
            startPythonListenerCallCount = 0;

            lastcapValue = string.Empty;
            lastend1 = string.Empty;
            lastend2 = string.Empty;
            lastfn1 = string.Empty;
            lastfn2 = string.Empty;
            lastcs1 = string.Empty;
            lastcs2 = string.Empty;
            lastSend1 = false;
            lastSend2 = false;
            lastABC1 = false;
            lastABC2 = false;
            abcHandlerCalled = false;
            sendHandlerCalled = false;
            f2 = false;
            currentCaseId = string.Empty;
            caseStartTime = default;
            caseEndTime = default;
            robotStartTime = default;
            robotEndTime = default;
            robotExecutionSuccess = false;
            endToEndSuccess = false;
            handExchangeCount = 0;

            try
            {
                pythonStream?.Close();
                pythonClient?.Close();
            }
            catch { }

            pythonStream = null;
            pythonClient = null;

            // Không kill/restart Python nữa vì Python chạy bằng source ở terminal riêng.
            pythonProcess = null;

            await System.Threading.Tasks.Task.Run(() =>
            {
                if (SelectedController == null || !SelectedController.Connected || currentTask1 == null || currentTask2 == null)
                    return;

                try
                {
                    using (Mastership m = Mastership.Request(SelectedController))
                    {
                        string module = "Module1";
                        string libL = "Lib_Rotate_HandL";
                        string libR = "Lib_Rotate_HandR";

                        RapidData rd1 = currentTask1.GetRapidData(module, "recvMsg");
                        var str1 = new ABB.Robotics.Controllers.RapidDomain.String();
                        str1.FillFromString("");
                        rd1.Value = str1;

                        RapidData rd2 = currentTask2.GetRapidData(module, "recvMsg");
                        var str2 = new ABB.Robotics.Controllers.RapidDomain.String();
                        str2.FillFromString("");
                        rd2.Value = str2;

                        RapidData s1 = currentTask1.GetRapidData(libR, "sol");
                        var strs1 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strs1.FillFromString("");
                        s1.Value = strs1;

                        RapidData s2 = currentTask2.GetRapidData(libL, "sol");
                        var strs2 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strs2.FillFromString("");
                        s2.Value = strs2;

                        RapidData n1 = currentTask1.GetRapidData(libR, "number");
                        var strn1 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strn1.FillFromString("");
                        n1.Value = strn1;

                        RapidData n2 = currentTask2.GetRapidData(libL, "number");
                        var strn2 = new ABB.Robotics.Controllers.RapidDomain.String();
                        strn2.FillFromString("");
                        n2.Value = strn2;
                    }
                }
                catch
                {
                    // Không để reset làm crash UI.
                }
            });

            rtbLog.AppendText("[C#] Reset xong. Python source server KHÔNG được restart tự động. Nếu socket đã đóng, hãy restart python main.py rồi bấm Start lại.\r\n");
        }

        private void rtbLog_TextChanged(object sender, EventArgs e)
        {
            // Di chuyển con trỏ về cuối text
            rtbLog.SelectionStart = rtbLog.TextLength;
            // Cuộn để con trỏ (caret) luôn hiển thị
            rtbLog.ScrollToCaret();
        }
    }
}
