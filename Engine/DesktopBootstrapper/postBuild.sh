#!/bin/bash

# arg1 is the build path, with a trailing slash.

# TODO: Run texturepage regeneration routine?

# Remove old texturepages and metadata, copy in new ones
rm -r $1textures
mkdir $1textures
cp ../../GameContent/Graphics/output/*.* $1textures

# Remove old level data, copy in new
rm -r $1worlds
cp -r ../../GameContent/Worlds $1worlds

# Build shaders, copy in new
rm -r $1shaders
mkdir $1shaders

DIR=../../GameContent/GameContent/Shaders

if [ "$(ls -A $DIR)" ];
then
    username=$(whoami)

    export MGFXC_WINE_PATH=/home/$username/.winemonogame

    dotnet tool install -g dotnet-mgfxc

    for f in ../../GameContent/GameContent/Shaders/*.fx
    do
        fullname="${f##*/}"
        mgfxc $f $1shaders/"${fullname%.*}"
    done
fi


