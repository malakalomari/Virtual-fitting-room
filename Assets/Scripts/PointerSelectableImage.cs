using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PointerSelectableImage : MonoBehaviour {

	public event Action onSelected;
	public event Action onDeselected;
	Transform[] pointers;
	Image image;
	Image fillPointer;

	bool inBounds;
	[SerializeField] bool debug;

	void Awake ()
	{
		image = GetComponent<Image>();
		fillPointer = GameObject.Find("Fill Pointer").GetComponent<Image>();
		pointers = new Transform[2];

		GameObject[] scenePointers = GameObject.FindGameObjectsWithTag("Pointer");

		for (int i = 0; i < 2; i++)
		{
			pointers[i] = scenePointers[i].transform;
		}

		if (debug)
			onSelected += () => image.color = Color.red;
	}
	
	void Update ()
	{
		//Check for the pointer if it's in the area of the image
		bool foundPointerInBounds = false;
		foreach (Transform pointer in pointers)
			if (RectTransformToScreenSpace(image.rectTransform).Contains((Vector2) Camera.main.WorldToScreenPoint(pointer.position)))
				foundPointerInBounds = true;

		if (foundPointerInBounds)
		{
			//And we weren't in bounds
			if (!inBounds)
				//Enable The Selection
				EnableSelection ();

			inBounds = true;
		}
		else
		{
			//if we didn't find the pointer and we were in the image's area
			if (inBounds)
				//Disable the selection
				DisableSelection();

			inBounds = false;
		}

		if (debug)
			image.color = Color.Lerp(image.color, Color.white, Time.deltaTime * 5);
	}

	void EnableSelection ()
	{
		fillPointer.transform.position = transform.position;

		//Start a thread
		StartCoroutine("ManageSelectionTimer");
	}
	void DisableSelection ()
	{
		fillPointer.transform.position = -Vector2.one * 99;

		//End the thread time
		StopCoroutine("ManageSelectionTimer");

		if (onDeselected != null)
			onDeselected();
	}
	IEnumerator ManageSelectionTimer ()
	{
		float timer = 0;
		float duration = 1;

		while((timer += Time.deltaTime / duration) < 1)
		{
			fillPointer.fillAmount = timer;
			yield return new WaitForEndOfFrame();
		}
		
		if (onSelected != null)
			onSelected();

		DisableSelection ();
	}

	public static Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		Rect rect = new Rect(transform.position.x, transform.position.y, size.x, size.y);
		rect.x -= (transform.pivot.x * size.x);
		rect.y -= ((1.0f - transform.pivot.y) * size.y);
		return rect;
	}
}