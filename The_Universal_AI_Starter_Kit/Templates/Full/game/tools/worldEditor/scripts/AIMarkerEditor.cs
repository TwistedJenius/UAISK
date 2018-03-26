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


//Change this to 1 to get rid of the delete marker buttons,
//leave it 0 to have that option available, this is to prevent accidents
$AIME_DisableDeleteMode = 0;

//When editing a preexisting marker, after changes are made to the current marker, should the next
//marker selected be lower or higher? 0 for lower, 1 for higher, anything else for the same
$AIME_SelectNextMarker = 1;

//When the save button is pressed; all bots are killed, then the file is saved. This is done to
//prevent markers from being saved when they are flagged as having spawned a bot already.
//Setting this variable to 1 will spawn all bots again after the save is finished, setting it to
//anything else will leave them unspawned.
$AIME_SpawnAfterSave = 0;

//Resets (or not) a marker's values when the datablock it's using is changed.
$AIME_ResetOnCharacterChange = false;

//Key binding to open the editor
GlobalActionMap.bind(keyboard, "alt a", toggleAIMarkerEditor);

//Load the other AI Marker Editor files
exec("./AIMEMain.cs");
exec("./AIPathEditor.cs");


//-----------------------------------------------------------------------------
//Loading Functions
//-----------------------------------------------------------------------------

//This function opens and closes the editor
function toggleAIMarkerEditor(%val)
{
    if (!%val)
        return;

	echo("Toggling AI Marker Editor");

    //If the editor is open then close, else open it
    if ($AIMarkerEditor::isOpen)
    {
        Canvas.popDialog(AIMarkerEditor);
        $AIMarkerEditor::isOpen = false;
        return;
    }

    %obj = getFirstSelectedObject();

    AIMarkerEditor.initEditor();
    Canvas.pushDialog(AIMarkerEditor);
    $AIMarkerEditor::isOpen = true;

    %obj = AIMarkerEditor.setSelectedOnLoad(%obj);

    AIMarkerEditor.updateControls();
    AIMarkerEditor.deselectEverything();

    AIMarkerEditor.setSelectedOnLoad2(%obj);
}

//Find the first object that's selected that we care about
function getFirstSelectedObject()
{
	if (isObject(EWorldEditor))
    {
	    if (EWorldEditor.isAwake())
        {
            %count = EWorldEditor.getSelectionSize();

            //Clear all selections
            for (%i = 0; %i < %count; %i++)
            {
                %obj = EWorldEditor.getSelectedObject(0);
                %name = %obj.getClassName();

                if (%name $= "Path" || %name $= "Marker" || %name $= "AIPlayer" || %obj.getFieldValue(Datablock).isAiMarker)
                    return %obj;
            }

            if (%count > 0)
                return 0;
        }
    }

    //If it's down here, then EWorldEditor isn't open or nothing is selected; so check if
    //the AIME has been opened before to get its previous value
    if (isObject(AIME_MarkerSelector))
    {
        %text = AIME_MarkerSelector.getText();

        if (isObject(%text))
            return %text.getId();
    }

    return 0;
}

//On load, select the correct object in the editor
function AIMarkerEditor::setSelectedOnLoad(%this, %obj)
{
    if (!isObject(%obj))
        return;

    %name = %obj.getClassName();

    if (%obj.getFieldValue(Datablock).isAiMarker)
    {
        AIME_MarkerSelector.setText(%obj.getName());
        AIME_MarkerSelector2.setText(%obj.getName());
        return 0;
    }
    else if (%name $= "AIPlayer")
    {
        AIME_MarkerSelector.setText(%obj.marker.getName());
        AIME_MarkerSelector2.setText(%obj.marker.getName());
        return 0;
    }
    else if (%name $= "Path")
        AIME_PageSelector.setText("Path Editing");
    else if (%name $= "Marker") //Path nodes
        AIME_PageSelector.setText("Path Editing");

    return %obj;
}

//On load, select the correct object in the editor
function AIMarkerEditor::setSelectedOnLoad2(%this, %obj)
{
    if (!isObject(%obj))
        return;

    %name = %obj.getClassName();

    if (%name $= "Path")
    {
        AIME_PathEditorSelector.setText(%obj.getName());
        AIME_PathEditorSelector2.setText(%obj.getName());

        AIMarkerEditor.updatePathControls();
        AIMarkerEditor.populateNodelist();

        AIME_EditorText99.setText(%obj.getCount());
    }
    else if (%name $= "Marker") //Path nodes
    {
        AIME_PathEditorSelector.setText(%obj.parentGroup.getName());
        AIME_PathEditorSelector2.setText(%obj.parentGroup.getName());

        AIMarkerEditor.updatePathControls();
        AIMarkerEditor.populateNodelist();

        AIME_EditorText99.setText(%obj.parentGroup.getCount());

        AIME_NodeSelector.setText(%obj.getName());
        AIME_NodeSelector2.setText(%obj.getName());

        AIMarkerEditor.updateNodeControls();
    }
}

