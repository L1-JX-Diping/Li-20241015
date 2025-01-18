using UnityEngine;
using TMPro;  // TextMeshProを使うために必要
public class LyricsColorController : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }
    public TextMeshProUGUI textComponent;  // シーンにあるTextMeshProUGUIをアタッチ
    public Color colorToSet = Color.red;   // 任意の色を設定できる

    void Start()
    {
        // textComponentが設定されているかチェック
        if (textComponent != null)
        {
            textComponent.color = colorToSet;  // 色を変更
        }
        else
        {
            Debug.LogWarning("Text Componentが設定されていません");
        }
    }

    // 色を変更する関数を追加
    public void ChangeTextColor(Color newColor)
    {
        if (textComponent != null)
        {
            textComponent.color = newColor;
        }
    }
}
