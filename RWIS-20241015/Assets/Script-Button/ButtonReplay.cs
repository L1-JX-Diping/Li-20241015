using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 扱うので
using UnityEngine.SceneManagement; // Scene の切り替えしたい場合に必要な宣言

public class ButtonReplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // ボタンが押されたらこれを実行
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SwitchScene()
    {
        // シーン DisplayLyrics を開く
        SceneManager.LoadScene("DisplayLyrics");
    }
}
