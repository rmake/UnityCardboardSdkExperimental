using UnityEngine;
using System.Collections;

public class TabbedScrollListBehavior : MonoBehaviour {

    public delegate void CloseFunction();
    public delegate void SelectFunction(string name);

    [System.NonSerialized]
    public CloseFunction CloseContent = null;

    [System.NonSerialized]
    public SelectFunction SelectTab = null;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
