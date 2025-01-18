using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MicDetection : MonoBehaviour
{
    private float _detectionThreshold = 0.01f; // 音声を検知する振幅の閾値
    private int _sampleWindow = 128; // 音量解析に使用するサンプル数
    private string _outputFileName = "MicDetectionLog.txt"; // 記録するファイル名
    private int _index = 0;

    private List<AudioClip> _microphoneClips = new List<AudioClip>(); // マイクからの入力を保存
    private List<string> _microphoneNames = new List<string>(); // 接続されているマイクの名前
    private List<MicVolumeInfo> _detectedList = new List<MicVolumeInfo>(); // 検出データの記録

    private List<StartSingInfo> _timingList = new List<StartSingInfo>(); // 歌い出しタイミングリスト

    [System.Serializable]
    public class StartSingInfo
    {
        public float timing; // タイミング
        public string color; // 色
    }

    [System.Serializable]
    public class MicVolumeInfo
    {
        public float time; // 検出時刻
        public string mic; // 検出されたマイク名
        public float volume; // 検出された音量
    }

    void Start()
    {
        // PartLogファイルを読み込む
        ReadPartLog();

        // マイクの初期設定
        InitializeMicrophones();
    }
    //private void InitializeMicrophones()
    //{
    //    // USBマイクを保持するリスト
    //    List<string> usbMicrophones = new List<string>();

    //    // 接続されているすべてのマイクを取得
    //    foreach (string device in Microphone.devices)
    //    {
    //        Debug.Log($"Detected microphone: {device}");

    //        // パソコン本体のマイクは無視し、USB で接続したマイクのみ検出
    //        // マイク名に "WebCam" を含むかどうかをチェック
    //        if (device.ToLower().Contains("WebCam"))
    //        {
    //            usbMicrophones.Add(device);
    //            Debug.Log($"USB Microphone found: {device}");

    //            // USBマイク入力の録音を開始
    //            AudioClip clip = Microphone.Start(device, true, 10, 44100);
    //            _microphoneNames.Add(device);
    //            _microphoneClips.Add(clip);
    //        }
    //        else
    //        {
    //            Debug.Log($"Skipping non-USB microphone: {device}");
    //        }
    //    }

    //    // USBマイクが見つからない場合のエラーハンドリング
    //    if (usbMicrophones.Count == 0)
    //    {
    //        Debug.LogError("No USB microphones detected. Please connect a USB microphone.");
    //    }
    //}

    /// <summary>
    /// 接続されたマイクを検出
    /// </summary>
    private void InitializeMicrophones()
    {
        // 接続されているマイクを取得
        if (Microphone.devices.Length > 0)
        {
            foreach (string device in Microphone.devices)
            {
                _microphoneNames.Add(device);
                Debug.Log($"Found microphone: {device}");

                // マイク入力の録音を開始
                AudioClip clip = Microphone.Start(device, true, 10, 44100);
                _microphoneClips.Add(clip);
            }
        }
        else
        {
            Debug.LogError("No microphones detected.");
        }
    }

    void Update()
    {
        float detectionTime = Time.timeSinceLevelLoad;

        // 現在のパートを歌い始めるべき時間になったら声の検出を開始
        if (_index < _timingList.Count && _timingList[_index].timing <= detectionTime)
        {
            // 各マイクについて音声を検出
            for (int i = 0; i < _microphoneClips.Count; i++)
            {
                string micName = _microphoneNames[i];
                AudioClip micClip = _microphoneClips[i];

                if (micClip != null && Microphone.IsRecording(micName))
                {
                    // 現在の音量を取得
                    float volume = GetVolumeLevel(micClip);

                    // 音量が閾値を超えたら音声を検知
                    if (volume > _detectionThreshold)
                    {
                        Debug.Log($"Voice detected from {micName} at {detectionTime:F2} seconds, Volume: {volume:F4}");
                        _detectedList.Add(new MicVolumeInfo
                        {
                            time = detectionTime,
                            mic = micName,
                            volume = volume
                        });
                    }
                }
            }
            _index++;
        }
    }

    private float GetVolumeLevel(AudioClip micClip)
    {
        // サンプルデータを取得
        float[] data = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - _sampleWindow + 1;
        if (micPosition < 0) return 0;

        micClip.GetData(data, micPosition);

        // サンプルデータの最大振幅を計算
        float maxAmplitude = 0;
        foreach (float sample in data)
        {
            maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(sample));
        }

        return maxAmplitude;
    }

    private void ReadPartLog()
    {
        string filePath = Path.Combine(Application.dataPath, "PartLog.txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            // コンマで区切って分割
            string[] parts = line.Split(',');

            if (parts.Length == 2 && float.TryParse(parts[0].Trim(), out float timing))
            {
                string color = parts[1].Trim();
                _timingList.Add(new StartSingInfo { timing = timing, color = color });
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        // 音声検知時間をファイルに記録
        string filePath = Path.Combine(Application.dataPath, _outputFileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"# time, mic, volume");
            foreach (var info in _detectedList)
            {
                writer.WriteLine($"{info.time:F2}, {info.mic}, {info.volume:F4}");
            }
        }

        Debug.Log($"Voice detection log saved to: {filePath}");
    }
}
