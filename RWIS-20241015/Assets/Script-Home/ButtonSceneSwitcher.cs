using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneSwitcher : MonoBehaviour
{
    // インスペクターで設定可能なシーン名
    public string sceneName = "DisplayLyrics";

    // ボタンクリック時に呼び出される関数
    public void SwitchScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is not set in the inspector.");
        }
    }
}
