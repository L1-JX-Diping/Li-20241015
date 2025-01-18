using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CheckMicColorUI : MonoBehaviour
{
    // ファイル名
    private string _micColorInfoFileName = "MicColorInfo.txt";

    // マイク名とImageのマッピング
    private Dictionary<string, Image> _micToImageMapDict = new Dictionary<string, Image>();

    // マイク名とAudioClipの対応
    private Dictionary<string, AudioClip> _micClipDict = new Dictionary<string, AudioClip>();

    // サンプルデータ取得用のウィンドウサイズ
    private int _sampleWindow = 128;

    // 光らせるタイマーの管理用
    private float _glowDuration = 0.5f; // 光らせる時間 (秒)
    private float _timer = 0f; // タイマー

    private Image _currentGlowingImage = null; // 現在光っているImage

    public class MicVolumeInfo
    {
        public string mic; // 検出されたマイク名
        public float volume; // 検出された音量
    }

    void Start()
    {
        // ファイルからマイクとImageの対応を読み込み Dict にセットする
        SetMicObjDict();

        // マイクを初期化
        InitializeMicrophones();
    }

    void Update()
    {
        // タイマーを更新
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            // タイマーが0以下なら次のマイクを検出して光らせる
            MicVolumeInfo maxMic = DetectLoudestMic();

            if (maxMic != null && _micToImageMapDict.ContainsKey(maxMic.mic))
            {
                // 前回光らせていたImageの色をリセット
                if (_currentGlowingImage != null)
                {
                    _currentGlowingImage.color = new Color(1f, 1f, 1f, 0.5f); // 半透明の白色
                }

                // 今回のImageを光らせる
                _currentGlowingImage = _micToImageMapDict[maxMic.mic];
                _currentGlowingImage.color = new Color(1f, 1f, 0f, 1f); // 光る黄色

                // タイマーをリセット
                _timer = _glowDuration;
            }
        }
    }

    void SetMicObjDict()
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
            int num = -1;
            num++;

            if (parts.Length == 2)
            {
                string micName = parts[1].Trim();
                string objectName = "Image" + num.ToString();

                GameObject obj = GameObject.Find(objectName);

                if (obj != null && obj.GetComponent<Image>() != null)
                {
                    _micToImageMapDict[micName] = obj.GetComponent<Image>();
                    Debug.Log($"Mapped mic '{micName}' to UI Image '{objectName}'");
                }
                else
                {
                    Debug.LogError($"UI Image '{objectName}' not found in the scene.");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid line format: {line}");
            }
        }
    }

    void InitializeMicrophones()
    {
        foreach (string micName in Microphone.devices)
        {
            Debug.Log($"Initializing microphone: {micName}");

            AudioClip clip = Microphone.Start(micName, true, 10, 44100);
            _micClipDict[micName] = clip;
        }
    }

    MicVolumeInfo DetectLoudestMic()
    {
        MicVolumeInfo micVolume = null;
        float maxVolume = 0;

        foreach (var mic in _micClipDict)
        {
            string micName = mic.Key;
            AudioClip clip = mic.Value;

            if (clip != null && Microphone.IsRecording(micName))
            {
                float volume = GetVolumeLevel(clip);

                if (volume > maxVolume)
                {
                    maxVolume = volume;
                    micVolume = new MicVolumeInfo
                    {
                        mic = micName,
                        volume = volume
                    };
                }
            }
        }

        return micVolume;
    }

    float GetVolumeLevel(AudioClip clip)
    {
        float[] data = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - _sampleWindow + 1;

        if (micPosition < 0) return 0;

        clip.GetData(data, micPosition);

        float maxAmplitude = 0;
        foreach (float sample in data)
        {
            maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(sample));
        }

        return maxAmplitude;
    }
}
