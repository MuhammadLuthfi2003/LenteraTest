using System;
using UnityEngine;

namespace MemoryMatch
{
    /// <summary>What one card in a pair shows when flipped face-up.</summary>
    [Serializable]
    public class CardFace
    {
        public Sprite sprite;
        public string label;
        public Color tint = Color.white;
        public AudioClip sound;
    }

    /// <summary>
    /// One matchable pair. Classic memory uses the same face on both cards;
    /// untick "Use Same Face For Both" for things like word + picture or question + answer.
    /// </summary>
    [CreateAssetMenu(menuName = "Memory Match/Match Pair Data", fileName = "NewMatchPair")]
    public class MatchPairData : ScriptableObject
    {
        [SerializeField, Tooltip("Unique ID used to compare cards. Auto-filled if empty.")]
        private string pairId;

        [SerializeField] private bool useSameFaceForBoth = true;
        [SerializeField] private CardFace faceA = new CardFace();
        [SerializeField] private CardFace faceB = new CardFace();

        public string PairId => pairId;
        public bool UseSameFaceForBoth => useSameFaceForBoth;

        /// <summary>Side 0 = first card of the pair, side 1 = second card.</summary>
        public CardFace GetFace(int side)
        {
            if (useSameFaceForBoth || side == 0) return faceA;
            return faceB;
        }

        public bool IsMatch(MatchPairData other) => other != null && other.pairId == pairId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(pairId))
                pairId = Guid.NewGuid().ToString("N");
        }

        [ContextMenu("Regenerate Pair ID")]
        private void RegenerateId()
        {
            pairId = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}