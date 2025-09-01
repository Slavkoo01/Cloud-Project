using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IServiceHealthCheck
    {
        [OperationContract]
        void HealthCheck(); // void jer ako ne pogodi ovaj endpoint nece ni vratiti pov vr, ide u catch blok 
    }
}
