using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using aspnetplayground.Models;
using Microsoft.VisualBasic.FileIO;

namespace aspnetplayground.Controllers
{
    public class PokeapiController : Controller
    {
        public ActionResult Sprite(int Id)
        {
            var path = HostingEnvironment.MapPath(@"~/App_Data/PokemonImages/" + Id.ToString() + ".png");
            if (!System.IO.File.Exists(path)) {
                return HttpNotFound();
            }
            return File(path, @"image/png");
        }
        
        [AddCustomHeaderFilter]
        public ActionResult Pokemon(int Id)
        {
            Pokemon pokemon;
            if (!Pokedex.TryGetValue(Id, out pokemon))
            {
                return HttpNotFound("No such Pokemon");
            }
            var pokemonTypes = PokemonTypes[Id];
            var types = new List<PokemonTypeRef>{};
            foreach (var i in pokemonTypes)
            {
                var t = Types[i.type_id];
                var typeRef = new TypeRef { name = t.identifier };
                var pokemonTypeRef = new PokemonTypeRef {
                    slot = i.slot,
                    type = typeRef
                };
                types.Add(pokemonTypeRef);
            }
            
            var req = System.Web.HttpContext.Current.Request;
            var baseUrl = string.Format("{0}://{1}{2}", req.Url.Scheme, req.Url.Authority, Url.Content("~"));
            var pokemonRef = new PokemonRef
            {
                id = Id,
                name = pokemon.name,
                sprites = new PokemonRef.SpriteList
                {
                    front_default = baseUrl + @"Pokeapi/Sprite/" + Id.ToString(),
                },
                types = types,
            };
            
            return Json(pokemonRef, JsonRequestBehavior.AllowGet);
        }

        static Dictionary<int, Pokemon> Pokedex { set; get; }
        static Dictionary<int, Models.Type> Types { set; get; }
        static Dictionary<int, List<PokemonType>> PokemonTypes { set; get; }
        
        static PokeapiController()
        {
            // ScrapePokemonImages();
            
            Types = new Dictionary<int, Models.Type>{};
            ParseCSV(HostingEnvironment.MapPath(@"~/App_Data/Types.csv"), fields =>
            {
                var id = Int32.Parse(fields[0]);
                Types[id] = new Models.Type
                {
                    id = id,
                    identifier = fields[1],
                    generation_id = Int32.Parse(fields[2]),
                    damage_class_id = Int32.Parse(fields[3])
                };
            });
            
            PokemonTypes = new Dictionary<int, List<PokemonType>>{};
            ParseCSV(HostingEnvironment.MapPath(@"~/App_Data/PokemonTypes.csv"), fields =>
            {
                var id = Int32.Parse(fields[0]);
                if (!PokemonTypes.ContainsKey(id))
                {
                    PokemonTypes[id] = new List<PokemonType>{};
                }
                
                PokemonTypes[id].Add(new PokemonType
                {
                    pokemon_id = id,
                    type_id = Int32.Parse(fields[1]),
                    slot = Int32.Parse(fields[2])
                });
            });
            
            Pokedex = new Dictionary<int, Pokemon>{};
            ParseCSV(HostingEnvironment.MapPath(@"~/App_Data/Pokemon.csv"), fields =>
            {
                var id = Int32.Parse(fields[0]);
                Pokedex[id] = new Pokemon
                {
                    id = id,
                    name = fields[1],
                };
            });
        }
        
        delegate void Parser(string[] fields);
        
        static void ParseCSV(string path, Parser p) {
            var parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.ReadFields(); // skip first line
            while (!parser.EndOfData) 
            {
                string[] fields = parser.ReadFields();
                p(fields);
            }
        }
        
        static void ScrapePokemonImages()
        {
            var webClient = new WebClient();
            var csvPath = HostingEnvironment.MapPath(@"~/App_Data/Pokemon.csv");
            var parser = new TextFieldParser(csvPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.ReadFields(); // skip first line
            while (!parser.EndOfData) 
            {
                string[] fields = parser.ReadFields();
                var id = Int32.Parse(fields[0]);
                if (id < 781)
                {
                    continue;
                }
                var name = fields[1];
                var bulbapediaIcon = fields[2];
                var imageUrl = ScrapePokemonImageUrl(id, bulbapediaIcon != "" ? bulbapediaIcon : name);
                
                var imagePath = HostingEnvironment.MapPath(@"~/App_Data/PokemonImages/"+id.ToString()+".png");
                System.Diagnostics.Debug.WriteLine($"Image Path: {imageUrl} {imagePath}");
                webClient.DownloadFile(imageUrl, imagePath);
            }
        }
        
        static string ScrapePokemonImageUrl(int id, string name) 
        {
            try
            {
                // Generate bulbapedia URL
                var idString = id.ToString("000");
                name = name.Replace(" ", "_");
                var url = String.Format("https://bulbapedia.bulbagarden.net/wiki/File:{0}{1}.png", idString, name);
                
                // Fetch Html
                var web = new HtmlWeb();
                var doc = web.Load(url);
                
                // Get image url
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='fullImageLink']/a");
                var imageUrl = node.Attributes["href"].Value;
                imageUrl = imageUrl.TrimStart('/');
                imageUrl = @"https://" + imageUrl;
                return imageUrl;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return "";
            }
        }
    }
}