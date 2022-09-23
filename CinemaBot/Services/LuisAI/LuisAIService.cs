using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaBot.Services.LuisAI
{
    public class LuisAIService : ILuisAIService
    {
        public LuisRecognizer _luisRecognizer { get; set; }

        public LuisAIService(IConfiguration configuration)
        {

            //instancio una aplicacion de LUIS
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],//lo leo desde el appseting.json
                configuration["LuisApiKey"],//lo leo desde el appseting.json
                configuration["LuisHostName"]//lo leo desde el appseting.json
             );

            //creo instancia del servicio de reconocimiento
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions()
                {
                    IncludeInstanceData = true
                }
            };

            //inyecto el servicio de Luis Recognizer a la variable de Luis
            _luisRecognizer = new LuisRecognizer(recognizerOptions);
        }
    }
}
