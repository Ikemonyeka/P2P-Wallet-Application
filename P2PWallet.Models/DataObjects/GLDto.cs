using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DataObjects
{
    public class GLDto
    {
        public class CreateGL
        {
            public string glName { get; set; } = string.Empty;
            public string glCurrency { get; set; } = string.Empty;
        }
    }
}
