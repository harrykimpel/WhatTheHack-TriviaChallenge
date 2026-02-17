using Microsoft.Playwright.NUnit;

namespace WthTriviaChallenge.Tests;

[TestFixture]
public class StarterTests : PageTest
{
    private const string AppUrl = "http://localhost:5122";

    [Test]
    public async Task HomepageLoadsWithTitleAndLogo()
    {
        await Page.GotoAsync(AppUrl);

        await Expect(Page).ToHaveTitleAsync("WTH O11y-Party");
        await Expect(Page.Locator(".wth-logo")).ToBeVisibleAsync();
    }
}
