using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch
{
    /// <summary>
    /// Configuration for one memory match level/mode: grid size, the pool of
    /// pairs to draw from, timings, and scoring rules.
    /// </summary>
    [CreateAssetMenu(menuName = "Memory Match/Memory Match Data", fileName = "NewMemoryMatchData")]
    public class MemoryMatchData : ScriptableObject
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int rows = 3;
        [SerializeField, Min(1)] private int columns = 4;

        [Header("Pair Pool")]
        [Tooltip("Pairs the generator can pick from. Needs at least (rows * columns / 2) entries.")]
        [SerializeField] private List<MatchPairData> pairPool = new List<MatchPairData>();

        [Header("Visuals")]
        [SerializeField] private Sprite cardBack;
        [SerializeField] private Color cardBackTint = Color.white;

        [Header("Timing (seconds)")]
        [SerializeField, Min(0f)] private float flipDuration = 0.25f;
        [SerializeField, Min(0f)] private float mismatchRevealTime = 0.8f;
        [SerializeField, Min(0f)] private float previewTime = 0f;
        [Tooltip("0 = no time limit")]
        [SerializeField, Min(0f)] private float timeLimit = 0f;

        [Header("Rules")]
        [Tooltip("0 = unlimited mistakes")]
        [SerializeField, Min(0)] private int maxMistakes = 0;

        [Header("Scoring")]
        [SerializeField] private int matchPoints = 100;
        [SerializeField] private int mismatchPenalty = 10;
        [SerializeField] private int comboBonusPerStreak = 25;
        [SerializeField] private int timeBonusPerSecondLeft = 5;

        // ---- Grid ----
        public int Rows => rows;
        public int Columns => columns;
        public int TotalCards => rows * columns;
        public int PairCount => TotalCards / 2;
        public bool HasValidGridSize => TotalCards % 2 == 0;

        // ---- Pool ----
        public IReadOnlyList<MatchPairData> PairPool => pairPool;

        // ---- Visuals ----
        public Sprite CardBack => cardBack;
        public Color CardBackTint => cardBackTint;

        // ---- Timing ----
        public float FlipDuration => flipDuration;
        public float MismatchRevealTime => mismatchRevealTime;
        public float PreviewTime => previewTime;
        public float TimeLimit => timeLimit;
        public bool HasTimeLimit => timeLimit > 0f;

        // ---- Rules ----
        public int MaxMistakes => maxMistakes;
        public bool HasMistakeLimit => maxMistakes > 0;

        // ---- Scoring ----
        public int MatchPoints => matchPoints;
        public int MismatchPenalty => mismatchPenalty;
        public int ComboBonusPerStreak => comboBonusPerStreak;
        public int TimeBonusPerSecondLeft => timeBonusPerSecondLeft;

        /// <summary>Returns true if this data can produce a playable board.</summary>
        public bool Validate(out string error)
        {
            if (!HasValidGridSize)
            {
                error = $"Grid {rows}x{columns} has an odd number of cards ({TotalCards}). Use an even total.";
                return false;
            }

            int valid = CountValidUniquePairs();
            if (valid < PairCount)
            {
                error = $"Need {PairCount} unique pairs but the pool only has {valid} valid ones.";
                return false;
            }

            error = null;
            return true;
        }

        private int CountValidUniquePairs()
        {
            var ids = new HashSet<string>();
            foreach (var pair in pairPool)
                if (pair != null) ids.Add(pair.PairId);
            return ids.Count;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);

            if (!Validate(out string error))
                Debug.LogWarning($"[{name}] {error}", this);
        }
#endif
    }
}