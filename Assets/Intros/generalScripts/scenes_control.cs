using UnityEngine;
using System.Collections;

public class scenes_control : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	
	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width/2-50f,50,100,20),"scene 1")){
			
			Application.LoadLevel(1);//loading levels
		}
		
			if (GUI.Button(new Rect(Screen.width/2-50f,100,100,20),"scene 2")){
			
				Application.LoadLevel(2);//loading levels
		}
		
			if (GUI.Button(new Rect(Screen.width/2-50f,150,100,20),"scene 3")){
			
				Application.LoadLevel(3);//loading levels
		}
		
		
			if (GUI.Button(new Rect(Screen.width/2-50f,200,100,20),"scene 4")){
			
				Application.LoadLevel(4);//loading levels
		}
		
			if (GUI.Button(new Rect(Screen.width/2-50f,250,100,20),"scene 5")){
			
				Application.LoadLevel(5);//loading levels
		}
		
	}
	// Update is called once per frame
	void Update () {
	
	}
}
