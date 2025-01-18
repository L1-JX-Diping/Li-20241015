//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class KaraokeLyricsGame : MonoBehaviour
//{
//    public TextMeshProUGUI[] textField; // 表示用のTextMeshProUGUIオブジェクト（複数行）
//    public string lrcFileName = "Lrc-LyricsPartLog.txt"; // LRCファイル名
//    public List<Image> characterImages; // キャラクター画像リスト

//    private List<LyricLineInfo> lyricsList = new List<LyricLineInfo>(); // 歌詞データ
//    private int currentLyricIndex = 0; // 現在の歌詞インデックス
//    private Outline currentOutline; // 現在選ばれたキャラクターの枠
//    private Color[] characterColors; // キャラクターごとに割り当てられた色
//    private System.Random random = new System.Random();

//    private List<Color> availableColors = new List<Color> { Color.red, Color.magenta, Color.blue };

//    [System.Serializable]
//    public class LyricPartInfo
//    {
//        public string word; // 各単語
//        public float startTime; // 単語開始時間
//        public Color color; // 単語の色
//    }

//    [System.Serializable]
//    public class LyricLineInfo
//    {
//        public float startTime; // 行の開始時間
//        public List<LyricPartInfo> parts = new List<LyricPartInfo>(); // 単語リスト
//    }

//    void Start()
//    {
//        // 歌詞の読み込み
//        LoadExtendedLrcFile();

//        // キャラクターの色設定
//        AssignUniqueColorsToCharacters();
//        DisplayCharacterColors();

//        // 最初の歌詞を表示
//        ClearTextFields();
//        UpdateLyricsDisplay(); // 初期歌詞表示
//    }

//    void Update()
//    {
//        float currentTime = Time.timeSinceLevelLoad;

//        if (currentLyricIndex < lyricsList.Count)
//        {
//            var currentLyric = lyricsList[currentLyricIndex];

//            // 単語ごとのタイミングで枠を点灯
//            foreach (var part in currentLyric.parts)
//            {
//                if (currentTime >= part.startTime)
//                {
//                    // キャラクター枠を更新
//                    UpdateCharacterOutline(part.color);
//                }
//            }

//            // 行全体が終了したら次の行へ
//            if (currentTime >= currentLyric.startTime + 5.0f) // 例: 5秒後に次の行に進む
//            {
//                currentLyricIndex++;
//                ClearTextFields();
//                UpdateLyricsDisplay(); // 次の歌詞を表示
//            }
//        }
//    }

//    void ClearTextFields()
//    {
//        foreach (var field in textField)
//        {
//            field.text = "";
//        }
//    }

//    void UpdateCharacterOutline(Color color)
//    {
//        if (currentOutline != null)
//        {
//            currentOutline.effectColor = Color.clear; // 前回の枠をクリア
//        }

//        for (int i = 0; i < characterColors.Length; i++)
//        {
//            if (characterColors[i] == color)
//            {
//                var selectedImage = characterImages[i];
//                currentOutline = selectedImage.gameObject.GetComponent<Outline>();
//                if (currentOutline == null)
//                {
//                    currentOutline = selectedImage.gameObject.AddComponent<Outline>();
//                }
//                currentOutline.effectColor = color;
//                currentOutline.effectDistance = new Vector2(5f, 5f);
//                break;
//            }
//        }
//    }

//    void AssignUniqueColorsToCharacters()
//    {
//        characterColors = new Color[characterImages.Count];
//        List<Color> tempColors = new List<Color>(availableColors);

//        for (int i = 0; i < characterImages.Count; i++)
//        {
//            int randomIndex = random.Next(tempColors.Count);
//            characterColors[i] = tempColors[randomIndex];
//            tempColors.RemoveAt(randomIndex);
//        }
//    }

//    void DisplayCharacterColors()
//    {
//        for (int i = 0; i < characterImages.Count; i++)
//        {
//            var outline = characterImages[i].gameObject.GetComponent<Outline>();
//            if (outline == null)
//            {
//                outline = characterImages[i].gameObject.AddComponent<Outline>();
//            }
//            outline.effectColor = Color.clear;
//            outline.effectDistance = new Vector2(5f, 5f);
//        }
//    }

