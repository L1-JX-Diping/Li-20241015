using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // UI �����̂�
using UnityEngine.SceneManagement;
using TMPro; // Scene �̐؂�ւ��������ꍇ�ɕK�v�Ȑ錾

public class MicColorAssignment : MonoBehaviour
{
    private string _outputFileName = "MicColorInfo.txt"; // �o�̓t�@�C����
    private List<string> _microphoneNameList = new List<string>(); // ���o���ꂽ�}�C�N��
    private List<ColorInfo> _assignmentInfoList = new List<ColorInfo>(); // ���蓖�Ă��}�C�N�ƐF�̏��
    private List<string> _colorNameList = new List<string> { "GREEN", "RED", "YELLOW" }; // �g�p����F
    private Color[] _colorList = { Color.green, Color.red, Color.yellow }; // �g�p����3�F

    // �C���X�y�N�^�[�Ŏ蓮�Ŋ��蓖�Ă�ꍇ�Ɏg�p
    public Text _textboxMic1Color;
    public Text _textboxMic2Color;

    // Home �V�[���� x = 2 �l�ŗV�Ԃ�I�񂾂� GameInfo.txt �ɂ��ꂪ�o�͂����
    // �����ł� GameInfo.txt �ǂݍ���� x �̃}�C�N�����o���A�F�����蓖�Ă�

    [System.Serializable]
    public class ColorInfo
    {
        public string microphone; // �}�C�N��
        public string colorName; // ���蓖�Ă��F
    }

    void Start()
    {
        // color assignment 
        AssignColorsToMicrophones();

        // ��ʂɃ}�C�N�ƐF�̑Ή���\������
        WriteIntoTextbox();

        // �p�[�g�̐F�ƃ}�C�N�̑Ή���ۑ�
        SaveColorInfoToFile();

        // �{�^���������ꂽ�炱������s
        //GameObject.Find("ButtonStart").GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        // �Q�[����ʂ� GO
        SwitchScene();
    }

    void SwitchScene()
    {
        // Game�V�[�� "DisplayLyrics" ���J��
        SceneManager.LoadScene("DisplayLyrics");
    }

    private void WriteIntoTextbox()
    {
        // ���O�� UI �I�u�W�F�N�g��T��
        Text mic1ColorField = GameObject.Find("Color-Mic1").GetComponent<Text>();
        Text mic1NameField = GameObject.Find("Name-Mic1").GetComponent<Text>();

        Text mic2ColorField = GameObject.Find("Color-Mic2").GetComponent<Text>();
        Text mic2NameField = GameObject.Find("Name-Mic2").GetComponent<Text>();

        // 
        Text mic3ColorField = GameObject.Find("Color-Mic3").GetComponent<Text>();
        Text mic3NameField = GameObject.Find("Name-Mic3").GetComponent<Text>();

        // mic 1
        // �e�L�X�g��ݒ�
        if (mic1ColorField != null)
        {
            string colorName = _assignmentInfoList[0].colorName;
            mic1ColorField.text = colorName; // Color-Mic1 �Ƀp�[�g�F�\��
            mic1ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic1 is not assigned.");
        }
        if (mic1NameField != null)
        {
            mic1NameField.text = _assignmentInfoList[0].microphone; // Name-Mic1 �Ƀ}�C�N���\��
        }
        else
        {
            Debug.LogError("Textbox Name-Mic1 is not assigned.");
        }

        // mic 2
        if (mic2ColorField != null)
        {
            string colorName = _assignmentInfoList[1].colorName;
            mic2ColorField.text = colorName; // Color-Mic2 �Ƀp�[�g�F�\��
            mic2ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic2 is not assigned.");
        }
        if (mic2NameField != null)
        {
            mic2NameField.text = _assignmentInfoList[1].microphone; // Name-Mic2 �Ƀ}�C�N���\��
        }
        else
        {
            Debug.LogError("Textbox Name-Mic2 is not assigned.");
        }

        // mic 3 �@�B�p
        if (mic3ColorField != null)
        {
            string colorName = "";
            string assignedColor0 = _assignmentInfoList[0].colorName;
            string assignedColor1 = _assignmentInfoList[1].colorName;

            // _colorNameList���疢�g�p�̐F������
            foreach (string name in _colorNameList)
            {
                if (name != assignedColor1 && name != assignedColor0)
                {
                    colorName = name; // ���g�p�̐F��ݒ�
                    break; // �ŏ��Ɍ��������g�p�̐F�Ń��[�v���I��
                }
            }
            mic3ColorField.text = colorName; // Color-Mic3 �Ƀp�[�g�F�\��
            mic3ColorField.color = NameToColor(colorName);
        }
        else
        {
            Debug.LogError("Textbox Color-Mic1 is not assigned.");
        }
        if (mic3NameField != null)
        {
            mic3NameField.text = "Robot Part"; // Name-Mic3 �͋@�B���S��, �����Ŗ��_���Z
        }
        else
        {
            Debug.LogError("Textbox Name-Mic1 is not assigned.");
        }

    }

    private void AssignColorsToMicrophones()
    {
        // �}�C�N�f�o�C�X���擾
        foreach (string deviceName in Microphone.devices)
        {
            // �p�\�R���{�̂̃}�C�N�͊܂߂Ȃ�
            if (deviceName == "�}�C�N�z�� (Realtek(R) Audio)")
            {
                continue;
            }

            // USB �Őڑ����ꂽ�}�C�N�̂݃��X�g�ɒǉ�
            _microphoneNameList.Add(deviceName);
        }

        if (_microphoneNameList.Count == 0)
        {
            Debug.LogError("No microphones detected.");
            return;
        }

        Debug.Log($"Detected {_microphoneNameList.Count} microphones.");

        // �F�������_���Ɋ��蓖��

        // ���ƂȂ鐔�� index of color
        List<int> numberList = new List<int> { 0, 1, 2 };

        // ���ʂ��i�[���郊�X�g
        List<int> indexList = SelectTwoRandomIndexes(numberList);

        // ���ʂ����O�o��
        Debug.Log($"Selected indexes: {indexList[0]}, {indexList[1]}");

        // �F�����蓖��
        for (int i = 0; i < _microphoneNameList.Count; i++)
        {
            string assignedColor = _colorNameList[indexList[i]]; // �F�����ԂɊ��蓖��

            // �\���̂ɒǉ�
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

        // �V���b�t�����ď��2���擾
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
                // RED, �}�C�N (Logi C615 HD WebCam)
                // �݂����Ȍ`���ŕۑ������
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
