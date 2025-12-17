using UnityEngine;
using System.Collections;

public class text_control : MonoBehaviour {
	public LensFlare l1;
	public LensFlare l2;
	private float t=0f;
	private string text1="YOUR FIRST TEXT";
	private string text2="YOUR SECOND TEXT";
	
	private bool ch_text=false;
		private float t_blink=0f;
	private int blink=1;
	// Use this for initialization
	void Start () {
		this.transform.localScale= new Vector3(1f,0f,1f);
		this.GetComponent<TextMesh>().text=text1;
		print (this.GetComponent<TextMesh>().text);
	}
	
	// Update is called once per frame
	void Update () {
		
		t+=Time.deltaTime;
		if (_Camera.angle<10f){
		this.transform.localScale += new Vector3(0f,(1f-this.transform.localScale.y)/10f,0f);
	
	l1.transform.position+= new Vector3((1.2f-l1.transform.position.x)/10f,0f,0f);
	l2.transform.position+= new Vector3((-1.2f+l1.transform.position.x)/10f,0f,0f);
			if (l1.GetComponent<LensFlare>().brightness>0f){
	l1.GetComponent<LensFlare>().brightness+=(-1.1f-(0f-l1.GetComponent<LensFlare>().brightness))/20f;
	l2.GetComponent<LensFlare>().brightness+=(-1.1f-(0f-l2.GetComponent<LensFlare>().brightness))/20f;
			}
		} else if (blink<7){
			t_blink+=Time.deltaTime;
				if (t_blink>.3f/blink && this.GetComponent<Renderer>().material.color.a!=0f){
				blink++;
				print(blink);
				this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r,this.GetComponent<Renderer>().material.color.g,this.GetComponent<Renderer>().material.color.b,0f);
				t_blink=0f;
			}else if (t_blink>.3f/blink && this.GetComponent<Renderer>().material.color.a==0f){
			
				this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r,this.GetComponent<Renderer>().material.color.g,this.GetComponent<Renderer>().material.color.b,1f);
				t_blink=0f;
			}
			if (blink==7){
				
				blink=10;
				this.transform.Rotate(Vector3.up,180f);
				this.GetComponent<TextMesh>().text=text2;
				
				this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r,this.GetComponent<Renderer>().material.color.g,this.GetComponent<Renderer>().material.color.b,1f);
			
			}
			
			}
				
	
	
		
	}
}