//This function updates all the lists in the editor
function AIMarkerEditor::initEditor(%this)
{
   //From here until the end of the three for loops is to cycle through everything
   //and add player data, item data and behaviors to the appropriate lists.
   %count1 = playerRandomizer.getCount();
   %playerBlockCount = 0;
   AIME_CharacterSelector.clear();
   AIME_CharacterSelector2.clear();

   for (%i = 0; %i < %count1; %i++)
   {
        %obj = playerRandomizer.getObject(%i);

        AIME_CharacterSelector.add(%obj.getName(), %playerBlockCount);
        AIME_CharacterSelector2.add(%obj.getName(), %playerBlockCount);

        //Check if this datablock should be the default, set it as the default if it is
        if (%obj.getName() $= $AISK_CHAR_TYPE)
            %playerDefaultNum = %playerBlockCount;

        %playerBlockCount++;
   }

   %count2 = weaponRandomizer.getCount();
   %itemBlockCount = 0;
   AIME_WeaponSelector.clear();

   for (%i = 0; %i < %count2; %i++)
   {
        %obj = weaponRandomizer.getObject(%i);

        AIME_WeaponSelector.add(%obj.getName(), %itemBlockCount);
        %itemBlockCount++;
   }

   %count3 = allBehaviorsSet.getCount();
   %behaviorCount = 0;
   AIME_BehaviorSelector.clear();

   for (%i = 0; %i < %count3; %i++)
   {
        %obj = allBehaviorsSet.getObject(%i);

        AIME_BehaviorSelector.add(%obj.getName(), %behaviorCount);
        %behaviorCount++;
   }

   AIME_MarkerSelector.clear();
   AIME_MarkerSelector2.clear();
   AIME_PathEditorSelector.clear();
   AIME_PathEditorSelector2.clear();
   AIME_PathEditorSelector3.clear();
   AIME_PathSelector.clear();
   AIME_SimGroupSelector.clear();
   AIME_SimGroupSelector2.clear();

   //Reset varaibles from last time
   $aiMarkerCount = 0;
   $pathMarkerCount = 0;
   $simgroupCount = 0;

   AIMarkerEditor.cycleSimGroups();

   //Keep going until we've gone through every simgroup in the missiongroup
   for (%i = 1; %i <= $simgroupCount; %i++)
   {
      %name = $simNameArray[%i];
      AIMarkerEditor.cycleSimGroups(%name);
   }

   echo("AI Marker Editor counts: " @ %playerBlockCount @ " players, " @ %itemBlockCount @ " items, " @ %behaviorCount @ " behaviors, " @
       $aiMarkerCount @ " markers, and " @ $pathMarkerCount @ " paths within " @ $simgroupCount @ " simgroups.");

   //Add the default, non-dynamic options to already created lists
   AIME_CharacterSelector.add("-RANDOM", %playerBlockCount++);
   AIME_CharacterSelector2.add("-RANDOM", %playerBlockCount++);
   AIME_PathSelector.add("-NOT PATHED", $pathMarkerCount++);
   AIME_WeaponSelector.add("-NOWEAPON", %itemBlockCount++);
   AIME_WeaponSelector.add("-RANDOM", %itemBlockCount++);
   AIME_SimGroupSelector.add($AISK_GROUP, $simgroupCount++);
   AIME_SimGroupSelector2.add($AISK_GROUP, $simgroupCount++);

   //Create some new non-dynamic lists
   AIME_WeaponModeSelector.clear();
   AIME_WeaponModeSelector.add("Pattern", 0);
   AIME_WeaponModeSelector.add("Random", 1);
   AIME_WeaponModeSelector.add("Range", 2);
   AIME_WeaponModeSelector.add("Best", 3);

   AIME_ActionFunctionSelector.clear();
   AIME_ActionFunctionSelector.add("Spawn Group", 0);
   AIME_ActionFunctionSelector.add("Team", 1);

   AIME_ShowNames.clear();
   AIME_ShowNames.add("DontShow", 0);
   AIME_ShowNames.add("Show", 1);
   AIME_ShowNames.add("Debug", 2);

   AIME_ActionTypeSelector.clear();
   AIME_ActionTypeSelector.add("Spawn", 0);
   AIME_ActionTypeSelector.add("Delete", 1);
   AIME_ActionTypeSelector.add("Stop", 2);
   AIME_ActionTypeSelector.add("Unspawned", 3);
   AIME_ActionTypeSelector.add("Kill", 4);
   AIME_ActionTypeSelector.add("Inactive", 5);

   AIME_PageSelector.clear();
   AIME_PageSelector.add("Creation", 0);
   AIME_PageSelector.add("Positioning", 1);
   AIME_PageSelector.add("General", 2);
   AIME_PageSelector.add("Distances", 3);
   AIME_PageSelector.add("Weapons", 4);
   AIME_PageSelector.add("Path Management", 5);
   AIME_PageSelector.add("Path Editing", 6);
   AIME_PageSelector.add("Actions", 7);
   AIME_PageSelector.add("Globals", 8);
   AIME_PageSelector.add("Search", 9);

   AIME_PathFlipSelector.clear();
   AIME_PathFlipSelector.add("X", 0);
   AIME_PathFlipSelector.add("Y", 1);
   AIME_PathFlipSelector.add("Z", 2);
   AIME_PathFlipSelector.add("All", 3);

   AIME_AdvancedSelector.clear();
   AIME_AdvancedSelector.add("Random", 0);
   AIME_AdvancedSelector.add("Side", 1);
   AIME_AdvancedSelector.add("Back", 2);
   AIME_AdvancedSelector.add("Serpentine", 3);

   AIME_SearchSign.clear();
   AIME_SearchSign.add("$=", 0);
   AIME_SearchSign.add("!$=", 1);
   AIME_SearchSign.add("==", 2);
   AIME_SearchSign.add("!=", 3);
   AIME_SearchSign.add(">=", 4);
   AIME_SearchSign.add(">", 5);
   AIME_SearchSign.add("<=", 6);
   AIME_SearchSign.add("<", 7);

   AIME_SearchVariable.clear();
   AIME_SearchVariable.add("block", 0);
   AIME_SearchVariable.add("Weapon", 1);
   AIME_SearchVariable.add("pathname", 2);
   AIME_SearchVariable.add("maxRange", 3);
   AIME_SearchVariable.add("minRange", 4);
   AIME_SearchVariable.add("distDetect", 5);
   AIME_SearchVariable.add("sidestepDist", 6);
   AIME_SearchVariable.add("paceDist", 7);
   AIME_SearchVariable.add("behavior", 8);
   AIME_SearchVariable.add("weaponMode", 9);
   AIME_SearchVariable.add("fov", 10);
   AIME_SearchVariable.add("leash", 11);
   AIME_SearchVariable.add("team", 12);
   AIME_SearchVariable.add("cycleCounter", 13);
   AIME_SearchVariable.add("countRespawn", 14);
   AIME_SearchVariable.add("respawn", 15);
   AIME_SearchVariable.add("activeDodge", 16);
   AIME_SearchVariable.add("npcAction", 17);
   AIME_SearchVariable.add("spawnGroup", 18);
   AIME_SearchVariable.add("realName", 19);

   //Populate then sort the global variables list
   populateTheAIGlobals();

   //Sort alphabetically
   AIME_CharacterSelector.sort();
   AIME_CharacterSelector2.sort();
   AIME_WeaponSelector.sort();
   AIME_MarkerSelector.sort();
   AIME_MarkerSelector2.sort();
   AIME_PathEditorSelector.sort();
   AIME_PathEditorSelector2.sort();
   AIME_PathEditorSelector3.sort();
   AIME_PathSelector.sort();
   AIME_SimGroupSelector.sort();
   AIME_SimGroupSelector2.sort();
   AIME_BehaviorSelector.sort();
   AIME_SearchVariable.sort();

   //Select default options
   AIME_MarkerSelector.setText(AIME_MarkerSelector.getTextById(0));
   AIME_MarkerSelector2.setText(AIME_MarkerSelector2.getTextById(0));
   AIME_CharacterSelector.setText(AIME_CharacterSelector.getTextById(%playerDefaultNum));
   AIME_CharacterSelector2.setText(AIME_CharacterSelector2.getTextById(%playerDefaultNum));
   AIME_PathEditorSelector.setText(AIME_PathEditorSelector.getTextById(0));
   AIME_PathEditorSelector2.setText(AIME_PathEditorSelector2.getTextById(0));
   AIME_PathEditorSelector3.setText(AIME_PathEditorSelector3.getTextById(0));
   AIME_PathSelector.setSelected($pathMarkerCount);
   AIME_SimGroupSelector.setSelected($simgroupCount);
   AIME_SimGroupSelector2.setSelected($simgroupCount);
   AIME_BehaviorSelector.setText($AISK_BEHAVIOR);
   AIME_WeaponModeSelector.setSelected(0);
   AIME_ActionFunctionSelector.setSelected(0);
   AIME_ActionTypeSelector.setSelected(0);
   AIME_PathFlipSelector.setSelected(0);
   AIME_AdvancedSelector.setSelected(0);
   AIME_SearchSign.setSelected(0);
   AIME_SearchVariable.setSelected(0);

    if ($aiMarkerCount < 9)
       AIME_NameSelector.text = "Marker0" @ ($aiMarkerCount + 1);
    else
       AIME_NameSelector.text = "Marker" @ ($aiMarkerCount + 1);

   AIME_SpawnGroupSelector.text = $AISK_SPAWN_GROUP;
   AIME_RealNameSelector.text = $AISK_REAL_NAME;
   AIME_RotationSelector.text = "1 0 0 0";
   AIME_PositionSelector.text = "0 0 0";
   AIME_ScaleSelector.text = "1 1 1";
   AIME_RangeSelector.text = $AISK_MAX_DISTANCE;
   AIME_MinRangeSelector.text = $AISK_MIN_DISTANCE;
   AIME_DetectSelector.text = $AISK_DETECT_DISTANCE;
   AIME_SidestepSelector.text = $AISK_SIDESTEP;
   AIME_PaceSelector.text = $AISK_MAX_PACE;
   AIME_NPCActionSelector.text = "0";
   AIME_FOVSelector.text = $AISK_FOV;
   AIME_LeashSelector.text = $AISK_LEASH_DISTANCE;
   AIME_CycleCounterSelector.text = $AISK_CYCLE_COUNTER;
   AIME_RespawnCountSelector.text = $AISK_RESPAWN_COUNT;
   AIME_WeaponModeSelector.text = $AISK_WEAPON_MODE;
   AIME_TeamSelector.text = $AISK_TEAM;
   AIME_ActionNumSelector.text = "1";
   AIME_ShowNames.text = $AISK_SHOW_NAME;
   AIME_EditorText54.setText("-NONE");

   if (AIME_SearchList.getSelectedItem() >= 0)
      AIME_SearchList.setSelected(AIME_SearchList.getSelectedItem(), false);

   if (isObject(AIME_PathEditorSelector.getText()))
      AIME_EditorText99.setText(AIME_PathEditorSelector.getText().getCount());

   if ($AISK_DEFAULT_RESPAWN)
      AIME_RespawnSelector.setValue(1);
   else
      AIME_RespawnSelector.setValue(0);

   AIME_DodgeSelector.text = $AISK_ACTIVE_DODGE;

   if ($AISK_MARKER_HIDE)
      AIME_HideMarkers.setValue(1);
   else
      AIME_HideMarkers.setValue(0);

   if (AIME_CreatorEditor.isVisible())
      AIME_PageSelector.setText("Creation");
   else if (AIME_PositioningEditor.isVisible())
      AIME_PageSelector.setText("Positioning");
   else if (AIME_GeneralEditor.isVisible())
      AIME_PageSelector.setText("General");
   else if (AIME_Actions.isVisible())
      AIME_PageSelector.setText("Actions");
   else if (AIME_PathManager.isVisible())
      AIME_PageSelector.setText("Path Management");
   else if (AIME_PathEditor.isVisible())
      AIME_PageSelector.setText("Path Editing");
   else if (AIME_WeaponEditor.isVisible())
      AIME_PageSelector.setText("Weapons");
   else if (AIME_DistanceEditor.isVisible())
      AIME_PageSelector.setText("Distances");
   else if (AIME_GlobalEditor.isVisible())
      AIME_PageSelector.setText("Globals");
   else if (AIME_SearchEditor.isVisible())
      AIME_PageSelector.setText("Search");
   else
      AIME_PageSelector.setText("Creation");

    //Disable the delete buttons if desired
    if ($AIME_DisableDeleteMode != 0)
    {
        AIME_DeleteMarkerAi.setVisible(0);
        AIME_DeletePath.setVisible(0);
        AIME_DeleteNode.setVisible(0);
        AIME_NodeSelector2.setVisible(0);
    }
    else
    {
        AIME_DeleteMarkerAi.setVisible(1);
        AIME_DeletePath.setVisible(1);
        AIME_DeleteNode.setVisible(1);
        AIME_NodeSelector2.setVisible(1);
    }

    AIMarkerEditor.updatePathControls();
    AIMarkerEditor.populateNodelist();
}

