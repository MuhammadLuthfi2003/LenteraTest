using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryMatch;
using System;

public enum PuzzleState
{
    None,
    Checking,
}

public class PuzzleManager : MonoBehaviour
{

    [Header("Puzzle UI Refrence")]
    [SerializeField] PuzzleUI puzzleUI;

    [Header("Puzzle Data")]
    [SerializeField] private MemoryMatchData data;
    [SerializeField] private Card cardPrefab;

    [Header("Debug")]
    [Tooltip("Show every card face-up so you can check the shuffle.")]
    [SerializeField] private bool revealAllForTesting = false;

    private readonly List<Card> cards = new List<Card>();

    [Header("Readonly")]
    [SerializeField] private InteractPodium currentPodium;
    [SerializeField] private Card firstChosenCard;
    [SerializeField] private Card secondChosenCard;
    private int currentPickedCount = 0;
    private int matchedCardCount = 0;

    private PuzzleState currentState = PuzzleState.None;
    public PuzzleState State { get { return currentState; } }

    public static event Action OnPuzzleSolved;
    public static event Action<int> OnAddScore;

    private void OnEnable()
    {
        Card.OnCardChosen += HandleChosenCard;
    }

    private void OnDisable()
    {
        Card.OnCardChosen -= HandleChosenCard;
    }

    public void SetCurrentPodium(InteractPodium podium)
    {
        currentPodium = podium;
    }

    public void StartPuzzle(MemoryMatchData data)
    {
        if (GameManager.Instance == null || GameManager.Instance.State.Equals(GameStates.InPuzzle))
        {
            return;
        }

        GameManager.Instance.ToggleCursor(false);
        GameManager.Instance.SetState(GameStates.InPuzzle);

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
            Card card = Instantiate(cardPrefab, puzzleUI.gridPanel.transform);
            card.name = $"Card_{i / data.Columns}_{i % data.Columns}";

            var (pair, side) = deck[i];
            card.Setup(pair, side, data.CardBack, data.CardBackTint);

            if (revealAllForTesting) card.SetFaceUp(true);

            cards.Add(card);
        }
    }

    public void StopPuzzle()
    {
        if (GameManager.Instance == null || GameManager.Instance.State.Equals(GameStates.Running))
        {
            return;
        }

        GameManager.Instance.SetState(GameStates.Running);
        Clear();
        data = null;
        SetPuzzleState(PuzzleState.None);

        firstChosenCard = null;
        secondChosenCard = null;
        currentPickedCount = 0;
        matchedCardCount = 0;

    }


    public void Clear()
    {
        foreach (var card in cards)
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        cards.Clear();
    }

    public void SetPuzzleState(PuzzleState newState)
    {
        currentState = newState;
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
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // memory match logic
    private void HandleChosenCard(Card card)
    {
        if (GameManager.Instance == null || !GameManager.Instance.State.Equals(GameStates.InPuzzle))
        if (card == null) return;

        switch (currentPickedCount)
        {
            case 0:
                firstChosenCard = card;
                currentPickedCount++;
                break;
            case 1:
                if (firstChosenCard == card) return;
                secondChosenCard = card;
                SetPuzzleState(PuzzleState.Checking);
                StartCoroutine(CheckPairs(1));
                break;
        }
    }

    IEnumerator CheckPairs(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (firstChosenCard == null || secondChosenCard == null) yield break;
        string firstId = firstChosenCard.PairId;
        string secondId = secondChosenCard.PairId;

        if (firstId == null || secondId == null) yield break;
        if (firstId != secondId)
        {
            firstChosenCard.SetFaceUp(false);
            secondChosenCard.SetFaceUp(false);

            if (data && data.HasPenalty)
            {
                OnAddScore?.Invoke(-data.MismatchPenalty);
            }
        }
        else
        {
            // if same, disable the card
            firstChosenCard.ToggleButton(false);
            secondChosenCard.ToggleButton(false);
            matchedCardCount += 2;

            if (data)
            {
                OnAddScore?.Invoke(data.PairPoints);
            }
        }

        firstChosenCard = null;
        secondChosenCard = null;
        currentPickedCount = 0;
        SetPuzzleState(PuzzleState.None);

        // if all cards have been matched
        if (matchedCardCount == cards.Count)
        {
            if (data)
            {
                OnAddScore?.Invoke(data.MatchPoints);
            }

            OnPuzzleSolved?.Invoke();

            if (currentPodium)
            {
                Destroy(currentPodium.gameObject);
                currentPodium = null;
            }

            data = null;
            matchedCardCount = 0;

        }
    }

}
