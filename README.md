I'm currently working on a hobby game project.
When complete it will have:
```
╭──────────────╮
│o in progress │
│x done        │
╰──────────────╯
o graphics
╠═o pixelart top-down
║ ├─o tile-based
║ └─o procedural plant graphics
┋
o worlds
╠═o multiplayer system
╠═o NPC entities
║ ├─o have the same abilities as players
║ │ └─o (competent AI)
║ ├─o animals
║ ├─o peoples
║ └─o monsters 😲
┋
o gameplay
╠═o systemic
║ ├─o interactions defined as "options -> results" instead of "if -> then"
║ ├─o weather
║ │ ├─o seasons
║ │ ├─o temperature
║ │ ├─o moisture
║ │ └─o audio
║ └─o world-event system
║   ├─o series of "listeners" for events
║   └─o everything that happens in the world fires off an event
║     ├─o of a certain type
║     └─o of a certain radius
║       └─o radius could be modified
║         ├─o by weather
║         └─o by entity skill (?)
╠═o as interconnected magic systems as I can cram in
║ ├─o rune
║ ├─o circle
║ ├─o arcane
║ ├─o divine
║ ├─o sympathetic
║ │ ├─o home territory
║ │ └─o implicit curses
║ ├─o herb
║ └─o alchemy
╠═o herbcraft
║ ├─o procedurally generated plants
║ │ ├─o effects
║ │ ├─o biome
║ │ └─o shape
║ ├─o plant research via experimentation
║ └─o crossbreeding
║   ├─o grafting
║   ├─o cross-pollination
║   └─o magical mutations
┋ 
```
The current working title is TileBasedSurvivalGame.

I'm using my own fork of [a C# port](github.com/DevChrome/Pixel-Engine) of [Javidx9's](https://onelonecoder.com/) ([his YouTube channel](https://www.youtube.com/channel/UC-yuWVUplUJZvieEligKBkA)) [olcPixelGameEngine](https://github.com/OneLoneCoder/olcPixelGameEngine) ([OLC-3 License](https://github.com/DevChrome/Pixel-Engine/blob/master/Licences.txt)).
