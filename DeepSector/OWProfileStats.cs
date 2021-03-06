﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSector
{
    public class ProfileQuickplay
    {
        public int won { get; set; }
    }

    public class ProfileCompetitive
    {
        public int won { get; set; }
        public int lost { get; set; }
        public int draw { get; set; }
        public int played { get; set; }
    }

    public class Games
    {
        public ProfileQuickplay quickplay { get; set; }
        public ProfileCompetitive competitive { get; set; }
    }

    public class Playtime
    {
        public string quickplay { get; set; }
        public string competitive { get; set; }
    }

    public class ProfileCompetitive2
    {
        public int rank { get; set; }
        public string rank_img { get; set; }
    }

    public class OWProfileStatsRootObject
    {
        public string username { get; set; }
        public int level { get; set; }
        public string portrait { get; set; }
        public Games games { get; set; }
        public Playtime playtime { get; set; }
        public ProfileCompetitive2 competitive { get; set; }
        public string levelFrame { get; set; }
        public string star { get; set; }
    }
}
