using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    private string currentMicrophone = "";  // 現在使用中のマイク
    private float startTime = 0f;           // 録音開始時刻
    private bool isRecording = false;      // 録音中かどうかのフラグ
    private AudioClip recordingClip;       // 録音データ
    private Dictionary<string, float> microphoneUsageTimes = new Dictionary<string, float>(); // マイク使用時間

    void Start()
    {
        // 接続されているすべてのマイクを取得
        string[] microphones = Microphone.devices;

        if (microphones.Length == 0)
        {
            Debug.LogError("No microphones detected.");
            return;
        }

        // マイク一覧を表示
        Debug.Log("Detected microphones:");
        foreach (var mic in microphones)
        {
            Debug.Log($"- {mic}");
            microphoneUsageTimes[mic] = 0f; // 各マイクの使用時間を初期化
        }

        // 最初のマイクを選択して録音開始
        StartRecording(microphones[0]);
    }

    void Update()
    {
        if (isRecording)
        {
            // 現在の録音経過時間を取得
            float elapsedTime = Time.time - startTime;

            // 現在の録音位置を取得（サンプル単位）
            int position = Microphone.GetPosition(currentMicrophone);
            Debug.Log($"Recording on {currentMicrophone}: {elapsedTime:F2} seconds (Position: {position} samples)");

            // エスケープキーで録音を停止
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopRecording();
            }
        }

        // 他のマイクに切り替える
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMicrophone();
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

        // マイクを選択して録音開始
        currentMicrophone = microphone;
        recordingClip = Microphone.Start(microphone, true, 10, 44100); // 最大10秒のループ録音
        startTime = Time.time;
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
        float elapsedTime = Time.time - startTime;

        // 使用時間を記録
        if (microphoneUsageTimes.ContainsKey(currentMicrophone))
        {
            microphoneUsageTimes[currentMicrophone] += elapsedTime;
        }

        Debug.Log($"Stopped recording with {currentMicrophone}. Total usage time: {microphoneUsageTimes[currentMicrophone]:F2} seconds.");
        isRecording = false;

        // Save the mic usage information
        SaveUsageLog();
    }


    void SwitchMicrophone()
    {
        // 他のマイクに切り替え
        string[] microphones = Microphone.devices;
        if (microphones.Length < 2)
        {
            Debug.LogWarning("Only one microphone available. Cannot switch.");
            return;
        }

        // 現在のマイクのインデックスを取得
        int currentIndex = System.Array.IndexOf(microphones, currentMicrophone);
        int nextIndex = (currentIndex + 1) % microphones.Length;

        Debug.Log($"Switching to microphone: {microphones[nextIndex]}");
        StartRecording(microphones[nextIndex]);
    }

    void SaveUsageLog()
    {
        string logPath = Path.Combine(Application.dataPath, "MicrophoneUsageLog.txt");
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Microphone Usage Log:");
            foreach (var entry in microphoneUsageTimes)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value:F2} seconds");
            }
        }
        Debug.Log($"Usage log saved to {logPath}");
    }

}
