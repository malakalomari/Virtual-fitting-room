using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class ColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private ColorSourceManager _ColorManager;
    
    void Update()
    {
        if (ColorSourceManager == null)
        {
            return;
        }
        
        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }
        
		GetComponent<RawImage>().texture = _ColorManager.GetColorTexture ();
    }
}