using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ccatInterpreter
{
    public static ccatInterpreter instance;
    public static string filename = "";
    public List<CcatDataPoint> points = new List<CcatDataPoint>();
    public float StartTime = 0;
    public float stepsize = 0;
    public float runningDistance = 0;
    public Vector3 MinVector = Vector3.one * 1000000;
    public Vector3 MaxVector = Vector3.one * -1000000;
    private Vector3 Centroid = Vector3.zero;


    public ccatInterpreter (string _path)
    {
        instance = this;
        var tr = File.OpenText(_path);
        bool afterFirst = false;
        while (!tr.EndOfStream)
        {
            var textLine = tr.ReadLine();
            VAME_Manager.cctCode.Add(textLine + "\n");
            var split = textLine.Split(',');
            if (afterFirst)
            {
                float t;
                float x;
                float y;
                float z;
                if (float.TryParse(split[0], out t))
                {
                    var v = Vector3.zero;
                    t /= 1000; // Scale To Seconds (from ms)
                    if (float.TryParse(split[0], out x) && float.TryParse(split[1], out z) && float.TryParse(split[2], out y))
                    {
                        v.x = x;
                        v.y = y;
                        v.z = z;
                        v /= 101600f; // Scale to inches (from turns)
                        var newPoint = new CcatDataPoint();
                        newPoint.Position = v;
                        if (points.Count == 0)
                        {
                            StartTime = t; // Set start time
                        }
                        t -= StartTime; // set so start is 0
                        newPoint.EventTime = t;
                        points.Add(newPoint);
                        if (points.Count > 1163)
                        {
                            var index = points.Count - 1;
                            runningDistance += Vector3.Distance(points[index].Position, points[index - 1].Position);
                        }
                        if (v.x < MinVector.x)
                            MinVector.x = v.x;
                        if (v.x > MaxVector.x)
                            MaxVector.x = v.x;
                        if (v.y < MinVector.y)
                            MinVector.y = v.y;
                        if (v.y > MaxVector.y)
                            MaxVector.y = v.y;
                        if (v.z < MinVector.z)
                            MinVector.z = v.z;
                        if (v.z > MaxVector.z)
                            MaxVector.z = v.z;
                    }
                }

            }
            afterFirst = true;
        }
        tr.Close();
        //stepsize = runningDistance / (points.Count - 1163);
        var smallStr = "";
        smallStr += points.Count.ToString() + " Points.\r\n";
        VAME_Manager.instance.CrazyBalls();
        if (VAME_Manager.cctCode.Count > 0)
            InspectorL.instance.btnPoints.interactable = true;
    }
    //private void UpDown_PointsPerSample_ValueChanged(object sender, EventArgs e)
    //{

    //}

    //private void SaveButton_Click(object sender, EventArgs e)
    //{
    //    SaveData();
    //}

//    void SaveData()
//    {
//        var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
//        saveFileDialog.InitialDirectory = "C://";
//        var sel = "CCAT Files (*.cct)|*.cct";
//        saveFileDialog.Filter = sel;
//        saveFileDialog.RestoreDirectory = true;
//        saveFileDialog.InitialDirectory = "C:\\Users\\David\\Documents\\GitHub\\VAME\\Assets\\Samples";
//        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//        {
//            stepsize *= (float)UpDown_PointsPerSample.Value;
//            var numPointsToSave = Mathf.FloorToInt((points.Count - 1163) / (float)UpDown_PointsPerSample.Value);
//            var distance = 1f;
//            var tw = File.CreateText(saveFileDialog.FileName);
//            tw.WriteLine("X,Y,Z,Temp,Layer");
//            tw.WriteLine("STEPSIZE:" + stepsize.ToString("f4"));
//            tw.WriteLine("DISTANCE:" + runningDistance.ToString("f4"));
//            tw.WriteLine("POINTS:" + numPointsToSave.ToString());
//            var height = 0f;
//            int layer = 0;
//            for (int i = 1163; i <= points.Count; i += (int)UpDown_PointsPerSample.Value)
//            {
//                if (i == 1163)
//                {
//                    height = points[1163].Position.y;
//                }
//                if (i < points.Count)
//                {
//                    var point = points[i];
//                    if (Mathf.Abs(point.Position.y - height) > 0.04f)
//                    {
//                        height = point.Position.y;
//                        layer++;
//                    }
//                    var p = point.Position - Centroid;
//                    tw.WriteLine(p.x.ToString("f4") + "," + p.y.ToString("f4") + "," + p.z.ToString("f4") + ", 3000" + "," + layer.ToString());
//                }
//            }
//            tw.Close();
//        }
//    }
}

public class CcatDataPoint
{
    public Vector3 Position { get; set; }
    public float EventTime { get; set; }
    public float TimeFactor { get; set; }
    public int LineInCode { get; set; }
}
