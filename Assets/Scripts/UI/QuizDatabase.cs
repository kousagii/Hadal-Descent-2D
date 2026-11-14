using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuizQuestion
{
    [TextArea(3, 5)]
    public string questionText;
    
    [Tooltip("Exactly 4 answers: A, B, C, D")]
    public string[] answers = new string[4];
    
    [Tooltip("The index (0 to 3) of the correct answer from the list above")]
    [Range(0, 3)]
    public int correctAnswerIndex;
}

[System.Serializable]
public class SpeciesQuestionPool
{
    [Tooltip("Name of the species (e.g., 'Clownfish')")]
    public string speciesName;
    
    [Tooltip("List of all possible questions for this species. 1 will be randomly picked.")]
    public List<QuizQuestion> questions;
}

[CreateAssetMenu(fileName = "NewQuizDatabase", menuName = "Hadal/Quiz Database")]
public class QuizDatabase : ScriptableObject
{
    [Tooltip("Add a pool for every species in the zone.")]
    public List<SpeciesQuestionPool> speciesPools;
}
