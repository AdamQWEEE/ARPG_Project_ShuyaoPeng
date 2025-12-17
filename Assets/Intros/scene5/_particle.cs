using UnityEngine;
using System.Collections;

public class _particle : MonoBehaviour {
	private Transform _camera;
	private float t=0f;
	private float t_blink=0f;
	private int blink=1;
	// Use this for initialization
	void Start () {
	_camera = GameObject.Find("Main Camera").transform;
		this.transform.eulerAngles= new Vector3(this.transform.eulerAngles.x+Random.Range(-90f,90f),this.transform.eulerAngles.y+Random.Range(-90f,90f),this.transform.eulerAngles.z+Random.Range(-90f,90f));
		
					this.GetComponent<Renderer>().material.SetColor("_TintColor",new Color(this.GetComponent<Renderer>().material.GetColor("_TintColor").r,this.GetComponent<Renderer>().material.GetColor("_TintColor").g,this.GetComponent<Renderer>().material.GetColor("_TintColor").b,0f));
		
	}
	
	// Update is called once per frame
	void Update () {
		t+=Time.deltaTime;//timer
		if (t<3f){
		if(this.GetComponent<Renderer>().material.GetColor("_TintColor").a<.99f){
		this.GetComponent<Renderer>().material.SetColor("_TintColor", this.GetComponent<Renderer>().material.GetColor("_TintColor")+new Color(0f,0f,0f,(1f-this.GetComponent<Renderer>().material.GetColor("_TintColor").a)/10f));
		}	}
		//
	this.transform.LookAt(_camera);
	this.transform.Rotate(Vector3.right,90f);
		//<-- making particles(represented by planes with textures) look into camera
		if (t>3f){
		t_blink+=Time.deltaTime;
			if (t_blink>.2f/blink &&this.GetComponent<Renderer>().material.GetColor("_TintColor").a!=0f){
				this.GetComponent<Renderer>().material.SetColor("_TintColor",new Color(this.GetComponent<Renderer>().material.GetColor("_TintColor").r,this.GetComponent<Renderer>().material.GetColor("_TintColor").g,this.GetComponent<Renderer>().material.GetColor("_TintColor").b,0f));
	this.transform.Find("Point light").GetComponent<Light>().intensity=0f;
				blink++;
				t_blink=0f;
			} else if (t_blink>.2f/blink &&this.GetComponent<Renderer>().material.GetColor("_TintColor").a==0f){
				this.transform.Find("Point light").GetComponent<Light>().intensity=.3f;
		
				this.GetComponent<Renderer>().material.SetColor("_TintColor",new Color(this.GetComponent<Renderer>().material.GetColor("_TintColor").r,this.GetComponent<Renderer>().material.GetColor("_TintColor").g,this.GetComponent<Renderer>().material.GetColor("_TintColor").b,1f));
				t_blink=0f;
				
			}
		}
		
		if (blink>=4){
			Destroy(this.gameObject);
		}
	}
}
