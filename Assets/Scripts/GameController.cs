using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private Stopwatch GameStopwatch = new Stopwatch();

    [Header("Player Stats")]
    public PlayerStats playerStats;
    
    [Header("Enemy Stats")]
    [Tooltip("Arraste TODOS os assets de EnemyStats aqui para que eles sejam escalonados.")]
    public List<EnemyStats> allEnemyStats; 
    
    [Header("Magic Stats")]
    public MagicStats FireBallStats;
    public bool FireballIsActive = false;
    public MagicStats IceBlastStats;
    public bool IceBlastIsActive = false;
    public MagicStats ShockAreaStats;
    public bool ShockAreaIsActive = false;
    public MagicStats MagicBoltStats; 
    public bool MagicBoltIsActive = true; 
    
    [Header("Experience Stats")]
    public List<ExperienceStats> XPStats;
    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI waveText;
    public List<Image> lstxpBar;
    public List<Image> lsthealthBar;
    public GameObject playerHealthBar;
    
    [Header("Screen Controllers")]
    public GameObject pauseMenu;
    public GameObject gameOverScreen;
    public GameObject gameOverScreenVictory;
    public GameObject gameOverScreenDefeat;
    public GameObject levelUpScreen;
    public LevelUp levelUpController;
    
    // --- CONFIGURAÇÃO DE ONDAS ---
    [Header("Wave Configuration")]
    public List<WaveConfig> waveConfigs;
    public EnemySpawner enemySpawner; 
    
    private int currentWaveIndex = -1;
    private float waveEndTime;

    // --- CONFIGURAÇÃO DO BOSS ---
    [Header("Boss Battle")]
    [Tooltip("O Prefab do Boss a ser spawnado.")]
    public GameObject bossPrefab;
    [Tooltip("Distância do player onde o Boss vai nascer.")]
    public float bossSpawnDistance = 15f;
    
    // Este índice agora é calculado automaticamente no Reset()
    private int bossWaveIndex; 
    // -----------------------------------

    [Header("Leveling Curve")]
    public float baseXpRequirement = 1000f;
    public float initialXpIncrease = 200f;
    public float xpIncreaseGrowth = 50f;

    [HideInInspector] public float xpPerLevel; 
    [HideInInspector] private float currentXpIncrease; 
    [HideInInspector] private bool isGamePaused = false;
    [HideInInspector] public bool isLevelingUp = false; 
    [HideInInspector] public bool canSpawn = true; 

    void Start()
    {
        Reset();
        if (levelUpController != null)
        {
            levelUpController.gameController = this;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isLevelingUp)
        {
            isGamePaused = TogglePause();
        }
        
        // Checa se a onda acabou
        if (!isGamePaused && !isLevelingUp && Time.time > waveEndTime)
        {
            StartNextWave();
        }
        
        EndGameDefeat();
        // Vitória é controlada pelo Boss, não por tempo
        LevelUpCheck();
        PickUpRangeSync();
        
        UpdateBars();
        UpdateUIText();
        TimeManagement();
    }
    
    void StartNextWave()
    {
        currentWaveIndex++; 
        
        if (currentWaveIndex >= waveConfigs.Count)
        {
            currentWaveIndex = waveConfigs.Count - 1; // Repete a última se acabar (segurança)
            Debug.LogWarning("Todas as ondas completas. Repetindo última onda.");
        }

        WaveConfig currentWave = waveConfigs[currentWaveIndex];

        waveEndTime = Time.time + currentWave.waveDuration;

        // 1. Atualiza o nível de todos os inimigos
        foreach (var enemyStat in allEnemyStats)
        {
            if (enemyStat != null)
                enemyStat.InitializeForWave(currentWave.enemyStatLevel);
        }

        // 2. Configura o Spawner (para os minions)
        if (enemySpawner != null)
        {
            enemySpawner.StartNewWave(currentWave);
        }

        // 3. --- SPAWN DO BOSS ---
        // Verifica se chegamos na ÚLTIMA onda da lista
        if (currentWaveIndex == bossWaveIndex)
        {
            SpawnBoss();
        }
        // ------------------------
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab não atribuído no GameController!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = Vector3.zero;
        
        if (player != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            spawnPos = player.transform.position + (Vector3)(randomDir * bossSpawnDistance);
        }

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        
        Enemy bossScript = boss.GetComponent<Enemy>();
        if (bossScript != null)
        {
            bossScript.isBoss = true; // Marca como Boss
        }
        
        Debug.Log("BOSS SPAWNADO!");
    }

    public void WinGame()
    {
        if (gameOverScreen.activeSelf) return; 
        
        gameOverScreen.SetActive(true);
        gameOverScreenVictory.SetActive(true);
        Time.timeScale = 0f; 
        canSpawn = false;
        Debug.Log("VITÓRIA! O Boss foi derrotado.");
    }

    public void TimeManagement()
    {
        int minutes = Mathf.FloorToInt(GameStopwatch.ElapsedTimeSec() / 60f);
        int seconds = Mathf.FloorToInt(GameStopwatch.ElapsedTimeSec() % 60f);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateBars()
    {
        if (playerStats.health >= playerStats.startingHealth)
            playerHealthBar.SetActive(false);
        else
            playerHealthBar.SetActive(true);

        foreach (var xpBar in lstxpBar)
            xpBar.fillAmount = playerStats.experience / xpPerLevel;
        
        foreach (var healthBar in lsthealthBar)
            healthBar.fillAmount = playerStats.health / playerStats.startingHealth;
    }

    public void LevelUpCheck()
    {
        if (playerStats.experience < xpPerLevel) return;
        if (isLevelingUp) return;

        playerStats.level += 1;
        playerStats.experience -= xpPerLevel; 
        xpPerLevel += currentXpIncrease; 
        currentXpIncrease += xpIncreaseGrowth; 

        if (levelUpController != null)
        {
            isLevelingUp = true;
            canSpawn = false; 
            levelUpController.LevelUpScreen(true);
        }
    }

    public void UpdateUIText()
    {
        healthText.text = playerStats.health.ToString("F0");
        experienceText.text = playerStats.experience.ToString("F0") + " | " + xpPerLevel.ToString("F0");
        levelText.text = "Lvl: " + playerStats.level.ToString("F0");
        waveText.text = "Wave: " + (currentWaveIndex + 1).ToString();
    }
    
    public bool TogglePause()
    {
        bool shouldPause = !isGamePaused; 
        if (shouldPause) 
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            canSpawn = false; 
            return true;
        } 
        else 
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            canSpawn = true; 
            return false;
        }
    }

    public void NotifyLevelUpClosed()
    {
        isLevelingUp = false;
        canSpawn = true; 
        LevelUpCheck(); 
    }
    
    public void EndGameDefeat()
    {
        if (playerStats.health > 0) return;
        gameOverScreen.SetActive(true);
        gameOverScreenDefeat.SetActive(true);
        Time.timeScale = 0f; 
        canSpawn = false; 
    }

    public void EndGameVictory()
    {
        // Vitória agora é via WinGame() chamado pelo Boss
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        Reset(); 
        SceneManager.LoadScene(1);
    }

    public void PickUpRangeSync()
    {
        foreach (var xpStat in XPStats)
        {
            xpStat.pickupRadius = playerStats.pickupRadius;
        }
    }

    public void ResumeGame()
    {
        isGamePaused = TogglePause();
    }

    public void Reset()
    {
        playerStats.ResetStats();
        
        foreach (var enemyStats in allEnemyStats) 
        {
            if (enemyStats != null) enemyStats.InitializeForWave(1);
        }

        if (MagicBoltStats != null) MagicBoltStats.ResetStats(true); 
        if (FireBallStats != null) FireBallStats.ResetStats(false); 
        if (IceBlastStats != null) IceBlastStats.ResetStats(false); 
        if (ShockAreaStats != null) ShockAreaStats.ResetStats(false);
        
        if (MagicBoltStats != null) MagicBoltIsActive = MagicBoltStats.isActive;
        if (FireBallStats != null) FireballIsActive = FireBallStats.isActive;
        if (IceBlastStats != null) IceBlastIsActive = IceBlastStats.isActive;
        if (ShockAreaStats != null) ShockAreaIsActive = ShockAreaStats.isActive;
        
        Time.timeScale = 1f;

        xpPerLevel = baseXpRequirement;
        currentXpIncrease = initialXpIncrease;
        
        // --- MUDANÇA: DEFINIÇÃO AUTOMÁTICA DA ONDA DO BOSS ---
        if (waveConfigs != null && waveConfigs.Count > 0)
        {
            bossWaveIndex = waveConfigs.Count - 1; // A última onda é sempre a do Boss
        }
        else
        {
            Debug.LogError("GameController: A lista 'Wave Configs' está vazia!");
            bossWaveIndex = -1;
        }
        // -----------------------------------------------------

        currentWaveIndex = -1; 
        
        waveEndTime = Time.time - 1f; 

        isLevelingUp = false;
        isGamePaused = false;
        canSpawn = true; 

        gameOverScreen.SetActive(false);
        pauseMenu.SetActive(false);
        levelUpScreen.SetActive(false);
        GameStopwatch.Restart();
        
        StartNextWave(); 
    }
}