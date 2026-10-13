using MemoryMatch;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private Button button;
    [SerializeField] private Image faceImage;   // shows the pair's sprite
    [SerializeField] private Image backImage;   // shows the card back
    [SerializeField] private TMP_Text label;    // optional

    public MatchPairData Pair { get; private set; }
    public int Side { get; private set; }
    public bool IsFaceUp { get; private set; }

    public string PairId => Pair != null ? Pair.PairId : null;

    public static Action<Card> OnCardChosen;

    public void Setup(MatchPairData pair, int side, Sprite back, Color backTint)
    {
        Pair = pair;
        Side = side;

        CardFace face = pair.GetFace(side);

        if (faceImage != null)
        {
            faceImage.sprite = face.sprite;
            faceImage.color = face.tint;
            faceImage.enabled = face.sprite != null;
        }

        if (label != null)
        {
            label.text = face.label;
            label.gameObject.SetActive(!string.IsNullOrEmpty(face.label));
        }

        if (backImage != null)
        {
            backImage.sprite = back;
            backImage.color = backTint;
        }

        SetFaceUp(false);
    }

    public void OnCardClicked()
    {
        if (GameManager.Instance == null || GameManager.Instance.puzzleManager == null) return;
        PuzzleManager manager = GameManager.Instance.puzzleManager;

        if (manager.State.Equals(PuzzleState.Checking)) return;        
        SetFaceUp(true);
        SendCardData();
    }

    public void SetFaceUp(bool faceUp)
    {
        IsFaceUp = faceUp;

        if (faceImage != null) faceImage.gameObject.SetActive(faceUp);
        if (label != null) label.gameObject.SetActive(faceUp && !string.IsNullOrEmpty(label.text));
        if (backImage != null) backImage.gameObject.SetActive(!faceUp);
    }

    public void SendCardData()
    {
        OnCardChosen?.Invoke(this);
    }

    public void ToggleButton(bool Toggle)
    {
        button.enabled = Toggle;
        faceImage.gameObject.SetActive(Toggle);
        backImage.gameObject.SetActive(Toggle);
    }
}
