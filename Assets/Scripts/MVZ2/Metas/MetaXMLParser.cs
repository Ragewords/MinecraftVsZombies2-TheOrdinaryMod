﻿using System.Xml;
using MVZ2.Modding;

namespace MVZ2.Metas
{
    public static class MetaXMLParser
    {
        public static void LoadMetaList(this ModResource resource, string metaPath, XmlDocument document, string defaultNsp)
        {
            switch (metaPath)
            {
                case "talkcharacters":
                    resource.TalkCharacterMetaList = TalkCharacterMetaList.FromXmlNode(document["characters"], defaultNsp);
                    break;
                case "sounds":
                    resource.SoundMetaList = SoundMetaList.FromXmlNode(document["sounds"], defaultNsp);
                    break;
                case "models":
                    resource.ModelMetaList = ModelMetaList.FromXmlNode(document["models"], defaultNsp);
                    break;
                case "fragments":
                    resource.FragmentMetaList = FragmentMetaList.FromXmlNode(document["fragments"]);
                    break;
                case "difficulties":
                    resource.DifficultyMetaList = DifficultyMetaList.FromXmlNode(document["difficulties"]);
                    break;
                case "entities":
                    resource.EntityMetaList = EntityMetaList.FromXmlNode(document["entities"], defaultNsp);
                    break;
                case "artifacts":
                    resource.ArtifactMetaList = ArtifactMetaList.FromXmlNode(document["artifacts"], defaultNsp);
                    break;
                case "almanac":
                    resource.AlmanacMetaList = AlmanacMetaList.FromXmlNode(document["almanac"], defaultNsp);
                    break;
                case "notes":
                    resource.NoteMetaList = NoteMetaList.FromXmlNode(document["notes"], defaultNsp);
                    break;
                case "stages":
                    resource.StageMetaList = StageMetaList.FromXmlNode(document["stages"], defaultNsp);
                    break;
                case "maps":
                    resource.MapMetaList = MapMetaList.FromXmlNode(document["maps"], defaultNsp);
                    break;
                case "areas":
                    resource.AreaMetaList = AreaMetaList.FromXmlNode(document["areas"], defaultNsp);
                    break;
                case "stats":
                    resource.StatMetaList = StatMetaList.FromXmlNode(document["stats"], defaultNsp);
                    break;
                case "achievements":
                    resource.AchievementMetaList = AchievementMetaList.FromXmlNode(document["achievements"], defaultNsp);
                    break;
                case "store":
                    resource.StoreMetaList = StoreMetaList.FromXmlNode(document["store"], defaultNsp);
                    break;
                case "archive":
                    resource.ArchiveMetaList = ArchiveMetaList.FromXmlNode(document["archive"], defaultNsp);
                    break;
                case "mainmenuviews":
                    resource.MainmenuViewMetaList = MainmenuViewMetaList.FromXmlNode(document["views"], defaultNsp);
                    break;
                case "musics":
                    resource.MusicMetaList = MusicMetaList.FromXmlNode(document["musics"], defaultNsp);
                    break;
                case "progressbars":
                    resource.ProgressBarMetaList = ProgressBarMetaList.FromXmlNode(document["bars"], defaultNsp);
                    break;
                case "blueprints":
                    resource.BlueprintMetaList = BlueprintMetaList.FromXmlNode(document["blueprints"], defaultNsp);
                    break;
                case "spawns":
                    resource.SpawnMetaList = SpawnMetaList.FromXmlNode(document["spawns"], defaultNsp);
                    break;
                case "chaptertransitions":
                    resource.ChapterTransitionMetaList = ChapterTransitionMetaList.FromXmlNode(document["transitions"], defaultNsp);
                    break;
                case "grids":
                    resource.GridMetaList = GridMetaList.FromXmlNode(document["grids"], defaultNsp);
                    break;
            }
        }
    }
}
