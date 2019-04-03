using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour {

	public GameObject currentObject;

	public void Plus ()
	{
		currentObject.transform.localScale += new Vector3(0.2f, 0.2f, 0);
	}
	public void Minus ()
	{
		currentObject.transform.localScale -= new Vector3(0.2f, 0.2f, 0);
        print(this.currentObject);
	}
}