using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Westwind.Utilities.Logging;


/// <summary>
/// A null logger implementation that doesn't do anything.
/// </summary>
public class NullLogAdapter : NullLogAdapter<LogEntry>
{
}

/// <summary>
/// A null logger implementation that doesn't do anything.
/// </summary>
/// <typeparam name="T">A LogEntry derived type</typeparam>
public class NullLogAdapter<T> : ILogAdapter<T> where T : LogEntry, new()
{
    public string ConnectionString { get; set; } 

    public string Filename { get; set; }

    public bool WriteEntry(T entry)
    {
        return true;
    }

    public Task<bool> WriteEntryAsync(T logEntry)
    {
        return Task.FromResult(true);
    }

    public T GetEntry(string id)
    {
        return null;
    }
    public IEnumerable<T> GetEntries(ErrorLevels errorLevel, int count, DateTime? dateFrom, DateTime? dateTo, string fieldList)
    {
        return new List<T>();
    }
    public bool CreateLog()
    {
        return true;
    }
    public bool DeleteLog()
    {
        return true;
    }
    public bool Clear()
    {
        return true;
    }
    public bool Clear(int countToLeave)
    {
        return true;
    }
    public bool Clear(decimal daysToDelete)
    {
        return true;
    }
    public int GetEntryCount(ErrorLevels errorLevel = ErrorLevels.All)
    {
        return 0;
    }
}