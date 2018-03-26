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


//Execute the other files in this folder
exec("./aiGlobals.cs");
exec("./aiBehaviors.cs");
//exec("./aiDatablocks.cs"); //UAISK+AFX Interop Change
exec("./aiFunctions.cs");
exec("./aiTraits.cs");
exec("./aiLoading.cs");
exec("./aiActions.cs");
exec("./aiSpawning.cs");
exec("./aiWeapons.cs");
exec("./aiThought.cs");
exec("./aiMovement.cs");
exec("./aiTargeting.cs");
exec("./aiPathed.cs");
exec("./aiGroups.cs");
exec("./aiNPC.cs");

//Editor tools
if (!isObject(AIMarkerEditor))
    exec("tools/worldEditor/gui/AIMarkerEditor.gui");

exec("tools/worldEditor/scripts/AIMarkerEditor.cs");
