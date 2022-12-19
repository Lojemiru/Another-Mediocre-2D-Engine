#!/bin/bash

recurse_and_generate () {
	for f in $1"/"*
	do
		if [ -d $f ];
		then
			recurse_and_generate $f
		elif [[ $f == *.ase* ]]
		then
			echo "Generating metadata for" $f
			folder="${f%.*}"
			rm -r $folder
			mkdir $folder
			"../Aseprite/Aseprite.exe" $f --batch --script generateMetadata.lua
		fi
	done
}

for d in "../GameContent/Graphics/pages/"*
do
	if [ -d $d ];
	then
		echo "Texturepage:" $d;
		recurse_and_generate $d
	else
		echo "WARNING:" $d "is not a directory and will be ignored."
	fi
done
