using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 扱うので
using UnityEngine.SceneManagement; // Scene の切り替えしたい場合に必要な宣言
using System.IO;

public class ButtonNext : MonoBehaviour
{
    // ボタンに直接アタッチするバージョンのスクリプト
    // でも上手くいかなかった
    // HomeSceneManager で同じような関数使った場合うまくいった (Birthday Song のみ)
    private string _gameInfoFileName = "GameInfo.txt"; // ファイル名
    
    // Start is called before the first frame update
    void Start()
    {
        // ボタンが押されたらこれを実行
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }

    /// <summary>
    /// Read file to get song title (string)
    /// </summary>
    /// <returns></returns>
    string GetSongTitle()
    {
        // ファイルパスの生成
        string filePath = Path.Combine(Application.dataPath, _gameInfoFileName);

        // ファイルが存在するか確認
        if (!File.Exists(filePath))
        {
            Debug.LogError($"GameInfo file not found: {filePath}");
            return "NONE";
        }

        // ファイルを行単位で読み込む
        string[] lines = File.ReadAllLines(filePath);
        string songTitle = "";

        // 1行目から songTitle を取得
        if (lines.Length > 0)
        {
            songTitle = lines[0].Trim(); // 1行目の曲名を取得
        }
        else
        {
            Debug.LogError("GameInfo.txt is empty.");
        }
        return songTitle;
    }

    void SwitchScene()
    {
        // songTitle を取得
        string songTitle = GetSongTitle();
        Debug.Log($"Song Title: {songTitle}");

        if (songTitle == "Birthday Song")
        {
            // "Birthday Song" っであれば
            // Mic-Color 対応をユーザに見せるシーン "AssignColor" を開く
            SceneManager.LoadScene("AssignColor");
        }
        else
        {
            // この歌はまだ準備中だよと知らせるシーン "ComingSoon" を開く
            SceneManager.LoadScene("ComingSoon");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
