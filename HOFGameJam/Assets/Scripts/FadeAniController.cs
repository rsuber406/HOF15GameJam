using UnityEngine;

public class FadeAniController : MonoBehaviour
{
    
    public Animator fadeAnimator;
    
   
    public void FadeToWhite()
    {
        fadeAnimator.Play("FadeToWhite");
    }
}
