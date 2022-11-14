-- AHHHHH I HATE LUA
-- Plots the origin point of GameChanger's raw output into an .aseprite file.

-- Open file and read each line for x/y
f = io.open(app.params["originFile"], "r")
originX = tonumber(f:read())
originY = tonumber(f:read())
io.close(f)

-- Get sprite and name reference
spr = app.activeSprite
name = spr.filename

-- CANVAS ADJUSTMENT FOR OFF-CANVAS ORIGINS --

if (originX < 0) then
	print("Origin X is lesser than width: " .. originX .. ". Resizing.")
	spr:crop(originX, 0, spr.width + (originX * -1), spr.height)
	originX = 0
elseif (spr.width <= originX) then
	print("Origin X is greater than width: " .. originX .. ". Resizing.")
	spr:crop(0, 0, originX + 1, spr.height)
end

if (originY < 0) then
	print("Origin Y is lesser than height: " .. originY .. ". Resizing.")
	spr:crop(0, originY, spr.width, spr.height + (originY * -1))
	originY = 0
elseif (spr.height <= originY) then
	print("Origin Y is greater than height: " .. originX .. ". Resizing.")
	spr:crop(0, 0, spr.width, originY + 1)
end

-- Create new layer and cell, get image reference


image = Image(1, 1)

-- Draw origin pixel
col = Color{ r=0, g=0, b=0 }
image:drawPixel(0, 0, col)
pos = Point(originX, originY)

layer = spr:newLayer()
layer.name = "_origin"
cell = spr:newCel(layer, 1, image, pos)




-- Save sprite
spr:saveAs(name)