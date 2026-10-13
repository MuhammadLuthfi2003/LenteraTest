using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryMatch;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle UI Refrence")]
    [SerializeField] PuzzleUI puzzleUI;

    [Header("Puzzle Data")]
    [SerializeField] private MemoryMatchData data;
    [SerializeField] private Card cardPrefab;

    [Header("Debug")]
    [Tooltip("Show every card face-up so you can check the shuffle.")]
    [SerializeField] private bool revealAllForTesting = false;


    private readonly List<Card> cards = new List<Card>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartPuzzle(MemoryMatchData data)
    {
        Cursor.lockState = CursorLockMode.None;

        this.data = data;

        if (data == null || cardPrefab == null || puzzleUI == null)
        {
            Debug.LogError("[PuzzleManager] Assign MemoryMatchData, a card prefab and the PuzzleUI.", this);
            return;
        }

        if (!data.Validate(out string error))
        {
            Debug.LogError($"[PuzzleManager] {data.name}: {error}", this);
            return;
        }

        Clear();
        puzzleUI.ConfigureLayout(data.Columns, data.Rows);

        // 1. Pick which pairs this game uses.
        List<MatchPairData> chosenPairs = PickPairs(data);

        // 2. Two entries per pair (side 0 and side 1), then shuffle them.
        var deck = new List<(MatchPairData pair, int side)>(chosenPairs.Count * 2);
        foreach (var pair in chosenPairs)
        {
            deck.Add((pair, 0));
            deck.Add((pair, 1));
        }
        Shuffle(deck);

        // 3. Spawn a card per grid cell and hand it the next entry from the shuffled deck.
        for (int i = 0; i < data.TotalCards; i++)
        {
            Card card = Instantiate(cardPrefab, puzzleUI.transform);
            card.name = $"Card_{i / data.Columns}_{i % data.Columns}";

            var (pair, side) = deck[i];
            card.Setup(pair, side, data.CardBack, data.CardBackTint);

            if (revealAllForTesting) card.SetFaceUp(true);

            cards.Add(card);
        }
    }


    public void Clear()
    {
        foreach (var card in cards)
            if (card != null) Destroy(card);
        cards.Clear();
    }


    public void TogglePuzzleUI(bool toggle)
    {
        if (puzzleUI != null)
        {
            puzzleUI.TogglePuzzlePanel(toggle);
        }
    }

    // Unique, non-null pairs from the pool, shuffled, then trimmed to what the grid needs.
    private static List<MatchPairData> PickPairs(MemoryMatchData data)
    {
        var unique = new List<MatchPairData>();
        var seen = new HashSet<string>();

        foreach (var pair in data.PairPool)
        {
            if (pair != null && seen.Add(pair.PairId))
                unique.Add(pair);
        }

        Shuffle(unique);
        return unique.GetRange(0, data.PairCount);
    }

    // Fisher-Yates
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

}
