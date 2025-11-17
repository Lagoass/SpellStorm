using UnityEngine;
public class GameManager : MonoBehaviour
{
    private int MainMenuIndex = 0;
    private int GameSceneIndex = 1;

    public GameObject MapMenuUI;

    void Start()
    {
        MapMenuUI.SetActive(false);
    }
    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuIndex);
    }
    public void LoadGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneIndex);
    }

    public void MainToMapMenu()
    {
        MapMenuUI.SetActive(true);
    }

    public void CloseMapMenu()
    {
        MapMenuUI.SetActive(false);
    }

    

}
