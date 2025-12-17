using UnityEngine;
using System.Collections;

public class particles_inst : MonoBehaviour {
	private float t1=0f;
		private float t2=0f;
		private float t3=0f;
	public Transform particle1;
	public Transform particle2;
	public Transform particle3;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	t1+=Time.deltaTime;
		if (t1>.4f){
			t1=0f;
			Instantiate(particle1, new Vector3(this.transform.position.x+Random.Range(-5f,5f),this.transform.position.y+Random.Range(-1f,1f),this.transform.position.z+Random.Range(-5f,5f)),this.transform.rotation);
		}
		
	t2+=Time.deltaTime;
		if (t2>.45f){
			t2=0f;
			Instantiate(particle2, new Vector3(this.transform.position.x+Random.Range(-5f,5f),this.transform.position.y+Random.Range(-1f,1f),this.transform.position.z+Random.Range(0f,5f)),this.transform.rotation);
		}
		
	t3+=Time.deltaTime;
		if (t3>.35f){
			t3=0f;
			Instantiate(particle3, new Vector3(this.transform.position.x+Random.Range(-5f,5f),this.transform.position.y+Random.Range(-1f,1f),this.transform.position.z+Random.Range(0f,5f)),this.transform.rotation);
		}	
		
	}
}
