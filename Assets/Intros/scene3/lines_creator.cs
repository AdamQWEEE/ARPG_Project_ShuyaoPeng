using UnityEngine;
using System.Collections;

public class lines_creator : MonoBehaviour {
	public GameObject lines;
	public GameObject unfocused_lines;
	public GameObject unfocused_lines_2;
		public GameObject transparent;
	public LensFlare lf;
		private Transform text;
	private float t=0f;
	private float delay=0f;
	private bool created=false;
	// Use this for initialization
	void Start () {
		if (this.name=="p_1"){
			
			delay=0f;
		} else if(this.name=="p_2"){
			delay=6f;
		}
		
			text= this.transform.Find("text").transform;
		text.localScale=new Vector3 (0f,0f,1f);
	
	}
	
	// Update is called once per frame
	void Update () {
		
		delay-=Time.deltaTime;
		if (delay<0f){
		if (created==false){
				Create();
			}
		
		
		t+=Time.deltaTime;
		
		
		if (text.localScale.x<1f && t>1f){
			text.localScale+= new Vector3((1.01f-text.transform.localScale.x)/10f,(1.01f-text.transform.localScale.y)/4f,0f);
		}
		
		if (t>.95f && t<1.2f){
			lf.brightness+=.1f;

		}
		
		if (lf.brightness>0f){
			lf.brightness-=.02f;
		}
		}
	}
	
	void Create(){//creating curving lines
		created=true;
		for (int i=0;i<5;i++){
	  GameObject lines_ins = Instantiate(lines,transform.position,transform.rotation) as GameObject;
			
	lines_ins.transform.parent= this.transform;
			lines_ins.name="lines";
		
		GameObject unfocused_lines_ins = Instantiate(unfocused_lines,this.transform.position,this.transform.rotation) as GameObject;
		unfocused_lines_ins.transform.parent= this.transform;
		unfocused_lines_ins.name= "unfocused_lines";
		GameObject unfocused_lines_2_ins = Instantiate(unfocused_lines_2,this.transform.position,this.transform.rotation) as GameObject;
		unfocused_lines_2_ins.transform.parent= this.transform;
		
		unfocused_lines_2_ins.name= "unfocused_lines_2";
		
			
			
		}
		GameObject transparent_ins = Instantiate(transparent,this.transform.position,this.transform.rotation) as GameObject;
		transparent_ins.transform.parent= this.transform;
		
		transparent_ins.name= "transparent";
		
	}
}
