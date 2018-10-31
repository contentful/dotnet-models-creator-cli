# dotnet-models-creator-cli
A dotnet CLI to automatically create strongly typed c-sharp models. This CLI tool is used internally by the Contentful Visual Studio plugin and the Contentful Visual Studio Code plugin.

## Prerequisites
The CLI tool uses the "global tools" feature of .NET Core 2.1 and requires the .NET Core 2.1 SDK to be installed. https://www.microsoft.com/net/download/dotnet-core/2.1

## Installation
Run `dotnet tool install -g contentful.modelscreator.cli` from your command line.

## Usage
Once installed you should now be able to run `contentful.modelscreator.cli --help` to list all the available commands.

- `-h` to list this help
- `-v` to show the version of the tool installed
- `-a` to set the accesstoken to use when communicating with the Contentful API
- `-s` to set the space id to fetch the content model from
- `-n` to set the namespace for the classes being created
- `-e` to specify which environment to fetch the content model for
- `-f` to force overwrite of any existing files 
- `-p` to set the path where the class files should be created

### Examples
Running `contentful.modelscreator.cli -s qz0n5cdakyl9 -a df2a18b8a5b4426741408fc95fa4331c7388d502318c44a5b22b167c3c1b1d03` will create a number of classes in the current working directory based on the content model of the Contentful Example App space.

If you want to specify the namespace of the created classes use the `-n` switch: `contentful.modelscreator.cli -s qz0n5cdakyl9 -a df2a18b8a5b4426741408fc95fa4331c7388d502318c44a5b22b167c3c1b1d03 -n MyProject.Models` 

If you want to specify the path to create the assets in use the `-p` switch: `contentful.modelscreator.cli -s qz0n5cdakyl9 -a df2a18b8a5b4426741408fc95fa4331c7388d502318c44a5b22b167c3c1b1d03 -n MyProject.Models -p c:\temp`