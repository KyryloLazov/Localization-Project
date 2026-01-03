using Cysharp.Threading.Tasks;

public static class ConfigInitGate
{
    private static UniTaskCompletionSource<bool> _tcs = new();
    private static bool _isReady;

    public static bool IsReady => _isReady;

    public static UniTask WaitReady()
    {
        if (_isReady) return UniTask.CompletedTask;
        
        return _tcs.Task;
    }

    public static void MarkReady()
    {
        if (_isReady) return;
        _isReady = true;
        _tcs.TrySetResult(true);
    }
    
    public static void Reset()
    {
        _isReady = false;
        _tcs = new UniTaskCompletionSource<bool>();
    }
}