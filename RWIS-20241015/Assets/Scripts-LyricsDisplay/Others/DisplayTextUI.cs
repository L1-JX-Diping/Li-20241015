using UnityEngine;
using TMPro;  // TextMeshProを使用するために必要

public class DisplayTextUI : MonoBehaviour
{
    public TextMeshProUGUI textComponent;  // Hierarchyで作成したTextMeshProUGUIをアタッチ

    void Start()
    {
        // テキストを設定
        textComponent.text = "Hello, Unity!";
        textComponent.color = Color.red;
    }

    void Update()
    {
        // シーン開始からの経過時間を取得
        float elapsedTime = Time.time;

        // 時間に応じてテキストを更新
        if (elapsedTime > 2)
        {
            textComponent.color = Color.blue;
            textComponent.text = elapsedTime.ToString();
        }
        // 時間に応じてテキストを更新
        // textComponent.text = "Time: " + Time.time.ToString("F2");
    }
    // テキスト内容を変更する関数
    public void UpdateLyricsText(string newText)
    {
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }

    // テキスト色を変更する関数
    public void UpdateTextColor(Color newColor)
    {
        if (textComponent != null)
        {
            textComponent.color = newColor;
        }
    }
}
