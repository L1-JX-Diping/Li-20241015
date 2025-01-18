using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 扱うので
using UnityEngine.SceneManagement; // Scene の切り替えしたい場合に必要な宣言
using System.IO;

public class ButtonGameStart : MonoBehaviour
{
    // ボタンに直接アタッチするバージョンのスクリプト
    // でも上手くいかなかった
    // HomeSceneManager で同じような関数使った場合うまくいった (Birthday Song のみ)
    private string _gameInfoFileName = "GameInfo.txt"; // ファイル名
    private string _songTitle; // 1行目に格納される曲名
    
    // Start is called before the first frame update
    void Start()
    {
        // _songTitle を取得
        ReadGameInfo();
        Debug.Log($"Song Title: {_songTitle}");

        // ボタンが押されたらこれを実行
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }
    void ReadGameInfo()
    {
        // ファイルパスの生成
        string filePath = Path.Combine(Application.dataPath, _gameInfoFileName);

        // ファイルが存在するか確認
        if (!File.Exists(filePath))
        {
            Debug.LogError($"GameInfo file not found: {filePath}");
            return;
        }

        // ファイルを行単位で読み込む
        string[] lines = File.ReadAllLines(filePath);

        // 1行目から_songTitleを取得
        if (lines.Length > 0)
        {
            _songTitle = lines[0].Trim(); // 1行目の曲名を取得
        }
        else
        {
            Debug.LogError("GameInfo.txt is empty.");
        }
    }

    void SwitchScene()
    {
        if (_songTitle == "Birthday Song")
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
