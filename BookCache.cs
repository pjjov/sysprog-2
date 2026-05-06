using System.Threading;
using Newtonsoft.Json.Linq;

namespace SysProg;

class BookCache
{
    private ReaderWriterLockSlim rwLock;
    private Dictionary<string, JObject> responses;
    private int max;
    private long hits;
    private long misses;
    public BookCache(int max)
    {
        this.max = max;
        this.responses = new Dictionary<string, JObject>();
        this.rwLock = new ReaderWriterLockSlim();
        hits = 0;
        misses = 0;
    }

    public List<KeyValuePair<string, JObject>> Snapshot()
    {
        return responses.ToList();
    }

    public (long hits, long misses) Statistics()
    {
        return (Interlocked.Read(ref hits), Interlocked.Read(ref misses));
    }

    public JObject? Find(string query)
    {
        JObject? result = null;
        rwLock.EnterReadLock();

        try
        {
            if (responses.TryGetValue(query, out result))
                hits++;
            else
                misses++;
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