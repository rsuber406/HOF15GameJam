using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
  public void Resume()
  {
    GameManager.instance.StateResume();
  }

  public void OpenSettings()
  {
    GameManager.instance.OpenSettingsMenu();
  }

  public void CloseSettings()
  {
    GameManager.instance.CloseSettingsMenu();
  }

  public void QuitGame()
  {
    GameManager.instance.QuitGame();
  }
}
