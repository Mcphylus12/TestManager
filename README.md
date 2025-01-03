# TestManager

> **SECURITY WARNING** This is a system that edits/deletes and creates pretty freely on the system its run on. Currently only minimal checks exist 
to ensure all write behaviour is contained in the root dir passed when launching. Bugs may exist that circumvent this and please dont open it up to the internet if you run it.

## What is it?
This tool is designed to make file based testing easier. File based testing being tests that are driven from files (EG uploading a file to a web api).
These tests can be annoying to maintain and you dont want to have to check in all the files and tests to something like git. This allows the tests to be defined next to the files and it all be centralised into a single place (a filesystem)

This application is run against a folder structure containing files to be used for testing. Alongside the files a JSON file is written defining the tests to run on that file. These json files a suffixed `.tsjson` to avoid clashing with normal json files that could be used in testing.
EG

- ATextFileIWouldLikeToTest.txt
```
Some Content of the file that contains information i would like to avoid changing
```

- ATextFileIWouldLikeToTest.txt.tsjson
```JSON
[{
    "Type": "SHA256",
    "Name": "CheckFileHash",
    "Parameters": {
        "ExpectedHash": "29D173D71F2F75C5E8F5CC4F7B0D8102011BABB865B742EEEF758C591908F9D8"
    }
}]
```

### Bulk mode
In bulk operation when provided with a root directory and a file globbing pattern the application will run all tests from all `tsjson` files that match the pattern
EG

```
dotnet TestManager.dll bulk -r "./Path/To/My/Test/Data/Folder" -p *.txt
```
> The file pattern is against the test files themselves not the tsjson files. The app will automatically append `.tsjson` to search. The flag defaults to * and will run all tests at the top of the test directory.

This will write JSON to the console containing the tests that were run and the results of the tests. This can be used with pipes and [tee](https://en.wikipedia.org/wiki/Tee_(command)) to write the output to file
```JSON
[
    {
        "Test": {
            "FilePath": "C:\\TestManager\\Tests\\Data\\ATextFileIWouldLikeToTest.txt",
            "TestName": "ATextFileIWouldLikeToTest.txt.tsjson:2",
            "TestFilePath": "ATextFileIWouldLikeToTest.txt.tsjson"
        },
        "Assertions": [
            {
                "AssertionName": "hashes_match",
                "IsSuccess": true,
                "Message": null
            }
        ]
    }
]
```

This mode is primarily for use in CI/CD systems.

## Management mode
For local development the application can be run to start a webserver
```
dotnet TestManager.dll manage -r "./Path/To/My/Test/Data/Folder
```

The console will log the url the server is running on and this can be navigated to on the browser to load the management UI.
The UI allows for
- Navigating the test directory
- Creating, Deleting and Editing Test configuration files
- Running single test files
- Loading/Running all tests matching a file blog patter (like bulk mode) **CLICK THE TITLE TO SWAP NAVIGATION MODES**

This is designed for local test management.

It is technically possible to host this and point it at a mounted a fileshare(This hasnt been tested but should allow the system scaling to use by teams).

## Test Type plugins
The only built in test types are checking a file hash and checking file length. More test types can be written and loaded via a plugin system.
- Build the `TestManager.PluginLib` Project
- Add the resulting DLL as a reference to a new C# Class library
- Write a new TestHandler that inherits from `ITestHandler<T>`. T is the type of the configuration object used to configure this handler
- Add test results to the Passed `TestResult` object
- Build the new C# project
- Create a config directory in the test directory used for testing called `./config/plugins` EG `./Path/To/My/Test/Data/Folder/.config/plugins`
- Copy the built dll and any dependencies (but not TestManager.PluginLib.dll) into the plugins folder

When writing the Test Handler you will be required to fill in a property called `Type` this is what is set in the tsjson to use the new test handler.

#### Example

Test Handler file
```C#
public class StartsWithTestHandler : ITestHandler<StartWithConfig>
{
    public string Type => "StartsWith";

    public Task RunTest(ITestParameters<StartWithConfig> testParameters, TestResult testResult)
    {
        var fileText = Encoding.UTF8.GetString(testParameters.File);

        testResult.AddResult("main", fileText.StartsWith(testParameters.Parameters.Text), message: "custom message i could populate with useful info in the event of failure");

        return Task.CompletedTask;
    }
}

public class StartWithConfig
{
    public string Text { get; set; }
}
```

Entry in tsjson file
```JSON
{
    "Type": "StartsWith",
    "Name": "PDF_magic_string_test",
    "Parameters": {
        "Text": "%PDF-"
    }
}
```


## Result submission plugins
A second type of plugin can be used to add functionality to how the results of a test run are processed.
Follow similar steps to above for new TestHandlers except inherit from `ITestResultIntegrator` rather than `ITestHanddler<T>`
The class will get passed the result of the run when it has finished.

- Only one Result submission plugin can be loaded at a time.
- A configuration file can be created as `.config/integrator.json`. This file will be passed as a dictionary to the plugin.

## Secrets
Some test handlers and result submission plugins may need to access secrets that youd rather not be checked in.
A secret loader can be used to load a secret in one of 2 ways. It will attempt to use the key passed to get an env variable prefixed with `TestManager_`. If the env is not present the console app (in bulk and manage modes) will prompt the user to input the secret manually.
- TestHandlers have access to a secret loader on `testParameters.SecretLoader`. 
- The test integrator is passed the secretloader in the static create method

Secrets are cached once loaded so if multiple tests require it it will only be prompted once.

## Limitations
- Keep integrator.json and Test parameter objects flat. Any nested JSON objects wont get loaded properly.

