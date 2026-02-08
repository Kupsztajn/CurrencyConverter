Lab1NBP
========

Overview
--------
Lab1NBP is a small .NET console application that downloads current foreign exchange rates from the Polish National Bank (NBP), parses the XML feed, and provides a simple interactive menu for viewing currency exchange rates.

The program demonstrates a minimal layered design with separate components for fetching remote data, handling text encoding, parsing XML, business logic (exchange service), and a console UI.

Key features
------------
- Downloads the latest exchange rates (NBP table A) as XML.
- Parses exchange rates into simple models (ExchangeTable, ExchangeRate).
- Provides a console menu to view and query rates.

Project structure
-----------------
- Lab1NBP/                  - main project folder
  - Program.cs              - application entry point
  - Lab1NBP.csproj          - project file
  - Implementations/        - concrete implementations (HTTP client, encoding, XML parser)
  - Services/               - business logic (Exchange service)
  - UI/                     - console menu and user interaction
  - Models/                 - data models (ExchangeRate, ExchangeTable)
  - interfaces/             - public interfaces used across components

Prerequisites
-------------
- .NET SDK 8.0 or later installed on your machine. You can download it from https://dotnet.microsoft.com/

Build and run (PowerShell)
--------------------------
Open a PowerShell prompt in the repository root (the folder that contains the `Lab1NBP` directory) and run:

```powershell
# restore (optional) and build the project
cd .\Lab1NBP
dotnet restore; dotnet build

# run the application
dotnet run --project .\Lab1NBP.csproj
```

If you prefer to run the already-built executable (created after build), run:

```powershell
# run the compiled exe (adjust path to match configuration and target framework)
.\Lab1NBP\bin\Debug\net8.0\Lab1NBP.exe
```

Default behavior and configuration
----------------------------------
- By default the app downloads the feed from: https://static.nbp.pl/dane/kursy/xml/lastA.xml
- The URL is currently set in `Program.cs` in the `nbpUrl` variable. You can change it there to point to a different NBP table or a local file for testing.

Common issues and troubleshooting
--------------------------------
- "dotnet" not found: ensure .NET SDK is installed and available in PATH.
- Network errors when fetching NBP data: check your internet connection and whether the NBP URL is reachable from your network.
- Encoding or parsing errors: the project contains a specific encoding implementation; if you change the XML source, make sure the encoding and XML format are compatible.

Developer notes
---------------
- The code is organized around small interfaces (IRemoteRepository, IEncoding, IDocument)
