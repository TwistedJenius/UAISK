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


//This function updates all controls in the editor to be equal to that of the selected path
function AIMarkerEditor::updatePathControls(%this)
{
	%data = AIME_PathEditorSelector.getText();

    if (!isObject(%data))
        return;

	if (%data.getName() !$= "")
		AIME_PathNameSelector.text = %data.getName();
	else
    {
        if ($pathMarkerCount < 10)
           AIME_PathNameSelector.text = "Path0" @ $pathMarkerCount;
        else
           AIME_PathNameSelector.text = "Path" @ $pathMarkerCount;
    }

	if (%data.getGroup().getName() !$= "")
		AIME_SimGroupSelector2.setText(getField(%data.getGroup().getName(), 0));
	else
		AIME_SimGroupSelector2.setText($AISK_GROUP);

	if (%data.isLooping $= "1")
  		AIME_LoopingSelector.setValue("1");
	else
  		AIME_LoopingSelector.setValue("0");

    AIMarkerEditor::editorSelectObject(%data);

	echo("Selected Path: " @ %data);
}

//This function applies any changes made to the path
function AIMarkerEditor::savePathEffect(%this, %mode)
{
    AIMarkerEditor::editorDeselectObject();

	%data = AIME_PathEditorSelector.getText();

    //Set the path's values if we're editing
    if (%mode $= "edit")
    {
	    if (AIME_PathNameSelector.getText() $= "")
        {
            if ($pathMarkerCount < 10)
               AIME_PathNameSelector.text = "Path0" @ $pathMarkerCount;
            else
               AIME_PathNameSelector.text = "Path" @ $pathMarkerCount;
        }

        %obj = %data.getid();

        %obj.setName(AIME_PathNameSelector.getText());
        %obj.isLooping = AIME_LoopingSelector.getValue();

	    //Put it in the correct simgroup
	    %simGroupSet = AIME_SimGroupSelector2.getText();

        if (getField(%obj.getGroup().getName(), 0) !$= %simGroupSet)
	        %simGroupSet.add(%obj);

        //Update all lists
        AIMarkerEditor.initEditor();
        AIMarkerEditor.updateControls();

        %order = AIME_PathEditorSelector.findText(%obj.getName());

        //Should the next path we select be above or below the last one
        if ($AIME_SelectNextMarker == 0)
        {
            if (%order < $pathMarkerCount - 2)
                %order++;
            else
                %order = 0;
        }
        else if ($AIME_SelectNextMarker == 1)
        {
            if (%order > 0)
                %order--;
            else
                %order = $pathMarkerCount - 2;
        }

        AIME_PathEditorSelector.setText(AIME_PathEditorSelector.getTextById(%order));
        AIME_PathEditorSelector2.setText(AIME_PathEditorSelector2.getTextById(%order));

        AIMarkerEditor.updatePathControls();
        AIMarkerEditor.populateNodelist();
    }
    //Make a new path
    else
    {
        if ($pathMarkerCount < 10)
           %name = "Path0" @ $pathMarkerCount;
        else
           %name = "Path" @ $pathMarkerCount;

        if (isObject(%name))
            %name = "Path" @ getRandom(1, 999);

        //Setup the objects data
        %marker = new Path(%name) {
            canSaveDynamicFields = "1";
            isLooping = "1";
        };

	    //Save it in the correct simgroup
	    %simGroupSet = AIME_SimGroupSelector2.getText();
	    %simGroupSet.add(%marker);

        //Update all lists
        AIMarkerEditor.initEditor();
        AIMarkerEditor.updateControls();

        AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%name));
    }
}

//Populate the nodes list based on the selected path
function AIMarkerEditor::populateNodelist(%this)
{
    AIME_NodeSelector.clear();
    AIME_NodeSelector2.clear();

    %path = AIME_PathEditorSelector.getText();

    if (!isObject(%path))
        return;

    %count = %path.getCount();

    //Cycle through all nodes on this path and add them to the node list
    for (%i = 0; %i < %count; %i++)
    {
        %nodeName = %path.getObject(%i).getName();
        AIME_NodeSelector.add(%nodeName, %i);
        AIME_NodeSelector2.add(%nodeName, %i);
    }

    AIME_NodeSelector.setSelected(0);
    AIME_NodeSelector2.setSelected(0);
    AIMarkerEditor.updateNodeControls();
}

//This function updates all controls in the editor to be equal to that of the selected node
function AIMarkerEditor::updateNodeControls(%this, %selectorId)
{
    if (%selectorId)
	    %data = AIME_NodeSelector2.getText();
    else
	    %data = AIME_NodeSelector.getText();

	if (!isObject(%data))
        return;

	if (%data.getName() !$= "")
		AIME_NodeNameSelector.text = %data.getName();
	else
		AIME_NodeNameSelector.text = "Node" @ getRandom(1, 9999);

	if (%data.position !$= "")
		AIME_PositionSelector2.text = %data.getFieldValue(position);
	else
		AIME_PositionSelector2.text = "0 0 0";

	AIME_SeqNumSelector.setText(getField(%data.seqNum, 0));
	AIME_MsToNextSelector.setText(getField(%data.msToNext, 0));

    AIMarkerEditor::editorSelectObject(%data);

	echo("Selected Node: " @ %data);
}

