using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	public PointerSelectableImage[] buttons;

	void Start ()
	{
		foreach (var item in buttons)
			item.onSelected += () => item.GetComponent<Button>().onClick.Invoke();
	}
}