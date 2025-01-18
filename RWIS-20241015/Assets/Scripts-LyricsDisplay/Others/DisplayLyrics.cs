using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Xml;
[System.Serializable]
public class ColorLog
{
    public List<ColorLogLine> ColorLogLines = new List<ColorLogLine>();
}
[System.Serializable]
public class ColorLogLine
{
    public int LineIndex;
    public List<ColorLogPart> Parts = new List<ColorLogPart>();
}
[System.Serializable]
public class ColorLogPart
{
    public string Text;
    public string Color;
}
public class DisplayLyrics : MonoBehaviour
{
    public TextMeshProUGUI textComponent;   // 表示用のTextMeshProUGUI
    public string colorLogFileName = "Assets/ColorLog.json"; // 色情報のログファイル
    private ColorLog colorLog;             // JSONデータを保持
    private int currentLineIndex = 0;      // 現在表示中の先頭行
    private Color[] colors = { Color.red, Color.green, Color.blue }; // 使用する3色
    private string[] colorNames = { "RED", "GREEN", "BLUE" }; // 色名対応
    private int colorIndex = 0;
    void Start()
    {
        // 最初の色ランダム生成
        colorIndex = Random.Range(0, colors.Length);
        // 歌い出しはこの人からだよってお知らせを入れる

        // Step 1: ランダム色分けを記録
        GenerateColorLog();
        // Step 2: 色情報をロード
        LoadColorLog();
        // Step 3: スクロール表示を開始
        StartCoroutine(ScrollLyricsCoroutine());
    }

    /// <summary>
    /// 単語ごとに色分け（パート分け）
    /// だから同じ色が連続してもいい
    /// こうすると同じ色連続が増える。。。
    /// やっぱ不採用でいこう
    /// </summary>
    //void GenerateColorLog()
    //{
    //    string inputFileName = "Lyrics.xml"; // 読み込む歌詞ファイル
    //    string path = Path.Combine(Application.dataPath, inputFileName);
    //    if (!File.Exists(path))
    //    {
    //        Debug.LogError($"File not found: {path}");
    //        return;
    //    }
    //    colorLog = new ColorLog();
    //    XmlDocument xmlDoc = new XmlDocument();
    //    xmlDoc.Load(path);
    //    XmlNodeList lines = xmlDoc.SelectNodes("/Lyrics/Line");
    //    for (int i = 0; i < lines.Count; i++)
    //    {
    //        ColorLogLine logLine = new ColorLogLine { LineIndex = i + 1 };
    //        string lineText = lines[i].InnerText;
    //        string[] parts = lineText.Split(' ');
    //        foreach (string part in parts)
    //        {
    //            int colorIndex = Random.Range(0, colors.Length);
    //            ColorLogPart logPart = new ColorLogPart
    //            {
    //                Text = part.Trim(),
    //                Color = colorNames[colorIndex]
    //            };
    //            logLine.Parts.Add(logPart);
    //        }
    //        colorLog.ColorLogLines.Add(logLine);
    //    }
    //    // 色情報をJSONとして保存
    //    string json = JsonUtility.ToJson(colorLog, true);
    //    File.WriteAllText(colorLogFileName, json);
    //    Debug.Log($"Color log saved to {colorLogFileName}");
    //}

    /// <summary>
    /// カンマでパート分けしたファイルを入力にとった場合
    /// </summary>
    void GenerateColorLog()
    {
        string inputFileName = "Orders.xml"; // 読み込む歌詞ファイル
        string path = Path.Combine(Application.dataPath, inputFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            return;
        }
        colorLog = new ColorLog();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNodeList lines = xmlDoc.SelectNodes("/Lyrics/Line");
        for (int i = 0; i < lines.Count; i++)
        {
            ColorLogLine logLine = new ColorLogLine { LineIndex = i + 1 };
            string lineText = lines[i].InnerText;
            string[] parts = lineText.Split(',');
            int lastColorIndex = -1;
            foreach (string part in parts)
            {
                while (lastColorIndex == colorIndex)
                {
                    colorIndex = Random.Range(0, colors.Length);
                }
                ColorLogPart logPart = new ColorLogPart
                {
                    Text = part.Trim(),
                    Color = colorNames[colorIndex]
                };
                logLine.Parts.Add(logPart);
                // 今の色を記憶（次のパート連続同じ色割り当てないよう）
                lastColorIndex = colorIndex;
            }
            colorLog.ColorLogLines.Add(logLine);
        }
        // 色情報をJSONとして保存
        string json = JsonUtility.ToJson(colorLog, true);
        File.WriteAllText(colorLogFileName, json);
        Debug.Log($"Color log saved to {colorLogFileName}");
    }

