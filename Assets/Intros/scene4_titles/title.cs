using UnityEngine;
using System.Collections;

public class title : MonoBehaviour {
	
	private string title1 = "YOUR FIRST TITLE";
		private string title2 = "YOUR SECOND TITLE";
		private string title3 = "ADD AS MANY TITLES, AS YOU NEED";
	
	public GameObject t_l;
	private float t=0f;
		private float t_general=0f;
	public static char[] characters1;
		public static char[] characters2;
	public static char[] characters3;
	private int let_n=0;
	private float delay=3f;
	private bool t1_stop=false;
	private bool t2_stop=false;
		private bool t3_stop=false;
	// Use this for initialization
	void Start () {
	characters1 = title1.ToCharArray();
	characters2 = title2.ToCharArray();
	characters3 = title3.ToCharArray();
	}
	
	// Update is called once per frame
	void Update () {
		t_general+=Time.deltaTime;
		if (t_general>delay && this.name=="title1" && t1_stop==false){
		t+=Time.deltaTime;
		
		if (t>.1f ){
			t=0f;
			Vector3 pos= new Vector3(this.transform.position.x+let_n/7f,this.transform.position.y,this.transform.position.z);
		GameObject text_letter =	Instantiate(t_l,pos,this.transform.rotation) as GameObject;
			text_letter.transform.parent = this.transform.parent;
			text_letter.GetComponent<TextMesh>().text=characters1[let_n].ToString();
			let_n++;
			if (let_n>=title1.Length){
				let_n=0;
				t1_stop=true;	
			}
		}
		}
		
		
		if (t_general>delay*3f && this.name=="title2" && t2_stop==false){
		t+=Time.deltaTime;
		
		if (t>.1f){
			t=0f;
			Vector3 pos= new Vector3(this.transform.position.x+let_n/7f,this.transform.position.y,this.transform.position.z);
		GameObject text_letter =	Instantiate(t_l,pos,this.transform.rotation) as GameObject;
			text_letter.transform.parent = this.transform.parent;
			text_letter.GetComponent<TextMesh>().text=characters2[let_n].ToString();
			let_n++;
			if (let_n>=title2.Length){
				let_n=0;
				t2_stop=true;	
			}
		}
		}
		
		if (t_general>delay*5f && this.name=="title3" && t3_stop==false){
		t+=Time.deltaTime;
		
		if (t>.1f){
			t=0f;
			Vector3 pos= new Vector3(this.transform.position.x+let_n/8f,this.transform.position.y,this.transform.position.z);
		GameObject text_letter =	Instantiate(t_l,pos,this.transform.rotation) as GameObject;
			text_letter.transform.parent = this.transform.parent;
			text_letter.GetComponent<TextMesh>().text=characters3[let_n].ToString();
			let_n++;
			if (let_n>=title3.Length){
				let_n=0;
				t3_stop=true;	
			}
		}
		}
		
		
	}
}
