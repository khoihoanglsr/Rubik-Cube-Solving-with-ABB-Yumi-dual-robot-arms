import pygame
from pygame.locals import QUIT, KEYDOWN, K_ESCAPE, K_LEFT, K_RIGHT, K_UP, K_DOWN, K_SPACE, DOUBLEBUF, OPENGL
from OpenGL.GL import *
from OpenGL.GLU import *
import numpy as np
import math

# ---------------------- DỮ LIỆU MẪU (dummy ordered_faces) ----------------------
ordered_faces = [
    [['white' for _ in range(3)] for _ in range(3)],   # U
    [['red' for _ in range(3)] for _ in range(3)],       # R
    [['green' for _ in range(3)] for _ in range(3)],     # F
    [['yellow' for _ in range(3)] for _ in range(3)],     # D
    [['orange' for _ in range(3)] for _ in range(3)],     # L
    [['blue' for _ in range(3)] for _ in range(3)]        # B
]

# Tạo trạng thái Rubik (cube_state) theo thứ tự URFDLB
cube_state = {
    'U': ordered_faces[0],
    'R': ordered_faces[1],
    'F': ordered_faces[2],
    'D': ordered_faces[3],
    'L': ordered_faces[4],
    'B': ordered_faces[5]
}

# ---------------------- HÀM CHUYỂN ĐỔI MÀU ----------------------
def get_gl_color(color_name):
    mapping = {
        'white': (1.0, 1.0, 1.0),
        'yellow': (1.0, 1.0, 0.0),
        'red': (1.0, 0.0, 0.0),
        'orange': (1.0, 0.5, 0.0),
        'green': (0.0, 1.0, 0.0),
        'blue': (0.0, 0.0, 1.0),
        'unknown': (0.3, 0.3, 0.3)
    }
    return mapping.get(color_name, (0.0, 0.0, 0.0))

# ---------------------- HÀM VẼ CUBIE ----------------------
def draw_cubie(x, y, z, size, colors):
    half = size / 2.0

    # Front face: vẽ nếu màu khác (0,0,0)
    if colors.get('F', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['F'])
        glVertex3f(x - half, y - half, z + half)
        glVertex3f(x + half, y - half, z + half)
        glVertex3f(x + half, y + half, z + half)
        glVertex3f(x - half, y + half, z + half)
        glEnd()

    # Back face:
    if colors.get('B', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['B'])
        glVertex3f(x - half, y - half, z - half)
        glVertex3f(x - half, y + half, z - half)
        glVertex3f(x + half, y + half, z - half)
        glVertex3f(x + half, y - half, z - half)
        glEnd()

    # Up face:
    if colors.get('U', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['U'])
        glVertex3f(x - half, y + half, z - half)
        glVertex3f(x - half, y + half, z + half)
        glVertex3f(x + half, y + half, z + half)
        glVertex3f(x + half, y + half, z - half)
        glEnd()

    # Down face:
    if colors.get('D', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['D'])
        glVertex3f(x - half, y - half, z - half)
        glVertex3f(x + half, y - half, z - half)
        glVertex3f(x + half, y - half, z + half)
        glVertex3f(x - half, y - half, z + half)
        glEnd()

    # Right face:
    if colors.get('R', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['R'])
        glVertex3f(x + half, y - half, z - half)
        glVertex3f(x + half, y + half, z - half)
        glVertex3f(x + half, y + half, z + half)
        glVertex3f(x + half, y - half, z + half)
        glEnd()

    # Left face:
    if colors.get('L', (0,0,0)) != (0,0,0):
        glBegin(GL_QUADS)
        glColor3fv(colors['L'])
        glVertex3f(x - half, y - half, z - half)
        glVertex3f(x - half, y - half, z + half)
        glVertex3f(x - half, y + half, z + half)
        glVertex3f(x - half, y + half, z - half)
        glEnd()

