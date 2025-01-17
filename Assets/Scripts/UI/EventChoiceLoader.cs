﻿using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unstable.Model;

namespace Unstable.UI
{
    public class EventChoiceLoader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDropHandler
    {
        [SerializeField]
        private TMP_Text _title, _description;

        [SerializeField]
        private GameObject _requirementPanel;

        [SerializeField]
        private GameObject _effectPanel;


        [SerializeField]
        private TMP_Text _requirementText;

        [SerializeField]
        private TMP_Text _effectText;

        private Image _image;
        private Color _baseColor;

        private EventChoice _choiceData;

        private Dictionary<string, int> _requirements;
        private List<string> _effects;

        private bool _isLast;

        public void Init(EventChoice choice, bool isLast)
        {
            _isLast = isLast;

            _title.text = choice.TargetTrigram != null
                ? GameManager.Instance.GetLeaderFromTrigram(choice.TargetTrigram).DomainName
                : "";
            _description.text = choice.Description;
            _choiceData = choice;

            _requirements = new();
            _effects = new();

            if (GameManager.Instance.IsLeaderAlive(choice.TargetTrigram))
            {
                if (choice.Requirements != null && choice.Requirements.Any())
                {
                    _requirements = choice.Requirements.Select(r =>
                    {
                        return (r.Key, GameManager.RequirementToInt(r.Value));
                    }).ToDictionary(x => x.Item1, x => x.Item2);
                }
                if (choice.Effects != null && choice.Effects.Any())
                {
                    _effects = _choiceData.Effects.Select(x => EventManager.ActionToString(x.MethodName, x.Argument)).ToList();
                }
            }
            else
            {
                _requirements.Add("ALIVE", -1);
            }

            UpdateRequirementDisplay();
        }

        public void UpdateRequirementDisplay()
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
                _baseColor = _image.color;
            }

            if (_requirements.ContainsKey("ALIVE"))
            {
                _requirementPanel.SetActive(true);
                _requirementText.text = "The corresponding leader is not alive";
                _image.color = new Color(_baseColor.r - .2f, _baseColor.g - .2f, _baseColor.b - .2f);
            }
            else
            {
                if (_choiceData.Requirements != null && _requirements.Any())
                {
                    _requirementPanel.SetActive(true);
                    _requirementText.text = string.Join("\n", _requirements.Select(r =>
                    {
                        return GameManager.Instance.GetEffect(r.Key) + ": " + r.Value;
                    }));
                    _image.color = new Color(_baseColor.r - .2f, _baseColor.g - .2f, _baseColor.b - .2f);
                }
                else
                {
                    _requirementPanel.SetActive(false);
                    _requirementText.text = "";
                    _image.color = _baseColor;
                }
            }

            if (_choiceData.Effects != null && _effects.Any())
            {
                _effectPanel.SetActive(true);
                _effectText.text = string.Join("\n", _effects);
            }
            else
            {
                _effectPanel.SetActive(false);
                _effectText.text = "";
            }

            if (_choiceData.Cost != "NONE")
            {
                _effectPanel.SetActive(true);
                if (!string.IsNullOrWhiteSpace(_effectText.text))
                {
                    _effectText.text += "\n";
                }
                _effectText.text += "Sanity penalty: " + GameManager.CostToString(_choiceData.Cost);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _image.color = new Color(_baseColor.r - .2f, _baseColor.g - .2f, _baseColor.b - .2f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_requirements.Any())
            {
                _image.color = _baseColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_requirements.Any())
            {
                if (_choiceData.Effects != null)
                {
                    foreach (var effect in _choiceData.Effects)
                    {
                        EventManager.DoAction(effect.MethodName, effect.Argument);
                    }
                }

                // Remove sanity
                var cost = GameManager.CostToInt(_choiceData.Cost);
                cost -= GameManager.Instance.ReduceCostBy * 2;
                if (cost < 0)
                {
                    cost = 0;
                }
                GameManager.Instance.ReduceCostBy = 0;

                if (_isLast)
                {
                    GameManager.Instance.RemoveRandomSanity(null, cost);
                }
                else
                {
                    var trigrams = GameManager.Instance.GetCurrentEvent().Choices.Select(x => x.TargetTrigram).Where(x => x != null).Distinct().ToList();
                    if (_choiceData.TargetTrigram != null)
                    {
                        trigrams.Remove(_choiceData.TargetTrigram);
                    }
                    GameManager.Instance.RemoveSanity(_choiceData.TargetTrigram, trigrams.ToArray(), cost);
                    // GameManager.Instance.RemoveRandomSanity(_choiceData.TargetTrigram, cost);
                }


                GameManager.Instance.EndEvent();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            UI.Card card = eventData.pointerDrag.GetComponent<UI.Card>();

            if (_requirements.ContainsKey("ALIVE")) // The leader is dead, card not available
            {
                return;
            }

            if (_requirements.Any())
            {
                bool _requirementFound = false;
                if (card.Effects != null)
                {
                    // if _requirement find something => we remove from the req
                    foreach (var effectValue in card.Effects)
                    {
                        if (_requirements.ContainsKey(effectValue.Key))
                        {
                            // remove effect to requirement
                            _requirements[effectValue.Key] -= effectValue.Value;
                            // if requirement <= 0, remove req
                            if (_requirements[effectValue.Key] <= 0)
                            {
                                _requirements.Remove(effectValue.Key);
                            }
                            UpdateRequirementDisplay();
                            _requirementFound = true;
                        }
                    }
                }

                if (!_requirementFound)
                {
                    // here, we have req but no matching effect, we remove 1 at the first 1
                    string key = _requirements.First().Key;
                    _requirements[key] -= 1;
                    if (_requirements[key] <= 0)
                    {
                        _requirements.Remove(key);
                    }
                }
                GameManager.Instance.RemoveCard(card);
                UpdateRequirementDisplay();

                OnPointerDown(eventData);
            }
        }
    }
}
