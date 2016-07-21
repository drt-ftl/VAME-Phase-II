﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;

namespace SlicerForm
{
    public partial class SlicerForm : Form
    {
        private bool sliced = false;
        float scale = 100f;

        public SlicerForm()
        {
            InitializeComponent();
        }

        public delegate void ButtonHandler(string _name, bool _toggle);
        public static event ButtonHandler buttonPressed;

        public static void ButtonPressed(string _name, bool _toggle)
        {
            if (buttonPressed != null)
                buttonPressed(_name, _toggle);
        }

        private void SliceButton_Click(object sender, EventArgs e)
        {
            //if (sliced) return;
            ButtonPressed("Slice", true);
            sliced = true;
        }

        private void radioButtonSingleStep_CheckedChanged(object sender, EventArgs e)
        {
            ButtonPressed("Radio_step", true);
        }

        private void radioButtonByGCD_CheckedChanged(object sender, EventArgs e)
        {
            ButtonPressed("Radio_gcd", true);
            var lines = VAME_Manager.pathLines[VAME_Manager.pathHeights[LayerTrackbar.Value]];
            //InspectorL.gcdTimeSlider = lines[lines.Count - 1].step;
        }

        private void None_CheckedChanged(object sender, EventArgs e)
        {
            ButtonPressed("Radio_none", true);
        }

        private void WallThickness_CheckedChanged(object sender, EventArgs e)
        {
            ButtonPressed("Radio_wt", true);
            //cSectionGCD.csMode = cSectionGCD.CsMode.WallThickness;
        }

