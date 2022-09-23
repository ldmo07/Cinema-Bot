using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaBot.Common.Models.EntityModel
{
    public class EntityLuis
    {
        [JsonProperty("$instance")]
        public Instance _instance { get; set; }
        public List<List<string>> ListPeliculas { get; set; }
    }

    public class Instance
    {
        //ListsPeliculas es el nombre de la entidad que creamos en Luis y se debe mapear en c#
        public List<ListPeliculas> ListPeliculas { get; set; }
    }

    public class ListPeliculas
    {
        public string type { get; set; }
        public string text { get; set; }
        public string modelType { get; set; }
    }
}
