using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aspnetplayground.Models
{
    public class Type
    {
        public int id { get; set; }
        public string identifier { get; set; }
        public int generation_id { get; set; }
        public int damage_class_id { get; set; }
    }
}