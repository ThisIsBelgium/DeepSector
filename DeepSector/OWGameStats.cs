using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSector
{
    public class Quickplay
    {
        public string hero { get; set; }
        public string played { get; set; }
        public string img { get; set; }
    }

    public class Competitive
    {
        public string hero { get; set; }
        public string played { get; set; }
        public string img { get; set; }
    }

    public class TopHeroes
    {
        public List<Quickplay> quickplay { get; set; }
        public List<Competitive> competitive { get; set; }
    }

    public class Quickplay2
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive2
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Combat
    {
        public List<Quickplay2> quickplay { get; set; }
        public List<Competitive2> competitive { get; set; }
    }

    public class Quickplay3
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive3
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Deaths
    {
        public List<Quickplay3> quickplay { get; set; }
        public List<Competitive3> competitive { get; set; }
    }

    public class Quickplay4
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive4
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class MatchAwards
    {
        public List<Quickplay4> quickplay { get; set; }
        public List<Competitive4> competitive { get; set; }
    }

    public class Quickplay5
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive5
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Assists
    {
        public List<Quickplay5> quickplay { get; set; }
        public List<Competitive5> competitive { get; set; }
    }

    public class Quickplay6
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive6
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Average
    {
        public List<Quickplay6> quickplay { get; set; }
        public List<Competitive6> competitive { get; set; }
    }

    public class Quickplay7
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive7
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Miscellaneous
    {
        public List<Quickplay7> quickplay { get; set; }
        public List<Competitive7> competitive { get; set; }
    }

    public class Quickplay8
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive8
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Best
    {
        public List<Quickplay8> quickplay { get; set; }
        public List<Competitive8> competitive { get; set; }
    }

    public class Quickplay9
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Competitive9
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Game
    {
        public List<Quickplay9> quickplay { get; set; }
        public List<Competitive9> competitive { get; set; }
    }

    public class Stats
    {
        public TopHeroes top_heroes { get; set; }
        public Combat combat { get; set; }
        public Deaths deaths { get; set; }
        public MatchAwards match_awards { get; set; }
        public Assists assists { get; set; }
        public Average average { get; set; }
        public Miscellaneous miscellaneous { get; set; }
        public Best best { get; set; }
        public Game game { get; set; }
    }

    public class OWGameStatsRootObject
    {
        public string username { get; set; }
        public int level { get; set; }
        public string portrait { get; set; }
        public Stats stats { get; set; }
    }
}