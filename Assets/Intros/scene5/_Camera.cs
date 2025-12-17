using UnityEngine;
using System.Collections;

public class _Camera : MonoBehaviour {
	private Transform tc;
	public static float angle=0f;
	private float delay=2f;
	private float t=0f;
	private Vector3 st_pos; 
	// Use this for initialization
	void Start () {
	tc= GameObject.Find("tc").transform;
			this.transform.LookAt(tc.position);
		st_pos = this.transform.position;
	}
	void OnGUI(){
		if (GUI.Button(new Rect(10,10,60,20),"MENU")){
			angle=0f;
			Application.LoadLevel(0);//back to menu
			
		}
	}
	
	// Update is called once per frame
	void Update () {
		t+=Time.deltaTime;
		this.transform.position += new Vector3(0f,0f,(-st_pos.z-1.1f+(st_pos.z+1f-this.transform.position.z))/500f);
			this.transform.LookAt(tc.position);
		if (t>delay){
			if (angle<90f){
				
		angle+=(180f-(179f-angle))/20f;
	this.transform.RotateAround(tc.position,Vector3.up,(180f-(179f-angle))/20f);
			}else{
			
				angle+=(180f-angle)/20f;
				this.transform.RotateAround(tc.position,Vector3.up,(180f-angle)/20f);
			}
		
		}
	}
}
