using Cysharp.Threading.Tasks;

public interface IRemoteTextProvider
{
    UniTask<string> Fetch(string url);
}