using System.Collections.Generic;
using System.Linq; 
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    [Header("Screen Control")]
    public GameObject levelUpPanel;
    
    [HideInInspector]
    public GameController gameController; 

    [Header("Cards")]
    public List<GameObject> commonCards; 
    public List<GameObject> rareCards;   
    public List<GameObject> epicCards;   
    [Header("Title")]
    public List<TextMeshProUGUI> cardTitles; 
    [Header("Images")]
    public List<Image> cardImages; 
    [Header("Stats")]
    public List<TextMeshProUGUI> cardStats_Left; 
    public List<TextMeshProUGUI> cardStats_Middle; 
    public List<TextMeshProUGUI> cardStats_Right; 
    
    [Header("Projectile Magic Stats")]
    public MagicStats FireballStats;
    public MagicStats IceBlastStats;
    public MagicStats MagicBoltStats; 

    [Header("Around Player Magic Stats")]
    public MagicStats ShockAreaStats;
    
    [Header("Player Stats")]
    public PlayerStats playerStats;

    [HideInInspector]
    public List<string> CardsOptions = new List<string>();
    public List<KeyValuePair<string, int>> SelectedCards = new List<KeyValuePair<string, int>>();
    public Dictionary<string, (Sprite, List<string>)> CardTemplateLeft = new Dictionary<string, (Sprite, List<string>)>();
    public Dictionary<string, (Sprite, List<string>)> CardTemplateMiddle = new Dictionary<string, (Sprite, List<string>)>();
    public Dictionary<string, (Sprite, List<string>)> CardTemplateRight = new Dictionary<string, (Sprite, List<string>)>();
    public Dictionary<string, List<float>> levelUpStatIncreases = new Dictionary<string, List<float>>();
    public Dictionary<string, List<float>> levelUpSpellsTraitsIncreases = new Dictionary<string, List<float>>();
    public Dictionary<string, List<float>> levelUpCards = new Dictionary<string, List<float>>();

    void Start()
    {
        levelUpCardsSetup(); 
    }
    
    public void LevelUpScreen(bool isActive)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(isActive);
        }
        else
        {
            Debug.LogError("LEVELUP: Referência 'levelUpPanel' não atribuída!");
            return; 
        }

        if (isActive)
        {
            LevelUpScreenSetup(); 
            Time.timeScale = 0f; 
        }
        else
        {
            Time.timeScale = 1f; 
        }
    }

    public void SelectCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= SelectedCards.Count)
        {
            Debug.LogError("Índice de carta inválido: " + cardIndex + ". SelectedCards Count: " + SelectedCards.Count);
            return;
        }

        string cardName = SelectedCards[cardIndex].Key;
        int rarityIndex = SelectedCards[cardIndex].Value;

        var selectedCardTemplate = cardIndex == 0 ? CardTemplateLeft : 
                                   cardIndex == 1 ? CardTemplateMiddle : 
                                   CardTemplateRight;
        
        KeyValuePair<string, (Sprite Sprite, List<string> Descriptions)> cardData = selectedCardTemplate.First();
        List<string> generatedDescriptions = cardData.Value.Descriptions;
        
        // Aplica melhorias (Player Stats)
        if (levelUpStatIncreases.ContainsKey(cardName))
        {
            float increaseValue = levelUpStatIncreases[cardName][rarityIndex];
            
            switch (cardName)
            {
                case "MaxHealth": playerStats.startingHealth += increaseValue; break;
                case "HealthRegen": playerStats.healthRegen += increaseValue; break;
                case "MoveSpeed": playerStats.moveSpeed += increaseValue; break;
                case "PickupRadius": playerStats.pickupRadius += increaseValue; break;
                
                // --- MUDANÇA AQUI ---
                case "XpGains": 
                    // Adiciona o bônus (0.05) ao multiplicador (1.0)
                    playerStats.XpGains += increaseValue; 
                    break;
                // --- FIM DA MUDANÇA ---
                    
                case "Luck": playerStats.luck += increaseValue; break;
            }
        }
        // Aplica melhorias (Spells)
        else if (levelUpCards.ContainsKey(cardName))
        {
            MagicStats magicStats = GetMagicStats(cardName);

            if (magicStats != null && magicStats.isActive == false)
            {
                magicStats.isActive = true; 
            }
            else if (magicStats != null && magicStats.isActive == true)
            {
                magicStats.level += 1; 
                ApplySpellUpgrades(magicStats, generatedDescriptions, rarityIndex);
            }
        }

        LevelUpScreen(false); 

        if (gameController != null)
        {
            gameController.NotifyLevelUpClosed(); 
        }
    }
    
    private void ApplySpellUpgrades(MagicStats magicStats, List<string> descriptions, int rarityIndex)
    {
        foreach (string description in descriptions)
        {
            string traitName = description.Split(' ')[0].Trim(); 
            
            if (levelUpSpellsTraitsIncreases.TryGetValue(traitName, out List<float> increases))
            {
                float increaseValue = increases[rarityIndex];

                if (magicStats.projectileMagicStats != null)
                {
                    ApplyProjectileStat(magicStats.projectileMagicStats, traitName, increaseValue);
                }
                else if (magicStats.aroundPlayerMagicStats != null)
                {
                    ApplyAroundPlayerStat(magicStats.aroundPlayerMagicStats, traitName, increaseValue);
                }
            }
        }
    }

    private void ApplyProjectileStat(ProjectileMagicStats stats, string traitName, float value)
    {
        switch (traitName)
        {
            case "Damage": stats.damage += value; break;
            case "AttackRate": stats.attackRate -= value; stats.attackRate = Mathf.Max(0.05f, stats.attackRate); break;
            case "Speed": stats.speed += value; break;
            case "EffectDamage": stats.EffectDamage += value; break;
            case "EffectDamageRadius": stats.EffectDamageRadius += value; break;
            case "EffectDuration": stats.EffectDuration += value; break;
            case "ProjectileAmount": stats.projectileAmount += (int)value; break;
        }
    }

    private void ApplyAroundPlayerStat(AroundPlayerMagicStats stats, string traitName, float value)
    {
        switch (traitName)
        {
            case "AreaDamage": stats.damage += value; break;
            case "AreaAttackRate": stats.attackRate -= value; stats.attackRate = Mathf.Max(0.05f, stats.attackRate); break;
            case "AreaRadius": stats.radius += value; break;
        }
    }
    
    public void LevelUpScreenSetup()
    {
        CardTemplateLeft.Clear(); CardTemplateMiddle.Clear(); CardTemplateRight.Clear();
        SelectedCards.Clear();
        
        SelectingCards();
        CreatingCardTemplate();
        
        var cardStatsLists = new List<List<TextMeshProUGUI>>
        {
            cardStats_Left, cardStats_Middle, cardStats_Right
        };

        var cardTemplates = new List<Dictionary<string, (Sprite, List<string>)>>
        {
            CardTemplateLeft, CardTemplateMiddle, CardTemplateRight
        };

        for (int i = 0; i < SelectedCards.Count; i++)
        {
            int rarityIndex = SelectedCards[i].Value; 
            
            if (cardTemplates[i].Count == 0) continue; 
            
            KeyValuePair<string, (Sprite Sprite, List<string> Descriptions)> cardData = cardTemplates[i].First();
            string cardName = cardData.Key;
            List<string> statDescriptions = cardData.Value.Descriptions;

            // Configurar Raridade Visual
            commonCards[i].SetActive(false); rareCards[i].SetActive(false); epicCards[i].SetActive(false);
            switch (rarityIndex)
            {
                case 0: commonCards[i].SetActive(true); break;
                case 1: rareCards[i].SetActive(true); break;
                case 2: epicCards[i].SetActive(true); break;
            }

            // Configurar Título e Imagem
            cardTitles[i].text = cardName; 
            if (cardData.Value.Sprite != null)
            {
                cardImages[i].sprite = cardData.Value.Sprite;
                cardImages[i].enabled = true;
            }
            else
            {
                cardImages[i].enabled = false; 
            }

            // Configurar Stats de Texto
            List<TextMeshProUGUI> currentStatsUI = cardStatsLists[i];
            for (int j = 0; j < currentStatsUI.Count; j++)
            {
                if (j < statDescriptions.Count)
                {
                    currentStatsUI[j].gameObject.SetActive(true); 
                    currentStatsUI[j].text = statDescriptions[j]; 
                }
                else
                {
                    currentStatsUI[j].gameObject.SetActive(false);
                }
            }
        }
    }

    public void CreatingCardTemplate()
    {
        CardTemplateLeft.Clear(); CardTemplateMiddle.Clear(); CardTemplateRight.Clear();

        var cardTemplates = new List<Dictionary<string, (Sprite, List<string>)>>
        {
            CardTemplateLeft, CardTemplateMiddle, CardTemplateRight
        };

        for (int i = 0; i < SelectedCards.Count; i++)
        {
            string cardName = SelectedCards[i].Key;
            int rarityIndex = SelectedCards[i].Value; 
            Sprite cardSprite = null; 
            List<string> statDescriptions = new List<string>();

            if (levelUpStatIncreases.ContainsKey(cardName))
            {
                statDescriptions = HandlePlayerStatCard(cardName, rarityIndex);
            }
            else if (levelUpCards.ContainsKey(cardName))
            {
                MagicStats magicStats = GetMagicStats(cardName);

                if (magicStats != null && magicStats.isActive == false)
                {
                    statDescriptions = HandleNewSpellCard(cardName);
                }
                else
                {
                    statDescriptions = HandleLevelUpSpellCard(cardName, rarityIndex); 
                }
            }
            cardTemplates[i].Add(cardName, (cardSprite, statDescriptions));
        }
    }
    
    private List<string> HandlePlayerStatCard(string cardName, int rarityIndex)
    {
        List<string> descriptions = new List<string>();
        float increaseValue = levelUpStatIncreases[cardName][rarityIndex];
        string formattedDescription = "";

        switch (cardName)
        {
            case "MaxHealth": formattedDescription = $"Saúde Máxima: +{increaseValue:F0}"; break;
            case "HealthRegen": formattedDescription = $"Regen. Saúde: +{increaseValue:F1}"; break;
            case "MoveSpeed": formattedDescription = $"Velocidade: +{increaseValue:F2}"; break;
            case "PickupRadius": formattedDescription = $"Raio de Coleta: +{increaseValue:F2}"; break;
            case "XpGains": formattedDescription = $"Ganhos de XP: +{increaseValue * 100:F0}%"; break;
            case "Luck": formattedDescription = $"Sorte: +{increaseValue:F2}"; break;
        }
        descriptions.Add(formattedDescription);
        return descriptions;
    }

    private List<string> HandleNewSpellCard(string cardName)
    {
        List<string> descriptions = new List<string>();
        descriptions.Add($"Desbloquear a Magia {cardName}");
        descriptions.Add("Nível 1"); 
        return descriptions;
    }

    private List<string> HandleLevelUpSpellCard(string cardName, int rarityIndex)
    {
        List<string> descriptions = new List<string>();
        int statsToSelect = rarityIndex + 1; 
        
        List<string> availableTraits = GetSpellTraits(cardName);
        
        if (rarityIndex == 0) 
        {
            availableTraits.Remove("ProjectileAmount");
        }
        
        List<string> selectedTraits = availableTraits.OrderBy(x => Random.value).Take(statsToSelect).ToList();

        foreach (string traitName in selectedTraits)
        {
            if (levelUpSpellsTraitsIncreases.TryGetValue(traitName, out List<float> increases))
            {
                float increaseValue = increases[rarityIndex];
                
                string statDesc = "";
                if (traitName.Contains("AttackRate"))
                {
                    statDesc = $"{traitName}: -{increaseValue:F2}s"; 
                }
                else if (traitName.Contains("EffectDuration"))
                {
                    statDesc = $"{traitName}: +{increaseValue:F2}s";
                }
                else if (traitName.Contains("ProjectileAmount"))
                {
                    statDesc = $"{traitName}: +{increaseValue:F0}"; 
                }
                else
                {
                    statDesc = $"{traitName}: +{increaseValue:F2}";
                }
                descriptions.Add(statDesc);
            }
        }
        return descriptions;
    }

    private MagicStats GetMagicStats(string spellName)
    {
        switch (spellName)
        {
            case "FireBall": return FireballStats;
            case "IceBlast": return IceBlastStats;
            case "ShockArea": return ShockAreaStats;
            case "MagicBolt": return MagicBoltStats; 
            default: return null;
        }
    }
    
    private List<string> GetSpellTraits(string spellName)
    {
        switch (spellName)
        {
            case "MagicBolt":
                return new List<string> { "Damage", "AttackRate", "Speed", "ProjectileAmount" };

            case "FireBall":
                return new List<string> { "Damage", "AttackRate", "Speed", 
                                          "EffectDamage", "EffectDamageRadius", "ProjectileAmount" };

            case "IceBlast":
                return new List<string> { "Damage", "AttackRate", "Speed", 
                                          "EffectDamage", "EffectDamageRadius", "EffectDuration", "ProjectileAmount" };

            case "ShockArea":
                return new List<string> { "AreaDamage", "AreaAttackRate", "AreaRadius" };
            
            default:
                return new List<string>();
        }
    }

    public void SelectingCards()
    {
        SelectedCards.Clear();
        
        if (CardsOptions.Count == 0)
        {
            levelUpCardsSetup();
        }

        if (CardsOptions.Count == 0)
        {
            Debug.LogError("LEVELUP: ERRO FATAL! CardsOptions continua vazia após tentativa de setup.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            float rand = Random.Range(0f, 1f);
            float luckFactor = playerStats.luck / 200f;
            int optionIndex = Random.Range(0, CardsOptions.Count);
            string cardName = CardsOptions[optionIndex];

            if (rand < 0.6f - luckFactor)
            {
                SelectedCards.Add(new KeyValuePair<string, int>(cardName, 0)); 
            }
            else if (rand < 0.9f - luckFactor / 2)
            {
                SelectedCards.Add(new KeyValuePair<string, int>(cardName, 1)); 
            }
            else
            {
                SelectedCards.Add(new KeyValuePair<string, int>(cardName, 2)); 
            }
        }
    }

    public void levelUpCardsSetup()
    {
        CardsOptions.Clear(); 
        CardsOptions.Add("MaxHealth"); CardsOptions.Add("HealthRegen");
        CardsOptions.Add("MoveSpeed"); CardsOptions.Add("PickupRadius");
        CardsOptions.Add("XpGains"); CardsOptions.Add("Luck");
        CardsOptions.Add("FireBall"); CardsOptions.Add("IceBlast");
        CardsOptions.Add("ShockArea");
        CardsOptions.Add("MagicBolt"); 
        
        levelUpCards.Clear();
        levelUpStatIncreases.Clear();
        levelUpSpellsTraitsIncreases.Clear();

        levelUpCards.Add("FireBall", new List<float> { 0 }); levelUpCards.Add("IceBlast", new List<float> { 0 }); 
        levelUpCards.Add("ShockArea", new List<float> { 0 }); 
        levelUpCards.Add("MagicBolt", new List<float> { 0 }); 
        levelUpCards.Add("MaxHealth", new List<float> { 0 }); levelUpCards.Add("HealthRegen", new List<float> { 0 });
        levelUpCards.Add("MoveSpeed", new List<float> { 0 }); levelUpCards.Add("PickupRadius", new List<float> { 0 });
        levelUpCards.Add("XpGains", new List<float> { 0 }); levelUpCards.Add("Luck", new List<float> { 0 });
        
        levelUpSpellsTraitsIncreases.Add("ProjectileAmount", new List<float> { 0, 1, 1 }); 
        
        levelUpSpellsTraitsIncreases.Add("Damage", new List<float> { 2, 5, 10 }); 
        levelUpSpellsTraitsIncreases.Add("AttackRate", new List<float> { 0.05f, 0.1f, 0.2f }); 
        levelUpSpellsTraitsIncreases.Add("Speed", new List<float> { 0.2f, 0.5f, 1f }); 
        levelUpSpellsTraitsIncreases.Add("EffectDamage", new List<float> { 1f, 3f, 5f }); 
        levelUpSpellsTraitsIncreases.Add("EffectDamageRadius", new List<float> { 0.1f, 0.25f, 0.5f }); 
        levelUpSpellsTraitsIncreases.Add("EffectDuration", new List<float> { 0.05f, 0.1f, 0.15f }); 
        levelUpSpellsTraitsIncreases.Add("AreaDamage", new List<float> { 2, 5, 10 }); 
        levelUpSpellsTraitsIncreases.Add("AreaAttackRate", new List<float> { 0.05f, 0.1f, 0.2f }); 
        levelUpSpellsTraitsIncreases.Add("AreaRadius", new List<float> { 0.1f, 0.25f, 0.5f }); 

        levelUpStatIncreases.Add("MaxHealth", new List<float> { 5f, 10f, 20f }); 
        levelUpStatIncreases.Add("HealthRegen", new List<float> { 0.5f, 1f, 1.5f }); 
        levelUpStatIncreases.Add("MoveSpeed", new List<float> { 0.25f, 0.5f, 0.75f }); 
        levelUpStatIncreases.Add("PickupRadius", new List<float> { 0.1f, 0.2f, 0.3f }); 
        
        // --- MUDANÇA AQUI ---
        // Altera os valores do card de XP para (5%, 10%, 20%)
        levelUpStatIncreases.Add("XpGains", new List<float> { 0.05f, 0.1f, 0.2f }); 
        // --- FIM DA MUDANÇA ---
        
        levelUpStatIncreases.Add("Luck", new List<float> { 0.1f, 0.25f, 0.3f }); 
    }
}