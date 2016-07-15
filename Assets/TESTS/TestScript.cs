using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

public class TestScript : MonoBehaviour {

	public void PushButton()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = UnityEngine.Application.dataPath + "/Models";
        ofd.Filter = "Sli Files (*.sli) | *.sli";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var _path = ofd.FileName;
            var readBytesArray = File.ReadAllBytes(_path);
            var buffer = new byte[50];
            for (int i = 0; i < 50; i++)
            {
                buffer[i] = readBytesArray[readBytesArray.Length - 100 +i];
            }
            var msg = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                print(msg);
        }
    }
}