# ---------------------- HÀM VẼ RUBIK FULL (Ánh xạ theo vị trí) ----------------------
def draw_rubik_full(cube_state):
    size = 1.0
    spacing = 1.02
    face_mapping = {
        'F': cube_state['F'],
        'B': cube_state['B'],
        'U': cube_state['U'],
        'D': cube_state['D'],
        'R': cube_state['R'],
        'L': cube_state['L']
    }

    for i in [-1, 0, 1]:
        for j in [-1, 0, 1]:
            for k in [-1, 0, 1]:
                x, y, z = i * spacing, j * spacing, k * spacing
                colors = {}

                # Front face (F): hướng +Z
                if k == 1:
                    colors['F'] = get_gl_color(face_mapping['F'][2 - (j + 1)][i + 1])

                # Back face (B): hướng -Z
                if k == -1:
                    colors['B'] = get_gl_color(face_mapping['B'][2 - (j + 1)][1 - i])

                # Up face (U): ĐÃ FIX CHUẨN 100%
                if j == 1:
                    colors['U'] = get_gl_color(face_mapping['U'][1 + k][i + 1])

                # Down face (D): ĐÃ FIX CHUẨN 100%
                if j == -1:
                    colors['D'] = get_gl_color(face_mapping['D'][1 - k][i + 1])

                # Right face (R): hướng +X
                if i == 1:
                    colors['R'] = get_gl_color(face_mapping['R'][2 - (j + 1)][2 - (k + 1)])

                # Left face (L): hướng -X
                if i == -1:
                    colors['L'] = get_gl_color(face_mapping['L'][2 - (j + 1)][k + 1])

                draw_cubie(x, y, z, size, colors)

def rotate_face_clockwise(face):
    return [[face[2 - j][i] for j in range(3)] for i in range(3)]

def rotate_face_counterclockwise(face):
    return [[face[j][2 - i] for j in range(3)] for i in range(3)]

# ——— U (xoay mặt U theo chiều kim đồng hồ) ———
def U(cube_state):
    # 1) Xoay mặt U
    cube_state['U'] = rotate_face_clockwise(cube_state['U'])
    # 2) Lưu hàng 0 của F, L, B, R
    f1,f2,f3 = cube_state['F'][0][0], cube_state['F'][0][1], cube_state['F'][0][2]
    l1,l2,l3 = cube_state['L'][0][0], cube_state['L'][0][1], cube_state['L'][0][2]
    b1,b2,b3 = cube_state['B'][0][0], cube_state['B'][0][1], cube_state['B'][0][2]
    r1,r2,r3 = cube_state['R'][0][0], cube_state['R'][0][1], cube_state['R'][0][2]
    # 3) F→L→B→R→F
    cube_state['L'][0][0], cube_state['L'][0][1], cube_state['L'][0][2] = f1, f2, f3
    cube_state['B'][0][0], cube_state['B'][0][1], cube_state['B'][0][2] = l1, l2, l3
    cube_state['R'][0][0], cube_state['R'][0][1], cube_state['R'][0][2] = b1, b2, b3
    cube_state['F'][0][0], cube_state['F'][0][1], cube_state['F'][0][2] = r1, r2, r3
    # 4) Vẽ lại
    draw_rubik_full(cube_state)

# ——— U′ (xoay mặt U ngược chiều kim đồng hồ) ———
def U_prime(cube_state):
    cube_state['U'] = rotate_face_counterclockwise(cube_state['U'])
    f1,f2,f3 = cube_state['F'][0][0], cube_state['F'][0][1], cube_state['F'][0][2]
    l1,l2,l3 = cube_state['L'][0][0], cube_state['L'][0][1], cube_state['L'][0][2]
    b1,b2,b3 = cube_state['B'][0][0], cube_state['B'][0][1], cube_state['B'][0][2]
    r1,r2,r3 = cube_state['R'][0][0], cube_state['R'][0][1], cube_state['R'][0][2]
    # Ngược chu trình F←R←B←L←F
    cube_state['L'][0][0], cube_state['L'][0][1], cube_state['L'][0][2] = b1, b2, b3
    cube_state['B'][0][0], cube_state['B'][0][1], cube_state['B'][0][2] = r1, r2, r3
    cube_state['R'][0][0], cube_state['R'][0][1], cube_state['R'][0][2] = f1, f2, f3
    cube_state['F'][0][0], cube_state['F'][0][1], cube_state['F'][0][2] = l1, l2, l3
    draw_rubik_full(cube_state)

# ——— U2 (xoay mặt U 180°) ———
def U2(cube_state):
    U(cube_state)
    U(cube_state)
    draw_rubik_full(cube_state)

