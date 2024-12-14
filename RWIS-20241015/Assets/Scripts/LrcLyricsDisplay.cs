using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LrcLyricsDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] textLines; // 歌詞を表示するTextMeshProUGUIオブジェクト（3行分）
    public string lrcFileName = "Lrc-BirthdaySong.txt"; // LRCファイル名（Assetsフォルダ内）
    private List<LyricLine> lyrics = new List<LyricLine>(); // 歌詞情報を格納するリスト
    private int currentLyricIndex = 0; // 現在の歌詞インデックス
    private float timeInit = 0; // 歌の始まりの時刻
    private Color[] colors = { Color.red, Color.green, Color.yellow }; // 使用する3色
    private string[] colorNames = { "Red", "Green", "Yellow" }; // 色名

    [System.Serializable]
    public class LyricPart
    {
        public string word; // 単語
        public Color color; // 割り当てられた色
    }

    [System.Serializable]
    public class LyricLine
    {
        public float time; // 表示時刻（秒単位）
        public string text; // 歌詞内容
        public List<LyricPart> parts = new List<LyricPart>(); // 単語ごとの色情報
    }

    void Start()
    {
        LoadLrcFile(); // LRCファイルを読み込む
        AssignRandomColors(); // 単語ごとにランダムに色を割り当て
        ExportColorLog(); // 色分け情報を記録
        lyrics.Add(new LyricLine { time = timeInit, text = "" });
        UpdateLyricsDisplay(); // 初期表示を更新
    }

    void Update()
    {
        // 現在の時刻に基づいて歌詞を更新
        float currentTime = Time.timeSinceLevelLoad;

        // 次の歌詞行に進むべきタイミングか確認
        if (currentLyricIndex < lyrics.Count - 1 && currentTime >= lyrics[currentLyricIndex + 1].time)
        {
            currentLyricIndex++;
            UpdateLyricsDisplay();
        }
    }

    void LoadLrcFile()
    {
        string path = Path.Combine(Application.dataPath, lrcFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"LRC file not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            // LRC形式をパースする正規表現
            if (System.Text.RegularExpressions.Regex.IsMatch(line, @"\[\d+:\d+\.\d+\]"))
            {
                // 時刻部分を取得
                string timePart = line.Substring(1, line.IndexOf("]") - 1);
                string[] timeComponents = timePart.Split(':');
                float minutes = float.Parse(timeComponents[0]);
                float seconds = float.Parse(timeComponents[1]);
                float totalSeconds = timeInit + minutes * 60 + seconds;

                // 歌詞部分を取得
                string textPart = line.Substring(line.IndexOf("]") + 1);

                // リストに追加
                lyrics.Add(new LyricLine { time = totalSeconds, text = textPart });
            }
        }

        Debug.Log($"Loaded {lyrics.Count} lyrics from {lrcFileName}");
    }

    void AssignRandomColors()
    {
        foreach (var line in lyrics)
        {
            string[] words = line.text.Split(' '); // 単語ごとに分割
            foreach (var word in words)
            {
                int randomIndex = Random.Range(0, colors.Length); // ランダムで色を選択
                line.parts.Add(new LyricPart { word = word, color = colors[randomIndex] });
            }
        }
    }

    void ExportColorLog()
    {
        string logPath = Path.Combine(Application.dataPath, "LyricsColorLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Lyrics Color Log:");
            foreach (var line in lyrics)
            {
                writer.WriteLine($"[{line.time:00.00}]");
                foreach (var part in line.parts)
                {
                    string colorName = ColorToName(part.color);
                    writer.WriteLine($"  \"{part.word}\" - {colorName}");
                }
            }
        }
        Debug.Log($"Color log saved to {logPath}");
    }

    string ColorToName(Color color)
    {
        if (color == Color.red) return "RED";
        if (color == Color.green) return "GREEN";
        if (color == Color.yellow) return "Yellow";
        return "UNKNOWN";
    }

    void UpdateLyricsDisplay()
    {
        // 真ん中の行を更新するためのインデックス
        int middleLineIndex = 1;

        for (int i = 0; i < textLines.Length; i++)
        {
            // 表示する歌詞行を決定（前後1行 + 現在行）
            int lyricIndex = currentLyricIndex + i - middleLineIndex;

            if (lyricIndex >= 0 && lyricIndex < lyrics.Count)
            {
                // テキストを色付きで構築
                string coloredText = "";
                foreach (var part in lyrics[lyricIndex].parts)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(part.color);
                    coloredText += $"<color=#{hexColor}>{part.word}</color> ";
                }

                textLines[i].text = coloredText.Trim();

                // 真ん中の行は不透明、それ以外は半透明
                if (i == middleLineIndex)
                {
                    textLines[i].color = new Color(1f, 1f, 1f, 1f); // 不透明
                }
                else
                {
                    textLines[i].color = new Color(1f, 1f, 1f, 0.2f); // 半透明
                }
            }
            else
            {
                // 歌詞がない場合は空白に設定
                textLines[i].text = "";
            }
        }
    }
}
