public class LevelParameters {
    public int entityCount;
    public float entitySpeed;
    public float cooldownDuration;
    public int advanceThreshold;

    public LevelParameters(int levelIndex,
                           int baseCount, float baseSpeed, float baseCooldown, int baseThreshold,
                           int countStep, float speedStep, float cooldownStep) {
        entityCount = baseCount + levelIndex * countStep;
        entitySpeed = baseSpeed + levelIndex * speedStep;
        cooldownDuration = baseCooldown + levelIndex * cooldownStep;
        advanceThreshold = baseThreshold;
    }
}
