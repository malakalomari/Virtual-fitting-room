using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour {

	public PointerSelectableImage[] clothesSelection;
	public GameObject[] clothes;
	public Sprite[] availableTextures;
	List<PointerSelectableImage> texturesSelection = new List<PointerSelectableImage>();
	Transform texturesHolder;

	GameObject currentCloth;

	void Awake ()
	{
		texturesHolder = GameObject.Find("Textures Holder").transform;
	}
	void Start ()
	{
		for (int i = 0; i < clothesSelection.Length; i++)
		{
			//Avoiding bugs
			int index = i;
			//We assign every image to it's model when the image has been Selected
			clothesSelection[i].onSelected += () => SetCloth(clothes[index]);
		}

		for (int i = 0; i < availableTextures.Length; i++)
		{
			PointerSelectableImage selectableTexture = Instantiate<PointerSelectableImage>(Resources.Load<PointerSelectableImage>("Texture Item"), texturesHolder);
			selectableTexture.GetComponent<Image>().sprite = availableTextures[i];
			texturesSelection.Add(selectableTexture);
		}

		texturesHolder.gameObject.SetActive(false);
	}
	
	public void SetCloth (GameObject cloth)
	{
		texturesHolder.gameObject.SetActive(true);

		foreach (var textureSelectionItem in texturesSelection)
			if (textureSelectionItem)
				Destroy(textureSelectionItem.gameObject);

		foreach (var texture in texturesHolder.GetComponentsInChildren<Image>())
			if (texture.transform != texturesHolder)
				Destroy(texture.gameObject);

		texturesSelection.Clear();

		for (int i = 0; i < availableTextures.Length; i++)
		{
			PointerSelectableImage selectableTexture = Instantiate<PointerSelectableImage>(Resources.Load<PointerSelectableImage>("Texture Item"), texturesHolder);
			selectableTexture.GetComponent<Image>().sprite = availableTextures[i];
			texturesSelection.Add(selectableTexture);
		}

		if (currentCloth)
			currentCloth.SetActive(false);

		cloth.SetActive(true);

		foreach (var item in cloth.GetComponentsInChildren<Renderer>(true))
			item.enabled = true;

		currentCloth = cloth;

		//Here we find the size controller script and set the current object for scaling
		FindObjectOfType<SizeController>().currentObject = cloth;

		foreach (var textureSelectionItem in texturesSelection)
			textureSelectionItem.onSelected += () => {
			foreach (var item in cloth.GetComponentInChildren<Renderer>().materials) {
				item.mainTexture = textureSelectionItem.GetComponent<Image>().sprite.texture;
			}};
	}
}