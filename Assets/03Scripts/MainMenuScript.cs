using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject selectScreen;
    [SerializeField] private GameObject loadingSceneImage;
    [Header("SceneList")]
    [SerializeField] private string[] sceneName;

    private void Awake()
    {
        selectScreen.SetActive(false);
        loadingSceneImage.SetActive(false);
    }

    public void MenuSelectButton()
    {
        selectScreen.SetActive(true);
    }
    public void MenuSelectCancelButton()
    {
        selectScreen.SetActive(false);
    }

    public void LoadingImageActive()
    {
        selectScreen.SetActive(false);
        loadingSceneImage.SetActive(true);
    }

    public void PlayGameSceneNo01()
    {
        LoadingImageActive();
        SceneManager.LoadScene(sceneName[0], LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
