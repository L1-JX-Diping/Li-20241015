using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class TestAudio : MonoBehaviour
{
    //[SerializeField]
    //private string audioPath = "Audio/BirthdaySong";
    public string audioPath = "Audio/BirthdaySong"; // Resourcesフォルダ内のAudioClipのパス（拡張子不要）
    private string _jsonFileName = "AudioMapping.json"; // JSON ファイル名（Resources フォルダ内）
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>(); // 曲名と AudioClip のマッピング

    void Start()
    {
        // ResourcesフォルダからAudioClipをロード
        AudioClip clip = Resources.Load<AudioClip>(audioPath);
        Debug.Log(clip != null ? "Clip loaded successfully." : "Failed to load clip.");

        if (clip != null)
        {
            //Vector3 position = Camera.main.transform.position;
            Vector3 position = Vector3.zero;

            // シーン原点（Vector3.zero）で音声を再生
            AudioSource.PlayClipAtPoint(clip, position, 1.0f); // 再生成功
            Debug.Log($"Playing Audio: {audioPath}");
        }
        else
        {
            Debug.LogError($"Failed to load AudioClip from path: {audioPath}");
        }

        // JSONファイルのパス
        string jsonFilePath = Path.Combine(Application.dataPath, "Resources", _jsonFileName);
        string jsonContent = File.ReadAllText(jsonFilePath);
        Dictionary<string, string> audioPaths = JsonUtility.FromJson<Dictionary<string, string>>(jsonContent);
        //InitializeAudioClipsAndDebug();
    }

    void InitializeAudioClipsAndDebug()
    {
        Debug.Log("InitializeAudioClips() START");

        // JSONファイルのパス
        string jsonFilePath = Path.Combine(Application.dataPath, "Resources", _jsonFileName);

        // JSONファイルを読み込む
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            Debug.Log($"JSON Content: {jsonContent}"); // JSONの内容をログに表示

            // JSONデータをラッパークラスを使ってパース
            AudioPathWrapper wrapper = JsonUtility.FromJson<AudioPathWrapper>(jsonContent);

            if (wrapper != null && wrapper.paths != null)
            {
                Debug.Log("audioPaths parsed successfully!");

                // audioPathsの中身を確認
                foreach (var entry in wrapper.paths)
                {
                    Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
                }

                // Dictionaryに追加
                foreach (var entry in wrapper.paths)
                {
                    string songName = entry.Key;
                    string audioClipPath = entry.Value;

                    // ResourcesフォルダからAudioClipをロード
                    AudioClip songClip = Resources.Load<AudioClip>(audioClipPath);
                    if (songClip != null)
                    {
                        _audioClips.Add(songName, songClip);
                        Debug.Log($"Loaded AudioClip: {songName} from {audioClipPath}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load AudioClip for {songName} from path: {audioClipPath}");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to parse JSON or paths is null.");
            }
        }
        else
        {
            Debug.LogError($"JSON file not found: {jsonFilePath}");
        }
    }

}
