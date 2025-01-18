using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerWithJSON : MonoBehaviour
{
    //private AudioSource _audioSource; // AudioSourceをアタッチ
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>(); // 曲名と AudioClip のマッピング
    private string _jsonFileName = "AudioMapping.json"; // JSON ファイル名（Resources フォルダ内）
    private string _infoFileName = "GameInfo.txt";

    void Start()
    {
        // AudioClipをDictionaryに登録
        InitializeAudioClips();

        // GameInfo.txtのファイルパス
        string filePath = Path.Combine(Application.dataPath, _infoFileName);

        // GameInfo.txtを読み込み
        if (File.Exists(filePath))
        {
            // 1 行目 (プレイする歌の名前) を取得
            string songNameToPlay = ReadFirstLine(filePath);
            Debug.Log($"Song name retrieved from GameInfo.txt: {songNameToPlay}");

            // 曲名に応じて伴奏を再生
            PlaySong(songNameToPlay);
        }
        else
        {
            Debug.LogError("GameInfo.txt not found.");
        }
    }

    //void InitializeAudioClips()
    //{
    //    Debug.Log("InitializeAudioClips() START");

    //    // JSONファイルのパス
    //    string jsonFilePath = Path.Combine(Application.dataPath, "Resources", _jsonFileName);

    //    // JSONファイルを読み込む
    //    if (File.Exists(jsonFilePath))
    //    {
    //        string jsonContent = File.ReadAllText(jsonFilePath);
    //        Debug.Log($"JSON Content: {jsonContent}"); // JSONの内容をログに表示

    //        // JSONデータをラッパークラスを使ってパース
    //        AudioPathWrapper wrapper = JsonUtility.FromJson<AudioPathWrapper>(jsonContent);

    //        if (wrapper != null && wrapper.paths != null)
    //        {
    //            Debug.Log("audioPaths parsed successfully!");

    //            // audioPathsの中身を確認
    //            foreach (var entry in wrapper.paths)
    //            {
    //                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
    //            }

    //            // Dictionaryに追加
    //            foreach (var entry in wrapper.paths)
    //            {
    //                string songName = entry.Key;
    //                string audioClipPath = entry.Value;

    //                // ResourcesフォルダからAudioClipをロード
    //                AudioClip songClip = Resources.Load<AudioClip>(audioClipPath);
    //                if (songClip != null)
    //                {
    //                    _audioClips.Add(songName, songClip);
    //                    Debug.Log($"Loaded AudioClip: {songName} from {audioClipPath}");
    //                }
    //                else
    //                {
    //                    Debug.LogError($"Failed to load AudioClip for {songName} from path: {audioClipPath}");
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to parse JSON or paths is null.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError($"JSON file not found: {jsonFilePath}");
    //    }
    //}

    void InitializeAudioClips()
    {
        //Debug.Log("InitializeAudioClips() START");
        // JSONファイルのパス
        string jsonFilePath = Path.Combine(Application.dataPath, "Resources", _jsonFileName);

        // JSONファイルを読み込む
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);

            // file の情報を Dictionary に代入
            Dictionary<string, string> audioPaths = JsonUtility.FromJson<Dictionary<string, string>>(jsonContent);
            //Dictionary<string, string> audioPaths = JsonUtility.FromJson<AudioPathWrapper>(jsonContent).paths;

            // Debug
            //Debug.Log($"JSON Content: {jsonContent}");
            //if (audioPaths == null)
            //{
            //    Debug.LogError("audioPaths is null.");
            //}

            // Debug
            //Debug.Log("audioPaths.Count" + audioPaths.Count);

            foreach (var audioInfo in audioPaths)
            {
                string songName = audioInfo.Key;
                string audioClipPath = audioInfo.Value;
                //Debug.Log($"songName: {songName}, audioClipPath: {audioClipPath}");

                // Resources フォルダから AudioClip をロード
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
            Debug.LogError($"JSON file not found: {jsonFilePath}");
        }
    }

    string ReadFirstLine(string filePath)
    {
        // ファイルの 1 行目を取得
        string[] lines = File.ReadAllLines(filePath);
        return lines.Length > 0 ? lines[0] : string.Empty;
    }

    void PlaySong(string songName)
    {
        // カメラの位置でなく、原点で再生するとトラブルシューティングしやすいらしい
        Vector3 position = Vector3.zero; // シーン原点で再生
        //Vector3 position = Camera.main.transform.position;
       
        // 曲名が Dictionary に登録されている場合、その AudioClip を再生
        if (_audioClips.TryGetValue(songName, out AudioClip clip))
        {
            //_audioSource.clip = clip;
            //_audioSource.Play();
            AudioSource.PlayClipAtPoint(clip, position);
            Debug.Log($"Playing song: {songName}");
        }
        else
        {
            Debug.LogError($"Song '{songName}' not found in the audio clips dictionary.");
        }
    }

    // 動的に曲を追加する
    public void AddSong(string songName, string clipPath)
    {
        AudioClip clip = Resources.Load<AudioClip>(clipPath);
        if (clip != null)
        {
            _audioClips[songName] = clip; // 存在する場合は上書き
            Debug.Log($"Added new song: {songName} from path: {clipPath}");
        }
        else
        {
            Debug.LogError($"Failed to load AudioClip for {songName} from path: {clipPath}");
        }
    }
}

// JSONの形式をサポートするためのラッパークラス
[System.Serializable]
public class AudioPathWrapper
{
    public Dictionary<string, string> paths;
}
