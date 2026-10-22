#!/usr/bin/env bash
# Собирает мод и выкладывает его в папку Workshop для проверки в игре. Игра должна быть закрыта: code.dll занят
# Использование: scripts/deploy-dev.sh [Debug|Release]
set -euo pipefail

CONFIGURATION="${1:-Debug}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MSBUILD="/c/Program Files/JetBrains/JetBrains Rider 2025.2.4/tools/MSBuild/Current/Bin/MSBuild.exe"
TARGET="/c/Program Files (x86)/Steam/steamapps/workshop/content/1922740/3606829008"

BUILD_LOG="$("$MSBUILD" "$ROOT/AugustDaysMod.csproj" -p:Configuration="$CONFIGURATION" -v:m -nologo)" || true
echo "$BUILD_LOG" | grep -E "error|->" || true
if echo "$BUILD_LOG" | grep -q "error"; then
  echo "сборка не удалась, ничего не выкладываю" >&2
  exit 1
fi

cp "$ROOT/bin/$CONFIGURATION/AugustDaysMod.dll" "$TARGET/code.dll"
for file in 0Harmony.dll icon.png info.json localRU.xml scenario.json state.json countries.json story.json; do
  cp "$ROOT/assets/$file" "$TARGET/$file"
done
mkdir -p "$TARGET/images" "$TARGET/portraits"
cp "$ROOT"/assets/images/* "$TARGET/images/"
cp "$ROOT"/assets/portraits/* "$TARGET/portraits/"
echo "выложено в $TARGET"
