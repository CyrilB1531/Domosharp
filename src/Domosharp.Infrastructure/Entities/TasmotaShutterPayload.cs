using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Infrastructure.Entities
{
  internal record TasmotaShutterPayload
  {
    public int Position { get; set; }
    public int Direction { get; set; }
    public int Target {  get; set; }
    public int Tilt {  get; set; }
  }
}
