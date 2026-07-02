using SharpGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RubikSolver
{
    public partial class DisplayForm : Form
    {
        //public event EventHandler? ResetClicked;
        //Giao dien OpenGL
        private float cameraAngleX = 0.0f;
        private float cameraAngleY = 0.0f;
        private string[,] cubeColors = new string[3, 3] {
            { "orange", "orange", "orange" },
            { "green", "green", "green" },
            { "yellow", "yellow", "yellow" }
        };
        private Dictionary<string, string[,]> cubeState;

        public DisplayForm(Dictionary<string, string[,]> shared)
      : this()    // gọi constructor mặc định
        {
            this.cubeState = shared;
        }

        public DisplayForm()
        {
            InitializeComponent();
            pbUnfold.SizeMode = PictureBoxSizeMode.Zoom;
            //btnReset.Click += btnReset_Click;
            this.KeyPreview = true;
            // Bắt KeyDown
            openGLControl1.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                    e.IsInputKey = true;
            };
            this.KeyDown += Form1_KeyDown;
            this.Load += DisplayForm_Load;
        }


        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            var gl = openGLControl1.OpenGL;
            gl.ClearColor(0.8f, 0.8f, 0.8f, 1);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            var gl = openGLControl1.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            gl.Translate(0, 0, -10);
            // Xoay camera theo góc
            gl.Rotate(cameraAngleX, 1, 0, 0); // X-axis
            gl.Rotate(cameraAngleY, 0, 1, 0); // Y-axis
            DrawRubikFull(gl, cubeState);
            gl.Flush();
        }

        private void openGLControl1_Resize(object sender, EventArgs e)
        {
            var gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45.0, (double)openGLControl1.Width / openGLControl1.Height, 0.1, 100);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            const float delta = 5.0f; // mỗi lần 5 độ
            switch (e.KeyCode)
            {
                case Keys.Left: cameraAngleY -= delta; break;
                case Keys.Right: cameraAngleY += delta; break;
                case Keys.Up: cameraAngleX -= delta; break;
                case Keys.Down: cameraAngleX += delta; break;
            }
            // Ép OpenGLControl vẽ lại
            openGLControl1.Refresh();
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

        private float[] ToGLColor(string name)
        {
            switch (name)
            {
                case "white": return new[] { 1f, 1f, 1f };
                case "yellow": return new[] { 1f, 1f, 0f };
                case "red": return new[] { 1f, 0f, 0f };
                case "orange": return new[] { 1f, 0.5f, 0f };
                case "green": return new[] { 0f, 1f, 0f };
                case "blue": return new[] { 0f, 0f, 1f };
                default: return new[] { 0.3f, 0.3f, 0.3f };
            }
        }

        private void DrawRubikFull(OpenGL gl, Dictionary<string, string[,]> S)
        {
            float size = 1f, sp = 1.02f;
            foreach (var xi in new[] { -1, 0, 1 })
                foreach (var yj in new[] { -1, 0, 1 })
                    foreach (var zk in new[] { -1, 0, 1 })
                    {
                        float x = xi * sp, y = yj * sp, z = zk * sp;
                        var cols = new Dictionary<string, float[]>();
                        if (zk == 1) cols["F"] = ToGLColor(S["F"][2 - (yj + 1), xi + 1]);
                        if (zk == -1) cols["B"] = ToGLColor(S["B"][2 - (yj + 1), 1 - (xi + 1) + 1]);
                        if (yj == 1) cols["U"] = ToGLColor(S["U"][1 + zk, xi + 1]);
                        if (yj == -1) cols["D"] = ToGLColor(S["D"][1 - zk, xi + 1]);
                        if (xi == 1) cols["R"] = ToGLColor(S["R"][2 - (yj + 1), 2 - (zk + 1)]);
                        if (xi == -1) cols["L"] = ToGLColor(S["L"][2 - (yj + 1), zk + 1]);
                        DrawCubie(gl, x, y, z, size, cols);
                    }
        }

        private void DrawCubie(OpenGL gl, float x, float y, float z, float s, Dictionary<string, float[]> cols)
        {
            float h = s / 2;
            if (cols.ContainsKey("F"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["F"]);
                gl.Vertex(x - h, y - h, z + h); gl.Vertex(x + h, y - h, z + h);
                gl.Vertex(x + h, y + h, z + h); gl.Vertex(x - h, y + h, z + h);
                gl.End();
            }
            if (cols.ContainsKey("B"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["B"]);
                gl.Vertex(x - h, y - h, z - h); gl.Vertex(x - h, y + h, z - h);
                gl.Vertex(x + h, y + h, z - h); gl.Vertex(x + h, y - h, z - h);
                gl.End();
            }
            if (cols.ContainsKey("U"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["U"]);
                gl.Vertex(x - h, y + h, z - h); gl.Vertex(x - h, y + h, z + h);
                gl.Vertex(x + h, y + h, z + h); gl.Vertex(x + h, y + h, z - h);
                gl.End();
            }
            if (cols.ContainsKey("D"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["D"]);
                gl.Vertex(x - h, y - h, z - h); gl.Vertex(x + h, y - h, z - h);
                gl.Vertex(x + h, y - h, z + h); gl.Vertex(x - h, y - h, z + h);
                gl.End();
            }
            if (cols.ContainsKey("R"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["R"]);
                gl.Vertex(x + h, y - h, z - h); gl.Vertex(x + h, y + h, z - h);
                gl.Vertex(x + h, y + h, z + h); gl.Vertex(x + h, y - h, z + h);
                gl.End();
            }
            if (cols.ContainsKey("L"))
            {
                gl.Begin(OpenGL.GL_QUADS);
                gl.Color(cols["L"]);
                gl.Vertex(x - h, y - h, z - h); gl.Vertex(x - h, y - h, z + h);
                gl.Vertex(x - h, y + h, z + h); gl.Vertex(x - h, y + h, z - h);
                gl.End();
            }
        }



        // Áp một bước move lên cubeState


        public void ShowUnfold()
        {
            // M?t theo th? t? U,R,F,D,L,B
            var facesOrder = new[] { "U", "R", "F", "D", "L", "B" };

            // 1) L?y m?ng 6 faces, m?i face 9 màu t? cubeState
            string[][] faceColors = facesOrder
                .Select(f => Enumerable.Range(0, 3)
                    .SelectMany(i => Enumerable.Range(0, 3)
                        .Select(j => cubeState[f][i, j]))
                    .ToArray()
                ).ToArray();

            // 2) V? trí 1-based (row,col) trên lư?i 12×9
            var facePositions = new List<List<Point>>
            {
                // U
                new List<Point> {
                    new Point(1,4), new Point(1,5), new Point(1,6),
                    new Point(2,4), new Point(2,5), new Point(2,6),
                    new Point(3,4), new Point(3,5), new Point(3,6)
                },
                // R
                new List<Point> {
                    new Point(4,7), new Point(4,8), new Point(4,9),
                    new Point(5,7), new Point(5,8), new Point(5,9),
                    new Point(6,7), new Point(6,8), new Point(6,9)
                },
                // F
                new List<Point> {
                    new Point(4,4), new Point(4,5), new Point(4,6),
                    new Point(5,4), new Point(5,5), new Point(5,6),
                    new Point(6,4), new Point(6,5), new Point(6,6)
                },
                // D
                new List<Point> {
                    new Point(7,4), new Point(7,5), new Point(7,6),
                    new Point(8,4), new Point(8,5), new Point(8,6),
                    new Point(9,4), new Point(9,5), new Point(9,6)
                },
                // L
                new List<Point> {
                    new Point(4,1), new Point(4,2), new Point(4,3),
                    new Point(5,1), new Point(5,2), new Point(5,3),
                    new Point(6,1), new Point(6,2), new Point(6,3)
                },
                // B
                new List<Point> {
                    new Point(4,10), new Point(4,11), new Point(4,12),
                    new Point(5,10), new Point(5,11), new Point(5,12),
                    new Point(6,10), new Point(6,11), new Point(6,12)
                }
            };


            // 3) T?o bitmap + graphics
            // 3) T?o bitmap + graphics
            const int cols = 12, rows = 9, cellSize = 60;
            var bmp = new Bitmap(cols * cellSize, rows * cellSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 4) B?ng màu
                var colorDict = new Dictionary<string, Color>
                {
                    { "white",  Color.White },
                    { "yellow", Color.Yellow },
                    { "red",    Color.Red },
                    { "orange", Color.Orange },
                    { "green",  Color.Lime },   // xanh sáng hơn
                    { "blue",   Color.Blue }
                };

                // 5) V? 6 m?t
                for (int f = 0; f < 6; f++)
                {
                    var colors = faceColors[f];
                    var poses = facePositions[f];
                    for (int k = 0; k < 9; k++)
                    {
                        int r1 = poses[k].X, c1 = poses[k].Y;
                        int x = (c1 - 1) * cellSize, y = (r1 - 1) * cellSize;
                        using (var b = new SolidBrush(colorDict[colors[k]]))
                            g.FillRectangle(b, x, y, cellSize, cellSize);
                        g.DrawRectangle(Pens.Black, x, y, cellSize, cellSize);
                    }
                }
            }

            // 6) Hi?n th? lên PictureBox
            pbUnfold.Image = bmp;
            pbUnfold.Visible = true;
            pbUnfold.BringToFront();
        }

        private void DisplayForm_Load(object sender, EventArgs e)
        {
            //// Hien thi Unfold
            //string exeDir = Application.StartupPath;
            //// K?t h?p đ? có đư?ng d?n đ?n rubik_state.txt trong cùng thư m?c EXE
            //string statePath = Path.Combine(exeDir, "rubik_state.txt");
            //if (!File.Exists(statePath))
            //{
            //    MessageBox.Show($"Không tìm thấy rubik_state.txt:\n{statePath}");
            //    return;
            //}
            //cubeState = LoadState(statePath);
            // 1) Hi?n th? ?nh unfold
            ShowUnfold();
            // 2) Hiển thị và refresh control 3D
            openGLControl1.Visible = true;
            openGLControl1.BringToFront();
            openGLControl1.Refresh();
            // End
            openGLControl1.TabStop = true;
            openGLControl1.Focus();
        }

        //private void btnReset_Click(object sender, EventArgs e)
        //{
        //    // Raise event, Form1 đã subscribe sẽ gọi ResetAll()
        //    ResetClicked?.Invoke(this, EventArgs.Empty);
        //}
    }
}
