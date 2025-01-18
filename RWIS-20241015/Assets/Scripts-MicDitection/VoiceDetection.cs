using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class VoiceDetection : MonoBehaviour
{
    private float _detectionThreshold = 0.01f; // 音声を検知する振幅の閾値
    private int _sampleWindow = 128; // 音量解析に使用するサンプル数
    private string _outputFileName = "VoiceDetectionLog.txt"; // 記録するファイル名
    private int _index = 0;

    private AudioClip _microphoneClip; // マイクからの入力を保存
    private List<AudioClip> _microphoneClipList; // 複数マイクからの入力を保存
    
    // 読み込みファイル 歌い出し時刻の情報
    private string _partLogFileName = "PartLog.txt"; // ファイル名
    // 読み込んだ各パート歌い出し時刻データを格納するリスト
    private List<StartSingInfo> _timingList = new List<StartSingInfo>(); 
    // マイクを複数使う場合用
    private List<string> _microphoneNameList = new List<string>();

    // Time Mic 格納するリスト // 音声が検出された秒数やVolumeを記録
    private List<MicVolumeInfo> _detectedList = new List<MicVolumeInfo>();
    string _microphoneName = "";

    [System.Serializable]
    public class StartSingInfo
    {
        public float timing; // タイミング, 時刻
        public string color; // パート割, 色
    }

    [System.Serializable]
    public class MicVolumeInfo
    {
        public float time; // タイミング, 時刻
        public string mic; // パート割, 色
        public float volume; // 検出した声の大きさ
    }

    // Start is called before the first frame update
    void Start()
    {
        // set info from PartLog file
        ReadPartLog();

        // Mic 関連の初期設定やら記録用設定諸々
        SetMic();
    }

    private void SetMic()
    {
        // マイクが接続されているか確認
        if (Microphone.devices.Length > 0)
        {
            _microphoneName = Microphone.devices[0];
            Debug.Log($"Using microphone: {_microphoneName}");

            // マイク入力の録音を開始
            _microphoneClip = Microphone.Start(_microphoneName, true, 10, 44100);
        }
        else
        {
            Debug.LogError("No microphone detected.");
        }

        //// マイクが接続されているか確認
        //int micNum = Microphone.devices.Length;
        //if (micNum > 0)
        //{
        //    //string microphoneName = Microphone.devices[0];

        //    // mic が複数接続されている場合
        //    int micIndex = 0;
        //    while (micIndex < micNum)
        //    {
        //        _microphoneNameList[micIndex] = Microphone.devices[micIndex];
        //        Debug.Log($"Using microphone: {_microphoneNameList[micIndex]}");
        //        // マイク入力の録音を開始
        //        _microphoneClipList.Add(Microphone.Start(_microphoneNameList[micIndex], true, 10, 44100));
        //    }
        //}
        //else
        //{
        //    Debug.LogError("No microphone detected.");
        //}
    }

    // Update is called once per frame
    void Update()
    {
        float detectionTime = Time.timeSinceLevelLoad;

        // 現在のパートを歌い始めるべき時間になったら声の検出を開始
        if (_timingList[_index].timing <= detectionTime)
        {
            // 一回だけ検知 ピッタリタイミングで歌えていなかったら反映されない
            // 結構ハードモード
            if (_microphoneClip != null && Microphone.IsRecording(null))
            {
                // 現在の音量を取得
                float volume = GetVolumeLevel();

                // 音量が閾値を超えたら音声を検知
                if (volume > _detectionThreshold)
                {
                    //Debug.Log($"Voice detected at {detectionTime:F2} seconds, Volume: {volume:F4}");

                    // 記録
                    _detectedList.Add(new MicVolumeInfo { 
                        time = detectionTime, 
                        mic = _microphoneName, 
                        volume = volume
                    });
                }
            }
            _index++;
            //Debug.Log($"_index: {_index}");
        }
    }
    void ReadPartLog()
    {
        string filePath = Path.Combine(Application.dataPath, _partLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        string[] lineList = File.ReadAllLines(filePath);

        foreach (string line in lineList)
        {
            // コンマで区切って分割
            string[] partInfo = line.Split(',');

            if (partInfo.Length == 2)
            {
                // タイミングと色を解析してリストに追加
                if (float.TryParse(partInfo[0].Trim(), out float startTime))
                {
                    string color = partInfo[1].Trim();
                    _timingList.Add(new StartSingInfo { timing = startTime, color = color });
                }
                else
                {
                    Debug.LogWarning($"Invalid timing format in line: {line}");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }
    }

    private float GetVolumeLevel()
    {
        // サンプルデータを取得
        float[] data = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - _sampleWindow + 1;
        if (micPosition < 0) return 0;

        _microphoneClip.GetData(data, micPosition);

        // サンプルデータの最大振幅を計算
        float maxAmplitude = 0;
        foreach (float sample in data)
        {
            maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(sample));
        }

        return maxAmplitude;
    }

    private void OnApplicationQuit()
    {
        // 音声検知時間をファイルに記録
        string filePath = System.IO.Path.Combine(Application.dataPath, _outputFileName);
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
        {
            //writer.WriteLine("Voice Detection Log:");

            // time と mic の情報も出力できたらいい
            foreach (var info in _detectedList)
            {
                writer.WriteLine($"{info.time:F2}, {info.mic}, {info.volume}");
            }
        }

        Debug.Log($"Voice detection times saved to: {filePath}");
    }

}
