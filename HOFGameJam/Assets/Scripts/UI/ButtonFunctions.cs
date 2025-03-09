using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
  public void Resume()
  {
    GameManager.instance.StateResume();
  }
  
}
