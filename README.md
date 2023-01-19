# EmyChess

![Preview picture](preview.png?raw=true)

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
The latest release of EmyChess is made to work with a project created with the [VRChat Creator Companion](https://vcc.docs.vrchat.com/), with the UdonSharp package installed. Make sure to migrate your project before installing.
### Installation
1. Download the latest release of EmyChess from [here](https://github.com/emymin/EmyChess/releases/latest) or from [Booth](https://emymin.booth.pm/items/3126194)
2. Import the Unity package inside the project
3. Drag the EmyChess prefab from Packages/EmyChess/Runtime into the scene
4. Position and scale as desired

## Contributing
This prefab is currently in beta, pull requests are appreciated! Refer to the task list for planned features, as well as known issues

## License
All code in this repository is licensed under the GNU Public License v3.0, as stated in the [license file](LICENSE)

All other assets are licensed under Creative Commons Attribution-ShareAlike 4.0 as stated in the [other license file](LICENSE-CC-BY-SA), with the exception of:
- [Wood Texture](Materials/wood.png) licensed under [CC0](https://opengameart.org/node/10010)
- [Move sound](Audio/move.ogg) and [capture sound](Audio/capture.ogg) licensed under CC0 [here](https://freesound.org/people/simone_ds/sounds/366065/) and [here](https://freesound.org/people/deleted_user_2104797/sounds/144947/)