# ——— R (xoay mặt R theo chiều kim đồng hồ) ———
def R(cube_state):
    # 1) Xoay mặt R
    cube_state['R'] = rotate_face_clockwise(cube_state['R'])
    # 2) Lưu các ô biên
    f9,f6,f3 = cube_state['F'][2][2], cube_state['F'][1][2], cube_state['F'][0][2]
    u3,u6,u9 = cube_state['U'][2][2], cube_state['U'][1][2], cube_state['U'][0][2]
    b7,b4,b1 = cube_state['B'][2][0], cube_state['B'][1][0], cube_state['B'][0][0]
    d3,d6,d9 = cube_state['D'][2][2], cube_state['D'][1][2], cube_state['D'][0][2]
    # 3) F→U→B→D→F
    cube_state['D'][0][2], cube_state['D'][1][2], cube_state['D'][2][2] = b7,b4,b1
    cube_state['F'][0][2], cube_state['F'][1][2], cube_state['F'][2][2] = d9,d6,d3
    cube_state['U'][0][2], cube_state['U'][1][2], cube_state['U'][2][2] = f3,f6,f9
    cube_state['B'][0][0], cube_state['B'][1][0], cube_state['B'][2][0] = u3,u6,u9
    draw_rubik_full(cube_state)

# ——— R′ (xoay mặt R ngược chiều kim đồng hồ) ———
def R_prime(cube_state):
    cube_state['R'] = rotate_face_counterclockwise(cube_state['R'])
    f3,f6,f9 = cube_state['F'][0][2], cube_state['F'][1][2], cube_state['F'][2][2]
    u9,u6,u3 = cube_state['U'][0][2], cube_state['U'][1][2], cube_state['U'][2][2]
    b1,b4,b7 = cube_state['B'][0][0], cube_state['B'][1][0], cube_state['B'][2][0]
    d9,d6,d3 = cube_state['D'][0][2], cube_state['D'][1][2], cube_state['D'][2][2]
    # Ngược chu trình F←D←B←U←F
    cube_state['D'][0][2], cube_state['D'][1][2], cube_state['D'][2][2] = f3,f6,f9
    cube_state['F'][0][2], cube_state['F'][1][2], cube_state['F'][2][2] = u9,u6,u3
    cube_state['U'][0][2], cube_state['U'][1][2], cube_state['U'][2][2] = b7,b4,b1
    cube_state['B'][0][0], cube_state['B'][1][0], cube_state['B'][2][0] = d3,d6,d9
    draw_rubik_full(cube_state)

# ——— R2 (xoay mặt R 180°) ———
def R2(cube_state):
    R(cube_state)
    R(cube_state)
    draw_rubik_full(cube_state)

# ——— Xoay mặt F theo chiều kim đồng hồ (F) ———
def F(cube_state):
    # 1) Xoay mặt F
    cube_state['F'] = rotate_face_clockwise(cube_state['F'])
    # 2) Lưu cạnh
    u7,u8,u9 = cube_state['U'][2][2], cube_state['U'][2][1], cube_state['U'][2][0]
    r1,r4,r7 = cube_state['R'][0][0], cube_state['R'][1][0], cube_state['R'][2][0]
    d1,d2,d3 = cube_state['D'][0][2], cube_state['D'][0][1], cube_state['D'][0][0]
    l3,l6,l9 = cube_state['L'][0][2], cube_state['L'][1][2], cube_state['L'][2][2]
    # 3) Gán theo chu trình U→R→D→L→U
    cube_state['L'][0][2], cube_state['L'][1][2], cube_state['L'][2][2] = d3, d2, d1
    cube_state['D'][0][2], cube_state['D'][0][1], cube_state['D'][0][0] = r1, r4, r7
    cube_state['R'][0][0], cube_state['R'][1][0], cube_state['R'][2][0] = u9, u8, u7
    cube_state['U'][2][0], cube_state['U'][2][1], cube_state['U'][2][2] = l9, l6, l3
    draw_rubik_full(cube_state)

