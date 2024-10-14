using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A puzzle in which several objects must be activated in the correct order
/// </summary>
public class SequencePuzzle : Puzzle
{
    [SerializeField] private Interactable[] sequenceObjects; // The objects that must be activated in the correct order
    [SerializeField, ReadOnly] private int[] sequenceOrder; // The order in which the objects must be activated
    [SerializeField, ReadOnly] private List<int> clickedSequence = new List<int>(); // The sequence of objects that have been clicked


    protected override void Start()
    {
        StartPuzzle();
        base.Start();
    }

    public override void StartPuzzle()
    {
        base.StartPuzzle();
        SetSequence();
    }

    public override void ResetPuzzle()
    {
        base.ResetPuzzle();
        clickedSequence.Clear();
    }

    /// <summary>
    /// Sets up the sequence of objects to be clicked in the correct order.
    /// It clears the clicked sequence, creates a new sequence order, and adds a listener to each sequence object that calls the OnSequenceObjectInteract function when they are interacted with.
    /// The sequence order is shuffled to randomize the correct order.
    /// </summary>
    private void SetSequence()
    {
        clickedSequence.Clear();
        sequenceOrder = new int[sequenceObjects.Length];

        for (int i = 0; i < sequenceOrder.Length; i++)
        {
            int sequenceIndex = i;
            sequenceObjects[i].OnInteract.AddListener(() => OnSequenceObjectInteract(sequenceIndex));
            sequenceOrder[i] = i;
        }

        var rng = new System.Random();
        rng.Shuffle(sequenceOrder);
    }

    /// <summary>
    /// Called when a sequence object is interacted with.
    /// Adds the index of the interacted object to the clicked sequence.
    /// If the clicked sequence is not equal to the sequence order, it fails the puzzle.
    /// If the end of the sequence is reached, it solves the puzzle.
    /// </summary>
    private void OnSequenceObjectInteract(int index)
    {
        if (PuzzeSolved) return;

        clickedSequence.Add(index);

        for (int i = 0; i < clickedSequence.Count; i++)
        {
            if(sequenceOrder[i] != clickedSequence[i])
            {
                Invoke(nameof(FailPuzzle), 0.4f);
                return;
            }

            if(i == sequenceOrder.Length - 1)
            {
                SolvePuzzle();
                return;
            }
        }
    }
}
