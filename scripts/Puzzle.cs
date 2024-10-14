using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for all puzzles
/// </summary>
public class Puzzle : MonoBehaviour
{
    [SerializeField] protected Sprite puzzleIcon; // The icon of the puzzle related icon. Like keys for a chest.
    [SerializeField] protected bool resetPuzzleOnFail = true; // If the puzzle should be reset when it fails
    public UnityEvent OnPuzzleSolved; // Called when the puzzle is solved
    public UnityEvent OnPuzzleFailed; // Called when the puzzle fails

    private bool puzzelStarted;
    public bool PuzzleStarted // If the puzzle is started
    {
        get => puzzelStarted;
        set
        {
            if (puzzelStarted == value) return;

            puzzelStarted = value;
        }
    }

    public bool PuzzeSolved { get; protected set; } // If the puzzle is solved

    protected virtual void Start()
    {
        ResetPuzzle(); // Reset the puzzle
    }

    protected virtual void Update()
    {
        
    }

    /// <summary>
    /// Gets the icon of the puzzle related icon. This is used to display the puzzle icon in the inventory.
    /// </summary>
    /// <returns>The puzzle icon.</returns>
    public virtual Sprite GetPuzzleIcon()
    {
        return puzzleIcon;
    }

    /// <summary>
    /// Resets the puzzle to its default state.
    /// </summary>
    public virtual void ResetPuzzle()
    {
        PuzzleStarted = false;
        PuzzeSolved = false;
    }

    /// <summary>
    /// Solves the puzzle. If the puzzle is already solved, this function does nothing.
    /// </summary>
    /// <remarks>
    /// Increments the PUZZLE_COUNT stat by one. (Steam achievements)
    /// </remarks>
    public virtual void SolvePuzzle()
    {
        if (PuzzeSolved) return;

        AchievementsManager.SetStatValueIncrement("PUZZLE_COUNT", 1);
        PuzzleStarted = false;
        PuzzeSolved = true;
        OnPuzzleSolved?.Invoke();
    }

    /// <summary>
    /// Fails the puzzle.
    /// </summary>
    /// <remarks>
    /// If resetPuzzleOnFail is true, the puzzle is reset to its default state.
    /// Invokes the OnPuzzleFailed event.
    /// </remarks>
    public virtual void FailPuzzle()
    {
        OnPuzzleFailed?.Invoke();

        if (resetPuzzleOnFail) ResetPuzzle();
    }

    /// <summary>
    /// Starts the puzzle.
    /// </summary>
    public virtual void StartPuzzle()
    {
        PuzzleStarted = true;
    }
}
