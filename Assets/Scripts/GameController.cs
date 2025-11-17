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
    [Tooltip("Lista de TODOS os assets de EnemyStats (Tiny, Red, etc.) para aplicar o escalonamento.")]
    public List<EnemyStats> allEnemyStats; // (Substitui a antiga enemyStatsList)
    
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
    
    // --- NOVO SISTEMA DE ONDAS ---
    [Header("Wave Configuration")]
    [Tooltip("A lista de todas as ondas (WaveConfig assets) em ordem.")]
    public List<WaveConfig> waveConfigs;
    [Tooltip("Referência ao 'Executor' de spawn.")]
    public EnemySpawner enemySpawner; 
    
    private int currentWaveIndex = -1;
    private float waveEndTime;
    // --- FIM DO NOVO SISTEMA ---

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
        
        // --- LÓGICA DE CONTROLE DE ONDA ---
        // Se o jogo não está pausado e o tempo da onda acabou...
        if (!isGamePaused && !isLevelingUp && Time.time > waveEndTime)
        {
            StartNextWave();
        }
        // --- FIM DA LÓGICA ---
        
        EndGameDefeat();
        EndGameVictory();
        LevelUpCheck();
        PickUpRangeSync();
        
        UpdateBars();
        UpdateUIText();
        TimeManagement();
    }
    
    // --- NOVAS FUNÇÕES DE ONDA ---
    void StartNextWave()
    {
        currentWaveIndex++; 
        
        if (currentWaveIndex >= waveConfigs.Count)
        {
            // Fim do jogo (vitória) ou modo infinito
            currentWaveIndex = waveConfigs.Count - 1; 
            Debug.LogWarning("Todas as ondas completas. Repetindo a última onda.");
            // Você pode chamar EndGameVictory() aqui se 10 ondas = vitória
        }

        WaveConfig currentWave = waveConfigs[currentWaveIndex];

        // 1. Define o tempo de término
        waveEndTime = Time.time + currentWave.waveDuration;

        // 2. Escalone TODOS os inimigos para o nível desta onda
        foreach (var enemyStat in allEnemyStats)
        {
            if (enemyStat != null)
            {
                enemyStat.InitializeForWave(currentWave.enemyStatLevel);
            }
        }

        // 3. Comanda o Spawner para executar esta onda
        if (enemySpawner != null)
        {
            enemySpawner.StartNewWave(currentWave);
        }
    }
    
    // --- FIM DAS NOVAS FUNÇÕES ---

    public void TimeManagement()
    {
        int minutes = Mathf.FloorToInt((float)GameStopwatch.ElapsedTimeSec() / 60f);
        int seconds = Mathf.FloorToInt((float)GameStopwatch.ElapsedTimeSec() % 60f);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateBars()
    {
        if (playerStats.health >= playerStats.startingHealth)
        {
            playerHealthBar.SetActive(false);
        }
        else
        {
            playerHealthBar.SetActive(true);
        }
        foreach (var xpBar in lstxpBar)
        {
            xpBar.fillAmount = playerStats.experience / xpPerLevel;
        }
        foreach (var healthBar in lsthealthBar)
        {
            healthBar.fillAmount = playerStats.health / playerStats.startingHealth;
        }
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
        if (GameStopwatch.ElapsedTimeSec() < 600f) return; 
        if (gameOverScreen.activeSelf) return; 
        
        gameOverScreen.SetActive(true);
        gameOverScreenVictory.SetActive(true);
        Time.timeScale = 0f; 
        canSpawn = false; 
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
        
        // Reseta todos os inimigos para o Nível 1 no início
        foreach (var enemyStats in allEnemyStats) 
        {
            if (enemyStats != null)
                enemyStats.InitializeForWave(1); 
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
        currentWaveIndex = -1; // Reseta o índice da onda
        waveEndTime = Time.time - 1f; // Força o StartNextWave no primeiro frame

        isLevelingUp = false;
        isGamePaused = false;
        canSpawn = true; 

        gameOverScreen.SetActive(false);
        pauseMenu.SetActive(false);
        levelUpScreen.SetActive(false);
        GameStopwatch.Restart();
    }
}