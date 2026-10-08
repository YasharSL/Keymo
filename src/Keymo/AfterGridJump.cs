namespace Keymo;

/// <summary>What happens right after the grid has moved the cursor to the picked cell.</summary>
internal enum AfterGridJump
{
    TurnOnCursorMode,
    Click,
    Nothing,
}
