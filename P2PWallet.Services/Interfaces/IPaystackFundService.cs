using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IPaystackFundService
    {
        Task<ActionResult<object>> InitializePaystack(decimal fundPaystack);
        Task<ActionResult> Webhooks(object obj);
    }
}
