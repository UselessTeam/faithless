# Faithless

This is Useless Team's attempt at making a game for the [Ludum Dare](https://ldjam.com/) 47.

## Downloading the repository

This repository uses [git lfs](https://git-lfs.github.com/) to better store large files (such as images and sounds), so start by installing it on your computer.

Then, you can clone the repository with
```
git lfs clone git@github.com:Swynfel/template.git
```

## Dependencies

To build and run the project, you will need:
- [Godot 3.2.3 - Mono version](https://godotengine.org/download)
- [.NET Core 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)

### Godot
Start by installing [Godot 3.2.3 - Mono version](https://godotengine.org/download) directly on the official website.

Make sure you take the **Mono** version, and the correct 64-bit / 32-bit depending on your architecture.

### .Net Core
Although Godot mentions installing *MSBuild*, for this project it is recommended to install the [.NET Core 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) on microsoft's website.

At the time of writing this, there is no stable version, so we are using `v5.0.0-rc.1`.

You should be able to call the following command with no error, and see a line mentioning the `v5.*` sdk appear.
```
dotnet --list-sdks
```

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