//    void LoadExtendedLrcFile()
//    {
//        string path = Path.Combine(Application.dataPath, lrcFileName);
//        if (!File.Exists(path))
//        {
//            Debug.LogError($"LRC file not found: {path}");
//            return;
//        }

//        string[] lines = File.ReadAllLines(path);
//        foreach (string line in lines)
//        {
//            if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\[\d+:\d+\.\d+\]"))
//            {
//                string[] parts = line.Split(']');
//                if (parts.Length < 2) continue;

//                string lineStartTimeStr = parts[0].Substring(1);
//                float lineStartTime = ParseTime(lineStartTimeStr);

//                var lyricInfo = new LyricLineInfo { startTime = lineStartTime };

//                for (int i = 1; i < parts.Length; i++)
//                {
//                    if (System.Text.RegularExpressions.Regex.IsMatch(parts[i], @"\[\d+:\d+\.\d+\]"))
//                    {
//                        int timeEndIndex = parts[i].IndexOf(']');
//                        string wordTimeStr = parts[i].Substring(1, timeEndIndex - 1);
//                        float wordStartTime = ParseTime(wordTimeStr);
//                        string word = parts[i].Substring(timeEndIndex + 1).Trim();

//                        lyricInfo.parts.Add(new LyricPartInfo
//                        {
//                            word = word,
//                            startTime = wordStartTime,
//                            color = availableColors[i % availableColors.Count]
//                        });
//                    }
//                }

//                lyricsList.Add(lyricInfo);
//            }
//        }
//    }

//    float ParseTime(string timeStr)
//    {
//        string[] timeComponents = timeStr.Split(':');
//        float minutes = float.Parse(timeComponents[0]);
//        float seconds = float.Parse(timeComponents[1]);
//        return minutes * 60 + seconds;
//    }

//    void UpdateLyricsDisplay()
//    {
//        if (currentLyricIndex < lyricsList.Count)
//        {
//            var currentLyric = lyricsList[currentLyricIndex];
//            string coloredText = "";

//            foreach (var part in currentLyric.parts)
//            {
//                string hexColor = ColorUtility.ToHtmlStringRGB(part.color);
//                coloredText += $"<color=#{hexColor}>{part.word}</color> ";
//            }

//            textField[1].text = coloredText.Trim();
//        }
//    }
//}



