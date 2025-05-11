// ☺ Editada y Reparada Por Pezzi Tomas / Fixed and Work by Pezzi Tomas
using System;
using System.Linq;
using COServer.Game;
using System.Drawing;
using COServer.Client;
using System.Threading;
using COServer.DB;
using System.Collections.Generic;
using COServer.Network.GamePackets;
using COServer.Game.ConquerStructures;
using COServer.Game.Features.Tournaments;
using Core.Packet;
using Core.Enums;
using Core;
using Game.Features.Event;
using COServer.Events;
using System.Threading.Generic;
using static COServer.Database.SystemBannedAccount;
using COServer.Interfaces;

namespace COServer
{
    public unsafe class PlayerThread
    {
        public Time32 Restart;
        public static bool RestartNow = false;
        public static bool SetRestartNow = false;
        public static StaticPool GenericThreadPool;
        public System.Threading.Generic.TimerRule<GameState> Characters, AutoAttack, Prayer,Pets;
        public MsgWarFlag CTF;
        public SteedRace SteedRace;
        public DelayedTask DelayedTask;
        public Game.Features.Tournaments.HeroOfGame HeroOfGame = new Game.Features.Tournaments.HeroOfGame(); 
        public PoleAssassin PoleAssassin;
        public List<KillTournament> Tournaments;
        public PlayerThread()
        {
            GenericThreadPool = new StaticPool(32).Run();
            DelayedTask = new DelayedTask();
        }
        public void Init()
        {
            ThunderCloud = new TimerRule<Player>(ThunderCloudTimer, 250, ThreadPriority.Lowest);
            Prayer = new TimerRule<GameState>(PrayerCallback, 1000, ThreadPriority.BelowNormal);
            Characters = new TimerRule<GameState>(CharactersCallback, 1000, ThreadPriority.BelowNormal);
            AutoAttack = new TimerRule<GameState>(AutoAttackCallback, 500, ThreadPriority.BelowNormal);
            Pets = new TimerRule<GameState>(CompanionsCallback, 1000, ThreadPriority.BelowNormal);
            Subscribe(WorldTournaments, 1000);
            Subscribe(ServerFunctions, 5000);
            Subscribe(QualifierFunctions, 1000, ThreadPriority.AboveNormal);
            Subscribe(TeamQualifierFunctions, 1000, ThreadPriority.AboveNormal);
        }
        public void CreateTournaments()
        {
            DMaps.LoadMap(1730);
            var map = Kernel.Maps[700];
            Tournaments = new List<KillTournament>();
            #region Class PK Tournament
            map = Kernel.Maps[1730];
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0, (client) =>
            {
                client.Player.ConquerPoints += 5000;
                client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopTrojan, 0, DateTime.Now.AddDays(7).AddHours(-1));
            }, "Class PK War (Trojan)", (p) => { return p.Player.Class >= 10 && p.Player.Class <= 15; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0, (client) =>
            {
                client.Player.ConquerPoints += 5000;
                client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopWarrior, 0, DateTime.Now.AddDays(7).AddHours(-1));
            }, "Class PK War (Warrior)", (p) => { return p.Player.Class >= 20 && p.Player.Class <= 25; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopArcher, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Archer)", (p) => { return p.Player.Class >= 40 && p.Player.Class <= 45; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopNinja, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Ninja)", (p) => { return p.Player.Class >= 50 && p.Player.Class <= 55; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopMonk, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Monk)", (p) => { return p.Player.Class >= 60 && p.Player.Class <= 65; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopPirate, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Pirate)", (p) => { return p.Player.Class >= 70 && p.Player.Class <= 75; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopWaterTaoist, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Water Taoist)", (p) => { return p.Player.Class >= 130 && p.Player.Class <= 135; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
                (client) =>
                {
                    client.Player.ConquerPoints += 5000;
                    client.Player.AddTopStatus((ulong)PacketFlag.Flags.TopFireTaoist, 0, DateTime.Now.AddDays(7).AddHours(-1));
                }, "Class PK War (Fire Taoist)", (p) => { return p.Player.Class >= 140 && p.Player.Class <= 145; },
                "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
              (client) =>
              {
                  client.Player.ConquerPoints += 5000;
                  client.Player.AddTopStatus((ulong)PacketFlag.Flags.DragonWarriorTop, 0, DateTime.Now.AddDays(7).AddHours(-1));
              }, "Class PK War (Dragon Warrior)", (p) => { return p.Player.Class >= 80 && p.Player.Class <= 85; },
              "Class PK War is about to begin! Will you join it"));
            Tournaments.Add(new KillTournament(map.MakeDynamicMap().ID, WeekDay.Monday, 22, 0,
      (client) =>
      {
          client.Player.ConquerPoints += 50000;
          client.Player.AddTopStatus((ulong)PacketFlag.Flags.WindwalkerTop, 0, DateTime.Now.AddDays(7).AddHours(-1));
      }, "Class PK War (Wind walker)", (p) => { return p.Player.Class >= 160 && p.Player.Class <= 165; },
      "Class PK War is about to begin! Will you join it"));
            #endregion
            CTF = new MsgWarFlag();
            PoleAssassin = new PoleAssassin(20000000);
            FamilyTournament.Create();
            if (DMaps.LoadMap(2068))
            {
                ElitePKTournament.Create();
                Game.Features.Tournaments.TeamElitePk.TeamTournament.Create();
                Game.Features.Tournaments.TeamElitePk.SkillTeamTournament.Create();
            }
            if (DMaps.LoadMap(2057))
                SteedRace = new SteedRace();
            DelayedTask = new DelayedTask();
            new Game.StatuesWar();
            new ClassPoleWar();
            new NobiltyPoleWar();
            new GuildScoreWar();
           // new GuildPoleWar(); ERROR EN QUE NO SE LE PUEDE PEGAR
        }
        private void CompanionsCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            if (client.Companion != null)
            {
                #region PetAPet
                if (!client.Player.Dead && client.Player.Hitpoints < client.Player.MaxHitpoints && (client.Companion.Body == 846 || client.Companion.Body == 847))
                {
                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    suse.Attacker = client.Companion.UID;
                    suse.SpellID = 1055;
                    suse.SpellLevel = 3;
                    suse.X = client.Player.X;
                    suse.Y = client.Player.Y;
                    suse.AddTarget(client.Player.UID, 0, null);
                    client.SendScreen(suse, true);
                    uint val = 1500;
                    if (client.Companion.Body == 847)
                        val += 1500;
                    client.Player.Hitpoints = Math.Min(client.Player.Hitpoints + val, client.Player.MaxHitpoints);
                }
                if (!client.Player.Dead && client.Companion.Body == 850 && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.Stigma))
                {
                    DB.SpellInformation spell = DB.SpellTable.SpellInformations[1095][4];
                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    suse.Attacker = client.Companion.UID;
                    suse.SpellID = 1055;
                    suse.SpellLevel = 3;
                    suse.X = client.Player.X;
                    suse.Y = client.Player.Y;
                    suse.AddTarget(client.Player.UID, 0, null);
                    client.SendScreen(suse, true);
                    client.Player.ShieldTime = 0;
                    client.Player.ShieldStamp = Time32.Now;
                    client.Player.MagicShieldStamp = Time32.Now;
                    client.Player.MagicShieldTime = 0;
                    client.Player.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                    client.Player.MagicShieldStamp = Time32.Now;
                    client.Player.MagicShieldIncrease = 1.1f;//spell.PowerPercent;
                    client.Player.MagicShieldTime = (byte)spell.Duration;
                    if (client.Player.PlayerFlag == PlayerFlag.Player)
                        client.Player.Owner.Send(DefineConstantsEn_Res.Shield(spell.PowerPercent, spell.Duration));
                    client.Player.AddFlag((ulong)PacketFlag.Flags.Stigma);
                    client.Player.StigmaStamp = Time32.Now;
                    client.Player.StigmaIncrease = spell.PowerPercent;
                    client.Player.StigmaTime = (byte)spell.Duration;
                    if (client.Player.PlayerFlag == PlayerFlag.Player)
                        client.Player.Owner.Send(DefineConstantsEn_Res.Stigma(spell.PowerPercent, spell.Duration));

                }
                #endregion
                #region Normal Companion
                short distance = Kernel.GetDistance(client.Companion.X, client.Companion.Y, client.Player.X, client.Player.Y);
                if (distance >= 15)
                {
                    ushort X = (ushort)(client.Player.X + Kernel.Random.Next(2));
                    ushort Y = (ushort)(client.Player.Y + Kernel.Random.Next(2));
                    if (!client.Map.SelectCoordonates(ref X, ref Y))
                    {
                        X = client.Player.X;
                        Y = client.Player.Y;
                    }
                    client.Companion.X = X;
                    client.Companion.Y = Y;
                    Network.GamePackets.MsgAction data = new COServer.Network.GamePackets.MsgAction(true);
                    data.ID = PacketMsgAction.Mode.Jump;
                    data.dwParam = (uint)((Y << 16) | X);
                    data.X = X;
                    data.Y = Y;
                    data.UID = client.Companion.UID;
                    client.Companion.MonsterInfo.SendScreen(data);
                }
                else if (distance > 3)
                {
                    Enums.ConquerAngle facing = Kernel.GetAngle(client.Companion.X, client.Companion.Y, client.Companion.Owner.Player.X, client.Companion.Owner.Player.Y);
                    if (!client.Companion.Move(facing))
                    {
                        facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                        if (client.Companion.Move(facing))
                        {
                            client.Companion.Facing = facing;
                            Network.GamePackets.GroundMovement move = new COServer.Network.GamePackets.GroundMovement(true);
                            move.Direction = facing;
                            move.UID = client.Companion.UID;
                            move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                            client.Companion.MonsterInfo.SendScreen(move);
                        }
                    }
                    else
                    {//test pls
                        client.Companion.Facing = facing;
                        Network.GamePackets.GroundMovement move = new COServer.Network.GamePackets.GroundMovement(true);
                        move.Direction = facing;
                        move.UID = client.Companion.UID;
                        move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                        client.Companion.MonsterInfo.SendScreen(move);
                    }
                }
                else
                {
                    var monster = client.Companion;
                    if (monster.MonsterInfo.InSight == 0)
                    {
                        if (client.Player.AttackPacket != null)
                        {
                            if (client.Player.AttackPacket.InteractType == MsgInteract.Magic)
                            {
                                if (client.Player.AttackPacket.Decoded)
                                {
                                    if (DB.SpellTable.SpellInformations.ContainsKey((ushort)client.Player.AttackPacket.Damage))
                                    {
                                        
                                            var info = DB.SpellTable.SpellInformations[(ushort)client.Player.AttackPacket.Damage].Values.ToArray()[client.Spells[(ushort)client.Player.AttackPacket.Damage].Level];
                                            if (info.CanKill)
                                            {
                                                monster.MonsterInfo.InSight = client.Player.AttackPacket.Attacked;
                                            }
                                       
                                    }
                                }
                            }
                            else
                            {
                                monster.MonsterInfo.InSight = client.Player.AttackPacket.Attacked;
                            }
                        }
                    }
                    else
                    {
                        if (monster.MonsterInfo.InSight > 400000 && monster.MonsterInfo.InSight < 600000 || monster.MonsterInfo.InSight > 800000 && monster.MonsterInfo.InSight != monster.UID)
                        {
                            Player attacked = null;

                            if (client.Screen.TryGetValue(monster.MonsterInfo.InSight, out attacked))
                            {
                                if (Now > monster.AttackStamp.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                {
                                    monster.AttackStamp = Now;
                                    if (attacked.Dead)
                                    {
                                        monster.MonsterInfo.InSight = 0;
                                    }
                                    else
                                        new Game.Attacking.Handle(null, monster, attacked);
                                }
                            }
                            else
                                monster.MonsterInfo.InSight = 0;
                        }
                    }
                }
            }
            #endregion Normal Companion
            #region Shadow2
            if (client.Companion2 != null)
            {
                #region PetAPet
                if (!client.Player.Dead && client.Player.Hitpoints < client.Player.MaxHitpoints && (client.Companion2.Body == 846 || client.Companion2.Body == 847))
                {
                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    suse.Attacker = client.Companion2.UID;
                    suse.SpellID = 1055;
                    suse.SpellLevel = 3;
                    suse.X = client.Player.X;
                    suse.Y = client.Player.Y;
                    suse.AddTarget(client.Player.UID, 0, null);
                    client.SendScreen(suse, true);
                    uint val = 1500;
                    if (client.Companion2.Body == 847)
                        val += 1500;
                    client.Player.Hitpoints = Math.Min(client.Player.Hitpoints + val, client.Player.MaxHitpoints);
                }
                if (!client.Player.Dead && client.Companion2.Body == 850 && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.Stigma))
                {
                    DB.SpellInformation spell = DB.SpellTable.SpellInformations[1095][4];
                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    suse.Attacker = client.Companion2.UID;
                    suse.SpellID = 1055;
                    suse.SpellLevel = 3;
                    suse.X = client.Player.X;
                    suse.Y = client.Player.Y;
                    suse.AddTarget(client.Player.UID, 0, null);
                    client.SendScreen(suse, true);
                    client.Player.ShieldTime = 0;
                    client.Player.ShieldStamp = Time32.Now;
                    client.Player.MagicShieldStamp = Time32.Now;
                    client.Player.MagicShieldTime = 0;
                    client.Player.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                    client.Player.MagicShieldStamp = Time32.Now;
                    client.Player.MagicShieldIncrease = 1.1f;//spell.PowerPercent;
                    client.Player.MagicShieldTime = (byte)spell.Duration;
                    if (client.Player.PlayerFlag == PlayerFlag.Player)
                        client.Player.Owner.Send(DefineConstantsEn_Res.Shield(spell.PowerPercent, spell.Duration));
                    client.Player.AddFlag((ulong)PacketFlag.Flags.Stigma);
                    client.Player.StigmaStamp = Time32.Now;
                    client.Player.StigmaIncrease = spell.PowerPercent;
                    client.Player.StigmaTime = (byte)spell.Duration;
                    if (client.Player.PlayerFlag == PlayerFlag.Player)
                        client.Player.Owner.Send(DefineConstantsEn_Res.Stigma(spell.PowerPercent, spell.Duration));

                }
                #endregion
                short distance = Kernel.GetDistance(client.Companion2.X, client.Companion2.Y, client.Player.X, client.Player.Y);
                if (distance >= 15)
                {
                    ushort X = (ushort)(client.Player.X + Kernel.Random.Next(2));
                    ushort Y = (ushort)(client.Player.Y + Kernel.Random.Next(2));
                    if (!client.Map.SelectCoordonates(ref X, ref Y))
                    {
                        X = client.Player.X;
                        Y = client.Player.Y;
                    }
                    client.Companion2.X = X;
                    client.Companion2.Y = Y;
                    Network.GamePackets.MsgAction data = new COServer.Network.GamePackets.MsgAction(true);
                    data.ID = PacketMsgAction.Mode.Jump;
                    data.dwParam = (uint)((Y << 16) | X);
                    data.X = X;
                    data.Y = Y;
                    data.UID = client.Companion2.UID;
                    client.Companion2.MonsterInfo.SendScreen(data);
                }
                else if (distance > 4)
                {
                    Enums.ConquerAngle facing = Kernel.GetAngle(client.Companion2.X, client.Companion2.Y, client.Companion2.Owner.Player.X, client.Companion2.Owner.Player.Y);
                    if (!client.Companion2.Move(facing))
                    {
                        facing = (Enums.ConquerAngle)Kernel.Random.Next(7);
                        if (client.Companion2.Move(facing))
                        {
                            client.Companion2.Facing = facing;
                            Network.GamePackets.GroundMovement move = new COServer.Network.GamePackets.GroundMovement(true);
                            move.Direction = facing;
                            move.UID = client.Companion2.UID;
                            move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                            client.Companion2.MonsterInfo.SendScreen(move);
                        }
                    }
                    else
                    {
                        client.Companion2.Facing = facing;
                        Network.GamePackets.GroundMovement move = new COServer.Network.GamePackets.GroundMovement(true);
                        move.Direction = facing;
                        move.UID = client.Companion2.UID;
                        move.GroundMovementType = Network.GamePackets.GroundMovement.Run;
                        client.Companion2.MonsterInfo.SendScreen(move);
                    }
                }
                else
                {
                    var monster = client.Companion2;
                    if (monster.MonsterInfo.InSight == 0)
                    {
                        if (client.Player.AttackPacket != null)
                        {
                            if (client.Player.AttackPacket.InteractType == MsgInteract.Magic)
                            {
                                if (client.Player.AttackPacket.Decoded)
                                {
                                    if (DB.SpellTable.SpellInformations.ContainsKey((ushort)client.Player.AttackPacket.Damage))
                                    {
                                        var info = DB.SpellTable.SpellInformations[(ushort)client.Player.AttackPacket.Damage].Values.ToArray()[client.Spells[(ushort)client.Player.AttackPacket.Damage].Level];
                                        if (info.CanKill)
                                        {
                                            monster.MonsterInfo.InSight = client.Player.AttackPacket.Attacked;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                monster.MonsterInfo.InSight = client.Player.AttackPacket.Attacked;
                            }
                        }
                    }
                    else
                    {
                        if (monster.MonsterInfo.InSight > 400000 && monster.MonsterInfo.InSight < 600000 || monster.MonsterInfo.InSight > 800000 && monster.MonsterInfo.InSight != monster.UID)
                        {
                            Player
 attacked = null;

                            if (client.Screen.TryGetValue(monster.MonsterInfo.InSight, out attacked))
                            {
                                if (Now > monster.AttackStamp.AddMilliseconds(monster.MonsterInfo.AttackSpeed))
                                {

                                    monster.AttackStamp = Now;
                                    if (attacked.Dead)
                                    {
                                        monster.MonsterInfo.InSight = 0;
                                    }
                                    else
                                        new Game.Attacking.Handle(null, monster, attacked);
                                }
                            }
                            else
                                monster.MonsterInfo.InSight = 0;
                        }
                    }
                }
            }
            #endregion
        }
        private void WorldTournaments(int time)
        {

            Time32 Now = new Time32(time);
            DateTime Now64 = DateTime.Now;
            #region Mr/Ms Conquer
            //if (DateTime.Now.Minute == 40 && Now64.Second == 15)
            //{
            //    Kernel.SendWorldMessage(new MsgTalk("Mr/Ms Conquer War began! Go Twin city ", Color.Red, MsgTalk.BroadcastMessage), Server.GamePool);
            //    foreach (var client in Server.GamePool)

            //        client.MessageBox("Mr/Ms Conquer  began! Would you like to join Priz ?",
            //            p => { p.Player.Teleport(1002, 290, 193); }, null, 60);
            //}
            #endregion         
            #region TreasureBox
            if ((Now64.Minute == 32) && (Now64.Second == 01))
            {
                TreasureBox.OnGoing = true;
                for (int i = 0; i < 10; i++)
                    Game.TreasureBox.Generate();
                Kernel.SendWorldMessage(new MsgTalk("The Lost TreasureBox event began!", Color.Red, MsgTalk.Center));

                foreach (var client in Server.GamePool)
                    client.MessageBox("Lost treasure box event has started! Would you like to join? [Prize: 5kk or 1 kk  CPs or more]",
                        (p) => { p.Player.Teleport(1002, 300, 229); }, null);
            }
            if (TreasureBox.OnGoing)
            {
                Game.TreasureBox.Generate();
            }
            if ((Now64.Minute == 37) && TreasureBox.OnGoing)
            {
                TreasureBox.OnGoing = false;
                foreach (var client in Server.GamePool)
                    if (client.Player.MapID == 3820)
                        client.Player.Teleport(1002, 302, 286);
                Kernel.SendWorldMessage(new MsgTalk("The Lost TreasureBox event ended!", Color.Red, MsgTalk.Center));
            }
            #endregion
            #region Caja Misteriosa [xx:58]
            if (DateTime.Now.Minute == 58 && DateTime.Now.Second == 42)
            {
                Kernel.SendWorldMessage(new MsgTalk("Mistery Box Event Has Begun!", Color.White, MsgTalk.TopLeft), Server.GamePool);
                foreach (var client in Server.GamePool)
                    client.MessageBox("Mistery Box Event Start u Wanna Join? ",
                        p => { p.Player.Teleport(1000, 277, 451); }, null, 30);
                Kernel.CajaMisteriosaOpen = true;
                //foreach (var client in Program.GamePool)
                //    Kernel.SendWorldMessage(new Message("Evento Caja Misteriosa Comenzo!", Color.White, Message.TopLeft), Program.GamePool);
                //foreach (var client in Program.GamePool)
                //    client.MessageBox("Caja Misteriosa Comenzo, Quieres Entrar a Probar tu Suerte? ",
                //    (p) => { p.Entity.Teleport(1000, 277, 451); }, null, 10);
            }
            if (DateTime.Now.Minute == 59 && DateTime.Now.Second == 02)
            {
                Kernel.CajaMisteriosaOpen = false;
            }
            #endregion
            #region Monthly PK
            if (Now64.Day <= 7 && Now64.DayOfWeek == DayOfWeek.Sunday)
            {
                if (Now64.Hour == 21 && Now64.Minute >= 50 && Now64.Second <= 2)
                {
                    int min = 60 - Now64.Minute;
                    Kernel.SendWorldMessage(new MsgTalk("The Monthly PK War will start in " + min.ToString() + " minutes!", Color.Red, 2012));
                }
                if (Now64.Hour == 22 && Now64.Minute == 00 && Now64.Second <= 2)
                {
                    Player.MonthlyPKWar = true;
                    Kernel.SendWorldMessage(new MsgTalk("The Monthly PK War began!", Color.Red, 2012));
                    foreach (var client in Server.GamePool)
                    {
                        if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                        {
                            MsgInviteTrans alert = new MsgInviteTrans
                            {
                                InviteID = 10523,
                                Countdown = 60,
                                Action = 1
                            };
                            client.Player.InviteID = 10523;
                            client.Send(alert.ToArray());
                        }
                    }
                }
                if (Now64.Hour == 22 && Now64.Minute >= 5 && Player.MonthlyPKWar)
                {
                    Player.MonthlyPKWar = false;
                    Kernel.SendWorldMessage(new MsgTalk("The Monthly PK War ended!", Color.Red, (uint)PacketMsgTalk.MsgTalkType.Center));
                }
            }
            #endregion
            #region Weekly PK
            if (Now64.DayOfWeek == DayOfWeek.Saturday && Now64.Hour == 20 && Now64.Minute == 00 && Now64.Second <= 0)
            {
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10521,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10521;
                        client.Send(alert.ToArray());
                    }
                }
            }
            if (Now64.DayOfWeek == DayOfWeek.Sunday && Now64.Hour == 20 && Now64.Minute == 5 && Now64.Second <= 0)
            {
                Kernel.SendWorldMessage(new MsgTalk("The Weekly PK War ended!", Color.Red, (uint)PacketMsgTalk.MsgTalkType.Center));
            }
            #endregion
            #region Team Qualifier
            if ((Now64.Hour == 11 || Now64.Hour == 19) && Now64.Minute == 02 && Now64.Second <= 1)
            {
                Kernel.SendWorldMessage(new MsgTalk("TeamQualifier Arena starts sign up now", Color.Red, (uint)PacketMsgTalk.MsgTalkType.Center));
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10562,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10562;
                        client.Send(alert.ToArray());
                    }
                }
            }
            #endregion
            #region WorldMessage
            //  if (Now64.Minute == 2 && Now64.Second <= 0)
            ///////   {
            ///////       Player.SendWorldMessage("if You Want Donate Call HelpDesk or [GM] Only Owner The Game");
            /////////     }
            ///////////////    if (Now64.Minute == 30 && Now64.Second <= 1 || Now64.Minute == 34 && Now64.Second <= 1 || Now64.Minute == 42 && Now64.Second <= 1 || Now64.Minute == 53 && Now64.Second <= 1 || Now64.Minute == 14 && Now64.Second <= 1)
            ///////////     {
            //  Kernel.SendWorldMessage(new MsgTalk("[Stay online] and gain 2,000,000 CPs Automatic for being online those 15 Minutes, Stay online and gain more", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
            /////////////////     }
            ///      if (Now64.Minute == 11 && Now64.Second <= 1)
            ////////     {
            //////////        Kernel.SendWorldMessage(new MsgTalk("[Stay online] and gain Online Training Points[OTP`s], Talk to ExchangeOfficer in TwinCity to exchange your points with the available prizes!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
            ////   }
            /////////     if (Now64.Minute == 50 && Now64.Second <= 1)
            ////     {
            //  Kernel.SendWorldMessage(new MsgTalk("Do not give your account details to others. Change your password at Website !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
            ////    }
            if (Now64.Minute == 59 && Now64.Second <= 1)
            {
                Kernel.SendWorldMessage(new MsgTalk("Remember at 14,19,22 PM Start CrazyHour During 1 Hour, Get  X2 CPs, X20 OnlinePoints!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
            }
            #endregion
            #region ClanWar
            if (Now64.Hour == 20 && Now64.Minute == 23 && Now64.Second <= 0)
            {
                Player.name = new object[] { "ClanLeader Go to every map to ClanWar npc to Apply 10 Minute And closed Apply !?" };
                Kernel.SendWorldMessage(new MsgTalk(string.Concat(Player.name), "ALLUSERS", "[ClanWar]", System.Drawing.Color.Red, 2500), Server.GamePool);
            }
            if (Now64.Hour == 20 && Now64.Minute == 33 && Now64.Second <= 0)
            {
                FamilyTournament.Start();
                foreach (var client in Server.GamePool)
                {
                    if (client.Player.GetClan != null)
                    {
                        if (client.Player.GetClan.TwinCityClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "TwinCityArena " }.Send(client);
                        }
                        if (client.Player.GetClan.WindPlainClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "WindPlainClan " }.Send(client);
                        }
                        if (client.Player.GetClan.DesertCityClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "DesertCityClan " }.Send(client);
                        }
                        if (client.Player.GetClan.DesertClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "DesertClan " }.Send(client);
                        }
                        if (client.Player.GetClan.BirdCityClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "BirdCityClan " }.Send(client);
                        }
                        if (client.Player.GetClan.BirdIslandClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "BirdIslandClan " }.Send(client);
                        }
                        if (client.Player.GetClan.ApeCityClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "ApeCityClan " }.Send(client);
                        }
                        if (client.Player.GetClan.LoveCanyonClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "LoveCanyonClan " }.Send(client);
                        }
                        if (client.Player.GetClan.PhoenixCastleClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "PhoenixCastleClan " }.Send(client);
                        }
                        if (client.Player.GetClan.MapleForestClan)
                        {
                            client.Player.GetClan.Claimed = false;
                            new MsgFamilyOccupy() { Action = 7, Button = 7, DominationMap = "MapleForestClan " }.Send(client);
                        }
                    }
                }
            }
            #endregion
            #region MatePorCPS
            if (Now64.DayOfWeek == DayOfWeek.Sunday && Now64.Hour == 14 && Now64.Minute == 26 && Now64.Second == 00)
            {
                
                Player.name = new object[] { "Mate Por CPs iniciou, deseja participar ?" };
                Kernel.SendWorldMessage(new MsgTalk(string.Concat(Player.name), "ALLUSERS", "", System.Drawing.Color.Red, 2500), Server.GamePool);
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10558,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10555;
                        client.Send(alert.ToArray());
                    }
                }

            }
            #endregion
            #region HeroOFGame[53]
            if (Now64.Minute == 53 && Now64.Second == 5)
            {
                //DMaps.LoadMap(1507);
                HeroOfGame.CheakUp();
            }
            #endregion
            #region Elite GW[30:45]
            if (!EliteGuildWar.IsWar)
            {
                if (Now64.Hour == 17 && Now64.Minute == 01 && Now64.Second == 01)
                {
                    EliteGuildWar.Start();
                    foreach (var client in Server.GamePool)
                        if (client.Player.GuildID != 0)
                            client.MessageBox(
                                "Elite GuildWar has begun! Would you like to join?",
                                p => { p.Player.Teleport(2071, 47, 131); }, null);
                }
            }
            if (EliteGuildWar.IsWar)
            {
                if (Time32.Now > EliteGuildWar.ScoreSendStamp.AddSeconds(3))
                {
                    EliteGuildWar.ScoreSendStamp = Time32.Now;
                    EliteGuildWar.SendScores();
                }
                if (Now64.Hour == 17 && Now64.Minute == 56 && Now64.Second == 01)
                {
                    Kernel.SendWorldMessage(
                        new Network.GamePackets.MsgTalk(
                            "5 Minutes left till Elite GuildWar End Hurry kick other Guild's Ass!.",
                            System.Drawing.Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                }
                if (Now64.Hour == 18 && Now64.Minute == 01 && Now64.Second == 59)
                {
                    EliteGuildWar.End();
                }

            }
            #endregion
            #region ElitePK Tournament[14:55 to 15:00]
            if (Now64.Hour == ElitePK.EventTime && Now64.Minute >= 55 && !ElitePKTournament.TimersRegistered)
            {
                ElitePKTournament.RegisterTimers();
                MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
                brackets.Type = MsgPKEliteMatchInfo.EPK_State;
                brackets.OnGoing = true;
                foreach (Client.GameState clients in Kernel.GamePool.Values)
                {
                    clients.Player.ClaimedElitePk = false;
                }
                Kernel.SendWorldMessage(new MsgTalk("ElitePK Tournament has started to signup go to TC ElitePKEnvoy in TwinCity!?", Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage), Server.GamePool);
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10533,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10533;
                        client.Send(alert.ToArray());
                    }
                    #region RemoveTopElite
                    var EliteChampion = MsgTitle.Titles.ElitePKChamption_High;
                    var EliteSecond = MsgTitle.Titles.ElitePK2ndPlace_High;
                    var EliteThird = MsgTitle.Titles.ElitePK3ndPlace_High;
                    var EliteEightChampion = MsgTitle.Titles.ElitePKChamption_Low;
                    var EliteEightSecond = MsgTitle.Titles.ElitePK2ndPlace_Low;
                    var EliteEightThird = MsgTitle.Titles.ElitePK3ndPlace_Low;
                    var EliteEight = MsgTitle.Titles.ElitePKTopEight_Low;
                    if (client.Player.Titles.ContainsKey(EliteChampion))
                        client.Player.RemoveTopStatus((ulong)EliteChampion);
                    if (client.Player.Titles.ContainsKey(EliteSecond))
                        client.Player.RemoveTopStatus((ulong)EliteSecond);
                    if (client.Player.Titles.ContainsKey(EliteThird))
                        client.Player.RemoveTopStatus((ulong)EliteThird);
                    if (client.Player.Titles.ContainsKey(EliteEightChampion))
                        client.Player.RemoveTopStatus((ulong)EliteEightChampion);
                    if (client.Player.Titles.ContainsKey(EliteEightSecond))
                        client.Player.RemoveTopStatus((ulong)EliteEightSecond);
                    if (client.Player.Titles.ContainsKey(EliteEightThird))
                        client.Player.RemoveTopStatus((ulong)EliteEightThird);
                    if (client.Player.Titles.ContainsKey(EliteEight))
                        client.Player.RemoveTopStatus((ulong)EliteEight);
                    #endregion
                }
            }
            if (Now64.Hour >= ElitePK.EventTime + 1 && ElitePKTournament.TimersRegistered)
            {
                bool done = true;
                foreach (var epk in ElitePKTournament.Tournaments)
                    if (epk.Players.Count != 0)
                        done = false;
                if (done)
                {
                    ElitePKTournament.TimersRegistered = false;
                    MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
                    brackets.Type = MsgPKEliteMatchInfo.EPK_State;
                    brackets.OnGoing = false;
                    foreach (var client in Server.GamePool)
                        client.Send(brackets);
                }
            }
            #endregion
            #region ClanWar[16:00 to 17:00 and 22:00 to 23:00]
            if (!ClanWar.IsWar)
            {
                if (Now64.Hour == 15 && Now64.Minute == 59 && Now64.Second == 59 || Now64.Hour == 21 && Now64.Minute == 59 && Now64.Second == 59)
                {
                    ClanWar.Start();
                }
            }
            if (ClanWar.IsWar)
            {
                if (Time32.Now > ClanWar.ScoreSendStamp.AddSeconds(3))
                {
                    ClanWar.ScoreSendStamp = Time32.Now;
                    ClanWar.SendScores();
                }
            }
            if (ClanWar.IsWar)
            {
                if (Now64.Hour == 16 && Now64.Minute == 59 && Now64.Second == 59 || Now64.Hour == 22 && Now64.Minute == 59 && Now64.Second == 59)
                {
                    ClanWar.End();
                }
            }
            #endregion
            #region TeamPk Tournament[18:55 to 19:00] -> 35%
            if (Now64.Hour == 18 && Now64.Minute == 55 && Now64.Second <= 0)
            {
                TeamElitePk.TeamTournament.Open();
                foreach (Client.GameState clients in Kernel.GamePool.Values)
                {
                    clients.Player.ClaimedTeamPK = false;
                }
                Kernel.SendWorldMessage(new MsgTalk("TeamPk Tournament has started to signup go to TC TeamPkManager in TwinCity.", Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage), Server.GamePool);
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10543,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10543;
                        client.Send(alert.ToArray());
                    }
                }
            }
            #endregion
            #region SkillTeamPk Tournament[19:55 to 20:00]
            if (Now64.Hour == 19 && Now64.Minute == 55 && Now64.Second <= 0)
            {
                TeamElitePk.SkillTeamTournament.Open();
                foreach (Client.GameState clients in Kernel.GamePool.Values)
                {
                    clients.Player.ClaimedSTeamPK = false;
                }
                Kernel.SendWorldMessage(new MsgTalk("SkillTeamPk Tournament has started to signup go to TC SkillPkManager in TwinCity.", Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage), Server.GamePool);
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10541,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10541;
                        client.Send(alert.ToArray());
                    }
                }
            }
            #endregion 
            #region GuildWar
            if (GuildWar.IsWar)
            {
                if (Time32.Now > GuildWar.ScoreSendStamp.AddSeconds(3))
                {
                    GuildWar.ScoreSendStamp = Time32.Now;
                    GuildWar.SendScores();
                }
            }
            if ((Now64.Hour == 20 && Now64.Minute >= 0 && Now64.Second <= 0))
            {
                if (!GuildWar.IsWar)
                {
                    GuildWar.Start();
                }
            }
            if (GuildWar.IsWar)
            {
                if (Now64.Hour == 21 && Now64.Minute >= 0 && Now64.Second >= 1)
                {
                    GuildWar.Flame10th = false;
                    GuildWar.End();
                }
            }
            #endregion
            #region Kingdom - Friday[21:00 to 22:00]
            if (Kingdom.IsWar)
            {
                if (Time32.Now > Kingdom.ScoreSendStamp.AddSeconds(3))
                {
                    Kingdom.ScoreSendStamp = Time32.Now;
                    Kingdom.SendScores();
                }
            }
            if (Now64.DayOfWeek == DayOfWeek.Friday && Now64.Hour == 21 && Now64.Minute == 00 && Now64.Second <= 1)
            {
                if (!Kingdom.IsWar)
                {
                    Kingdom.Start();
                    foreach (var client in Server.GamePool)
                    {
                        if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                        {
                            MsgInviteTrans alert = new MsgInviteTrans
                            {
                                InviteID = 10559,
                                Countdown = 60,
                                Action = 1
                            };
                            client.Player.InviteID = 10559;
                            client.Send(alert.ToArray());
                        }
                    }
                }
            }
            if (Kingdom.IsWar)
            {
                if (Now64.Hour == 22 && Now64.Second <= 2)
                {
                    Kingdom.End();
                }
            }
            #endregion
            #region TopSpouse - Friday[20:00 to 20:15 and 10:00 to 10:15]
            if (Now64.DayOfWeek == DayOfWeek.Friday && Now64.Hour == 21 && Now64.Minute == 00 && Now64.Second <= 1)
            {
                Player.name = new object[] { "Couples PkWar has started! You have 5 minute to signup go to TC CouplesPkGuide in TwinCity!" };
                Kernel.SendWorldMessage(new MsgTalk(string.Concat(Player.name), "ALLUSERS", "[Couples PkWar]", System.Drawing.Color.Red, 2500), Server.GamePool);
                foreach (var client in Server.GamePool)
                {
                    if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                    {
                        MsgInviteTrans alert = new MsgInviteTrans
                        {
                            InviteID = 10555,
                            Countdown = 60,
                            Action = 1
                        };
                        client.Player.InviteID = 10555;
                        client.Send(alert.ToArray());
                    }
                }
            }
            #endregion
        }
        private void CharactersCallback(GameState client, int time)
        {

            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            DateTime Now64 = DateTime.Now;
          
            if (client.Player.HandleTiming)
            {
                #region Box Times
                if ((client.Player.BoxOpened == true) && (DateTime.Now > client.Player.BoxTime.AddSeconds(5.0)))
                {
                    client.Player.BoxOpened = false;
                }
                #endregion
                #region ClearNulledItems
                if ((DateTime.Now.Second == 0) || (DateTime.Now.Second == 5))
                {
                    DB.ConquerItemTable.ClearNulledItems();
                }
                #endregion
                #region OnlinePoints
                if (Time32.Now > client.Player.OnlinePointStamp.AddMinutes(2))
                {
                    if(DateTime.Now.Hour >= 14 && DateTime.Now.Hour <= 15 && DateTime.Now.Minute == 00 || DateTime.Now.Hour >= 19 && DateTime.Now.Hour <= 20 && DateTime.Now.Minute == 00 || DateTime.Now.Hour >= 22 && DateTime.Now.Hour <= 23 && DateTime.Now.Minute == 00)
                    {
                        client.Player.OnlinePoints += 21;
                        client.Player.OnlinePointStamp = Time32.Now;
                        client.Send(new MsgTalk("Congratulations! " + client.Player.Name + " You Get + 21 Online Points FOR CRAZYHOUR, Stay Online and Get More Every Minute!", System.Drawing.Color.Red, MsgTalk.Whisper));
                    }
                    client.Player.OnlinePoints += 1;
                    client.Player.OnlinePointStamp = Time32.Now;
                    client.Send(new MsgTalk("Congratulations! " + client.Player.Name + " You Get + 1 Online Points, Stay Online and Get More Every Minute!", System.Drawing.Color.Red, MsgTalk.Whisper));
                    //if (client.Entity.VIPLevel > 6)
                    //{
                    //    uint lol = (uint)Kernel.Random.Next(1, 3);
                    //    client.Send(new Message("Fecilidades! " + client.Entity.Name + " has obtenido " + lol + " Puntos de cazador por estar conectado 15 minutos.", System.Drawing.Color.Red, Message.FirstRightCorner));
                    //}
                }
                #endregion
                #region ACOMODACION AUTOMATICA Y CONSTANTE DE ATRIBUTOS
                if (client.Player.Atributes + client.Player.Agility + client.Player.Strength + client.Player.Vitality + client.Player.Spirit + client.Player.MysteryFruit >= 1301)
                {
                    client.Player.Atributes = 1300;
                }
                #endregion
                #region WardRobe
                if (client.WardRobe != null)
                    client.WardRobe.CheckUpUser(client);
                #endregion
                #region Eventos
                #region  DailyPk[00:04]
                if (DateTime.Now.Minute == 00 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("Daily PkWar held do you want to join?", user =>
                    {
                        if (DateTime.Now.Minute >= 00 && DateTime.Now.Minute <= 04)
                        {
                            user.Player.Teleport(Pezzi.ServerEvents.DailyMap.ID, 150, 162);
                        }
                    });
                }
                #endregion
                #region BittleCPs[15:20]
                //if (DateTime.Now.Minute == 15 && DateTime.Now.Second == 1)
                //{
                //    if (DateTime.Now.Minute >= 15 && DateTime.Now.Minute <= 19)
                //    {
                //        BittleCPs.Sendinvite();
                //    }
                //}
                #endregion
                #region SS/FB[20:25]
                if (DateTime.Now.Minute == 20 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("ScentSword/FastBlade is about to begin! Do you want to join?", user =>
                    {
                        if (DateTime.Now.Minute >= 20 && DateTime.Now.Minute <= 24)
                        {
                            Random R = new Random();
                            int G = R.Next(1, 5);
                            if (G == 1) client.Player.Teleport(1707, 70, 70);
                            if (G == 2) client.Player.Teleport(1707, 67, 34);
                            if (G == 3) client.Player.Teleport(1707, 51, 73);
                            if (G == 4) client.Player.Teleport(1707, 33, 68);
                            if (G == 5) client.Player.Teleport(1707, 34, 33);
                        }
                    });
                }
                #endregion
                #region LastMan[25:30]
                if (DateTime.Now.Minute == 25 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("Last Man Standing PKWar is about to begin! Do you want to join?", user =>
                    {
                        if (DateTime.Now.Minute >= 25 && DateTime.Now.Minute <= 29)
                        {
                            Random R = new Random();
                            int G = R.Next(1, 5);
                            if (G == 1) user.Player.Teleport(Pezzi.ServerEvents.LastManStanding.ID, 70, 70);
                            if (G == 2) user.Player.Teleport(Pezzi.ServerEvents.LastManStanding.ID, 67, 34);
                            if (G == 3) user.Player.Teleport(Pezzi.ServerEvents.LastManStanding.ID, 51, 73);
                            if (G == 4) user.Player.Teleport(Pezzi.ServerEvents.LastManStanding.ID, 33, 68);
                            if (G == 5) user.Player.Teleport(Pezzi.ServerEvents.LastManStanding.ID, 34, 33);
                        }
                    });
                }
                #endregion
                #region IronMan[18:00 to 18:15 and at 8:00 to 8:15]
                if (Now64.Hour == 8 && Now64.Minute == 00 && Now64.Second == 10 || Now64.Hour == 18 && Now64.Minute == 00 && Now64.Second == 10)
                {
                    Kernel.SendWorldMessage(new MsgTalk("IronMan PK has Started you need to SignUp now find IronHead To Sign-up in Twin city at(421, 369)", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                    client.MessageBox("Iron PKWar is about to begin! Do you want to join?", user =>
                    {
                        if (Now64.Hour == 8 && Now64.Minute >= 00 && Now64.Minute <= 14 || Now64.Hour == 18 && Now64.Minute >= 00 && Now64.Minute <= 14)
                        {
                            var coords = Pezzi.ServerEvents.IronMap.RandomCoordinates();
                            user.Player.Teleport(Pezzi.ServerEvents.IronMap.ID, coords.Item1, coords.Item2);
                        }
                    });
                }
                #region Attention
                if (Now64.Hour == 8 && Now64.Minute == 14 && Now64.Second == 10 || Now64.Hour == 18 && Now64.Minute == 14 && Now64.Second == 10)
                {
                    Kernel.SendWorldMessage(new MsgTalk("IronMan PK will end at 8:15 or 18:15 please claim you prize fast!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                }
                #endregion
                #region Out From Map
                if (Now64.Minute == 15 && Now64.Second == 10 && client.Player.MapID == Pezzi.ServerEvents.IronMap.ID)
                {
                    client.Player.Teleport(1002, 300, 278);
                    Kernel.SendWorldMessage(new MsgTalk("You can`t signup to IronMan PK come again next Time!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                }
                #endregion
                #endregion
                #region ExtremePk[19:00 to 19:11 and at 9:00 to 9:11]
                if (DateTime.Now.Hour == 19 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 1 || DateTime.Now.Hour == 9 && DateTime.Now.Minute == 00 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("Extreme PkWar held do you want to join?", user =>
                    {
                        if (DateTime.Now.Hour == 19 && DateTime.Now.Minute >= 00 && DateTime.Now.Minute <= 10 || DateTime.Now.Hour == 9 && DateTime.Now.Minute >= 00 && DateTime.Now.Minute <= 10)
                        {
                            user.Player.Teleport(1002, 266, 218);
                        }
                    });
                }
                #endregion
                #region CP Castle[14:00 to 14:30]
                if ((Now64.Hour == 14) && (Now64.Minute == 00) && (Now64.Second == 00))
                {
                    Kernel.SendWorldMessage(new MsgTalk("CP Castle Started Now Go Faster!!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                }
                if ((Now64.Hour == 14) && (Now64.Minute == 30) && (Now64.Second == 00))
                {
                    if (client.Player.MapID == 3030)
                    {
                        client.Player.Teleport(1002, 300, 280);
                    }
                }
                if ((Now64.Hour == 14) && (Now64.Minute == 30) && (Now64.Second == 00))
                {
                    Kernel.SendWorldMessage(new MsgTalk("CP Castle Has been Ended !!", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Server.GamePool);
                }
                #endregion
                #region Betting[15:10 to 15:20]
                if (DateTime.Now.Hour == 15 && DateTime.Now.Minute == 10 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("Betting PkWar held do you want to join?", user =>
                    {
                        if (DateTime.Now.Hour == 15 && DateTime.Now.Minute >= 10 && DateTime.Now.Minute <= 19)
                        {
                            user.Player.Teleport(1002, 345, 232);
                        }
                    });
                }
                #endregion
                #region Tops
                #region OneHit
                if (DateTime.Now.Minute == 50 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("OneHit PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("OneHit PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region BlackName
                if (DateTime.Now.Minute == 38 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("BlackName PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("BlackName PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region ChampionRace
                if (DateTime.Now.Minute == 5 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("ChampionRace PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("ChampionRace PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region RedName
                if (DateTime.Now.Minute == 13 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("RedName PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("RedName PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region DeadWorld
                if (DateTime.Now.Minute == 18 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("DeadWorld PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("DeadWorld PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region Revenger
                if (DateTime.Now.Minute == 25 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("Revenger PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("Revenger PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region Life Pk
                if (DateTime.Now.Minute == 21 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("Life Pk PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("Life Pk PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region BigBosses
                if (DateTime.Now.Minute == 10 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("BigBosses PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("BigBosses PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region GentleWar
                if (DateTime.Now.Minute == 29 && DateTime.Now.Second == 1)
                {
                    client.MessageBox("");
                    Kernel.SendWorldMessage(new MsgTalk("GentleWar PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("GentleWar PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region CrazyWar
                if (DateTime.Now.Minute == 45 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("CrazyWar PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("CrazyWar PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #region ButchersWar
                if (DateTime.Now.Minute == 32 && DateTime.Now.Second == 1)
                {
                    Kernel.SendWorldMessage(new MsgTalk("ButchersWar PKWar began !", Color.White, (uint)PacketMsgTalk.MsgTalkType.Center), Kernel.GamePool.Values.ToArray());
                    foreach (var clientX in Kernel.GamePool.Values)
                        clientX.MessageBox("ButchersWar PKWar is about to begin! Do you want to join?",
                       p => { p.Player.Teleport(1002, 313, 292); }, null, 60);
                }
                #endregion
                #endregion
                #region Auto Spell
                #region Trojan
                if (client.Player.Class >= 10 && client.Player.Class <= 15)
                {
                    if (client.Player.Class >= 10 && client.Player.Class <= 15 && client.Player.Level >= 15)
                    {
                        if (!client.Spells.ContainsKey(11960))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11960 });//BreathFoucs
                        }
                        //if (!client.Spells.ContainsKey(11990))
                        //{
                        //    client.AddSpell(new Spell(true) { ID = 11990 });//Mortal Strike
                        //}
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 10 && client.Player.Class <= 15 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(11980))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11980 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(11970))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11970 });//Super Cyclone
                        }
                        if (!client.Spells.ContainsKey(1110))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1110 });//XPSkills
                        }
                        if (!client.Spells.ContainsKey(1015))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1015 });//XPSkills
                        }
                        if (!client.Spells.ContainsKey(1015))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1270 });//Hercules
                        }
                        if (!client.Spells.ContainsKey(1270))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1270 });//Golem
                        }
                        if (!client.Spells.ContainsKey(1190))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1190 });//Spritual Healing
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Worior
                if (client.Player.Class >= 20 && client.Player.Class <= 25)
                {
                    if (client.Player.Class >= 20 && client.Player.Class <= 25 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(11160))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11160 });//DefensiveStance
                        }
                        // client.AddSpell(new Spell(true) { ID = 11990 });//Mortal Strike
                    }
                    if (client.Player.Class >= 20 && client.Player.Class <= 25 && client.Player.Level >= 61)
                    {
                        if (!client.Spells.ContainsKey(11160))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11160 });//Dash
                        }
                        // client.AddSpell(new Spell(true) { ID = 11990 });//Mortal Strike
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 20 && client.Player.Class <= 25 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(1025))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1025 });//XpSkills
                        }
                        if (!client.Spells.ContainsKey(1020))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1020 });//XpSkills
                        }
                        if (!client.Spells.ContainsKey(1015))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1015 });//XpSkills
                        }
                        if (!client.Spells.ContainsKey(11200))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11200 });//MagicDefender
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Archer
                if (client.Player.Class >= 40 && client.Player.Class <= 45)
                {
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 1)
                    {
                        if (!client.Spells.ContainsKey(8002))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8002 });//XPFly
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 23)
                    {
                        if (!client.Spells.ContainsKey(8001))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8001 });//Scatter
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(8000))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8000 });//RapidFire
                        }
                        if (!client.Spells.ContainsKey(11620))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11620 });// path of shadow
                        }
                        if (!client.Spells.ContainsKey(11610))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11610 });// Blade~Flurry~(XP)
                        }
                        if (!client.Spells.ContainsKey(11660))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11660 });// Mortal~Wound~(Offensive)
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 50)
                    {
                        if (!client.Spells.ContainsKey(11590))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11590 });//Kinetic~Spark~(Passive)
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(8003))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8003 });//Fly
                        }
                        if (!client.Spells.ContainsKey(9000))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 9000 });//Intensify
                        }
                        if (!client.Spells.ContainsKey(8030))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8030 });//Arrow rain
                        }
                        if (!client.Spells.ContainsKey(11650))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11650 });//Blistering~Wave~(Mass)
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 90)
                    {
                        if (!client.Spells.ContainsKey(11670))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11670 });//Spirit~Focus~(Boost)
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 40 && client.Player.Class <= 45 && client.Player.Level >= 100)
                    {
                        if (!client.Spells.ContainsKey(8003))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 8003 });//Advanced Fly
                        }
                        if (!client.Spells.ContainsKey(11600))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11600 });//Dagger~Storm~(Mass)
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Ninja
                if (client.Player.Class >= 50 && client.Player.Class <= 55)
                {
                    if (client.Player.Class >= 50 && client.Player.Class <= 55 && client.Player.Level >= 15)
                    {
                        if (!client.Spells.ContainsKey(11230))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11230 });//GapingWounds
                        }
                        if (!client.Spells.ContainsKey(6010))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 6010 });//Xp Skill
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 50 && client.Player.Class <= 55 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(6010))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 6010 });//Xp Skill
                        }
                        if (!client.Spells.ContainsKey(11170))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11170 });//Scythe
                        }
                        if (!client.Spells.ContainsKey(12080))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12080 });// SuperTwofolds
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 50 && client.Player.Class <= 55 && client.Player.Level >= 50)
                    {
                        if (!client.Spells.ContainsKey(11180))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11180 });//MortalDrug
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 50 && client.Player.Class <= 55 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(6001))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 6001 });//Fog
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 50 && client.Player.Class <= 55 && client.Player.Level >= 110)
                    {
                        if (!client.Spells.ContainsKey(6004))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 6004 });//Advanced Fly
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Monk
                if (client.Player.Class >= 60 && client.Player.Class <= 65)
                {
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 5)
                    {
                        if (!client.Spells.ContainsKey(10490))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10490 });//Triple Attack
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 15)
                    {
                        if (!client.Spells.ContainsKey(10415))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10415 });//Whirlwind Kick
                        }
                        if (!client.Spells.ContainsKey(10390))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10390 });//Oblivion Xp Skill
                        }
                        //Database.SkillTable.SaveSpells(client); ;
                    }
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 20)
                    {
                        if (!client.Spells.ContainsKey(10395))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10395 });//Tyrant
                        }
                        if (!client.Spells.ContainsKey(10410))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10410 });//Fend
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(10381))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10381 });//Radiant Palm
                        }
                        if (!client.Spells.ContainsKey(10400))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10400 });//Serenity
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(10425))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10425 });//Tranquility
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 60 && client.Player.Class <= 65 && client.Player.Level >= 100)
                    {
                        if (!client.Spells.ContainsKey(10430))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10430 });//Compassion
                        }
                        if (!client.Spells.ContainsKey(10420))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10420 });//Metal
                        }
                        if (!client.Spells.ContainsKey(10421))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10421 });//Wood
                        }
                        if (!client.Spells.ContainsKey(10422))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10422 });//Water
                        }
                        if (!client.Spells.ContainsKey(10423))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10423 });//Fire
                        }
                        if (!client.Spells.ContainsKey(10424))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10424 });//Earth
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Priate
                if (client.Player.Class >= 70 && client.Player.Class <= 75)
                {
                    if (client.Player.Class >= 70 && client.Player.Class <= 75 && client.Player.Level >= 15)
                    {
                        if (!client.Spells.ContainsKey(11110))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11110 });//BladeTempest
                        }
                        if (!client.Spells.ContainsKey(11050))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11050 });//CannonBarrage
                        }
                        if (!client.Spells.ContainsKey(11060))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11060 });//BlackbeardRage 
                        }
                        if (!client.Spells.ContainsKey(11140))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11140 });//WindStorm
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 70 && client.Player.Class <= 75 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(11070))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11070 });//GaleBomb
                        }
                        if (!client.Spells.ContainsKey(11100) && client.Player.FirstRebornClass == 75 && client.Player.Reborn == 1)
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11100 });//KrakensRevenge
                        }
                        if (!client.Spells.ContainsKey(11120))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11120 });// BlackSpot
                        }
                        if (!client.Spells.ContainsKey(11130))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11130 });// AdrenalineRush
                        }
                        if (!client.Spells.ContainsKey(11030))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 11030 });// EagleEye
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Dragon-Warrior
                if (client.Player.Class >= 80 && client.Player.Class <= 85)
                {
                    if (client.Player.Class >= 80 && client.Player.Class <= 85 && client.Player.Level >= 5)
                    {
                        if (!client.Spells.ContainsKey(12240))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12240 });//Dragon~Punch
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 80 && client.Player.Class <= 85 && client.Player.Level >= 15)
                    {
                        if (!client.Spells.ContainsKey(12240))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12240 });//Dragon~Strides
                        }
                        if (!client.Spells.ContainsKey(12240))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12330 });//Dragon~Strides
                        }
                        if (!client.Spells.ContainsKey(12240))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12340 });//Dragon~Strides
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 80 && client.Player.Class <= 85 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(12270))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12270 });//Dragon~Flow
                        }
                        if (!client.Spells.ContainsKey(12120))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12120 });//Dragon Kicks
                        }
                        if (!client.Spells.ContainsKey(12130))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12130 });//Dragon Kicks
                        }
                        if (!client.Spells.ContainsKey(12140))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12140 });//Dragon Kicks
                        }
                        if (!client.Spells.ContainsKey(12290))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12290 });//Dragon~Cyclone
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 80 && client.Player.Class <= 85 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(12160))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12160 });//Cracking~Swipe
                        }
                        if (!client.Spells.ContainsKey(12170))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12170 });//Cracking~Swipe
                        }
                        if (!client.Spells.ContainsKey(12280))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12280 });//Dragon~Roar
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 80 && client.Player.Class <= 85 && client.Player.Level >= 100)
                    {
                        if (!client.Spells.ContainsKey(12280))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 12350 });//Dragon~Slash
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Toaist
                if (client.Player.Class >= 100 && client.Player.Class <= 145)
                {
                    if (client.Player.Class >= 100 && client.Player.Class <= 145 && client.Player.Level >= 1)
                    {
                        if (!client.Spells.ContainsKey(1005))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1005 });
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 100 && client.Player.Class <= 145 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(1195))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1195 });
                        }
                        if (!client.Spells.ContainsKey(10309))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 10309 });
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Water
                if (client.Player.Class >= 130 && client.Player.Class <= 135)
                {
                    if (client.Player.Class >= 130 && client.Player.Class <= 135 && client.Player.Level >= 1)
                    {
                        if (!client.Spells.ContainsKey(1125))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1125 });//BreathFoucs
                        }
                        if (!client.Spells.ContainsKey(1010))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1010 });//Mortal Strike
                        }
                        if (!client.Spells.ContainsKey(5001))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 5001 });//Mortal Strike
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 130 && client.Player.Class <= 135 && client.Player.Level >= 70)
                    {
                        if (!client.Spells.ContainsKey(1170))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1170 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1175))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1175 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1100))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1100 });//Fatal Cross
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 130 && client.Player.Class <= 135 && client.Player.Level >= 40)
                    {
                        if (!client.Spells.ContainsKey(1050))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1050 });//Fatal Cross.
                        }
                        if (!client.Spells.ContainsKey(1050))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1050 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1095))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1095 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1090))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1090 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1085))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1085 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1075))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1075 });//Fatal Cross
                        }
                        if (!client.Spells.ContainsKey(1055))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1055 });//Fatal Cross
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #region Fire
                if (client.Player.Class >= 140 && client.Player.Class <= 145)
                {
                    if (client.Player.Class >= 130 && client.Player.Class <= 135 && client.Player.Level >= 81)
                    {
                        if (!client.Spells.ContainsKey(1002))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1002 });//BreathFoucs
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 140 && client.Player.Class <= 145 && client.Player.Level >= 48)
                    {
                        if (!client.Spells.ContainsKey(1165))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1165 });//BreathFoucs
                        }
                        if (!client.Spells.ContainsKey(1120))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1120 });//BreathFoucs
                        }
                        if (!client.Spells.ContainsKey(1180))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1180 });//BreathFoucs
                        }
                        if (!client.Spells.ContainsKey(1150))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1150 });//BreathFoucs
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                    if (client.Player.Class >= 140 && client.Player.Class <= 145 && client.Player.Level >= 43)
                    {
                        if (!client.Spells.ContainsKey(1160))
                        {
                            client.AddSpell(new MsgMagicInfo(true) { ID = 1160 });//BreathFoucs
                        }
                        //Database.SkillTable.SaveSpells(client);
                    }
                }
                #endregion
                #endregion
              
                #endregion
                #region Cps por Tiempo
                //if (Time32.Now > client.Player.OnlinePointStamp.AddSeconds(1))
                //{
                //    client.Player.OnlinePoints += 1;
                //    client.Player.ConquerPoints += 10;
                //    client.KingDomTitle += 5;
                //    client.Player.OnlinePointStamp = Time32.Now;
                //}
                #endregion
                #region BlackbeardsRage
                if (Now >= client.Player.BlackbeardsRageStamp.AddSeconds(60))
                {
                    client.Player.RemoveFlag2((ulong)PacketFlag.Flags.BlackbeardsRage);
                }
                #endregion
                #region CannonBarrage
                if (Now >= client.Player.CannonBarrageStamp.AddSeconds(60))
                {
                    client.Player.RemoveFlag2((ulong)PacketFlag.Flags.CannonBarrage);
                }
                #endregion
                #region GuildRequest
                if (Now > client.Player.GuildRequest.AddSeconds(30))
                {
                    client.GuildJoinTarget = 0;
                }
                #endregion
                #region EnlightenPoints
                if (client.Player.EnlightenPoints >= 100)
                {
                    client.Player.Update((byte)PacketFlag.DataType.EnlightPoints, client.Player.EnlightenPoints, true);
                }
                else if ((client.Player.EnlightenPoints < 100) && client.Player.ContainsFlag((byte)PacketFlag.DataType.EnlightPoints))
                {
                    client.Player.RemoveFlag((byte)PacketFlag.DataType.EnlightPoints);
                }
                #endregion
                #region SnowBanshee
                //if (Now64.Minute == 27 && Now64.Second == 05 && Kernel.SpawnBanshee == false ||
                //    Now64.Minute == 57 && Now64.Second == 05 && Kernel.SpawnBanshee == false)
                //{
                //    DB.MonsterSpawn.StartSnowBanshee(client);
                //}
                #endregion
                #region NemesisTyrant
                //if (Now64.Minute == 15 && Now64.Second == 05 && Kernel.SpawnNemesis == false ||
                //    Now64.Minute == 45 && Now64.Second == 05 && Kernel.SpawnNemesis == false)
                //{
                //    DB.MonsterSpawn.StartNemesisTyrant(client);
                //}
                #endregion
                #region MentorPrizeSave
                if (Now > client.LastMentorSave.AddSeconds(5))
                {
                    DB.KnownPersons.SaveApprenticeInfo(client.AsApprentice);
                    client.LastMentorSave = Now;
                }
                #endregion
                
                #region Attackable
                if (client.JustLoggedOn)
                {
                    client.JustLoggedOn = false;
                    client.ReviveStamp = Now;
                }
                if (!client.Attackable)
                {
                    if (Now > client.ReviveStamp.AddSeconds(5))
                    {
                        client.Attackable = true;
                    }
                }
                #endregion
                #region DoubleExperience
                if (client.Player.DoubleExperienceTime > 0)
                {
                    if (Now > client.Player.DoubleExpStamp.AddMilliseconds(1000))
                    {
                        client.Player.DoubleExpStamp = Now;
                        client.Player.DoubleExperienceTime--;
                    }
                }
                #endregion
                #region ExpProtectionTime
                if (client.Player.ExpProtectionTime > 0)
                {
                    if (Now > client.Player.ProtectionStamp.AddMilliseconds(1000))
                    {
                        client.Player.ProtectionStamp = Now;
                        client.Player.ExpProtectionTime--;
                    }
                }
                #endregion
                #region HeavenBlessing
                if (client.Player.HeavenBlessing > 0)
                {
                    if (Now > client.Player.HeavenBlessingStamp.AddMilliseconds(1000))
                    {
                        client.Player.HeavenBlessingStamp = Now;
                        client.Player.HeavenBlessing--;
                    }
                }
                #endregion
                #region ShiledBreak
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.ShieldBreak))
                    if (Now >= client.Player.ShieldBreak.AddSeconds(10))
                    {
                        client.Player.Block = (ushort)client.Player.BlockBreak;
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.ShieldBreak);
                    }
                #endregion
                #region DivineGuard
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DivineGuard))
                    if (Now >= client.Player.GuardDefenseStamp.AddSeconds(10))
                    {
                        client.Player.Defence = client.Player.GuardDefense;
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.DivineGuard);
                    }
                #endregion
                #region Flags
                #region EpicWarrior
                //ManiacDance
                #region ManiacDance
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.ManiacDance))
                {
                    if (Now > client.Player.ManiacDanceStamp.AddSeconds(15))
                    {
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.ManiacDance);
                    }
                    else
                    {
                        var spell = DB.SpellTable.GetSpell(12700, client);
                        if (spell != null)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            var attack = new MsgInteract(true);
                            attack.Attacker = suse.Attacker = client.Player.UID;
                            suse.SpellID = spell.ID;
                            suse.SpellLevel = spell.Level;
                            suse.X = client.Player.X;
                            suse.Y = client.Player.Y;

                            foreach (Interfaces.IMapObject _obj in client.Screen.Objects)
                            {
                                if (_obj == null)
                                    continue;
                                if (_obj.MapObjType == MapObjectType.Monster ||
                                    _obj.MapObjType == MapObjectType.Player)
                                {
                                    var attacked = _obj as Player;
                                    if (Kernel.GetDistance(client.Player.X, client.Player.Y, attacked.X, attacked.Y) <=
                                        spell.Range)
                                    {
                                        if (Game.Attacking.Handle.CanAttack(client.Player, attacked, spell,
                                            attack.InteractType == MsgInteract.Melee))
                                        {
                                            uint damage = Game.Attacking.Calculate.Melee(client.Player, attacked, ref attack) / 2;
                                            //  damage = (UInt32)((damage * 20) / 100);
                                            suse.Effect = attack.Effect;

                                            Game.Attacking.Handle.ReceiveAttack(client.Player, attacked, attack, ref damage, spell);

                                            suse.AddTarget(attacked.UID, damage, attack);
                                        }
                                    }
                                }
                            }
                            client.SendScreen(suse, true);
                        }
                    }
                }
                #endregion
                #region BackFire
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.BackFire))
                {
                    if (Now > client.Player.BackFireStamp.AddSeconds(10))
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.BackFire);
                }
                #endregion
                #endregion
                #region WindWalker
                #region HealingSnow
                if (client.Player.ContainsFlag4((ulong)PacketFlag.Flags.HealingSnow))
                {
                    if (Now > client.Player.HealingSnow.AddSeconds(5))
                    {
                        client.Player.HealingSnow = Time32.Now;
                        client.Player.HealingRate = (int)DB.SpellTable.GetSpell(12950, client).NeedExperience;
                        client.Player.Hitpoints = (uint)Math.Min(client.Player.MaxHitpoints, client.Player.Hitpoints + client.Player.HealingRate);
                        client.Player.Mana = (ushort)Math.Min(client.Player.MaxMana, client.Player.Mana + client.Player.HealingRate);
                    }
                }
                #endregion
                #region Omnipotence
                if (Now > client.Player.OmnipotenceStamp.AddSeconds(40))
                    client.Player.RemoveFlag4((ulong)PacketFlag.Flags.Omnipotence);
                #endregion
                #region GreenEffect
                if (client.Player.ContainsFlag4((ulong)PacketFlag.Flags.JusticeChant))
                    if (client.Player.Stamina < 100)
                    {
                        if (Now > client.Player.GreenEffectStamp.AddSeconds(5))
                        {
                            client.Player.GreenEffectStamp = Time32.Now;
                            switch (client.Player.Action)
                            {
                                case Enums.ConquerAction.Sit:
                                    {
                                        client.Player.Stamina += 10;
                                        break;
                                    }
                                default:
                                    {
                                        client.Player.Stamina += 5;
                                        break;
                                    }
                            }
                            WindWalker.SendGreenEffect(client);
                        }
                    }
                #endregion
                #region RevengeTail
                if (client.Player.ContainsFlag4((ulong)PacketFlag.Flags.RevengeTaill))
                    if (Now > client.Player.RevengeTailStamp.AddSeconds(10))
                        client.Player.RemoveFlag4((ulong)PacketFlag.Flags.RevengeTaill);
                #endregion
                #region xFreezingPelter
                if (client.Player.ContainsFlag4((ulong)PacketFlag.Flags.xFreezingPelter))
                    if (client.Player.xFreezing > 0)
                        client.Player.xFreezing--;
                    else
                        client.Player.RemoveFlag4((ulong)PacketFlag.Flags.xFreezingPelter);
                #endregion
                #region xChillingSnow
                if (client.Player.ContainsFlag4((ulong)PacketFlag.Flags.xChillingSnow))
                    if (client.Player.xChilling > 0)
                        client.Player.xChilling--;
                    else
                        client.Player.RemoveFlag4((ulong)PacketFlag.Flags.xChillingSnow);
                #endregion
                #endregion
                #region JiangHu
                if (client.Player.MyKongFu != null)
                {
                    client.Player.MyKongFu.TheadTime(client);
                }
                #endregion
                #region DefensiveStance
                if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.DefensiveStance))
                {
                    if (Time32.Now > client.Player.DefensiveStanceStamp.AddSeconds(client.Player.DefensiveStance))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.DefensiveStance);
                        client.Player.DefensiveStance = 0;
                    }
                }
                #endregion
                #region MagicDefender
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.MagicDefender))
                {
                    if (Time32.Now > client.Player.MagicDefenderStamp.AddSeconds(client.Player.MagicDefender))
                    {
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.MagicDefender);
                        if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.MagicDefenderIcon))
                            client.Player.RemoveFlag2((ulong)PacketFlag.Flags.MagicDefenderIcon);
                        client.Player.MagicDefender = 0;
                    }
                }
                #endregion
                #region BlockShield
                if (client.Player.BlockShieldCheck)
                {
                    if (Now > client.Player.BlockShieldStamp.AddSeconds(client.Player.BlockShield))
                    {
                        client.Player.BlockShieldCheck = false;
                        client.ReloadBlock();
                    }
                }
                #endregion
                #region ScurvyBomb
                if (client.Player.OnScurvyBomb())
                {
                    if (Now > client.Player.ScurbyBombStamp.AddSeconds(client.Player.ScurbyBomb))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.ScurvyBomb);
                        MsgUpdate upgrade = new MsgUpdate(true);
                        upgrade.UID = client.Player.UID;
                        upgrade.Append((byte)PacketFlag.DataType.Fatigue, 0, 0, 0, 0);
                        client.Send(upgrade.ToArray());
                    }
                    else if (Now > client.Player.ScurbyBomb2Stamp.AddSeconds(2))
                    {
                        if (client.Player.Stamina >= 5)
                        {
                            client.Player.Stamina -= 5;
                            client.Player.ScurbyBomb2Stamp = Time32.Now;
                            client.Player.AddFlag2((ulong)PacketFlag.Flags.ScurvyBomb);
                        }
                        client.Player.Stamina = client.Player.Stamina;
                        client.Player.ScurbyBomb2Stamp = Time32.Now;
                    }
                }
                #endregion
                #region Intensify
                if (client.Player.IntensifyPercent != 0)
                {
                    if (Now > client.Player.IntensifyStamp.AddSeconds(5))
                    {
                        client.Player.AddFlag((ulong)PacketFlag.Flags.Intensify);
                    }
                }
                #endregion
                #region Stun
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Stun))
                {
                    if (Now > client.Player.ShockStamp.AddSeconds(client.Player.Shock))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Stun);
                    }
                }
                #endregion
                #region Bless
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
                {
                    if (client.BlessTime <= 7198500)
                        client.BlessTime += 1000;
                    else
                        client.BlessTime = 7200000;
                    client.Player.Update((byte)PacketFlag.DataType.LuckyTimeTimer, client.BlessTime, false);
                }
                else if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
                {
                    if (client.PrayLead != null)
                    {
                        if (client.PrayLead.Socket.IsAlive)
                        {
                            if (client.BlessTime <= 7199000)
                                client.BlessTime += 500;
                            else
                                client.BlessTime = 7200000;
                            client.Player.Update((byte)PacketFlag.DataType.LuckyTimeTimer, client.BlessTime, false);
                        }
                        else
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                    }
                    else
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                }
                else
                {
                    if (client.BlessTime > 0)
                    {
                        if (client.BlessTime >= 500)
                            client.BlessTime -= 500;
                        else
                            client.BlessTime = 0;
                        client.Player.Update((byte)PacketFlag.DataType.LuckyTimeTimer, client.BlessTime, false);
                    }
                }
                #endregion
                #region FlashingName
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.FlashingName))
                {
                    if (Now > client.Player.FlashingNameStamp.AddSeconds(client.Player.FlashingNameTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.FlashingName);
                    }
                }
                #endregion
                #region XPList
                if (!client.Player.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                {
                    if (Now > client.XPCountStamp.AddSeconds(3))
                    {
                        #region Arrows
                        if (client.Equipment != null)
                        {
                            if (!client.Equipment.Free(5))
                            {
                                if (ItemHandler.IsArrow(client.Equipment.TryGetItem(5).ID))
                                {
                                    ConquerItemTable.UpdateDurabilityItem(client.Equipment.TryGetItem(5));
                                }
                            }
                        }
                        #endregion
                        client.XPCountStamp = Now;
                        client.XPCount++;
                        if (client.XPCount >= 100)
                        {
                            client.XPCount = 0;
                            if (client.Player.InHangUp) return;
                            client.Player.AddFlag((ulong)PacketFlag.Flags.XPList);
                            client.XPListStamp = Now;
                        }
                    }
                }
                else if (Now > client.XPListStamp.AddSeconds(20))
                {
                    client.Player.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                }
                #endregion
                #region KoSpell
                if (client.Player.OnKOSpell())
                {
                    if (client.Player.OnCyclone())
                    {
                        int Seconds = Now.AllSeconds() - client.Player.CycloneStamp.AddSeconds(client.Player.CycloneTime).AllSeconds();
                        if (Seconds >= 1)
                        {
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Cyclone);
                        }
                    }
                    if (client.Player.OnSuperman())
                    {
                        int Seconds = Now.AllSeconds() - client.Player.SupermanStamp.AddSeconds(client.Player.SupermanTime).AllSeconds();
                        if (Seconds >= 1)
                        {
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Superman);
                        }
                    }
                    if (client.Player.OnSuperCyclone())
                    {
                        int Seconds = Now.AllSeconds() - client.Player.SuperCycloneStamp.AddSeconds(client.Player.SuperCycloneTime).AllSeconds();
                        if (Seconds >= 1)
                        {
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.SuperCyclone);
                        }
                    }
                    if (client.Player.OnDragonCyclone())
                    {
                        int Seconds = Now.AllSeconds() - client.Player.CycloneStamp.AddSeconds(client.Player.CycloneTime).AllSeconds();
                        if (Seconds >= 1)
                        {
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.DragonCyclone);
                        }
                    }
                    if (!client.Player.OnKOSpell())
                    {
                        KoBoard.New(client.Player.Name, client.Player.KOCount);
                        client.Player.KOCount = 0;
                    }
                }
                #endregion
               
                #region Stigma
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Stigma))
                {
                    if (Now >= client.Player.StigmaStamp.AddSeconds(client.Player.StigmaTime))
                    {
                        client.Player.StigmaTime = 0;
                        client.Player.StigmaIncrease = 0;
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Stigma);
                    }
                }
                #endregion
                #region Dodge
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Dodge))
                {
                    if (Now >= client.Player.DodgeStamp.AddSeconds(client.Player.DodgeTime))
                    {
                        client.Player.DodgeTime = 0;
                        client.Player.DodgeIncrease = 0;
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Dodge);
                    }
                }
                #endregion
                #region Invisibility
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Invisibility))
                {
                    if (Now >= client.Player.InvisibilityStamp.AddSeconds(client.Player.InvisibilityTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Invisibility);
                    }
                }
                #endregion
                #region StarOfAccuracy
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.StarOfAccuracy))
                {
                    if (client.Player.StarOfAccuracyTime != 0)
                    {
                        if (Now >= client.Player.StarOfAccuracyStamp.AddSeconds(client.Player.StarOfAccuracyTime))
                        {
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.StarOfAccuracy);
                        }
                    }
                    else if (Now >= client.Player.AccuracyStamp.AddSeconds(client.Player.AccuracyTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.StarOfAccuracy);
                    }
                }
                #endregion
                #region RestoreStamine
                if (Now >= client.Player.ReStoreStamine.AddSeconds(8) && client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DragonFlow))
                {
                    client.Player.ReStoreStamine = Time32.Now;
                    byte percent = client.Player.RestorePercent;
                    int limit = 0;
                    if (client.Player.HeavenBlessing > 0)
                        limit = 50;
                    if (client.Player.Stamina != 100 + limit)
                    {
                        if (client.Player.Stamina <= (100 - percent) + limit)
                        {
                            client.Player.Stamina += percent;
                        }
                        else
                        {
                            if (client.Player.Stamina != 100 + limit)
                                client.Player.Stamina = (byte)(100 + limit);
                        }

                    }
                }
                #endregion
                #region MagicShield
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.MagicShield))
                {
                    if (client.Player.MagicShieldTime != 0)
                    {
                        if (Now >= client.Player.MagicShieldStamp.AddSeconds(client.Player.MagicShieldTime))
                        {
                            client.Player.MagicShieldIncrease = 0;
                            client.Player.MagicShieldTime = 0;
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.MagicShield);
                        }
                    }
                    else if (Now >= client.Player.ShieldStamp.AddSeconds(client.Player.ShieldTime))
                    {
                        client.Player.ShieldIncrease = 0;
                        client.Player.ShieldTime = 0;
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.MagicShield);
                    }
                }
                #endregion
                #region Fly
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                {
                    if (Now >= client.Player.FlyStamp.AddSeconds(client.Player.FlyTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Fly);
                        client.Player.FlyTime = 0;
                    }
                }
                #endregion
                #region PoisonStar
                if (client.Player.NoDrugsTime > 0)
                {
                    if (Now > client.Player.NoDrugsStamp.AddSeconds(client.Player.NoDrugsTime))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.PoisonStar);
                        client.Player.NoDrugsTime = 0;
                    }
                }
                #endregion
                #region ToxicFog
                if (client.Player.ToxicFogLeft > 0)
                {
                    if (Now >= client.Player.ToxicFogStamp.AddSeconds(2))
                    {
                        float Percent = client.Player.ToxicFogPercent;
                        if (client.Player.Detoxication != 0)
                        {
                            float immu = 1 - client.Player.Detoxication / 100F;
                            Percent = Percent * immu;
                        }
                        client.Player.ToxicFogLeft--;
                        if (client.Player.ToxicFogLeft == 0)
                        {
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                            return;
                        }
                        client.Player.ToxicFogStamp = Now;
                        if (client.Player.Hitpoints > 1)
                        {
                            uint damage = Game.Attacking.Calculate.Percent(client.Player, Percent);
                            if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.AzureShield))
                            {

                                if (damage > client.Player.AzureShieldDefence)
                                {
                                    damage -= client.Player.AzureShieldDefence;
                                    Game.Attacking.Calculate.CreateAzureDMG(client.Player.AzureShieldDefence, client.Player, client.Player);
                                    client.Player.RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                                }
                                else
                                {
                                    Game.Attacking.Calculate.CreateAzureDMG((uint)damage, client.Player, client.Player);
                                    client.Player.AzureShieldDefence -= (ushort)damage;
                                    client.Player.AzureShieldPacket();
                                    damage = 1;
                                }
                            }
                            else
                                client.Player.Hitpoints -= damage;

                            Network.GamePackets.MsgMagicEffect suse = new Network.GamePackets.MsgMagicEffect(true);
                            suse.Attacker = client.Player.UID;
                            suse.SpellID = 10010;
                            suse.AddTarget(client.Player.UID, damage, null);
                            client.SendScreen(suse, true);
                            if (client != null)
                                client.UpdateQualifier(client.ArenaStatistic.PlayWith, client, damage);

                        }
                    }
                }
                else
                {
                    if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Poisoned))
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                }
                #endregion
                //#region ToxicFog
                //if (client.Player.ToxicFogLeft > 0)
                //{

                //    if (Now >= client.Player.ToxicFogStamp.AddSeconds(2))
                //    {
                //        float Percent = client.Player.ToxicFogPercent;
                //        if (client.Player.Detoxication != 0)
                //        {
                //            float immu = 1 - client.Player.Detoxication / 100F;
                //            Percent = Percent * immu;
                //        }
                //        client.Player.ToxicFogLeft--;
                //        if (client.Player.ToxicFogLeft == 0)
                //        {
                //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                //            return;
                //        }
                //        client.Player.ToxicFogStamp = Now;
                //        if (client.Player.Hitpoints > 1)
                //        {
                //            uint damage = Game.Attacking.Calculate.Percent(client.Player, Percent);
                //            if (damage == 0)
                //            {
                //                client.Player.ToxicFogLeft = 0;
                //                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                //                return;
                //            }
                //            if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.AzureShield))
                //            {

                //                if (damage > client.Player.AzureShieldDefence)
                //                {
                //                    damage -= client.Player.AzureShieldDefence;
                //                    //  Game.Attacking.Calculate.CreateAzureDmg(client.Entity.AzureShieldDefence, client.Entity, client.Entity);
                //                    client.Player.RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                //                }
                //                else
                //                {
                //                    // Game.Attacking.Calculate.CreateAzureDmg((uint)damage, client.Entity, client.Entity);
                //                    client.Player.AzureShieldDefence -= (ushort)damage;
                //                    client.Player.AzureShieldPacket();
                //                    damage = 1;
                //                }
                //            }
                //            else
                //                client.Player.Hitpoints -= damage;

                //            Network.GamePackets.MsgMagicEffect suse = new Network.GamePackets.MsgMagicEffect(true);
                //            suse.Attacker = client.Player.UID;
                //            suse.SpellID = 10010;
                //            suse.AddTarget(client.Player, damage, null);
                //            client.SendScreen(suse, true);
                //            if (client != null)
                //                client.UpdateQualifier(damage);
                //        }
                //    }
                //}
                //else
                //{
                //    if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Poisoned))
                //        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                //}
                //#endregion
                #region FatalStrike
                if (client.Player.OnFatalStrike())
                {
                    if (Now > client.Player.FatalStrikeStamp.AddSeconds(client.Player.FatalStrikeTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.FatalStrike);
                    }
                }
                #endregion
                #region Oblivion
                if (client.Player.OnOblivion())
                {
                    if (Now > client.Player.OblivionStamp.AddSeconds(client.Player.OblivionTime))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.Oblivion);
                    }
                }
                #endregion
                #region ShurikenVortex
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.ShurikenVortex))
                {
                    if (Now > client.Player.ShurikenVortexStamp.AddSeconds(client.Player.ShurikenVortexTime))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.ShurikenVortex);
                    }
                }
                #endregion
                #region Transformations
                if (client.Player.Transformed)
                {
                    if (Now > client.Player.TransformationStamp.AddSeconds(client.Player.TransformationTime))
                    {
                        client.Player.Untransform();
                    }
                }
                #endregion
                #region SoulShackle
                if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.SoulShackle))
                {
                    if (Now > client.Player.ShackleStamp.AddSeconds(client.Player.ShackleTime))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.SoulShackle);
                    }
                }
                #endregion
                #region AzureShield
                if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.AzureShield))
                {
                    if (Now > client.Player.MagicShieldStamp.AddSeconds(client.Player.MagicShieldTime))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                    }
                }
                #endregion
                #region BladeFlurry
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.BladeFlurry))
                {
                    if (Time32.Now > client.Player.BladeFlurryStamp.AddSeconds(45))
                    {
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.BladeFlurry);
                    }
                }
                #endregion
                #region FreezeSmall
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.FreezeSmall))
                {
                    if (client.RaceFrightened)
                    {
                        if (Now > client.FrightenStamp.AddSeconds(20))
                        {
                            client.RaceFrightened = false;
                            {
                                MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                update.UID = client.Player.UID;
                                update.Remove(MsgRaceTrackStatus.Flustered);
                                client.SendScreen(update, true);
                            }
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.FreezeSmall);
                        }
                        else
                        {
                            int rand;
                            ushort x, y;
                            do
                            {
                                rand = Kernel.Random.Next(Map.XDir.Length);
                                x = (ushort)(client.Player.X + Map.XDir[rand]);
                                y = (ushort)(client.Player.Y + Map.YDir[rand]);
                            }
                            while (!client.Map.Floor[x, y, MapObjectType.Player]);
                            client.Player.Facing = Kernel.GetAngle(client.Player.X, client.Player.Y, x, y);
                            client.Player.X = x;
                            client.Player.Y = y;
                            client.SendScreen(new MsgSyncAction()
                            {
                                EntityCount = 1,
                                Facing = client.Player.Facing,
                                FirstEntity = client.Player.UID,
                                WalkType = 9,
                                X = client.Player.X,
                                Y = client.Player.Y,
                                MovementType = MsgSyncAction.Walk
                            }, true);
                        }
                    }
                }
                #endregion
                #region Stunned
                if (client.Player.Stunned)
                {
                    if (Now > client.Player.StunStamp.AddMilliseconds(2000))
                    {
                        client.Player.Stunned = false;
                    }
                }
                #endregion
                #region Freeze
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Freeze))
                {
                    if (Now > client.Player.FrozenStamp.AddSeconds(client.Player.FrozenTime))
                    {
                        client.Player.FrozenTime = 0;
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Freeze);
                        MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                        update.UID = client.Player.UID;
                        update.Remove(MsgRaceTrackStatus.Freeze);
                        client.SendScreen(update, true);
                    }
                }
                #endregion
                #region Stun
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Stun))
                {
                    if (client.RaceDizzy)
                    {
                        if (Now > client.DizzyStamp.AddSeconds(5))
                        {
                            client.RaceDizzy = false;
                            {
                                MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                update.UID = client.Player.UID;
                                update.Remove(MsgRaceTrackStatus.Dizzy);
                                client.SendScreen(update);
                            }
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Stun);
                        }
                    }
                }
                #endregion
                #region Mentor
                client.ReviewMentor();
                #endregion
                #region ChaosCycle
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.ChaosCycle))
                {
                    if (Now > client.FrightenStamp.AddSeconds(5))
                    {
                        client.RaceFrightened = false;
                        {
                            MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                            update.UID = client.Player.UID;
                            update.Remove(MsgRaceTrackStatus.Flustered);
                            client.SendScreen(update);
                        }
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.ChaosCycle);
                    }
                }
                #endregion
                #region IceBlock
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.FreezeSmall))
                {
                    if (Now > client.FrightenStamp.AddSeconds(client.Player.Fright))
                    {
                        MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                        update.UID = client.Player.UID;
                        update.Remove(MsgRaceTrackStatus.Dizzy);
                        client.SendScreen(update, true);
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.FreezeSmall);
                    }
                    else
                    {
                        int rand;
                        ushort x, y;
                        do
                        {
                            rand = Kernel.Random.Next(Map.XDir.Length);
                            x = (ushort)(client.Player.X + Map.XDir[rand]);
                            y = (ushort)(client.Player.Y + Map.YDir[rand]);
                        }
                        while (!client.Map.Floor[x, y, MapObjectType.Player]);
                        client.Player.Facing = Kernel.GetAngle(client.Player.X, client.Player.Y, x, y);
                        client.Player.X = x;
                        client.Player.Y = y;
                        client.SendScreen(new MsgSyncAction()
                        {
                            EntityCount = 1,
                            Facing = client.Player.Facing,
                            FirstEntity = client.Player.UID,
                            WalkType = 9,
                            X = client.Player.X,
                            Y = client.Player.Y,
                            MovementType = MsgSyncAction.Walk
                        }, true);
                    }
                }
                #endregion
                #region OrangeSparkles
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.OrangeSparkles))
                {
                    if (Time32.Now > client.RaceExcitementStamp.AddSeconds(15))
                    {
                        var upd = new MsgRaceTrackStatus(true)
                        {
                            UID = client.Player.UID
                        };
                        upd.Remove(MsgRaceTrackStatus.Accelerated);
                        client.SendScreen(upd);
                        client.SpeedChange = null;
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.SpeedIncreased);
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.OrangeSparkles);
                    }
                }
                #endregion
                #region PurpleSparkles
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.PurpleSparkles))
                {
                    if (Time32.Now > client.DecelerateStamp.AddSeconds(10))
                    {
                        {
                            client.RaceDecelerated = false;
                            var upd = new MsgRaceTrackStatus(true)
                            {
                                UID = client.Player.UID
                            };
                            upd.Remove(MsgRaceTrackStatus.Decelerated);
                            client.SendScreen(upd);
                            client.SpeedChange = null;
                        }
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.PurpleSparkles);
                    }
                }
                #endregion
                #region CarryingFlag
                if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.CarryingFlag))
                {
                    if (Time32.Now > client.Player.FlagStamp.AddSeconds(60))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.CarryingFlag);
                    }
                }
                #endregion
                #region DragonFury
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DragonFury))
                {
                    if (Time32.Now > client.Player.DragonFuryStamp.AddSeconds(client.Player.DragonFuryTime))
                    {
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.DragonFury);
                        MsgUpdate upgrade = new MsgUpdate(true);
                        upgrade.UID = client.Player.UID;
                        upgrade.Append((byte)PacketFlag.DataType.DragonFury, 0, 0, 0, 0);
                        client.Send(upgrade.ToArray());
                    }
                }
                #endregion
                #region DragonFlow
                if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DragonFlow))
                {
                    if (Time32.Now > client.Player.DragonFlowStamp.AddSeconds(8))
                    {
                        int stamina = 0;
                        if (client.Player.HeavenBlessing > 0) stamina = 50;
                        var spell = client.Spells[12270];
                        if (spell != null)
                        {
                            if (client.Player.Stamina != 100 + stamina)
                            {
                                var Spell = SpellTable.GetSpell(spell.ID, spell.Level);
                                client.Player.Update(MsgName.Mode.Effect, "leedragonblood", true);
                                client.Player.Stamina += (byte)Spell.Power;
                            }
                        }
                        client.Player.DragonFlowStamp = Time32.Now;
                    }
                }
                #endregion
                #region CursedTime
                if (client.Player.CursedTime > 0)
                {
                    if (Now > client.Player.Cursed.AddSeconds(1))
                    {
                        client.Player.Cursed = Time32.Now;
                        client.Player.CursedTime--;
                    }
                }
                else if (client.Player.CursedTime == 0)
                {
                    if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Cursed))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Cursed);
                    }
                }
                #endregion
                #region CheckTeamAura
                client.CheckTeamAura();
                #endregion
                #region TeamMemberPos
                if (client.Team != null)
                {
                    if (client.Player.MapID == client.Team.Lider.Player.MapID)
                    {
                        MsgAction Data = new MsgAction(true);
                        Data.UID = client.Team.Lider.Player.UID;
                        Data.dwParam = client.Team.Lider.Player.MapID;
                        Data.ID = PacketMsgAction.Mode.TeamMemberPos;
                        Data.X = client.Team.Lider.Player.X;
                        Data.Y = client.Team.Lider.Player.Y;
                        Data.Send(client);
                    }
                }
                #endregion
                #region FlameLayer
                if (client.Player.FlameLayerLeft > 0)
                {
                    if (Now >= client.Player.FlameLayerStamp.AddSeconds(2))
                    {
                        float Percent = client.Player.FlameLayerPercent;
                        if (client.Player.Detoxication != 0)
                        {
                            float immu = 1 - client.Player.Detoxication / 100F;
                            Percent = Percent * immu;
                        }
                        client.Player.FlameLayerLeft--;
                        if (client.Player.FlameLayerLeft == 0)
                        {
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer);
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer2);
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer3);
                            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer4);
                            return;
                        }
                        client.Player.FlameLayerStamp = Now;
                        if (client.Player.Hitpoints > 1)
                        {
                            uint damage = Game.Attacking.Calculate.Percent(client.Player, Percent);
                            if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.AzureShield))
                            {
                                if (damage > client.Player.AzureShieldDefence)
                                {
                                    damage -= client.Player.AzureShieldDefence;
                                    Game.Attacking.Calculate.CreateAzureDMG(client.Player.AzureShieldDefence, client.Player, client.Player);
                                    client.Player.RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                                }
                                else
                                {
                                    Game.Attacking.Calculate.CreateAzureDMG((uint)damage, client.Player, client.Player);
                                    client.Player.AzureShieldDefence -= (ushort)damage;
                                    client.Player.AzureShieldPacket();
                                    damage = 1;
                                }
                            }
                            else
                                client.Player.Hitpoints -= damage;
                            client.UpdateQualifier(damage, true);
                        }
                    }
                }
                else
                {
                    if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.FlameLayer))
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer);
                    if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.FlameLayer2))
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer2);
                    if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.FlameLayer3))
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer3);
                    if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.FlameLayer4))
                        client.Player.RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer4);
                }
                #endregion
                #endregion
                #region Enlightment
                if (client.Player.EnlightmentTime > 0)
                {
                    if (Now >= client.Player.EnlightmentStamp.AddMinutes(1))
                    {
                        client.Player.EnlightmentStamp = Now;
                        client.Player.EnlightmentTime--;
                        if (client.Player.EnlightmentTime % 10 == 0 && client.Player.EnlightmentTime > 0)
                            client.IncreaseExperience(Game.Attacking.Calculate.Percent((int)client.ExpBall, .10F), false);
                    }
                }
                #endregion
                #region PKPoints
                if (Now >= client.Player.PKPointDecreaseStamp.AddMinutes(6))
                {
                    client.Player.PKPointDecreaseStamp = Now;
                    if (client.Player.PKPoints > 0)
                    {
                        client.Player.PKPoints--;
                    }
                    else client.Player.PKPoints = 0;
                }
                #endregion
                #region OverHP
                if (client.Player.FullyLoaded)
                {
                    if (client.Player.Hitpoints > client.Player.MaxHitpoints && client.Player.MaxHitpoints > 1 && !client.Player.Transformed)
                    {
                        client.Player.Hitpoints = client.Player.MaxHitpoints;
                    }
                }
                #endregion
                #region Die Delay
                if (client.Player.Hitpoints == 0 && client.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead) && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ghost))
                {
                    if (Now > client.Player.DeathStamp.AddSeconds(2))
                    {
                        client.Player.AddFlag((ulong)PacketFlag.Flags.Ghost);
                        if (client.Player.Body % 10 < 3)
                            client.Player.TransformationID = 99;
                        else client.Player.TransformationID = 98;
                        client.SendScreenSpawn(client.Player, true);
                    }
                }
                #endregion
                #region ChainBoltActive
                if (client.Player.ContainsFlag2((ulong)PacketFlag.Flags.ChainBoltActive))
                {
                    if (Now > client.Player.ChainboltStamp.AddSeconds(client.Player.ChainboltTime))
                    {
                        client.Player.RemoveFlag2((ulong)PacketFlag.Flags.ChainBoltActive);
                    }
                }
                #endregion
                #region AutoHunting
                if (client.Player.AutoRev > 0)
                {
                    if (client.Player.HeavenBlessing > 0 && client.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead) && client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ghost))
                    {
                        if (Time32.Now > client.Player.AutoRevStamp.AddSeconds(20))
                        {
                            client.Player.Action = Enums.ConquerAction.None;
                            client.ReviveStamp = Time32.Now;
                            client.Attackable = false;
                            client.Player.TransformationID = 0;
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Dead);
                            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Ghost);
                            client.Player.Hitpoints = client.Player.MaxHitpoints;
                            client.Player.Mana = client.Player.MaxMana;
                            client.Player.AutoRev = 0;
                        }
                    }
                }
                #endregion
                #region AuroraLotus
                if (client.Spells.ContainsKey(12370))
                {
                    if (!client.Player.ContainsFlag3((ulong)PacketFlag.Flags.AuroraLotus))
                    {
                        client.Player.AuroraLotusEnergy = 0;
                        if (client.Player.Lotus(client.Player.AuroraLotusEnergy, (byte)PacketFlag.DataType.AuroraLotus))
                            client.Player.AddFlag3((ulong)PacketFlag.Flags.AuroraLotus);
                    }
                }
                #endregion
                #region FlameLotus
                if (client.Spells.ContainsKey(12380))
                {
                    if (!client.Player.ContainsFlag3((ulong)PacketFlag.Flags.FlameLotus))
                    {
                        client.Player.FlameLotusEnergy = 0;
                        if (client.Player.Lotus(client.Player.FlameLotusEnergy, (byte)PacketFlag.DataType.FlameLotus))
                            client.Player.AddFlag3((ulong)PacketFlag.Flags.FlameLotus);
                    }
                }
                #endregion
            }
        }
        private bool Valid(GameState client)
        {
            if (client.Fake) return false;
            if (!client.Socket.IsAlive || client.Player == null)
            {
                client.Disconnect();
                return false;
            }
            return true;
        }
        public bool Register(GameState client)
        {
            if (client.TimerSubscriptions == null)
            {
                client.TimerSyncRoot = new object();
                client.TimerSubscriptions = new IDisposable[]
                {
                    Characters.Add(client),
                    AutoAttack.Add(client),
                    Prayer.Add(client),
                    Pets.Add(client),
                };
                return true;
            }
            return false;
        }
        public void UnRegister(GameState client)
        {
            if (client.TimerSubscriptions == null) return;
            lock (client.TimerSyncRoot)
            {
                if (client.TimerSubscriptions != null)
                {
                    foreach (var timer in client.TimerSubscriptions)
                        timer.Dispose();
                    client.TimerSubscriptions = null;
                }
            }
        }
        private void PrayerCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            if (client.Player.Reborn > 1) return;
            if (client.Player.HandleTiming)
            {
                if (!client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
                {
                    foreach (Interfaces.IMapObject ClientObj in client.Screen.Objects)
                    {
                        if (ClientObj != null)
                        {
                            if (ClientObj.MapObjType == MapObjectType.Player)
                            {
                                var Client = ClientObj.Owner;
                                if (Client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
                                {
                                    if (Kernel.GetDistance(client.Player.X, client.Player.Y, ClientObj.X, ClientObj.Y) <= 3)
                                    {
                                        client.Player.AddFlag((ulong)PacketFlag.Flags.Praying);
                                        client.PrayLead = Client;
                                        client.Player.Action = Client.Player.Action;
                                        Client.Prayers.Add(client);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (client.PrayLead != null)
                {
                    if (Kernel.GetDistance(client.Player.X, client.Player.Y, client.PrayLead.Player.X, client.PrayLead.Player.Y) > 4)
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                        client.PrayLead.Prayers.Remove(client);
                        client.PrayLead = null;
                    }
                }
            }
        }
        private void AutoAttackCallback(GameState client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            if (client.Player.AttackPacket != null || client.Player.VortexAttackStamp != null)
            {
                try
                {
                    if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.ShurikenVortex))
                    {
                        if (client.Player.VortexPacket != null && client.Player.VortexPacket.ToArray() != null)
                        {
                            if (Now > client.Player.VortexAttackStamp.AddMilliseconds(1400))
                            {
                                client.Player.VortexAttackStamp = Now;
                                new Game.Attacking.Handle(client.Player.VortexPacket, client.Player, null);
                            }
                        }
                    }
                    else
                    {
                        client.Player.VortexPacket = null;
                        var AttackPacket = client.Player.AttackPacket;
                        if (AttackPacket != null && AttackPacket.ToArray() != null)
                        {
                            uint InteractType = AttackPacket.InteractType;
                            if (InteractType == MsgInteract.Magic || InteractType == MsgInteract.Melee || InteractType == MsgInteract.Ranged)
                            {
                                if (InteractType == MsgInteract.Magic)
                                {
                                    if (Now > client.Player.AttackStamp.AddMilliseconds(900))
                                    {
                                        if (AttackPacket.Damage != 12160 &&
                                            AttackPacket.Damage != 12170 &&
                                            AttackPacket.Damage != 12120 &&
                                            AttackPacket.Damage != 12130 &&
                                            AttackPacket.Damage != 12140 &&
                                            AttackPacket.Damage != 12320 &&
                                            AttackPacket.Damage != 12330 &&
                                            AttackPacket.Damage != 12340 &&
                                            AttackPacket.Damage != 12210)
                                            new Game.Attacking.Handle(AttackPacket, client.Player, null);
                                    }
                                }
                                else
                                {
                                    int decrease = -300;
                                    if (client.Player.OnCyclone())
                                        decrease = 700;
                                    if (client.Player.OnSuperman())
                                        decrease = 200;
                                    if (Now > client.Player.AttackStamp.AddMilliseconds((500 - client.Player.Agility - decrease) * (int)(InteractType == MsgInteract.Melee ? 1 : 1)))
                                    {
                                        new Game.Attacking.Handle(AttackPacket, client.Player, null);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Server.SaveException(e);
                    client.Player.AttackPacket = null;
                    client.Player.VortexPacket = null;
                }
            }
        }
        private void ServerFunctions(int time)
        {
            DateTime Now = DateTime.Now;
            #region CaptureTheFlag
            if (Now.DayOfWeek == DayOfWeek.Saturday)
            {
                if (Now.Hour == 21 && Now.Minute >= 36 && !MsgWarFlag.IsWar)
                {
                    MsgWarFlag.IsWar = true;
                    MsgWarFlag.StartTime = DateTime.Now;
                    MsgWarFlag.ClearHistory();
                    foreach (var Guilds in Kernel.Guilds.Values)
                    {
                        Guilds.CTFFlagScore = 0;
                        Guilds.CTFPoints = 0;
                        Guilds.CTFdonationCPs = 0;
                        Guilds.CTFdonationSilver = 0;
                        Guilds.CalculateCTFRank(true);
                        foreach (var Members in Guilds.Members.Values)
                        {
                            Members.Exploits = 0;
                            Members.ExploitsRank = 0;
                            Members.CTFCpsReward = 0;
                            Members.CTFSilverReward = 0;
                        }
                        Guilds.CalculateCTFRank(false);
                    }
                    Player.name = new object[] { "Capture the Flag has begun! Would you like to join Go to Twin City to signup at GulidController" };
                    Kernel.SendWorldMessage(new MsgTalk(string.Concat(Player.name), "ALLUSERS", "[Capture The Flag]", System.Drawing.Color.Red, 2500), Server.GamePool);
                    foreach (var client in Server.GamePool)
                    {
                        if (!client.InQualifier() && client.Map.BaseID != 6001 && client.Map.BaseID != 6000 && !client.Player.Dead)
                        {
                            MsgInviteTrans alert = new MsgInviteTrans
                            {
                                InviteID = 10535,
                                Countdown = 60,
                                Action = 1
                            };
                            client.Player.InviteID = 10535;
                            client.Send(alert.ToArray());
                        }
                    }
                }
            }
            if (MsgWarFlag.IsWar)
            {
                foreach (var State in Server.GamePool)
                {
                    if (State.Player.MapID == 2057)
                    {
                        MsgWarFlag.SortScoresJoining(State, out State.Guild);
                        MsgWarFlag.SendScores();
                    }
                }
                Server.Thread.CTF.SendUpdates();
                if (Now > MsgWarFlag.StartTime.AddHours(1))
                {
                    MsgWarFlag.IsWar = false;
                    MsgWarFlag.Close();
                }
            }
            if (CTF != null)
                CTF.SpawnFlags();
            #endregion
            #region FakeLoad
            foreach (Client.GameState client in Kernel.GamePool.Values)
            {
                if (client.Fake)
                {
                    if (!client.SignedUpForEPK)
                        ElitePKTournament.SignUp(client);
                    if (client.ElitePKMatch != null)
                    {
                        if (client.ElitePKMatch.OnGoing && client.ElitePKMatch.Inside)
                        {
                            if (Time32.Now > client.FakeQuit.AddSeconds(5))
                            {
                                client.FakeQuit = Time32.Now;
                                if (Kernel.Rate(1, 10))
                                {
                                    client.ElitePKMatch.End(client);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            var kvpArray = Kernel.GamePool.ToArray();
            foreach (var kvp in kvpArray)
                if (kvp.Value == null || kvp.Value.Player == null)
                    Kernel.GamePool.Remove(kvp.Key);
            Server.GamePool = Kernel.GamePool.Values.ToArray();
            new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration").Set("GuildID", Game.ConquerStructures.Society.Guild.GuildCounter.Now).Set("ItemUID", Server._NextItemID).Where("Server", InfoFile.ServerName).Execute();
            EntityVariableTable.Save(0, Server.Vars);
            if (Kernel.BlackSpoted.Values.Count > 0)
            {
                foreach (var spot in Kernel.BlackSpoted.Values)
                {
                    if (Time32.Now >= spot.BlackSpotStamp.AddSeconds(spot.BlackSpotStepSecs))
                    {
                        if (spot.Dead && spot.PlayerFlag == PlayerFlag.Player)
                        {
                            foreach (var h in Server.GamePool)
                            {
                                h.Send(Kernel.MsgDeadMark.ToArray(false, spot.UID));
                            }
                            Kernel.BlackSpoted.Remove(spot.UID); continue;
                        }
                        foreach (var h in Server.GamePool)
                        {
                            h.Send(Kernel.MsgDeadMark.ToArray(false, spot.UID));
                        }
                        spot.IsBlackSpotted = false;
                        Kernel.BlackSpoted.Remove(spot.UID);
                    }
                }
            }
            if (Now > Broadcast.LastBroadcast.AddMinutes(1))
            {
               
                if (Broadcast.Broadcasts.Count > 0)
                {
                    Broadcast.CurrentBroadcast = Broadcast.Broadcasts[0];
                    Broadcast.Broadcasts.Remove(Broadcast.CurrentBroadcast);
                    Broadcast.LastBroadcast = Now;
                    Kernel.SendWorldMessage(new MsgTalk(Broadcast.CurrentBroadcast.Message, "ALLUSERS", Broadcast.CurrentBroadcast.EntityName, System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage, Game.ConquerStructures.Broadcast.CurrentBroadcast.UnionTitle), Server.GamePool);
                }
                else Broadcast.CurrentBroadcast.EntityID = 1;
            }
            if (Now > Server.LastRandomReset.AddMinutes(30))
            {
                Server.LastRandomReset = Now;
                Kernel.Random = new FastRandom(Server.RandomSeed);
            }
            Server.Today = Now.DayOfWeek;
        }
        private void QualifierFunctions(int time)
        {
            Qualifier.EngagePlayers();
            Qualifier.CheckGroups();
            Qualifier.VerifyAwaitingPeople();
            Qualifier.Reset();
        }
        internal void SendServerMessaj(string p)
        {
            Kernel.SendWorldMessage(new MsgTalk(p, System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.TopLeft), Server.GamePool);
        }
        public bool Register(Player ThunderCloudd)
        {
            if (ThunderCloudd.Owner.TimerSubscriptions == null)
            {
                ThunderCloudd.Owner.TimerSyncRoot = new object();
                ThunderCloudd.Owner.TimerSubscriptions = new IDisposable[]
                {
                    ThunderCloud.Add(ThunderCloudd)
                };
                return true;
            }
            return false;
        }
        public void Unregister(Player Thundercloud)
        {
            if (Thundercloud.Owner == null || Thundercloud.Owner.TimerSubscriptions == null) return;
            lock (Thundercloud.Owner.TimerSyncRoot)
            {
                if (Thundercloud.Owner.TimerSubscriptions != null)
                {
                    foreach (var timer in Thundercloud.Owner.TimerSubscriptions)
                        timer.Dispose();
                    Thundercloud.Owner.TimerSubscriptions = null;
                }
            }
        }
        public TimerRule<Player> ThunderCloud;
        private void ThunderCloudTimer(Player ThunderCloud, int time)
        {
            if (ThunderCloud == null || !Kernel.Maps.ContainsKey(ThunderCloud.MapID)) return;
            if (Time32.Now >= ThunderCloud.ThunderCloudStamp.AddSeconds(1))
            {
                ThunderCloud.ThunderCloudStamp = Time32.Now;
                if (ThunderCloud.Hitpoints > 400)
                    ThunderCloud.Hitpoints -= 400;
                else
                {
                    Kernel.Maps[ThunderCloud.MapID].RemoveEntity(ThunderCloud);
                    MsgAction data = new MsgAction(true);
                    data.UID = ThunderCloud.UID;
                    data.ID = PacketMsgAction.Mode.RemoveEntity;
                    ThunderCloud.MonsterInfo.SendScreen(data);
                    ThunderCloud.MonsterInfo.SendScreen(data);
                    foreach (var client in Kernel.GamePool.Values)
                    {
                        if (Kernel.GetDistance(ThunderCloud.X, ThunderCloud.Y, client.Player.X, client.Player.Y) > 16) continue;
                        client.RemoveScreenSpawn(ThunderCloud, true);
                    }
                    Unregister(ThunderCloud);
                    return;
                }
            }
            if ((ThunderCloud.SpawnPacket[50] == 0 && Time32.Now >= ThunderCloud.MonsterInfo.LastMove.AddMilliseconds(750)) || ThunderCloud.SpawnPacket[50] == 128)
            {
                ThunderCloud.MonsterInfo.LastMove = Time32.Now;
                if (ThunderCloud.MonsterInfo.InSight == 0)
                {
                    if (Kernel.Maps.ContainsKey(ThunderCloud.MapID))
                    {
                        foreach (var one in Kernel.Maps[ThunderCloud.MapID].Entities.Values.Where(i => Kernel.GetDistance(ThunderCloud.X, ThunderCloud.Y, i.X, i.Y) <= ThunderCloud.MonsterInfo.AttackRange))
                        {
                            if (one == null || one.Dead || one.MonsterInfo.Guard || one.Companion) continue;
                            ThunderCloud.MonsterInfo.InSight = one.UID;
                            Player insight = null;
                            if (Kernel.Maps[ThunderCloud.MapID].Entities.ContainsKey(ThunderCloud.MonsterInfo.InSight))
                                insight = Kernel.Maps[ThunderCloud.MapID].Entities[ThunderCloud.MonsterInfo.InSight];
                            else if (Kernel.GamePool.ContainsKey(ThunderCloud.MonsterInfo.InSight))
                                insight = Kernel.GamePool[ThunderCloud.MonsterInfo.InSight].Player;
                            if (insight == null || insight.Dead || (insight.MonsterInfo != null && insight.MonsterInfo.Guard))
                            {
                                ThunderCloud.MonsterInfo.InSight = 0;
                                break;
                            }
                            new Game.Attacking.Handle(null, ThunderCloud, insight);
                            break;
                        }
                    }
                }
                else
                {
                    Player insight = null;
                    if (Kernel.Maps[ThunderCloud.MapID].Entities.ContainsKey(ThunderCloud.MonsterInfo.InSight))
                        insight = Kernel.Maps[ThunderCloud.MapID].Entities[ThunderCloud.MonsterInfo.InSight];
                    else if (Kernel.GamePool.ContainsKey(ThunderCloud.MonsterInfo.InSight))
                        insight = Kernel.GamePool[ThunderCloud.MonsterInfo.InSight].Player;
                    if (insight == null || insight.Dead || (insight.MonsterInfo != null && insight.MonsterInfo.Guard))
                    {
                        ThunderCloud.MonsterInfo.InSight = 0;
                        return;
                    }
                    new Game.Attacking.Handle(null, ThunderCloud, insight);
                }
            }

        }
        private void TeamQualifierFunctions(int time)
        {
            TeamQualifier.PickUpTeams();
            TeamQualifier.EngagePlayers();
            TeamQualifier.CheckGroups();
            TeamQualifier.VerifyAwaitingPeople();
            TeamQualifier.Reset();
        }
        #region Functions
        public static void Execute(Action<int> action, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe(new LazyDelegate(action, timeOut, priority));
        }
        public static void Execute<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            GenericThreadPool.Subscribe<T>(new LazyDelegate<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe(Action<int> action, int period = 1, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe(new TimerRule(action, period, priority));
        }
        public static IDisposable Subscribe<T>(Action<T, int> action, T param, int timeOut = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            return GenericThreadPool.Subscribe<T>(new TimerRule<T>(action, timeOut, priority), param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StandalonePool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StaticPool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param)
        {
            return GenericThreadPool.Subscribe<T>(rule, param);
        }
        #endregion
       
    }
}
