using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glass.Mapper.Sc.Fields
{
    public class AdvanceImageField : Glass.Mapper.Sc.Fields.Image
    {
        public float CropX { get; set; }
        public float CropY { get; set; }
        public float FocusX { get; set; }
        public float FocusY { get; set; }
    }
}