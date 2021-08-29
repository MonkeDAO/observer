using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CoinGecko.Clients;
using CoinGecko.Interfaces;
using CoreTweet;
using Microsoft.Extensions.Logging;
using Solnet.Rpc;

namespace Observer.Handlers
{
    public abstract class Handler
    {
        /// <summary>
        /// 
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        protected string _name;

        /// <summary>
        /// 
        /// </summary>
        protected string _address;

        /// <summary>
        /// The rpc client instance.
        /// </summary>
        protected IRpcClient _client;

        /// <summary>
        /// The json serializer options.
        /// </summary>
        protected readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The source of the cancellation token for the rpc and http requests done by this view model.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// The current price of SOL.
        /// </summary>
        private float _price;

        private static List<string> _quotes = new ()
        {
            "I'll buy as much SOL has you have, right now, at $3. - SBF",
            "so bullish on $SOL i’m learning Rust - Flood",
            "I believe no monke should be sold for under 30 sol as their minimum value - burner208 11/08/2021",
            "Tired of seeing all these simps selling for under 30 sol - burner208 12/08/2021",
            "It's not hopium it's a meme sir. Memes drive market whether you like or not - burner208 11/08/2021",
            "It's clearly giga rare - burner208 12/08/2021",
            "fuck your mother if you want fuck",
            "Probably nothing."
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        internal Handler(string name, string address)
        {
            _name = name;
            _address = address;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}