# ——— Xoay mặt F ngược chiều kim đồng hồ (F') ———
def F_prime(cube_state):
    cube_state['F'] = rotate_face_counterclockwise(cube_state['F'])
    u7,u8,u9 = cube_state['U'][2][2], cube_state['U'][2][1], cube_state['U'][2][0]
    r1,r4,r7 = cube_state['R'][0][0], cube_state['R'][1][0], cube_state['R'][2][0]
    d1,d2,d3 = cube_state['D'][0][2], cube_state['D'][0][1], cube_state['D'][0][0]
    l3,l6,l9 = cube_state['L'][0][2], cube_state['L'][1][2], cube_state['L'][2][2]
    # Ngược chu trình F ← U ← L ← D ← R ← F
    cube_state['U'][2][0], cube_state['U'][2][1], cube_state['U'][2][2] = r1, r4, r7
    cube_state['R'][0][0], cube_state['R'][1][0], cube_state['R'][2][0] = d1, d2, d3
    cube_state['D'][0][2], cube_state['D'][0][1], cube_state['D'][0][0] = l9, l6, l3
    cube_state['L'][0][2], cube_state['L'][1][2], cube_state['L'][2][2] = u7, u8, u9
    draw_rubik_full(cube_state)

# ——— F2 (xoay 180°) ———
def F2(cube_state):
    F(cube_state)
    F(cube_state)
    draw_rubik_full(cube_state)

# ——— Xoay mặt D theo chiều kim đồng hồ (D) ———
def D(cube_state):
    cube_state['D'] = rotate_face_clockwise(cube_state['D'])
    # Lấy hàng 2 của F, R, B, L
    f7,f8,f9 = cube_state['F'][2][0], cube_state['F'][2][1], cube_state['F'][2][2]
    l7,l8,l9 = cube_state['L'][2][0], cube_state['L'][2][1], cube_state['L'][2][2]
    b7,b8,b9 = cube_state['B'][2][0], cube_state['B'][2][1], cube_state['B'][2][2]
    r7,r8,r9 = cube_state['R'][2][0], cube_state['R'][2][1], cube_state['R'][2][2]
    # 3) F→L→B→R→F
    cube_state['L'][2][0], cube_state['L'][2][1], cube_state['L'][2][2] = b7,b8,b9
    cube_state['B'][2][0], cube_state['B'][2][1], cube_state['B'][2][2] = r7,r8,r9
    cube_state['R'][2][0], cube_state['R'][2][1], cube_state['R'][2][2] = f7,f8,f9
    cube_state['F'][2][0], cube_state['F'][2][1], cube_state['F'][2][2] = l7,l8,l9
    draw_rubik_full(cube_state)

# ——— Xoay mặt D ngược chiều kim đồng hồ (D') ———
def D_prime(cube_state):
    cube_state['D'] = rotate_face_counterclockwise(cube_state['D'])
    f7,f8,f9 = cube_state['F'][2][0], cube_state['F'][2][1], cube_state['F'][2][2]
    l7,l8,l9 = cube_state['L'][2][0], cube_state['L'][2][1], cube_state['L'][2][2]
    b7,b8,b9 = cube_state['B'][2][0], cube_state['B'][2][1], cube_state['B'][2][2]
    r7,r8,r9 = cube_state['R'][2][0], cube_state['R'][2][1], cube_state['R'][2][2]
    # 3) F→L→B→R→F
    cube_state['L'][2][0], cube_state['L'][2][1], cube_state['L'][2][2] = f7,f8,f9
    cube_state['B'][2][0], cube_state['B'][2][1], cube_state['B'][2][2] = l7,l8,l9
    cube_state['R'][2][0], cube_state['R'][2][1], cube_state['R'][2][2] = b7,b8,b9
    cube_state['F'][2][0], cube_state['F'][2][1], cube_state['F'][2][2] = r7,r8,r9
    draw_rubik_full(cube_state)

# ——— D2 (180°) ———
def D2(cube_state):
    D(cube_state)
    D(cube_state)
    draw_rubik_full(cube_state)

