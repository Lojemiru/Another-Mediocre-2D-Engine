#!/bin/bash

# Generate Aseprite output
for d in "output/raw/"*
do
	echo "Generating .aseprite "$d"..."
	"../../Aseprite/Aseprite.exe" --batch $d"/"0.png --save-as output/aseprite/${d##*/}.aseprite
	"../../Aseprite/Aseprite.exe" output/aseprite/${d##*/}.aseprite --batch --script-param originFile=$d"/"origin.txt --script addOrigin.lua
done
