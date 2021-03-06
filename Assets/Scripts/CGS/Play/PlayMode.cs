/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

using CardGameDef;
using CardGameView;
using CGS.Cards;
using CGS.Decks;
using CGS.Play.Multiplayer;
using CGS.Play.Zones;

namespace CGS.Play
{
    [RequireComponent(typeof(Canvas))]
    public class PlayMode : MonoBehaviour
    {
        public const string MainMenuPrompt = "Go back to the main menu?";
        public const string DealHandPrompt = "Draw initial starting hand?";
        public const string NoRulesErrorMessage = "Rules Url does not exist for this game!";

        public GameObject cardViewerPrefab;
        public GameObject lobbyMenuPrefab;
        public GameObject deckLoadMenuPrefab;
        public GameObject diceMenuPrefab;
        public GameObject searchMenuPrefab;

        public ZonesViewer zones;
        public RectTransform playAreaContent;
        public Image currentBanner;
        public Text netText;

        public LobbyMenu Lobby => _lobby ?? (_lobby = Instantiate(lobbyMenuPrefab).GetOrAddComponent<LobbyMenu>());
        private LobbyMenu _lobby;

        public DeckLoadMenu DeckLoader => _deckLoader ?? (_deckLoader = Instantiate(deckLoadMenuPrefab).GetOrAddComponent<DeckLoadMenu>());
        private DeckLoadMenu _deckLoader;

        public DiceMenu DiceManager => _diceManager ?? (_diceManager = Instantiate(diceMenuPrefab).GetOrAddComponent<DiceMenu>());
        private DiceMenu _diceManager;

        public CardSearchMenu CardSearcher => _cardSearcher ?? (_cardSearcher = Instantiate(searchMenuPrefab).GetOrAddComponent<CardSearchMenu>());
        private CardSearchMenu _cardSearcher;

        void OnEnable()
        {
            Instantiate(cardViewerPrefab);
            CardGameManager.Instance.OnSceneActions.Add(ResetPlayArea);
        }

        void Start()
        {
            CardGameManager.Instance.CardCanvases.Add(GetComponent<Canvas>());
            playAreaContent.gameObject.GetOrAddComponent<CardStack>().OnAddCardActions.Add(AddCardToPlay);
            if (CardGameManager.Instance.IsSearchingForServer)
                Lobby.Show(BackToMainMenu);
            else
            {
                ShowDeckMenu();
                Lobby.Host();
            }
        }

        void Update()
        {
            if (CardViewer.Instance.IsVisible || CardViewer.Instance.Zoom || !Input.anyKeyDown || CardGameManager.Instance.ModalCanvas != null)
                return;

            if (Input.GetButtonDown(Inputs.Load))
                ShowDeckMenu();
            else if (Input.GetButtonDown(Inputs.Save))
                ShowDiceMenu();
            else if (Input.GetButtonDown(Inputs.Filter))
                ShowCardsMenu();
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(Inputs.Cancel))
                PromptBackToMainMenu();
        }

        public void ResetPlayArea()
        {
            playAreaContent.sizeDelta = CardGameManager.Current.PlayAreaSize * CardGameManager.PixelsPerInch;
            currentBanner.sprite = CardGameManager.Current.BannerImageSprite;
        }

        public void ViewRules()
        {
            if (CardGameManager.Current.RulesUrl != null && CardGameManager.Current.RulesUrl.IsWellFormedOriginalString())
                Application.OpenURL(CardGameManager.Current.RulesUrl.OriginalString);
            else
                CardGameManager.Instance.Messenger.Show(NoRulesErrorMessage);
        }

        public void ShowDeckMenu()
        {
            DeckLoader.Show(LoadDeck);
        }

        public void ShowDiceMenu()
        {
            DiceManager.Show(playAreaContent);
        }

        public void ShowCardsMenu()
        {
            CardSearcher.Show(DisplayResults);
        }

        public void LoadDeck(Deck deck)
        {
            if (deck == null)
                return;

            Dictionary<string, List<Card>> extraGroups = deck.GetExtraGroups();
            foreach (KeyValuePair<string, List<Card>> cardGroup in extraGroups)
                zones.CreateExtraZone(cardGroup.Key, cardGroup.Value);

            List<Card> deckCards = new List<Card>();
            List<Card> extraCards = deck.GetExtraCards();
            foreach (Card card in deck.Cards)
                if (!extraCards.Contains(card))
                    deckCards.Add(card);
            deckCards.Shuffle();

            LoadDeckCards(deckCards);

            foreach (Card card in deck.Cards)
                foreach (GameBoardCard boardCard in CardGameManager.Current.GameBoardCards)
                    if (card.Id.Equals(boardCard.Card))
                        CreateGameBoards(boardCard.Boards);
        }

