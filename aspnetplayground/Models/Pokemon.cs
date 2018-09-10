using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aspnetplayground.Models
{
    public class Pokemon
    {
        public int id { get; set; }
        public string name { get; set; }
        public SpriteList sprites { get; set; }
        public List<PokemonTypeRef> types { get; set; }
        
        public class SpriteList
        {
            public string front_default;
        }
    }
}