using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Render
{
    public abstract class RendererDataBase
    {
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
