# Copilot Instructions

## Build & Run

```bash
dotnet restore          # restore dependencies
dotnet build            # build the project
dotnet run              # run the app (HTTPS, typically https://localhost:7xxx)
dotnet watch            # run with hot reload
```

### E2E Tests (Playwright)

The test project is at `Tests/WthTriviaChallenge.Tests/`. Tests require the app to be running.

```bash
# First-time setup: install Playwright browsers
cd Tests/WthTriviaChallenge.Tests
dotnet build
cp /dev/stdin bin/Debug/net10.0/Microsoft.Playwright.runtimeconfig.json <<'EOF'
{"runtimeOptions":{"tfm":"net10.0","framework":{"name":"Microsoft.NETCore.App","version":"10.0.0"}}}
EOF
PLAYWRIGHT_DRIVER_SEARCH_PATH="$(pwd)/bin/Debug/net10.0" dotnet exec bin/Debug/net10.0/Microsoft.Playwright.dll install chromium

# Run tests (app must be running on http://localhost:5122)
dotnet run --project ../../WthTriviaChallenge.csproj --urls http://localhost:5122 &
cd Tests/WthTriviaChallenge.Tests && dotnet test

# Run a single test
dotnet test --filter "FullGameFlow_SetupThroughWinner"
```

## Architecture

This is a .NET 10 Blazor Server app (`WthTriviaChallenge.csproj`) that implements a Jeopardy-style trivia game. The entire game runs as a single interactive Blazor page with server-side rendering.

- **`Program.cs`** — Entry point. Registers `TriviaDataService` as a singleton and configures Blazor Server with interactive server components.
- **`Models/TriviaBoard.cs`** — Domain model: `TriviaBoard` → `TriviaCategory` → `TriviaQuestion`. All classes are `sealed`.
- **`Services/TriviaDataService.cs`** — Loads and caches trivia data and team names from JSON files under `wwwroot/data/`. Falls back to hardcoded defaults if files are missing. Returns deep-cloned boards to avoid shared state mutation.
- **`Components/Pages/Home.razor`** — The main (and only) game page. Contains all game logic, UI, and state management in a single component. Uses a `GamePhase` enum (`Setup` → `Board` → `Question` → `Answer` → `Winner`) to drive the UI. The `Team` class is defined inline in the `@code` block.
- **`Components/Layout/`** — Minimal layout. `ReconnectModal` handles Blazor Server SignalR reconnection UX.
- **`Components/App.razor`** — Root HTML shell. Loads `newrelic.js` for optional New Relic Browser monitoring.

## Game Data

Trivia content and teams are configured via JSON files, not code:

- **`wwwroot/data/trivia-board.json`** — Categories, questions (prompts), answers, and point values.
- **`wwwroot/data/teams.json`** — Initial team names.

Alternative board files (`trivia-board-initial.json`, `trivia-board-wth-original.json`) exist as backups/variants.

## Key Conventions

- The render mode is `InteractiveServer` (SignalR-based), set on the `Home.razor` page via `@rendermode InteractiveServer`.
- `TriviaDataService` is a **singleton** — it caches data on first load and clones boards to prevent cross-request state sharing.
- All game state lives in `Home.razor`'s `@code` block — there is no separate state management layer.
- The board UI uses an orbital/radial CSS layout (not a grid), positioning tiles with inline `left`/`top` percentages calculated in Razor.
- **Playwright note:** Orbit tiles overlap with category labels, so use `DispatchEventAsync("click")` instead of `ClickAsync()` when clicking tiles in E2E tests.
- New Relic Browser integration is optional — the `wwwroot/newrelic.js` file is a placeholder. The `RecordTeamScore` JS function is called from Blazor via `IJSRuntime` on score changes.
- Model classes use `sealed` modifiers.
- The project uses nullable reference types and implicit usings (`<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`).
