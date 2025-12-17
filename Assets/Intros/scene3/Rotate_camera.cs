using UnityEngine;

/// <summary>
/// Placing this script on the game object will make that game object pan with mouse movement.
/// </summary>

public class Rotate_camera : MonoBehaviour 
{
	private Transform point;
	private float t=0f;
	private float rot_k=.99f;
	void Start ()
	{
		point= GameObject.Find("p_1").transform;
	
	}
	void OnGUI(){
		if (GUI.Button(new Rect(10,10,60,20),"MENU")){
			
			Application.LoadLevel(0);//back to menu
		}
	}
	void Update ()
	{
		t+=Time.deltaTime;//timer
		if (t>11f){
		rot_k=rot_k*.99f;
		}
	this.transform.RotateAround(point.transform.position,Vector3.up,-.1f*(Mathf.PI/2f-Mathf.Atan(1.5f*t-5f))*Time.deltaTime*75f*rot_k);
		
			this.transform.Rotate(Vector3.up,-2f*(Mathf.PI/2f-Mathf.Abs(Mathf.Atan(9f*(t-6.5f))))*Time.deltaTime*75f*rot_k);
		if (rot_k<=0.05f){
			    Application.LoadLevel(3);//restarting the intro
		}
	}
}