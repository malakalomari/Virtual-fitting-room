using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDirector : MonoBehaviour {

    //public bool menueDisplayed = false;
    public Animator anim;
	// Update is called once per frame
	void Update () {
        if(Input.GetKey(KeyCode.Escape))
        {
            ActivatePauseMenue();
        }
		
	}
    void ActivatePauseMenue()
    {
        anim.SetBool("menuDisplayed", true);
    }
    void DeactivatePauseMenue()
    {
        anim.SetBool("menuDisplayed", false);
    }

    public void ResumeGame()
    {
        DeactivatePauseMenue();
    }
    public void RestartGame()
    {
        Application.LoadLevel(0);
        DeactivatePauseMenue();
    }
   public void ExitGame()
    {
        Application.Quit();
    }
}
