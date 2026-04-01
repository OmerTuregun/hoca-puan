using System.Collections.Concurrent;
using HocaPuan.Core.DTOs.Import;
using HocaPuan.Core.Models;

namespace HocaPuan.Services;

public class ImportJobStore
{
    private readonly ConcurrentDictionary<string, ImportJob> _jobs = new();

    public ImportJob Create()
    {
        var j = new ImportJob();
        _jobs[j.Id] = j;
        return j;
    }

    public ImportJob? Get(string id) =>
        _jobs.TryGetValue(id, out var job) ? job : null;

    public void Complete(string id, YokBulkImportResponseDto result)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        job.Status = "done";
        job.Result = result;
        job.FinishedAt = DateTime.UtcNow;
    }

    public void Fail(string id, string error)
    {
        if (!_jobs.TryGetValue(id, out var job)) return;
        job.Status = "failed";
        job.Error = error;
        job.FinishedAt = DateTime.UtcNow;
    }
}
