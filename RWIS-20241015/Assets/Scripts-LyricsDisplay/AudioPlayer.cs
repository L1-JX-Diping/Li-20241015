using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource; // 再生するAudioSource
    public AudioClip audioClip;     // 再生する音声ファイル
    public float delayInSeconds = 0f; // 再生を開始する遅延時間（秒）

    private bool hasPlayed = false; // 音声が再生済みかどうか

    void Start()
    {
        // AudioSourceにAudioClipを設定
        if (audioSource != null && audioClip != null)
        {
            audioSource.clip = audioClip;
        }
        else
        {
            Debug.LogError("AudioSource or AudioClip is not assigned.");
        }
    }

    void Update()
    {
        // シーンが開始してX秒を過ぎたら音声を再生
        if (!hasPlayed && Time.timeSinceLevelLoad >= delayInSeconds)
        {
            audioSource.Play();
            hasPlayed = true; // 再生済みフラグを設定
            Debug.Log("Audio started playing.");
        }
    }
}