//This function cycles through simgroups that are inside of the MissionGroup or another simgroup
function AIMarkerEditor::cycleSimGroups(%this, %name)
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

      //Is it an AI marker
      if (%obj.getFieldValue(Datablock).isAiMarker)
      {
         //If the marker doesn't have a name, give it one to avoid problems later
         if (%obj.getName() $= "")
         {
            if ($aiMarkerCount < 9)
               %obj.setName("Marker0" @ ($aiMarkerCount + 1));
            else
               %obj.setName("Marker" @ ($aiMarkerCount + 1));
         }

         AIME_MarkerSelector.add(%obj.getName(), $aiMarkerCount);
         AIME_MarkerSelector2.add(%obj.getName(), $aiMarkerCount);
         $aiMarkerCount++;
      }
      //Is it a path
      else if (%obj.getClassName() $= "Path")
      {
         //If the path doesn't have a name, give it one to avoid problems later
         if (%obj.getName() $= "")
         {
            if ($pathMarkerCount < 9)
               %obj.setName("Path0" @ ($pathMarkerCount + 1));
            else
               %obj.setName("Path" @ ($pathMarkerCount + 1));
         }

         AIME_PathEditorSelector.add(%obj.getName(), $pathMarkerCount);
         AIME_PathEditorSelector2.add(%obj.getName(), $pathMarkerCount);
         AIME_PathEditorSelector3.add(%obj.getName(), $pathMarkerCount);
         AIME_PathSelector.add(%obj.getName(), $pathMarkerCount);
         $pathMarkerCount++;
      }
      //If this object is a simgroup, get its name so we can go through it later
      else if (%obj.getClassName() $= "SimGroup")
      {
         AIME_SimGroupSelector.add(%obj.getName(), $simgroupCount);
         AIME_SimGroupSelector2.add(%obj.getName(), $simgroupCount);
         $simgroupCount++;
         $simNameArray[$simgroupCount] = %obj;
      }
    }
}

