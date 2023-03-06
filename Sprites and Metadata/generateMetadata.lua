-- Lua script which handles export of files

-- 99% of this script is from M3D!

-- --HELPER FUNCTIONS--

-- Returns frame count of a sprite
function frameCount(spr)
	local count = 0
	
	-- isn't lua great?
	for i = 1,#spr.frames do
	  count = count + 1
	end
	
	return count
end

-- Both "borrowed" from exportLayers.lua by @_Gaspi
function getPath(str)
   -- Source: https://stackoverflow.com/questions/9102126/lua-return-directory-path-from-path
    sep='/'
    return str:match("(.*"..sep..")")
end

function getFileName(str)
   --[[ Sources:
      - https://codereview.stackexchange.com/questions/90177/get-file-name-with-extension-and-get-only-extension
      - https://stackoverflow.com/questions/18884396/extracting-filename-only-with-pattern-matching
   --]]
   sep = '/'
   str = str:match("^.+"..sep.."(.+)$")
   return str:match("(.+)%..+")
end


function print_r(arr, indentLevel)
    local str = ""
    local indentStr = "#"

    if(indentLevel == nil) then
        print(print_r(arr, 0))
        return
    end

    for i = 0, indentLevel do
        indentStr = indentStr.."\t"
    end

    for index,value in pairs(arr) do
        if type(value) == "table" then
            str = str..indentStr..index..": \n"..print_r(value, (indentLevel + 1))
        else 
            str = str..indentStr..index..": "..value.."\n"
        end
    end
    return str
end



-- Get the current sprite
local sprite = app.activeSprite

-- Get the filename, standardized for UNIX-like because Microsoft are morons who still insist on using the backslash
local trueFilename = sprite.filename:gsub("\\", "/")

-- Get the sprite's name
local name = getFileName(trueFilename)

-- Handle there being no sprite, show an error
if (sprite == nil) then
	print("No sprite! Things are about to break horribly!")
end



-- Find the sprite's origin point
local originX = 0
local originY = 0

-- Name of the layer which has the sprite's origin
local originName = "_origin"

local attachPoints = {}
-- attach points
	-- point []
	-- [frame frame frame frame]
	-- [x, y]

-- First, find the origin layer if it exists
for i, layer in ipairs(sprite.layers) do
	-- if ((layer:cel(1) == nil)) then
		-- break
	-- end
	
	if ((layer.name == originName)) then
		-- Set the origin point
		local cell = layer:cel(1)
		local image = cell.image
		
		originX = cell.position.x
		originY = cell.position.y
		
		layer.isVisible = false
	end
	
	if (string.match(layer.name,"_attach_")) then
		local cels = layer.cels
		local aName = layer.name:gsub("_attach_", "")
				
		attachPoints[aName] = {}

		for i, cel in ipairs(cels) do
			local point = {}
			table.insert(point, cel.position.x)
			table.insert(point, cel.position.y)
			table.insert(attachPoints[aName], point)			
		end
		
		layer.isVisible = false
	end
end


---- Export the file
local filename = name..".png"
local directory = getPath(trueFilename)
local path = directory.."/"..name.."/"
path = string.gsub(path, "/","/")
-- Make the path to store the file

-- This has been removed due to being a massive pain to make work cross-platform.
-- Has to be handled outside of the script instead.

-- os.execute("rmdir \""..path.."\" /s /q")
-- os.execute("mkdir \""..path.."\"")

-- Export every frame
for i, frame in ipairs(sprite.frames) do
	local frameImage = Image(sprite.width, sprite.height)
	frameImage:drawSprite(sprite, i, Point(0,0), 0)
	frameImage:saveAs(path .. i - 1 .. ".png")
end



-- print("Hel!")
-- Opens a file 
file = io.open(path.."mdat.json", "w")

io.output(file)

local attachPointsString = ""


-- Generate attach point json
for i, aPoint in pairs(attachPoints) do
	attachPointsString = attachPointsString .. "\"" .. string.gsub(i, "_attach_", "") .. "\": ["
	
	-- Iterate through frames
	for j, frame in ipairs(aPoint) do
		-- Add frames
		attachPointsString = attachPointsString .. "[" .. frame[1] .. ", " .. frame[2] .. "]"
		
		-- Add seperator if not the last frame
		if next(aPoint, j) ~= nil then
			attachPointsString = attachPointsString .. ", "
		end
	end
	attachPointsString = attachPointsString .. "]"
	
	-- Add seperator if not the last attach point
	if next(attachPoints, i) ~= nil then
		attachPointsString = attachPointsString .. ", "
	end
end


-- print("string: " .. attachPointsString)
-- print("We reached the end folks!")
-- print(print_r(attachPoints, 1))




-- Writes the information to the file
local finishedJSON = "{\"frameCount\":" .. frameCount(sprite) .. ",\"name\":\""..name.."\",\"origin\": [" .. originX .. ", " .. originY .. "], \"attachPoints\": {" .. attachPointsString .. "}}"
-- print(finishedJSON)
io.write(finishedJSON)

-- closes the open file
io.close(file)
