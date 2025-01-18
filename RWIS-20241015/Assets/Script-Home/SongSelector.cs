using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SongSelector : MonoBehaviour
{
    // 
    public GameObject _content; // ScrollView("SelectSong") の Content
    public GameObject _buttonPrefab; // 動的に生成するボタンのプレハブ
    public string _songListFileName = "SongTitleList.txt"; // 歌タイトルリストのファイル名
    private string _outputFilePath = "SongToPlay.txt"; // 記録ファイルのパス
    private List<string> _songList = new List<string>(); // 歌の名前のリスト
    //public float _itemSpacing = 20f; // 選択用歌名ボタン(item)間の間隔（縦方向）

    void Start()
    {
        // リストファイルのパスを設定
        string listFilePath = Path.Combine(Application.dataPath, _songListFileName);

        // リストを読み込み
        if (File.Exists(listFilePath))
        {
            _songList.AddRange(File.ReadAllLines(listFilePath));
            Debug.Log($"Loaded {_songList.Count} songs from {listFilePath}");
        }
        else
        {
            Debug.LogError($"Song list file not found: {listFilePath}");
            return;
        }

        // 記録ファイルのパスを設定
        _outputFilePath = Path.Combine(Application.dataPath, "SongToPlay.txt");

        // 各歌の名前に対応するボタンを生成
        for (int i = 0; i < _songList.Count; i++)
        {
            CreateButton(_songList[i], i+1); // i+1 にするのは選択肢がスクロールボックス域の真ん中に表示されるように
        }
        
        // Contentのサイズを調整
        AdjustContentSize();

    }

    void CreateButton(string songName, int index)
    {
        // ボタンを Content の子オブジェクトとして生成
        GameObject button = Instantiate(_buttonPrefab, _content.transform);

        // ボタンのテキストを設定
        button.GetComponentInChildren<Text>().text = songName;

        // ボタンの位置を調整
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        // x 方向開始位置 50 にするのは選択肢がスクロールボックス域の真ん中に表示されるように
        //buttonRect.anchoredPosition = new Vector2(0, -index * _itemSpacing); // 縦方向に一定間隔を設定

        // ボタンがクリックされたときに記録するイベントを設定
        button.GetComponent<Button>().onClick.AddListener(() => SaveSongTitle(songName));
    }

    void SaveSongTitle(string songName)
    {
        // ファイルに歌の名前を記録
        File.WriteAllText(_outputFilePath, songName);

        // SongTitle を TextBox"" に表示
        Text TextSelectedSongTitle = GameObject.Find("DisplaySongTitle").GetComponent<Text>();
        TextSelectedSongTitle.text = songName;
        Debug.Log($"Song title saved: {songName}");
    }

    void AdjustContentSize()
    {
        // Contentの高さを調整
        RectTransform contentRect = _content.GetComponent<RectTransform>();
        // ContentからGrid Layout Groupコンポーネントを取得
        GridLayoutGroup gridLayoutGroup = _content.GetComponent<GridLayoutGroup>();

        if (gridLayoutGroup != null)
        {
            // Cell Size の Y (高さ) を取得
            float cellHeight = gridLayoutGroup.cellSize.y;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, _songList.Count * cellHeight);

            //Debug.Log($"Cell Height: {cellHeight}");
        }
        else
        {
            Debug.LogError("Grid Layout Group component not found on the Content object.");
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
