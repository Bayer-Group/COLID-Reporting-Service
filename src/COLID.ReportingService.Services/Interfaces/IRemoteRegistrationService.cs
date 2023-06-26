using System.Collections.Generic;
using System.Threading.Tasks;
using COLID.Graph.TripleStore.DataModels.ConsumerGroups;

namespace COLID.ReportingService.Services.Interfaces
{
    public interface IRemoteRegistrationService
    {
        Task<IEnumerable<ConsumerGroupResultDTO>> GetConsumerGroups();
    }
}
