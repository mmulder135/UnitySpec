# ScenarioContext

You may have at least seen the ScenarioContext from the code that UnitySpec generates when a missing step definition is found: `ScenarioContext.Pending();`

`ScenarioContext` provides access to several functions, which are demonstrated using the following scenarios.

## Accessing the ScenarioContext

### In Bindings

To access the `ScenarioContext` you have to get it via [context injection](Context-Injection.md).

Example:

```csharp
[Binding]
public class Binding
{
    private ScenarioContext _scenarioContext;

    public Binding(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
}
```

Now you can access the ScenarioContext in all your Bindings with the `_scenarioContext` field.

### In Hooks

#### Before/AfterTestRun

Accessing the `ScenarioContext` is not possible, as no `Scenario` is executed when the hook is called.

#### Before/AfterFeature

Accessing the `ScenarioContext` is not possible, as no `Scenario` is executed when the hook is called.

#### Before/AfterScenario

Accessing the `ScenarioContext` is done like in [normal bindings](#in-bindings)

#### Before/AfterStep

Accessing the `ScenarioContext` is done like in [normal bindings](#in-bindings)


## ScenarioContext.Pending

See [Mark Steps as not implemented](../Execution/Mark-Steps-As-Not-Implemented.md)

## Storing data in the ScenarioContext

ScenarioContext helps you store values in a dictionary between steps. This helps you to organize your step definitions better than using private variables in step definition classes.

## ScenarioContext.ScenarioInfo

`ScenarioContext.ScenarioInfo` allows you to access information about the scenario currently being executed, such as its title and scenario and feature tags:

In the .feature file:

```gherkin
@showUpInScenarioInfo @andThisToo

Scenario: Showing information of the scenario

When I execute any scenario
Then the ScenarioInfo contains the following information
    | Field | Value                               |
    | Tags  | showUpInScenarioInfo, andThisToo    |
    | Title | Showing information of the scenario |
```

and in the step definition:

```csharp
private class ScenarioInformation
{
    public string Title { get; set; }
    public string[] Tags { get; set; }
}

[When(@"I execute any scenario")]
public void ExecuteAnyScenario(){}

[Then(@"the ScenarioInfo contains the following information")]
public void ScenarioInfoContainsInterestingInformation(Table table)
{
    // Create our small DTO for the info from the step
    var fromStep = table.CreateInstance<ScenarioInformation>();
    fromStep.Tags =  table.Rows[0]["Value"].Split(',');

    // Short-hand to the scenarioInfo
    var si = _scenarioContext.ScenarioInfo;

    // Assertions
    si.Title.Should().Equal(fromStep.Title);
    for (var i = 0; i < si.Tags.Length -1; i++)
    {
        si.Tags[i].Should().Equal(fromStep.Tags[i]);
    }
}
```

`ScenarioContext.ScenarioInfo` also provides access to the current set of arguments from the scenario's examples in the form of an `IOrderedDictionary`: 

```gherkin
Scenario: Accessing the current example 

When I use examples in my scenario
Then the examples are available in ScenarioInfo 

    Examples:
    | Sport      | TeamSize |
    | Soccer     | 11       |
    | Basketball | 6        |
```

```csharp
public class ScenarioExamplesDemo
{
    private ScenarioInfo _scenarioInfo;

    public ScenarioExamplesDemo(ScenarioInfo scenarioInfo)
    {
        _scenarioInfo = scenarioInfo;
    }

    [When(@"I use examples in my scenario")]
    public void IUseExamplesInMyScenario() {}

    [Then(@"the examples are available in ScenarioInfo")]
    public void TheExamplesAreAvailableInScenarioInfo()
    {
        var currentArguments = _scenarioInfo.Arguments;
        var currentSport = currentArguments["Sport"];
        var currentTeamSize = currentArguments["TeamSize"];
        Console.WriteLine($"The current sport is {currentSport}");
        Console.WriteLine($"The current sport allows teams of {currentTeamSize} players");
    }
}
```

Another use is to check if an error has occurred, which is possible with the `ScenarioContext.TestError` property, which simply returns the exception.

You can use this information for �error handling�. Here is an uninteresting example:

in the .feature file:

```gherkin
#This is not so easy to write a scenario for but I've created an AfterScenario-hook
@showingErrorHandling
Scenario: Display error information in AfterScenario
When an error occurs in a step

```

and the step definition:

```csharp
[When("an error occurs in a step")]
public void AnErrorOccurs()
{
    "not correct".Should().Equal("correct");
}

[AfterScenario("showingErrorHandling")]
public void AfterScenarioHook()
{
    if(_scenarioContext.TestError != null)
    {
        var error = _scenarioContext.TestError;
        Console.WriteLine("An error ocurred:" + error.Message);
        Console.WriteLine("It was of type:" + error.GetType().Name);
    }
}
```

This is another example, that might be more useful:

```csharp
[AfterScenario]
public void AfterScenario()
{
    if(_scenarioContext.TestError != null)
    {
        WebBrowser.Driver.CaptureScreenShot(_scenarioContext.ScenarioInfo.Title);
    }
}
```

In this case, MvcContrib is used to capture a screenshot of the failing test and name the screenshot after the title of the scenario.

## ScenarioContext.CurrentScenarioBlock

Use `ScenarioContext.CurrentScenarioBlock` to query the �type� of step (Given, When or Then). This can be used to execute additional setup/cleanup code right before or after Given, When or Then blocks.

in the .feature file:

```gherkin
Scenario: Show the type of step we're currently on
    Given I have a Given step
    And I have another Given step
    When I have a When step
    Then I have a Then step
```

and the step definition:

```csharp
[Given("I have a (.*) step")]
[Given("I have another (.*) step")]
[When("I have a (.*) step")]
[Then("I have a (.*) step")]
public void ReportStepTypeName(string expectedStepType)
{
    var stepType = _scenarioContext.CurrentScenarioBlock.ToString();
    stepType.Should().Equal(expectedStepType);
}
```

## ScenarioContext.StepContext

Sometimes you need to access the currently executed step, e.g. to improve tracing. Use the `_scenarioContext.StepContext` property for this purpose.

>**Note** Big parts of this page where taken over from <https://github.com/SpecFlowOSS/SpecFlow/blob/master/docs/Bindings/ScenarioContext.md>.