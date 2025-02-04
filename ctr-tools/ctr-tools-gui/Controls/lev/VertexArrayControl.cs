﻿using CTRFramework;
using CTRFramework.Shared;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CTRTools.Controls
{
    public partial class VertexArrayControl : UserControl
    {
        Random random = new Random();

        public List<Vertex> VertexArray = new List<Vertex>();

        public VertexArrayControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void applyColorsButton_Click(object sender, EventArgs e)
        {
            foreach (var vertex in VertexArray)
            {
                vertex.SetColor(new Vector4b(setMainColorButton.BackColor), Vcolor.Default);
                vertex.SetColor(new Vector4b(setMorphColorButton.BackColor), Vcolor.Morph);
            }
        }

        private void setMainColorButton_Click(object sender, EventArgs e)
        {
            if (cd.ShowDialog() == DialogResult.OK)
                setMainColorButton.BackColor = cd.Color;
        }

        private void setMorphColorButton_Click(object sender, EventArgs e)
        {
            if (cd.ShowDialog() == DialogResult.OK)
                setMorphColorButton.BackColor = cd.Color;
        }

        private void darkenButton_Click(object sender, EventArgs e)
        {
            float red = redSlider.Value / 255f;
            float green = greenSlider.Value / 255f;
            float blue = blueSlider.Value / 255f;

            foreach (var vertex in VertexArray)
            {
                vertex.Color.Scale(red, green, blue, 1f);
                vertex.MorphColor.Scale(red, green, blue, 1f);
            }
        }

        private void mainToMorphButton_Click(object sender, EventArgs e)
        {
            foreach (var vertex in VertexArray)
                vertex.MorphColor = vertex.Color;
        }

        private void rainbowButton_Click(object sender, EventArgs e)
        {
            foreach (var vertex in VertexArray)
            {
                vertex.Color = new Vector4b((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255), 0);
                vertex.MorphColor = vertex.Color;
            }
        }
    }
}
