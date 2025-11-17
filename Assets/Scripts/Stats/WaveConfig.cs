using UnityEngine;
using System.Collections.Generic;

// --- DEFINIÇÕES DE EVENTOS (Classes auxiliares) ---
// (Estas classes não são ScriptableObjects, elas apenas guardam os dados)

[System.Serializable]
public class BatchSettings
{
    [Header("Batch Event Config")]
    [Tooltip("O número MÍNIMO de inimigos neste evento.")]
    public int minSpawnCount = 15;
    
    [Tooltip("O número MÁXIMO de inimigos neste evento.")]
    public int maxSpawnCount = 20;
    
    [Tooltip("A velocidade forçada para os inimigos do Batch.")]
    public float speedOverride = 4f;
    
    // O 'batchSpread' (raio do cluster) ficará fixo no EnemySpawner (ex: 1.5f)
}

[System.Serializable]
public class CircleSettings
{
    [Header("Circle Event Config")]
    [Tooltip("O número MÍNIMO de inimigos neste evento.")]
    public int minSpawnCount = 10;
    
    [Tooltip("O número MÁXIMO de inimigos neste evento.")]
    public int maxSpawnCount = 15;
    
    // Os raios (8-10) e a velocidade (0.5) serão valores padrão no EnemySpawner, como você pediu.
}


// --- O SCRIPTABLE OBJECT PRINCIPAL (A ONDA) ---

[CreateAssetMenu(fileName = "Wave_01", menuName = "Stats/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Configuração Geral")]
    
    [Tooltip("Duração total desta onda em segundos.")]
    public float waveDuration = 60f; // <-- CAMPO ADICIONADO (Duração da wave)
    
    [Tooltip("O orçamento (saldo) total para esta onda.")]
    public int totalBudget = 1000; // <-- CAMPO ADICIONADO (Saldo para wave)

    [Tooltip("Inimigos que podem ser 'comprados' (spawnados) nesta onda.")]
    public List<EnemyStats> availableEnemies; // <-- CAMPO ADICIONADO (Inimigos que podem spawnar)

    [Header("Escalonamento de Inimigos")]
    [Tooltip("Define o nível dos inimigos para esta onda (ex: 1, 2, 3...).")]
    public int enemyStatLevel = 1; // <-- CAMPO ADICIONADO (Nivel dos inimigos)

    [Header("Gotejamento (Spawn Padrão)")]
    [Tooltip("Intervalo (em segundos) entre os spawns normais.")]
    public float trickleSpawnInterval = 1.0f;
    
    [Tooltip("O limite máximo de inimigos VIVOS vindos do gotejamento.")]
    public int maxTrickleEnemies = 20;

    [Header("Configuração de Eventos")]
    [Tooltip("O tempo (em segundos) entre as tentativas de disparar um evento.")]
    public float eventInterval = 20f;
    
    [Header("Evento: Batch (Emboscada)")]
    public bool enableBatchEvents;
    public BatchSettings batchSettings; // Contém Min/Max e Speed

    [Header("Evento: Circle (Cerco)")]
    public bool enableAroundEvents;
    public CircleSettings circleSettings; // Contém Min/Max
}