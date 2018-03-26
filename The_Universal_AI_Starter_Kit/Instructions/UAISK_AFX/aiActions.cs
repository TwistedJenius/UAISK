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


//-----------------------------------------------------------------------------
//Action and Command Functions
//-----------------------------------------------------------------------------

//This function does an action by spawn group when called
function AIPlayer::actionByGroup(%spawnGroup, %action)
{
    echo("Now " @ %action @ "ing group: " @ %spawnGroup);

    if (%spawnGroup !$= "all")
        %name = "MarkerSpawnGroup" @ %spawnGroup;
    else
        %name = allMarkersSet;

    if (!isObject(%name))
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Group " @ %spawnGroup @ " is not valid.");

        return;
    }

    AIPlayer::actionBeingDone(%name, %action);
}

//This function does an action by team number when called
function AIPlayer::actionByTeam(%team, %action)
{
    echo("Now " @ %action @ "ing team: " @ %team);

    if (%team !$= "all")
        %name = "MarkerTeamsSet" @ %team;
    else
        %name = allMarkersSet;

    if (!isObject(%name))
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Team " @ %team @ " is not valid.");

        return;
    }

    AIPlayer::actionBeingDone(%name, %action);
}

//Do the action now that we found the correct simset
function AIPlayer::actionBeingDone(%name, %action)
{
    //Get the number of markers
    %n = %name.getCount();

    for (%i = 0; %i < %n; %i++)
    {
        //Get the name of what we're dealing with now
        %obj = %name.getObject(%i);
        %bot = %obj.botBelongsToMe;

        //Now check what action to take
        switch$(%action)
        {
        case "spawn":
            //Let's spawn some bad guys!
            %player = AIPlayer::spawnAtMarker(%obj);

        case "delete":
            //Check if there's a bot from this marker then get rid of it
            if (%obj.botBelongsToMe !$= "")
            {
				if (isObject(%bot))
                {
                    if (%bot.getState() $= "Dead" || %bot.specialMove)
                        continue;

					cancel(%bot.ailoop);
					%bot.delete();
				}

                %obj.botBelongsToMe = "";
                cancel(%obj.delayRespawn);
                %obj.delayRespawn = "";
                %obj.respawnCount = "";
                %obj.respawnCounter = "";
            }

        case "kill":
            //Get the bot's id from the marker then kill it
            if (%obj.botBelongsToMe !$= "")
                %bot.kill();

        case "stop":
            //Set the bot to not respawn
            %bot.respawn = false;
            cancel(%obj.delayRespawn);
            %obj.delayRespawn = "";
            %obj.respawnCount = "";
            %obj.respawnCounter = "";

        case "unspawned":
            //Spawn all bots that have not been spawned yet
            if ((!isObject(%bot) || %bot.getState() $= "Dead") && %obj.delayRespawn < 1)
                %player = AIPlayer::spawnAtMarker(%obj);

        case "inactive":
            //Check if there's a bot from this marker
            if (%obj.botBelongsToMe !$= "")
            {
                //Make sure the bot is inactive then get rid of it
                if (%bot.action $= "Guarding" || %bot.action $= "Returning")
                {
					if (isObject(%bot))
					{
                        if (%bot.getState() $= "Dead" || %bot.specialMove)
                            continue;

						cancel(%bot.ailoop);
						%bot.delete();
					}

                    %obj.botBelongsToMe = "";
                    cancel(%obj.delayRespawn);
                    %obj.delayRespawn = "";
                    %obj.respawnCount = "";
                    %obj.respawnCounter = "";
                }
            }

        default:
            %player = AIPlayer::spawnAtMarker(%obj);
        }

        //Hide or unhide the marker
        %obj.sethidden($AISK_MARKER_HIDE);

        //Work around for T3D bug
        if ($AISK_MARKER_HIDE)
            %obj.setTransform(%obj.getPosition());
    }
}

//Calls the spawn function to create the bots and place them at their starting positions.
function AIPlayer::spawnAtMarker(%obj)
{
    if (!isObject(%obj))
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Marker " @ %obj @ " is not valid.");

        return;
    }

    %player = AIPlayer::spawn(%obj);
    return %player;
}

//Calls the spawn function to create a bot and place it at where the object is.
function AIPlayer::spawnAtObject(%obj, %block, %delete, %path)
{
    if (!isObject(%obj))
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Object " @ %obj @ " is not valid.");

        return;
    }

    //Get the object's transform while it's still here
    %pos = %obj.getTransform();

    //Delete the old object we're spawning from, this is so it doesn't occupy
    //the same location as the new bot
    %obj.delete();

    //Call spawnAtPosition which does the real work
    %player = AIPlayer::spawnAtPosition(%pos, %block, %delete, %path);
    return %player;
}

//Calls the spawn function to create a bot and place it at the location supplied.
function AIPlayer::spawnAtPosition(%pos, %block, %delete, %path)
{
   //Name the new marker, try in order first
   %num = allMarkersSet.getCount() + 1;

   if (%num < 10)
       %name = "Marker0" @ %num;
   else
       %name = "Marker" @ %num;

   //If in order doesn't work, do it at random
   if (isObject(%name))
       %name = "Marker" @ getRandom(1, 999);

   if (%block $= "")
      %block = $AISK_CHAR_TYPE;

   %markerData = %block @ "Marker";

   if (!isObject(%markerData))
      %markerData = "AIPlayerMarker";

    //Add a new marker
    %marker = new StaticShape(%name) {
        canSaveDynamicFields = "1";
        position = "0 0 0";
        rotation = "1 0 0 0";
        scale = "1 1 1";
        dataBlock = %markerData;
           block = %block;
    };

    //Instead of setting the position above, we'll set the transform here
    //in case we want a specific rotation
    %marker.setTransform(%pos);

    //Get the marker's team and group
    if (%block.team !$= "")
        %team = %block.team;
    else    
        %team = $AISK_TEAM;

    if (%block.spawnGroup !$= "")
        %group = %block.spawnGroup;
    else    
        %group = $AISK_SPAWN_GROUP;

    //Add the new marker to this SimSet
    allMarkersSet.add(%marker);
    changeMarkerTeams(%marker, %team);
    changeSpawnGroups(%marker, %group);

    $AISK_GROUP.add(%marker);

    //Hide the new marker if needed
    if ($AISK_MARKER_HIDE == true)
    {
        %marker.sethidden(true);
        //Work around for T3D bug
        %marker.setTransform(%marker.getPosition());
    }

    //Set the marker's path if needed
    if (%path !$= "" && isObject(%path))
        %marker.pathname = %path;

    %player = AIPlayer::spawn(%marker);

    //Do we want to delete the marker after it's made?
    if (%delete)
    {
        //Make sure the marker isn't needed for anything before we delete it
        if (!%player.respawn && !%marker.respawnCount)
        {
            if ((!%player.behavior.returnToMarker && %player.path $= "") || (%player.behavior.returnToMarker && %player.path !$= ""))
            {
                if (!%player.behavior.isLeashed || (%player.behavior.isLeashed && %player.behavior.leashedTo !$= "%obj.marker"))
                {
                    %player.marker = "";
                    %marker.delete();
                }
            }
        }
    }

    return %player;
}
