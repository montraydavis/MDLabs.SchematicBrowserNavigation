using System.Data.Common;
using System.Text;
using MDLabs.SchematicBrowserNavigation;
using MDLabs.SchematicBrowserNavigation.Schemas;
using Microsoft.Playwright;
using Microsoft.TypeChat;

public class BrowserApp : ConsoleApp
{
    IPlaywright? _playwright;
    IBrowserContext? _context;
    IBrowser? _browser;
    IPage? _page;

    protected IPage Page => _page ?? throw new Exception("Playwright has not been initialized.");
    private AppConfig _appConfig;
    JsonTranslator<UserInterfaceIntent> _translator;

    public override async Task RunAsync(string consolePrompt, string? inputFilePath = null)
    {
        ConsolePrompt = consolePrompt;
        await InitApp();
        if (string.IsNullOrEmpty(inputFilePath))
        {
            await RunAsync();
        }
        else
        {
            await RunBatchAsync(inputFilePath);
        }
    }

    public BrowserApp()
    {
        OpenAIConfig config = Config.LoadOpenAI();
        this._appConfig = Config.LoadAppConfig();
        // Although this sample uses config files, you can also load config from environment variables
        // OpenAIConfig config = OpenAIConfig.LoadFromJsonFile("your path");
        // OpenAIConfig config = OpenAIConfig.FromEnvironment();
        _translator = new JsonTranslator<UserInterfaceIntent>(new LanguageModel(config));
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        var response = await _translator.TranslateAsync(input, cancelToken);

        try
        {
            await HandleIntentAsync(response);
        }
        catch (Exception ex)
        {
            WriteError(ex);
        }

        var sb = new StringBuilder();

        sb.AppendLine($"The intent is {response.Intent}");
        sb.AppendLine($"The target element is {response.TargetElement}");
        sb.AppendLine($"The target page is {response.TargetPage}");
        
        if(response.Inputs != null && response.Inputs.Any())
        {
            sb.AppendLine($"The inputs are {string.Join(", ", response.Inputs)}");
        }
        
        
        Console.WriteLine(sb.ToString());
    }

    private async Task HandleIntentAsync(UserInterfaceIntent response)
    {
        switch (response.Intent)
        {
            case UserInterfaceIntentType.NavigateToUrl:
                await HandleNavigateToUrlAsync(response);
                break;
            case UserInterfaceIntentType.NavigateToPage:
                await HandleNavigateToPageAsync(response);
                break;
            case UserInterfaceIntentType.ClickButton:
                await HandleClickButtonAsync(response);
                break;
            case UserInterfaceIntentType.FillInput:
                await HandleFillInputAsync(response);
                break;
            case UserInterfaceIntentType.SelectDropdownOption:
                await HandleSelectDropdownOptionAsync(response);
                break;
            case UserInterfaceIntentType.HoverElement:
                await HandleHoverElementAsync(response);
                break;
            case UserInterfaceIntentType.FocusElement:
                await HandleFocusElementAsync(response);
                break;
            case UserInterfaceIntentType.BlurElement:
                await HandleBlurElementAsync();
                break;
            case UserInterfaceIntentType.AssertElementAttached:
                await HandleAssertElementAttachedAsync(response);
                break;
            case UserInterfaceIntentType.AssertElementDetached:
                await HandleAssertElementDetachedAsync(response);
                break;
            case UserInterfaceIntentType.AssertElementVisible:
                await HandleAssertElementVisibleAsync(response);
                break;
            case UserInterfaceIntentType.AssertElementHidden:
                await HandleAssertElementHiddenAsync(response);
                break;
            case UserInterfaceIntentType.WaitForNetwork:
                await HandleWaitForNetworkAsync();
                break;
            case UserInterfaceIntentType.WaitForSelector:
                await HandleWaitForSelectorAsync(response);
                break;
            case UserInterfaceIntentType.OtherOrUnknown:
                // Handle OtherOrUnknown intent
                break;
        }
    }

    private async Task HandleNavigateToUrlAsync(UserInterfaceIntent response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        ValidateInputs(response, UserInterfaceIntentType.NavigateToUrl);

        if (response.Inputs == null || !response.Inputs.Any())
        {
            WriteError(new Exception("Inputs cannot be null or empty for NavigateToUrl intent."));
            return;
        }

        string url = response.Inputs.First();
        if (string.IsNullOrEmpty(url))
        {
            WriteError(new Exception("URL cannot be null or empty."));
            return;
        }

        url = NormalizeUrl(url);
        await this.Page.GotoAsync(url);
    }

    private async Task HandleNavigateToPageAsync(UserInterfaceIntent response)
    {
        ValidateTargetPage(response);
        var navPageName = response.TargetPage.ToString();

        if (string.IsNullOrEmpty(navPageName))
        {
            WriteError(new Exception("Did not supply page to command NavigateToPage"));
            return;
        }

        ValidatePageExists(navPageName);

        string pageUrl = GetPageUrl(navPageName);

        await this.Page.GotoAsync(pageUrl);
    }

