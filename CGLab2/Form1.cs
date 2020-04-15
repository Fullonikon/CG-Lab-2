using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenGL;
using OpenTK;
using System.IO;

namespace CGLab2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                glControl1.Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Application.Idle += Application_Idle();
        }

        static int clamp (int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        class Bin
        {
            public static int x, y, z;
            public static short[] arr;
            public Bin() { }
            public void readBin(string path)
            {
                if (File.Exists(path))
                {
                    BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
                    x = reader.ReadInt32();
                    y = reader.ReadInt32();
                    z = reader.ReadInt32();
                    int arrSize = x * y * z;
                    arr = new short[arrSize];
                    for (int i=0; i<arrSize; i++)
                    {
                        arr[i] = reader.ReadInt16();
                    }
                }
                return;
            }
        }

        Bin bin = new Bin();
        View view = new View();
        

        class View
        {
            Color TransferFunction(short value)
            {
                int min = 0;
                int max = 2000;
                int newval = clamp((value - min) * 255 / (max - min), 0, 255);
                return Color.FromArgb(255, newval, newval, newval);
            }
            public void SetupView(int width, int height)
            {
                Gl.ShadeModel(OpenGL.ShadingModel.Smooth);
                Gl.MatrixMode(OpenGL.MatrixMode.Projection);
                Gl.LoadIdentity();
                Gl.Ortho(0, Bin.x, 0, Bin.y, -1, 1);
                Gl.Viewport(0, 0, width, height);
            }

            public void DrawQuads(int layerNumber)
            {
                Gl.Clear(OpenGL.ClearBufferMask.ColorBufferBit | OpenGL.ClearBufferMask.DepthBufferBit);
                Gl.Begin(PrimitiveType.Quads);
                for (int x_coord = 0; x_coord<Bin.x-1; x_coord++)
                {
                    for (int y_coord = 0; y_coord < Bin.y; y_coord++)
                    {
                        short value;
                        value = Bin.arr[x_coord + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                        Gl.Color3(TransferFunction(value).R, TransferFunction(value).G, TransferFunction(value).B);
                        Gl.Vertex2(x_coord, y_coord);

                        value = Bin.arr[x_coord + (y_coord + 1) * Bin.x + layerNumber * Bin.x * Bin.y];
                        Gl.Color3(TransferFunction(value).R, TransferFunction(value).G, TransferFunction(value).B);
                        Gl.Vertex2(x_coord, y_coord + 1);

                        value = Bin.arr[x_coord + 1 + (y_coord + 1) * Bin.x + layerNumber * Bin.x * Bin.y];
                        Gl.Color3(TransferFunction(value).R, TransferFunction(value).G, TransferFunction(value).B);
                        Gl.Vertex2(x_coord + 1, y_coord + 1);

                        value = Bin.arr[x_coord + 1 + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                        Gl.Color3(TransferFunction(value).R, TransferFunction(value).G, TransferFunction(value).B);
                        Gl.Vertex2(x_coord + 1, y_coord);
                    }
                }
                Gl.End();
            }
        }

        private void glControl2_Load(object sender, EventArgs e)
        {
            trackBar1.Maximum = Bin.z;
        }

        bool loaded = false;
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBin(str);
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }
        int currentLayer = 0;
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
        }

        private void glControl2_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                view.DrawQuads(currentLayer);
                glControl1.SwapBuffers();
            }
        }

        
    }
}
