using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // UI 扱うので
using UnityEngine.SceneManagement;
using TMPro; // Scene の切り替えしたい場合に必要な宣言

public class MicColorAssignment : MonoBehaviour
{
    private string _outputFileName = "MicColorInfo.txt"; // 出力ファイル名
    private List<string> _microphoneNameList = new List<string>(); // 検出されたマイク名
    private List<ColorInfo> _assignmentInfoList = new List<ColorInfo>(); // 割り当てたマイクと色の情報
    private List<string> _colorNameList = new List<string> { "GREEN", "RED", "YELLOW" }; // 使用する色
    private Color[] _colorList = { Color.green, Color.red, Color.yellow }; // 使用する3色

    // インスペクターで手動で割り当てる場合に使用
    public Text _textboxMic1Color;
    public Text _textboxMic2Color;

    // Home シーンで x = 2 人で遊ぶを選んだら GameInfo.txt にそれが出力される
    // ここでは GameInfo.txt 読み込んで x 個のマイクを検出し、色を割り当てる

    [System.Serializable]
    public class ColorInfo
    {
        public string microphone; // マイク名
        public string colorName; // 割り当てた色
    }

    void Start()
    {
        // color assignment 
        AssignColorsToMicrophones();

        // 画面にマイクと色の対応を表示する
        WriteIntoTextbox();

        // パートの色とマイクの対応を保存
        SaveColorInfoToFile();

        // ボタンが押されたらこれを実行
        //GameObject.Find("ButtonStart").GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        // ゲーム画面へ GO
        SwitchScene();
    }

    void SwitchScene()
    {
        // Gameシーン "DisplayLyrics" を開く
        SceneManager.LoadScene("DisplayLyrics");
    }

    private void WriteIntoTextbox()
    {
        // 名前で UI オブジェクトを探す
        Text mic1ColorField = GameObject.Find("Color-Mic1").GetComponent<Text>();
        Text mic1NameField = GameObject.Find("Name-Mic1").GetComponent<Text>();

        Text mic2ColorField = GameObject.Find("Color-Mic2").GetComponent<Text>();
        Text mic2NameField = GameObject.Find("Name-Mic2").GetComponent<Text>();

        // 
        Text mic3ColorField = GameObject.Find("Color-Mic3").GetComponent<Text>();
        Text mic3NameField = GameObject.Find("Name-Mic3").GetComponent<Text>();

        // mic 1
        // テキストを設定
        if (mic1ColorField != null)
        {
            string colorName = _assignmentInfoList[0].colorName;
            mic1ColorField.text = colorName; // Color-Mic1 にパート色表示
            mic1ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic1 is not assigned.");
        }
        if (mic1NameField != null)
        {
            mic1NameField.text = _assignmentInfoList[0].microphone; // Name-Mic1 にマイク名表示
        }
        else
        {
            Debug.LogError("Textbox Name-Mic1 is not assigned.");
        }

        // mic 2
        if (mic2ColorField != null)
        {
            string colorName = _assignmentInfoList[1].colorName;
            mic2ColorField.text = colorName; // Color-Mic2 にパート色表示
            mic2ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic2 is not assigned.");
        }
        if (mic2NameField != null)
        {
            mic2NameField.text = _assignmentInfoList[1].microphone; // Name-Mic2 にマイク名表示
        }
        else
        {
            Debug.LogError("Textbox Name-Mic2 is not assigned.");
        }

        // mic 3 機械用
        if (mic3ColorField != null)
        {
            string colorName = "";
            string assignedColor0 = _assignmentInfoList[0].colorName;
            string assignedColor1 = _assignmentInfoList[1].colorName;

            // _colorNameListから未使用の色を検索
            foreach (string name in _colorNameList)
            {
                if (name != assignedColor1 && name != assignedColor0)
                {
                    colorName = name; // 未使用の色を設定
                    break; // 最初に見つけた未使用の色でループを終了
                }
            }
            mic3ColorField.text = colorName; // Color-Mic3 にパート色表示
            mic3ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic1 is not assigned.");
        }
        if (mic3NameField != null)
        {
            mic3NameField.text = "Robot Part"; // Name-Mic3 は機械が担当, 自動で満点換算
        }
        else
        {
            Debug.LogError("Textbox Name-Mic1 is not assigned.");
        }

    }

    private void AssignColorsToMicrophones()
    {
        // マイクデバイスを取得
        foreach (string deviceName in Microphone.devices)
        {
            // パソコン本体のマイクは含めない
            if (deviceName == "マイク配列 (Realtek(R) Audio)")
            {
                continue;
            }

            // USB で接続されたマイクのみリストに追加
            _microphoneNameList.Add(deviceName);
        }

        if (_microphoneNameList.Count == 0)
        {
            Debug.LogError("No microphones detected.");
            return;
        }

        Debug.Log($"Detected {_microphoneNameList.Count} microphones.");

        // 色をランダムに割り当て

        // 候補となる数字 index of color
        List<int> numberList = new List<int> { 0, 1, 2 };

        // 結果を格納するリスト
        List<int> indexList = SelectTwoRandomIndexes(numberList);

        // 結果をログ出力
        Debug.Log($"Selected indexes: {indexList[0]}, {indexList[1]}");

        // 色を割り当て
        for (int i = 0; i < _microphoneNameList.Count; i++)
        {
            string assignedColor = _colorNameList[indexList[i]]; // 色を順番に割り当て

            // 構造体に追加
            _assignmentInfoList.Add(new ColorInfo
            {
                microphone = _microphoneNameList[i],
                colorName = assignedColor
            });

            Debug.Log($"Assigned color {assignedColor} to {_microphoneNameList[i]}");
        }
    }

    List<int> SelectTwoRandomIndexes(List<int> numberList)
    {
        List<int> result = new List<int>();

        // シャッフルして上位2つを取得
        for (int i = numberList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (numberList[i], numberList[randomIndex]) = (numberList[randomIndex], numberList[i]);
        }

        result.Add(numberList[0]);
        result.Add(numberList[1]);

        return result;
    }

    private void SaveColorInfoToFile()
    {
        string filePath = Path.Combine(Application.dataPath, _outputFileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var colorInfo in _assignmentInfoList)
            {
                writer.WriteLine($"{colorInfo.colorName}, {colorInfo.microphone}");
                // RED, マイク (Logi C615 HD WebCam)
                // みたいな形式で保存される
            }
        }

        Debug.Log($"Color information saved to {filePath}");
    }

    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    Color NameToColor(string colorName)
    {
        if (colorName == "RED") return Color.red;
        if (colorName == "GREEN") return Color.green;
        if (colorName == "YELLOW") return Color.yellow;
        return Color.white;
    }
}
