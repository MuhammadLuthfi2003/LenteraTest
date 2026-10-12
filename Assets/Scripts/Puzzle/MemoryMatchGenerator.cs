using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MemoryMatch
{
    /// <summary>One card position on the board.</summary>
    [Serializable]
    public struct CardSlot
    {
        public int index;          // position in the grid, row-major
        public int row;
        public int column;
        public MatchPairData pair; // which pair this card belongs to
        public int side;           // 0 or 1: which face of the pair to show

        public CardFace Face => pair.GetFace(side);
        public string PairId => pair.PairId;
    }

    /// <summary>
    /// Builds a shuffled board from a MemoryMatchData. Swap in different
    /// generator assets to change shuffle behaviour without touching game code.
    /// </summary>
    [CreateAssetMenu(menuName = "Memory Match/Memory Match Generator", fileName = "NewMemoryMatchGenerator")]
    public class MemoryMatchGenerator : ScriptableObject
    {
        [Header("Randomness")]
        [Tooltip("Pick a new seed on every Generate call.")]
        [SerializeField] private bool randomizeSeed = true;
        [SerializeField] private int seed = 12345;

        [Header("Pair Selection")]
        [Tooltip("Shuffle the pool before taking pairs, so each game uses a different subset.")]
        [SerializeField] private bool shufflePool = true;

        [Header("Layout")]
        [Tooltip("Try to avoid placing the two cards of a pair next to each other (best effort).")]
        [SerializeField] private bool avoidAdjacentPairs = true;
        [SerializeField, Min(1)] private int maxLayoutAttempts = 50;

        /// <summary>Seed used by the most recent Generate call (use it to replay a board).</summary>
        public int LastSeed { get; private set; }

        /// <summary>Generate using this asset's seed settings.</summary>
        public List<CardSlot> Generate(MemoryMatchData data)
        {
            int useSeed = randomizeSeed ? Environment.TickCount : seed;
            return Generate(data, useSeed);
        }

        /// <summary>Generate with an explicit seed (e.g. daily challenge or replay).</summary>
        public List<CardSlot> Generate(MemoryMatchData data, int useSeed)
        {
            if (data == null)
            {
                Debug.LogError("[MemoryMatchGenerator] No MemoryMatchData supplied.", this);
                return new List<CardSlot>();
            }

            if (!data.Validate(out string error))
            {
                Debug.LogError($"[MemoryMatchGenerator] {data.name}: {error}", this);
                return new List<CardSlot>();
            }

            LastSeed = useSeed;
            var rng = new Random(useSeed);

            List<MatchPairData> chosen = PickPairs(data, rng);
            List<CardSlot> cards = BuildCards(chosen);
            ArrangeCards(cards, data, rng);
            return cards;
        }

        private List<MatchPairData> PickPairs(MemoryMatchData data, Random rng)
        {
            // De-duplicate by PairId and skip nulls.
            var unique = new List<MatchPairData>();
            var seen = new HashSet<string>();
            foreach (var pair in data.PairPool)
            {
                if (pair != null && seen.Add(pair.PairId))
                    unique.Add(pair);
            }

            if (shufflePool) Shuffle(unique, rng);
            return unique.GetRange(0, data.PairCount);
        }

        private static List<CardSlot> BuildCards(List<MatchPairData> pairs)
        {
            var cards = new List<CardSlot>(pairs.Count * 2);
            foreach (var pair in pairs)
            {
                cards.Add(new CardSlot { pair = pair, side = 0 });
                cards.Add(new CardSlot { pair = pair, side = 1 });
            }
            return cards;
        }

        private void ArrangeCards(List<CardSlot> cards, MemoryMatchData data, Random rng)
        {
            int attempts = avoidAdjacentPairs ? maxLayoutAttempts : 1;

            for (int i = 0; i < attempts; i++)
            {
                Shuffle(cards, rng);
                if (!avoidAdjacentPairs || !HasAdjacentPair(cards, data.Rows, data.Columns))
                    break;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                var slot = cards[i];
                slot.index = i;
                slot.row = i / data.Columns;
                slot.column = i % data.Columns;
                cards[i] = slot;
            }
        }

        private static bool HasAdjacentPair(List<CardSlot> cards, int rows, int columns)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    string id = cards[r * columns + c].PairId;
                    if (c + 1 < columns && cards[r * columns + c + 1].PairId == id) return true;
                    if (r + 1 < rows && cards[(r + 1) * columns + c].PairId == id) return true;
                }
            }
            return false;
        }

        // Fisher-Yates
        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}