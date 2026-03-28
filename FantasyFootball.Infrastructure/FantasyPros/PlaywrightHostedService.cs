using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class PlaywrightHostedService : IHostedService, IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IPlaywright Playwright => _playwright ?? throw new InvalidOperationException("Playwright not initialized.");
    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized.");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        cancellationToken.ThrowIfCancellationRequested();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        if (_playwright is not null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
    }
}
