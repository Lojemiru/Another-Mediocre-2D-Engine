#!/bin/bash

for d in "../GameContent/Graphics/pages/"*
do
	if [ -d $d ];
	then
		echo "Generating metadata for texturepage:" $d;
		"../Aseprite/Aseprite.exe" $d/*.ase* --batch --script generateMetadata.lua
	else
		echo "WARNING:" $d "is not a directory and will be ignored."
	fi
done
