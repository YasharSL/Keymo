namespace Keymo.Tests;

public class InputDeferralTests
{
    [Fact]
    public void Input_injected_inside_a_hook_callback_is_queued_not_sent()
    {
        int queued = QueuedWhile(() =>
        {
            Input.EnterHookCallback();
            try
            {
                Input.TapKey(Input.AltMenuMaskKey);
            }
            finally
            {
                Input.LeaveHookCallback();
            }
        });

        Assert.Equal(1, queued);
    }

    [Fact]
    public void Nested_hook_callbacks_keep_queueing_until_the_outermost_returns()
    {
        int queued = QueuedWhile(() =>
        {
            Input.EnterHookCallback();
            Input.EnterHookCallback();
            Input.LeaveHookCallback();
            try
            {
                Input.TapKey(Input.AltMenuMaskKey);
            }
            finally
            {
                Input.LeaveHookCallback();
            }
        });

        Assert.Equal(1, queued);
    }

    // Runs the action with a stand-in message loop and returns how many injections were queued on it.
    // The stand-in is only current during the action, so the test runner's own work is not swallowed by it.
    private static int QueuedWhile(Action action)
    {
        SynchronizationContext? previous = SynchronizationContext.Current;
        var messageLoop = new RecordingContext();
        SynchronizationContext.SetSynchronizationContext(messageLoop);
        try
        {
            action();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }

        return messageLoop.Queued;
    }

    /// <summary>Counts what is posted and never runs it, so the test injects nothing into the desktop.</summary>
    private sealed class RecordingContext : SynchronizationContext
    {
        public int Queued { get; private set; }

        public override void Post(SendOrPostCallback d, object? state) => Queued++;
    }
}
