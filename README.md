# MDLabs.SchematicBrowserNavigation Documentation

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Getting Started](#getting-started)
4. [Core Concepts](#core-concepts)
5. [API Reference](#api-reference)
6. [Schema Definition](#schema-definition)
7. [Natural Language Commands and Prompt Examples](#natural-language-commands-and-prompt-examples)
8. [Configuration](#configuration)
9. [Troubleshooting](#troubleshooting)
10. [Best Practices](#best-practices)

## 1. Introduction

MDLabs.SchematicBrowserNavigation is a C# library that combines TypeChat schema-based prompt engineering with Playwright to enable natural language-driven web automation. It allows automation engineers to control web browsers using AI-powered natural language commands.

## 2. Installation

### Prerequisites

- .NET 6.0 or later
- Node.js 14.0 or later (for Playwright)

### Steps

1. Install the NuGet package:
   ```
   dotnet add package MDLabs.SchematicBrowserNavigation
   ```

2. Install Playwright browsers:
   ```
   pwsh bin/Debug/net6.0/playwright.ps1 install
   ```

3. Configure OpenAI or Azure OpenAI API key:
   Edit the `appsettings.json` file and add your API key in the OpenAI section:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```

## 3. Getting Started

Basic usage example:

```csharp
using MDLabs.SchematicBrowserNavigation;

var cancellationTokenSource = new CancellationTokenSource();
var app = new BrowserApp();

await app.InitializePlaywright();
await app.EvalInputAsync("Navigate to Home page.", cancellationTokenSource.Token).ConfigureAwait(false);
```

## 4. Core Concepts

- **BrowserApp**: The main class for interacting with the library
- **UserInterfaceIntent**: Defines the structure of automation commands
- **Natural Language Commands**: Human-readable instructions for automation tasks

## 5. API Reference

### BrowserApp

- `InitializePlaywright()`: Initializes the Playwright browser
- `ProcessInputAsync(string input, CancellationToken cancelToken)`: Processes a natural language command

### UserInterfaceIntent

- `Intent`: The type of action to perform (e.g., NavigateToUrl, ClickButton)
- `TargetElement`: The UI element to interact with
- `TargetPage`: The page to navigate to
- `Inputs`: Additional input data for the command

## 6. Schema Definition

The schema is defined using C# enums and classes:

```csharp
public enum UserInterfaceIntentType
{
    NavigateToUrl,
    NavigateToPage,
    ClickButton,
    FillInput,
    // ... other intent types
}

public class UserInterfaceIntent
{
    public UserInterfaceIntentType Intent { get; set; }
    public UserInterfaceElement? TargetElement { get; set; }
    public UserInterfacePage? TargetPage { get; set; }
    public string[]? Inputs { get; set; }
}
```

## 7. Natural Language Commands and Prompt Examples

Examples of supported commands with their corresponding markdown prompt examples:

1. NavigateToUrl:
   ```markdown
   Navigate to https://www.example.com
   ```

2. NavigateToPage:
   ```markdown
   Go to the Home page
   ```

3. ClickButton:
   ```markdown
   Click the Login button
   ```

4. FillInput:
   ```markdown
   Enter "user@example.com" in the Email input
   ```

5. SelectDropdownOption:
   ```markdown
   Select "Manage Profile" from the "User Options" menu
   ```

6. HoverElement:
   ```markdown
   Hover over the Products menu
   ```

7. FocusElement:
   ```markdown
   Focus on the Search input
   ```

8. BlurElement:
   ```markdown
   Blur the current element
   ```

9. AssertElementAttached:
   ```markdown
   Verify that the Login button is attached to the page
   ```

10. AssertElementDetached:
    ```markdown
    Check if the Loading spinner is detached
    ```

11. AssertElementVisible:
    ```markdown
    Ensure the Error message is visible
    ```

12. AssertElementHidden:
    ```markdown
    Confirm that the Password field is hidden
    ```

13. WaitForNetwork:
    ```markdown
    Wait for the network to be idle
    ```

14. WaitForSelector:
    ```markdown
    Wait for "#someId" to appear
    ```

## 8. Configuration

The library uses configuration files for OpenAI and application-specific settings:

```csharp
OpenAIConfig config = Config.LoadOpenAI();
AppConfig appConfig = Config.LoadAppConfig();
```

### App Configuration

The `AppConfig` class defines the structure for application-specific settings:

```csharp
public class AppConfig
{
    public string BaseUrl { get; set; }
    public Dictionary<string, string> Pages { get; set; }
    public Dictionary<string, string> Elements { get; set; }
}
```

These settings are loaded from the `appsettings.json` file:

```json
{
  "App": {
    "BaseUrl": "https://sauce-demo.myshopify.com",
    "Pages": {
      "Home": "{base}/collections/all#sauce-show-wish-list",
      "Login": "{base}/account/login",
      "Catalog": "{base}/collections/all",
      "Blog": "{base}/blogs/news",
      "AboutUs": "{base}/pages/about-us",
      "SignUp": "{base}/account/register"
    },
    "Elements": {
      "SignupMenu": "[href='/account/register']",
      "LoginMenu": "[href='/account/login']",
      "LogoMenu": "#logo > a",
      "HomeMenu": "[href='/']",
      "CatalogMenu": "[href='/collections/all']",
      "BlogMenu": "[href='/blog/news']",
      "AboutUsMenu": "[href='/pages/about-us']",
      "EmailInput": "#customer_email",
      "PasswordInput": "#customer_password",
      "LoginButton": ".action_bottom > button[value='Sign In']"
    }
  }
}
```

#### Pages Configuration

The `Pages` dictionary maps page names to their URLs. The `{base}` placeholder is replaced with the `BaseUrl` value when constructing the full URL.

#### Elements Configuration

The `Elements` dictionary maps element names to their CSS selectors. These selectors are used to locate and interact with specific elements on the web pages.

This configuration allows for easy maintenance and updates of page URLs and element selectors without modifying the core code.

## 9. Troubleshooting

Common issues and solutions:

- **Playwright initialization fails**: Ensure Node.js is installed and Playwright browsers are properly set up
- **Command not recognized**: Check if the command matches the defined UserInterfaceIntentType

## 10. Best Practices

- Keep natural language commands clear and concise
- Use try-catch blocks to handle potential errors in command execution
- Validate inputs and target elements before performing actions
