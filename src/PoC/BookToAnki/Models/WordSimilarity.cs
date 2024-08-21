namespace BookToAnki.Models;

public record WordSimilarity(string Word1, string Word2, double Similarity, string Word1Example,
    string Word2Example, string? Word1Translation, string? Word2Translation, string? Word1ExamplePl, string? Word2ExamplePl);
