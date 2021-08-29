# NFT Observer

This project includes:

- An [Observer](https://github.com/MonkeDAO/observer/blob/master/Observer/) which can be trivially adapted to include more NFT marketplaces in order to track sales happening in them.
- A [Scraper](https://github.com/MonkeDAO/observer/blob/master/Scraper/) which can be used to create a unified dataset of NFT collection items with their metadatas (attributes, image urls etc).

This code is currently powering [Solana Observer](https://twitter.com/SolanaObserver), a twitter bot which currently tracks sales happening in the following marketplaces, but it's twitter functionality is not included here:

- [Solana Monkey Business](https://market.solanamonkey.business)
- [Digital Eyes](https://digitaleyes.market)
- [Solanart](https://solanart.io)

## Getting started

For several reasons the datasets that can be achieve using the scraper are not included in the repo, primarily due to size,
although the observer is designed to load these from the JSON files the scraper produces in order to not have to continually request for data.

### Scraper

The idea behind the scraper is that it is able to get all metadata accounts related to the [Metaplex](https://github.com/metaplex-foundation/metaplex) protocol for a certain collection.
It achieves this by using the very powerful RPC method known as `getProgramAccounts`, and comparing the values which represent either a metadata account's name or symbol.

To retrieve a collection by their name you use the `memcmp` parameter at offset 69, which equals to this [value](https://github.com/metaplex-foundation/metaplex/blob/master/rust/token-metadata/program/src/state.rs#L80).
To retrieve a collection by their symbol you use the `memcmp` parameter at offset 105, which equals to this [value](https://github.com/metaplex-foundation/metaplex/blob/master/rust/token-metadata/program/src/state.rs#L82).

For certain collections you may need to perform requests with both `memcmp` at offset 69 and offset 105.

Example:

To get metadatas for Degenerate Apes Academy you could either scrape by `DAPE` symbol or by `Degen Ape` name.

To get metadatas for Ape Shit Social you will need to use both in order to filter out bogus metadata accounts. 

Remarks:

Due to the numerous ways different projects are formatting their names and symbols you will likely need to alter code in order to perform certain sanity checks.

Considering you have edited the `_collections` dictionary present in `Scraper/Program.cs`, all you need to do to run this tool is:

```bash
dotnet run --project Scraper/Scraper.csproj --configuration Release
```

### Observer

The idea behind the observer is to periodically query the RPC for recent signatures w.r.t. the marketplace's addresses, getting the transactions for these signatures,
decoding them and by analyzing their instructions and thus being able to see which are a listing, an nft being unlisted or an sale, this will depend on how the marketplaces implemented their programs.

Considering you have datasets and have added them to the `Observer/Resources/` folder, you just need to run:

```bash
dotnet run --project Observer/Observer.csproj --configuration Release
```

Observations: This is suboptimal quality code hacked together in 2 days without any other prior code or knowledge in the Metaplex context,
and sort is sort of just an example of the fun little things you can already build using [Solnet](https://github.com/bmresearch/Solnet).

## Contribution

We encourage everyone to contribute, submit issues, PRs, discuss. Every kind of help is welcome.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/MonkeDAO/observer/blob/master/LICENSE) file for details

### Supporting

If you like these tools and/or are actively making use of them, consider support the developer by sending tips to:

SOL → hoak.sol

SOL → 7y62LXLwANaN9g3KJPxQFYwMxSdZraw5PkqwtqY9zLDF