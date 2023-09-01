#region Using directives
using UAManagedCore;
using FTOptix.UI;
using FTOptix.NetLogic;
using System.Collections.Generic;
#endregion

public class SensibleAreaStatus : BaseNetLogic
{
    private Rectangle sensibleArea;
    private IUAVariable inUseVar;
    private IUANode sandBox;
    private (float TopLeftMarginY, float TopLeftMarginX) sandBoxTopLeft;
    private (float BottomRightMarginY, float BottomRightMarginX) sandBoxBottomRight;

    public override void Start()
    {
        sensibleArea = (Rectangle)Owner;
        inUseVar = sensibleArea.GetVariable("InUse");
        sandBox = sensibleArea.Owner;
        GetSensibleAresCoordinates();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void SensibleAreaHasOverlaps()
    {
        var movables = GetMovables(sandBox);
        foreach (var movable in movables)
        {
            if (IsMovableInsideSensibleArea(movable))
            {
                inUseVar.Value = true;
                return;
            }
        }
        inUseVar.Value = false;
    }

    private bool IsMovableInsideSensibleArea(movable movable)
    {
        var topLeftIntoArea = movable.TopMargin >= sandBoxTopLeft.TopLeftMarginY
            && movable.TopMargin <= sandBoxBottomRight.BottomRightMarginY
            && movable.LeftMargin >= sandBoxTopLeft.TopLeftMarginX
            && movable.LeftMargin <= sandBoxBottomRight.BottomRightMarginX;

        var bottomRightIntoArea = movable.TopMargin + movable.Height >= sandBoxTopLeft.TopLeftMarginY
            && movable.TopMargin + movable.Height <= sandBoxBottomRight.BottomRightMarginY
            && movable.LeftMargin + movable.Width >= sandBoxTopLeft.TopLeftMarginX
            && movable.LeftMargin + movable.Width <= sandBoxBottomRight.BottomRightMarginX;

        var bottomLeftIntoArea = movable.TopMargin + movable.Height >= sandBoxTopLeft.TopLeftMarginY
            && movable.TopMargin + movable.Height <= sandBoxBottomRight.BottomRightMarginY
            && movable.LeftMargin >= sandBoxTopLeft.TopLeftMarginX
            && movable.LeftMargin <= sandBoxBottomRight.BottomRightMarginX;

        var topRightIntoArea = movable.TopMargin >= sandBoxTopLeft.TopLeftMarginY
            && movable.TopMargin <= sandBoxBottomRight.BottomRightMarginY
            && movable.LeftMargin + movable.Width >= sandBoxTopLeft.TopLeftMarginX
            && movable.LeftMargin + movable.Width <= sandBoxBottomRight.BottomRightMarginX;

        return topLeftIntoArea || bottomRightIntoArea || bottomLeftIntoArea || topRightIntoArea;
    }

    private List<movable> GetMovables(IUANode sandBox) => GetMovablesIntoNode(sandBox);

    private void GetSensibleAresCoordinates()
    {
        sandBoxTopLeft = (sensibleArea.TopMargin, sensibleArea.LeftMargin);
        sandBoxBottomRight = (sensibleArea.TopMargin + sensibleArea.Height, sensibleArea.LeftMargin + sensibleArea.Width);
    }

    public List<movable> GetMovablesIntoNode(IUANode uANode)
    {
        var res = new List<movable>();
        try
        {
            foreach (var c in uANode.Children)
            {
                switch (c)
                {
                    case UAVariable _:
                        break;
                    case UAObject _:
                        if (!(((UAManagedCore.UANode)((UAManagedCore.UAObject)c).ObjectType).BrowseName == "movable")) continue;
                        res.Add(c as movable);
                        break;
                    default:
                        res.AddRange(GetMovablesIntoNode(c));
                        break;
                }
            }

            return res;
        }
        catch (System.Exception ex)
        {
            Log.Error(ex.Message);
            return null;
        }
    }
}
