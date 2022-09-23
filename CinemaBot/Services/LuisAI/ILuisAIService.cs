using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaBot.Services.LuisAI
{
    public interface ILuisAIService
    {
        LuisRecognizer _luisRecognizer { get; set; }
    }
}
