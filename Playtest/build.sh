#!/usr/bin/env bash
# Builds the phone playtest page into out/: the simulation compiled for the browser, the page, and the game's data files.
# Needs the .NET 8 SDK.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
out="$here/out"
rm -rf "$out" "$here/bin/publish"
dotnet publish "$here/Playtest.csproj" -c Release -o "$here/bin/publish" -nologo -v quiet
mkdir -p "$out"
# The artifact host won't serve paths that start with "_", so _framework becomes framework.
cp -r "$here/bin/publish/wwwroot/_framework" "$out/framework"
find "$out/framework" \( -name '*.br' -o -name '*.gz' -o -name '*.pdb' \) -delete
cp "$here/../Data/terrain.json" "$here/../Data/units.json" "$here/../Data/names.json" "$out/"
cp "$here/page/playtest.html" "$out/playtest.html"
du -sh "$out"
