---
permalink: custom.html
---

# Defining Custom Games
Card Game Simulator allows users to [download custom card games to use within the application](index.html#create--share-custom-games).

## CGS Core Concept
A custom game in CGS is a specification for a collection of information about a card game. This information primarily includes sets of cards and a definition of what properties those cards have. Ancillary information can include decks and boards to use with the game. See below for the full specification.

## CGS games directory
Custom games are created with a new folder within the persistent games data directory. The location of this persistent data directory varies depending on platform. Some examples include:
- Android: /Data/Data/com.finoldigital.cardgamesim/files/games/
- Mac: ~/Library/Application Support/Finol Digital LLC/Card Game Simulator/games/
- Windows: C:\Users\\<user \>\AppData\Local\Packages\FinolDigitalLLC.CardGameSimulator_499qk536pdy94\LocalState\games

## Custom game folder structure
The structure of this custom game folder is:
- *Game:Id*/
  - *Game:Name*.json
  - AllCards.json
  - AllSets.json
  - Banner.\<bannerImageFileType\>
  - CardBack.\<cardBackImageFileType\>
  - boards/
    * *GameBoard:Id*.\<gameBoardFileType\>
    * ...
  - decks/
    * *Deck:Name*.\<deckFileType\>
    * ...
  - sets/
    * *Set:Code*/
      * *Card:Id*.\<cardImageFileType\>
      * ...
    * ...

## JSON File Structure
The GGS AutoUpdate Url that is used to download a card game is actually a pointer to the *Game:Name*.json file. CGS generates the rest of the folder structure based off that *CardGameDef.json* file. 

You can create your own json and [validate](https://www.jsonschemavalidator.net/) against these schema:
- [CardGameDef](schema/CardGameDef.json)
- [AllCards](schema/AllCards.json)
- [AllSets](schema/AllSets.json)

## Examples
The default examples can be found in the [CGS GitHub Repository](https://github.com/finol-digital/Card-Game-Simulator/tree/develop/docs/games).
Further examples can be found in the [CGS Google Drive folder](https://drive.google.com/open?id=1kVms-_CXRw1e4Ob18fRkS84MN_cxQGF5).
