#!/bin/bash

echo "Cleaning up old texturepages and metadata..."

# Remove old enum data
for f in "../GameContent/Graphics/output/enums/"*
do
	rm $f
done

# Clean up output directory
for f in "../GameContent/Graphics/output/"*
do
	rm $f
done

echo "Building texturepages and metadata..."

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

# Rebuild enums
TexturePacker/TexturePacker enum --input "../GameContent/Graphics/output/enums/sprites" --output ../GameContent/GameContent/AM2EAutomated/SpriteIndex.cs --name SpriteIndex --namespace GameContent
TexturePacker/TexturePacker enum --input "../GameContent/Graphics/output/enums" --output ../GameContent/GameContent/AM2EAutomated/PageIndex.cs --name PageIndex --namespace GameContent
