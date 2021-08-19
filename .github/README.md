# UNES

<div align="center">    
<img src="images/UNES_Preview.png"/>
</div>


`UNES` is an emulator plug-in that runs `Nintendo Entertainment System` Nintendo FC Game `*.nes` files in the `Unity` environment. The project is based on [Emulator.NES](https://github.com/Xyene/Emulator.NES) to achieve cross-platform through `Unity`.


![license](https://img.shields.io/github/license/ls9512/UNES)
[![Release Version](https://img.shields.io/badge/release-1.0.0-red.svg)](https://github.com/ls9512/UNES/releases)
![topLanguage](https://img.shields.io/github/languages/top/ls9512/UNES)
![size](https://img.shields.io/github/languages/code-size/ls9512/UNES)
![last](https://img.shields.io/github/last-commit/ls9512/UNES)
[![996.icu](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu)

[[中文文档]](README_CN.md)

<!-- vscode-markdown-toc -->
* 1. [Start](#Start)
* 2. [Load](#Load)
	* 2.1. [Resources loading](#Resourcesloading)
	* 2.2. [FileStream loading](#FileStreamloading)
* 3. [Configuration](#Configuration)
	* 3.1. [Filter Mode](#FilterMode)
	* 3.2. [Logic Thread](#LogicThread)
	* 3.3. [Input Config](#InputConfig)
* 4. [input](#input)
* 5. [API](#API)
	* 5.1. [Boot](#Boot)
	* 5.2. [Save](#Save)
	* 5.3. [Load](#Load-1)
* 6. [Mapper](#Mapper)
* 7. [ Problem](#Problem)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  1. <a name='Start'></a>Start
* 1. Create a new or select a `GameObject` in the scene and add the `UNESBehaviour` component.
* 2. Create a new `RenderTexture` to render the game screen.
* 3. Use any way you want to display the `RenderTexture` file in the game.
* 4. Use the default input method or realize custom input on demand.
* 5. Implement the loading of `*.nes` files on demand to obtain `byte[]` format data.
* 6. Call the `UNESBehaviour.Boot(byte[] romData)` interface to start the game.

##  2. <a name='Load'></a>Load
###  2.1. <a name='Resourcesloading'></a>Resources loading
If you need to use the `Resources.Load()` interface to load the ROM file, you need to pay attention to changing the `.nes` extension to `.bytes`, and then use the following method to load:
``` csharp
var bytes = Resources.Load<TextAsset>(romPath).bytes;
UNES.BootRom(bytes);
```
###  2.2. <a name='FileStreamloading'></a>FileStream loading
If you use the method of loading the original file byte stream, you can directly call the `UNESBehaviour.Boot(byte[] romData)` interface.

##  3. <a name='Configuration'></a>Configuration
###  3.1. <a name='FilterMode'></a>Filter Mode
Filter mode of game screen rendering:
|Mode|Description|
|-|-|
|Point|Texture pixels become blocky at close range. |
|Bilinear|Bilinear bilinear filtering-averages the texture samples. |
|Trilinear|Trilinear Trilinear filtering-averages texture samples and blends between mipmap levels. |

For detailed explanation, please refer to [FilterMode](https://docs.unity3d.com/ScriptReference/FilterMode.html)

###  3.2. <a name='LogicThread'></a>Logic Thread
If the `Logic Thread` option is turned on, the simulation calculations of the `CPU` and `PPU` parts will be executed by other thread, and the Unity main thread is only responsible for reading the status data to refresh the game screen, which can significantly increase the number of frames.

###  3.3. <a name='InputConfig'></a>Input Config
Customize the physical keyboard keys corresponding to the native keys.

##  4. <a name='input'></a>input
Default configuration control method:
|Native buttons|Operation buttons|
|-|-|
|Start|Num1|
|Select|Num2|
|Up|Up Arrow|
|Down|Down Arrow|
|Left|Left Arrow|
|Right|Right Arrow|
|A|A|
|B|S|

##  5. <a name='API'></a>API
###  5.1. <a name='Boot'></a>Boot
Obtain the byte array format of the original ROM file in any way for the emulator to start:
``` csharp
public void Boot(byte[] romData);
```

###  5.2. <a name='Save'></a>Save
The simulator itself only provides the data of the current running state, and does not provide the persistence implementation of the data file. Need to realize the preservation of archived data by oneself.
``` csharp
public byte[] GetSaveData();
```

###  5.3. <a name='Load-1'></a>Load
Obtain archive file data in any way for the emulator to restore game progress:
``` csharp
public void LoadSaveData(byte[] saveData);
```

##  6. <a name='Mapper'></a>Mapper
There are many Mapper extension formats in NES, and the implemented part of the project implementation can theoretically support most common games.
|||
|-|-|
|0|[NROM](http://bootgod.dyndns.org:7777/search.php?ines=0)|
|1|[MMC1](http://bootgod.dyndns.org:7777/search.php?ines=1)|
|2|[UxROM](http://bootgod.dyndns.org:7777/search.php?ines=2)
|3|[CNROM](http://bootgod.dyndns.org:7777/search.php?ines=3)|
|4|[MMC3](http://bootgod.dyndns.org:7777/search.php?ines=4)|
|7| [AxROM](http://bootgod.dyndns.org:7777/search.php?ines=7)|
|9|[MMC2](http://bootgod.dyndns.org:7777/search.php?ines=9) (*Mike Tyson's Punch-Out!!*)|
|10|[MMC4](http://bootgod.dyndns.org:7777/search.php?ines=10)|
|11|[Color Dreams](http://bootgod.dyndns.org:7777/search.php?ines=11)|
|66|[GxROM](http://bootgod.dyndns.org:7777/search.php?ines=66)|
|71|[Camerica](http://bootgod.dyndns.org:7777/search.php?ines=71)|
|79|[NINA-003-006](http://bootgod.dyndns.org:7777/search.php?ines=79)|
|94|[*Senjou no Ookami*](http://bootgod.dyndns.org:7777/search.php?ines=94)|
|140|[Jaleco](http://bootgod.dyndns.org:7777/search.php?ines=140)|
|155|[MMC1A](http://bootgod.dyndns.org:7777/search.php?ines=155)|
|180|[*Crazy Climber*](http://bootgod.dyndns.org:7777/search.php?ines=180)|
|206|[DxROM](http://bootgod.dyndns.org:7777/search.php?ines=206)|

##  7. <a name='Problem'></a> Problem
* Audio `APU` simulation is not implemented.
* Only realize Unity basic input system and pure keyboard operation mode.
* Not all Mappers are implemented.
* The performance of the PPU simulation part is low, and the frame number is unstable on the low-end mobile devices.