using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace WthTriviaChallenge.Tests;

[TestFixture]
public class FullGameFlowTests : PageTest
{
    private const string AppUrl = "http://localhost:5122";

    [Test]
    public async Task FullGameFlow_SetupThroughWinner()
    {
        // Navigate to the app
        await Page.GotoAsync(AppUrl);
        await Expect(Page.Locator(".wth-app")).ToBeVisibleAsync();

        // --- Setup Phase ---
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Set up teams" })).ToBeVisibleAsync();

        // Use the default teams loaded from teams.json
        var firstTeamName = await Page.Locator(".wth-team-row .wth-input").First.InputValueAsync();
        Assert.That(firstTeamName, Is.Not.Empty, "Expected at least one default team name");

        // Start the game
        await Page.GetByRole(AriaRole.Button, new() { Name = "Start Game" }).ClickAsync();

        // --- Board Phase ---
        await Expect(Page.Locator(".wth-orbit")).ToBeVisibleAsync();
        await Expect(Page.Locator(".wth-orbit-hub")).ToBeVisibleAsync();

        // Verify all team scores start at 0
        var scoreElements = Page.Locator(".team-score");
        var scoreCount = await scoreElements.CountAsync();
        for (var i = 0; i < scoreCount; i++)
        {
            await Expect(scoreElements.Nth(i)).ToHaveTextAsync("0");
        }

        // Select the first available tile
        var firstTile = Page.Locator(".wth-orbit-tile:not([disabled])").First;
        var tileValue = await firstTile.Locator(".wth-orbit-tile-value").TextContentAsync();
        var pointValue = int.Parse(tileValue!.Replace("$", ""));
        // Orbit layout has overlapping category labels; dispatch click directly
        await firstTile.DispatchEventAsync("click");

        // --- Question Phase ---
        await Expect(Page.Locator(".wth-question")).ToBeVisibleAsync();
        await Expect(Page.Locator(".wth-question-text")).ToBeVisibleAsync();

        // Reveal the answer
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Answer" }).ClickAsync();

        // --- Answer Phase ---
        await Expect(Page.Locator(".wth-answer")).ToBeVisibleAsync();

        // Award points to the first team
        await Page.Locator(".wth-team-score-row").First
            .GetByRole(AriaRole.Button, new() { NameRegex = new Regex(@"^\+ \d+$") })
            .ClickAsync();

        // Verify first team's score updated
        await Expect(Page.Locator(".team-score").First).ToHaveTextAsync(pointValue.ToString());

        // Mark the question as answered
        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark Answered" }).ClickAsync();

        // --- Back on Board ---
        await Expect(Page.Locator(".wth-orbit")).ToBeVisibleAsync();
        await Expect(Page.Locator(".wth-orbit-tile-used")).ToHaveCountAsync(1);

        // Finish the game
        await Page.GetByRole(AriaRole.Button, new() { Name = "Finish Game" }).ClickAsync();

        // --- Winner Phase ---
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Winner" })).ToBeVisibleAsync();
        var winnerText = Page.Locator(".wth-winner-name");
        await Expect(winnerText).ToContainTextAsync(firstTeamName.Trim());
        await Expect(winnerText).ToContainTextAsync(pointValue.ToString());

        // Reset and verify we're back to setup
        await Page.GetByRole(AriaRole.Button, new() { Name = "Play Again" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Set up teams" })).ToBeVisibleAsync();
    }
}
