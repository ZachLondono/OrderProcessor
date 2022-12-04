using ApplicationCore.Features.CNC.GCode;
using ApplicationCore.Shared;
using Dapper;

namespace ApplicationCore.Features.CNC.LabelDB;

internal class AvailableJobProvider : IAvailableJobProvider
{

    private readonly MachineNameProvider _machineNameProvider;
    private readonly IAccessDBConnectionFactory _connFactory;

    public AvailableJobProvider(MachineNameProvider machineNameProvider, IAccessDBConnectionFactory connFactory)
    {
        _machineNameProvider = machineNameProvider;
        _connFactory = connFactory;
    }

    public async Task<IEnumerable<AvailableJob>> GetAvailableJobsFromLabelFileAsync(string filePath)
    {

        using var connection = _connFactory.CreateConnection(filePath);

        var jobs = new List<AvailableJob>();
        try
        {

            var data = await connection.QueryAsync<(string name, DateTime created)>(@"SELECT [Job Name] AS Name, Created FROM Jobs;");

            foreach (var (name, created) in data)
            {
                var machineName = await _machineNameProvider.GetMachineNameAsync(filePath, name);
                var job = new AvailableJob(name, created, machineName);
                jobs.Add(job);
            }

        }
        catch
        {

            // TODO: log error

        }


        return jobs;

    }
}
