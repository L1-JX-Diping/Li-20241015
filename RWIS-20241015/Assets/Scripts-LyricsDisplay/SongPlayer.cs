using System.IO;
using UnityEngine;

public class SongPlayer : MonoBehaviour
{
    public AudioSource _audioSource; // AudioSourceをアタッチ
    public AudioClip _birthdaySongClip; // "Birthday Song" の AudioClip
    public AudioClip _otherSongClip;    // 他の曲のAudioClip
    private string _songName; // GameInfo.txtから取得する曲名
    private int _playerCount;
    private string _gameInfoFileName = "GameInfo.txt";

    void Start()
    {
        ReadAndSetGameData();

        // 曲名に応じて音声を再生
        PlaySongBasedOnName(_songName);
    }

    private void ReadAndSetGameData()
    {
        // GameInfo.txtのファイルパス
        string filePath = Path.Combine(Application.dataPath, _gameInfoFileName);

        // GameInfo.txtを読み込み
        if (File.Exists(filePath))
        {
            // ファイルの内容を行ごとに読み込む
            string[] lineList = File.ReadAllLines(filePath);

            if (lineList.Length >= 2) // ファイルに2行以上あることを確認
            {
                // 1行目を_songNameに格納
                _songName = lineList[0];

                // 2行目を_playerNumに格納（文字列からint型に変換）
                if (int.TryParse(lineList[1], out int playerNum))
                {
                    _playerCount = playerNum;
                }
                else
                {
                    Debug.LogError($"Invalid number format in line 2: {lineList[1]}");
                    _playerCount = 0; // デフォルト値
                }

                Debug.Log($"Song name: {_songName}, Player number: {_playerCount}");
            }
            else
            {
                Debug.LogError("GameInfo.txt does not contain enough lines.");
            }
        }
        else
        {
            Debug.LogError("GameInfo.txt not found.");
        }
    }

    void PlaySongBasedOnName(string songName)
    {
        // 曲名に応じてAudioClipを設定
        if (songName == "Birthday Song")
        {
            _audioSource.clip = _birthdaySongClip;
            Debug.Log("Birthday Song is selected.");
        }
        else
        {
            _audioSource.clip = _otherSongClip; // 他の曲を再生
        }

        // AudioClipが設定されていれば再生
        if (_audioSource.clip != null)
        {
            _audioSource.Play();
            Debug.Log($"Playing song: {_audioSource.clip.name}");
        }
        else
        {
            Debug.LogError("No AudioClip assigned for the given song name.");
        }
    }
}
