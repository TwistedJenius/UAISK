//-----------------------------------------------------------------------------
// Copyright (c) 2018 Twisted Jenius LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------


//Make new versions of the weapon datablocks as needed for better bot aiming
function createDataWeapon(%name)
{
    if (!isObject(%name))
        return;

    %rename = %name @ "Unset";
    %image = %name.image;

    eval("datablock ItemData(" @ %rename @ " : " @ %name @ "){image = " @ %image @ "Unset;};");
    //offset = \"0.0 0.15 0.025\";
    eval("datablock ShapeBaseImageData(" @ %rename.image @ " : " @ %image @ "){shapeFileFP = \"" @ %image.shapeFile @ "\";item = " @
    %rename @ ";useEyeNode = false;correctMuzzleVector = false;eyeOffset = \"0.25 0.6 -0.4\";};");
}

//This code is used for player and weapon randomization and
//it creates a datablock for each player.
function createDataRandom()
{
    %count = DatablockGroup.getCount();

    new SimSet(playerRandomizer);
    new SimSet(weaponRandomizer);

    //Cycle through all datablocks and get the names of the player types and weapons
    for (%i = 0; %i < %count; %i++)
    {
        %data = DatablockGroup.getObject(%i);
        %dataClass = %data.getClassName();

        //Put the players and weapons into simsets
        if (%dataClass $= "PlayerData")
        {
            %dataName = %data.getName();
            playerRandomizer.add(%dataName);

            //Comment this out if you don't want an extra datablock made for each player
            //datablock, but make sure all of your markers are set to "AIPlayerMarker"
            eval("datablock StaticShapeData(" @ %dataName @ "Marker){shapeFile = \"" @ %dataName.shapeFile @ "\";isAiMarker = true;};");
        }
        else if (%dataClass $= "ItemData")
        {
            %name = %data.getName();

            //Make sure we're just adding weapons to the list and not ammo
            if (%name.className $= "Weapon")
            {
                weaponRandomizer.add(%name);

                if (%name.image.useEyeNode)
                    createDataWeapon(%name);
            }
        }
    }
}

//Execute the function above as soon as the file is loaded.
createDataRandom();

//This function calls the functions below as needed
function loadMarkers(%noCycle)
{
    if (!%noCycle)
        echo("Preparing Markers...");
    else
        echo("Preparing Prefabs...");

    //Execute the function above if it was skipped for some reason
    if (!isObject(playerRandomizer))
        createDataRandom();

    if (!isObject(UaiskData))
    {
        //Make a simgroup that will contain all AI data
        new SimGroup(UaiskData);

        //Load the behaviors
        loadAiBehaviors();

        //Group the weapon and player datablocks
        UaiskData.add(playerRandomizer);
        UaiskData.add(weaponRandomizer);

        //Organization for all markers and bots
        new SimSet(allMarkersSet);
        new SimGroup(allBotsGroup);
        UaiskData.add(allMarkersSet);
        UaiskData.add(allBotsGroup);

        //On load, make the first two teams because we'll need at least that many later
        $TotalTeams = 2;
        new SimSet(TeamSet1);
        new SimSet(TeamSet2);
        UaiskData.add(TeamSet1);
        UaiskData.add(TeamSet2);
    }

    if (isObject(MissionCleanup))
        MissionCleanup.add(UaiskData);

    //Reset varaibles from last time
    $simgroupCount = 0;

    if (!%noCycle)
    {
        getAllObjectsWanted();

        //Keep going until we've gone through every simgroup in the missiongroup
        for (%i = 1; %i <= $simgroupCount; %i++)
        {
            %name = $simgroupNameArray[%i];
            getAllObjectsWanted(%name);
        }
    }

    hideMarkers();
}

//This function cycles through all objects in a mission
function getAllObjectsWanted(%name)
{
    //If this is our first cycle, start with the missiongroup
    if (%name $= "")
        %name = $AISK_GROUP;

    //Get the number of things in this simgroup
    %n = %name.getCount();

    for (%i = 0; %i < %n; %i++)
    {
        //Get the name of what we're dealing with now
        %obj = %name.getObject(%i);

        //If this object is a simgroup, get its name so we can go through it later
        if (%obj.getClassName() $= "SimGroup")
        {
           $simgroupCount++;
           $simgroupNameArray[$simgroupCount] = %obj;
        }
        else
        {
            //Check if the object is an AI marker
            if (%obj.getFieldValue(Datablock).isAiMarker)
            {
                //Call a specific function to do things with the object
                groupMarkers(%obj);
            }
            //Other functions can be added here for other types of objects
            //Put them inside an "else if" that checks the datablock or getClassName
        }
    }
}

//This function places all markers in a simset
function groupMarkers(%obj)
{
    //Add the marker to this SimSet
    allMarkersSet.add(%obj);

   //Get the datablock that this marker will spawn
   if (%obj.block $= "" || %obj.block $= "-random")
      %block = $AISK_CHAR_TYPE;
   else
      %block = %obj.block;

    //Get the spawn group that this marker belongs to
    if (%obj.spawnGroup !$= "")
        %spawnGroup = %obj.spawnGroup;
    else if (%block.spawnGroup !$= "")
        %spawnGroup = %block.spawnGroup;
    else
        %spawnGroup = $AISK_SPAWN_GROUP;

    changeSpawnGroups(%obj, %spawnGroup);

    //Get the team that this marker belongs to
    if (%obj.team !$= "")
        %team = %obj.team;
    else if (%block.team !$= "")
        %team = %block.team;
    else
        %team = $AISK_TEAM;

    changeMarkerTeams(%obj, %team);
}

//This function hides all markers
function hideMarkers()
{
    //Should the markers be hidden on unhidden
    if ($AISK_MARKER_HIDE == true)
        echo("Hiding AI Markers...");
    else
        echo("Unhiding AI Markers...");

    //Get the number of markers
    %n = allMarkersSet.getCount();

    for (%i = 0; %i < %n; %i++)
    {
        //Get the name of what we're dealing with now
        %obj = allMarkersSet.getObject(%i);

        //Work around for T3D bug
        if ($AISK_MARKER_HIDE)
        {
            //Unhide then hide the marker again
            %obj.sethidden(0);
            %obj.sethidden(1);
            //Then set it's position again to update it
            %obj.setTransform(%obj.getPosition());
        }
        else
            //This one line is what the code should really be,
            //like the TGE and TGEA versions
            %obj.sethidden($AISK_MARKER_HIDE);
    }
}

function Prefab::onLoad(%this, %obj)
{
    //Create the standard simsets if it hasn't been done already
    if (!isObject(UaiskData))
        loadMarkers(1);

    //Get the number of things in this simgroup
    %n = %obj.getCount();

    for (%i = 0; %i < %n; %i++)
    {
        //Get the name of what we're dealing with now
        %item = %obj.getObject(%i);

        //Check if the object is an AI marker
        if (%item.getFieldValue(Datablock).isAiMarker)
        {
            //Call a specific function to do things with the object
            groupMarkers(%item);
        }
        //Other functions can be added here for other types of objects
        //Put them inside an "else if" that checks the datablock or getClassName
    }
}
