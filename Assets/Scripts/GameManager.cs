using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Unstable.Model;
using Unstable.UI;

namespace Unstable
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { set; get; }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// List of all the leaders
        /// </summary>
        private List<Leader> _leaders;
        /// <summary>
        /// List of all the events
        /// </summary>
        private List<Model.Event> _standardEvents, _crisisEvents;

        public Leader GetLeaderFromTrigram(string trigram)
        {
            return _leaders.FirstOrDefault(x => x.Trigram == trigram);
        }

        /// <summary>
        /// Number of rounds where we just got "normal" events
        /// </summary>
        private int _numberOfRoundsWithoutCrisis = 0;

        [SerializeField]
        private RectTransform _hand;
        private List<UI.Card> _cards = new();

        [SerializeField]
        private GameObject _cardPrefab;

        [SerializeField]
        private EventLoader _eventLoader;

        private void Start()
        {
            _leaders = JsonConvert.DeserializeObject<List<Leader>>(Resources.Load<TextAsset>("Leaders").text);
            var events = JsonConvert.DeserializeObject<Model.Event[]>(Resources.Load<TextAsset>("Events").text);

            Assert.IsNotNull(_leaders, "Leaders info failed to load");
            Assert.IsNotNull(events, "Events info failed to load");
            Assert.IsTrue(_leaders.Count > 0, "No leader was found");
            Assert.IsTrue(events.Length > 0, "No event was found");

            _standardEvents = events.Where(x => !x.IsCrisis).ToList();
            _crisisEvents = events.Where(x => x.IsCrisis).ToList();

            // DEBUG
            AddCard(_leaders[0].Cards.First(x => true).Value);
            AddCard(_leaders[0].Cards.First(x => true).Value);

            NextEvent();
        }

        public void NextEvent()
        {
            var isCrisis = _numberOfRoundsWithoutCrisis > 5;

            var e = isCrisis ? _crisisEvents[Random.Range(0, _crisisEvents.Count)] : _standardEvents[Random.Range(0, _standardEvents.Count)];
            _eventLoader.Load(e);

            _numberOfRoundsWithoutCrisis++;
        }

        private void AddCard(Model.Card card)
        {
            var cardGo = Instantiate(_cardPrefab, _hand);
            var cardIns = cardGo.GetComponent<UI.Card>();
            cardIns.Init(card);
            _cards.Add(cardIns);

            var cardSize = 100;
            var half = _cards.Count / 2f;
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].SetTarget(Vector3.right * (i - half) * cardSize + Vector3.right * (cardSize / 2f));
            }
        }
    }
}