//Populate then sort the global variables list
function populateTheAIGlobals()
{
    %i = 0;
    AIME_VariableValueSelector.text = 1;
    AIME_GlobalVariableSelector.clear();

    AIME_GlobalVariableSelector.add("MARKER_HIDE", %i++);
    AIME_GlobalVariableSelector.add("FIRE_DELAY", %i++);
    AIME_GlobalVariableSelector.add("TRIGGER_DOWN", %i++);
    AIME_GlobalVariableSelector.add("FOV", %i++);
    AIME_GlobalVariableSelector.add("ENHANCED_FOV_CHANCE", %i++);
    AIME_GlobalVariableSelector.add("ENHANCED_FOV_TIME", %i++);
    AIME_GlobalVariableSelector.add("ENHANCED_DEFENDING_TIME", %i++);
    AIME_GlobalVariableSelector.add("ENHANCED_DEFENDING_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("DETECT_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("ATTENTION_RANGE", %i++);
    AIME_GlobalVariableSelector.add("MAX_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("MIN_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("LEASH_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("MAX_PACE", %i++);
    AIME_GlobalVariableSelector.add("MIN_PACE", %i++);
    AIME_GlobalVariableSelector.add("PACE_SPEED", %i++);
    AIME_GlobalVariableSelector.add("PACE_TIME", %i++);
    AIME_GlobalVariableSelector.add("SIDESTEP", %i++);
    AIME_GlobalVariableSelector.add("ACTIVE_DODGE", %i++);
    AIME_GlobalVariableSelector.add("LOS_TIME", %i++);
    AIME_GlobalVariableSelector.add("SCAN_TIME", %i++);
    AIME_GlobalVariableSelector.add("MAX_ATTENTION", %i++);
    AIME_GlobalVariableSelector.add("HOLDCNT_MAX", %i++);
    AIME_GlobalVariableSelector.add("LOOP_COUNTER", %i++);
    AIME_GlobalVariableSelector.add("CREATION_DELAY", %i++);
    AIME_GlobalVariableSelector.add("DEFAULT_RESPAWN", %i++);
    //Item gathering has been commented out because it does not work properly
    //AIME_GlobalVariableSelector.add("SEEK_HEALTH_LVL", %i++);
    //AIME_GlobalVariableSelector.add("DETECT_ITEM_RANGE", %i++);
    AIME_GlobalVariableSelector.add("HEALTH_IGNORE", %i++);
    AIME_GlobalVariableSelector.add("TEAM", %i++);
    AIME_GlobalVariableSelector.add("FREE_FOR_ALL", %i++);
    AIME_GlobalVariableSelector.add("SPAWN_GROUP", %i++);
    AIME_GlobalVariableSelector.add("CHAR_TYPE", %i++);
    AIME_GlobalVariableSelector.add("WEAPON", %i++);
    AIME_GlobalVariableSelector.add("WEAPON_USES_AMMO", %i++);
    AIME_GlobalVariableSelector.add("ENDLESS_AMMO", %i++);
    AIME_GlobalVariableSelector.add("IGNORE_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("CYCLE_COUNTER", %i++);
    AIME_GlobalVariableSelector.add("RAND_CYCLE_MAX", %i++);
    AIME_GlobalVariableSelector.add("WEAPON_MODE", %i++);
    AIME_GlobalVariableSelector.add("SHOW_NAME", %i++);
    AIME_GlobalVariableSelector.add("OBSTACLE", %i++);
    AIME_GlobalVariableSelector.add("CHAR_HEIGHT", %i++);
    AIME_GlobalVariableSelector.add("RESPAWN_DELAY", %i++);
    AIME_GlobalVariableSelector.add("FRIENDLY_FIRE", %i++);
    AIME_GlobalVariableSelector.add("REAL_NAME", %i++);
    AIME_GlobalVariableSelector.add("QUICK_THINK", %i++);
    AIME_GlobalVariableSelector.add("MIN_IGNORE_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("WEAPON_RATING", %i++);
    AIME_GlobalVariableSelector.add("BEHAVIOR", %i++);
    AIME_GlobalVariableSelector.add("STARTING_AMMO", %i++);
    AIME_GlobalVariableSelector.add("ASSIST_DISTANCE", %i++);
    AIME_GlobalVariableSelector.add("CAN_ASSIST", %i++);
    AIME_GlobalVariableSelector.add("PATH_SPEED", %i++);
    AIME_GlobalVariableSelector.add("RAND_DODGE_MAX", %i++);
    AIME_GlobalVariableSelector.add("RESPAWN_COUNT", %i++);
    AIME_GlobalVariableSelector.add("ADVANCED_DODGE", %i++);
    AIME_GlobalVariableSelector.add("NOT_AIM", %i++);
	AIME_GlobalVariableSelector.add("GROUP", %i++);

    //AFX compatibility globals
    AIME_GlobalVariableSelector.add("AFX_DATA_TYPE", %i++);
    AIME_GlobalVariableSelector.add("AFX_WAIT_TIME", %i++);

    AIME_GlobalVariableSelector.sort();
}


//-----------------------------------------------------------------------------
//Utility Functions
//-----------------------------------------------------------------------------

//This function applies any changes then respawn the bots
function AIMarkerEditor::saveAndRespawn(%this)
{
    %name = AIME_NameSelector.getText();

    if (AIME_SpawnGroupSelector.getText() !$= "")
        %spawnGroup = AIME_SpawnGroupSelector.getText();
    else
        %spawnGroup = "all";

    //Apply the marker settings
    AIMarkerEditor.saveEffect("edit");
    //Delete then spawn the entire group
    AIPlayer::actionByGroup(%spawnGroup, "delete");
    AIPlayer::actionByGroup(%spawnGroup, "spawn");
}

//This function deletes the currently selected marker
function AIMarkerEditor::deleteAiMarker(%this)
{
    AIMarkerEditor::editorDeselectObject();

	%data = AIME_MarkerSelector2.getText();

    if (isObject(%data))
        %data.delete();

    %selectSame2 = AIME_CharacterSelector2.getSelected();

	AIMarkerEditor.initEditor();
	AIMarkerEditor.updateControls();

    AIME_CharacterSelector2.setSelected(%selectSame2);
}

//This function gets the player or camera's postion and sets the marker to that position
function AIMarkerEditor::setAIPositioning()
{
    //Get what we're controling, whether it's a camera or player
    %tempHolder = LocalClientConnection.getControlObject();
    //Get that object's position
    %tempHolder = %tempHolder.getposition();
    //Set that position as the marker's position
    AIME_PositionSelector.text = %tempHolder;
}

//This function gets the player or camera's rotation and sets the marker to that rotation
function AIMarkerEditor::setAIRotate()
{
    %tempHolder = LocalClientConnection.getControlObject();
    %tempHolder = %tempHolder.getTransform();
    AIME_RotationSelector.text = getWords(%tempHolder, 3, 5) SPC mRadToDeg(getWord(%tempHolder, 6));
}

//This function renames all markers in order
function AIMarkerEditor::renameMarkers()
{
    AIMarkerEditor::editorDeselectObject();

    //Get the number of markers
    %n = allMarkersSet.getCount();

    for (%i = 0; %i < %n; %i++)
    {
        //Get the name of what we're dealing with now
        %obj = allMarkersSet.getObject(%i);

        //Rename the marker, giving it a 0 in front if needed
        if (%i < 9)
            %obj.setName("Marker0" @ %i + 1);
        else
            %obj.setName("Marker" @ %i + 1);
    }

    //Update all lists
    AIMarkerEditor.initEditor();
    AIMarkerEditor.updateControls();
}

//This function applies the selected action
function AIMarkerEditor::justDoIt()
{
    //Is the action by group or team
    if (AIME_ActionFunctionSelector.getText() $= "Spawn Group")
        AIPlayer::actionByGroup(AIME_ActionNumSelector.getText(), AIME_ActionTypeSelector.getText());
    else if (AIME_ActionFunctionSelector.getText() $= "Team")
        AIPlayer::actionByTeam(AIME_ActionNumSelector.getText(), AIME_ActionTypeSelector.getText());

	if (%data.botBelongsToMe !$= "")
        AIME_EditorText54.setText(%data.botBelongsToMe);
	else
		AIME_EditorText54.setText("-NONE");
}

//This function saves the mission file
function AIMarkerEditor::MissionFileSave()
{
    //Delete all bots so the marker's "botBelongsToMe" value isn't saved
    AIPlayer::actionByGroup("all", "delete");

    //Save the mission
    EWorldEditor.isDirty = false;
    $AISK_GROUP.save($Server::MissionFile);

    //Spawn all bots again if needed
    if ($AIME_SpawnAfterSave == 1)
        AIPlayer::actionByGroup("all", "spawn");
}

//Set the value of a global AI variable
function AIMarkerEditor::setGlobalsValue()
{
    %doAnEval = "$AISK_" @ AIME_GlobalVariableSelector.getText() SPC "= \"" @ AIME_VariableValueSelector.getText() @ "\";";
    eval(%doAnEval);
}

//Deselect whatever is selected
function AIMarkerEditor::deselectEverything()
{
    %text = AIME_PageSelector.getText();

    if (%text $= "" || %text $= "null")
        return;

	//Set all windows to not visible
    AIME_MarkerModeSelections.setVisible(false);
    AIME_MarkerApplyButtons.setVisible(false);
    AIME_PathSelectionDrop.setVisible(false);
	AIME_GeneralEditor.setVisible(false);
	AIME_PositioningEditor.setVisible(false);
	AIME_Actions.setVisible(false);
	AIME_PathManager.setVisible(false);
	AIME_PathEditor.setVisible(false);
    AIME_DistanceEditor.setVisible(false);
    AIME_WeaponEditor.setVisible(false);
    AIME_GlobalEditor.setVisible(false);
    AIME_SearchEditor.setVisible(false);
    AIME_CreatorEditor.setVisible(false);

    switch$(%text)
    {
        case "General":
            //Set the proper windows visible
            AIME_MarkerModeSelections.setVisible(true);
            AIME_MarkerApplyButtons.setVisible(true);
	        AIME_GeneralEditor.setVisible(true);
	        //Set the window's name
	        AIME_Window.setFieldValue("text", "AI Marker Editor - General Editor");

        case "Positioning":
            AIME_MarkerModeSelections.setVisible(true);
            AIME_MarkerApplyButtons.setVisible(true);
	        AIME_PositioningEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Positioning Editor");

        case "Actions":
	        AIME_Actions.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Actions");

        case "Search":
	        AIME_SearchEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Search");

        case "Globals":
	        AIME_GlobalEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Globals");

        case "Path Management":
            AIME_PathSelectionDrop.setVisible(true);
	        AIME_PathManager.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Path Manager");

        case "Path Editing":
            AIME_PathSelectionDrop.setVisible(true);
	        AIME_PathEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Path Editor");

        case "Weapons":
            AIME_MarkerModeSelections.setVisible(true);
            AIME_MarkerApplyButtons.setVisible(true);
	        AIME_WeaponEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Weapon Editor");

        case "Distances":
            AIME_MarkerModeSelections.setVisible(true);
            AIME_MarkerApplyButtons.setVisible(true);
	        AIME_DistanceEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Distance Editor");

        case "Creation":
	        AIME_CreatorEditor.setVisible(true);
	        AIME_Window.setFieldValue("text", "AI Marker Editor - Creation");
    }
}

//Unselect everything then select the correct object
function AIMarkerEditor::editorSelectObject(%data)
{
	//Check if the world editor is open
	if (isObject(EWorldEditor))
    {
	    if (EWorldEditor.isAwake())
        {
            %count = EWorldEditor.getSelectionSize();

            //Clear all selections
            for (%i = 0; %i < %count; %i++)
            {
                EWorldEditor.unselectObject(EWorldEditor.getSelectedObject(0));
            }

	        //Select the marker we're working with
	        EWorldEditor.selectObject(%data);
        }
    }
}

//Unselect everything
function AIMarkerEditor::editordeselectObject()
{
	//Check if the world editor is open
	if (isObject(EWorldEditor))
    {
	    if (EWorldEditor.isAwake())
        {
            %count = EWorldEditor.getSelectionSize();

            //Clear all selections
            for (%i = 0; %i < %count; %i++)
            {
                EWorldEditor.unselectObject(EWorldEditor.getSelectedObject(0));
            }
        }
    }
}

//Look through all markers and find the ones we want
function AIMarkerEditor::searchObjects()
{
    AIME_SearchList.clearItems();
    %variable = AIME_SearchVariable.getText();
    %sign = AIME_SearchSign.getText();
    %key = AIME_SearchKey.getText();
    %total = 0;
    %count = allMarkersSet.getCount();

    for (%i = 0; %i < %count; %i++)
    {
        %obj = allMarkersSet.getObject(%i);
        //Get the variable we want
        eval("%look = " @ %obj @ "." @ %variable @ ";");

        //If that was blank, try the value on the datablock
        if (%look $= "")
        {
            if (%obj.block !$= "")
                %block = %obj.block;
            else
                %block = $AISK_CHAR_TYPE;

            eval("%look = " @ %block @ "." @ %variable @ ";");
        }

        //If it's still blank, it should try the overall default at this point, but doing that
        //would be a lot of code since there's no pattern to the global variables
        //that make up those defaults, so instead we just continue
        if (%look $= "")
            continue;

        //Check how we should compare the values
        switch$(%sign)
        {
        case ">=":
            if (%look >= %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case ">":
            if (%look > %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "<=":
            if (%look <= %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "<":
            if (%look < %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "==":
            if (%look == %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "$=":
            if (%look $= %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "!=":
            if (%look != %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        case "!$=":
            if (%look !$= %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }

        default:
            if (%look $= %key)
            {
                AIME_SearchList.addItem(%obj.getName());
                %total++;
            }
        }
    }

    //Display the total number of results
    AIME_SearchTotal.setText(%total);
}


//-----------------------------------------------------------------------------
//Button Specific Functions
//-----------------------------------------------------------------------------

//This function displays the correct window settings
function AIME_PageSelector::onSelect(%this, %obj)
{
    AIMarkerEditor::deselectEverything();
}

//If the option is on, reset a marker's values if the datablock is changed.
function AIME_CharacterSelector::onSelect(%this, %obj)
{
    if ($AIME_ResetOnCharacterChange)
    {
        %data = AIME_MarkerSelector.getText();
        %charType = AIME_CharacterSelector.getText();

        //Check if this datablock is the global default or not
        if (isObject(%data) && %charType !$= $AISK_CHAR_TYPE)
            %data.block = %charType;
        else if (isObject(%data) && %charType $= $AISK_CHAR_TYPE)
            %data.block = "";
        //We're editing something that's not an object?
        else
            return;

        AIMarkerEditor::updateControls();
    }
}

//This function adds selected weapons to the weapons list
function AIME_WeaponSelector::onSelect(%this, %obj)
{
    //Get the selected weapon's name
    %text = AIME_WeaponSelector.getText();

    if (%text $= "" || %text $= "null")
        return;

    //Now we add that text to the weapons list
    AIME_WeaponsList.addItem(%text);
}

//This function deletes selected weapons from the weapons list
function AIME_WeaponsList::onSelect(%this, %obj)
{
    //Delete the selected item
    AIME_WeaponsList.deleteItem(%obj);
}

//This function globally selects the marker selected in this GUI
function AIME_SearchList::onSelect(%this, %obj)
{
    %text = AIME_SearchList.getItemText(%obj);
    %id = AIME_MarkerSelector.findText(%text);

    if (%id >= 0)
        AIME_MarkerSelector.setSelected(%id);
}

//This function shows or hides the markers
function AIME_HideMarkers::onAction(%this)
{
    $AISK_MARKER_HIDE = AIME_HideMarkers.getValue();

    hideMarkers();
}

//This function refreshes the variables
function AIME_MarkerSelector::onSelect(%this, %obj)
{
    %selected = AIME_MarkerSelector.getText();
    AIME_MarkerSelector2.setText(%selected);

    AIMarkerEditor::updateControls();
}

//This function refreshes the variables
function AIME_MarkerSelector2::onSelect(%this, %obj)
{
    %selected = AIME_MarkerSelector2.getText();
    AIME_MarkerSelector.setText(%selected);

    AIMarkerEditor::updateControls();
}

//This function shows or hides bot's names
function AIME_ShowNames::onSelect(%this, %obj)
{
    $AISK_SHOW_NAME = AIME_ShowNames.getText();
}

//Display the value of the selected variable
function AIME_GlobalVariableSelector::onSelect(%this, %obj)
{
    %DisplayValue = "AIME_VariableValueSelector.text = $AISK_" @ AIME_GlobalVariableSelector.getText() @ ";";
    eval(%DisplayValue);
}
