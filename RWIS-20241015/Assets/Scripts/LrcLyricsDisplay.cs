using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LrcLyricsDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] textField; // 歌詞を表示するTextMeshProUGUIオブジェクト（3行分）
    public string lrcFileName = "Lrc-BirthdaySong.txt"; // LRCファイル名（Assetsフォルダ内）
    private List<LyricLineInfo> lyricsList = new List<LyricLineInfo>(); // 歌詞情報を格納するリスト
    private int currentLyricIndex = 0; // 現在の歌詞インデックス
    private float timeInit = 0; // 歌の始まりの時刻
    private Color[] colors = { Color.red, Color.green, Color.yellow }; // 使用する3色
    private string[] colorNames = { "Red", "Green", "Yellow" }; // 色名

    [System.Serializable]
    public class LyricPartInfo
    {
        public string word; // 単語
        public Color color; // 割り当てられた色
    }

    [System.Serializable]
    public class LyricLineInfo
    {
        public float startTime; // 表示時刻（秒単位）
        public string text; // 歌詞内容
        public List<LyricPartInfo> parts = new List<LyricPartInfo>(); // 単語ごとの色情報
    }

    void Start()
    {
        LoadLrcFile(); // LRCファイルを読み込む
        AssignRandomColors(); // 単語ごとにランダムに色を割り当て
        ExportColorLog(); // 色分け情報を記録
        lyricsList.Add(new LyricLineInfo { startTime = timeInit, text = "" });
        UpdateLyricsDisplay(); // 初期表示を更新
        Debug.Log("lyrics.Count: " + lyricsList.Count);
    }

    /// <summary>
    /// Update 関数：自動的に一定間隔で呼び出される関数
    /// 現在の時刻に基づいて歌詞を更新
    /// </summary>
    void Update()
    {
        // if (lyricsList.Count: 7) then (currentLyricIndex: 0 - 6)
        int numTotalLine = lyricsList.Count;

        // Quit game if finish to display all lyrics 
        if (currentLyricIndex >= numTotalLine - 1)
        {
            ExitApplication();
        }

        // currentTime: 現在のシーンがロードされてからの経過時間を取得
        float currentTime = Time.timeSinceLevelLoad;

        // 次の歌詞行に進むべきタイミングか確認
        LyricLineInfo nextLine = lyricsList[currentLyricIndex + 1];
        if (currentLyricIndex < numTotalLine - 2 && currentTime >= nextLine.startTime)
        {
            Debug.Log("currentLyricIndex: " + currentLyricIndex + ", " + lyricsList[currentLyricIndex].text);
            // Update the index of lyrics
            currentLyricIndex++;
            // Display the lyrics of NEXT line
            UpdateLyricsDisplay();
        }
    }

    /// <summary>
    /// lyricsList 作成
    /// </summary>
    void LoadLrcFile()
    {
        string path = Path.Combine(Application.dataPath, lrcFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"LRC file not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        // 最終行時刻記録用変数
        float timeEndLine = 0f;

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
                float startTime = timeInit + minutes * 60 + seconds;

                // 歌詞部分を取得
                string textPart = line.Substring(line.IndexOf("]") + 1);

                // リストに追加
                lyricsList.Add(new LyricLineInfo { startTime = startTime, text = textPart });
                timeEndLine = startTime;
            }
        }
        // 最後に終わりの指示 EOF てきな
        lyricsList.Add(new LyricLineInfo { startTime = timeEndLine + 4, text = "GAME END." });
        lyricsList.Add(new LyricLineInfo { startTime = timeEndLine + 4, text = "" });
        // 
        Debug.Log("lyricsList: \n");
        foreach (LyricLineInfo lyricsLine in lyricsList)
        {
            Debug.Log(lyricsLine.startTime + ", " + lyricsLine.text + "\n");
        }

        Debug.Log($"Loaded {lyricsList.Count} lyrics from {lrcFileName}");
    }

    void AssignRandomColors()
    {
        foreach (var line in lyricsList)
        {
            string[] wordList = line.text.Split(' '); // 単語ごとに分割
            foreach (var word in wordList)
            {
                int randomIndex = Random.Range(0, colors.Length); // ランダムで色を選択
                line.parts.Add(new LyricPartInfo { word = word, color = colors[randomIndex] });
            }
        }
    }

    void ExportColorLog()
    {
        string logPath = Path.Combine(Application.dataPath, "LyricsColorLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Lyrics Color Log:");
            foreach (var line in lyricsList)
            {
                writer.WriteLine($"[{line.startTime:00.00}]");
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
        if (color == Color.yellow) return "YELLOW";
        return "UNKNOWN";
    }

    void UpdateLyricsDisplay()
    {
        // 真ん中の行を更新するためのインデックス
        int middleLineIndex = 1;

        for (int i = 0; i < textField.Length; i++)
        {
            // 表示する歌詞行を決定（前後1行 + 現在行）
            int lyricIndex = currentLyricIndex + i - middleLineIndex;

            if (lyricIndex >= 0 && lyricIndex < lyricsList.Count)
            {
                // テキストを色付きで構築
                string coloredText = "";
                //string coloredText = (currentLyricIndex+1).ToString();
                foreach (var part in lyricsList[lyricIndex].parts)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(part.color);
                    coloredText += $"<color=#{hexColor}>{part.word}</color> ";
                }

                textField[i].text = coloredText.Trim();

                // 真ん中の行は不透明、それ以外は半透明
                if (i == middleLineIndex)
                {
                    textField[i].color = new Color(1f, 1f, 1f, 1f); // 不透明
                }
                else
                {
                    textField[i].color = new Color(1f, 1f, 1f, 0.2f); // 半透明
                }
            }
            else
            {
                // 歌詞がない場合は空白に設定
                textField[i].text = "";
            }
        }
    }

    void ExitApplication()
    {
#if UNITY_EDITOR
        // Unityエディタの場合、再生モードを停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // ランタイムの場合、アプリケーションを終了
    Application.Quit();
#endif
    }
}
