// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CinemaBot.Common.Models.EntityModel;
using CinemaBot.Common.Models.Movies;
using CinemaBot.Services.LuisAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaBot
{
    public class CinemaBot : ActivityHandler
    {
        private readonly ILuisAIService _luisAIService;
        public CinemaBot(ILuisAIService luisAIService)
        {
            //inyecto el servicio de LUIS
            _luisAIService = luisAIService;
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //capturo el texto que escribe el usuario
            var userText = turnContext.Activity.Text;

            //obtengo el resultado de Luis
            var luisResult = await _luisAIService._luisRecognizer.RecognizeAsync(turnContext, cancellationToken);

            //metodo que maneja las intenciones
            await Intentions(turnContext, luisResult, cancellationToken);
        }

        private async Task Intentions(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //obtengo la intencion con mayor proabilidad
            var topIntent = luisResult.GetTopScoringIntent();

            switch (topIntent.intent.ToLower())
            {
                case "saludar":
                    await IntentSaludar(turnContext, luisResult, cancellationToken);
                    break;
                case "despedir":
                    await IntentDespedir(turnContext, luisResult, cancellationToken);
                    break;
                case "agradecer":
                    await IntentAgradecer(turnContext, luisResult, cancellationToken);
                    break;
                case "comprarpelicula":
                    await IntentComprarPelicula(turnContext, luisResult, cancellationToken);
                    break;
                case "none":
                    await IntentNone(turnContext, luisResult, cancellationToken);
                    break;
                default:
                    await IntentNone(turnContext, luisResult, cancellationToken);
                    break;

            }
        }

        private async Task IntentNone(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Disculpa no entiendo lo que me dices", cancellationToken: cancellationToken);
        }

        private async Task IntentComprarPelicula(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //Valido si se ha escrito una entidad y mapeo al modelo EntityLuis
            var entities = luisResult.Entities.ToObject<EntityLuis>();

            //valido si la entidad tiene alguna pelicula en especifico sino muestro todas las peliculas
            //ej => quiero ver la peli del wason
            if (entities.ListPeliculas?.Count > 0)
            {
                //obtengo la primera pelicua segun la entidad y luego la filtro en el listado de peliculas
                var movie = entities.ListPeliculas.FirstOrDefault().FirstOrDefault();
                var filteredList = GetMovies().Where(x => x.name.ToLower() == movie.ToLower()).ToList();

                await turnContext.SendActivityAsync($"Aqui tienes el Detalle de {movie}", cancellationToken: cancellationToken);
                await Task.Delay(1500);
                //muestro la pelicula filtrada segun la entidad
                await turnContext.SendActivityAsync(activity: ShowMovies(filteredList), cancellationToken: cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("Estas son las peliculas de nuestra cartelera", cancellationToken: cancellationToken);
                await Task.Delay(1500);
                //muestro las peliculas en un carousel llamando la actividad ShowMovie
                await turnContext.SendActivityAsync(activity: ShowMovies(GetMovies()), cancellationToken: cancellationToken);
            }
            
            
        }

        private List<MoviesModel> GetMovies()
        {
            var list = new List<MoviesModel> {
                new MoviesModel
                {
                    name="Joker",
                    price="10",
                    imageUrl="https://hips.hearstapps.com/hmg-prod.s3.amazonaws.com/images/joker-2-1569239913.jpg",
                    informationUrl="https://es.wikipedia.org/wiki/Joker_(pel%C3%ADcula)",
                },
                new MoviesModel
                {
                    name="1917",
                    price="9.50",
                    imageUrl="https://media.vogue.mx/photos/5e26e68b07b0840008ae2c13/master/pass/1917.jpg",
                    informationUrl="https://en.wikipedia.org/wiki/1917_(2019_film)",
                },
                new MoviesModel
                {
                    name="El Llamado Salvaje",
                    price="8.70",
                    imageUrl="https://lumiere-a.akamaihd.net/v1/images/tcotw_002b_g_spa-ar_70x100_r_75eec6fc.jpeg",
                    informationUrl="https://es.wikipedia.org/wiki/The_Call_of_the_Wild_(pel%C3%ADcula)",
                }
            };

            return list;
        }

        private IActivity ShowMovies(List<MoviesModel> listMovies)
        {
            var optionsAttacments = new List<Attachment>();

            //creo la tarejta de forma dinamica segun la lista de peliculas
            foreach (var item in listMovies)
            {
                var card = new HeroCard
                {
                    Title = item.name,
                    Subtitle = $"Precion: ${item.price}",
                    Images = new List<CardImage> { new CardImage(item.imageUrl) },
                    Buttons = new List<CardAction>() {
                    new CardAction (){Title ="Comprar", Value="Comprar",Type = ActionTypes.ImBack },
                    new CardAction () { Title = "Ver informacion",Value= item.informationUrl ,Type = ActionTypes.OpenUrl },
                    }
                };

                //añado las tarjetas a los attanchment
                optionsAttacments.Add(card.ToAttachment());

            }

            /*
            var cardJoker = new HeroCard
            {
                Title = "Joker",
                Subtitle = "Precion: $10",
                Images = new List<CardImage> { new CardImage("https://hips.hearstapps.com/hmg-prod.s3.amazonaws.com/images/joker-2-1569239913.jpg") },
                Buttons = new List<CardAction>() {
                    new CardAction (){Title ="Comprar", Value="Comprar",Type = ActionTypes.ImBack },
                    new CardAction () { Title = "Ver informacion",Value= "https://es.wikipedia.org/wiki/Joker_(pel%C3%ADcula)",Type = ActionTypes.OpenUrl },
                }
            };

            var card1917 = new HeroCard
            {
                Title = "1917",
                Subtitle = "Precion: $9.50",
                Images = new List<CardImage> { new CardImage("https://media.vogue.mx/photos/5e26e68b07b0840008ae2c13/master/pass/1917.jpg") },
                Buttons = new List<CardAction>() {
                    new CardAction (){Title ="Comprar", Value="Comprar",Type = ActionTypes.ImBack },
                    new CardAction () { Title = "Ver informacion",Value= "https://en.wikipedia.org/wiki/1917_(2019_film)",Type = ActionTypes.OpenUrl },
                }
            };

            var cardCallWild = new HeroCard
            {
                Title = "Llamado Salvaje",
                Subtitle = "Precion: $8.70",
                Images = new List<CardImage> { new CardImage("https://lumiere-a.akamaihd.net/v1/images/tcotw_002b_g_spa-ar_70x100_r_75eec6fc.jpeg") },
                Buttons = new List<CardAction>() {
                    new CardAction (){Title ="Comprar", Value="Comprar",Type = ActionTypes.ImBack },
                    new CardAction () { Title = "Ver informacion",Value= "https://es.wikipedia.org/wiki/The_Call_of_the_Wild_(pel%C3%ADcula)",Type = ActionTypes.OpenUrl },
                }
            };

            //añado las tarjetas a los attanchment
            optionsAttacments.Add(cardJoker.ToAttachment());
            optionsAttacments.Add(card1917.ToAttachment());
            optionsAttacments.Add(cardCallWild.ToAttachment());

            */

            //mando el reply como un carousel
            var reply = MessageFactory.Attachment(optionsAttacments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            return reply as Activity;
        }

        private async Task IntentAgradecer(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("De nada , Gracias a ti por contactarme", cancellationToken: cancellationToken);
        }

        private async Task IntentDespedir(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Espero verte pronto", cancellationToken: cancellationToken);
        }

        private async Task IntentSaludar(ITurnContext<IMessageActivity> turnContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Hola, Que Gusto Saludarte", cancellationToken: cancellationToken);
        }
    }
}