//This function applies any changes made to the node
function AIMarkerEditor::saveNodeEffect(%this, %mode)
{
    %path = AIME_PathEditorSelector.getText();

    AIMarkerEditor::editorDeselectObject();

	//Make sure the path has a name
	if (AIME_NodeNameSelector.getText() $= "")
		AIME_NodeNameSelector.text = "Node" @ getRandom(1, 9999);

	%data = AIME_NodeSelector.getText();
    %data3 = AIME_NodeNameSelector.getText();

    //Set the node's values if we're editing
    if (%mode $= "edit")
    {
        %obj = %data.getid();

        %obj.setName(%data3);
        %obj.position = AIME_PositionSelector2.getText();
        %obj.seqNum = AIME_SeqNumSelector.getText();
        %obj.msToNext = AIME_MsToNextSelector.getText();

        //Update all lists
        AIMarkerEditor.initEditor();
        AIMarkerEditor.updateControls();
        AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%path));

        %order = AIME_NodeSelector.findText(%obj.getName());

        //Should the next node we select be above or below the last one
        if ($AIME_SelectNextMarker == 0)
        {
            if (%order < %path.getCount() - 1)
                %order++;
            else
                %order = 0;
        }
        else if ($AIME_SelectNextMarker == 1)
        {
            if (%order > 0)
                %order--;
            else
                %order = %path.getCount() - 1;
        }

        AIME_NodeSelector.setText(AIME_NodeSelector.getTextById(%order));
        AIMarkerEditor.updateNodeControls();
    }
    //Make a new node
    else
    {
        //If a path hasn't been made yet, make one first
        if (!isObject(%path))
        {
            AIMarkerEditor.savePathEffect("create");
            AIMarkerEditor.saveNodeEffect("create");
            return;
        }

        if (isObject(%data3))
            AIME_NodeNameSelector.text = "Node" @ getRandom(1, 9999);

        AIMarkerEditor.setNodePositioning();

        //Setup the objects data
        %marker = new Marker(AIME_NodeNameSelector.getText()) {
            canSaveDynamicFields = "1";
            rotation = "1 0 0 0";
            scale = "1 1 1";
            type = "Normal";
            smoothingType = "Linear";
            position = AIME_PositionSelector2.getText();
            seqNum = "1";
            msToNext = "0";
        };

	    //Save it in the correct path
	    %simGroupSet = AIME_PathEditorSelector.getText();
	    %simGroupSet.add(%marker);

        //Update all lists
        AIMarkerEditor.initEditor();
        AIMarkerEditor.updateControls();

        AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%path));
    }
}

//This function gets the player or camera's postion and sets the node to that position
function AIMarkerEditor::setNodePositioning()
{
    //Get what we're controling, whether it's a camera or player
    %tempHolder = LocalClientConnection.getControlObject();
    //Get that object's position
    %tempHolder = %tempHolder.getposition();
    //Set that position as the marker's position
    AIME_PositionSelector2.text = %tempHolder;
}

//This function renames all paths and nodes in order
function AIMarkerEditor::renamePaths()
{
    AIMarkerEditor::editorDeselectObject();
    %nodeNameCount = 0;

    for (%i = 0; %i < $pathMarkerCount - 1; %i++)
    {
        //Get the name of what we're dealing with now
        %path = AIME_PathEditorSelector.getTextById(%i);
        %count = %path.getCount();

        for (%j = 0; %j < %count; %j++)
        {
            %node = %path.getObject(%j);
            %nodeNameCount++;

            //Rename the node, giving it a 0 in front if needed
            if (%nodeNameCount < 10)
                %node.setName("Node0" @ %nodeNameCount);
            else
                %node.setName("Node" @ %nodeNameCount);
        }

        //Rename the path, giving it a 0 in front if needed
        if (%i < 9)
            %path.setName("Path0" @ %i + 1);
        else
            %path.setName("Path" @ %i + 1);
    }

    //Update all lists
    AIMarkerEditor.initEditor();
    AIMarkerEditor.updateControls();
}

//This function deletes the currently selected marker
function AIMarkerEditor::deleteAiPath(%this)
{
    AIMarkerEditor::editorDeselectObject();

	%data = AIME_PathEditorSelector2.getText();

    if (isObject(%data))
        %data.delete();

	AIMarkerEditor.initEditor();
}

//This function deletes the currently selected marker
function AIMarkerEditor::deleteAiNode(%this)
{
    AIMarkerEditor::editorDeselectObject();

    %path = AIME_PathEditorSelector2.getText();
	%data = AIME_NodeSelector2.getText();

    if (isObject(%data))
        %data.delete();

	AIMarkerEditor.initEditor();
    AIME_PathEditorSelector.setText(%path);
    AIME_PathEditorSelector2.setText(%path);
}

