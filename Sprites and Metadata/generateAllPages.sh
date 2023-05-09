#!/bin/bash

echo "Cleaning up old texturepages and metadata..."

if [ -d "../GameContent/Graphics/output" ]; then
	rm -rf "../GameContent/Graphics/output"
fi

mkdir "../GameContent/Graphics/output"

mkdir "../GameContent/Graphics/output/enums"

mkdir "../GameContent/Graphics/output/enums/sprites"

echo "Building new texturepages..."

# Rebuild pages
for d in "../GameContent/Graphics/pages/"*
do
	if [ -d $d ];
	then
		if [[ ! -z "$(ls -A "$d")" ]];
		then
			echo "Building texturepage:" $d
			TexturePacker/TexturePacker pack --input $d --name ${d##*/} --output "../GameContent/Graphics/output"
		else
			echo "Skipping empty texturepage:" $d
		fi
	fi
done

if [ ! -d "../GameContent/GameContent/AM2EAutomated" ]; then
	mkdir "../GameContent/GameContent/AM2EAutomated"
fi

# Rebuild enums
TexturePacker/TexturePacker enum --input "../GameContent/Graphics/output/enums/sprites" --output ../GameContent/GameContent/AM2EAutomated/SpriteIndex.cs --name SpriteIndex --namespace GameContent
TexturePacker/TexturePacker enum --input "../GameContent/Graphics/output/enums" --output ../GameContent/GameContent/AM2EAutomated/PageIndex.cs --name PageIndex --namespace GameContent
