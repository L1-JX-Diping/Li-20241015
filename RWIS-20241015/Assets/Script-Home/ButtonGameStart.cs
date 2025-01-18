using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI �����̂�
using UnityEngine.SceneManagement; // Scene �̐؂�ւ��������ꍇ�ɕK�v�Ȑ錾
using System.IO;

public class ButtonGameStart : MonoBehaviour
{
    // �{�^���ɒ��ڃA�^�b�`����o�[�W�����̃X�N���v�g
    // �ł���肭�����Ȃ�����
    // HomeSceneManager �œ����悤�Ȋ֐��g�����ꍇ���܂������� (Birthday Song �̂�)
    private string _gameInfoFileName = "GameInfo.txt"; // �t�@�C����
    private string _songTitle; // 1�s�ڂɊi�[�����Ȗ�
    
    // Start is called before the first frame update
    void Start()
    {
        // _songTitle ���擾
        ReadGameInfo();
        Debug.Log($"Song Title: {_songTitle}");

        // �{�^���������ꂽ�炱������s
        this.GetComponent<Button>().onClick.AddListener(SwitchScene);
    }
    void ReadGameInfo()
    {
        // �t�@�C���p�X�̐���
        string filePath = Path.Combine(Application.dataPath, _gameInfoFileName);

        // �t�@�C�������݂��邩�m�F
        if (!File.Exists(filePath))
        {
            Debug.LogError($"GameInfo file not found: {filePath}");
            return;
        }

        // �t�@�C�����s�P�ʂœǂݍ���
        string[] lines = File.ReadAllLines(filePath);

        // 1�s�ڂ���_songTitle���擾
        if (lines.Length > 0)
        {
            _songTitle = lines[0].Trim(); // 1�s�ڂ̋Ȗ����擾
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
            // "Birthday Song" ���ł����
            // Mic-Color �Ή������[�U�Ɍ�����V�[�� "AssignColor" ���J��
            SceneManager.LoadScene("AssignColor");
        }
        else
        {
            // ���̉̂͂܂�����������ƒm�点��V�[�� "ComingSoon" ���J��
            SceneManager.LoadScene("ComingSoon");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
