namespace WthTriviaChallenge.Models;

public sealed class TriviaBoard
{
    public List<TriviaCategory> Categories { get; set; } = new();
}

public sealed class TriviaCategory
{
    public string Name { get; set; } = string.Empty;
    public List<TriviaQuestion> Questions { get; set; } = new();
}

public sealed class TriviaQuestion
{
    public int Value { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
}
