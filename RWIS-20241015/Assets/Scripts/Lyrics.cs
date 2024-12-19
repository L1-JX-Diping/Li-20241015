using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Lyrics : MonoBehaviour
{
    public TextMeshProUGUI[] textField; // 歌詞を表示するTextMeshProUGUIオブジェクト（3行分）
    public string lyricsFileName = "Lyrics-BirthdaySong.txt"; // 入力ファイル名（Assetsフォルダ内）
    private List<LyricLineInfo> lyricsList = new List<LyricLineInfo>(); // 歌詞情報を格納するリスト
    private int currentLyricIndex = 0; // 現在の歌詞インデックス
    private float lyricsStartTime = 0f; // 歌の始まりの時刻
    private float clock = 3f; // Second per Beat
    private float beat = 4; // 何拍子か？
    private float lineStartTime = 0f; // intro 前奏終了時刻 = 歌詞表示(のスクロール用時刻計算)開始時刻
    private Color[] colorList = { Color.red, Color.green, Color.yellow }; // 使用する3色

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
        LoadLyricsFile(); // ファイルを読み込む
        AssignRandomColors(); // 単語ごとにランダムに色を割り当て
        ExportColorLog(); // 色分け情報を記録
        UpdateLyricsDisplay(); // 初期表示を更新
    }

    void Update()
    {
        // 現在の時刻に基づいて歌詞を更新
        float currentTime = Time.timeSinceLevelLoad;

        // 次の歌詞行に進むべきタイミングか確認
        if (currentLyricIndex < lyricsList.Count - 1 && currentTime >= lyricsList[currentLyricIndex + 1].startTime - 0.2f)
        {
            currentLyricIndex++;
            UpdateLyricsDisplay();
        }
    }

    void LoadLyricsFile()
    {
        string path = Path.Combine(Application.dataPath, lyricsFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"LRC file not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // 1行目から bpm と intro を取得
        if (lines.Length > 0 && lines[0].StartsWith("#"))
        {
            string metaLine = lines[0];
            // 曲の speed 情報
            int bpm = ParseMetaLine(metaLine, "bpm");
            beat = ParseMetaLine(metaLine, "beat");
            int introEndBeat = ParseMetaLine(metaLine, "intro");
            clock = 60f / (float)bpm; // clock を計算
            // 歌詞スクロール計算開始時刻
            lyricsStartTime = introEndBeat * clock;
            Debug.Log($"Parsed BPM: {bpm} beats/min, beat: {beat} count/bar, intro/startTime(init): {introEndBeat} beats, Clock Interval: {clock:F2} seconds");
        }
        else
        {
            Debug.LogError("Meta information not found in the first line.");
            return;
        }

        //* 2 行目以降を歌詞として処理 *//
        for (int i = 1; i < lines.Length; i++)
        {
            string lyricsPart = lines[i];
            // Line ごとに更新
            List<int> ratioList = new List<int>();
            List<float> timeList = new List<float>();

            // 正規表現を使用してデータを抽出 // 例: 2[0,1,3,4]Happy birthday to you
            Regex regex = new Regex(@"(\d+)\[([0-9,]+)\](.*)");
            Match match = regex.Match(lyricsPart);
            if (match.Success)
            {
                // 小節: bar と 時刻比率List: ratioList を抽出
                int bar = int.Parse(match.Groups[1].Value); // `2` を bar に保存

                foreach (string timeRatio in match.Groups[2].Value.Split(','))
                {
                    ratioList.Add(int.Parse(timeRatio)); // `[0,1,3,4]` をリストに変換
                }
                string lyrics = match.Groups[3].Value.Trim(); // 残りの文字列を歌詞として取得

                // 結果を確認
                Debug.Log("bar: " + bar);
                Debug.Log("ratioList: [" + string.Join(", ", ratioList) + "]");
                Debug.Log("lyrics: " + lyrics);

                // 2 行目だけ特殊処理: 歌詞 1 行目 
                if (i == 1)
                {
                    // シーン開始と同時に表示させる startTime = 0.0f
                    lyricsList.Add(new LyricLineInfo { startTime = 0.0f, text = lyrics });
                    lineStartTime = lyricsStartTime;
                    // For Riri
                    //timeList = CalcTimeList(ratioList);

                    Debug.Log("continue;");
                    continue;
                }

                Debug.Log("No continue;");
                // 3 行目以降 // この歌詞行の開始時刻
                lineStartTime += beat * bar * clock; // 3拍子 * 2小節 --> 6拍子 * 0.5秒/拍子 = 3秒
                // For Riri // timeList 計算
                //timeList = CalcTimeList(ratioList);

                // lyricsList に追加
                lyricsList.Add(new LyricLineInfo { startTime = lineStartTime, text = lyrics });
            }
        }

        // 終了メッセージを追加
        //float endTime = lyricsStartTime + lines.Length * clock;
        lyricsList.Add(new LyricLineInfo { startTime = lineStartTime + 3f, text = "" });
        lyricsList.Add(new LyricLineInfo { startTime = lineStartTime + 3f, text = "GAME END." });

        Debug.Log($"Loaded {lyricsList.Count} lyrics from {lyricsFileName}");
    }

    List<float> CalcTimeList(List<int> ratioList)
    {
        // startTime 更新 (次の行の歌詞表示開始時刻計算)
        int index = 0;
        List<float> timeList = new List<float>();
        foreach(int ratio in ratioList)
        {
            // ratio [/]
            timeList[index] = lineStartTime + clock * (float)ratio;
            Debug.Log($"timeList[{index}]: {timeList[index]}");
            index++;
        }
        return timeList;
    }

    int ParseMetaLine(string metaLine, string key)
    {
        // 指定されたキーの値を正規表現で取得
        var match = System.Text.RegularExpressions.Regex.Match(metaLine, $@"{key}\[(\d+)\]");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
        {
            return value;
        }

        Debug.LogWarning($"Failed to parse {key} from: {metaLine}");
        return 0;
    }

    void AssignRandomColors()
    {
        foreach (var line in lyricsList)
        {
            string[] wordList = line.text.Split(' '); // 単語ごとに分割
            foreach (var word in wordList)
            {
                int randomIndex = Random.Range(0, colorList.Length); // ランダムで色を選択
                line.parts.Add(new LyricPartInfo { word = word, color = colorList[randomIndex] });
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
}
