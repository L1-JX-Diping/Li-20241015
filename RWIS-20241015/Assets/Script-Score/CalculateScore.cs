using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CalculateScore : MonoBehaviour
{
    // MicColorInfo.txt を読み込んで色のリスト作成・スコア計算する必要
    private string _micColorInfoFileName = "MicColorInfo.txt"; // マイクとパート色の対応が記されたファイル名
    // MicColorInfo.txt からマイクとパート色の対応を Dictionary に格納
    Dictionary<string, string> _micColorDict = new Dictionary<string, string>(); // key:mic, value:colorName
    private string _robotPartColor = "";
    private List<string> _colorNameList = new List<string> { "GREEN", "RED", "YELLOW" }; // 使用する色
    // PartLog.txt から時刻と正解のパート色情報を取得
    private string _partLogFileName = "PartLog.txt"; // 時刻と割り当てられた正解のパート色が記されたファイル名
    List<TimeColorInfo> _timeColorInfoList = new List<TimeColorInfo>();

    private int _countBottom = 0; // Bottom 分母
    private int _countCorrect = 0; // Top 分子

    // MicDetectionLog.txt から時刻と使用されたマイクの情報取得
    private string _micDetectionLogFileName = "MicDetectionLog.txt";

    [System.Serializable]
    public class TimeColorInfo
    {
        public float time; // 表示時刻（秒単位）
        public string assignedColor; // 正解のパート色割り当て
        public List<Detection> micVolumeList; // 実際に歌った人 最もvolumeの大きいmicを判定
    }

    [System.Serializable]
    public class Detection
    {
        public string micColor; // 歌った人の持ち色 マイク名から判定
        public float volume; // volume
    }

    // Start is called before the first frame update
    void Start()
    {
        // _micColorDict 
        SetMicColorDict();
        
        // 
        SetCorrectPartInfo();
        ProcessMicDetectionLog();

        // 
        SetRobotPart();
        CalcScore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CalcScore()
    {
        _countBottom = 0; // 分母（全体の判定数）
        _countCorrect = 0; // 分子（正解数）

        foreach (TimeColorInfo timeColorInfo in _timeColorInfoList)
        {
            // 分母を増加（総判定数）
            _countBottom++;

            float maxVolume = 0f;
            Detection micUsed = null;

            // micVolumeListから最大音量のマイクを見つける
            foreach (Detection micStatus in timeColorInfo.micVolumeList)
            {
                if (micStatus.volume > maxVolume)
                {
                    maxVolume = micStatus.volume;
                    micUsed = micStatus;
                }
            }

            // 最大音量のマイクが存在する場合
            if (micUsed != null)
            {
                string correctColor = timeColorInfo.assignedColor;
                if (correctColor == _robotPartColor)
                {
                    _countCorrect++; // 正解数を増加
                }
                // 正解のパート色と比較して判定
                if (correctColor == micUsed.micColor)
                {
                    _countCorrect++; // 正解数を増加
                }
            }
        }

        // スコア計算 (分母がゼロの場合を考慮)
        float scoreResult = _countBottom > 0 ? (float)_countCorrect / _countBottom : 0f;

        // Text として画面上に描画
        DisplayScore(scoreResult);

        // スコアをデバッグログに出力
        Debug.Log($"Score: {_countCorrect}/{_countBottom} ({scoreResult * 100:F2}%)");
    }

    // _robotPartColor に _colorNameList にある三色のうち、_micColorDict のどの value にも一致しない色を設定
    void SetRobotPart()
    {
        // 使用中の色を収集
        HashSet<string> usedColors = new HashSet<string>(_micColorDict.Values);

        // _colorNameListから未使用の色を見つける
        foreach (string color in _colorNameList)
        {
            if (!usedColors.Contains(color))
            {
                _robotPartColor = color;
                Debug.Log($"Robot part assigned color: {_robotPartColor}");
                return;
            }
        }

        // 全色が使用中の場合のデフォルト動作
        Debug.LogWarning("All colors are used. No color assigned to the robot part.");
        _robotPartColor = "NONE"; // デフォルト値
    }

    void DisplayScore(float scoreResult)
    {
        //float scoreResult = _countBottom > 0 ? (float)_countCorrect / _countBottom : 0f;

        // スコアをデバッグログに出力
        Debug.Log($"Final Score: {_countCorrect}/{_countBottom} ({scoreResult * 100:F2}%)");

        // UI要素に表示する場合（TextMeshProUGUIを使用）
        GameObject scoreTextObj = GameObject.Find("ScoreText"); // シーンにScoreTextオブジェクトがあることを想定
        if (scoreTextObj != null)
        {
            var scoreText = scoreTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (scoreText != null)
            {
                scoreText.text = $"Score: {_countCorrect}/{_countBottom}\n({scoreResult * 100:F2}%)";
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found on ScoreText object.");
            }
        }
        else
        {
            Debug.LogWarning("ScoreText object not found in the scene.");
        }
    }


    void ProcessMicDetectionLog()
    {
        // ファイルパスを取得
        string filePath = Path.Combine(Application.dataPath, _micDetectionLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"MicDetectionLog file not found: {filePath}");
            return;
        }

        // ファイルを読み込み
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            // 行をカンマで分割
            string[] parts = line.Split(',');

            if (parts.Length == 3)
            {
                // 時刻を取得
                // time, micName, volume の形式 
                // 例: 4.00, マイク (Logi C615 HD WebCam), 0.0109
                if (float.TryParse(parts[0].Trim(), out float detectedTime))
                {
                    string micName = parts[1].Trim();
                    // Dict よりマイク名から対応する色へ変換
                    string micColorName = _micColorDict[micName];
                    if (float.TryParse(parts[2].Trim(), out float volume))
                    {
                        // 対応する TimeColorInfo を探して追加
                        foreach (TimeColorInfo timeColorInfo in _timeColorInfoList)
                        {
                            // 時刻が一致する場合
                            if (Mathf.Approximately(timeColorInfo.time, detectedTime))
                            {
                                timeColorInfo.micVolumeList.Add(new Detection
                                {
                                    micColor = micColorName,
                                    volume = volume
                                });
                                Debug.Log($"Added Detection: Time={detectedTime}, Mic={micName}, Volume={volume}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid volume format in line: {line}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid time format in line: {line}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }

        Debug.Log("Processing of MicDetectionLog completed.");
    }

    void SetCorrectPartInfo()
    {
        // ファイルパスを生成
        string filePath = Path.Combine(Application.dataPath, _partLogFileName);

        // ファイルが存在するか確認
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Part log file not found: {filePath}");
            return;
        }

        // ファイルを読み込み
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            // 行をカンマで分割
            string[] parts = line.Split(',');

            if (parts.Length == 2) // 正しい形式か確認
            {
                // 時刻をパース
                if (float.TryParse(parts[0].Trim(), out float time))
                {
                    string colorName = parts[1].Trim();

                    // リストに追加
                    _timeColorInfoList.Add(new TimeColorInfo
                    {
                        time = time,
                        assignedColor = colorName
                    });
                }
                else
                {
                    Debug.LogWarning($"Invalid time format in line: {line}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }

        Debug.Log($"Loaded {_timeColorInfoList.Count} entries from part log.");
    }

    void SetMicColorDict()
    {
        string filePath = Path.Combine(Application.dataPath, _micColorInfoFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"MicColorInfo file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length == 2)
            {
                // RED, micName の形式
                string colorName = parts[0].Trim();
                string micName = parts[1].Trim();

                _micColorDict[micName] = colorName;
                Debug.Log($"'{micName}' '{colorName}'");
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }
    }

}
