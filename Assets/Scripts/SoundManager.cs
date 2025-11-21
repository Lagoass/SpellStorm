using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
    Fireball,
    IceBlast,
    MagicBolt,
    ShockArea,
    EnemyHit,
    PlayerDamage
}

[System.Serializable]
public class SoundData
{
    public string name; // Apenas para organização no Inspector
    public SoundType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    
    // Cooldown para evitar spam de som (ex: 50 inimigos atingidos no mesmo frame)
    [HideInInspector] public float lastTimePlayed;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton

    [Header("Configuração")]
    [Tooltip("Arraste a Main Camera aqui (ou deixe vazio que ele acha).")]
    public AudioSource sfxSource;

    [Header("Lista de Sons")]
    public List<SoundData> soundList;

    [Header("Settings")]
    [Tooltip("Tempo mínimo entre o mesmo som tocar novamente (evita estourar áudio).")]
    public float spamThreshold = 0.05f; 

    private Dictionary<SoundType, SoundData> soundDictionary;

    void Awake()
    {
        // Configuração do Singleton (Para acessar via SoundManager.Instance)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Configura o AudioSource se não estiver setado
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Cria dicionário para acesso rápido
        soundDictionary = new Dictionary<SoundType, SoundData>();
        foreach (var sound in soundList)
        {
            if (!soundDictionary.ContainsKey(sound.type))
            {
                soundDictionary.Add(sound.type, sound);
            }
        }
    }

    public void PlaySound(SoundType type)
    {
        if (soundDictionary.TryGetValue(type, out SoundData data))
        {
            // Verifica se pode tocar (Cooldown de spam)
            if (Time.time - data.lastTimePlayed < spamThreshold) return;

            if (data.clip != null)
            {
                // PlayOneShot permite tocar vários sons simultâneos sem cortar o anterior
                sfxSource.PlayOneShot(data.clip, data.volume);
                data.lastTimePlayed = Time.time;
            }
        }
        else
        {
            Debug.LogWarning("SoundManager: Som não encontrado ou não configurado: " + type);
        }
    }
}