using System.Collections.Concurrent;

namespace Aspire.Hosting.AWS.LambdaServiceEmulator.Services;

public interface ILambdaRuntimeDataStoreManager
{
    ILambdaRuntimeDataStore GetLambdaRuntimeDataStore(string functionName);

    string[] GetListOfFunctionNames();
}

internal class LambdaRuntimeDataStoreManager : ILambdaRuntimeDataStoreManager
{
    ConcurrentDictionary<string, ILambdaRuntimeDataStore> _dataStores = new ConcurrentDictionary<string, ILambdaRuntimeDataStore>();

    public ILambdaRuntimeDataStore GetLambdaRuntimeDataStore(string functionName)
    {
        return _dataStores.GetOrAdd(functionName, name => new LambdaRuntimeDataStore());
    }

    public string[] GetListOfFunctionNames()
    {
        return _dataStores.Keys.ToArray();
    }
}