# ——— Xoay mặt L theo chiều kim đồng hồ (L) ———
def L(cube_state):
    cube_state['L'] = rotate_face_clockwise(cube_state['L'])
    # Lấy cột 0 của F, U, B, D
    f1,f4,f7 = cube_state['F'][0][0], cube_state['F'][1][0], cube_state['F'][2][0]
    u1,u4,u7 = cube_state['U'][2][0], cube_state['U'][1][0], cube_state['U'][0][0]
    b3,b6,b9 = cube_state['B'][0][2], cube_state['B'][1][2], cube_state['B'][2][2]
    d1,d4,d7 = cube_state['D'][2][0], cube_state['D'][1][0], cube_state['D'][0][0]
    # F→U→B→D→F
    cube_state['U'][0][0], cube_state['U'][1][0], cube_state['U'][2][0] = b9,b6,b3
    cube_state['B'][0][2], cube_state['B'][1][2], cube_state['B'][2][2] = d1,d4,d7
    cube_state['D'][0][0], cube_state['D'][1][0], cube_state['D'][2][0] = f1,f4,f7
    cube_state['F'][0][0], cube_state['F'][1][0], cube_state['F'][2][0] = u7,u4,u1
    draw_rubik_full(cube_state)

# ——— Xoay mặt L ngược chiều kim đồng hồ (L') ———
def L_prime(cube_state):
    cube_state['L'] = rotate_face_counterclockwise(cube_state['L'])
    f1,f4,f7 = cube_state['F'][0][0], cube_state['F'][1][0], cube_state['F'][2][0]
    u1,u4,u7 = cube_state['U'][2][0], cube_state['U'][1][0], cube_state['U'][0][0]
    b3,b6,b9 = cube_state['B'][0][2], cube_state['B'][1][2], cube_state['B'][2][2]
    d1,d4,d7 = cube_state['D'][2][0], cube_state['D'][1][0], cube_state['D'][0][0]
    # F→U→B→D→F
    cube_state['U'][0][0], cube_state['U'][1][0], cube_state['U'][2][0] = f1,f4,f7
    cube_state['B'][0][2], cube_state['B'][1][2], cube_state['B'][2][2] = u1,u4,u7
    cube_state['D'][0][0], cube_state['D'][1][0], cube_state['D'][2][0] = b9,b6,b3
    cube_state['F'][0][0], cube_state['F'][1][0], cube_state['F'][2][0] = d7,d4,d1
    draw_rubik_full(cube_state)

# ——— L2 (180°) ———
def L2(cube_state):
    L(cube_state)
    L(cube_state)
    draw_rubik_full(cube_state)

# ——— Xoay mặt B theo chiều kim đồng hồ (B) ———
def B(cube_state):
    cube_state['B'] = rotate_face_clockwise(cube_state['B'])
    u3,u2,u1 = cube_state['U'][0][0], cube_state['U'][0][1], cube_state['U'][0][2]
    r9,r6,r3 = cube_state['R'][0][2], cube_state['R'][1][2], cube_state['R'][2][2]
    d9,d8,d7 = cube_state['D'][2][0], cube_state['D'][2][1], cube_state['D'][2][2]
    l7,l4,l1 = cube_state['L'][0][0], cube_state['L'][1][0], cube_state['L'][2][0]
    # U→R→D→L→U
    cube_state['R'][0][2], cube_state['R'][1][2], cube_state['R'][2][2] = d7,d8,d9
    cube_state['D'][2][0], cube_state['D'][2][1], cube_state['D'][2][2] = l7,l4,l1
    cube_state['L'][0][0], cube_state['L'][1][0], cube_state['L'][2][0] = u1,u2,u3
    cube_state['U'][0][0], cube_state['U'][0][1], cube_state['U'][0][2] = r9,r6,r3
    draw_rubik_full(cube_state)

# ——— Xoay mặt B ngược chiều kim đồng hồ (B') ———
def B_prime(cube_state):
    cube_state['B'] = rotate_face_counterclockwise(cube_state['B'])
    u3,u2,u1 = cube_state['U'][0][0], cube_state['U'][0][1], cube_state['U'][0][2]
    r9,r6,r3 = cube_state['R'][0][2], cube_state['R'][1][2], cube_state['R'][2][2]
    d9,d8,d7 = cube_state['D'][2][0], cube_state['D'][2][1], cube_state['D'][2][2]
    l7,l4,l1 = cube_state['L'][0][0], cube_state['L'][1][0], cube_state['L'][2][0]
    # U→R→D→L→U
    cube_state['R'][0][2], cube_state['R'][1][2], cube_state['R'][2][2] = u3,u2,u1
    cube_state['D'][2][0], cube_state['D'][2][1], cube_state['D'][2][2] = r3,r6,r9
    cube_state['L'][0][0], cube_state['L'][1][0], cube_state['L'][2][0] = d9,d8,d7
    cube_state['U'][0][0], cube_state['U'][0][1], cube_state['U'][0][2] = l1,l4,l7
    draw_rubik_full(cube_state)

