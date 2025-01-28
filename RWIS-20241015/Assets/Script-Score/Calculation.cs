using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Calculation : MonoBehaviour
{
    public TextMeshProUGUI _scoreDisplayText;

    private string _micColorInfoFileName = "MicColorInfo.txt";
    private string _partLogFileName = "PartLog.txt";
    private string _micDetectionLogFileName = "MicDetectionLog.txt";

    private Dictionary<string, string> _micColorDict = new Dictionary<string, string>(); // マイクと色の対応
    private List<PartInfo> _partInfoList = new List<PartInfo>(); // 時刻と色・マイクの正解情報
    private List<Detection> _detectionList = new List<Detection>(); // 時刻ごとの検出情報

    private int _totalScore = 0; // 合計スコア
    private int _maxScore = 0;   // 最大スコア（100点満点換算用）

    [System.Serializable]
    public class PartInfo
    {
        public float time;       // 時刻
        public string color;     // 正解の色
        public string mic;       // 正解のマイク名 ("Robot"含む)
    }

    [System.Serializable]
    public class Detection
    {
        public float time;       // 時刻
        public string mic;       // 検出されたマイク
        public float volume;     // 検出された音量
    }

    void Start()
    {
        LoadMicColorInfo();
        LoadPartLog();
        LoadMicDetectionLog();
        CalculateScore();
    }

    void LoadMicColorInfo()
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
            if (parts.Length == 3)
            {
                string color = parts[0].Trim();
                string mic = parts[1].Trim();
                _micColorDict[mic] = color;
            }
        }

        Debug.Log("MicColorInfo loaded successfully.");
    }

    void LoadPartLog()
    {
        string filePath = Path.Combine(Application.dataPath, _partLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"PartLog file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length == 2 && float.TryParse(parts[0].Trim(), out float time))
            {
                string color = parts[1].Trim();
                string mic = "Robot"; // デフォルトはRobot

                // MicColorDictを基に対応するマイクを検索
                foreach (var pair in _micColorDict)
                {
                    if (pair.Value == color)
                    {
                        mic = pair.Key;
                        break;
                    }
                }

                _partInfoList.Add(new PartInfo 
                { 
                    time = time, 
                    color = color, 
                    mic = mic 
                });
            }
        }

        Debug.Log("PartLog loaded successfully.");
    }

    void LoadMicDetectionLog()
    {
        string filePath = Path.Combine(Application.dataPath, _micDetectionLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"MicDetectionLog file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith("#")) continue; // コメント行をスキップ

            string[] parts = line.Split(',');
            if (parts.Length == 3 && float.TryParse(parts[0].Trim(), out float time) &&
                float.TryParse(parts[2].Trim(), out float volume))
            {
                string mic = parts[1].Trim();
                _detectionList.Add(new Detection 
                { 
                    time = time, 
                    mic = mic, 
                    volume = volume 
                });
            }
        }

        Debug.Log("MicDetectionLog loaded successfully.");
    }

    void CalculateScore()
    {
        _totalScore = 0;
        _maxScore = _partInfoList.Count;

        foreach (var part in _partInfoList)
        {
            // Robotパートは自動的に正解
            if (part.mic == "Robot")
            {
                _totalScore++;
                continue;
            }

            // 該当時刻の最大音量マイクを取得
            Detection maxDetection = null;
            foreach (Detection detection in _detectionList)
            {
                if (Mathf.Approximately(detection.time, part.time))
                {
                    if (maxDetection == null || detection.volume > maxDetection.volume)
                    {
                        maxDetection = detection;
                    }
                }
            }

            // 最大音量マイクが正解と一致するか確認
            if (maxDetection != null && maxDetection.mic == part.mic)
            {
                _totalScore++;
            }
        }

        // スコアを100点満点に換算
        float percentageScore = (float)_totalScore / _maxScore * 100f;
        Debug.Log($"Score: {_totalScore}/{_maxScore} ({percentageScore:F2}%)");
        DisplayScore(percentageScore);
    }

    void DisplayScore(float result)
    {
        string displayText = "";
        //float result = Convert.ToSingle(sum) / 300 * 100;
        displayText += $"{result:00.00}\n";

        if (_scoreDisplayText != null)
        {
            _scoreDisplayText.text = displayText;
        }
        else
        {
            Debug.LogError("ScoreDisplayText is not assigned in the inspector.");
        }
    }

}
