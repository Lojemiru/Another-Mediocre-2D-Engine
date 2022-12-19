#!/bin/bash

# arg1 is the build path, with a trailing slash.

# TODO: Run texturepage regeneration routine?

# Remove old texturepages and metadata, copy in new ones
rm $1textures/*
cp ../../GameContent/Graphics/output/*.* $1textures/
