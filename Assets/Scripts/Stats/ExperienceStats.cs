using UnityEngine;

[CreateAssetMenu(menuName = "ExperienceStats")]
public class ExperienceStats : ScriptableObject
{
    public enum ExperienceType { Small, Medium, Large }
    [Header("Experience Stats")]
    public ExperienceType experienceType;
    
    [Tooltip("Defina os valores base (ex: 1, 5, 10)")]
    public float experienceValue; 
    
    public float pickupRadius = 3f;
}