using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KaraokeLyricsGame : MonoBehaviour
{
    // 歌詞表示用
    //public TextMeshProUGUI[] textField; // 表示用のTextMeshProUGUIオブジェクト（3行分）
    //public string lrcFileName = "Lrc-BirthdaySong.txt"; // LRCファイル名（Assetsフォルダ内に配置）
    private List<LyricLineInfo> lyricsList = new List<LyricLineInfo>(); // 歌詞データのリスト
    private int currentLyricIndex = 0; // 現在の歌詞インデックス

    /* キャラクター関連 */
    // public 実行前にアタッチの必要
    public List<Image> characterImages; // キャラクター画像リスト
    // private 
    private Outline currentOutline; // 現在選ばれたキャラクターの枠
    private Color[] characterColors; // キャラクターごとに割り当てられた色
    private System.Random random = new System.Random();

    // 色設定
    //Color darkYellow = Color.Lerp(Color.yellow, Color.black, 0.3f); // 黄色を30%黒に近づける
    private List<Color> availableColors = new List<Color> { Color.red, Color.magenta, Color.blue }; // 使用する色
    //private List<Color> availableColors = new List<Color> { Color.yellow, Color.green}; // 使用する色
    private float timeInit = 0; // 歌詞の初期時間

    [System.Serializable]
    public class LyricLineInfo
    {
        public float startTime; // 表示開始時間
        public string text; // 歌詞のテキスト
    }

    void Start()
    {
        // 歌詞の読み込みと初期設定
        //LoadLrcFile();
        //UpdateLyricsDisplay();

        // キャラクター画像の初期設定と色の割り当て
        AssignUniqueColorsToCharacters();

        // キャラクター枠に色を反映
        DisplayCharacterColors();

        // 最初のキャラクターを選択して枠付け
        DecideChallenger();
    }

    void Update()
    {
        // 現在の時間に応じて歌詞を進行
        float currentTime = Time.timeSinceLevelLoad;

        if (currentLyricIndex < lyricsList.Count - 1 && currentTime >= lyricsList[currentLyricIndex + 1].startTime)
        {
            currentLyricIndex++;
            //UpdateLyricsDisplay();
            DecideChallenger(); // 歌詞ごとにキャラクター枠を変更
        }
    }

    void AssignUniqueColorsToCharacters()
    {
        characterColors = new Color[characterImages.Count];

        List<Color> tempColors = new List<Color>(availableColors); // コピーしたリストを使う
        for (int i = 0; i < characterImages.Count; i++)
        {
            int randomIndex = random.Next(tempColors.Count);
            characterColors[i] = tempColors[randomIndex];
            tempColors.RemoveAt(randomIndex); // 重複を避けるため選んだ色を削除
            Debug.Log($"Character {i} assigned color: {characterColors[i]}");
        }
    }

    void DisplayCharacterColors()
    {
        // キャラクターの枠に色を反映
        for (int i = 0; i < characterImages.Count; i++)
        {
            var outline = characterImages[i].gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = characterImages[i].gameObject.AddComponent<Outline>();
            }
            outline.effectColor = characterColors[i]; // 割り当てられた色を適用
            outline.effectDistance = new Vector2(5f, 5f); // 枠の太さ
        }
    }

    void DecideChallenger()
    {
        // キャラクターをランダムに選択
        int challengerIndex = random.Next(characterImages.Count);

        // 前回の枠をクリア
        if (currentOutline != null)
        {
            currentOutline.effectColor = Color.clear;
        }

        // 選ばれたキャラクターの枠を再設定
        var selectedImage = characterImages[challengerIndex];
        currentOutline = selectedImage.gameObject.GetComponent<Outline>();
        if (currentOutline == null)
        {
            currentOutline = selectedImage.gameObject.AddComponent<Outline>();
        }
        currentOutline.effectColor = characterColors[challengerIndex];
        currentOutline.effectDistance = new Vector2(5f, 5f);
    }

    //void UpdateLyricsDisplay()
    //{
    //    int middleLineIndex = 1;

    //    for (int i = 0; i < textField.Length; i++)
    //    {
    //        int lyricIndex = currentLyricIndex + i - middleLineIndex;

    //        if (lyricIndex >= 0 && lyricIndex < lyricsList.Count)
    //        {
    //            string coloredText = "";
    //            foreach (var part in lyricsList[lyricIndex].text.Split(' '))
    //            {
    //                // 各単語にランダムな色を割り当てて表示
    //                int randomColorIndex = Random.Range(0, availableColors.Count);
    //                string hexColor = ColorUtility.ToHtmlStringRGB(availableColors[randomColorIndex]);
    //                coloredText += $"<color=#{hexColor}>{part}</color> ";
    //            }

    //            textField[i].text = coloredText.Trim();

    //            if (i == middleLineIndex)
    //            {
    //                textField[i].color = new Color(1f, 1f, 1f, 1f); // 現在行を強調表示
    //            }
    //            else
    //            {
    //                textField[i].color = new Color(1f, 1f, 1f, 0.5f); // 他の行を薄く表示
    //            }
    //        }
    //        else
    //        {
    //            textField[i].text = ""; // 範囲外の行は空にする
    //        }
    //    }
    //}


    //void LoadLrcFile()
    //{
    //    string path = Path.Combine(Application.dataPath, lrcFileName);
    //    if (!File.Exists(path))
    //    {
    //        Debug.LogError($"LRC file not found: {path}");
    //        return;
    //    }

    //    string[] lines = File.ReadAllLines(path);
    //    foreach (string line in lines)
    //    {
    //        if (System.Text.RegularExpressions.Regex.IsMatch(line, @"\[\d+:\d+\.\d+\]"))
    //        {
    //            string timePart = line.Substring(1, line.IndexOf("]") - 1);
    //            string[] timeComponents = timePart.Split(':');
    //            float minutes = float.Parse(timeComponents[0]);
    //            float seconds = float.Parse(timeComponents[1]);
    //            float startTime = timeInit + minutes * 60 + seconds;

    //            string textPart = line.Substring(line.IndexOf("]") + 1);
    //            lyricsList.Add(new LyricLineInfo { startTime = startTime, text = textPart });
    //        }
    //    }

    //    Debug.Log($"Loaded {lyricsList.Count} lyrics from {lrcFileName}");
    //}
}