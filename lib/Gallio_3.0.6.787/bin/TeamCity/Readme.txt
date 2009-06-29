Gallio.TeamCityIntegration
==========================

This plugin provides a Test Runner Extension that publishes "service messages" that
TeamCity can interpret and present in its test results.

To use it, set the RunnerExtensions argument of the Gallio test runner you are using to
  TeamCityExtension,Gallio.TeamCityIntegration

Examples:
  
If you are using the Gallio MSBuild task:
    <Gallio RunnerExtensions="TeamCityExtension,Gallio.TeamCityIntegration"
        ... other arguments... />

If you are using the Gallio NAnt task:
    <gallio ... other arguments>
      <runner-extension value="TeamCityExtension,Gallio.TeamCityIntegration" />
      ... other arguments
    </gallio>

If you are using the Gallio Echo task:
    Gallio.Echo /re:TeamCityExtension,Gallio.TeamCityIntegration ... other arguments...
