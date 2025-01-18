using UnityEngine;
using UnityEngine.UI; // UI要素にアクセスするために必要
using UnityEngine.SceneManagement; // シーン切り替えのために必要

public class SceneSwitcherGameDisp : MonoBehaviour
{
    void Start()
    {
        // DisplayLyrics シーンにあるボタンの操作
        GameObject.Find("ButtonHome").GetComponent<Button>().onClick.AddListener(ClickButtonEndGame);
        GameObject.Find("ButtonScore").GetComponent<Button>().onClick.AddListener(ClickButtonViewScore);
    }
    void ClickButtonEndGame()
    {
        // ボタンクリック時に呼び出される
        string sceneName = "Home";
        Debug.Log(sceneName);
        SwitchScene(sceneName);
    }
    void ClickButtonViewScore()
    {
        // ボタンクリック時に呼び出される
        string sceneName = "Score";
        Debug.Log(sceneName);
        SwitchScene(sceneName);
    }

    void SwitchScene(string sceneName)
    {
        // sceneName という名前のシーンをロード
        SceneManager.LoadScene(sceneName);
    }
}
