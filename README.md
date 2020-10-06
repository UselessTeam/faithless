![Faithless](https://static.jam.vg/raw/27e/1/z/38d0b.jpg)

This is Useless Team's attempt at making a game for the [Ludum Dare](https://ldjam.com/) 47.

The submittion, called **Faithless Land** can be found at https://ldjam.com/events/ludum-dare/47/faithless-land.

## Downloading the repository

This repository uses [git lfs](https://git-lfs.github.com/) to better store large files (such as images and sounds), so start by installing it on your computer.

Then, you can clone the repository with
```
git lfs clone git@github.com:UselessTeam/faithless.git
```

## Dependencies

To build and run the project, you will need:
- [Godot 3.2.3 - Mono version](https://godotengine.org/download)
- [.NET Core 3.1](https://docs.microsoft.com/en-us/dotnet/core/install/)

### Godot
Start by installing [Godot 3.2.3 - Mono version](https://godotengine.org/download) directly on the official website.

Make sure you take the **Mono** version, and the correct 64-bit / 32-bit depending on your architecture.

### .Net Core
Although Godot mentions installing *MSBuild*, for this project it is recommended to install the [.NET Core 3.1 SDK](https://docs.microsoft.com/en-us/dotnet/core/install/) on microsoft's website.

You should be able to call the following command with no error, and see a line mentioning the `v3.1.*` sdk appear.
```
dotnet --list-sdks
```

We tried to use the `.Net Core 5.0` version, but there were many issues with, so we downgraded it.

## Build and Run
Open the Godot editor, and select the project.
Press the run icon (triangle in top-right corner), or press F5.

If Godot has trouble finding the *.NET Core* SDK, go in 
*Editor/"Editor Settings..."/Mono/Builds/"Build Tool"* and check that the correct tool is selected (probably *dotnet CLI*)


## Development

### Omnisharp

There is a `omnisharp.json` file to handle the code formatting conventions.

If you are using VSCode, and you installed dotnet through snap, Omnisharp might have trouble finding the dotnet SDK.
A workaround is to create a direct simlink from the `dotnet` executable to a folder in your `$PATH`.
```
ln -s /snap/dotnet-sdk/current/dotnet /usr/local/bin/dotnet
```
