using System;
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
        public enum SlicerCcatMode { None, DistanceError, TimeError, Temperature}
        public SlicerCcatMode slicerCcatMode;
        public enum SelectByMode { None, Sloxel, PathLine, DataPoint }
        public SelectByMode selectByMode;
        public GameObject selectedBall;
        public PathLine selectedPathLine;
        public Sloxel selectedSloxel;
        public float h = 0;

        public SlicerForm()
        {
            InitializeComponent();
            slicerCcatMode = SlicerCcatMode.None;
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
            var lines = VAME_Manager.pathLines[h];
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
            h = VAME_Manager.pathHeights[LayerTrackbar.Value];
            if (Sloxelizer2.instance.sloxels.Count > 0)
            {
                var defBallColor = System.Drawing.Color.Goldenrod;
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
                System.Drawing.Pen pen3 = new System.Drawing.Pen(defBallColor);
                var center = new Vector2(panel1.Size.Width / 2, panel1.Size.Height / 2);
                var dim = (0.5f * Sloxelizer2.instance.increment) * scale;
                var readout = "";
                if (ShowGCD.Checked)
                {
                    foreach (var gcdLine in VAME_Manager.pathLines[h])
                    {

                        var _p1 = gcdLine.p1 * scale;
                        var _p2 = gcdLine.p2 * scale;
                        var p1 = new Point((int)_p1.x + (int)center.x, -(int)_p1.z + (int)center.y);
                        var p2 = new Point((int)_p2.x + (int)center.x, -(int)_p2.z + (int)center.y);
                        if (selectedPathLine == null || gcdLine != selectedPathLine)
                        {
                            pen2.Color = System.Drawing.Color.Aquamarine;
                        }
                        else
                        {
                            pen2.Color = System.Drawing.Color.FloralWhite;
                            readout = "Layer: " + h.ToString("f3") + "\n" +
                                "Line: " + VAME_Manager.pathLines[h].IndexOf(gcdLine).ToString() + "\n";
                        }
                        g.DrawLine(pen2, p1, p2);
                    }
                }
                if (ShowBalls.Checked)
                {
                    if (VAME_Manager.crazyBallsByLayer.ContainsKey(h))
                    {
                        foreach (var cb in VAME_Manager.crazyBallsByLayer[h])
                        {
                            if (cb != selectedBall)
                            {
                                switch (slicerCcatMode)
                                {
                                    case SlicerCcatMode.None:
                                        pen3.Color = defBallColor;
                                        break;
                                    case SlicerCcatMode.DistanceError:
                                        var c = System.Drawing.Color.FromArgb(0, 255, 0, 10);
                                        if (cb.GetComponent<ccatBall>().Distance < 0)
                                        {
                                            c = System.Drawing.Color.FromArgb(255, 0, 0, 255);
                                        }
                                        else
                                        {
                                            var d = cb.GetComponent<ccatBall>().Distance;
                                            var _c = (int)(d * 0.25f * 255);
                                            if (_c > 254) _c = 254;
                                            c = System.Drawing.Color.FromArgb(255, _c, 255 - _c, 0);
                                        }
                                        pen3.Color = c;
                                        break;
                                    case SlicerCcatMode.TimeError:
                                        //var t = cb.GetComponent<ccatBall>().TimeError;
                                        //var c = System.Drawing.Color.FromArgb(255, (int)(t * 400), 10, 10);
                                        pen3.Color = defBallColor;
                                        break;
                                    case SlicerCcatMode.Temperature:
                                        pen3.Color = defBallColor;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                var layer = VAME_Manager.crazyBallsByLayer[h];
                                var thisIndex = layer.IndexOf(selectedBall);
                                var ccb = selectedBall.GetComponent<ccatBall>();
                                pen3.Color = System.Drawing.Color.White;
                                readout = "DataPoint[" + h.ToString("f3") + "][" + thisIndex.ToString() + "]\n" +
                                    "DitanceError: " + ccb.Distance.ToString() + "\n" +
                                    "Time Error: " + "\n" +
                                    "Temperature: " + "\n";
                            }
                            var _pos = new Vector2(cb.transform.position.x, cb.transform.position.z) * scale;
                            var pos = new Point((int)_pos.x + (int)center.x, -(int)_pos.y + (int)center.y);
                            g.DrawEllipse(pen3, pos.X - 3, pos.Y - 3, 6, 6);
                        }
                    }
                }
                
                if (ShowSloxels.Checked)
                {
                    var a = 255;
                    var r = 255;
                    var gr = 0;
                    var b = 0;
                    var col = System.Drawing.Color.FromArgb(a, r, gr, b);
                    p = new System.Drawing.Pen(col);
                    foreach (var sloxel in sloxList)
                    {
                        var x = center.x + sloxel.origin.x * scale;
                        var y = center.y - sloxel.origin.z * scale;
                        if (sloxel != selectedSloxel)
                        //if (!sloxel.Voxel.singleCube.partOfHighlightedSet)
                        {
                            p.Color = System.Drawing.Color.Red;
                            g.DrawRectangle(p, x - dim, y - dim, dim * 2, dim * 2);
                        }
                    }
                    if (sloxList.Contains(selectedSloxel))
                    {
                        var x = center.x + selectedSloxel.origin.x * scale;
                        var y = center.y - selectedSloxel.origin.z * scale;
                        p.Color = System.Drawing.Color.Yellow;
                        g.DrawRectangle(p, x - dim, y - dim, dim * 2, dim * 2);
                        //InspectorR.highlight = hSlox.Voxel.Id;
                        var sloxelInVoxel = selectedSloxel.Voxel.Sloxels.IndexOf(selectedSloxel) + 1;
                        var sloxCount = selectedSloxel.Voxel.Sloxels.Count;
                        readout += "\r\n";
                        readout += "Sloxel #: " + sloxelInVoxel.ToString() + " / " + sloxCount.ToString() + "\r\n";
                        readout += "Voxel #: " + Sloxelizer2.instance.voxels.IndexOf(selectedSloxel.Voxel).ToString() + "\r\n";
                        readout += "Layer #: " + h.ToString() + "\r\n";
                        //readout += "Wall Thickness : " + hSlox.WallThickness.ToString() + "\r\n";
                        if (selectedSloxel.PathLines.Count == 1)
                            readout += "Intersected By 1 Line. \r\n";
                        else
                            readout += "Intersected By " + selectedSloxel.PathLines.Count.ToString() + " Lines. \r\n";
                        foreach (var intLine in selectedSloxel.PathLines)
                        {
                            var IntLine = VAME_Manager.pathLines[intLine.x][(int)intLine.y];
                            readout += "\r\n";
                            readout += "Line " + intLine.x.ToString() + "\r\n";
                            readout += "p1: " + IntLine.p1.ToString("f4") + "\r\n";
                            readout += "p2: " + IntLine.p2.ToString("f4") + "\r\n";
                        }
                    }
                }
                if (ShowCsection.Checked)
                {
                    
                    foreach (var border in cSection.cSectionsGCD[h])
                    {
                        wtLabel.Text = "Wall Thickness: " + (wtSlider.Value / 100f).ToString("f4");
                        var minWT = wtSlider.Value / 100f;
                        var r = 0;
                        var gr = 0;
                        var b = 255;
                        //if (border.WallThickness < minWT)
                        //{
                        //    r = (int)((1.0f - border.WallThickness / minWT) * 255f);
                        //    b = (int)((border.WallThickness / minWT) * 255f);
                        //}
                        var col = System.Drawing.Color.FromArgb(r, gr, b);
                        p = new System.Drawing.Pen(col);
                        var x0 = (int)(center.x + border.p1.x * scale);
                        var y0 = (int)(center.y - border.p1.z * scale);
                        var x1 = (int)(center.x + border.p2.x * scale);
                        var y1 = (int)(center.y - border.p2.z * scale);
                        g.DrawLine(p, new Point(x0, y0), new Point(x1, y1));
                    }
                }
                var pCam = new System.Drawing.Pen(System.Drawing.Color.LimeGreen);
                var camPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
                camPos = Vector3.Normalize(camPos);
                camPos.x = center.x + camPos.x * 200;
                camPos.y = center.y - camPos.y * 200;
                g.DrawEllipse(pCam, new Rectangle((int)camPos.x - 5, (int)camPos.y - 5, 10, 10));
                SloxelReadout.Text = readout;
            }
            }

        private void LayerTrackbar_Scroll(object sender, EventArgs e)
        {
           ChangeLayer(LayerTrackbar.Value, null);            
        }


        private void LayerUpDown_ValueChanged(object sender, EventArgs e)
        {
            ChangeLayer((int)LayerUpDown.Value, null);
        }

        public void ChangeLayer(int layer, Voxel _voxel)
        {
            LayerUpDown.Value = layer;
            LayerTrackbar.Value = layer;
            h = VAME_Manager.pathHeights[layer];
            var lines = VAME_Manager.pathLines[h];
            var old = selectedSloxel;
            if (_voxel != null)
            {
                selectedSloxel = _voxel.Sloxels[0];
            }
            if (Sloxelizer2.instance.sloxels.ContainsKey(h))
            {
                foreach (var sloxel in Sloxelizer2.instance.sloxels[h])
                {
                    if (selectedSloxel != null && sloxel.origin.x == selectedSloxel.origin.x && sloxel.origin.z == selectedSloxel.origin.z)
                        selectedSloxel = sloxel;
                }
            }
            if (selectedSloxel == old) selectedSloxel = null;
            selectedPathLine = null;
            selectedBall = null;
            //if (cSectionGCD.csMode == cSectionGCD.CsMode.ByGcdCode)
            //    InspectorL.gcdTimeSlider = lines[lines.Count - 1].step;
            panel1.Invalidate();
        }

        private void SloxelNumber_Scroll(object sender, EventArgs e)
        {
            SloxelUpDown.Value = SloxelNumber.Value;
            if (Sloxelizer2.instance.sloxels[h].Count <= SloxelNumber.Value) return;
            selectedSloxel = Sloxelizer2.instance.sloxels[h][SloxelNumber.Value];
            selectedPathLine = null;
            selectedBall = null;
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
            var mPos = e.Location;
            var center = new Vector2(panel1.Size.Width / 2, panel1.Size.Height / 2);
            switch (selectByMode)
            {                
                case SelectByMode.PathLine:
                    var lineList = VAME_Manager.pathLines[h];
                    foreach (var line in lineList)
                    {
                        var d = TestLine(mPos, line);
                        if (d < 6)
                        {
                            selectedBall = null;
                            selectedPathLine = line;
                            selectedSloxel = null;
                            panel1.Invalidate();
                        }
                        MonoBehaviour.print(d.ToString("f3"));
                    }
                    break;
                case SelectByMode.DataPoint:
                    var ballList = VAME_Manager.crazyBallsByLayer[h];
                    foreach (var ball in ballList)
                    {
                        var r = 6;
                        var bPosRaw = new Vector2(ball.transform.position.x, -ball.transform.position.z);
                        var bPos = bPosRaw * scale + center;
                        var bDelta = Mathf.Sqrt(Mathf.Pow((mPos.X - bPos.x), 2) + Mathf.Pow((mPos.Y - bPos.y), 2));
                        if (bDelta <= r)
                        {
                            //mouse hit ball!
                            selectedBall = ball;
                            selectedPathLine = null;
                            selectedSloxel = null;
                            panel1.Invalidate();
                        }
                    }
                    break;
                default:
                    var sloxList = Sloxelizer2.instance.sloxels[h];
                    var dim = (0.5f * Sloxelizer2.instance.increment) * scale;
                    foreach (var sloxel in sloxList)
                    {
                        var sPosRaw = new Vector2(sloxel.origin.x, -sloxel.origin.z);
                        var sPos = sPosRaw * scale + center;
                        if (mPos.X > sPos.x - dim && mPos.X < sPos.x + dim
                            && mPos.Y > sPos.y - dim && mPos.Y < sPos.y + dim)
                        {
                            SloxelNumber.Value = sloxList.IndexOf(sloxel);
                            selectedBall = null;
                            selectedPathLine = null;
                            selectedSloxel = sloxel;
                            panel1.Invalidate();

                            sloxel.Voxel.Cube.GetComponent<Info>().SelectVoxel(false);
                        }
                    }
                    break;
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

        private void comboBox_ccatMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_ccatMode.SelectedIndex)
            {
                case 0:
                    slicerCcatMode = SlicerCcatMode.None;
                    break;
                case 1:
                    slicerCcatMode = SlicerCcatMode.DistanceError;
                    break;
                case 2:
                    slicerCcatMode = SlicerCcatMode.TimeError;
                    break;
                case 3:
                    slicerCcatMode = SlicerCcatMode.Temperature;
                    break;
                default:
                    break;
            }
            panel1.Invalidate();
        }

        private void selectBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (selectBy.SelectedIndex)
            {
                case 0:
                    selectByMode = SelectByMode.None;
                    break;
                case 1:
                    selectByMode = SelectByMode.Sloxel;
                    break;
                case 2:
                    selectByMode = SelectByMode.PathLine;
                    break;
                case 3:
                    selectByMode = SelectByMode.DataPoint;
                    break;
                default:
                    break;
            }
            panel1.Invalidate();
        }

        public float TestLine(Point _pt, PathLine testLine)
        {
            var center = new Vector2(panel1.Size.Width / 2, panel1.Size.Height / 2);
            var p1 = new Vector2 (testLine.p1.x, -testLine.p1.z) * scale + center;
            var p2 = new Vector2 (testLine.p2.x, -testLine.p2.z) * scale + center;
            var pt = new Vector2(_pt.X, _pt.Y);
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                var d = Vector2.Distance(p1, pt);
                return d;
            }
            else
            {
                // Calculate the t that minimizes the distance.
                float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) /
                    (dx * dx + dy * dy);

                // See if this represents one of the segment's
                // end points or a point in the middle.
                if (t < 0)
                {
                    var d = Vector2.Distance(p1, pt);
                    return d;
                }
                else if (t > 1)
                {
                    var d = Vector2.Distance(p2, pt);
                    return d;
                }
                else
                {
                    var closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
                    var d = Vector2.Distance(closest, pt);
                    return d;
                }
            }
        }
    }
}
