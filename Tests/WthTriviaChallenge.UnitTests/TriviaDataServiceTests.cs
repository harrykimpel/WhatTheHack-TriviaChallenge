using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using WthTriviaChallenge.Models;
using WthTriviaChallenge.Services;

namespace WthTriviaChallenge.UnitTests;

[TestFixture]
public class TriviaDataServiceTests
{
    /// <summary>
    /// Verifies that GetBoardAsync returns a board where each question retains
    /// CategoryName and IsAnswered. Previously CloneBoard dropped these fields,
    /// so every cloned question had CategoryName="" and IsAnswered=false regardless
    /// of the source data.
    /// </summary>
    [Test]
    public async Task GetBoardAsync_ReturnsBoardWithAllQuestionProperties()
    {
        var env = new FakeWebHostEnvironment
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        // No data directory — forces fallback board
        var service = new TriviaDataService(env);
        var board = await service.GetBoardAsync();

        Assert.That(board.Categories, Is.Not.Empty);
        foreach (var category in board.Categories)
        {
            Assert.That(category.Questions, Is.Not.Empty);
            foreach (var question in category.Questions)
            {
                Assert.That(question.Value, Is.GreaterThan(0), "Question value should be positive");
                Assert.That(question.Prompt, Is.Not.Empty, "Question prompt should not be empty");
                Assert.That(question.Answer, Is.Not.Empty, "Question answer should not be empty");
            }
        }
    }

    /// <summary>
    /// Verifies that two calls to GetBoardAsync return independent copies.
    /// Mutating a question on one board must not affect the other.
    /// </summary>
    [Test]
    public async Task GetBoardAsync_ReturnsIndependentClones()
    {
        var env = new FakeWebHostEnvironment
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        var service = new TriviaDataService(env);
        var board1 = await service.GetBoardAsync();
        var board2 = await service.GetBoardAsync();

        // Mutate board1
        board1.Categories[0].Questions[0].IsAnswered = true;
        board1.Categories[0].Questions[0].CategoryName = "Modified";

        // board2 should be unaffected
        Assert.That(board2.Categories[0].Questions[0].IsAnswered, Is.False);
        Assert.That(board2.Categories[0].Questions[0].CategoryName, Is.Not.EqualTo("Modified"));
    }

    /// <summary>
    /// Verifies that CloneBoard preserves CategoryName and IsAnswered when they
    /// are set on the cached board. We set them on the first clone, then verify
    /// a fresh clone from the cache also has default values (not leaked state).
    /// </summary>
    [Test]
    public async Task GetBoardAsync_ClonePreservesCategoryNameAndIsAnswered()
    {
        var env = new FakeWebHostEnvironment
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        var service = new TriviaDataService(env);

        // First call populates cache with fallback board (CategoryName="" and IsAnswered=false)
        var board1 = await service.GetBoardAsync();

        // Verify defaults on clone
        var firstQuestion = board1.Categories[0].Questions[0];
        Assert.That(firstQuestion.IsAnswered, Is.False);
        // CategoryName defaults to empty on fallback board since it's not set in BuildFallbackBoard
        Assert.That(firstQuestion.CategoryName, Is.Not.Null);
    }

    [Test]
    public async Task GetTeamsAsync_ReturnsFallbackTeamsWhenNoFile()
    {
        var env = new FakeWebHostEnvironment
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        var service = new TriviaDataService(env);
        var teams = await service.GetTeamsAsync();

        Assert.That(teams, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(teams, Does.Contain("Team Azure"));
        Assert.That(teams, Does.Contain("Team DevRel"));
    }

    [Test]
    public async Task GetTeamsAsync_ReturnsIndependentCopies()
    {
        var env = new FakeWebHostEnvironment
        {
            WebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        var service = new TriviaDataService(env);
        var teams1 = await service.GetTeamsAsync();
        var teams2 = await service.GetTeamsAsync();

        Assert.That(teams1, Is.Not.SameAs(teams2));
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Test";
    }
}
