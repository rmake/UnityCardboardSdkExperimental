using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FpsView
{

    public static GameObject Create()
    {
        var go = new GameObject("FpsView");
        var text = go.AddComponent<Text>();
        var fpsViewBehavior = go.AddComponent<FpsViewBehavior>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return go;
    }


}