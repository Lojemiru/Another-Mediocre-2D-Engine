#!/bin/bash

echo "Removing old raw output..."

# Remove old raw output
for rf in "output/raw/"*
do
	rm -rf $rf
done

# Generate new raw output
for f in "input/"*
do
	echo "Generating raw "$f"..."
	"../GameChanger/GameChanger.exe" sprite --input  $f"/" --output output/raw
done

# Generate Aseprite output
for d in "output/raw/"*
do
	echo "Generating .aseprite "$d"..."
	"../../Aseprite/Aseprite.exe" --batch $d"/"0.png --save-as output/aseprite/${d##*/}.aseprite
	"../../Aseprite/Aseprite.exe" output/aseprite/${d##*/}.aseprite --batch --script-param originFile=$d"/"origin.txt --script addOrigin.lua
done
