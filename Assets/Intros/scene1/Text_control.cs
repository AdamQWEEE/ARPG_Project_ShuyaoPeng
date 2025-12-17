using UnityEngine;
using System.Collections;

public class Text_control : MonoBehaviour {
	public static float visible=0f;//transparecy
	public static bool collided;
	private float t=0f;
	private float sp_adj=5f;
	private int count=0;
	private float flicker=2f;
	private float fl2=.4f;
	
	// Use this for initialization
	void Start () {
	}
	void OnGUI(){
		if (GUI.Button(new Rect(10,10,60,20),"MENU")){
			
			Application.LoadLevel(0);//going back to menu
		}
	}
	// Update is called once per frame
	void Update () {
		if (collided==true){
		t+=sp_adj*Time.deltaTime;//timer
		}
	this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r,this.GetComponent<Renderer>().material.color.g,this.GetComponent<Renderer>().material.color.b,visible);
	if (collided==true && t>4f && count <= 3){
		if (t<4f+flicker){
				visible=1f;//transparecy
			} else if(t<4f+flicker+fl2){
				visible=0f;//transparecy
			} else if(t<4f+2*flicker+fl2){
				visible=1f;//transparecy
			} else if(t<4f+2*flicker+2*fl2){
				visible=0f;//transparecy
				t=4.1f;
				count++;
				flicker=flicker/1.5f;
				fl2=fl2/1.2f;
			}
			
			
		}else if (count>3){
			visible=0f;

		}
	}
}
