using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using Unity.VisualScripting;

public class ShowcaseStatsnSpells : MonoBehaviour
{
    // Stats = ["Health", "HealthRegen", speed, pickuprange, xpgains luck]
    [Header("Self Reference")]
    public GameObject self;
    [Header("Text References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthRegenText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI pickupRangeText;
    public TextMeshProUGUI xpGainsText;
    public TextMeshProUGUI luckText;
    [Header("Player Stats")]
    public PlayerStats playerStats;
    [Header("Spell References")]
    public List<GameObject> spellContainers;
    public List<TextMeshProUGUI> spellTexts;
    public List<Image> spellImages;
    [Header("Magic Stats List")]
    public List<MagicStats> magicStatsList;
    [HideInInspector]
    public List<MagicStats> activeSpellsList;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (self.activeInHierarchy)
        {
            UpdateStatsDisplay();
            UpdateSpellsDisplay();
        }
    }

    public void UpdateStatsDisplay()
    {
        // Update Player Stats Display only text is the value
        // xpgains and luck in percentage
        healthText.text = playerStats.startingHealth.ToString("F0");
        healthRegenText.text = playerStats.healthRegen.ToString("F1");
        speedText.text = playerStats.moveSpeed.ToString("F1");
        pickupRangeText.text = playerStats.pickupRadius.ToString("F1");
        xpGainsText.text = (playerStats.XpGains * 100f).ToString("F0") + "%";
        luckText.text = (playerStats.luck * 100f).ToString("F0") + "%";
    }

    public void UpdateSpellsDisplay()
    {
        ActiveSpells();
        for (int i = 0; i < spellContainers.Count; i++)
        {
            if (i < activeSpellsList.Count)
            {
                spellContainers[i].SetActive(true);
                spellTexts[i].text = activeSpellsList[i].magicName + " (Lvl " + activeSpellsList[i].level + ")";
                if (activeSpellsList[i].magicIcon != null)
                {
                    spellImages[i].sprite = activeSpellsList[i].magicIcon;
                }
            }
            else
            {
                spellContainers[i].SetActive(false);
            }
        }
    }

    public void ActiveSpells()
    {
        activeSpellsList.Clear();
        foreach (MagicStats magic in magicStatsList)
        {
            if (magic.isActive)
            {
                activeSpellsList.Add(magic);
            }
        }
    }
}
