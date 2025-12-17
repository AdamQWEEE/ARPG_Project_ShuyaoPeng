using UnityEngine;
using System.Collections;

public class t_l_script : MonoBehaviour {
	private float y;
	private float t=0f;
	// Use this for initialization
	void Start () {
	y = this.transform.position.y;
			this.GetComponent<Renderer>().material.color = new Color(this.GetComponent<Renderer>().material.color.r,this.GetComponent<Renderer>().material.color.g,this.GetComponent<Renderer>().material.color.b,0f);
	}
	
	// Update is called once per frame
	void Update () {
		
		t+=Time.deltaTime;//timer

		if (t<1f){
				this.GetComponent<Renderer>().material.color += new Color(0f,0f,0f,(1f-this.GetComponent<Renderer>().material.color.a)/10f);
				this.transform.Rotate(Vector3.right,(0f-this.transform.eulerAngles.x)/10f);
		this.transform.position+= new Vector3(0f,Mathf.Sin(t*3.14f*2f)/200f,0f);
		}
	if (t>4f && t<5f){
				this.transform.Rotate(Vector3.right,(40f-this.transform.eulerAngles.x)/10f);
			this.transform.position+= new Vector3(0f,Mathf.Sin(t*3.14f*2f)/100f,0f);	
	this.GetComponent<Renderer>().material.color += new Color(0f,0f,0f,(0f-this.GetComponent<Renderer>().material.color.a)/10f);
			
		}	
	}
}
