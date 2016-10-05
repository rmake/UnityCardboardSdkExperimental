using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour {

    private GameObject canvasObject = null;
    private Canvas canvas = null;
    private CanvasScaler canvasScaler = null;
    private GraphicRaycaster graphicRaycaster = null;
    private TabbedScrollListBehavior tabbedScrollList = null;
    

	// Use this for initialization
	void Start () {

        canvasObject = GameObject.Find("Canvas");
        canvas = canvasObject.GetComponent<Canvas>();
        canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        graphicRaycaster = canvasObject.GetComponent<GraphicRaycaster>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(480, 320);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 1.0f;


        var tabbedCallbacks = new Dictionary<string, Dictionary<string, ScrollListView.Callback>>() {

            {"menu",
                new Dictionary<string, ScrollListView.Callback>(){
                    { "Interior", () => {
                        SceneManager.LoadScene("Scenes/Interior", LoadSceneMode.Single);
                    } },
                    { "360 degree photo", () => {
                        SceneManager.LoadScene("Scenes/Main", LoadSceneMode.Single);
                    } },
                    { "google sample", () => {
                        SceneManager.LoadScene("GoogleVR/DemoScenes/HeadsetDemo/DemoScene", LoadSceneMode.Single);
                    } },
                }
            }
        };

        tabbedScrollList = ScrollListView.CreateTabedScroll(new Rect(5, -5, 160, 280), 32, tabbedCallbacks);
        tabbedScrollList.transform.SetParent(canvas.transform, false);

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
