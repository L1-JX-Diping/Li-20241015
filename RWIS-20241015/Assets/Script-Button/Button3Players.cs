using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 扱うので
using UnityEngine.SceneManagement; // Scene の切り替えしたい場合に必要な宣言
using System.IO; // ファイル扱うので (Path, StreamWritter を使いたい)

public class Button3Player : MonoBehaviour
{
    private string _outputFileName = "GameInfo.txt";
    private int _playerCount = 3;

    // Start is called before the first frame update
    void Start()
    {
        // ボタンが押されたらこれを実行
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SwitchScene()
    {
        AddSaveGameInfo();
        // シーン ComingSoon を開く
        SceneManager.LoadScene("ComingSoon");
    }

    void AddSaveGameInfo()
    {
        // 記録ファイルのパスを取得
        string filePath = Path.Combine(Application.dataPath, _outputFileName);

        // ファイル全体を読み込む
        string[] lineList;
        if (File.Exists(filePath))
        {
            lineList = File.ReadAllLines(filePath);
        }
        else
        {
            // ファイルが存在しない場合、空の配列を作成
            lineList = new string[0];
        }

        // 2行目を設定
        if (lineList.Length >= 2)
        {
            // 2行目が存在する場合は上書き
            lineList[1] = _playerCount.ToString();
        }
        else
        {
            // 2行目が存在しない場合は新たに追加
            List<string> linesList = new List<string>(lineList);
            while (linesList.Count < 2)
            {
                linesList.Add(""); // 空行を追加
            }
            linesList[1] = _playerCount.ToString();
            lineList = linesList.ToArray();
        }

        // ファイルに書き戻す
        File.WriteAllLines(filePath, lineList);

        Debug.Log($"Player count ({_playerCount}) successfully saved to {filePath}");
    }
}
