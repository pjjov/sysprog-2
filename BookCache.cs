using System.Threading;
using Newtonsoft.Json.Linq;

namespace SysProg;

class BookCache
{
    private ReaderWriterLockSlim rwLock;
    private Dictionary<string, JObject> responses;
    private int max;
    public BookCache(int max)
    {
        this.max = max;
        this.responses = new Dictionary<string, JObject>();
        this.rwLock = new ReaderWriterLockSlim();
    }

    public JObject? Find(string query)
    {
        JObject? result = null;
        rwLock.EnterReadLock();

        try
        {
            responses.TryGetValue(query, out result);
        }
        finally
        {
            rwLock.ExitReadLock();
        }

        return result;
    }

    public void Insert(string query, JObject result)
    {
        rwLock.EnterUpgradeableReadLock();
        try
        {
            if (!responses.ContainsKey(query))
            {
                rwLock.EnterWriteLock();
                try
                { 
                    if (responses.Count == max)
                        responses.Remove(responses.First().Key);
                    responses.Add(query, result);
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    throw;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
                if (responses.Count == max)
                {
                    responses.Remove(responses.First().Key);
                }
            }
        }
        finally
        {
            rwLock.ExitUpgradeableReadLock();
        }
    }
}