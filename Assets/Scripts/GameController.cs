using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Diagnostics; 

public class GameController : MonoBehaviour
{
    private Stopwatch GameStopwatch = new Stopwatch();

    [Header("Player Stats")]
    public PlayerStats playerStats;
    [Header("Enemy Stats")]
    public List<EnemyStats> enemyStatsList;
    
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
    
    // --- MUDANÇAS NO REQUISITO DE XP ---
    [Header("Leveling Curve")]
    [Tooltip("O XP necessário para o Nível 2.")]
    public float baseXpRequirement = 1000f;
    [Tooltip("O primeiro aumento de XP (ex: 200).")]
    public float initialXpIncrease = 200f;
    [Tooltip("Quanto o aumento cresce a cada nível (ex: 50).")]
    public float xpIncreaseGrowth = 50f;

    [HideInInspector]
    public float xpPerLevel; // Controlado pela nova lógica
    
    [HideInInspector]
    private float currentXpIncrease; // Variável de runtime para a "soma da soma"
    // --- FIM DAS MUDANÇAS ---
    
    [HideInInspector]
    private bool isGamePaused = false;
    [HideInInspector] 
    public bool isLevelingUp = false; 
    [HideInInspector] 
    public bool canSpawn = true; 

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
        
        EndGameDefeat();
        EndGameVictory();
        LevelUpCheck();
        PickUpRangeSync();
        
        UpdateBars();
        UpdateUIText();
        TimeManagement();
    }

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

        // --- MUDANÇA: LÓGICA "SOMA DA SOMA" ---
        // Define o requisito para o PRÓXIMO nível
        xpPerLevel += currentXpIncrease; 
        // Aumenta o valor do PRÓXIMO aumento
        currentXpIncrease += xpIncreaseGrowth; 
        // --- FIM DA MUDANÇA ---

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
        foreach (var enemyStats in enemyStatsList)
        {
            enemyStats.ResetStats();
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

        // --- MUDANÇA: INICIALIZA A CURVA DE XP ---
        xpPerLevel = baseXpRequirement;
        currentXpIncrease = initialXpIncrease;
        // --- FIM DA MUDANÇA ---

        isLevelingUp = false;
        isGamePaused = false;
        canSpawn = true; 

        gameOverScreen.SetActive(false);
        pauseMenu.SetActive(false);
        levelUpScreen.SetActive(false);
        GameStopwatch.Restart();
    }
}