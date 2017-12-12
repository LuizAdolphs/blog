using static Newtonsoft.Json.JsonConvert;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                IEnumerable<Pokemon> pokemons = Enumerable
                .Range(1,11)
                .Select(x => 
                {
                    var result = client.GetAsync($"http://pokeapi.co/api/v2/pokemon/{x}").Result; 

                    return DeserializeObject<Pokemon>(result.Content.ReadAsStringAsync().Result);
                });   

                foreach(var pokemon in pokemons)
                {
                    Console.WriteLine(pokemon.Name);

                    if(pokemon.Types.Any(x => x.Type.Name.ToLower() == "fire"))
                        break;
                }

            }

            stopWatch.Stop();

            Console.WriteLine($"Tempo de execução: {stopWatch.Elapsed.Seconds} segundos");
        }
    }
    public class Pokemon
    {
        public string Name { get; set; }
        public IList<PokemonTypeSlot> Types { get; set; }

        public class PokemonTypeSlot
        {
            public int Slot { get; set; }
            public PokemonType Type { get; set; }
            public class PokemonType {
                public string Name { get; set; }
            }
        }
    }
}