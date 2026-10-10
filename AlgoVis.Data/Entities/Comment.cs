namespace AlgoVis.Data.Entities;

/// <summary>
/// Комментарий к submission. Могут писать автор задания и сам ученик.
/// </summary>
public sealed class Comment
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
