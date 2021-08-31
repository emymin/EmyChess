# EmyChess

![Preview picture](Resources/preview.png?raw=true)

A chess prefab for VRChat SDK3 worlds, powered by Udon, written in C# using UdonSharp

Can also be found on [Booth](https://emymin.booth.pm/items/3126194)

Featuring:
- Fully synced board
- Standard mode with legal move checking and game over states
- Support for castling, pawn promotion and en passant
- Synced timer with an automatic side switching option
- Anarchy mode to remove move checking, and allow piece spawning/deleting

## Setup
### Requirements
- [Unity 2019.4.29f1](https://docs.vrchat.com/docs/current-unity-version)
- [Latest VRCSDK3 Worlds package](https://vrchat.com/home/download) 
- [Latest release of UdonSharp](https://github.com/Merlin-san/UdonSharp/releases/latest)
- (optional) [CyanEmu](https://github.com/CyanLaser/CyanEmu) for testing in editor
### Installation
1. Download the latest release of EmyChess from [here](https://github.com/emymin/EmyChess/releases/latest) or from [Booth](https://emymin.booth.pm/items/3126194)
2. Import the Unity package inside the project
3. Drag the EmyChess prefab on the scene
4. Position and scale as desired

If there are a bunch of errors after importing while using the latest SDK and UdonSharp release, recompile all scripts. If that isn't working, modifying a script and saving it seems to also force recompilation, fixing the errors.

## Contributing
This prefab is currently in beta, pull requests are appreciated! Refer to the task list for planned features, as well as known issues

## License
All code in this repository is licensed under the GNU Public License v3.0, as stated in the [license file](LICENSE)

All other assets are licensed under Creative Commons Attribution-ShareAlike 4.0 as stated in the [other license file](LICENSE-CC-BY-SA), with the exception of:
- [Wood Texture](Materials/wood.png) licensed under [CC0](https://opengameart.org/node/10010)
- [Move sound](Audio/move.ogg) and [capture sound](Audio/capture.ogg) licensed under CC0 [here](https://freesound.org/people/simone_ds/sounds/366065/) and [here](https://freesound.org/people/deleted_user_2104797/sounds/144947/)
