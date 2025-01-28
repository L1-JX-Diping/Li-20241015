using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using ColorUtility = UnityEngine.ColorUtility;
using UnityEngine.UI;


public class LyricsController : MonoBehaviour
{
    // 注意: _textField もし名前変更するなら Inspector のところで TextMeshPro みたいなの Row1,2,3 入れなおして
    public TextMeshProUGUI[] _textField; // 歌詞を表示するTextMeshProUGUIオブジェクト（3行分）

    /* private な変数たち */
    private string _markDictFileName = "MarkColorDict.txt";
    //private string _songTitle = "";
    //private int _playerCount = 0;
    // 色弱用 GREEN, *(=Heart) 色とマークの対応
    private Dictionary<string, string> _markDict = new Dictionary<string, string>(); 
    private string _lyricsFileName = "Lyrics-BirthdaySong.txt"; // 入力ファイル名（Assetsフォルダ内）
    private List<LyricLineInfo> _lyricsList = new List<LyricLineInfo>(); // 歌詞情報(表示開始時刻＋表示する歌詞)を格納するリスト
    private float _loadingTime = 0.8f; // time lag
    private int _currentLyricIndex = 0; // 現在の歌詞インデックス
    private float _clock = 3f; // Second per Beat
    private float _beat = 4; // 何拍子か？ Birthday song は 3 拍子
    private string _eofText = "GAME END.";
    private float _lineStartTime = 0f; // intro 前奏終了時刻 = 歌詞表示(のスクロール用時刻計算)開始時刻
    // 使用する3色
    private Color[] _colorList = 
    { 
        Color.red, 
        Color.green, 
        Color.yellow 
    }; 
    private int _indexForNumList = 0;

    /// <summary>
    /// Which song do you want to play? Set/Get the FILE NAME
    /// </summary>
    public string LyricsFileName
    {
        get => _lyricsFileName;
        set => _lyricsFileName = value;
    }

    /// <summary>
    /// LyricsList has the information of each line's StartTime/Text/
    /// </summary>
    public List<LyricLineInfo> LyricsList
    {
        get => _lyricsList;
        set => _lyricsList = value;
    }

    /// <summary>
    /// Get/Set Lag(LoadingTime); To adjust the song accompaniment with the lyrics display scrolling
    /// </summary>
    public float LoadingTime
    {
        get => _loadingTime;
        set => _loadingTime = value;
    }

    /// <summary>
    /// Clock: second per beat
    /// </summary>
    public float Clock
    {
        get => _clock;
        set => _clock = value;
    }

    [System.Serializable]
    public class LyricPartInfo
    {
        public float timing; // タイミング
        public string word; // 単語
        public Color color; // 割り当てられた色
        public string mark; // 色弱用: 割り当てられた色に対応するマーク
    }

    [System.Serializable]
    public class LyricLineInfo
    {
        public float startTime; // 表示時刻（秒単位）
        public string text; // 歌詞内容
        public List<LyricPartInfo> partList = new List<LyricPartInfo>(); // 単語ごとの色情報
    }

    void Start()
    {
        InitMarkDict(); // パートと Mark の対応作成
        InitAvatarColor(); // 画面上にアバターの色を反映する
        LoadLyricsFile(); // ファイルを読み込む

        ExportColorLog(); // 色分け情報を記録
        ExportPartLog(); // パート分け情報を記録
        UpdateLyricsDisplay(); // 初期表示を更新
    }

    void Update()
    {
        // 現在の時刻に基づいて歌詞を更新
        float currentTime = Time.timeSinceLevelLoad;

        // 次の歌詞行に進むべきタイミングか確認
        if (_currentLyricIndex < LyricsList.Count - 1 && currentTime >= LyricsList[_currentLyricIndex + 1].startTime - LoadingTime)
        {
            _currentLyricIndex++;
            UpdateLyricsDisplay();
        }
    }

