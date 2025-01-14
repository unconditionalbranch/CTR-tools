﻿using CTRFramework;
using CTRFramework.Shared;
using System;
using System.IO;
using System.Windows.Forms;

namespace CTRTools.Controls
{
    public partial class EmuControl : UserControl
    {
        string path;
        CtrScene scn;

        Mem m;
        Char c;


        public EmuControl()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0xF;
            this.DoubleBuffered = true;

            if (File.Exists("mapping.xml"))
                memstr = MemMapStruct.Load("mapping.xml");
        }

        MemMapStruct memstr;

        private void actionAttach_Click(object sender, EventArgs e)
        {
            m = new Mem("ePSXe");

            if (m.ready)
            {
                uint baseP1ptr = 0;
                uint fpspatch = 0;
                uint fpspatch2 = 0;

                char[] vers1 = m.ReadCharArray(0x8008CFB8, 4);
                char[] vers2 = m.ReadCharArray(0x8008D338, 4);
                char[] vers3 = m.ReadCharArray(0x800903BC, 4);

                if ("ENG\0" == new string(vers1))
                {
                    baseP1ptr = 0x8009900C;
                    fpspatch = 0x80037930;
                    fpspatch2 = 0x8008d2b4;
                    textBox5.Text = "NTSC-U." + "\r\n" + textBox5.Text;
                }
                else if ("ENG\0" == new string(vers2))
                {
                    baseP1ptr = 0x800993CC;
                    textBox5.Text = "PAL." + "\r\n" + textBox5.Text;
                }
                else if ("ENG\0" == new string(vers3))
                {
                    baseP1ptr = 0x8009C4CC;
                    fpspatch = 0x800395f4;
                    fpspatch2 = 0x800906c0;
                    textBox5.Text = "NTSC-J." + "\r\n" + textBox5.Text;
                }
                else
                {
                    textBox5.Text = "Unsupported game/version." + "\r\n" + textBox5.Text;
                    return;
                }

                m.WritePSXUInt16(fpspatch, (ushort)(checkBox1.Checked ? 1 : 2), textBox5);
                m.WritePSXUInt16(fpspatch2, (ushort)(checkBox1.Checked ? 1 : 2), textBox5);

                textBox5.Text = "ePSXe.exe base address: " + m.process.MainModule.BaseAddress.ToString("X8") + "\r\n" + textBox5.Text;

                uint charPtr = m.ReadPSXUInt32(baseP1ptr);
                textBox5.Text = baseP1ptr.ToString("X8") + "\r\n" + textBox5.Text;
                textBox5.Text = charPtr.ToString("X8") + "\r\n" + textBox5.Text;

                if (memstr != null)
                {
                    memstr.BasePointer = (int)charPtr;
                    memstr.Write(m, "curWeapon", comboBox1.SelectedIndex.ToString());
                    memstr.Write(m, "numCharges", numericUpDown2.Value.ToString());
                }

                /*
                using (BinaryReader br = new BinaryReader(new MemoryStream(m.ReadArray(charPtr, 1024))))
                {
                    c = new Char(br);
                    //c.wheelScale = 0x2000;
                    c.curWeapon = (byte)comboBox1.SelectedIndex;
                    c.numCharges = (byte)numericUpDown2.Value;

                    byte[] b = new byte[14 * 4 + 2];
                    using (BinaryWriter bw = new BinaryWriter(new MemoryStream(b)))
                    {
                        c.Write(bw);
                        m.WriteArray(charPtr, b);
                    }
                }
                
                propertyGrid2.SelectedObject = c;

                */
            }
            else
            {
                textBox5.Text = "Failed to find ePSXe process." + "\r\n" + textBox5.Text;
            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            m = new Mem("ePSXe");

            uint levPtr = 0x80083a48;
            uint lev = m.ReadPSXUInt32(levPtr);
            uint size = m.ReadPSXUInt32(lev - 4);
            uint ptrMeshInfo = m.ReadPSXUInt32(lev - 4);

            byte[] meshinfodata = m.ReadArray(ptrMeshInfo, 8 * 4);

            MeshInfo mi;

            using (var br = new BinaryReaderEx(new MemoryStream(meshinfodata)))
            {
                mi = new MeshInfo(br);
            }

            ushort ind = m.ReadPSXUInt16(mi.ptrQuadBlocks.ToUInt32());

            textBox5.Text += ind.ToString("X8");

            m.WritePSXUInt32((uint)(mi.ptrVertices + ind * 16 + 8), 0, textBox5);
            m.WritePSXUInt32((uint)(mi.ptrVertices + ind * 16 + 12), 0, textBox5);


            //m.WriteArray(lev, b);
        }

        Timer timer;
        CtrGameconfig cfg;

        private void GetGameConfig(object sender, EventArgs e)
        {
            if (m == null)
            {
                timer.Stop();
                return;
            }

            cfg = CtrGameconfig.FromStream(new MemoryStream(m.ReadArray(m.ReadPSXUInt32(0x8008d2ac), 9604)));
            propertyGrid2.SelectedObject = cfg;
        }

        private void TimerInit()
        {
            timer = new Timer();
            m = new Mem("ePSXe");
            cfg = CtrGameconfig.FromStream(new MemoryStream(m.ReadArray(m.ReadPSXUInt32(0x8008d2ac), 9604)));
            timer.Interval = 100;
            timer.Tick += GetGameConfig;
            propertyGrid2.SelectedObject = cfg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TimerInit();
            timer.Start();
        }
    }
}
