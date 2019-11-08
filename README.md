# SkylessDump
Sunless Skies serialized game data export/import tool.

Can be used for modding, translation, etc.

## Usage
```
SkylessDump.exe extract <dllPath> <assetPath> <jsonOutputPath>
   Export game data from resources.assets file into JSON format.

   arguments:
       dllPath         : Path of game dll files.
       assetPath       : resources.assets file path.
       jsonOutputPath  : Dumped JSON output directory path.

SkylessDump.exe import <dllPath> <assetInputPath> <jsonInputPath> <assetOutputPath>
   Import JSON files into resources.assets file.

   arguments:
       dllPath         : Path of game dll files.
       assetInputPath  : Original resources.assets file input path.
       jsonInputPath   : JSON input directory path.
       assetOutputPath : Patched resources.assets file output path.

Examples:
   SkylessDump.exe extract .\Managed\ .\Input\resources.assets .\dump
   SkylessDump.exe import .\Managed\ .\Input\resources.assets .\dump .\Output\resources.assets
```
