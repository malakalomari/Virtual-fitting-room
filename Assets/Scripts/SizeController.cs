using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour {

	public GameObject currentObject;

	public void Plus ()
	{
		currentObject.transform.localScale += new Vector3(0.5f, 0, 0.5f);
	}
	public void Minus ()
	{
		currentObject.transform.localScale -= new Vector3(0.1f, 0, 0.1f);
	}
}