        private void SlicerEC_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            e.Cancel = true;
            this.Hide();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var h = VAME_Manager.pathHeights[LayerTrackbar.Value];
            if (Sloxelizer2.instance.sloxels.Count > 0)
            {
                LayerTrackbar.Maximum = Sloxelizer2.instance.sloxels.Count - 1;
                var sloxList = Sloxelizer2.instance.sloxels[h];
                if (SloxelNumber.Value > sloxList.Count - 1 || SloxelUpDown.Value > sloxList.Count - 1)
                {
                    SloxelNumber.Value = sloxList.Count - 1;
                    SloxelUpDown.Value = sloxList.Count - 1;
                }
                SloxelUpDown.Minimum = 0;
                SloxelUpDown.Maximum = sloxList.Count - 1;
                SloxelNumber.Minimum = 0;
                SloxelNumber.Maximum = sloxList.Count - 1;
                System.Drawing.Graphics g = panel1.CreateGraphics();
                System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Red);
                System.Drawing.Pen pen2 = new System.Drawing.Pen(System.Drawing.Color.Aquamarine);
                System.Drawing.Pen pen3 = new System.Drawing.Pen(System.Drawing.Color.Bisque);
                var center = new Vector2(panel1.Size.Width / 2, panel1.Size.Height / 2);
                var dim = (0.5f * Sloxelizer2.instance.increment) * scale;
                var readout = "";
                if (ShowGCD.Checked)
                {
                    foreach (var gcdLine in VAME_Manager.pathLines[h])
                    {
                        //if (gcdLine.MovesInX || gcdLine.MovesInZ)
                        //{
                        var _p1 = gcdLine.p1 * scale;
                        var _p2 = gcdLine.p2 * scale;
                        var p1 = new Point((int)_p1.x + (int)center.x, -(int)_p1.z + (int)center.y);
                        var p2 = new Point((int)_p2.x + (int)center.x, -(int)_p2.z + (int)center.y);
                        g.DrawLine(pen2, p1, p2);
                        //}
                    }
                }
                if (ShowBalls.Checked)
                {
                    if (VAME_Manager.crazyBallsByLayer.ContainsKey(h))
                    {
                        foreach (var cb in VAME_Manager.crazyBallsByLayer[h])
                        {
                            var _pos = new Vector2(cb.transform.position.x, cb.transform.position.z) * scale;
                            var pos = new Point((int)_pos.x + (int)center.x, -(int)_pos.y + (int)center.y);
                            g.DrawEllipse(pen3, pos.X - 3, pos.Y - 3, 6, 6);
                        }
                    }
                }
                //}
                //if (ShowBalls.Checked)
                //{
                //    foreach (var gcdLine in VAME_Manager.pathLines[VAME_Manager.pathHeights[LayerTrackbar.Value]])
                //    {
                //        foreach (var cb in gcdLine.CrazyBalls)
                //        {
                //            switch (CCAT_Explorer.CCAT_E.ccatExplorerMode)
                //            {
                //                case CCAT_Explorer.CCAT_E.CcatExplorerMode.Error:
                //                    var color = cb.GetColor();
                //                    if (color.r < 0) color.r = 0;
                //                    if (color.r > 1) color.r = 1;
                //                    if (color.g < 0) color.g = 0;
                //                    if (color.g > 1) color.g = 1;
                //                    if (color.b < 0) color.b = 0;
                //                    if (color.b > 1) color.b = 1;
                //                    if (color.a < 0) color.a = 0;
                //                    if (color.a > 1) color.a = 1;
                //                    pen3.Color = System.Drawing.Color.FromArgb((int)(color.a * 255), (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
                //                    break;
                //                case CCAT_Explorer.CCAT_E.CcatExplorerMode.Temperature:
                //                    var temp = cb.Temperature;
                //                    var _r = (temp - 2000) / 1000f;
                //                    var r = (int)(_r * 255);
                //                    if (r < 0) r = 0;
                //                    if (r > 255) r = 255;
                //                    var _b = (2750 - temp) / 2000f;
                //                    var b = (int)(_r * 255);
                //                    if (b < 0) b = 0;
                //                    if (b > 255) b = 255;
                //                    pen3.Color = System.Drawing.Color.FromArgb(255, r, 0, b);
                //                    break;
                //                default:
                //                    break;
                //            }
                //            var _pos = new Vector2(cb.transform.position.x, cb.transform.position.z) * scale;
                //            var pos = new Point((int)_pos.x + (int)center.x, -(int)_pos.y + (int)center.y);
                //            g.DrawEllipse(pen3, pos.X - 3, pos.Y - 3, 6, 6);
                //        }
                //    }
                //}
                if (ShowSloxels.Checked)
                {
                    var hSlox = sloxList[SloxelNumber.Value];
                    foreach (var sloxel in sloxList)
                    {
                        if (sloxel == hSlox) continue;
                        //if (!sloxel.Voxel.singleCube.partOfHighlightedSet)
                        {
                            var a = 255;
                            var r = 255;
                            var gr = 0;
                            var b = 0;
                            var col = System.Drawing.Color.FromArgb(a, r, gr, b);
                            p = new System.Drawing.Pen(col);
                            var x = center.x + sloxel.origin.x * scale;
                            var y = center.y - sloxel.origin.z * scale;
                            g.DrawRectangle(p, x - dim, y - dim, dim * 2, dim * 2);
                        }
                    }
                    //foreach (var sloxel in sloxList)
                    //{
                    //    if (sloxel == hSlox) continue;
                    //    if (sloxel.Voxel.singleCube.partOfHighlightedSet)
                    //    {

                    //        var r = 120;
                    //        var gr = 60;
                    //        var b = 255;
                    //        var col = System.Drawing.Color.FromArgb(r, gr, b);
                    //        p = new System.Drawing.Pen(col);
                    //        var x = center.x + sloxel.origin.x * scale;
                    //        var y = center.y - sloxel.origin.z * scale;
                    //        g.DrawRectangle(p, x - dim, y - dim, dim * 2, dim * 2);
                    //    }
                    //}
                    p = new System.Drawing.Pen(System.Drawing.Color.Yellow);
                    SolidBrush sbH = new SolidBrush(System.Drawing.Color.Red);
                    var xH = center.x + hSlox.origin.x * scale;
                    var yH = center.y - hSlox.origin.z * scale;
                    g.DrawRectangle(p, xH - dim, yH - dim, dim * 2, dim * 2);
                    //InspectorR.highlight = hSlox.Voxel.Id;
                    var sloxelInVoxel = hSlox.Voxel.Sloxels.IndexOf(hSlox) + 1;
                    var sloxCount = hSlox.Voxel.Sloxels.Count;
                    readout += "\r\n";
                    readout += "Sloxel #: " + sloxelInVoxel.ToString() + " / " + sloxCount.ToString() + "\r\n";
                    readout += "Voxel #: " + Sloxelizer2.instance.voxels.IndexOf(hSlox.Voxel).ToString() + "\r\n";
                    readout += "Layer #: " + h.ToString() + "\r\n";
                    //readout += "Wall Thickness : " + hSlox.WallThickness.ToString() + "\r\n";
                    if (hSlox.PathLines.Count == 1)
                        readout += "Intersected By 1 Line. \r\n";
                    else
                        readout += "Intersected By " + hSlox.PathLines.Count.ToString() + " Lines. \r\n";
                    foreach (var intLine in hSlox.PathLines)
                    {
                        var IntLine = VAME_Manager.pathLines[intLine.x][(int)intLine.y];
                        readout += "\r\n";
                        readout += "Line " + intLine.x.ToString() + "\r\n";
                        readout += "p1: " + IntLine.p1.ToString("f4") + "\r\n";
                        readout += "p2: " + IntLine.p2.ToString("f4") + "\r\n";
                    }
                }
                //    if (ShowCsection.Checked)
                //    {
                //        foreach (var border in cSectionGCD.layers[cSectionGCD.layerHeights[LayerTrackbar.Value]].border)
                //        {
                //            wtLabel.Text = "Wall Thickness: " + (wtSlider.Value / 100f).ToString("f4");
                //            var minWT = wtSlider.Value / 100f;
                //            var r = 0;
                //            var gr = 0;
                //            var b = 255;
                //            if (border.WallThickness < minWT)
                //            {
                //                r = (int)((1.0f - border.WallThickness / minWT) * 255f);
                //                b = (int)((border.WallThickness / minWT) * 255f);
                //            }
                //            var col = System.Drawing.Color.FromArgb(r, gr, b);
                //            p = new System.Drawing.Pen(col);
                //            var x0 = (int)(center.x + border.Endpoint0.x * scale);
                //            var y0 = (int)(center.y - border.Endpoint0.z * scale);
                //            var x1 = (int)(center.x + border.Endpoint1.x * scale);
                //            var y1 = (int)(center.y - border.Endpoint1.z * scale);
                //            g.DrawLine(p, new Point(x0, y0), new Point(x1, y1));
                //        }                    
                //    }
                //    var pCam = new System.Drawing.Pen(System.Drawing.Color.LimeGreen);
                //    var camPos = new Vector3 (InspectorT.CamPos().x, InspectorT.CamPos().y, 0);
                //    camPos = Vector3.Normalize(camPos);
                //    camPos.x = center.x + camPos.x * 200;
                //    camPos.y = center.y - camPos.y * 200;
                //    g.DrawEllipse(pCam, new Rectangle((int)camPos.x - 5, (int)camPos.y - 5, 10, 10));
                //    SloxelReadout.Text = readout;
            }
            }

        private void LayerTrackbar_Scroll(object sender, EventArgs e)
        {
            LayerUpDown.Value = LayerTrackbar.Value;
            var lines = VAME_Manager.pathLines[VAME_Manager.pathHeights[LayerTrackbar.Value]];
            //if (cSectionGCD.csMode == cSectionGCD.CsMode.ByGcdCode)
            //    InspectorL.gcdTimeSlider = lines[lines.Count - 1].step;
            panel1.Invalidate();
        }

        private void SloxelNumber_Scroll(object sender, EventArgs e)
        {
            SloxelUpDown.Value = SloxelNumber.Value;
            panel1.Invalidate();
        }

        private void ShowSloxels_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void ShowGCD_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void LayerUpDown_ValueChanged(object sender, EventArgs e)
        {
            LayerTrackbar.Value = (int)LayerUpDown.Value;
            var lines = VAME_Manager.pathLines[VAME_Manager.pathHeights[LayerTrackbar.Value]];
            //if (cSectionGCD.csMode == cSectionGCD.CsMode.ByGcdCode)
            //    InspectorL.gcdTimeSlider = lines[lines.Count - 1].step;
            panel1.Invalidate();
        }

        private void SloxelUpDown_ValueChanged(object sender, EventArgs e)
        {
            SloxelNumber.Value = (int)SloxelUpDown.Value;
            panel1.Invalidate();
        }

        private void panel1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }
        private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var sloxList = Sloxelizer2.instance.sloxels[VAME_Manager.pathHeights[LayerTrackbar.Value]];
            var mPos = e.Location;
            var dim = (0.5f * Sloxelizer2.instance.increment) * scale;
            var center = new Vector2(panel1.Size.Width / 2, panel1.Size.Height / 2);
            foreach (var sloxel in sloxList)
            {
                var sPosRaw = new Vector2(sloxel.origin.x, -sloxel.origin.z);
                var sPos = sPosRaw * scale + center;
                if (mPos.X > sPos.x - dim && mPos.X < sPos.x + dim
                    && mPos.Y > sPos.y - dim && mPos.Y < sPos.y + dim)
                {
                    SloxelNumber.Value = sloxList.IndexOf(sloxel);
                    //InspectorR.highlightVector = sloxel.Voxel.Origin;
                    panel1.Invalidate();
                }
            }
        }

        private void ShowCsection_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void wtSlider_Scroll(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            //if (LoadFile.playback)
            //    LoadFile.playback = false;
            //else
            //{
            //    LoadFile.playbackStartTime = UnityEngine.Time.realtimeSinceStartup;
            //    LoadFile.playback = true;
            //}
        }

        private void ShowBalls_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }
    }
}
