using System.Collections.Generic;
using Observer.Handlers;

namespace Observer
{
    /// <summary>
    /// Represents the marketplaces and exposes maps between their names, program address and their respective handler implementation..
    /// </summary>
    public static class Marketplaces
    {
        /// <summary>
        /// The mapping of a marketplace name to their program address.
        /// </summary>
        public static Dictionary<string, string> AddressMap = new()
        {
            { "Solanart", "CJsLwbP1iu5DuUikHEJnLfANgKy6stB2uFgvBBHoyxwz" },
            { "Solana Monkey Business", "GvQVaDNLV7zAPNx35FqWmgwuxa4B2h5tuuL73heqSf1C" },
            { "Digital Eyes", "A7p8451ktDCHq5yYaHczeLMYsjRsAkzc3hCXcSrwYHU7"}/**/
        };

        /// <summary>
        /// The mapping of a marketplace name to their respective handler implementation.
        /// </summary>
        public static Dictionary<string, IHandler> HandlerMap = new()
        {
            {
                "Solanart",
                new SolanartHandler("Solanart", "CJsLwbP1iu5DuUikHEJnLfANgKy6stB2uFgvBBHoyxwz")
            },
            {
                "Solana Monkey Business",
                new MonkeyBusinessHandler("Solana Monkey Business", "GvQVaDNLV7zAPNx35FqWmgwuxa4B2h5tuuL73heqSf1C")
            },
            {
                "Digital Eyes",
                new DigitalEyesHandler("Digital Eyes", "A7p8451ktDCHq5yYaHczeLMYsjRsAkzc3hCXcSrwYHU7")
            }
        };
    }
}