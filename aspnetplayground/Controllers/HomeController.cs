using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Text;
using HtmlAgilityPack;
using aspnetplayground.Models;
using Microsoft.VisualBasic.FileIO;

namespace aspnetplayground.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        
        public ActionResult Pokemon(int Id)
        {
            return Json(Pokedex[Id], JsonRequestBehavior.AllowGet);
        }

        static Dictionary<int, Pokemon> Pokedex { set; get; }
        static Dictionary<int, Models.Type> Types { set; get; }
        static Dictionary<int, List<PokemonType>> PokemonTypes { set; get; }
        
        static HomeController()
        {
            // ScrapePokemonImages();
            
            Pokedex = new Dictionary<int, Pokemon>{};
            ParseCSV(HostingEnvironment.MapPath(@"~/App_Data/PokemonImages.csv"), fields =>
            {
                var id = Int32.Parse(fields[0]);
                Pokedex[id] = new Pokemon
                {
                    id = id,
                    name = fields[1],
                    sprites = new Pokemon.SpriteList
                    {
                        front_default = fields[2]
                    }
                };
            });
            
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
            var imageCsvPath = HostingEnvironment.MapPath(@"~/App_Data/PokemonImages.csv");
            var imageCsvWriter = new StreamWriter(imageCsvPath);
            
            var csvPath = HostingEnvironment.MapPath(@"~/App_Data/Pokemon.csv");
            var parser = new TextFieldParser(csvPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.ReadFields(); // skip first line
            while (!parser.EndOfData) 
            {
                string[] fields = parser.ReadFields();
                var id = Int32.Parse(fields[0]);
                var name = fields[1];
                var bulbapediaIcon = fields[2];
                var imageUrl = ScrapePokemonImageUrl(id, bulbapediaIcon != "" ? bulbapediaIcon : name);
                
                var line = string.Format("{0},{1},{2}", id, name, imageUrl);
                imageCsvWriter.WriteLine(line);
                System.Diagnostics.Debug.WriteLine(line);
            }
            imageCsvWriter.Close();
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