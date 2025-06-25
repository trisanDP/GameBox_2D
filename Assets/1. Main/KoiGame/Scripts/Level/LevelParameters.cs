
// ===== LevelParameters.cs =====
public class LevelParameters {
    /// <summary>The zero-based index of this level.</summary>
    public int levelIndex { get; private set; }

    /// <summary>How many entities to spawn this round.</summary>
    public int entityCount { get; private set; }
    /// <summary>Speed (units/sec) for each entity.</summary>
    public float entitySpeed { get; private set; }
    /// <summary>Cooldown (sec) between spawns/selections.</summary>
    public float cooldownDuration { get; private set; }
    /// <summary>How many wrong feeds allowed before failure.</summary>
    public int advanceThreshold { get; private set; }

    public LevelParameters(
        int levelIndex,
        int baseCount, float baseSpeed, float baseCooldown, int baseThreshold,
        int countStep, float speedStep, float cooldownStep
    ) {
        this.levelIndex = levelIndex;

        entityCount = baseCount + levelIndex * countStep;
        entitySpeed = baseSpeed + levelIndex * speedStep;
        cooldownDuration = baseCooldown + levelIndex * cooldownStep;
        advanceThreshold = baseThreshold;
    }
}
