using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Xml.Linq;
using WorldOfTanks.Controllers;
using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.ViewModels;

namespace WorldOfTanks.Models.GamePlayModels
{
    public class Game
    {
        public string? GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastTime { get; set; }
        public Map? Map { get; set; }
        public List<PassiveMapElement>? MapElementList { get; set; }
        public List<Player>? PlayerList { get; set; }
        public List<Tank>? TankList { get; set; }
        public int NumOfTeams { get; set; }
        public int RebirthPoints { get; set; }
        public bool FriendlyFire { get; set; }
        public List<BGMapElement>? bgMapElements;
        public List<CVMapElement>? cvMapElements;
        public List<CVMapElement>? cvGameObjmapElements;
        public List<Shot>? shotsList;
        public List<MapPoint>? explosionList;
        public List<MapPoint>? fireExplosionList;
        public List<string>? removeCVMapElements;
        public Dictionary<string, int>? removeTankDict;
        public List<string>? removeCVGameObjmapElements;
        public List<string>? chatMessageList;
        public Dictionary<int, List<Player>>? teamPlayerDict;
        public List<int>? teamList;
        public int[,][]? arrMapBG;
        public int[,]? arrMapCVDefault;
        public int[,][]? arrMapCV;
        public int rpMin;

        public string RenderMap()
        {
            if (Map == null || TankList ==null || Map.MapPointListBG == null || Map.MapPointListCV == null || PlayerList == null || MapElementList == null) 
            {
                return "Error";
            }
            int num = -1;
            rpMin = 9999999;
            LastTime = DateTime.Now;
            bgMapElements = new List<BGMapElement>();
            cvMapElements = new List<CVMapElement>();
            cvGameObjmapElements= new List<CVMapElement>();
            shotsList = new List<Shot>();
            explosionList = new List<MapPoint>();
            fireExplosionList = new List<MapPoint>();
            removeCVMapElements = new List<string>();
            removeTankDict = new ();
            removeCVGameObjmapElements = new List<string>();
            teamPlayerDict = new Dictionary<int, List<Player>>();
            chatMessageList = new List<string>();
            teamList = new List<int>();
            foreach (var tank in TankList)
            {
                if (rpMin > tank.RebirthPoints)
                {
                    rpMin = tank.RebirthPoints;
                }
            }
            for (int i = 1; i <= NumOfTeams; i++)
            {
                teamPlayerDict.Add(i, new());
            }
            foreach (var player in PlayerList.ToList())
            {
                if (!teamList.Exists(u => u == player.Team)) 
                {
                    teamList.Add(player.Team);
                }
                player.GameDataJSON = new GameDataJSON()
                {
                    PJSON = new PJSON(),
                    PlayerListJSON = new List<PlayerJSON>(),
                    TeamVisionListJSON = new List<int>(),
                    PlayerShotListJSON = new List<MapPoint>(),
                    FireExplosionListJSON = new List<MapPoint>(),
                    ExplosionListJSON = new List<MapPoint>(),
                    RemoveCVListJSON = new List<string>(),
                };
                player.PlayerVision = new();
                player.CVVision = new();
                player.RemoveTankList = new();
                player.BeamList1 = new List<MapPoint>();
                player.RebirthPoints = RebirthPoints;
                player.PlayerNum = num;
                num--;
                CVMapElement cvTankME = new CVMapElement()
                {
                    Id = player.PlayerNum,
                    Index = player.PlayerNum,
                    Heath = 0,
                    Resp = false,
                    RespTeam = 0,
                    HQ = false,
                    HQTeam = 0,
                    BulletPermeability = false,
                    MachinePermeability = false,
                    Invulnerability = false,
                };
                cvMapElements.Add(cvTankME);
                CVMapElement cvShotME = new CVMapElement()
                {
                    Id = -1000000,
                    Index = player.PlayerNum,
                    Heath = 0,
                    Resp = false,
                    RespTeam = 0,
                    HQ = false,
                    HQTeam = 0,
                    BulletPermeability = false,
                    MachinePermeability = false,
                    Invulnerability = false,
                };
                cvMapElements.Add(cvShotME);
            }
            foreach (var p in PlayerList)
            {
                teamPlayerDict[p.Team].Add(p);
                foreach (var pl in PlayerList)
                {
                    PlayerJSON playerJSON = new PlayerJSON()
                    {
                        Name = pl.Name,
                        RebirthPoints = pl.RebirthPoints,
                        Kills = pl.Kills,
                        Death = pl.Death,
                        DamageSum = pl.DamageSum,
                        FriendlyFire = pl.DamageFriendly,
                        TankId = 0,
                        X = pl.X,
                        Y = pl.Y,
                        Direction = pl.Direction,
                        DirectionTower = pl.DirectionTower,
                    };
                    p.GameDataJSON!.PlayerListJSON!.Add(playerJSON);
                }
            }
            foreach (var mapElement in MapElementList.ToList()) 
            {
                if (mapElement.Background)
                {
                    BGMapElement bgME = new BGMapElement()
                    {
                        Id = mapElement.PasMapElementId,
                        Viscosity = mapElement.Viscosity,
                        BulletPermeability = mapElement.BulletPermeability,
                        MachinePermeability = mapElement.MachinePermeability,
                    };
                    bgMapElements.Add(bgME);
                }
                else
                {
                    CVMapElement cvME = new CVMapElement()
                    {
                        Id = mapElement.PasMapElementId,
                        Heath = mapElement.Heath,
                        Resp = mapElement.Resp,
                        RespTeam = mapElement.RespTeam,
                        HQ = mapElement.HQ,
                        HQTeam = mapElement.HQTeam,
                        BulletPermeability = mapElement.BulletPermeability,
                        MachinePermeability = mapElement.MachinePermeability,
                        Invulnerability = mapElement.Invulnerability,
                    };
                    cvMapElements.Add(cvME);
                }
            }
            BGMapElement bgME0 = new BGMapElement()
            {
                Id = 0,
                Viscosity = 0,
                BulletPermeability = false,
                MachinePermeability = false,
            };
            bgMapElements.Add(bgME0);
            CVMapElement cvME0 = new CVMapElement()
            {
                Id = 0,
                Heath = 0,
                Resp = false,
                RespTeam = 0,
                HQ = false,
                HQTeam = 0,
                BulletPermeability = true,
                MachinePermeability = true,
                Invulnerability = true,
            };
            cvMapElements.Add(cvME0);
            string val;
            int meId;
            int width = Map.Width;
            int height = Map.Height;
            int w = width/20;
            int h = height/20;
            arrMapBG = new int[height + 40, width + 40][] ;
            arrMapCVDefault = new int[height + 40, width + 40];
            arrMapCV = new int[height + 40, width + 40][];
            for (int j = 0; j < height + 40; j++)
            {
                for (int f = 0; f < width + 40; f++) 
                {
                    arrMapBG[j, f] = new int[] { 0, 0 };
                    arrMapCV[j, f] = new int[] { 0, 0 };
                }
            }
            Dictionary<string, string>? meBGDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Map.MapPointListBG);
            Dictionary<string, string>? meCVDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Map.MapPointListCV);
            if (meBGDict == null || meCVDict == null)
            {
                return "Error";
            }
            foreach (var item in meBGDict)
            {
                if (item.Value=="")
                {
                    return"Error";
                }
                else
                {
                    val = item.Value;
                }
                meId = Convert.ToInt32(item.Key.Replace("bgme", ""));
                BGMapElement? bgME = bgMapElements.FirstOrDefault(u => u.Id == Convert.ToInt32(val));
                if (bgME == null)
                {
                    return "Error";
                }
                for (int j = 0; j < 20; j++)
                {
                    for (int f = 0; f < 20; f++)
                    {
                        arrMapBG[(meId / w * 20 + j) + 20, (meId % w * 20 + f) + 20][0] = Convert.ToInt32(val);
                        arrMapBG[(meId / w * 20 + j) + 20, (meId % w * 20 + f) + 20][1] = meId;

                    }
                }
            }
            foreach (var item in meCVDict)
            {
                if (item.Value == "")
                {
                    continue;
                }
                else
                {
                    val = item.Value;
                }
                meId = Convert.ToInt32(item.Key.Replace("cvme", ""));
                CVMapElement? cvME = cvMapElements.FirstOrDefault(u => u.Id == Convert.ToInt32(val));
                if (cvME == null) 
                {
                    return "Error";
                }
                CVMapElement cvGOME = new CVMapElement()
                {
                    Id = cvME.Id,
                    Index = meId,
                    Heath = cvME.Heath,
                    Resp = cvME.Resp,
                    RespTeam = cvME.RespTeam,
                    HQ = cvME.HQ,
                    HQTeam = cvME.HQTeam,
                    BulletPermeability = cvME.BulletPermeability,
                    MachinePermeability = cvME.MachinePermeability,
                    Invulnerability = cvME.Invulnerability,
                };
                cvGameObjmapElements.Add(cvGOME);
                for (int j = 0; j < 20; j++)
                {
                    for (int f = 0; f < 20; f++)
                    {
                        arrMapCVDefault[(meId / w * 20 + j) + 20, (meId % w * 20 + f) + 20] = Convert.ToInt32(val);
                        arrMapCV[(meId / w * 20 + j) + 20, (meId % w * 20 + f) + 20][0] = Convert.ToInt32(val);
                        arrMapCV[(meId / w * 20 + j) + 20, (meId % w * 20 + f) + 20][1] = meId;
                    }
                }
            }
            ShotOn();
            return "Ok";
        }

        public void AddMessage(string? message)
        {
            if (message != null)
            {
                if (chatMessageList!.Count > 50)
                {
                    chatMessageList.Remove(chatMessageList[0]);
                }
                chatMessageList!.Add(message);
            }
        }

        public void AddRP(Player player, int rp, string name)
        {
            Player? pl = PlayerList!.FirstOrDefault(u => u.Name == name);
            if (pl != null && player.RebirthPoints >= rp)
            {
                player.RebirthPoints -= rp;
                pl.RebirthPoints += rp;
            }
        }

        public void DeletePlayer(Player player)
        {
            if (cvGameObjmapElements == null || cvMapElements == null || PlayerList == null || arrMapCV == null)  
            {
                return;
            }
            if (player.TankPoint != null) 
            {
                foreach (var point in player.TankPoint)
                {
                    arrMapCV[point[1] + 20, point[0] + 20][0] = 0;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
                }
            }
            cvGameObjmapElements.RemoveAll(u => u.Index == player.PlayerNum);
            cvMapElements.RemoveAll(u => u.Index == player.PlayerNum);
            player.Tank = null;
            player.RebirthPoints = -9999999;
        }

        public string SelectTank(int x, int y, int direction, double directionDouble, int tankId, Player? player)
        {
            int count = 0;
            CVMapElement? cvMapElementResp;
            CVMapElement? cvMapElement;
            if (player == null || player.Tank != null || cvMapElements == null || arrMapCVDefault == null || arrMapCV == null || TankList == null) 
            {
                return "Error";
            }
            List<MapPoint> mapPointRespTank = NeighbourhoodPoint(x, y, direction);
            foreach (MapPoint point in mapPointRespTank)
            {
                cvMapElementResp = cvMapElements.FirstOrDefault(u => u.Id == arrMapCVDefault[point.Y + 20, point.X + 20]);
                cvMapElement = cvMapElements.FirstOrDefault(u => u.Id == arrMapCV[point.Y + 20, point.X + 20][0]);
                if (cvMapElementResp == null || cvMapElement == null)
                {
                    return "Error";
                }
                if (cvMapElementResp.RespTeam == player.Team && cvMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    return "Error";
                }
            }
            if (count == 9)
            {
                player.X = x;
                player.XDouble = x;
                player.Y = y;
                player.YDouble = y;
                player.Direction = direction;
                player.DirectionDouble = directionDouble;
                Tank? tank = TankList.FirstOrDefault(u => u.TankId == tankId);
                if (tank == null)
                {
                    return "Error";
                }
                if (player.RebirthPoints < tank.RebirthPoints)
                {
                    return "Error";
                }
                else
                {
                    player.RebirthPoints -= tank.RebirthPoints;
                }
                player.Tank = new Tank()
                {
                    TankId = tankId,
                    Name = tank.Name,
                    SpeedTank = tank.SpeedTank,
                    SpeedUp = tank.SpeedUp,
                    SpeedRotation = tank.SpeedRotation,
                    Health = tank.Health,
                    Weapon = tank.Weapon,
                };
                player.TankPoint = TankPoint(mapPointRespTank);
                foreach (var point in player.TankPoint)
                {
                    arrMapCV[point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                player.TankPointClear = false;
                player.Invulnerability = true;
                player.RespTime = DateTime.Now;
                return "Ok";
            }
            return "Error";
        }

        private void Vision(Player player)
        {
            int team = player.Team;
            int idCV1;
            int idBG1;
            int index1;
            int indexY1;
            int indexX1;
            CVMapElement? cVMapElement1;
            BGMapElement? bGMapElement1;
            Player? playerEnemy;
            double delta = 90;//delta%2=0
            int range = 50;
            double alfa0;
            List<int> playerVision = new List<int>();
            List<int> cvVision = new List<int>();
            player.BeamList1!.Clear();
            alfa0 = player.DirectionTower - delta / 2;
            for (int i = 0; i < delta; i++)
            {
                MapPoint beam1 = Beam(player.X, player.Y, alfa0, 22);
                player.BeamList1!.Add(beam1);
                alfa0 += 1;
            }
            for (int i = 0; i < delta; i++)
            {
                MapPoint bm1 = player.BeamList1[i];
                for (int k = 0; k < range; k++)
                {
                    indexY1 = bm1.Y;
                    indexX1 = bm1.X;
                    idCV1 = arrMapCV![indexY1 + 20, indexX1 + 20][0];
                    index1 = arrMapBG![indexY1 + 20, indexX1 + 20][1];
                    idBG1 = arrMapBG[indexY1 + 20, indexX1 + 20][0];
                    cVMapElement1 = cvMapElements!.FirstOrDefault(u => u.Id == idCV1);
                    bGMapElement1 = bgMapElements!.FirstOrDefault(u => u.Id == idBG1);
                    if (bGMapElement1 == null || cVMapElement1 == null) 
                    {
                        break;
                    }
                    if (idCV1 >= 0)
                    {
                        if (bGMapElement1.BulletPermeability && cVMapElement1.BulletPermeability) 
                        {
                            if (!cvVision.Exists(u => u == index1)) 
                            {
                                cvVision.Add(index1);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (idCV1 > -1000000 && idCV1 < 0) 
                    {
                        playerEnemy = PlayerList!.FirstOrDefault(u => u.PlayerNum == idCV1);
                        if (playerEnemy != null && playerEnemy.Team != team)
                        {
                            if (playerEnemy.Tank == null && !playerEnemy.TankPointClear) 
                            {
                                foreach (var item in arrMapCV!)
                                {
                                    if (item[1] == playerEnemy.PlayerNum)
                                    {
                                        item[0] = 0;
                                        item[1] = 0;
                                    }
                                }
                                playerEnemy.TankPointClear = true;
                            }
                            else
                            {
                                if (!playerVision.Exists(u => u == idCV1))
                                {
                                    playerVision.Add(idCV1);
                                }
                            }
                        }
                    }
                    BeamVoid(bm1, 5);
                }
            }
            player.PlayerVision = playerVision;
            player.CVVision = cvVision;
        }

        private static MapPoint Beam(double x, double y, double directionTower, double step)
        {
            double xShot = x + step * Math.Cos(directionTower * Math.PI / 180);
            double yShot = y + step * Math.Sin(directionTower * Math.PI / 180);
            MapPoint mapPoint= new()
            {
                XDouble = xShot,
                YDouble = yShot,
                X = (int)Math.Round(xShot),
                Y = (int)Math.Round(yShot),
                Direction = (int)directionTower,
            };
            return mapPoint;
        }
        private static void BeamVoid(MapPoint mapPoint, double step)
        {
            mapPoint.XDouble += step * Math.Cos(mapPoint.Direction * Math.PI / 180);
            mapPoint.YDouble += step * Math.Sin(mapPoint.Direction * Math.PI / 180);
            mapPoint.X = (int)Math.Round(mapPoint.XDouble);
            mapPoint.Y = (int)Math.Round(mapPoint.YDouble);
        }

        private static int[,][] TankPoint(List<MapPoint> mapPointRespTank)
        {
            int dx, dy, dxb, dyb, delta, deltaX, deltaY;
            double k1, k2, k3, k4, x0, y0, x, y, dxBase, dyBase;
            dxBase = mapPointRespTank[3].XDouble - mapPointRespTank[1].XDouble;
            dyBase = mapPointRespTank[3].YDouble - mapPointRespTank[1].YDouble;
            dxb = mapPointRespTank[3].X - mapPointRespTank[1].X;
            dyb = mapPointRespTank[3].Y - mapPointRespTank[1].Y;
            dx = dxb;
            dy = dyb;
            x0 = mapPointRespTank[1].X;
            y0 = mapPointRespTank[1].Y;
           
            if (dxBase <= 0 && dyBase > 0)
            {
                x0 = mapPointRespTank[7].X;
                y0 = mapPointRespTank[7].Y;
                dx=Math.Abs(dyb);
                dy=Math.Abs(dxb);
            }

            if (dxBase < 0 && dyBase <= 0)
            {
                x0 = mapPointRespTank[5].X;
                y0 = mapPointRespTank[5].Y;
                dx = Math.Abs(dxb);
                dy = Math.Abs(dyb);
            }

            if (dxBase >= 0 && dyBase < 0)
            {
                x0 = mapPointRespTank[3].X;
                y0 = mapPointRespTank[3].Y;
                dx = Math.Abs(dyb);
                dy = Math.Abs(dxb);
            }

            if (Math.Abs(dxBase) >= Math.Abs(dyBase))
            {
                delta = Math.Abs(dxb);
            }
            else
            {
                delta = Math.Abs(dyb);
            }
            if (dxBase ==0||dyBase ==0)
            {
                k1 = 0;
                k2 = 0;
                k3 = 0;
                k4 = 0;
            }
            else
            {
                k1 = (double)dy / (double)dx;
                k2 = -1 * k1;
                k3 = 1 / k1;
                k4 = -1 / k2;
            }
            x = x0;
            y = y0;
            deltaX = 1;
            deltaY = 1;
            int[,][] tankArr = new int[Math.Abs(delta + 1), Math.Abs(delta + 1)][];
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                for (int i = 0; i < Math.Abs(delta + 1); i++)
                {
                    for (int j = 0; j < Math.Abs(delta + 1); j++)
                    {
                        tankArr[i, j] = new int[] { (int)x, (int)y };
                        y = Math.Round(k1 * deltaX + y0);
                        deltaX++;
                        x++;
                    }
                    deltaX = 1;
                    x = Math.Round(k2 * deltaY + x0);
                    y0++;
                    y = y0;
                    deltaY++;
                }
            }
            else
            {
                for (int i = 0; i < Math.Abs(delta + 1); i++)
                {
                    for (int j = 0; j < Math.Abs(delta + 1); j++)
                    {
                        tankArr[i, j] = new int[] { (int)x, (int)y };
                        x = Math.Round(k3 * deltaY + x0);
                        deltaY++;
                        y++;
                    }
                    deltaY = 1;
                    y = Math.Round(k4 * deltaX + y0);
                    x0--;
                    x = x0;
                    deltaX++;
                }
            }
            return tankArr;
        }

        private static List<MapPoint> NeighbourhoodPoint(int x, int y, int direction)
        {
            direction = direction - 90;
            int directionA = direction - 45;
            double side = 14;
            double sideA = 19;//19.79899;
            double angle = direction * Math.PI / 180;
            double angleA = directionA * Math.PI / 180;
            List<MapPoint> mapPointList = new List<MapPoint>()
            {
                new MapPoint()
                {
                    XDouble = x,
                    YDouble = y,
                    X = x,
                    Y = y,
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA),
                    YDouble = y + sideA * Math.Sin(angleA),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA)),
                    Direction= (int)(angleA/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle),
                    YDouble = y + side * Math.Sin(angle),
                    X = (int)Math.Round(x + side * Math.Cos(angle)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle)),
                    Direction= (int)(angle/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI / 2),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI / 2),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI / 2)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI / 2)),
                    Direction= (int)((angleA + Math.PI / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI / 2),
                    YDouble = y + side * Math.Sin(angle + Math.PI / 2),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI / 2)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI / 2)),
                    Direction= (int)((angle + Math.PI / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI)),
                    Direction= (int)((angleA + Math.PI)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI),
                    YDouble = y + side * Math.Sin(angle + Math.PI),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI)),
                    Direction= (int)((angle + Math.PI)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI * 3 / 2),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI * 3 / 2),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI * 3 / 2)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI * 3 / 2)),
                    Direction= (int)((angleA + Math.PI * 3 / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI * 3 / 2),
                    YDouble = y + side * Math.Sin(angle + Math.PI * 3 / 2),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI * 3 / 2)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI * 3 / 2)),
                    Direction= (int)((angle + Math.PI * 3 / 2)/Math.PI*180),
                },
            };
            return mapPointList;
        }

        private static List<MapPoint> NeighbourhoodUpDownPoint(double x, double y, int direction, bool down, double step)
        {
            direction = direction - 90;
            int directionA = direction - 45;
            double delta = step;
            if (down)
            {
                delta = -step;
            }
            double side = 14;
            double sideA = 19;
            double angle = direction * Math.PI / 180;
            double angleA = directionA * Math.PI / 180;
            x = x + delta * Math.Cos(angle);
            y = y + delta * Math.Sin(angle);
            List<MapPoint> mapPointList = new List<MapPoint>()
            {
                new MapPoint()
                {
                    XDouble = x,
                    YDouble = y,
                    X = (int)Math.Round(x),
                    Y = (int)Math.Round(y),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA),
                    YDouble = y + sideA * Math.Sin(angleA),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA)),
                    Direction= (int)(angleA/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle),
                    YDouble = y + side * Math.Sin(angle),
                    X = (int)Math.Round(x + side * Math.Cos(angle)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle)),
                    Direction= (int)(angle/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI / 2),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI / 2),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI / 2)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI / 2)),
                    Direction= (int)((angleA + Math.PI / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI / 2),
                    YDouble = y + side * Math.Sin(angle + Math.PI / 2),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI / 2)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI / 2)),
                    Direction= (int)((angle + Math.PI / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI)),
                    Direction= (int)((angleA + Math.PI)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI),
                    YDouble = y + side * Math.Sin(angle + Math.PI),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI)),
                    Direction= (int)((angle + Math.PI)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + sideA * Math.Cos(angleA + Math.PI * 3 / 2),
                    YDouble = y + sideA * Math.Sin(angleA + Math.PI * 3 / 2),
                    X = (int)Math.Round(x + sideA * Math.Cos(angleA + Math.PI * 3 / 2)),
                    Y = (int)Math.Round(y + sideA * Math.Sin(angleA + Math.PI * 3 / 2)),
                    Direction= (int)((angleA + Math.PI * 3 / 2)/Math.PI*180),
                },
                new MapPoint()
                {
                    XDouble = x + side * Math.Cos(angle + Math.PI * 3 / 2),
                    YDouble = y + side * Math.Sin(angle + Math.PI * 3 / 2),
                    X = (int)Math.Round(x + side * Math.Cos(angle + Math.PI * 3 / 2)),
                    Y = (int)Math.Round(y + side * Math.Sin(angle + Math.PI * 3 / 2)),
                    Direction= (int)((angle + Math.PI * 3 / 2)/Math.PI*180),
                },
            };
            return mapPointList;
        }

        private void RefreshPlayerListJSON(Player p)
        {
            PlayerJSON playerJSON;
            List<int> playerVision = new();
            List<int> cvVision = new();
            foreach (var item in teamPlayerDict![p.Team].ToList())
            {
                playerVision.AddRange(item.PlayerVision!.ToList());
                cvVision.AddRange(item.CVVision!.ToList());
            }
            playerVision = playerVision.Distinct().ToList();
            p.GameDataJSON!.TeamVisionListJSON = cvVision.Distinct().ToList();
            foreach (var pl in PlayerList!.ToList())
            {
                playerJSON = p.GameDataJSON!.PlayerListJSON!.FirstOrDefault(u => u.Name == pl.Name)!;
                playerJSON.Invulnerability = pl.Invulnerability;
                playerJSON.RebirthPoints = pl.RebirthPoints;
                playerJSON.Kills = pl.Kills;
                playerJSON.Death = pl.Death;
                playerJSON.DamageSum = pl.DamageSum;
                playerJSON.FriendlyFire = pl.DamageFriendly;
                if (pl.Tank != null && pl.Team == p.Team) 
                {
                    playerJSON.TankId = pl.Tank.TankId;
                    playerJSON.X = pl.X;
                    playerJSON.Y = pl.Y;
                    playerJSON.Direction = pl.Direction;
                    playerJSON.DirectionTower = pl.DirectionTower;
                }
                else
                {
                    if (pl.Tank != null && playerVision!.Exists(u => u == pl.PlayerNum))
                    {
                        playerJSON.TankId = pl.Tank.TankId;
                        playerJSON.X = pl.X;
                        playerJSON.Y = pl.Y;
                        playerJSON.Direction = pl.Direction;
                        playerJSON.DirectionTower = pl.DirectionTower;
                    }
                    else
                    {
                        if (p.RemoveTankList!.ContainsKey(pl.Name!))
                        {
                            playerJSON.TankId = p.RemoveTankList![pl.Name!];
                            playerJSON.X = pl.X;
                            playerJSON.Y = pl.Y;
                            playerJSON.Direction = pl.Direction;
                            playerJSON.DirectionTower = pl.DirectionTower;
                        }
                        else
                        {
                            playerJSON.TankId = 0;
                            playerJSON.X = 0;
                            playerJSON.Y = 0;
                            playerJSON.Direction = 0;
                            playerJSON.DirectionTower = 0;
                        }
                    }
                }
            }
        }

        private void RefreshPlayerDefeatVictoryListJSON(Player p)
        {
            PlayerJSON playerJSON;
            List<int> cvVision = new();
            if (p.LostVictory != 1)
            {
                foreach (var item in PlayerList!.ToList())
                {
                    cvVision.AddRange(item.CVVision!.ToList());
                }
                p.GameDataJSON!.TeamVisionListJSON = cvVision.Distinct().ToList();
            }
            else
            {
                p.GameDataJSON!.TeamVisionListJSON!.Clear();
            }
            foreach (var pl in PlayerList!.ToList())
            {
                playerJSON = p.GameDataJSON!.PlayerListJSON!.FirstOrDefault(u => u.Name == pl.Name)!;
                playerJSON.Invulnerability = pl.Invulnerability;
                playerJSON.RebirthPoints = pl.RebirthPoints;
                playerJSON.Kills = pl.Kills;
                playerJSON.Death = pl.Death;
                playerJSON.DamageSum = pl.DamageSum;
                playerJSON.FriendlyFire = pl.DamageFriendly;
                if (pl.Tank != null)
                {
                    playerJSON.TankId = pl.Tank.TankId;
                    playerJSON.X = pl.X;
                    playerJSON.Y = pl.Y;
                    playerJSON.Direction = pl.Direction;
                    playerJSON.DirectionTower = pl.DirectionTower;
                }
                else
                {
                    if (p.RemoveTankList!.ContainsKey(pl.Name!))
                    {
                        playerJSON.TankId = p.RemoveTankList![pl.Name!];
                        playerJSON.X = pl.X;
                        playerJSON.Y = pl.Y;
                        playerJSON.Direction = pl.Direction;
                        playerJSON.DirectionTower = pl.DirectionTower;
                    }
                    else
                    {
                        playerJSON.TankId = 0;
                        playerJSON.X = 0;
                        playerJSON.Y = 0;
                        playerJSON.Direction = 0;
                        playerJSON.DirectionTower = 0;
                    }
                }
            }
        }

        public string GetGameData(Player player)
        {
            TimeSpan timeSpan;
            player.OnGet = true;
            if (player.Tank != null)
            {
                if (player.Invulnerability)
                {
                    timeSpan = DateTime.Now - player.RespTime;
                    if (timeSpan.TotalSeconds > 10)
                    {
                        player.Invulnerability = false;
                    }
                }
                player.GameDataJSON!.PJSON!.Health = player.Tank.Health;
                player.GameDataJSON.PJSON.FireReady = player.FireReady;
                Vision(player);
            }
            else
            {
                
                
                if (player.LostVictory != -1)
                {
                    DefeatTeam(player.Team);
                }
                player.GameDataJSON!.PJSON!.Health = 0;
            }
            if (player.LostVictory != 0)
            {
                RefreshPlayerDefeatVictoryListJSON(player);
            }
            else
            {
                RefreshPlayerListJSON(player);
            }
            string gdata = System.Text.Json.JsonSerializer.Serialize(player.GameDataJSON);
            player.RemoveTankList!.Clear();
            player.GameDataJSON!.FireExplosionListJSON!.Clear();
            player.GameDataJSON.ExplosionListJSON!.Clear();
            player.GameDataJSON.RemoveCVListJSON!.Clear();
            return gdata;
        }

        private bool ValidMoveStreif(Player player, double[] vectorSum, double k)
        {
            int idCV;
            int idBG;
            int indexY;
            int indexX;
            CVMapElement? cVMapElement;
            BGMapElement? bGMapElement;
            double modulV = Math.Sqrt(vectorSum[0] * vectorSum[0] + vectorSum[1] * vectorSum[1]);
            double XDouble = player.XDouble - vectorSum[0] / modulV * k;
            double YDouble = player.YDouble - vectorSum[1] / modulV * k;
            List<MapPoint> mpList = NeighbourhoodUpDownPoint(XDouble, YDouble, player.Direction, false, 0);
            int[,][] tp = TankPoint(mpList);
            int count = 0;
            int row = tp.GetUpperBound(0);
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[0, i][1];
                indexX = tp[0, i][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0] ;
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null) 
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[i, row][1];
                indexX = tp[i, row][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[row, i][1];
                indexX = tp[row, i][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[i, 0][1];
                indexX = tp[i, 0][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count == (row + 1) * 4)
            {
                player.XDouble = mpList[0].XDouble;
                player.YDouble = mpList[0].YDouble;
                player.X = mpList[0].X;
                player.Y = mpList[0].Y;
                player.TankPoint = tp;
                foreach (var point in player.TankPoint)
                {
                    arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                return true;
            }
            else
            {
                foreach (var point in player.TankPoint!)
                {
                    arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                return false;
            }
        }

        private bool ValidMoveUp(Player player, double step)
        {
            double k = 0.75;
            int idCV;
            int idBG;
            int indexY;
            int indexX;
            CVMapElement? cVMapElement;
            BGMapElement? bGMapElement;
            List<double[]> vector = new List<double[]>();
            List<double[]> vectorRemove = new List<double[]>();
            List<int> vectorIndRemove = new List<int>();
            double[] vectorSum = new double[2];
            List<MapPoint> mpList = NeighbourhoodUpDownPoint(player.XDouble, player.YDouble, player.Direction, false, step);
            double[] vectorUp = new double[] { mpList[2].X - mpList[0].X, mpList[2].Y - mpList[0].Y };
            int[,][] tp = TankPoint(mpList);
            int count = 0;
            int row = tp.GetUpperBound(0);
            foreach (var point in player.TankPoint!)
            {
                arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
            }
            if (player.Direction >= 0 && player.Direction < 90)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[0, i][1];
                    indexX = tp[0, i][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 90 && player.Direction < 180)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[i, row][1];
                    indexX = tp[i, row][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 180 && player.Direction < 270)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[row, i][1];
                    indexX = tp[row, i][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 270 && player.Direction < 360)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[i, 0][1];
                    indexX = tp[i, 0][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (count == row + 1)
            {
                player.XDouble = mpList[0].XDouble;
                player.YDouble = mpList[0].YDouble;
                player.X = mpList[0].X;
                player.Y = mpList[0].Y;
                player.TankPoint = tp;
                foreach (var point in player.TankPoint)
                {
                    arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                return true;
            }
            else
            {
                player.Speed = 0;
                for (int i = 0; i < vector.Count(); i++)
                {
                    for (int j = i + 1; j < vector.Count(); j++)
                    {
                        if (Enumerable.SequenceEqual(vector[i], vector[j]))
                        {
                            vectorIndRemove.Add(j);
                        }
                    }
                }
                vectorIndRemove = vectorIndRemove.Distinct().ToList();
                foreach (var ind in vectorIndRemove)
                {
                    vectorRemove.Add(vector[ind]);
                }
                foreach (var vr in vectorRemove)
                {
                    vector.Remove(vr);
                }
                foreach (var vec in vector)
                {
                    vectorSum[0] = vectorSum[0] + vec[0];
                    vectorSum[1] = vectorSum[1] + vec[1];
                }
                vectorSum[0] = vectorSum[0] - vectorUp[0] * 4;
                vectorSum[1] = vectorSum[1] - vectorUp[1] * 4;
                if (vectorSum[0] == 0 && vectorSum[1] == 0)
                {
                    foreach (var point in player.TankPoint)
                    {
                        arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                        arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                    }
                }
                else
                {
                    if (ValidMoveStreif(player, vectorSum, k))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool ValidMoveDown(Player player, double step)
        {
            double k = 0.75;
            int idCV;
            int idBG;
            int indexY;
            int indexX;
            CVMapElement? cVMapElement;
            BGMapElement? bGMapElement;
            List<double[]> vector = new List<double[]>();
            List<double[]> vectorRemove = new List<double[]>();
            List<int> vectorIndRemove = new List<int>();
            double[] vectorSum = new double[2];
            List<MapPoint> mpList = NeighbourhoodUpDownPoint(player.XDouble, player.YDouble, player.Direction, true, step);
            double[] vectorDown = new double[] { mpList[6].X - mpList[0].X, mpList[6].Y - mpList[0].Y };
            int[,][] tp = TankPoint(mpList);
            int count = 0;
            int row = tp.GetUpperBound(0);
            foreach (var point in player.TankPoint!)
            {
                arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
            }
            if (player.Direction >= 0 && player.Direction < 90)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[row, i][1];
                    indexX = tp[row, i][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 90 && player.Direction < 180)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[i, 0][1];
                    indexX = tp[i, 0][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 180 && player.Direction < 270)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[0, i][1];
                    indexX = tp[0, i][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (player.Direction >= 270 && player.Direction < 360)
            {
                for (int i = 0; i <= row; i++)
                {
                    indexY = tp[i, row][1];
                    indexX = tp[i, row][0];
                    idCV = arrMapCV![indexY + 20, indexX + 20][0];
                    idBG = arrMapBG![indexY + 20, indexX + 20][0];
                    cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                    bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                    if (cVMapElement == null || bGMapElement == null)
                    {
                        break;
                    }
                    if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                    {
                        count++;
                    }
                    else
                    {
                        vector.Add(new double[] { indexX - mpList[0].X, indexY - mpList[0].Y });
                        break;
                    }
                }
            }
            if (count == row + 1)
            {
                player.XDouble = mpList[0].XDouble;
                player.YDouble = mpList[0].YDouble;
                player.X = mpList[0].X;
                player.Y = mpList[0].Y;
                player.TankPoint = tp;
                foreach (var point in player.TankPoint)
                {
                    arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                return true;
            }
            else
            {
                player.Speed = 0;
                for (int i = 0; i < vector.Count(); i++)
                {
                    for (int j = i + 1; j < vector.Count(); j++)
                    {
                        if (Enumerable.SequenceEqual(vector[i], vector[j]))
                        {
                            vectorIndRemove.Add(j);
                        }
                    }
                }
                vectorIndRemove = vectorIndRemove.Distinct().ToList();
                foreach (var ind in vectorIndRemove)
                {
                    vectorRemove.Add(vector[ind]);
                }
                foreach (var vr in vectorRemove)
                {
                    vector.Remove(vr);
                }
                foreach (var vec in vector)
                {
                    vectorSum[0] = vectorSum[0] + vec[0];
                    vectorSum[1] = vectorSum[1] + vec[1];
                }
                vectorSum[0] = vectorSum[0] - vectorDown[0] * 4;
                vectorSum[1] = vectorSum[1] - vectorDown[1] * 4;
                if (vectorSum[0] == 0 && vectorSum[1] == 0)
                {
                    foreach (var point in player.TankPoint)
                    {
                        arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                        arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                    }
                }
                else
                {
                    if (ValidMoveStreif(player, vectorSum, k))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool ValidMoveLeftRight(Player player, double step)
        {
            double k = 1.4;
            int idCV;
            int idBG;
            int indexY;
            int indexX;
            CVMapElement? cVMapElement;
            BGMapElement? bGMapElement;
            List<double[]> vector = new List<double[]>();
            List<double[]> vectorRemove = new List<double[]>();
            List<int> vectorIndRemove = new List<int>();
            double[] vectorSum = new double[2];
            double directionDouble = player.DirectionDouble;
            player.DirectionDouble = player.DirectionDouble + step;
            int direction = (int)player.DirectionDouble;
            List<MapPoint> mp = NeighbourhoodPoint(player.X, player.Y, direction);
            int count = 0;
            foreach (var point in player.TankPoint!)
            {
                arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
            }
            int[,][] tp = TankPoint(mp);
            int row = tp.GetUpperBound(0);
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[0, i][1];
                indexX = tp[0, i][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    vector.Add(new double[] { indexX - mp[0].X, indexY - mp[0].Y });
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[i, row][1];
                indexX = tp[i, row][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    vector.Add(new double[] { indexX - mp[0].X, indexY - mp[0].Y });
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[row, i][1];
                indexX = tp[row, i][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    vector.Add(new double[] { indexX - mp[0].X, indexY - mp[0].Y });
                    break;
                }
            }
            for (int i = 0; i <= row; i++)
            {
                indexY = tp[i, 0][1];
                indexX = tp[i, 0][0];
                idCV = arrMapCV![indexY + 20, indexX + 20][0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                cVMapElement = cvMapElements!.FirstOrDefault(u => u.Id == idCV);
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (cVMapElement == null || bGMapElement == null)
                {
                    break;
                }
                if (cVMapElement.MachinePermeability && bGMapElement.MachinePermeability)
                {
                    count++;
                }
                else
                {
                    vector.Add(new double[] { indexX - mp[0].X, indexY - mp[0].Y });
                    break;
                }
            }
            if (count == (row + 1) * 4)
            {
                player.Direction = direction;
                player.TankPoint = tp;
                foreach (var point in player.TankPoint)
                {
                    arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                    arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                }
                return true;
            }
            else
            {
                for (int i = 0; i < vector.Count(); i++)
                {
                    for (int j = i+1; j < vector.Count(); j++)
                    {
                        if (Enumerable.SequenceEqual(vector[i], vector[j])) 
                        {
                            vectorIndRemove.Add(j);
                        }
                    }
                }
                vectorIndRemove = vectorIndRemove.Distinct().ToList();
                foreach (var ind in vectorIndRemove)
                {
                    vectorRemove.Add(vector[ind]);
                }
                foreach (var vr in vectorRemove)
                {
                    vector.Remove(vr);
                }
                foreach (var vec in vector)
                {
                    vectorSum[0] = vectorSum[0] + vec[0];
                    vectorSum[1] = vectorSum[1] + vec[1];
                }
                if (vectorSum[0] == 0 && vectorSum[1] == 0)
                {
                    foreach (var point in player.TankPoint)
                    {
                        arrMapCV![point[1] + 20, point[0] + 20][0] = player.PlayerNum;
                        arrMapCV[point[1] + 20, point[0] + 20][1] = player.PlayerNum;
                    }
                }
                else
                {
                    if (ValidMoveStreif(player, vectorSum, k))
                    {
                        return true;
                    }
                }
                player.DirectionDouble = directionDouble;
                return false;
            }
        }

        public void TankMove(int w, int s, int a, int d, int mbtn, int directionTower, Player player, double timeSpan)
        {
            if (w != 0 || s != 0 || a != 0 || d != 0 || mbtn != 0 || player.DirectionTower != directionTower) 
            {
                player.DirectionTower = directionTower;
                LastTime = DateTime.Now;
            }
            if (player.Tank == null || player.Tank.Weapon == null)
            {
                return;
            }
            player.OnMove = true;
            double firingRate;
            int left = a;
            int right = d;
            double step;
            double stepSum = 0;
            double deltaTimeBase = 1;
            double timeStep = timeSpan / deltaTimeBase;
            double boost;
            TimeSpan tSp = DateTime.Now - player.ShotTime;
            firingRate = 60000 / (double)player.Tank.Weapon.FiringRate;
            if (tSp.TotalMilliseconds >= firingRate)
            {
                player.FireReady = true;
            }
            else
            {
                player.FireReady = false;
            }
            if (w > s)
            {
                for (int i = 0; i < timeStep; i++)
                {
                    boost = (player.Tank.SpeedTank - player.Speed) * player.Tank.SpeedUp / 10000;
                    if (boost <= 0)
                    {
                        boost = 0;
                    }
                    player.Speed = player.Speed + boost;
                    step = player.Speed * 1 / 500;
                    step = StepCorrection(player, step);
                    stepSum += step;
                }
                do
                {
                    if (stepSum < 2)
                    {
                        step = stepSum;
                    }
                    else
                    {
                        step = 2;
                    }
                    stepSum -= 2;
                    if (step < 0)
                    {
                        if (!ValidMoveDown(player, -step))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!ValidMoveUp(player, step))
                        {
                            break;
                        }
                    }
                } while (stepSum > 0);
            }
            if (s > w)
            {
                right = a;
                left = d;
                for (int i = 0; i < timeStep; i++)
                {
                    boost = (-player.Tank.SpeedTank - player.Speed) * player.Tank.SpeedUp / 10000;
                    if (boost >= 0)
                    {
                        boost = 0;
                    }
                    player.Speed = player.Speed + boost;
                    step = -player.Speed * 1 / 500;
                    step = StepCorrection(player, step);
                    stepSum += step;
                }
                do
                {
                    if (stepSum < 2)
                    {
                        step = stepSum;
                    }
                    else
                    {
                        step = 2;
                    }
                    stepSum -= 2;
                    if (step > 0)
                    {
                        if (!ValidMoveDown(player, step))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!ValidMoveUp(player, -step))
                        {
                            break;
                        }
                    }
                } while (stepSum > 0);
            }
            if (w == s)
            {
                if (player.Speed > 0) 
                {
                    for (int i = 0; i < timeStep; i++)
                    {
                        player.Speed = player.Speed - (player.Tank.SpeedTank - player.Speed + 1) * 0.01;
                        if (player.Speed <= 0)
                        {
                            player.Speed = 0;
                        }
                        step = player.Speed * 1 / 500;
                        step = StepCorrection(player, step);
                        stepSum += step;
                    }
                    do
                    {
                        if (stepSum < 2)
                        {
                            step = stepSum;
                        }
                        else
                        {
                            step = 2;
                        }
                        stepSum -= 2;
                        if (!ValidMoveUp(player, step))
                        {
                            break;
                        }
                    } while (stepSum > 0);
                }
                if (player.Speed < 0)
                {
                    for (int i = 0; i < timeStep; i++)
                    {
                        player.Speed = player.Speed + (player.Tank.SpeedTank + player.Speed + 1) * 0.01;
                        if (player.Speed >= 0)
                        {
                            player.Speed = 0;
                        }
                        step = -player.Speed * 1 / 500;
                        step = StepCorrection(player, step);
                        stepSum += step;
                    }
                    do
                    {
                        if (stepSum < 2)
                        {
                            step = stepSum;
                        }
                        else
                        {
                            step = 2;
                        }
                        stepSum -= 2;
                        if (!ValidMoveDown(player, step))
                        {
                            break;
                        }
                    } while (stepSum > 0);
                }
            }
            if (left > right)
            {
                for (int i = 0; i < timeStep; i++)
                {
                    step = (double)player.Tank.SpeedRotation / -500;
                    step = StepCorrection(player, step);
                    if (!ValidMoveLeftRight(player, step)) 
                    {
                        break;
                    }
                }
            }
            if (right > left) 
            {
                for (int i = 0; i < timeStep; i++)
                {
                    step = (double)player.Tank.SpeedRotation / 500;
                    step = StepCorrection(player, step);
                    if (!ValidMoveLeftRight(player, step)) 
                    {
                        break;
                    }
                }
            }
            if (mbtn == 1)
            {
                if (player.FireReady)
                {
                    TankFire(player);
                    player.ShotTime = DateTime.Now;
                }
            }
        }

        private double StepCorrection(Player player, double step)
        {
            double stepCorrect;
            int idBG;
            int indexY;
            int indexX;
            double visc;
            double viscAverage = 0;
            BGMapElement? bGMapElement;
            int[,][] tp = player.TankPoint!;
            int row = tp.GetUpperBound(0);
            int[][] pointArr = new int[4][];
            pointArr[0] = tp[0, 0];
            pointArr[1] = tp[0, row];
            pointArr[2] = tp[row, 0];
            pointArr[3] = tp[row, row];
            foreach (var point in pointArr)
            {
                indexY = point[1];
                indexX = point[0];
                idBG = arrMapBG![indexY + 20, indexX + 20][0];
                bGMapElement = bgMapElements!.FirstOrDefault(u => u.Id == idBG);
                if (bGMapElement == null)
                {
                    return 0;
                }
                visc = bGMapElement.Viscosity;
                viscAverage += visc / 4;
            }
            stepCorrect = (100 - viscAverage) / 100 * step;
            return stepCorrect;
        }

        private static List<MapPoint> NeighbourhoodShot(double x, double y, int directionTower, double step)
        {
            double xShot = x + step * Math.Cos(directionTower * Math.PI / 180);
            double yShot = y + step * Math.Sin(directionTower * Math.PI / 180);
            List<MapPoint> mapPoints = new List<MapPoint>()
            {
                new MapPoint()
                {
                    XDouble = xShot,
                    YDouble = yShot,
                    X = (int)Math.Round(xShot),
                    Y = (int)Math.Round(yShot),
                    Direction = directionTower,
                },
                new MapPoint()
                {
                    XDouble = xShot,
                    YDouble = yShot - 1,
                    X = (int)Math.Round(xShot),
                    Y = (int)Math.Round(yShot - 1),
                },
                new MapPoint()
                {
                    XDouble = xShot + 1,
                    YDouble = yShot,
                    X = (int)Math.Round(xShot + 1),
                    Y = (int)Math.Round(yShot),
                },
                new MapPoint()
                {
                    XDouble = xShot + 1,
                    YDouble = yShot,
                    X = (int)Math.Round(xShot + 1),
                    Y = (int)Math.Round(yShot),
                },
                new MapPoint()
                {
                    XDouble = xShot - 1,
                    YDouble = yShot,
                    X = (int)Math.Round(xShot - 1),
                    Y = (int)Math.Round(yShot),
                },
            };
            return mapPoints;
        }

        private void TankFire(Player player)
        {
            List<MapPoint> mapPoints = NeighbourhoodShot(player.XDouble, player.YDouble, player.DirectionTower, 22);
            player.ShotCount++;
            Shot shot = new Shot()
            {
                PlayerNum = player.PlayerNum,
                ShotNum = player.PlayerNum * 100000 - player.ShotCount,
                Speed = player.Tank!.Weapon!.BulletSpeed,
                Damage = player.Tank.Weapon.Damage,
                MapPoints = mapPoints,
            };
            fireExplosionList!.Add(shot.MapPoints[0]);
            shotsList!.Add(shot);
        }

        private void ShotOn()
        {
            Task.Run(async () =>
            {
                await ShotListTrackAsync();
            });
        }

        private async Task ShotListTrackAsync()
        {
            if (PlayerList == null || shotsList == null || arrMapCV == null || arrMapBG == null || cvMapElements == null ||
                bgMapElements == null || explosionList == null || fireExplosionList == null || removeCVMapElements == null || 
                removeTankDict == null)
            {
                return;
            }
            int count;
            int idCV;
            int idBG;
            int index;
            int indexY;
            int indexX;
            double step;
            Shot shot;
            CVMapElement? cVMapElement;
            BGMapElement? bGMapElement;
            try
            {
                while (true)
                {
                    foreach (var player in PlayerList)
                    {
                        player.GameDataJSON!.PlayerShotListJSON!.Clear();
                    }
                    for (int j = 0; j < shotsList.Count; j++)
                    {
                        shot = shotsList[j];
                        count = 0;
                        if (shot == null || shot.MapPoints == null)
                        {
                            continue;
                        }
                        if (shot.Coords != null)
                        {
                            for (int i = 0; i < shot.Coords.Length; i++)
                            {
                                arrMapCV[shot.Coords[i][1] + 20, shot.Coords[i][0] + 20][0] = 0;
                                arrMapCV[shot.Coords[i][1] + 20, shot.Coords[i][0] + 20][1] = 0;
                            }
                        }
                        if (shot.Remove)
                        {
                            explosionList.Add(shot.MapPoints[0]);
                            shotsList.Remove(shot);
                            continue;
                        }
                        foreach (var mp in shot.MapPoints)
                        {
                            indexY = mp.Y;
                            indexX = mp.X;
                            idCV = arrMapCV[indexY + 20, indexX + 20][0];
                            index = arrMapCV[indexY + 20, indexX + 20][1];
                            idBG = arrMapBG[indexY + 20, indexX + 20][0];
                            cVMapElement = cvMapElements.FirstOrDefault(u => u.Id == idCV);
                            bGMapElement = bgMapElements.FirstOrDefault(u => u.Id == idBG);
                            if (bGMapElement == null || cVMapElement == null)
                            {
                                break;
                            }
                            if (bGMapElement.BulletPermeability)
                            {
                                if (cVMapElement.BulletPermeability)
                                {
                                    count++;
                                }
                                else
                                {
                                    GetDamage(shot, idCV, index);
                                    explosionList.Add(shot.MapPoints[0]);
                                    shotsList.Remove(shot);
                                    break;
                                }
                            }
                            else
                            {
                                explosionList.Add(shot.MapPoints[0]);
                                shotsList.Remove(shot);
                                break;
                            }
                        }
                        if (count == 5)
                        {
                            if (shot.Coords == null)
                            {
                                shot.Coords = new int[5][];
                                shot.Coords[0] = new int[] { 0, 0 };
                                shot.Coords[1] = new int[] { 0, 0 };
                                shot.Coords[2] = new int[] { 0, 0 };
                                shot.Coords[3] = new int[] { 0, 0 };
                                shot.Coords[4] = new int[] { 0, 0 };
                            }
                            for (int i = 0; i < shot.MapPoints.Count; i++)
                            {
                                shot.Coords[i][0] = shot.MapPoints[i].X;
                                shot.Coords[i][1] = shot.MapPoints[i].Y;
                                arrMapCV[shot.Coords[i][1] + 20, shot.Coords[i][0] + 20][0] = -1000000;
                                arrMapCV[shot.Coords[i][1] + 20, shot.Coords[i][0] + 20][1] = shot.ShotNum;
                            }
                            step = (double)shot.Speed / 1000;
                            shot.MapPoints = NeighbourhoodShot(shot.MapPoints[0].XDouble, shot.MapPoints[0].YDouble, shot.MapPoints[0].Direction, step);
                        }
                    }
                    foreach (var player in PlayerList.ToList())
                    {
                        foreach (var shott in shotsList.ToList())
                        {
                            if (shott.MapPoints == null)
                            {
                                continue;
                            }
                            MapPoint mapPoint = new MapPoint()
                            {
                                X = shott.MapPoints[0].X,
                                Y = shott.MapPoints[0].Y,
                                Direction = shott.MapPoints[0].Direction,
                            };
                            player.GameDataJSON!.PlayerShotListJSON!.Add(mapPoint);
                        }
                        player.RemoveTankList.AddRange(removeTankDict);
                        player.GameDataJSON!.FireExplosionListJSON!.AddRange(fireExplosionList);
                        player.GameDataJSON.ExplosionListJSON!.AddRange(explosionList);
                        player.GameDataJSON.RemoveCVListJSON!.AddRange(removeCVMapElements);
                    }
                    removeTankDict.Clear();
                    fireExplosionList.Clear();
                    explosionList.Clear();
                    removeCVMapElements.Clear();
                    await Task.Delay(5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private void GetDamage(Shot shot, int idCV, int index)
        {
            Player? player;
            Player? playerEnemy;
            Shot? shotEnemy;
            CVMapElement? cVMapElement;
            if (idCV == -1000000)
            {
                shotEnemy = shotsList!.FirstOrDefault(u => u.ShotNum == index);
                if (shotEnemy != null)
                {
                    shotEnemy.Remove = true;
                }
                return;
            }
            if (idCV < 0)
            {
                player = PlayerList!.FirstOrDefault(u => u.PlayerNum == shot!.PlayerNum);
                playerEnemy = PlayerList!.FirstOrDefault(u => u.PlayerNum == idCV);
                if (playerEnemy != null && playerEnemy.Tank != null && player != null)
                {
                    if (!playerEnemy.Invulnerability)
                    {
                        if (player.Team == playerEnemy.Team)
                        {
                            if (FriendlyFire)
                            {
                                if (playerEnemy.Tank.Health >= shot.Damage)
                                {
                                    playerEnemy.Tank.Health = playerEnemy.Tank.Health - shot.Damage;
                                    player.DamageFriendly += shot.Damage;
                                    if (player.RebirthPoints >= shot.Damage / 2)
                                    {
                                        player.RebirthPoints -= shot.Damage / 2;
                                    }
                                    else
                                    {
                                        player.RebirthPoints = 0;
                                    }
                                }
                                else
                                {
                                    player.DamageFriendly += playerEnemy.Tank.Health;
                                    player.RebirthPoints -= playerEnemy.Tank.Health / 2;
                                    playerEnemy.Tank.Health = 0;
                                }
                                if (playerEnemy.Tank.Health <= 0)
                                {
                                    player.Kills--;
                                    if (playerEnemy.TankPoint != null)
                                    {
                                        foreach (var point in playerEnemy.TankPoint)
                                        {
                                            arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                                            arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
                                        }
                                    }
                                    if (playerEnemy.Tank != null)
                                    {
                                        removeTankDict!.Add(playerEnemy.Name!, playerEnemy.Tank.TankId);
                                        playerEnemy.Tank = null;
                                        playerEnemy.PlayerVision!.Clear();
                                        playerEnemy.CVVision!.Clear();
                                        removeCVMapElements!.Add("tank_" + playerEnemy.Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (playerEnemy.Tank.Health >= shot.Damage)
                            {
                                playerEnemy.Tank.Health = playerEnemy.Tank.Health - shot.Damage;
                                player.DamageSum += shot.Damage;
                                player.RebirthPoints += shot.Damage / 2;
                            }
                            else
                            {
                                player.DamageSum += playerEnemy.Tank.Health;
                                player.RebirthPoints += playerEnemy.Tank.Health / 2;
                                playerEnemy.Tank.Health = 0;
                            }
                            if (playerEnemy.Tank.Health <= 0)
                            {
                                player.Kills++;
                                playerEnemy.Death++;
                                if (playerEnemy.TankPoint != null)
                                {
                                    foreach (var point in playerEnemy.TankPoint)
                                    {
                                        arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                                        arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
                                    }
                                }
                                if (playerEnemy.Tank != null)
                                {
                                    removeTankDict!.Add(playerEnemy.Name!, playerEnemy.Tank.TankId);
                                    playerEnemy.Tank = null;
                                    playerEnemy.PlayerVision!.Clear();
                                    playerEnemy.CVVision!.Clear();
                                    removeCVMapElements!.Add("tank_" + playerEnemy.Name);
                                }
                            }
                        }
                    }
                }
                return;
            }
            cVMapElement = cvGameObjmapElements!.FirstOrDefault(u => u.Index == index);
            if (cVMapElement != null && !cVMapElement.Invulnerability)
            {
                cVMapElement.Heath -= shot.Damage;
                if (cVMapElement.Heath <= 0)
                {
                    if (cVMapElement.HQ)
                    {
                        player = PlayerList!.FirstOrDefault(u => u.PlayerNum == shot!.PlayerNum);
                        if (player != null && teamPlayerDict!.ContainsKey(cVMapElement.HQTeam)) 
                        {
                            foreach (var pl in teamPlayerDict[cVMapElement.HQTeam])
                            {
                                if (player.Team != cVMapElement.HQTeam && player.RebirthPoints > 0)
                                {
                                    player.RebirthPoints += pl.RebirthPoints;
                                }
                                if (pl.TankPoint != null)
                                {
                                    foreach (var point in pl.TankPoint)
                                    {
                                        arrMapCV![point[1] + 20, point[0] + 20][0] = 0;
                                        arrMapCV[point[1] + 20, point[0] + 20][1] = 0;
                                    }
                                }
                                if (pl.Tank != null)
                                {
                                    removeTankDict!.Add(pl.Name!, pl.Tank.TankId);
                                    pl.Tank = null;
                                    pl.PlayerVision!.Clear();
                                    pl.CVVision!.Clear();
                                    removeCVMapElements!.Add("tank_" + pl.Name);
                                }
                                pl.LostVictory = -1;
                                pl.RebirthPoints = -99999999;
                            }
                            teamList!.Remove(cVMapElement.HQTeam);
                            if (teamList.Count == 1)
                            {
                                VictoryTeam(teamList[0]);
                            }
                            List<CVMapElement> hqList = cvGameObjmapElements!.FindAll(u => u.HQTeam == cVMapElement.HQTeam);
                            foreach (var hq in hqList)
                            {
                                removeCVMapElements!.Add("cvme" + hq.Index);
                                removeCVGameObjmapElements!.Add("cvme" + hq.Index);
                                foreach (var item in arrMapCV!)
                                {
                                    if (item[1] == hq.Index)
                                    {
                                        item[0] = 0;
                                        item[1] = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        removeCVMapElements!.Add("cvme" + cVMapElement.Index);
                        removeCVGameObjmapElements!.Add("cvme" + cVMapElement.Index);
                        foreach (var item in arrMapCV!)
                        {
                            if (item[1] == index)
                            {
                                item[0] = 0;
                                item[1] = 0;
                            }
                        }
                    }
                }
            }
        }

        private void DefeatTeam(int team)
        {
            int count = 0;
            int rpSum = 0;
            foreach (var p in teamPlayerDict![team])
            {
                if (p.Tank == null)
                {
                    count++;
                }
                else
                {
                    return;
                }
                rpSum += p.RebirthPoints;
            }
            if (count == teamPlayerDict[team].Count && rpSum < rpMin)
            {
                if (!shotsList!.ToList().Exists(u => teamPlayerDict[team].ToList().Exists(x => x.PlayerNum == u.PlayerNum)))
                {
                    foreach (var pl in teamPlayerDict[team].ToList())
                    {
                        pl.LostVictory = -1;
                        pl.RebirthPoints = -99999999;
                    }
                    teamList!.Remove(team);
                    if (teamList.Count == 1) 
                    {
                        VictoryTeam(teamList[0]);
                    }
                }
            }
        }

        private void VictoryTeam(int team)
        {
            foreach (var pl in teamPlayerDict![team].ToList())
            {
                pl.LostVictory = 1;
                pl.RebirthPoints = 99999999;
            }
        }
    }
}
