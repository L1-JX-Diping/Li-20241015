using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MicUsageRecorder : MonoBehaviour
{
    private string[] microphones; // 検出されたマイクのリスト
    private string currentMicrophone = ""; // 現在使用中のマイク
    private bool isRecording = false; // 録音中フラグ
    private float startTime = 0f; // 録音開始時刻
    private List<MicUsage> micUsageLog = new List<MicUsage>(); // マイク使用情報のリスト
    private AudioClip recordingClip;

    // マイク使用情報を保持するクラス
    private class MicUsage
    {
        public string Mic;       // マイク名
        public float TimeCount;  // 使用時間
        public float StartTime;  // 開始時刻
        public float EndTime;    // 終了時刻
    }

    void Start()
    {
        // 接続されているマイクを検出
        microphones = Microphone.devices;

        if (microphones.Length == 0)
        {
            Debug.LogError("No microphones detected.");
            return;
        }

        Debug.Log("Detected microphones:");
        foreach (var mic in microphones)
        {
            Debug.Log($"- {mic}");
        }

        // 最初のマイクで録音開始
        StartRecording(microphones[0]);
    }

    void Update()
    {
        if (isRecording)
        {
            // 現在の録音経過時間を取得
            float currentTime = Time.timeSinceLevelLoad;

            Debug.Log($"Recording on {currentMicrophone}: StartTime = {startTime:F2}, Elapsed = {currentTime - startTime:F2}");

            // 録音を切り替える場合（例：スペースキーで切り替え）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopRecording();
                SwitchMicrophone();
            }

            // 録音を終了（例：エスケープキーで終了）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopRecording();
                SaveUsageLog();
            }
        }
    }

    void StartRecording(string microphone)
    {
        if (Microphone.devices.Length == 0 || string.IsNullOrEmpty(microphone))
        {
            Debug.LogError("No microphone available to start recording.");
            return;
        }

        if (isRecording)
        {
            StopRecording();
        }

        currentMicrophone = microphone;
        recordingClip = Microphone.Start(microphone, true, 10, 44100); // 最大10秒のループ録音
        startTime = Time.timeSinceLevelLoad;
        isRecording = true;

        Debug.Log($"Started recording with microphone: {currentMicrophone}");
    }

    void StopRecording()
    {
        if (string.IsNullOrEmpty(currentMicrophone) || !isRecording)
        {
            return;
        }

        // 録音を停止
        Microphone.End(currentMicrophone);
        float endTime = Time.timeSinceLevelLoad;
        float timeCount = endTime - startTime;

        // 使用情報を記録
        micUsageLog.Add(new MicUsage
        {
            Mic = currentMicrophone,
            TimeCount = timeCount,
            StartTime = startTime,
            EndTime = endTime
        });

        Debug.Log($"Stopped recording with {currentMicrophone}. TimeCount = {timeCount:F2}");
        isRecording = false;
    }

    void SwitchMicrophone()
    {
        // 他のマイクに切り替える
        if (microphones.Length < 2)
        {
            Debug.LogWarning("Only one microphone available. Cannot switch.");
            return;
        }

        int currentIndex = System.Array.IndexOf(microphones, currentMicrophone);
        int nextIndex = (currentIndex + 1) % microphones.Length;

        StartRecording(microphones[nextIndex]);
    }

    void SaveUsageLog()
    {
        string logPath = Path.Combine(Application.dataPath, "MicUsage.txt");

        using (StreamWriter writer = new StreamWriter(logPath))
        {
            // 1行目に変数名を記録
            writer.WriteLine("Mic,TimeCount,StartTime,EndTime");

            // 各使用情報を記録
            foreach (var usage in micUsageLog)
            {
                writer.WriteLine($"{usage.Mic},{usage.TimeCount:F2},{usage.StartTime:F2},{usage.EndTime:F2}");
            }
        }

        Debug.Log($"Mic usage log saved to {logPath}");
    }
}
