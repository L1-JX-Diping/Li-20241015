using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;
using TMPro;

public class Test : MonoBehaviour
{
    public TextMeshProUGUI textComponent;  // Hierarchyで作成したTextMeshProUGUIをアタッチ
    public string inputFileName = "Orders.xml";  // 読み込むXMLファイル名（Assets/配下）
    public string logFileName = "Assets/ColorLog.txt"; // 色情報を記録するファイル
    private string[] lines;  // 読み込んだ歌詞を保持
    private int currentLineIndex = 0;  // 現在の行を追跡
    private Color[] colors = { Color.red, Color.green, Color.blue }; // 使用する3色

    void Start()
    {
        // Orders.xmlを読み込む
        LoadLyrics();
        // 色ログファイルを初期化
        if (File.Exists(logFileName)) File.Delete(logFileName);
        File.WriteAllText(logFileName, "Color Log:\n");

        // コルーチンを開始して2秒ごとに表示
        StartCoroutine(DisplayLyricsCoroutine());
    }

    void LoadLyrics()
    {
        string path = Path.Combine(Application.dataPath, inputFileName);
        if (File.Exists(path))
        {
            // XMLファイルをロード
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNodeList lineNodes = xmlDoc.SelectNodes("/Lyrics/Line");
            lines = new string[lineNodes.Count];
            for (int i = 0; i < lineNodes.Count; i++)
            {
                lines[i] = lineNodes[i].InnerText.Trim();
            }
        }
        else
        {
            Debug.LogError($"File not found: {path}");
            lines = new string[0];
        }
    }

    IEnumerator DisplayLyricsCoroutine()
    {
        while (currentLineIndex < lines.Length)
        {
            // 現在の行を取得
            string line = lines[currentLineIndex];
            currentLineIndex++;

            // コンマで区切り、各部分にランダムな色を適用
            string[] parts = line.Split(',');
            string formattedText = "";  // 最終的に表示するテキスト
            string logText = $"Line {currentLineIndex}:\n"; // ログ用

            foreach (string part in parts)
            {
                // ランダムな色を選択
                Color randomColor = colors[Random.Range(0, colors.Length)];
                textComponent.color = randomColor;

                // テキストを表示
                textComponent.text = part.Trim();
                formattedText += part.Trim() + " ";

                // ログに追加
                logText += $"  \"{part.Trim()}\" - {randomColor}\n";

                // 少し待機して次の部分を表示
                yield return new WaitForSeconds(0.5f);
            }

            // 全体の行を最終表示
            textComponent.text = formattedText.Trim();

            // ログをファイルに保存
            File.AppendAllText(logFileName, logText + "\n");

            // 待機する秒数の指定
            // 2秒待機して次の行に進む
            yield return new WaitForSeconds(2f);
        }
    }
}
