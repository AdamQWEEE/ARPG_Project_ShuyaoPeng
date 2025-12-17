using UnityEngine;
using System.Collections;

public class back_to_menu4 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	void OnGUI(){
		if (GUI.Button(new Rect(10,10,60,20),"MENU")){
			
			Application.LoadLevel(0);
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