        public void LoadDeckCards(List<Card> deckCards, bool isShared = false)
        {
            zones.CreateDeck();
            zones.scrollView.verticalScrollbar.value = 0;
#if !UNITY_WEBGL
            if (!isShared)
                CGSNetManager.Instance.LocalPlayer?.RequestNewDeck(deckCards);
#endif
            zones.CurrentDeck.Sync(deckCards);
            StartCoroutine(zones.CurrentDeck.WaitForLoad(CreateHand));
        }

        public void CreateGameBoards(List<GameBoard> boards)
        {
            if (boards == null || boards.Count < 1)
                return;

            foreach (GameBoard board in boards)
                CreateBoard(board);
        }

        public void CreateBoard(GameBoard board)
        {
            if (board == null)
                return;

            GameObject newBoard = new GameObject(board.Id, typeof(RectTransform));
            RectTransform rt = (RectTransform)newBoard.transform;
            rt.SetParent(playAreaContent);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.offsetMin = board.OffsetMin * CardGameManager.PixelsPerInch;
            rt.offsetMax = board.Size * CardGameManager.PixelsPerInch + rt.offsetMin;

            string boardFilepath = CardGameManager.Current.GameBoardsFilePath + "/" + board.Id + "." +
                                   CardGameManager.Current.GameBoardFileType;
            Sprite boardImageSprite = System.IO.File.Exists(boardFilepath) ? UnityExtensionMethods.CreateSprite(boardFilepath) : null;
            if (boardImageSprite != null)
                newBoard.AddComponent<Image>().sprite = boardImageSprite;

            rt.localScale = Vector3.one;
        }

        public void CreateHand()
        {
            if (zones.Hand == null)
                zones.CreateHand();

            if (CardGameManager.Current.GameStartHandCount > 0)
                CardGameManager.Instance.Messenger.Ask(DealHandPrompt, () => { }, DealStartingHand);
        }

        public void DealStartingHand()
        {
            if (zones.Hand == null)
                return;

            if (!zones.Hand.IsExtended)
                zones.Hand.ToggleExtension();

            Deal(CardGameManager.Current.GameStartHandCount);
        }

        public void Deal(int cardCount)
        {
            AddCardsToHand(PopDeckCards(cardCount));
        }

        public void Burn(int cardCount)
        {
            foreach (Card card in PopDeckCards(cardCount))
                CatchDiscard(card);
        }

        public List<Card> PopDeckCards(int cardCount)
        {
            List<Card> cards = new List<Card>(cardCount);
            if (zones.CurrentDeck == null)
                return cards;

            for (int i = 0; i < cardCount && zones.CurrentDeck.Cards.Count > 0; i++)
                cards.Add(zones.CurrentDeck.PopCard());
            return cards;
        }

        public void AddCardsToHand(List<Card> cards)
        {
            if (zones.Hand == null)
                zones.CreateHand();

            foreach (Card card in cards)
                zones.Hand.AddCard(card);
        }

        public void AddCardToPlay(CardStack cardStack, CardModel cardModel)
        {
            if (NetworkManager.singleton.isNetworkActive)
                CGSNetManager.Instance.LocalPlayer.MoveCardToServer(cardStack, cardModel);
            else
                SetPlayActions(cardStack, cardModel);
        }

        public void SetPlayActions(CardStack cardStack, CardModel cardModel)
        {
            cardModel.DoubleClickAction = CardActions.Rotate90;
            cardModel.SecondaryDragAction = cardModel.Rotate;
        }

        public void CatchDiscard(Card card)
        {
            if (zones.Discard == null)
                zones.CreateDiscard();
            zones.Discard.AddCard(card);
        }

        public void DisplayResults(string filters, List<Card> cards)
        {
            if (zones.Results == null)
                zones.CreateResults();
            zones.Results.Sync(cards);
        }

        public void PromptBackToMainMenu()
        {
            CardGameManager.Instance.Messenger.Prompt(MainMenuPrompt, BackToMainMenu);
        }

        public void BackToMainMenu()
        {
            if (NetworkManager.singleton.isNetworkActive)
            {
                CGSNetManager.Instance.Discovery.StopDiscovery();
                if (NetworkServer.active)
                    NetworkManager.singleton.StopHost();
                else if (NetworkClient.isConnected)
                    NetworkManager.singleton.StopClient();
            }

            SceneManager.LoadScene(CGS.Menu.MainMenu.MainMenuSceneIndex);
        }
    }
}
