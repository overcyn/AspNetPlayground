using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aspnetplayground.Models
{
    public class Pokemon
    {
        public Pokemon(int id, string name, string sprite)
        {
            Id = id;
            Name = name;
            Sprites = new SpriteList { front_default = sprite };
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public SpriteList Sprites { get; set; }
        
        public class SpriteList
        {
            public string front_default;
        }
    }
}