    /// <summary>
    /// 単語一つ一つでパート分けしたファイルを入力にとった場合
    /// </summary>
    //void GenerateColorLog()
    //{
    //    string inputFileName = "Lyrics.xml"; // 読み込む歌詞ファイル
    //    string path = Path.Combine(Application.dataPath, inputFileName);
    //    if (!File.Exists(path))
    //    {
    //        Debug.LogError($"File not found: {path}");
    //        return;
    //    }
    //    colorLog = new ColorLog();
    //    XmlDocument xmlDoc = new XmlDocument();
    //    xmlDoc.Load(path);
    //    XmlNodeList lines = xmlDoc.SelectNodes("/Lyrics/Line");
    //    for (int i = 0; i < lines.Count; i++)
    //    {
    //        ColorLogLine logLine = new ColorLogLine { LineIndex = i + 1 };
    //        string lineText = lines[i].InnerText;
    //        string[] parts = lineText.Split(' ');
    //        int lastColorIndex = -1;
    //        foreach (string part in parts)
    //        {
    //            while (lastColorIndex == colorIndex)
    //            {
    //                colorIndex = Random.Range(0, colors.Length);
    //            }
    //            ColorLogPart logPart = new ColorLogPart
    //            {
    //                Text = part.Trim(),
    //                Color = colorNames[colorIndex]
    //            };
    //            logLine.Parts.Add(logPart);
    //            // 今の色を記憶（次のパート連続同じ色割り当てないよう）
    //            lastColorIndex = colorIndex;
    //        }
    //        colorLog.ColorLogLines.Add(logLine);
    //    }
    //    // 色情報をJSONとして保存
    //    string json = JsonUtility.ToJson(colorLog, true);
    //    File.WriteAllText(colorLogFileName, json);
    //    Debug.Log($"Color log saved to {colorLogFileName}");
    //}
    void LoadColorLog()
    {
        if (File.Exists(colorLogFileName))
        {
            string json = File.ReadAllText(colorLogFileName);
            colorLog = JsonUtility.FromJson<ColorLog>(json);
            Debug.Log("Color log loaded.");
        }
        else
        {
            Debug.LogError($"Color log file not found: {colorLogFileName}");
        }
    }
    IEnumerator ScrollLyricsCoroutine()
    {
        while (currentLineIndex < colorLog.ColorLogLines.Count)
        {
            // 現在の行と次の行を取得
            ColorLogLine line1 = colorLog.ColorLogLines[currentLineIndex];
            ColorLogLine line2 = currentLineIndex + 1 < colorLog.ColorLogLines.Count
                ? colorLog.ColorLogLines[currentLineIndex + 1]
                : null;
            // テキストを構築
            int rowNum = currentLineIndex + 1;
            // 
            string displayedText = rowNum.ToString() + BuildTextWithColors(line1, 1f);
            if (line2 != null)
            {
                displayedText += "\n" + (rowNum + 1).ToString() + BuildTextWithColors(line2, 0.3f);
            }
            // 表示テキストを更新
            textComponent.text = displayedText;
            // 3.5秒ごとに表示歌詞更新
            currentLineIndex++;
            yield return new WaitForSeconds(3.5f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param> 処理したい行の中身（from ColorLog ファイル）
    /// <param name="colorAlpha"></param> 色の濃さ指定 0～1
    /// <returns></returns>
    string BuildTextWithColors(ColorLogLine line, float colorAlpha)
    {
        string text = "";
        // ColorLog ファイルから取得したこの１行の中でパート分けごとに色設定反映
        foreach (ColorLogPart part in line.Parts)
        {
            // ColorLog から色名を取得し、Color 型に変換
            Color color = ParseColor(part.Color);
            Debug.Log("color rgb: " + color.r + color.g + color.b);
            Debug.Log("color a: " + color.a);
            // 表示する濃さ
            Color finalColor = SetAlpha(color, colorAlpha);
            Debug.Log("color rgb: " + finalColor.r + finalColor.g + finalColor.b);
            Debug.Log("color a: " + finalColor.a);
            // Color 型を 16 進カラーコードに変換して <color> タグを挿入
            string colorCode = ColorToCode(SetAlpha(color, colorAlpha));
            text += $"<color={colorCode}>{part.Text}</color> ";
        }
        return text.Trim();
    }

    /// <summary>
    /// カラーネームからカラー型に
    /// </summary>
    /// <param name="colorName"></param>
    /// <returns></returns>
    Color ParseColor(string colorName)
    {
        switch (colorName)
        {
            case "RED": return Color.red;
            case "GREEN": return Color.green;
            case "BLUE": return Color.blue;
            default: return Color.white;
        }
    }

    /// <summary>
    /// 表示歌詞の色の濃さ調節
    /// </summary>
    /// <param name="color"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    Color SetAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    // ColorToHex : UnityのColor 型を HTML 形式の 16 進カラーコードに変換
    string ColorToCode(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
}