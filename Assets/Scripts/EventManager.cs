﻿using System;

namespace Unstable
{
    public static class EventManager
    {
        public static void DoAction(string command, string argument)
        {
            switch (command.ToUpperInvariant())
            {
                case "ADD":
                    {
                        string[] s = argument.Split(' ');
                        var trigram = s[0];
                        var cardTrigram = s[1];

                        for (int i = 0; i < int.Parse(s[2]); i++)
                        {
                            var res = GameManager.Instance.GetCard(trigram == "null" ? null : trigram, cardTrigram);
                            GameManager.Instance.AddCard(res.Item1, res.Item2);
                        }
                    }
                    break;

                case "REM":
                    {
                        string[] s = argument.Split(' ');
                        var trigram = s[0];
                        var count = int.Parse(s[1]);
                        GameManager.Instance.RemoveCard(trigram, count);
                    }
                    break;

                case "MED":
                    {
                        GameManager.Instance.ReduceCostBy++;
                    }
                    break;

                case "CRI":
                    {
                        GameManager.Instance.PreventNextCrisis();
                    }
                    break;

                case "SCO":
                    {
                        GameManager.Instance.Score = 0;
                    }
                    break;

                case "KIL":
                    {
                        GameManager.Instance.LowerSectorSanity(argument, 1000);
                    }
                    break;

                default:
                    throw new NotImplementedException("Unknown command " + command);
            }
        }

        public static string ActionToString(string command, string argument)
        {
            switch (command.ToUpperInvariant())
            {
                case "ADD":
                    {
                        string[] s = argument.Split(' ');
                        var trigram = s[0];
                        var cardTrigram = s[1];
                        if (trigram == "ANY")
                        {
                            return $"Gain {s[2]} card";
                        }
                        var card = GameManager.Instance.GetCard(trigram == "null" ? null : trigram, cardTrigram);
                        return $"Gain {s[2]} {card.Item1.Name.ToLowerInvariant()}";
                    }

                case "REM":
                    {
                        return $"Loose {argument} random cards";
                    }

                case "MED":
                    {
                        return $"Reduce the impact of the next crisis";
                    }

                case "CRI":
                    {
                        return $"Prevent the next crisis to happen";
                    }

                case "SCO":
                    {
                        return $"Reset your score to 0";
                    }

                case "KIL":
                    {
                        return $"Kill the {GameManager.Instance.GetLeaderFromTrigram(argument).DomainName} faction leader";
                    }

                default:
                    throw new NotImplementedException("Unknown command " + command);
            }
        }
    }
}
