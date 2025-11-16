cd   C:\Users\EA68~1\RiderProjects\AugustDaysMod\bin\Debug
copy .\AugustDaysMod.dll           C:\Users\EA68~1\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2\mods\1991mod
cd   C:\Users\EA68~1\RiderProjects\AugustDaysMod\assets
copy .\rules.json    C:\Users\EA68~1\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2\mods\1991mod
copy .\scenario.json C:\Users\EA68~1\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2\mods\1991mod
copy .\state.json    C:\Users\EA68~1\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2\mods\1991mod
cd                   C:\Users\EA68~1\AppData\LocalLow\Nostalgames\CrisisInTheKremlin2\mods\1991mod
del .\code.dll
ren "AugustDaysMod.dll" "code.dll"
cd C:\Users\EA68~1\RiderProjects\AugustDaysMod
start steam://rungameid/1922740