    private async Task HandleClickButtonAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.ClickButton);
        await this.Page.ClickAsync(elementSelector);
    }

    private async Task HandleFillInputAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.FillInput);
        ValidateInputs(response, UserInterfaceIntentType.FillInput);

        if (response.Inputs == null || !response.Inputs.Any())
        {
            WriteError(new Exception("Inputs cannot be null or empty for NavigateToUrl intent."));
            return;
        }

        await this.Page.FillAsync(elementSelector, response.Inputs.First());
    }

    private async Task HandleSelectDropdownOptionAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.SelectDropdownOption);
        ValidateInputs(response, UserInterfaceIntentType.SelectDropdownOption);

        if (response.Inputs == null || !response.Inputs.Any())
        {
            WriteError(new Exception("Inputs cannot be null or empty for NavigateToUrl intent."));
            return;
        }

        await this.Page.SelectOptionAsync(elementSelector, response.Inputs.First());
    }

    private async Task HandleHoverElementAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.HoverElement);
        await this.Page.HoverAsync(elementSelector);
    }

    private async Task HandleFocusElementAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.FocusElement);
        await this.Page.FocusAsync(elementSelector);
    }

    private async Task HandleBlurElementAsync()
    {
        await this.Page.FocusAsync("html");
    }

    private async Task HandleAssertElementAttachedAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.AssertElementAttached);
        var isAttached = await this.Page.QuerySelectorAsync(elementSelector) != null;
        if (!isAttached)
        {
            throw new Exception($"Element not attached: {response.TargetElement}");
        }
    }

    private async Task HandleAssertElementDetachedAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.AssertElementDetached);
        var isDetached = await this.Page.QuerySelectorAsync(elementSelector) == null;
        if (!isDetached)
        {
            throw new Exception($"Element not detached: {response.TargetElement}");
        }
    }

    private async Task HandleAssertElementVisibleAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.AssertElementVisible);
        var isVisible = await this.Page.EvalOnSelectorAsync<bool>(elementSelector, "(element) => !element.hidden");
        if (!isVisible)
        {
            throw new Exception($"Element not visible: {response.TargetElement}");
        }
    }

    private async Task HandleAssertElementHiddenAsync(UserInterfaceIntent response)
    {
        string elementSelector = GetElementSelector(response, UserInterfaceIntentType.AssertElementHidden);
        var isHidden = await this.Page.EvalOnSelectorAsync<bool>(elementSelector, "(element) => element.hidden");
        if (!isHidden)
        {
            throw new Exception($"Element not hidden: {response.TargetElement}");
        }
    }

    private async Task HandleWaitForNetworkAsync()
    {
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task HandleWaitForSelectorAsync(UserInterfaceIntent response)
    {
        ValidateInputs(response, UserInterfaceIntentType.WaitForSelector);

        if (response.Inputs == null || !response.Inputs.Any())
        {
            throw new Exception($"Did not supply element to command {UserInterfaceIntentType.WaitForSelector}");
        }

        await this.Page.WaitForSelectorAsync(response.Inputs.First());
    }

    private string GetElementSelector(UserInterfaceIntent response, UserInterfaceIntentType intentType)
    {
        ValidateTargetElement(response, intentType);
        var targetElementName = response.TargetElement.ToString();

        if (targetElementName == null)
        {
            WriteError(new Exception($"Did not supply element to command {intentType}"));

            return string.Empty;
        }

        ValidateElementExists(targetElementName, intentType);

        return _appConfig.Elements[targetElementName];
    }

    private void ValidateInputs(UserInterfaceIntent response, UserInterfaceIntentType intentType)
    {
        if (response.Inputs == null || !response.Inputs.Any())
        {
            throw new Exception($"Did not supply input to command {intentType}");
        }
    }

    private void ValidateTargetElement(UserInterfaceIntent response, UserInterfaceIntentType intentType)
    {
        if (response.TargetElement == null || string.IsNullOrEmpty(response.TargetElement.ToString()))
        {
            throw new Exception($"Did not supply element to command {intentType}");
        }
    }

    private void ValidateElementExists(string elementName, UserInterfaceIntentType intentType)
    {
        if (!_appConfig.Elements.ContainsKey(elementName))
        {
            throw new Exception($"Invalid element for {intentType}: {elementName}");
        }
    }

    private void ValidateTargetPage(UserInterfaceIntent response)
    {
        if (response.TargetPage == null || string.IsNullOrEmpty(response.TargetPage.ToString()))
        {
            throw new Exception("Did not supply page to command NavigateToPage");
        }
    }

    private void ValidatePageExists(string pageName)
    {
        if (!_appConfig.Pages.ContainsKey(pageName))
        {
            throw new Exception($"Invalid page: {pageName}");
        }
    }

    private string GetPageUrl(string pageName)
    {
        string pageUrl = _appConfig.Pages[pageName].Replace("{base}", _appConfig.BaseUrl);
        return NormalizeUrl(pageUrl);
    }

    private string NormalizeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            url = "https://" + url;
        }
        return url;
    }

    public async Task InitializePlaywright()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var app = new BrowserApp();
            await app.InitializePlaywright();
            await app.RunAsync("😀> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            Console.ReadLine();
            return -1;
        }

        return 0;
    }
}