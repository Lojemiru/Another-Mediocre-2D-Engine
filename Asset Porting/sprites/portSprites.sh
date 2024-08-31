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
	"../GameChanger/GameChanger" sprite --input  $f"/" --output output/raw
done

# Generate Aseprite output
for d in "output/raw/"*
do
	echo "Generating .aseprite "$d"..."
	aseprite --batch $d"/"0.png --save-as output/aseprite/${d##*/}.aseprite
	aseprite output/aseprite/${d##*/}.aseprite --batch --script-param originFile=$d"/"origin.txt --script addOrigin.lua
done
