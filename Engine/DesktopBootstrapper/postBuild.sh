#!/bin/bash

# arg1 is the build path, with a trailing slash.

# TODO: Run texturepage regeneration routine?

# Remove old texturepages and metadata, copy in new ones
rm $1textures/*
cp ../../GameContent/Graphics/output/*.* $1textures/

# Remove old level data, copy in new
rm -r $1worlds
cp -r ../../GameContent/Worlds $1worlds