    void InitAvatarColor()
    {
        foreach (var info in _markDict)
        {
            string colorName = info.Key;
            Color color = NameToColor(colorName);
            string markName = info.Value;
            //string mark = MarkToChar(markName);
            Debug.Log($"from _markDict: {colorName}, {markName}");
            ReflectToAvatar(markName, color); // Heart, Color.green
        }
        // Club 
        ReflectToAvatar("Club", Color.gray);
    }

    void ReflectToAvatar(string avatarName, Color assignedColor)
    {
        //objName = "Avatar1" 
        // Canvas内の objName オブジェクトを探す
        GameObject avatarObject = GameObject.Find(avatarName);

        // オブジェクトが見つかった場合
        if (avatarObject != null)
        {
            // Imageコンポーネントを取得
            Image avatarImage = avatarObject.GetComponent<Image>();

            if (avatarImage != null)
            {
                // ImageのColorプロパティに色を設定
                avatarImage.color = assignedColor;
                Debug.Log($"Color {assignedColor} applied to {avatarName}.");
            }
            else
            {
                Debug.LogError($"{avatarName} does not have an Image component.");
            }
        }
        else
        {
            Debug.LogError($"{avatarName} object not found in the scene.");
        }
    }

    private void UpdateLyricsDisplay()
    {
        // 真ん中の行を更新するためのインデックス
        int middleLineIndex = 1;

        for (int i = 0; i < _textField.Length; i++)
        {
            // 表示する歌詞行を決定（前後1行 + 現在行）
            int lyricIndex = _currentLyricIndex + i - middleLineIndex;

            if (lyricIndex >= 0 && lyricIndex < LyricsList.Count)
            {
                // テキストを色付きで構築
                string coloredText = "";
                foreach (LyricPartInfo part in LyricsList[lyricIndex].partList)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(part.color);
                    coloredText += $"<color=#{hexColor}>{part.mark}{part.word}</color> ";
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

    void InitMarkDict()
    {
        // ファイルパスを取得
        string filePath = Path.Combine(Application.dataPath, _markDictFileName);

        // ファイルの存在確認
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        // ファイルを行ごとに読み込む
        string[] lineList = File.ReadAllLines(filePath);

        foreach (string line in lineList)
        {
            // カンマで分割
            string[] pairColorMark = line.Split(',');

            if (pairColorMark.Length == 2)
            {
                string colorName = pairColorMark[0].Trim();  // 1列目: colorName (例: GREEN)
                string markName = pairColorMark[1].Trim();   // 3列目: markName (例: Spade)

                // Dictionaryに追加
                if (!_markDict.ContainsKey(colorName))
                {
                    _markDict[colorName] = markName;
                }
                else
                {
                    Debug.LogWarning($"Duplicate color entry found: {colorName}, ignoring the second entry.");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }

        Debug.Log($"Initialized _markDict with {_markDict.Count} entries.");
    }

    void LoadLyricsFile()
    {
        string path = Path.Combine(Application.dataPath, LyricsFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"LRC file not found: {path}");
            return;
        }

        string[] lyricsLineList = File.ReadAllLines(path);
        CreateLyricsList(lyricsLineList);

        // Debug
        DebugToConsole();

        Debug.Log($"Loaded {LyricsList.Count} lyrics from {LyricsFileName}");
    }

    private void DebugToConsole()
    {
        Debug.Log("_lyricsList: \n");
        foreach (LyricLineInfo line in LyricsList)
        {
            Debug.Log(line.startTime + ", " + line.text);
        }
    }

    private void CreateLyricsList(string[] lyricsLineList)
    {
        // パート色割り当て用
        List<int> randomNumList = CreateRandomNumList(3);

        // 前奏 intro 部分用
        LyricsList.Add(new LyricLineInfo { startTime = 0.0f, text = "" });

        // meta info part (1行目) の処理
        // bpm と intro を取得
        if (lyricsLineList.Length > 0 && lyricsLineList[0].StartsWith("#"))
        {
            string metaLine = lyricsLineList[0];
            // 曲の speed 情報
            int bpm = ParseMetaLine(metaLine, "bpm");
            _beat = ParseMetaLine(metaLine, "beat");
            int introEndBeat = ParseMetaLine(metaLine, "intro");
            Clock = 60f / (float)bpm; // clock を計算
            // 歌詞スクロール計算の開始時刻
            _lineStartTime = introEndBeat * Clock; // lyricsStartTime
            Debug.Log($"Parsed BPM: {bpm} beats/min, beat: {_beat} count/bar, intro/startTime(init): {introEndBeat} beats, Clock Interval: {Clock:F2} seconds");
        }
        else
        {
            Debug.LogError("Meta information not found in the first line.");
            return;
        }

        // lyrics part (2 行目以降) の処理
        // 歌詞の表示開始時間情報付き lyricsList を作成
        for (int i = 1; i < lyricsLineList.Length; i++)
        {
            string lyricsInfo = lyricsLineList[i];
            // Line ごとに更新
            List<int> ratioList = new List<int>();

            // 正規表現を使用してデータを抽出 
            // 例: 2[0,1,3,4]Happy birthday to you
            Regex regex = new Regex(@"(\d+)\[([0-9,]+)\](.*)");
            Match match = regex.Match(lyricsInfo);
            if (!match.Success)
            {
                Debug.LogError("match unsuccessful. Please write lyrics information to input file like \"2[0,1,3,4]Happy birthday to you\"");
                continue;
            }

            /* match.Success なら
             * 小節数: bar と 時刻比率List: ratioList を抽出 */
            // 表示「行」の小節数 `2` を bar に保存
            int barCount = int.Parse(match.Groups[1].Value);

            foreach (string timeRatio in match.Groups[2].Value.Split(','))
            {
                // パート開始タイミング `[0,1,3,4]` をリストに変換
                ratioList.Add(int.Parse(timeRatio));
            }
            // 残りの文字列 "Happy birthday to you" を歌詞として取得
            string lyrics = match.Groups[3].Value.Trim();

            // この歌詞行について part 情報をセット
            List<LyricPartInfo> partInfoList = SetPartInfoListForThisLine(randomNumList, ratioList, barCount, lyrics);

            // lyricsList に追加
            LyricsList.Add(new LyricLineInfo
            {
                startTime = _lineStartTime,
                text = lyrics,
                partList = partInfoList
            });
            // 次の行の開始時刻計算
            _lineStartTime += _beat * barCount * Clock; // 6拍 (3拍子 * 2小節) * 0.5秒/拍 = 3秒
        }

        // 終了メッセージを追加
        //float endTime = lyricsStartTime + lines.Length * clock;
        LyricsList.Add(new LyricLineInfo
        {
            startTime = _lineStartTime,
            text = _eofText
        });
        LyricsList.Add(new LyricLineInfo
        {
            startTime = _lineStartTime + 2f,
            text = ""
        });

    }

    private List<LyricPartInfo> SetPartInfoListForThisLine(List<int> randomNumList, List<int> ratioList, int barCount, string lyrics)
    {
        List<LyricPartInfo> partInfoList = new List<LyricPartInfo>();
        /* LyricLineInfo の partList 情報生成*/
        partInfoList = new List<LyricPartInfo>();
        // この行の歌詞を単語ごとに分割
        string[] wordList = lyrics.Split(' ');
        int index = 0;
        foreach (float timeRatio in ratioList)
        {
            float haku = _beat * barCount; // この行の総拍数
            float addSecond = timeRatio / haku;
            float partStartTime = _lineStartTime + addSecond;
            //Debug.Log($"ratioList:{timeRatio}, partStartTime:{partStartTime}");

            // generate part List
            string word = wordList[index];
            Color color = _colorList[randomNumList[_indexForNumList]];
            string colorName = ColorToName(color);
            string markName = _markDict[colorName];
            string mark = MarkToChar(markName); // markName to mark (例: ♠)
            Debug.Log($"add _markDict: {colorName}, {mark}");

            // part 情報格納
            LyricPartInfo part = new LyricPartInfo
            {
                timing = partStartTime,
                word = word,
                color = color,
                mark = mark
            };
            partInfoList.Add(part);
            index++;
            _indexForNumList++;
        }

        return partInfoList;
    }

    List<int> CreateRandomNumList(int num)
    {
        List<int> resultList = new List<int>();
        List<int> candidateList = GenerateCandidateList(num);
        int maxLength = 200; // 作成するリストの長さ（必要に応じて変更）
        int maxRepeats = 2; // 同じ数字が連続できる最大回数

        while (resultList.Count < maxLength)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidateList.Count);
            int selectedNum = candidateList[randomIndex];

            // 連続回数をチェック
            if (resultList.Count >= maxRepeats &&
                resultList[resultList.Count - 1] == selectedNum &&
                resultList[resultList.Count - 2] == selectedNum)
            {
                // 条件を満たさない場合は再選択
                continue;
            }

            // 条件を満たす場合、リストに追加
            resultList.Add(selectedNum);
        }

        return resultList;
    }

    List<int> GenerateCandidateList(int num)
    {
        List<int> candidateList = new List<int>();

        // 0から(num-1)までの整数をリストに追加
        for (int i = 0; i < num; i++)
        {
            candidateList.Add(i);
        }

        return candidateList;
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

    void ExportColorLog()
    {
        string logPath = Path.Combine(Application.dataPath, "LyricsColorLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Lyrics Color Log:");
            foreach (LyricLineInfo thisLine in LyricsList)
            {
                if (thisLine.text == "" || thisLine.text == _eofText)
                {
                    continue;
                }

                // XX.XX という形式で開始時刻をファイルに書き込み
                writer.WriteLine($"[{thisLine.startTime:00.00}]");

                foreach (LyricPartInfo part in thisLine.partList)
                {
                    // パートの歌詞と色を出力
                    writer.WriteLine($"  \"{part.word}\" - {part.color}");
                }
            }
        }
        Debug.Log($"Color log saved to {logPath}");
    }

    void ExportPartLog()
    {
        string logPath = Path.Combine(Application.dataPath, "PartLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            //writer.WriteLine("Part Log:");
            foreach (LyricLineInfo thisLine in LyricsList)
            {
                if (thisLine.text == "" || thisLine.text == _eofText)
                {
                    continue;
                }

                // XX.XX という形式で開始時刻をファイルに書き込み
                //writer.WriteLine($"[{thisLine.startTime:00.00}] {thisLine.text}");

                foreach (LyricPartInfo part in thisLine.partList)
                {
                    // 誰のパートか？開始時間は？
                    string name = ColorToName(part.color);
                    writer.WriteLine($"{part.timing:00.00}, {name}");
                }
            }
        }
        Debug.Log($"Color log saved to {logPath}");
    }

    /// <summary>
    /// GET color name from color (Type: Color)
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    string ColorToName(Color color)
    {
        if (color == Color.red) return "RED";
        if (color == Color.green) return "GREEN";
        if (color == Color.yellow) return "YELLOW";
        if (color == Color.blue) return "BLUE";

        return "UNKNOWN";
    }

    /// <summary>
    /// GET color from color name (Type: string)
    /// </summary>
    /// <param name="colorName"></param>
    /// <returns></returns>
    Color NameToColor(string colorName)
    {
        if (colorName == "RED") return Color.red;
        if (colorName == "GREEN") return Color.green;
        if (colorName == "YELLOW") return Color.yellow;
        if (colorName == "BLUE") return Color.blue;

        return Color.white;
    }


    string MarkToChar(string markName)
    {
        if (markName == "Heart") return "♥";
        if (markName == "Spade") return "♠";
        if (markName == "Diamond") return "♦";
        if (markName == "Club") return "♣";

        return "*"; // 全員で歌う部分
    }

    //string MarkToChar(string markName)
    //{
    //    if (markName == "Heart") return "*";
    //    if (markName == "Spade") return "!";
    //    if (markName == "Diamond") return "+";
    //    if (markName == "Club") return "#";

    //    string chorusMark = "";
    //    int i = 0;
    //    while (i < _playerCount)
    //    {
    //        chorusMark += "<";
    //        i++;
    //    }
    //    return chorusMark; // 全員で歌う部分
    //}


}