//Make a new path and copy the old nodes to it
function clonePathWithOffset()
{
    %path = AIME_PathEditorSelector.getText();

    //Make a new path
    AIMarkerEditor.savePathEffect();
    %pathNew = AIME_PathEditorSelector.getText();
    %pathNew.isLooping = %path.isLooping;

    if (getField(%path.getGroup().getName(), 0) !$= $AISK_GROUP)
        %path.getGroup().add(%pathNew);

    %count = %path.getCount();

    //Copy all the old node settings to the new path
    for (%j = 0; %j < %count; %j++)
    {
        %node = %path.getObject(%j);

        %name = "Node" @ getRandom(1, 9999);

        if (isObject(%name))
            %name = "Node" @ getRandom(1, 9999);

        %marker = new Marker(%name) {
            canSaveDynamicFields = "1";
            rotation = "1 0 0 0";
            scale = "1 1 1";
            type = "Normal";
            smoothingType = "Linear";
            position = %node.getPosition();
            seqNum = %node.seqNum;
            msToNext = %node.msToNext;
        };

	    %pathNew.add(%marker);
    }

    //Apply an offset
    AIMEMoveWholePath(%pathNew);

    //Update all lists
    AIMarkerEditor.initEditor();
    AIMarkerEditor.updateControls();

    AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%pathNew));
}

//Move each node on the path a certain amount
function AIMEMoveWholePath(%path)
{
    if (%path $= "")
        %path = AIME_PathEditorSelector.getText();

    %offset = AIME_PathOffsetSelector.getText();

    moveWholePath(%path, %offset);
}

//Reverse the direction that the bot will travel the path in
function AIMEReversePathOrder()
{
    %path = AIME_PathEditorSelector.getText();

    reversePathOrder(%path);
}

//Flipping is really just rescaling by -200%
function flipThePath()
{
    %flip = AIME_PathFlipSelector.getText();

    switch$(%flip)
    {
        case "X":
            %scale = "-200 0 0";
        case "Y":
            %scale = "0 -200 0";
        case "Z":
            %scale = "0 0 -200";
        case "All":
            %scale = "-200 -200 -200";
    }

    AIMERescalePath(%scale);
}

//Change the scale of a path by moving each node a % to/from the center
function AIMERescalePath(%scale)
{
    %path = AIME_PathEditorSelector.getText();

    if (%scale $= "")
        %scale = AIME_PathScaleSelector.getText();

    rescalePath(%path, %scale);
}

//Merge all nodes from two different paths
function mergePathNodes(%path1, %path2)
{
    //Have this as an if statement so it could be called from script if needed
    if (%path1 $= "")
        %path1 = AIME_PathEditorSelector.getText();

    if (%path2 $= "")
        %path2 = AIME_PathEditorSelector3.getText();

    if (!isObject(%path1))
        return;

    if (%path1 $= %path2)
        return;

    %count1 = %path1.getCount();
    %count2 = %path2.getCount();

    //Change around each node's seqNum then move it to its new path
    for (%j = 0; %j < %count1; %j++)
    {
        %node = %path1.getObject(0);
        %node.seqNum = %count2 + %j;
        %path2.add(%node);
    }

    %path1.delete();

    //Update all lists
    AIMarkerEditor.initEditor();
    AIMarkerEditor.updateControls();

    AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%path2));
}

//Take nodes off a current path and add them to a new path
function splitPathNodes(%path, %split)
{
    //Have this as an if statement so it could be called from script if needed
    if (%path $= "")
        %path = AIME_PathEditorSelector.getText();

    if (%split $= "")
        %split = AIME_PathSplitSelector.getText();

    %count = %path.getCount();

    if (!isObject(%path) || %split < 1 || %split > %count)
        return;

    if ($pathMarkerCount < 10)
       %name = "Path0" @ $pathMarkerCount;
    else
       %name = "Path" @ $pathMarkerCount;

    if (isObject(%name))
        %name = "Path" @ getRandom(1, 999);

    %pathNew = new Path(%name) {
        canSaveDynamicFields = "1";
        isLooping = %path.isLooping;
    };

    %path.getGroup().add(%pathNew);

    //Change around each node's seqNum then move it to its new path
    for (%j = %split; %j < %count; %j++)
    {
        %node = %path.getObject(%split);
        %node.seqNum = %j - (%split - 1);
        %pathNew.add(%node);
    }

    //Update all lists
    AIMarkerEditor.initEditor();
    AIMarkerEditor.updateControls();

    AIME_PathEditorSelector.setSelected(AIME_PathEditorSelector.findText(%name));
}

//This function syncs both path selectors
function AIME_PathEditorSelector::onSelect(%this, %obj)
{
    %selected = AIME_PathEditorSelector.getText();
    AIME_PathEditorSelector2.setText(%selected);

    AIMarkerEditor.updatePathControls();
    AIMarkerEditor.populateNodelist();

    AIME_EditorText99.setText(%selected.getCount());
}

//This function syncs both path selectors
function AIME_PathEditorSelector2::onSelect(%this, %obj)
{
    %selected = AIME_PathEditorSelector2.getText();
    AIME_PathEditorSelector.setText(%selected);

    AIMarkerEditor.updatePathControls();
    AIMarkerEditor.populateNodelist();

    AIME_EditorText99.setText(%selected.getCount());
}
