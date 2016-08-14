using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FpsViewBehavior : MonoBehaviour {

    private int count = 0;
    private float time = 0.0f;
    private Text text;

	// Use this for initialization
	void Start () {
        time = Time.realtimeSinceStartup;
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 30);
        text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {

        var now = Time.realtimeSinceStartup;
        var tm = now - time;
        count++;
        if (tm > 1.0f)
        {
            text.text = "fps = " + (count / tm).ToString();
            time = now;
            count = 0;
        }


    }
}
