using UnityEngine;
using UnityEngine.UI; // UI要素にアクセスするために必要
using UnityEngine.SceneManagement; // シーン切り替えのために必要

public class SceneSwitcher : MonoBehaviour
{
    void Start()
    {
        // AssignColor シーンにあるボタンの操作
        GameObject.Find("ButtonStart").GetComponent<Button>().onClick.AddListener(ClickButtonStartGame);
        GameObject.Find("ButtonBack").GetComponent<Button>().onClick.AddListener(ClickButtonBackToHome);
    }
    void ClickButtonStartGame()
    {
        // ボタンクリック時に呼び出される
        string sceneName = "DisplayLyrics";
        SwitchScene(sceneName);
    }
    void ClickButtonBackToHome()
    {
        // ボタンクリック時に呼び出される
        string sceneName = "Home";
        SwitchScene(sceneName);
    }

    void SwitchScene(string sceneName)
    {
        // sceneName という名前のシーンをロード
        SceneManager.LoadScene(sceneName);
    }
}
