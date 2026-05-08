using System.Threading;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;
using Superpower.Model;

namespace SysProg;

class BookCache
{
    private struct Response
    {
        public ManualResetEvent wait;
        public JObject? result;

        public Response()
        {
            wait = new ManualResetEvent(false);
            result = null;
        }
    }

    private ReaderWriterLockSlim rwLock;
    private Dictionary<string, Response> responses;
    private int max;
    private int count;
    private long hits;
    private long misses;

    public BookCache(int max)
    {
        this.max = max;
        responses = new Dictionary<string, Response>();
        rwLock = new ReaderWriterLockSlim();
        count = 0;
        hits = 0;
        misses = 0;
    }

    public List<KeyValuePair<string, JObject>> Snapshot()
    {
        List<KeyValuePair<string, JObject>> result;
        
        rwLock.EnterReadLock();
        try
        { 
            result = responses
                .Select(p => new KeyValuePair<string, JObject>(p.Key, p.Value.result!))
                .Where(p => p.Value != null)
                .ToList();
        }
        catch (Exception e)
        {
            Console.Write(e.ToString());
            throw;
        }
        finally
        {
            rwLock.ExitReadLock();
        }

        return result;
    }

    public (long hits, long misses) Statistics()
    {
        return (Interlocked.Read(ref hits), Interlocked.Read(ref misses));
    }

    private JObject? WaitPending(Response response)
    {
        JObject? result;
        response.wait.WaitOne();

        rwLock.EnterUpgradeableReadLock();
        try
        {
            result = response.result;
        }
        finally
        {
            rwLock.ExitUpgradeableReadLock();
        }

        return result;
    }

    public JObject? Find(string query)
    {
        JObject? result = null;
        Response response;
        bool missed = false;
        bool pending = false;

        rwLock.EnterUpgradeableReadLock();
        try
        {
            if (!responses.TryGetValue(query, out response))
            {
                rwLock.EnterWriteLock();
                try
                { 
                    if (!responses.TryGetValue(query, out response))
                    {
                        responses.Add(query, new Response());
                        missed = true;
                    }
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
            }

            if (!missed)
            {
                result = response.result;
                if (response.result == null)
                    pending = true;
            }
        }
        finally
        {
            rwLock.ExitUpgradeableReadLock();
        }

        if (missed)
        {
            Interlocked.Increment(ref misses);
            return null;
        }
        
        Interlocked.Increment(ref hits);   

        if (pending)
            result = WaitPending(response);
        return result;
    }

    public void Insert(string query, JObject result)
    {
        rwLock.EnterWriteLock();
        try
        { 
            if (count == max)
            {
                var toRemove = responses.FirstOrDefault(p => p.Value.result != null);
                if (!string.IsNullOrEmpty(toRemove.Key))
                    responses.Remove(toRemove.Key);
            }
            else
                Interlocked.Increment(ref count);

            var r = responses[query];
            r.result = result;
            r.wait.Set();
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
    }

    public void Abort(string query)
    {
        rwLock.EnterWriteLock();
        try
        { 
            var r = responses[query];
            r.result = null;
            r.wait.Set();
            responses.Remove(query);
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
    }
}