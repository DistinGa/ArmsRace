using UnityEngine;
using System.Collections;

public class CameraSwither : MonoBehaviour {
	public Camera camera1;
	public Camera camera2;
	
	void Start()
	{
		camera2.enabled = false;
	}
	
	void Switch()
	{
		camera1.enabled = !camera1.enabled;
		camera2.enabled = !camera2.enabled;
	}
}