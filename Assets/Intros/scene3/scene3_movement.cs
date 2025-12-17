using UnityEngine;
using System.Collections;

public class scene3_movement : MonoBehaviour {
private float t=1f;
private Vector3 start_rot;
private float extra_force=.99f;
private bool coll=false;
	private float wiggle_x=0f;
	private float wiggle_y=0f;
	private float wiggle_z=0f;
	private float time_c=0f;
	public Transform collide_p;
	public LensFlare lf;
	// Use this for initialization
	void OnAwake () {
		
	}
	
	
	void Start () {
		this.transform.eulerAngles +=  new Vector3(Random.Range(-90f,90f),Random.Range(-90f,90f),Random.Range(-90f,90f));
	start_rot = this.transform.eulerAngles;
	wiggle_z=start_rot.z;	
		wiggle_x=start_rot.x;
		wiggle_y=start_rot.y;	
		this.GetComponent<ParticleSystem>().startDelay=Random.Range(0f,5f);
	}
	
	// Update is called once per frame
	void Update () {
		time_c+=Time.deltaTime;
		if (this.name=="unfocused_lines" || (this.name=="unfocused_lines_2" && time_c>=1f) || (this.name=="lines" && time_c>=2f)){
		t+=6f*Time.deltaTime;
		wiggle_x+= Mathf.Sin(t)*10f*Random.Range(0f,2f)+1f;
		wiggle_y+=Mathf.Sin(t)*10f*Random.Range(0f,2f)+1f;
		wiggle_z+=Mathf.Sin(t)*10f*Random.Range(0f,2f)+1f;
		
	this.GetComponent<Rigidbody>().AddRelativeForce(transform.worldToLocalMatrix.MultiplyVector(this.transform.forward)*Time.deltaTime*120f*extra_force);
		extra_force=Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(extra_force)))))*extra_force/1.00f;
		this.transform.eulerAngles = new Vector3(wiggle_x,wiggle_y,wiggle_z);
		//this.particleSystem.emissionRate= 3f*(120f+Mathf.Sin(t)*80f)/2f;
	
		}
	}
	
	

}
