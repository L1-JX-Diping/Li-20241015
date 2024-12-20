using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Lyrics : MonoBehaviour
{
    // 注意: _textField もし名前変更するなら Inspector のところで TextMeshPro みたいなの Row1,2,3 入れなおして
    public TextMeshProUGUI[] _textField; // 歌詞を表示するTextMeshProUGUIオブジェクト（3行分）
    public string _lyricsFileName = "Lyrics-BirthdaySong.txt"; // 入力ファイル名（Assetsフォルダ内）
    private List<LyricLineInfo> _lyricsList = new List<LyricLineInfo>(); // 歌詞情報(表示開始時刻＋表示する歌詞)を格納するリスト
    private float _loadingTime = 1.0f;
    private int _currentLyricIndex = 0; // 現在の歌詞インデックス
    private float _clock = 3f; // Second per Beat
    private float _beat = 4; // 何拍子か？
    private float _lineStartTime = 0f; // intro 前奏終了時刻 = 歌詞表示(のスクロール用時刻計算)開始時刻
    private Color[] _colorList = { Color.red, Color.green, Color.yellow }; // 使用する3色

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
        if (_currentLyricIndex < _lyricsList.Count - 1 && currentTime >= _lyricsList[_currentLyricIndex + 1].startTime - _loadingTime)
        {
            _currentLyricIndex++;
            UpdateLyricsDisplay();
        }
    }
    void UpdateLyricsDisplay()
    {
        // 真ん中の行を更新するためのインデックス
        int middleLineIndex = 1;

        for (int i = 0; i < _textField.Length; i++)
        {
            // 表示する歌詞行を決定（前後1行 + 現在行）
            int lyricIndex = _currentLyricIndex + i - middleLineIndex;

            if (lyricIndex >= 0 && lyricIndex < _lyricsList.Count)
            {
                // テキストを色付きで構築
                string coloredText = "";
                foreach (var part in _lyricsList[lyricIndex].parts)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(part.color);
                    coloredText += $"<color=#{hexColor}>{part.word}</color> ";
                }

                _textField[i].text = coloredText.Trim();

                // 真ん中の行は不透明、それ以外は半透明
                if (i == middleLineIndex)
                {
                    _textField[i].color = new Color(1f, 1f, 1f, 1f); // 不透明
                }
                else
                {
                    _textField[i].color = new Color(1f, 1f, 1f, 0.2f); // 半透明
                }
            }
            else
            {
                // 歌詞がない場合は空白に設定
                _textField[i].text = "";
            }
        }
    }

    void LoadLyricsFile()
    {
        string path = Path.Combine(Application.dataPath, _lyricsFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"LRC file not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        CreateLyricsList(lines);

        // Debug
        Debug.Log("_lyricsList: \n");
        foreach (LyricLineInfo line in _lyricsList)
        {
            Debug.Log(line.startTime + ", " + line.text);
        }

        Debug.Log($"Loaded {_lyricsList.Count} lyrics from {_lyricsFileName}");
    }

    private void CreateLyricsList(string[] lines)
    {
        // 前奏 intro 部分用
        _lyricsList.Add(new LyricLineInfo { startTime = 0.0f, text = "" });

        // meta info part (1行目) の処理
        // bpm と intro を取得
        if (lines.Length > 0 && lines[0].StartsWith("#"))
        {
            string metaLine = lines[0];
            // 曲の speed 情報
            int bpm = ParseMetaLine(metaLine, "bpm");
            _beat = ParseMetaLine(metaLine, "beat");
            int introEndBeat = ParseMetaLine(metaLine, "intro");
            _clock = 60f / (float)bpm; // clock を計算
            // 歌詞スクロール計算の開始時刻
            _lineStartTime = introEndBeat * _clock; // lyricsStartTime
            Debug.Log($"Parsed BPM: {bpm} beats/min, beat: {_beat} count/bar, intro/startTime(init): {introEndBeat} beats, Clock Interval: {_clock:F2} seconds");
        }
        else
        {
            Debug.LogError("Meta information not found in the first line.");
            return;
        }

        // lyrics part (2 行目以降) の処理
        // 歌詞の表示開始時間情報付き lyricsList を作成
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
                string lyrics = match.Groups[3].Value.Trim(); // 残りの文字列 "Happy birthday to you" を歌詞として取得
                // For Riri // timeList 計算
                //timeList = CalcTimeList(ratioList);

                // 結果を確認
                Debug.Log("bar: " + bar + ", ratioList: [" + string.Join(", ", ratioList) + "]" + "lyrics: " + lyrics);

                // lyricsList に追加
                _lyricsList.Add(new LyricLineInfo { startTime = _lineStartTime, text = lyrics });

                // 次の行の開始時刻計算
                _lineStartTime += _beat * bar * _clock; // 3拍子 * 2小節 --> 6拍子 * 0.5秒/拍子 = 3秒

            }
        }

        // 終了メッセージを追加
        //float endTime = lyricsStartTime + lines.Length * clock;
        _lyricsList.Add(new LyricLineInfo { startTime = _lineStartTime, text = "GAME END." });
        _lyricsList.Add(new LyricLineInfo { startTime = _lineStartTime + 2f, text = "" });
    }

    List<float> CalcTimeList(List<int> ratioList)
    {
        // startTime 更新 (次の行の歌詞表示開始時刻計算)
        int index = 0;
        List<float> timeList = new List<float>();
        foreach(int ratio in ratioList)
        {
            // ratio [/]
            timeList[index] = _lineStartTime + _clock * (float)ratio;
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
        foreach (LyricLineInfo line in _lyricsList)
        {
            // line.text = "" の場合色アサインなしでいい

            // line.text に歌詞がちゃんとある場合
            string[] wordList = line.text.Split(' '); // 単語ごとに分割
            foreach (var word in wordList)
            {
                int randomIndex = Random.Range(0, _colorList.Length); // ランダムで色を選択
                line.parts.Add(new LyricPartInfo { word = word, color = _colorList[randomIndex] });
            }
        }
    }

    void ExportColorLog()
    {
        string logPath = Path.Combine(Application.dataPath, "LyricsColorLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Lyrics Color Log:");
            foreach (var line in _lyricsList)
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
}
