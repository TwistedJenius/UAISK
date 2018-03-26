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


//Change what team a bot is on
function changeTeams(%obj, %team)
{
    if (%obj.team !$= "")
    {
        if (%obj.team == %team)
        {
            if ($AISK_SHOW_NAME $= "Debug")
                warn("Bot ID " @ %obj @ " is already on team " @ %team @ ".");

            return;
        }

        //Get the name of the bot's old SimSet
        %teamSet = "TeamSet" @ %obj.team;
        //Remove the bot from its old team's SimSet
        %teamSet.remove(%obj);
    }

    //Set the bot's new team
    %obj.team = %team;
    //Add the bot to its new team
    TeamSimSets(%obj, %team);

    //Change the marker's team too if this is a bot
    if (%obj.getClassName() !$= "Player")
        changeMarkerTeams(%obj.marker, %team);
    //If it's a player, also change the client's team
    else
        %obj.getControllingClient().team = %team;
}

//Change what team a marker is on
function changeMarkerTeams(%obj, %team)
{
   //Get the datablock that this marker will spawn
   if (%obj.block $= "" || %obj.block $= "-random")
      %block = $AISK_CHAR_TYPE;
   else
      %block = %obj.block;

    //Get the team that this marker belongs to
    if (%obj.team !$= "")
        %team2 = %obj.team;
    else if (%block.team !$= "")
        %team2 = %block.team;
    else    
        %team2 = $AISK_TEAM;

    //Get the name of the marker's old SimSet
    %teamSet = "MarkerTeamsSet" @ %team2;

    if (isObject(%teamSet))
        if (%teamSet.isMember(%obj))
            //Remove the marker from its old team's SimSet
            %teamSet.remove(%obj);

    //Set the marker's new team
    //%obj.team = %team;
    //Add the marker to its new team
    TeamSimSets(%obj, %team);
}

//Put the bot into a SimeSet with its teammates
function TeamSimSets(%obj, %team)
{
    //Get the total number of teams
    if (%obj.team > $TotalTeams)
        $TotalTeams = %obj.team;

    //Get the name of the SimSet this bot or marker should belong to
    if (!%obj.getFieldValue(Datablock).isAiMarker)
        %name = "TeamSet" @ %team;
    else
        %name = "MarkerTeamsSet" @ %team;

    //If the SimSet doesn't exist, make a new one
    if (!isObject(%name))
    {
        new SimSet(%name);
        UaiskData.add(%name);
    }

    //Add the bot to that SimSet
    %name.add(%obj);
}

//Change what spawn group a marker is in
function changeSpawnGroups(%obj, %spawnGroup)
{
    //Get the datablock that this marker will spawn
    if (%obj.block $= "" || %obj.block $= "-random")
      %block = $AISK_CHAR_TYPE;
    else
      %block = %obj.block;

    //Get the spawn group that this marker belongs to
    if (%obj.spawnGroup !$= "")
        %spawnGroup2 = %obj.spawnGroup;
    else if (%block.spawnGroup !$= "")
        %spawnGroup2 = %block.spawnGroup;
    else    
        %spawnGroup2 = $AISK_SPAWN_GROUP;

    //Get the name of the marker's old SimSet
    %groupSet = "MarkerSpawnGroup" @ %spawnGroup2;

    if (isObject(%groupSet))
        if (%groupSet.isMember(%obj))
            //Remove the marker from its old group's SimSet
            %groupSet.remove(%obj);

    //Set the marker's new group
    //%obj.spawnGroup = %spawnGroup;
    //Add the marker to its new group
    GroupSimSets(%obj, %spawnGroup);
}

//Put the bot into a SimeSet with its spawn group
function GroupSimSets(%obj, %spawnGroup)
{
    //Get the name of the SimSet this bot or marker should belong to
    %name = "MarkerSpawnGroup" @ %spawnGroup;

    //If the SimSet doesn't exist, make a new one
    if (!isObject(%name))
    {
        new SimSet(%name);
        UaiskData.add(%name);
    }

    //Add the bot to that SimSet
    %name.add(%obj);
}