# ——— B2 (180°) ———
def B2(cube_state):
    B(cube_state)
    B(cube_state)
    draw_rubik_full(cube_state)

def load_state(file_path):
     # Khởi tạo trạng thái ban đầu cho Rubik
    cube_state = {
        'U': [['unknown' for _ in range(3)] for _ in range(3)],
        'R': [['unknown' for _ in range(3)] for _ in range(3)],
        'F': [['unknown' for _ in range(3)] for _ in range(3)],
        'D': [['unknown' for _ in range(3)] for _ in range(3)],            
        'L': [['unknown' for _ in range(3)] for _ in range(3)],
        'B': [['unknown' for _ in range(3)] for _ in range(3)]
    }

    with open(file_path, 'r') as file:
        lines = file.readlines()
        faces = ['U', 'R', 'F', 'D', 'L', 'B']
        for idx, line in enumerate(lines):
            row = line.strip().split()
            for i in range(3):
                for j in range(3):
                    cube_state[faces[idx]][i][j] = row[i * 3 + j]

    return cube_state

# ---------------------- THIẾT LẬP CỬA SỔ OPENGL ----------------------
def run_opengl(cube_state):
    pygame.init()
    display = (800, 600)
    pygame.display.set_mode(display, DOUBLEBUF | OPENGL)
    pygame.display.set_caption("Rubik3D")
    gluPerspective(45, (display[0] / display[1]), 0.1, 50.0)
    glTranslatef(0.0, 0.0, -10)

    # Khai báo biến xoay camera
    camera_angle_x = 0.0
    camera_angle_y = 0.0

    file_path = "C:\\Users\\Anhat\\rubik_state.txt"  
    cube_state = load_state(file_path)

    # --- ĐỌC CÁC BƯỚC GIẢI TỪ FILE ---
    solution_path = r"C:\Users\Anhat\solution.txt"
    with open(solution_path, 'r') as f:
        moves = [line.strip() for line in f if line.strip()]
    move_index = 0

    # --- BẢNG ÁNH XẠ TỪ CHUỖI TÊN PHÉP XOAY SANG HÀM ---
    move_functions = {
        "U": U,  "U'": U_prime,  "U2": U2,
        "R": R,  "R'": R_prime,  "R2": R2,
        "F": F,  "F'": F_prime,  "F2": F2,
        "D": D,  "D'": D_prime,  "D2": D2,
        "L": L,  "L'": L_prime,  "L2": L2,
        "B": B,  "B'": B_prime,  "B2": B2,
    }

        
    # ---------------------- VÒNG LẶP CHÍNH VỚI XOAY CAMERA ----------------------
    running = True
    while running:
        for event in pygame.event.get():
            if event.type == QUIT:
                running = False
            elif event.type == KEYDOWN:
                if event.key == K_ESCAPE:
                    running = False
                    
                # Xoay bước kế tiếp mỗi khi nhấn SPACE
                elif event.key == K_SPACE:
                    if move_index < len(moves):
                        mv = moves[move_index]
                        if mv in move_functions:
                            move_functions[mv](cube_state)
                        move_index += 1

        keys = pygame.key.get_pressed()
        if keys[K_LEFT]:
            camera_angle_y -= 1.0
        if keys[K_RIGHT]:
            camera_angle_y += 1.0
        if keys[K_UP]:
            camera_angle_x -= 1.0
        if keys[K_DOWN]:
            camera_angle_x += 1.0

        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
        glEnable(GL_DEPTH_TEST)

        glPushMatrix()
        glRotatef(camera_angle_x, 1, 0, 0)
        glRotatef(camera_angle_y, 0, 1, 0)
        draw_rubik_full(cube_state)
        glPopMatrix()

        pygame.display.flip()
        pygame.time.wait(10)

    pygame.quit()

if __name__ == '__main__':
    run_opengl(cube_state)
    