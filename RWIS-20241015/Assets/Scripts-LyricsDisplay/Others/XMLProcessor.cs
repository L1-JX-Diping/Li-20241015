using System.IO;
using System.Xml;
using UnityEngine;
public class XMLProcessor : MonoBehaviour
{
    public string inputFileName = "Lyrics"; // Resourcesフォルダ内のXMLファイル名（拡張子不要）
    public string outputFileName = "Orders.xml"; // 出力するXMLファイル名
    void Start()
    {
        // Resourcesフォルダから入力ファイルをロード
        TextAsset xmlAsset = Resources.Load<TextAsset>(inputFileName);
        if (xmlAsset == null)
        {
            Debug.LogError($"Input file not found in Resources: {inputFileName}.xml");
            return;
        }
        // 出力パスを`Assets/`配下に設定
        string outputFilePath = Path.Combine(Application.dataPath, outputFileName);
        try
        {
            // 入力データを処理し、出力ファイルを生成
            using (StringReader stringReader = new StringReader(xmlAsset.text))
            using (XmlReader reader = XmlReader.Create(stringReader))
            using (XmlWriter writer = XmlWriter.Create(outputFilePath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Lyrics");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Line")
                    {
                        string originalLine = reader.ReadElementContentAsString();
                        string processedLine = RandomWordSplit(originalLine); // 単語単位で処理
                        writer.WriteElementString("Line", processedLine);
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                Debug.Log($"Processed XML saved to {outputFilePath}");
            }
#if UNITY_EDITOR
            // UnityエディタでAssetsフォルダを更新
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing XML: {ex.Message}");
        }
    }
    // ランダムに数単語ごとにカンマを挿入
    private string RandomWordSplit(string line)
    {
        // 単語単位で分割
        string[] words = line.Split(' ');
        string result = "";
        int wordCount = 0;
        for (int i = 0; i < words.Length; i++)
        {
            result += words[i].Trim();
            wordCount++;
            // ランダムに区切り文字を追加（1から2単語単位で分割）
            if (wordCount >= UnityEngine.Random.Range(1, 3) && i < words.Length - 1)
            {
                result += ",";
                wordCount = 0;
            }
            else if (i < words.Length - 1)
            {
                result += " ";
            }
        }
        return result;
    }
}