// ☺ Editada y Reparada Por Pezzi Tomas / Fixed and Work by Pezzi Tomas
using System;
using System.Linq;
using COServer.Client;
using COServer.Network;
using COServer.DB;
using COServer.Interfaces;
using System.Collections.Generic;
using COServer.Network.GamePackets;
using System.Collections.Concurrent;
using Core.Packet;
using Core.Enums;
using Core;
using static COServer.Database.SystemBannedAccount;

namespace COServer.Game
{
    public unsafe class Player : Writer, IBaseEntity, IMapObject
    {
        //Elite pK
        public void Teleport()
        {
            Owner.InArenaMatch = false;
          
            Teleport(1002, 301, 280);
        }
        //
        public bool InRealm = false;
        public bool BoxOpened;
        public DateTime BoxTime;
        public uint OwnerEliteGuildWar;
        public ulong AjustHitRate(SpellInformation spell, Player attacked = null)
        {
            ulong value = spell.Percent;
            return value;
        }
        #region WardRobe
        public Dictionary<uint, uint> Wings = new Dictionary<uint, uint>();
        public List<string> NowEquippedWing = new List<string>();
        public List<string> NowEquippedTitle = new List<string>();
        public Dictionary<uint, uint> WTitles = new Dictionary<uint, uint>();
        //public WardrobeTitles WTitles;
        public int EquippedTitle
        {
            get
            {
                return System.BitConverter.ToInt32(SpawnPacket, MsgPlayer.MyTitle);
            }
            set
            {
                Writer.WriteInt32(value, MsgPlayer.MyTitle, SpawnPacket);
                UpdateDatabase("TitleID", value);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Player) != null)
                            (player as Player).Owner.Send(SpawnPacket);
            }
        }
        public int UTitlePoints
        {
            get
            {
                return System.BitConverter.ToInt32(SpawnPacket, MsgPlayer.MyTitleScore);
            }
            set
            {
                Writer.Write(value, MsgPlayer.MyTitleScore, SpawnPacket);
                Update(89, (uint)value, false);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Player) != null)
                            (player as Player).Owner.Send(SpawnPacket);
            }
        }
        public int EquippedWing
        {
            get
            {
                return System.BitConverter.ToInt32(SpawnPacket, MsgPlayer.MyWing);
            }
            set
            {
                Writer.WriteInt32(value, MsgPlayer.MyWing, SpawnPacket);
                UpdateDatabase("WingID", value);
                foreach (var player in Owner.Screen.Objects.Where(p => p.MapObjType == MapObjectType.Player))
                    if (player != null)
                        if ((player as Player) != null)
                            (player as Player).Owner.Send(SpawnPacket);
            }
        }
        #endregion
        public bool Reflect;
        public Time32 CPSgold;
        public Time32 XDROP;
        public Time32 ChangeEquip;
        public int SelectedStage;
        public int SelectedAttribute;
        public bool taskReward;
        public uint WarScore;
        public List<Clone> MyClones = new List<Clone>();
        public ConcurrentDictionary<ushort, Game.Features.FloorSpell.ClientFloorSpells> FloorSpells = new ConcurrentDictionary<ushort, Game.Features.FloorSpell.ClientFloorSpells>();
        public uint LastClientTick, LastRecClientTick, LastReqClientTick;
        public uint GuildArenaCps = 0;
        // Inner Desativado // public COServer.MaTrix.Inner.InnerPower InnerPower;
        #region Windwalker
        byte _wwalker;
        public Time32 JusticeChant, HealingSnow, OmnipotenceStamp, GreenEffectStamp, ChaserStaminaStamp, RevengeTailStamp, StomperStaminaStamp, ChillingSnowStamp, FreezingPelterStamp;
        public byte ChillingSnow = 0, FreezingPelter = 0;
        public int xFreezing, xChilling;
        public int HealingRate;
        public ushort JusticeRateSkill = 0, GreenEffect = 0;
        public byte WindWalker
        {
            get { return _wwalker; }
            set
            {
                _wwalker = value;
                SpawnPacket[PlayerSpawnPacket.Windwalker] = value;
            }
        }
        public bool IsStomper2()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item2 != null)
                    if (weapons.Item2.ID / 1000 == 626)
                        if (this.Class >= 160 && this.Class <= 165)
                            if (WindWalker == (byte)COServer.WindWalker.CharacterType.Stomper)
                                return true;
            }
            return false;
        }
        public bool IsChaser()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item2 != null)
                    if (weapons.Item2.ID / 1000 == 626)
                        if (this.Class >= 160 && this.Class <= 165)
                            if (WindWalker == (byte)COServer.WindWalker.CharacterType.Chaser)
                                return true;
            }
            return false;
        }
        #endregion
        private Regions mRegion;
        public Regions MapRegion
        {
            get
            {
                return this.mRegion;
            }
            set
            {
                if (this.mRegion != value)
                {
                    string str = (this.mRegion != null) ? this.mRegion.Name : string.Empty;
                    this.mRegion = value;
                    if (this.MapRegion != null)
                    {

                        if (this.MapRegion.Name != str)
                        {
                            if (this.MapRegion.Name != string.Empty)
                            {
                                Owner.Send(new MsgTalk("You entered the region " + this.MapRegion.Name + " (Lineage: " + this.MapRegion.Lineage + ").", System.Drawing.Color.BurlyWood, (uint)PacketMsgTalk.MsgTalkType.TopLeft));

                            }
                            else
                            {
                                Owner.Send(new MsgTalk("You left the region " + str + "", System.Drawing.Color.BurlyWood, (uint)PacketMsgTalk.MsgTalkType.TopLeft));

                            }
                        }
                        if (ContainsFlag((ulong)PacketFlag.Flags.Ride))
                        {
                            var q = Owner.Equipment.TryGetItem((byte)12);
                            if (!Owner.Equipment.Free(12))
                                if (Owner.Map.ID == 1036 && q.Plus < this.MapRegion.Lineage)
                                    RemoveFlag((ulong)PacketFlag.Flags.Ride);

                        }
                    }
                }
            }
        }

        public uint MaxVigor
        {
            get
            {
                uint MaxVigor2 = 0;
                if (!Owner.Equipment.Free(12))
                {
                    MsgItemInfo dbi = Owner.Equipment.TryGetItem(12);
                    if (dbi.Plus == 1)
                        MaxVigor2 += 50;
                    if (dbi.Plus == 2)
                        MaxVigor2 += 120;
                    if (dbi.Plus == 3)
                        MaxVigor2 += 200;
                    if (dbi.Plus == 4)
                        MaxVigor2 += 350;
                    if (dbi.Plus == 5)
                        MaxVigor2 += 650;
                    if (dbi.Plus == 6)
                        MaxVigor2 += 1000;
                    if (dbi.Plus == 7)
                        MaxVigor2 += 1400;
                    if (dbi.Plus == 8)
                        MaxVigor2 += 2000;
                    if (dbi.Plus == 9)
                        MaxVigor2 += 2800;
                    if (dbi.Plus == 10)
                        MaxVigor2 += 3100;
                    if (dbi.Plus == 11)
                        MaxVigor2 += 3500;
                    if (dbi.Plus == 12)
                        MaxVigor2 += 4000;
                    MaxVigor2 += 30;
                    if (!Owner.Equipment.Free(MsgItemInfo.SteedCrop))
                    {
                        if (Owner.Equipment.Objects[18] != null)
                        {
                            if (Owner.Equipment.Objects[18].ID % 10 == 9)
                            {
                                MaxVigor2 += 1000;
                            }
                            else if (Owner.Equipment.Objects[18].ID % 10 == 8)
                            {
                                MaxVigor2 += 700;
                            }
                            else if (Owner.Equipment.Objects[18].ID % 10 == 7)
                            {
                                MaxVigor2 += 500;
                            }
                            else if (Owner.Equipment.Objects[18].ID % 10 == 6)
                            {
                                MaxVigor2 += 300;
                            }
                            else if (Owner.Equipment.Objects[18].ID % 10 == 5)
                            {
                                MaxVigor2 += 100;
                            }
                        }
                    }
                }
                return MaxVigor2;
            }
        }
        public Time32 LastTimeUseSlide = Time32.Now;
        public uint TitleScore
        {
            get
            {
                return SpawnPacket[PlayerSpawnPacket.MyTitleScore];
            }
            set
            {
                WriteUInt32(value, PlayerSpawnPacket.MyTitleScore, SpawnPacket);
            }
        }
        #region Poker
        public void Send(Byte[] Buffer)
        {
            Owner.Send(Buffer);
        }
        public uint PokerTable = 0;
        public MaTrix.Poker.COPokerTable MyPokerTable
        {
            get
            {
                if (MaTrix.Poker.PokerDataBase.PokerTables.ContainsKey(PokerTable))
                    return MaTrix.Poker.PokerDataBase.PokerTables[PokerTable];
                else return null;
            }
            set
            {
                PokerTable = value.UID;
            }
        }
        #endregion
        public int ToxinEraserLevel;
        public int StrikeLockLevel;
        public int LuckyStrike;
        public int CalmWind;
        public int TempPerfection;
        public int DrainingTouch;
        public int BloodSpawn;
        public int LightOfStamina;
        public Time32 ShieldBreak;
        public int BlockBreak;
        public ushort GuardDefense;
        public Time32 GuardDefenseStamp;
        public int ShiledBreak;
        public int KillingFlash;
        public int MirrorOfSin;
        public int DivineGuard;
        public int CoreStrike;
        public int InvisableArrow;
        public int FreeSoul;
        public int StraightLife;
        public int AbsoluteLuck;
        //uint _pres;
        //public uint Prestige
        //{
        //    get
        //    {
        //        return _pres;
        //    }
        //    set
        //    {
        //        _pres = value;
        //        if (FullyLoaded)
        //            UpdateDatabase("LastPrestige", value);
        //    }
        //}
        uint _perf;
        public uint PerfectionLevel
        {
            get
            {
                _perf = 0;
                foreach (var item in Owner.Equipment.GetAllItems)
                {
                    if (item == null) continue;
                    if (item.Position > 29) continue;
                 
                    if (item.PerfectionLevel > 54) { _perf += 54; continue; }
                    _perf += item.PerfectionLevel;
                   
                }
                return _perf;
            }
        }
        public void UpdateDatabase(string column, long value)
        {
            if (PlayerFlag == PlayerFlag.Player)
                if (FullyLoaded)
                    new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", UID).Execute();

        }
        public static int _NewTitle = 288,
             _TitleScore = 292,
             _WingTitle = 296;
        private WardRobe.WardRobeTitle _wingTitle;
        public WardRobe.WardRobeTitle WingTitle
        {
            get
            {
                return _wingTitle;
            }
            set
            {
                _wingTitle = value;
                WriteUInt32((uint)((_wingTitle.type.ID * 10000) + _wingTitle.type.SubID), PlayerSpawnPacket.MyWing, SpawnPacket);
                UpdateDatabase("WingID", (uint)((_wingTitle.type.ID * 10000) + _wingTitle.type.SubID));
            }
        }
        private WardRobe.WardRobeTitle _newTitle;
        public WardRobe.WardRobeTitle NewTitle
        {
            get
            {
                return _newTitle;
            }
            set
            {
                _newTitle = value;
                WriteUInt32((uint)((_newTitle.type.ID * 10000) + _newTitle.type.SubID), PlayerSpawnPacket.MyTitle, SpawnPacket);
                UpdateDatabase("TitleID", (uint)((_newTitle.type.ID * 10000) + _newTitle.type.SubID));
            }
        }
        private byte _vipDays;
        public byte VIPDays
        {
            get
            {
                return _vipDays;
            }
            set
            {
                _vipDays = value;
            }
        }
        private byte _vipLevelDays;
        public byte VIPLevelDays
        {
            get
            {
                return _vipLevelDays;
            }
            set
            {
                _vipLevelDays = value;
            }
        }  
        public bool EpicWarrior()
        {
            if (this.Owner.Weapons != null)
                if (this.Owner.Weapons.Item2 != null && this.Owner.Weapons.Item1 != null)
                    if (this.Owner.Weapons.Item2.ID / 1000 == 624 && this.Owner.Weapons.Item1.ID / 1000 == 624) return true;
                    else
                        return false;
                else
                    return false;
            else
                return false;
        }
        public byte XPCountTwist = 0;
        private Msg2ndPsw.Nextaction _NextAction = Msg2ndPsw.Nextaction.Nothing;
        private ushort _actionX, _actionY;
        public ushort actionX
        {
            get { return _actionX; }
            set { _actionX = value; }
        }
        public ushort actionY
        {
            get { return _actionY; }
            set { _actionY = value; }
        }
        public Msg2ndPsw.Nextaction NextAction
        {
            get { return _NextAction; }
            set { _NextAction = value; }
        }
        public List<string> BlackList = new List<string>();
        #region Variables
        public Dictionary<uint, string> Enemies = new Dictionary<uint, string>();
        public static int Leadrinmap, Memberrinmap;
        public float IntensifyPercent;
        public bool BlockShieldCheck = false;
        public Features.Flowers Flowers;
        public MsgItemInfo LotteryPrize;
        public uint LotteryItemID = 0, LotteryItemPlus, LotteryItemColor, LotteryItemSoc1, LastXLocation, LastYLocation;
        public bool UseItem = false;
        public uint LotteryItemSoc2;
        public byte LotteryJadeAdd;
        public int KillCount = 0, KillCount2 = 0;
        public int BlockShield, MagicDefender, DefensiveStance;
        public bool InSteedRace, Invisable, IsBlackSpotted, IsDefensiveStance = false, IsEagleEyeShooted = false;
        public DB.MonsterInformation MonsterInfo;
        public Time32 AutoHp;
        public Time32
        DeathStamp, BlockShieldStamp, ScurbyBombStamp, ScurbyBomb2Stamp,
        VortexAttackStamp, AttackStamp, StaminaStamp, FlashingNameStamp,
        CycloneStamp, SuperCycloneStamp, DragonCycloneStamp, SupermanStamp, AutoRevStamp,
        TwoFlod, CannonBarrageStamp, StigmaStamp, InvisibilityStamp, StarOfAccuracyStamp,
        MagicShieldStamp, DodgeStamp, EnlightmentStamp, BlackSpotStamp, BlackbeardsRageStamp,
        DefensiveStanceStamp, AccuracyStamp, ShieldStamp, FlyStamp, NoDrugsStamp, ToxicFogStamp,
        FatalStrikeStamp, DoubleExpStamp, ProtectionStamp, BladeTempest, MagicDefenderStamp, FlagStamp,
        ShurikenVortexStamp, IntensifyStamp, TransformationStamp, CounterKillStamp, PKPointDecreaseStamp,
        LastPopUPCheck, HeavenBlessingStamp, DragonFlowStamp, DragonFuryStamp, DragonSwingStamp, OblivionStamp,
        BallonStamp, ShackleStamp, AzureStamp, StunStamp, WhilrwindKick, GuildRequest, Confuse, LastTeamLeaderLocationSent = Time32.Now, BladeFlurryStamp, EagleEyeStamp, Cursed,
        ShockStamp, FreezeStamp, spiritFocusStamp, CheckGuardStamp, MortalWoundStamp, ChainboltStamp, SpellStamp, LastExpSave, FrozenStamp, FlagStampCursed, FlameStamp, DiceKingTime, DoTransfer, BackFireStamp, ManiacDanceStamp, WaveOfBloodStamp;
        public bool
        MagicDefenderOwner = false, InteractionInProgress = false, IsDropped = false,
        HasMagicDefender = false, InteractionSet = false, Tournament_Signed = false;
        public byte FreezeTime;
        public int Fright;
        public uint InteractionType = 0;
        public uint InteractionWith = 0;
        public ushort InteractionX = 0;
        public ushort InteractionY = 0;
        public int CurrentTreasureBoxes = 0;
        public ConcurrentDictionary<MsgTitle.Titles, DateTime> Titles;
        public ConcurrentDictionary<int, DateTime> Halos;
        public bool IsWarTop(ulong Title)
        {
            return Title >= 11 && Title <= 26;
        }
        public void AddTopStatus(UInt64 Title, byte flagtype, DateTime EndsOn, Boolean Db = true)
        {
            Boolean HasFlag = false;
            if (IsWarTop(Title))
            {
                HasFlag = Titles.ContainsKey((MsgTitle.Titles)Title);
                Titles.TryAdd((MsgTitle.Titles)Title, EndsOn);
            }
            else
            {
                switch (flagtype)
                {
                    case 1:
                        HasFlag = ContainsFlag(Title);
                        AddFlag(Title);
                        break;
                    case 2:
                        HasFlag = ContainsFlag2(Title);
                        AddFlag2(Title);
                        break;
                    case 3:
                        HasFlag = ContainsFlag3(Title);
                        AddFlag3(Title);
                        break;
                }
            }
            if (Db)
            {
                if (HasFlag)
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
                    cmd.Update("status").Set("time", Kernel.ToDateTimeInt(EndsOn))
                        .Where("status", Title).And("flagtype", flagtype).And("entityid", (UInt32)UID);
                    cmd.Execute();
                }
                else
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.INSERT);
                    cmd.Insert("status").Insert("entityid", (UInt32)UID).Insert("status", Title).Insert("flagtype", flagtype).Insert("time", Kernel.ToDateTimeInt(EndsOn));
                    cmd.Execute();
                }
            }
        }
        public void RemoveTopStatus(UInt64 Title, byte flagtype = 0)
        {
            ulong baseFlag = Title;
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.DELETE);
            cmd.Delete("status", "entityid", UID).And("status", baseFlag).And("flagtype", flagtype).Execute();


            switch (flagtype)
            {
                case 0:
                    {
                        var title = (MsgTitle.Titles)baseFlag;
                        if (Titles.ContainsKey(title))
                        {
                            Titles.Remove(title);
                            if (MyTitle == title)
                                MyTitle = Network.GamePackets.MsgTitle.Titles.None;

                            Owner.SendScreenSpawn(this, true);
                        }
                        break;
                    }
                case 1:
                    RemoveFlag(baseFlag);
                    break;
                case 2:
                    RemoveFlag2(baseFlag);
                    break;
                case 3:
                    RemoveFlag3(baseFlag);
                    break;
            }
        }
        public void LoadTopStatus()
        {
            using (MySqlCommand Command = new MySqlCommand(MySqlCommandType.SELECT))
            {
                Command.Select("status").Where("entityid", UID).Execute();
                using (MySqlReader Reader = new MySqlReader(Command))
                {
                    while (Reader.Read())
                    {
                        UInt64 Title = Reader.ReadUInt64("status");
                        byte flagtype = Reader.ReadByte("flagtype");
                        DateTime Time = Kernel.FromDateTimeInt(Reader.ReadUInt64("time"));
                        if (DateTime.Now > Time)
                            RemoveTopStatus(Title, flagtype);
                        else
                        {
                            AddTopStatus(Title, flagtype, Time, false);
                        }
                    }
                }
            }
        }
        public UInt64 TopStatusToInt(UInt64 top)
        {
            switch (top)
            {
                case (ulong)PacketFlag.Flags.TopWaterTaoist: return 1;
                case (ulong)PacketFlag.Flags.TopWarrior: return 2;
                case (ulong)PacketFlag.Flags.TopTrojan: return 3;
                case (ulong)PacketFlag.Flags.TopArcher: return 4;
                case (ulong)PacketFlag.Flags.TopNinja: return 5;
                case (ulong)PacketFlag.Flags.TopFireTaoist: return 6;
                case (ulong)PacketFlag.Flags.TopMonk: return 7;
                case (ulong)PacketFlag.Flags.TopSpouse: return 8;
                case (ulong)PacketFlag.Flags.TopGuildLeader: return 9;
                case (ulong)PacketFlag.Flags.TopDeputyLeader: return 10;
                case (ulong)PacketFlag.Flags.MonthlyPKChampion: return 20;
                case (ulong)PacketFlag.Flags.WeeklyPKChampion: return 21;
                case (ulong)PacketFlag.Flags.TopPirate: return 22;
                case (ulong)PacketFlag.Flags.TopPirate2: return 23;
                case (ulong)PacketFlag.Flags.Top2Archer: return 24;
                case (ulong)PacketFlag.Flags.Top2Fire: return 25;
                case (ulong)PacketFlag.Flags.Top2Monk: return 26;
                case (ulong)PacketFlag.Flags.Top2Ninja: return 27;
                case (ulong)PacketFlag.Flags.Top2Trojan: return 28;
                case (ulong)PacketFlag.Flags.Top2Warrior: return 29;
                case (ulong)PacketFlag.Flags.Top2Water: return 30;
                case (ulong)PacketFlag.Flags.ConuqerSuperBlue: return 31;
                case (ulong)PacketFlag.Flags.ConuqerSuperYellow: return 32;
                case (ulong)PacketFlag.Flags.ConuqerSuperUnderBlue: return 33;
                case (ulong)PacketFlag.Flags.DragonWarriorTop: return 34;
            }
            return top;
        }
        public UInt64 IntToTopStatus(UInt64 top)
        {
            switch (top)
            {
                case 1: return (ulong)PacketFlag.Flags.TopWaterTaoist;
                case 2: return (ulong)PacketFlag.Flags.TopWarrior;
                case 3: return (ulong)PacketFlag.Flags.TopTrojan;
                case 4: return (ulong)PacketFlag.Flags.TopArcher;
                case 5: return (ulong)PacketFlag.Flags.TopNinja;
                case 6: return (ulong)PacketFlag.Flags.TopFireTaoist;
                case 7: return (ulong)PacketFlag.Flags.TopMonk;
                case 8: return (ulong)PacketFlag.Flags.TopSpouse;
                case 9: return (ulong)PacketFlag.Flags.TopGuildLeader;
                case 10: return (ulong)PacketFlag.Flags.TopDeputyLeader;
                case 20: return (ulong)PacketFlag.Flags.MonthlyPKChampion;
                case 21: return (ulong)PacketFlag.Flags.WeeklyPKChampion;
                case 22: return (ulong)PacketFlag.Flags.TopPirate;
                case 23: return (ulong)PacketFlag.Flags.TopPirate2;
                case 24: return (ulong)PacketFlag.Flags.Top2Archer;
                case 25: return (ulong)PacketFlag.Flags.Top2Fire;
                case 26: return (ulong)PacketFlag.Flags.Top2Monk;
                case 27: return (ulong)PacketFlag.Flags.Top2Ninja;
                case 28: return (ulong)PacketFlag.Flags.Top2Trojan;
                case 29: return (ulong)PacketFlag.Flags.Top2Warrior;
                case 30: return (ulong)PacketFlag.Flags.Top2Water;
                case 31: return (ulong)PacketFlag.Flags.ConuqerSuperBlue;
                case 32: return (ulong)PacketFlag.Flags.ConuqerSuperYellow;
                case 33: return (ulong)PacketFlag.Flags.ConuqerSuperUnderBlue;
                case 34: return (ulong)PacketFlag.Flags.DragonWarriorTop;
            }
            return top;
        }
        public UInt32 ActivePOPUP;
        public MsgTitle.Titles MyTitle
        {
            get { return (MsgTitle.Titles)SpawnPacket[PlayerSpawnPacket.TitleActivated]; }
            set
            {
                SpawnPacket[PlayerSpawnPacket.TitleActivated] = (Byte)value;
                if (FullyLoaded)
                {
                    MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
                    cmd.Update("entities").Set("My_Title", (Byte)value).Where("uid", UID).Execute();
                }
            }
        }
        public Updating.Offset1 UpdateOffset1 = Updating.Offset1.None;
        public Updating.Offset2 UpdateOffset2 = Updating.Offset2.None;
        public Updating.Offset3 UpdateOffset3 = Updating.Offset3.None;
        public Updating.Offset4 UpdateOffset4 = Updating.Offset4.None;
        public Updating.Offset5 UpdateOffset5 = Updating.Offset5.None;
        public Updating.Offset6 UpdateOffset6 = Updating.Offset6.None;
        public Updating.Offset7 UpdateOffset7 = Updating.Offset7.None;
        public uint FlowerAmounts = 0;
        public int BlackSpotStepSecs;
        public ushort Detoxication;
        public int Immunity;
        public int FatigueSecs;
        public int CriticalStrike;
        public int SkillCStrike;
        public int Counteraction;
        public int Breaktrough;
        public ushort Intensification;
        public int Block;
        public ushort FinalMagicDmgPlus;
        public ushort FinalMagicDmgReduct;
        public ushort FinalDmgPlus;
        public ushort FinalDmgReduct;
        public int Penetration;
        public int MetalResistance;
        public int WoodResistance;
        public int WaterResistance;
        public int FireResistance;
        public int EarthResistance;
        public byte _SubPro, _SubProLevel;
        public SubProStages SubProStages = new SubProStages();
        public bool Stunned = false, Confused = false;
        public bool Companion;
        public bool CauseOfDeathIsMagic = false;
        public bool OnIntensify;
        private uint flower_R;
        public uint AddFlower
        {
            get { return flower_R; }
            set
            {
                flower_R = value;
            }
        }
        public short KOSpellTime
        {
            get
            {
                if (KOSpell == 1110)
                {
                    if (ContainsFlag((ulong)PacketFlag.Flags.Cyclone))
                    {
                        return CycloneTime;
                    }
                }
                else if (KOSpell == 1025)
                {
                    if (ContainsFlag((ulong)PacketFlag.Flags.Superman))
                    {
                        return SupermanTime;
                    }
                }
                return 0;
            }
            set
            {
                if (KOSpell == 1110)
                {
                    if (ContainsFlag((ulong)PacketFlag.Flags.Cyclone))
                    {
                        int Seconds = CycloneStamp.AddSeconds(value).AllSeconds() - Time32.Now.AllSeconds();
                        if (Seconds >= 20)
                        {
                            CycloneTime = 20;
                            CycloneStamp = Time32.Now;
                        }
                        else
                        {
                            CycloneTime = (short)Seconds;
                            CycloneStamp = Time32.Now;
                        }
                    }
                }
                if (KOSpell == 1025)
                {
                    if (ContainsFlag((ulong)PacketFlag.Flags.Superman))
                    {
                        int Seconds = SupermanStamp.AddSeconds(value).AllSeconds() - Time32.Now.AllSeconds();
                        if (Seconds >= 20)
                        {
                            SupermanTime = 20;
                            SupermanStamp = Time32.Now;
                        }
                        else
                        {
                            SupermanTime = (short)Seconds;
                            SupermanStamp = Time32.Now;
                        }
                    }
                }
            }
        }
        public short CycloneTime = 0, ManiacDanceTime = 0, SupermanTime = 0, BladeFlurryTime = 0, SuperCycloneTime = 0, DragonCycloneTime = 0, NoDrugsTime = 0, FatalStrikeTime = 0, ShurikenVortexTime = 0, OblivionTime = 0, ShackleTime = 0, AzureTime, BackFireTime = 0;
        public ushort KOSpell = 0;
        public int AzureDamage = 0;
        private ushort _enlightenPoints;
        private byte _receivedEnlighenPoints;
        private ushort _enlightmenttime;
        public float ToxicFogPercent, StigmaIncrease, MagicShieldIncrease, DodgeIncrease, ShieldIncrease;
        public byte ToxicFogLeft, ScurbyBomb, FlashingNameTime, FlyTime, StigmaTime, InvisibilityTime, StarOfAccuracyTime, MagicShieldTime, DodgeTime, AccuracyTime, ShieldTime, MagicDefenderSecs;
        public ushort KOCount = 0;
        public bool CounterKillSwitch = false;
        public MsgInteract AttackPacket;
        public MsgInteract VortexPacket;
        public byte[] SpawnPacket;
        private string _Name, _Spouse;
        private ushort _MDefence, _MDefencePercent;
        public ushort BaseDefence;
        public ushort Dexterity;
        private GameState _Owner;
        public uint ItemHP = 0;
        public double ItemBless = 1.0;
        public ushort ItemMP = 0, PhysicalDamageDecrease = 0, PhysicalDamageIncrease = 0, MagicDamageDecrease = 0, MagicDamageIncrease = 0, AttackRange = 1, Vigor = 0, ExtraVigor = 0;
        public int[] Gems = new int[GemTypes.Last];
        private uint _MinAttack, _MaxAttack, _MagicAttack;
        public uint BaseMinAttack, BaseMaxAttack, BaseMagicAttack, BaseMagicDefence;
        private uint _TransMinAttack, _TransMaxAttack, _TransDodge, _TransPhysicalDefence, _TransMagicDefence;
        public bool Killed = false;
        public bool Transformed
        {
            get
            {
                return TransformationID != 98 && TransformationID != 99 && TransformationID != 0;
            }
        }
        public uint TransformationAttackRange = 0;
        public int TransformationTime = 0;
        public uint TransformationMaxHP = 0;
        private byte _Dodge;
        private PKMode _PKMode;
        private PlayerFlag _PlayerFlag;
        private MapObjectType _MapObjectType;
        public Enums.Mode Mode;
        private ulong _experience;
        public Time32 CrackingSwipeStamp;
        public Time32 HerculesStamp;
        public Time32 FatalCrossStamp;
        public Time32 ResidentPalmStamp;
        public Time32 BloodyScytheStamp;
        public Time32 BladeTempestStamp;
        public bool BodyGuard;
        public bool WrathoftheEmperor { get; set; }
        public DateTime WrathoftheEmperorStamp;
        private uint _heavenblessing, _Cursed, _uid, _hitpoints, _maxhitpoints, _conquerpoints, _status4, _FirstCreditPoints;
        private ushort _doubleexp, _doubleexp2, _body, _transformationid, _face, _strength, _agility, _spirit, _vitality, _atributes, _mana, _maxmana, _hairstyle, _mapid, _previousmapid, _x, _y, _pkpoints;
        private byte _stamina, _class, _reborn, _level;
        private byte cls, secls;    
        public byte FirstRebornLevel, SecondRebornLevel;
        public bool FullyLoaded = false, SendUpdates = false, HandleTiming = false;
        private MsgUpdate update;
        #endregion
        #region Acessors
        private byte _TalentStaus;
        public byte TalentStaus
        {
            get
            {
                SpawnPacket[PlayerSpawnPacket.KongfuTalen] = _TalentStaus;
                return _TalentStaus;
            }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    _TalentStaus = value;
                    SpawnPacket[PlayerSpawnPacket.KongfuTalen] = value;
                    SpawnPacket[PlayerSpawnPacket.KongfuActive] = 1;
                }
                if (PlayerFlag == PlayerFlag.Monster)
                {
                    _TalentStaus = value;
                    SpawnPacket[PlayerSpawnPacket.KongfuTalen] = value;
                    SpawnPacket[PlayerSpawnPacket.KongfuActive] = 1;
                }
            }
        }
        public bool KongfuActive
        {
            get { return (SpawnPacket[PlayerSpawnPacket.KongfuActive] == 1); }
            set { WriteByte(value ? ((byte)1) : ((byte)0), PlayerSpawnPacket.KongfuActive, SpawnPacket); }
        }
        public byte ServerID
        {
            get { return SpawnPacket[PlayerSpawnPacket.ServerID]; }
            set { SpawnPacket[PlayerSpawnPacket.ServerID] = value; }
        }
        public int BattlePower
        {
            get
            {
                return BattlePowerCalc(this);
            }
        }
        public int NMBattlePower
        {
            get
            {
                return (int)(BattlePowerCalc(this) - MentorBattlePower);
            }
        }
        public uint BattlePowerFrom(Player mentor)
        {
            if (mentor.NMBattlePower < NMBattlePower) return 0;
            uint bp = (uint)((mentor.NMBattlePower - NMBattlePower) / 3.3F);
            if (Level >= 125) bp = (uint)((bp * (135 - Level)) / 10);
            if (bp < 0) bp = 0;
            return bp;
        }
        public uint _LastLogin;
        public uint LastLogin
        {
            get { return _LastLogin; }
            set
            {
                _LastLogin = value;
                if (Owner != null)
                {
                    if (Owner.AsMember != null)
                    {
                        Owner.AsMember.LastLogin = value;
                    }
                }
            }
        }
        public bool WearsGoldPrize = false;
        public string LowerName;
        public uint OwnerUID
        {
            get
            {
                return Core.BitConverter.ToUInt32(SpawnPacket, PlayerSpawnPacket.OwnerUID);
            }
            set
            {
                Writer.Write(value, PlayerSpawnPacket.OwnerUID, SpawnPacket);
            }
        }
        private string _unionname = "";
        public string UnionName
        {
            get { return _unionname; }
            set
            {
                _unionname = value;
                WriteStringList(new List<string>() { _Name, "", clan, "", "", "", _unionname }, PlayerSpawnPacket.PlayerName, SpawnPacket);
                Owner.SendScreen(SpawnPacket, false);
            }
        }
        //public string Name
        //{
        //    get
        //    {
        //        return _Name;
        //    }
        //    set
        //    {
        //        _Name = value;
        //        LowerName = value.ToLower();
        //        if (PlayerFlag == PlayerFlag.Player)
        //        {
        //            if (ClanName != "")
        //            {
        //                SpawnPacket = new byte[8 + PlayerSpawnPacket.PlayerName + _Name.Length + ClanName.Length + 10];
        //                WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
        //                WriteUInt16(PlayerSpawnPacket.PlayerPacket, 2, SpawnPacket);
        //                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
        //                WriteStringList(new List<string>() { _Name, "", ClanName, "", "", "", "" }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //            }
        //            else
        //            {
        //                SpawnPacket = new byte[8 + PlayerSpawnPacket.PlayerName + _Name.Length + 36];
        //                WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
        //                WriteUInt16(PlayerSpawnPacket.PlayerPacket, 2, SpawnPacket);
        //                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);

        //                if (Owner == null)
        //                    WriteStringList(new List<string>() { _Name, "", clan, "", "", "", "" }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //                else WriteStringList(new List<string>() { _Name, "", clan, "", "", "", UnionName }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //            }
        //        }
        //        else
        //        {
        //            if (ClanName != "")
        //            {
        //                SpawnPacket = new byte[8 + PlayerSpawnPacket.PlayerName + _Name.Length + ClanName.Length + 30];
        //                WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
        //                WriteUInt16(PlayerSpawnPacket.PlayerPacket, 2, SpawnPacket);
        //                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
        //                WriteStringList(new List<string>() { _Name, "", "", ClanName }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //            }
        //            else
        //            {
        //                SpawnPacket = new byte[8 + PlayerSpawnPacket.PlayerName + _Name.Length + 30];
        //                WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
        //                WriteUInt16(PlayerSpawnPacket.PlayerPacket, 2, SpawnPacket);
        //                WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
        //                WriteStringList(new List<string>() { _Name, "", "", "" }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //            }

        //        }
        //    }
        //}
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                LowerName = value.ToLower();
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (ClanName != "")
                    {
                        SpawnPacket = new byte[8 + MsgPlayer.NameClan + _Name.Length + ClanName.Length + 10]; // 10
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(MsgPlayer.PlayerPacket, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", ClanName, "", "", "", "" }, MsgPlayer.NameClan, SpawnPacket);
                    }
                    else
                    {
                        SpawnPacket = new byte[8 + MsgPlayer.NameClan + _Name.Length + 36];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(MsgPlayer.PlayerPacket, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);

                        if (Owner == null)
                            WriteStringList(new List<string>() { _Name, "", clan, "", "", "", "" }, MsgPlayer.NameClan, SpawnPacket);
                        else WriteStringList(new List<string>() { _Name, "", clan, "", "", "", UnionName }, MsgPlayer.NameClan, SpawnPacket);
                    }
                }
                else
                {
                    if (ClanName != "")
                    {
                        SpawnPacket = new byte[8 + MsgPlayer.NameClan + _Name.Length + ClanName.Length + 30];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(MsgPlayer.PlayerPacket, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", ClanName }, MsgPlayer.NameClan, SpawnPacket);
                    }
                    else
                    {
                        SpawnPacket = new byte[8 + MsgPlayer.NameClan + _Name.Length + 30];
                        WriteUInt16((ushort)(SpawnPacket.Length - 8), 0, SpawnPacket);
                        WriteUInt16(MsgPlayer.PlayerPacket, 2, SpawnPacket);
                        WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, SpawnPacket);
                        WriteStringList(new List<string>() { _Name, "", "", "" }, MsgPlayer.NameClan, SpawnPacket);
                    }

                }
            }
        }
        public string Spouse
        {
            get { return _Spouse; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update(MsgName.Mode.Spouse, value, false);
                }
                _Spouse = value;
            }
        }
        private ulong _money;
        public ulong Money
        {
            get { return _money; }
            set
            {
                _money = value;
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Money, value, false);
            }
        }
        private byte _vipLevel;
        public byte VIPLevel
        {
            get { return _vipLevel; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (value > 0)
                    {
                        Update((byte)PacketFlag.DataType.VIPLevel, value, false);
                    }
                    if (FullyLoaded)
                        UpdateDB("VIPLevel", value);
                }
                _vipLevel = value;
            }
        }
        public uint ConquerPoints
        {
            get { return _conquerpoints; }
            set
            {
                value = (uint)Math.Max(0, (int)value);
                if (value >= int.MaxValue)
                {
                    value = int.MaxValue;
                    Owner.MessageBox("Max Allowed ConquerPoints :" + int.MaxValue);
                }
                _conquerpoints = value;
                EntityTable.UpdateCps(Owner);
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.ConquerPoints, (uint)value, false);
                }
            }
        }
        public uint FirstCreditPoints
        {
            get { return _FirstCreditPoints; }
            set
            {
                if (value <= 0)
                    value = 0;
                _FirstCreditPoints = value;
                DB.EntityTable.UpdateFirstCreditPoints(this.Owner);
            }
        }
        public ushort Body
        {
            get
            {
                return _body;
            }
            set
            {
                WriteUInt32((uint)(TransformationID * 10000000 + Face * 10000 + value), PlayerSpawnPacket.Mesh, SpawnPacket);
                _body = value;
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (Owner != null && Owner.ArenaStatistic != null)
                    {
                        Owner.ArenaStatistic.Model = (uint)(Face * 10000 + value);
                        Update((byte)PacketFlag.DataType.Mesh, Mesh, true);
                    }
                }
            }
        }
        public ushort DoubleExperienceTime
        {
            get { return _doubleexp; }
            set
            {
                ushort oldVal = DoubleExperienceTime;
                _doubleexp = value;
                if (FullyLoaded)
                {
                    if (oldVal <= _doubleexp)
                    {
                        if (PlayerFlag == PlayerFlag.Player)
                        {
                            if (Owner != null)
                            {
                                MsgUpdate upgrade = new MsgUpdate(true);
                                upgrade.UID = UID;
                                upgrade.Append((byte)PacketFlag.DataType.DoubleExpTimer, 0, DoubleExperienceTime, 0, 200);
                                Owner.Send(upgrade.ToArray());
                                if (FullyLoaded)
                                {
                                    UpdateDB("DoubleExpTime", value);
                                }
                            }
                        }
                    }
                }
            }
        }
        public ushort ExpProtectionTime
        {
            get { return _doubleexp2; }
            set
            {
                ushort oldVal = ExpProtectionTime;
                _doubleexp2 = value;
                if (FullyLoaded)
                {
                    if (oldVal <= _doubleexp2)
                    {
                        if (PlayerFlag == PlayerFlag.Player)
                        {
                            if (Owner != null)
                            {
                                MsgUpdate upgrade = new MsgUpdate(true);
                                upgrade.UID = UID;
                                upgrade.Append((byte)PacketFlag.DataType.ExpProtection, 0, ExpProtectionTime, 1, 0);
                                Owner.Send(upgrade.ToArray());
                                if (FullyLoaded)
                                {
                                    UpdateDB("ProtectionTime", value);
                                }
                            }
                        }
                    }
                }
            }
        }
        public uint HeavenBlessing
        {
            get { return _heavenblessing; }
            set
            {
                uint oldVal = HeavenBlessing;
                _heavenblessing = value;
                if (FullyLoaded)
                    if (value > 0)
                        if (!ContainsFlag((ulong)PacketFlag.Flags.HeavenBlessing) || oldVal <= _heavenblessing)
                        {
                            AddFlag((ulong)PacketFlag.Flags.HeavenBlessing);
                            Update((byte)PacketFlag.DataType.HeavensBlessing, HeavenBlessing, false);
                            Update(MsgName.Mode.Effect, "bless", true);
                            UpdateDB("HeavenBlessingTime", value);
                        }
            }
        }
        public uint CursedTime
        {
            get { return _Cursed; }
            set
            {
                uint oldVal = CursedTime;
                _Cursed = value;
                if (FullyLoaded)
                    if (value > 0)
                        if (!ContainsFlag((ulong)PacketFlag.Flags.Cursed) || oldVal <= _Cursed)
                        {
                            Cursed = Time32.Now;
                            AddFlag((ulong)PacketFlag.Flags.Cursed);
                            Update((byte)PacketFlag.DataType.CursedTimer, CursedTime, false);
                            UpdateDB("CursedTime", value);
                        }
            }
        }
        public byte Stamina
        {
            get { return _stamina; }
            set
            {
                _stamina = value;
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Stamina, value, false);
            }
        }
        public ushort TransformationID
        {
            get { return _transformationid; }
            set
            {
                _transformationid = value;
                WriteUInt32((uint)(value * 10000000 + Face * 10000 + Body), PlayerSpawnPacket.Mesh, SpawnPacket);
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Mesh, Mesh, true);
            }
        }
        public ushort Face
        {
            get { return _face; }
            set
            {
                WriteUInt32((uint)(TransformationID * 10000000 + value * 10000 + Body), PlayerSpawnPacket.Mesh, SpawnPacket);
                _face = value;
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.ArenaStatistic != null)
                            Owner.ArenaStatistic.Model = (uint)(value * 10000 + Body);
                        Update((byte)PacketFlag.DataType.Mesh, Mesh, true);
                    }
                }
            }
        }
        public uint Mesh
        {
            get
            {
                return System.BitConverter.ToUInt32(SpawnPacket, PlayerSpawnPacket.Mesh);
            }
            set
            {
                Writer.Write(value, PlayerSpawnPacket.Mesh, SpawnPacket);
            }

        }
        public byte Class
        {
            get { return _class; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.ArenaStatistic != null)
                            Owner.ArenaStatistic.Class = value;
                        Update((byte)PacketFlag.DataType.Class, value, false);
                    }
                }
                _class = value;
                SpawnPacket[PlayerSpawnPacket.Class] = value;
            }
        }
        public byte Reborn
        {
            get
            {
                return _reborn;
            }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.Reborn, value, true);
                }
                _reborn = value;
                SpawnPacket[PlayerSpawnPacket.Reborn] = value;
            }
        }
        public byte FirstRebornClass
        {
            get
            {
                return cls;
            }
            set
            {
                cls = value;
                Update((byte)PacketFlag.DataType.FirsRebornClass, value, false);
                SpawnPacket[PlayerSpawnPacket.FirstRebornClass] = value;
            }
        }
        public byte SecondRebornClass
        {
            get
            {
                return secls;
            }
            set
            {
                secls = value;
                Update((byte)PacketFlag.DataType.SecondRebornClass, value, false);
                SpawnPacket[PlayerSpawnPacket.SecondRebornClass] = value;
            }
        }
        public uint EquipmentColor
        {
            get { return Core.BitConverter.ToUInt32(SpawnPacket, PlayerSpawnPacket.EquipmentColor); }
            set { WriteUInt32(value, PlayerSpawnPacket.EquipmentColor, SpawnPacket); }
        }
        public byte Level
        {
            get
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    SpawnPacket[MsgPlayer.Level] = _level;
                    return _level;
                }
                else
                {
                    SpawnPacket[MsgPlayer.MonsterLevel] = _level;
                    return _level;
                }
            }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    MsgAction update = new MsgAction(true);
                    update.UID = UID;
                    update.ID = PacketMsgAction.Mode.Leveled;
                    update.Data28_Uint = value;
                    if (Owner != null)
                    {
                        (Owner as GameState).SendScreen(update, true);
                        if (Owner.ArenaStatistic != null)
                        {
                            Owner.ArenaStatistic.Level = value;
                            Owner.ArenaStatistic.ArenaPoints = 1000;
                        }
                    }
                    if (Owner != null)
                    {
                        if (Owner.AsMember != null)
                        {
                            Owner.AsMember.Level = value;
                        }
                    }
                    SpawnPacket[MsgPlayer.Level] = value;
                    Update((byte)PacketFlag.DataType.Level, value, true);
                    UpdateDB("Level", value);
                }
                else
                {
                    SpawnPacket[MsgPlayer.MonsterLevel] = value;
                }
                _level = value;
            }
        }
        private uint mentorBP;
        public uint MentorBattlePower
        {
            get { return mentorBP; }
            set
            {
                if ((int)value < 0)
                    value = 0;
                if (Owner != null)
                {
                    if (Owner.Mentor != null)
                    {
                        if (Owner.Mentor.IsOnline)
                        {
                            ExtraBattlePower -= mentorBP;
                            mentorBP = value;
                            ExtraBattlePower += value;
                            int val = Owner.Mentor.Client.Player.BattlePower;
                            Update((byte)PacketFlag.DataType.ExtraBattlePower, (uint)Math.Min(val, value), (uint)val);
                        }
                        else
                        {
                            ExtraBattlePower -= mentorBP;
                            mentorBP = 0;
                            Update((byte)PacketFlag.DataType.ExtraBattlePower, 0, 0);
                        }
                    }
                }
                else
                {
                    ExtraBattlePower -= mentorBP;
                    mentorBP = 0;
                    Update((byte)PacketFlag.DataType.ExtraBattlePower, 0, 0);
                }
            }
        }
        public uint _ExtraBattlePower;
        public uint ExtraBattlePower
        {
            get { return _ExtraBattlePower; }
            set
            {
                if (value > 200)
                    value = 0;
                if (ExtraBattlePower > 1000)
                    WriteUInt32(0, MsgPlayer.ExtraBattlePower, SpawnPacket);
                if (ExtraBattlePower > 0 && value == 0 || value > 0)
                {
                    if (value == 0 && ExtraBattlePower == 0) return;
                    WriteUInt32(value, MsgPlayer.ExtraBattlePower, SpawnPacket);
                    Update((byte)PacketFlag.DataType.ExtraBattlePower, value, false);
                    _ExtraBattlePower = value;
                }
            }
        }
        public bool awayTeleported = false;
        public void SetAway(bool isAway)
        {
            if (!isAway && Away == 1)
            {
                Away = 0;
                Owner.SendScreen(SpawnPacket, false);
                if (awayTeleported)
                {
                    awayTeleported = false;
                    Teleport(PreviousMapID, PrevX, PrevY);
                }
            }
            else if (isAway && Away == 0)
            {
                if (!Constants.PKFreeMaps.Contains(MapID))
                {
                    if (!(MapID == 1036 || MapID == 1002 || Owner.Mining))
                    {
                        PreviousMapID = MapID;
                        PrevX = X;
                        PrevY = Y;
                        Teleport(1036, 100, 100);
                        awayTeleported = true;
                    }
                }
            }
            Away = isAway ? (byte)1 : (byte)0;
        }
        public byte Away
        {
            get { return SpawnPacket[PlayerSpawnPacket.Away]; }
            set { SpawnPacket[PlayerSpawnPacket.Away] = value; }
        }
        public byte Boss
        {
            get { return SpawnPacket[PlayerSpawnPacket.Boss]; }
            set
            {
                SpawnPacket[108] = 2;
                SpawnPacket[PlayerSpawnPacket.Boss] = 1;
            }
        }
        public uint UID
        {
            get
            {
                if (SpawnPacket != null)
                    return Core.BitConverter.ToUInt32(SpawnPacket, PlayerSpawnPacket.UID);
                else return _uid;
            }
            set
            {
                _uid = value;
                WriteUInt32(value, PlayerSpawnPacket.UID, SpawnPacket);
            }
        }
        public ushort GuildID
        {
            get { return Core.BitConverter.ToUInt16(SpawnPacket, PlayerSpawnPacket.GuildID); }
            set
            {
                WriteUInt32(value, PlayerSpawnPacket.GuildID, SpawnPacket);
                if (FullyLoaded)
                    UpdateDB("GuildID", value);
            }
        }
        public ushort GuildRank
        {
            get { return Core.BitConverter.ToUInt16(SpawnPacket, PlayerSpawnPacket.GuildRank); }
            set { WriteUInt16(value, PlayerSpawnPacket.GuildRank, SpawnPacket); }
        }
        public ushort Strength
        {
            get { return _strength; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.Strength, value, false);
                }
                _strength = value;
            }
        }
        public ushort Agility
        {
            get
            {
                if (OnCyclone())
                    return (ushort)(_agility);
                return _agility;
            }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Agility, value, false);
                _agility = value;
            }
        }
        public ushort Spirit
        {
            get { return _spirit; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Spirit, value, false);
                _spirit = value;
            }
        }
        public ushort Vitality
        {
            get { return _vitality; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Vitality, value, false);
                _vitality = value;
            }
        }
        public ushort Atributes
        {
            get { return _atributes; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Atributes, value, false);
                _atributes = value;
            }
        }
        public uint MysteryFruit;
        public uint MaxHitpoints
        {
            get { return _maxhitpoints; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {

                    if (/*TransformationID != 0 && */TransformationID != 98)
                        Update((byte)PacketFlag.DataType.MaxHitpoints, value, true);
                }
                _maxhitpoints = value;
            }
        }
        public uint Hitpoints
        {
            get
            {
                return _hitpoints;
            }
            set
            {
                value = (uint)Math.Max(0, (int)value);
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.Hitpoints, value, true);
                }
                else if (PlayerFlag == PlayerFlag.Monster)
                {
                    if (Name != "Thundercloud")
                    {
                        var update = new MsgUpdate(true);
                        update.UID = UID;
                        update.Append((byte)PacketFlag.DataType.Hitpoints, value);
                        MonsterInfo.SendScreen(update);
                    }
                }
                _hitpoints = value;
                if (Boss > 0 && Name != "Thundercloud")
                {
                    uint key = (uint)(MaxHitpoints / 10000);
                    if (key != 0)
                        Writer.Write((ushort)(value / key), PlayerSpawnPacket.Hitpoints, SpawnPacket);
                    else
                        Writer.Write((ushort)(value * MaxHitpoints / 1000 / 1.09), PlayerSpawnPacket.Hitpoints, SpawnPacket);
                }
                else
                    Writer.Write((ushort)value, PlayerSpawnPacket.Hitpoints, SpawnPacket);
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (Owner != null)
                    {
                        if (Owner.Team != null)
                        {
                            foreach (var Team in Owner.Team.Temates)
                            {
                                MsgTeamMember addme = new MsgTeamMember();
                                addme.UID = Owner.Player.UID;
                                addme.Hitpoints = (ushort)Owner.Player.Hitpoints;
                                addme.Mesh = Owner.Player.Mesh;
                                addme.Name = Owner.Player.Name;
                                addme.MaxHitpoints = (ushort)Owner.Player.MaxHitpoints;
                                Team.entry.Send(addme.ToArray());
                            }
                        }
                    }
                }
            }
        }
        public ushort Mana
        {
            get { return _mana; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Mana, value, false);
                _mana = value;
            }
        }
        public ushort MaxMana
        {
            get { return _maxmana; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.MaxMana, value, false);
                _maxmana = value;
            }
        }
        public ushort HairStyle
        {
            get { return _hairstyle; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.HairStyle, value, true);
                }
                _hairstyle = value;
                WriteUInt16(value, PlayerSpawnPacket.HairStyle, SpawnPacket);
            }
        }
        public byte SubPro
        {
            get { return _SubPro; }
            set
            {
                _SubPro = value;
                SpawnPacket[PlayerSpawnPacket.SubPro] = (PlayerFlag != PlayerFlag.Monster) ? _SubPro : ((byte)0);
                if (SubProStages != null)
                    WriteUInt32(SubProStages.GetHashPoint(), PlayerSpawnPacket.SubPro + 1, SpawnPacket);
                if (PlayerFlag == PlayerFlag.Player)
                {
                    if (FullyLoaded)
                    {
                        UpdateDB("SubPro", _SubPro);
                    }
                }
            }
        }
        public byte SubProActive
        {
            get { return SpawnPacket[PlayerSpawnPacket.SubProActive]; }
            set { SpawnPacket[PlayerSpawnPacket.SubProActive] = value; }
        }
        public byte SubClassLevel
        {
            get { return _SubProLevel; }
            set
            {
                _SubProLevel = value;
                switch (PlayerFlag)
                {
                    case PlayerFlag.Player:
                        if (FullyLoaded)
                        {
                            UpdateDB("SubProLevel", value);
                        }
                        break;
                }
            }
        }
        public ConquerStructures.NobilityRank NobilityRank
        {
            get { return (ConquerStructures.NobilityRank)SpawnPacket[PlayerSpawnPacket.NobilityRank]; }
            set
            {
                SpawnPacket[PlayerSpawnPacket.NobilityRank] = (byte)value;
                if (Owner != null)
                {
                    if (Owner.AsMember != null)
                    {
                        Owner.AsMember.NobilityRank = value;
                    }
                }
            }
        }
        public byte HairColor
        {
            get { return (byte)(HairStyle / 100); }
            set { HairStyle = (ushort)((value * 100) + (HairStyle % 100)); }
        }
        public Enums.AppearanceType Appearance
        {
            get { return (Enums.AppearanceType)Core.BitConverter.ToUInt16(SpawnPacket, PlayerSpawnPacket.Appearance); }
            set { WriteUInt16((ushort)value, PlayerSpawnPacket.Appearance, SpawnPacket); }
        }
        public ushort MapID
        {
            get
            {
                return _mapid;
            }
            set
            {
                _mapid = value;
            }
        }
        //public ushort MapID
        //{
        //    get { return _mapid; }
        //    set
        //    {
        //        _mapid = value;
        //        if (PlayerFlag == PlayerFlag.Player && FullyLoaded)
        //            UpdateDB("MapID", value);
        //    }
        //}
        public uint Status4
        {
            get { return _status4; }
            set { _status4 = value; }
        }
        public ushort PreviousMapID
        {
            get { return _previousmapid; }
            set { _previousmapid = value; }
        }
        public ushort X
        {
            get { return _x; }
            set
            {
                _x = value;
                WriteUInt16(value, PlayerSpawnPacket.X, SpawnPacket);
                if (PlayerFlag == PlayerFlag.Player)
                    if (FullyLoaded)
                        UpdateDB("X", value);
            }
        }
        public ushort Y
        {
            get { return _y; }
            set
            {
                _y = value;
                WriteUInt16(value, PlayerSpawnPacket.Y, SpawnPacket);
                if (PlayerFlag == PlayerFlag.Player)
                    if (FullyLoaded)
                        UpdateDB("Y", value);
            }
        }
        public ushort PX
        {
            get;
            set;
        }
        public ushort PY
        {
            get;
            set;
        }
        public bool Dead
        {
            get { return Hitpoints < 1; }
            set { throw new NotImplementedException(); }
        }
       
        public ushort Defence
        {
            get
            {
                if (Time32.Now < ShieldStamp.AddSeconds(ShieldTime) && ContainsFlag((ulong)PacketFlag.Flags.MagicShield))
                    if (ShieldIncrease > 0)
                        return (ushort)Math.Min(65535, (int)(BaseDefence * ShieldIncrease));
                if (SuperItemBless > 0)
                    return (ushort)(BaseDefence + (float)BaseDefence / 100 * SuperItemBless);
                return BaseDefence;
            }
            set { BaseDefence = value; }
        }
        public ushort TransformationDefence
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.MagicShield))
                {
                    if (ShieldTime > 0)
                        return (ushort)(_TransPhysicalDefence * ShieldIncrease);
                    else
                        return (ushort)(_TransPhysicalDefence * MagicShieldIncrease);
                }
                return (ushort)_TransPhysicalDefence;
            }
            set { _TransPhysicalDefence = value; }
        }
        public ushort MagicDefencePercent
        {
            get { return _MDefencePercent; }
            set { _MDefencePercent = value; }
        }
        public ushort TransformationMagicDefence
        {
            get { return (ushort)_TransMagicDefence; }
            set { _TransMagicDefence = value; }
        }
        public ushort MagicDefence
        {
            get { return _MDefence; }
            set { _MDefence = value; }
        }
        public Client.GameState Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }
        public uint TransformationMinAttack
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.Stigma))
                    return (uint)(_TransMinAttack * StigmaIncrease);
                return _TransMinAttack;
            }
            set { _TransMinAttack = value; }
        }
        public uint TransformationMaxAttack
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.Stigma))
                    return (uint)(_TransMaxAttack * StigmaIncrease);
                return _TransMaxAttack;
            }
            set { _TransMaxAttack = value; }
        }
        public uint MinAttack
        {
            get { return _MinAttack; }
            set { _MinAttack = value; }
        }
        public uint MaxAttack
        {
            get { return _MaxAttack; }
            set { _MaxAttack = value; }
        }
        public uint MagicAttack
        {
            get { return _MagicAttack; }
            set { _MagicAttack = value; }
        }
        public uint MAttack;
        public byte Dodge
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.Dodge))
                {
                    return (byte)(_Dodge * DodgeIncrease);
                }
                return _Dodge;
            }
            set { _Dodge = value; }
        }
        public byte TransformationDodge
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.Dodge))
                    return (byte)(_TransDodge * DodgeIncrease);
                return (byte)_TransDodge;
            }
            set { _TransDodge = value; }
        }
        public MapObjectType MapObjType
        {
            get { return _MapObjectType; }
            set { _MapObjectType = value; }
        }
        public PlayerFlag PlayerFlag
        {
            get { return _PlayerFlag; }
            set { _PlayerFlag = value; }
        }
        public ulong Experience
        {
            get { return _experience; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                    Update((byte)PacketFlag.DataType.Experience, value, false);
                _experience = value;
            }
        }
        public ushort EnlightenPoints
        {
            get { return _enlightenPoints; }
            set { _enlightenPoints = value; }
        }
        public byte ReceivedEnlightenPoints
        {
            get { return _receivedEnlighenPoints; }
            set { _receivedEnlighenPoints = value; }
        }
        public ushort EnlightmentTime
        {
            get { return _enlightmenttime; }
            set { _enlightmenttime = value; }
        }
        public ushort PKPoints
        {
            get { return _pkpoints; }
            set
            {
                _pkpoints = value;
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.PKPoints, value, false);
                    if (PKPoints > 99)
                    {
                        RemoveFlag((ulong)PacketFlag.Flags.RedName);
                        AddFlag((ulong)PacketFlag.Flags.BlackName);
                    }
                    else if (PKPoints > 29)
                    {
                        AddFlag((ulong)PacketFlag.Flags.RedName);
                        RemoveFlag((ulong)PacketFlag.Flags.BlackName);
                    }
                    else if (PKPoints < 30)
                    {
                        RemoveFlag((ulong)PacketFlag.Flags.RedName);
                        RemoveFlag((ulong)PacketFlag.Flags.BlackName);
                    }
                }
            }
        }     
        public UInt32 ClanId
        {
            get { return Core.BitConverter.ToUInt32(SpawnPacket, PlayerSpawnPacket.ClanUID); }
            set { WriteUInt32((UInt32)value, PlayerSpawnPacket.ClanUID, SpawnPacket); }
        }
        public MsgFamily Myclan;
        public MsgFamily.Ranks ClanRank
        {
            get { return (MsgFamily.Ranks)SpawnPacket[PlayerSpawnPacket.ClanRank]; }
            set { SpawnPacket[PlayerSpawnPacket.ClanRank] = (Byte)value; }
        }
        public MsgFamily GetClan
        {
            get
            {
                MsgFamily cl;
                Kernel.Clans.TryGetValue(ClanId, out cl);
                return cl;
            }
        }
        string clan = "";
        public string ClanName
        {
            get { return clan; }
            set
            {
                clan = value;
                if (Owner == null)
                    WriteStringList(new List<string>() { _Name, "", clan, "", "", "", "" }, MsgPlayer.NameClan, SpawnPacket);
                else
                    WriteStringList(new List<string>() { _Name, "", clan, "", "", "", UnionName }, MsgPlayer.NameClan, SpawnPacket);
            }
        }
        //string clan = "";
        //public string ClanName
        //{
        //    get { return clan; }
        //    set
        //    {
        //        clan = value;
        //        if (clan.Length > 15)
        //            clan = clan.Substring(0, 15);
        //        Writer.Write(new List<string>() { _Name, "", clan, "" }, PlayerSpawnPacket.PlayerName, SpawnPacket);
        //    }
        //}
        public PKMode PKMode
        {
            get { return _PKMode; }
            set { _PKMode = value; }
        }
        public ushort Action
        {
            get { return Core.BitConverter.ToUInt16(SpawnPacket, PlayerSpawnPacket.Action); }
            set { WriteUInt16(value, PlayerSpawnPacket.Action, SpawnPacket); }
        }
        public Enums.ConquerAngle Facing
        {
            get { return (Enums.ConquerAngle)SpawnPacket[PlayerSpawnPacket.Facing]; }
            set { SpawnPacket[PlayerSpawnPacket.Facing] = (byte)value; }
        }
        private ulong _Stateff2 = 0, _Stateff3 = 0, _Stateff4 = 0;
        public ulong StatusFlag
        {
            get { return Core.BitConverter.ToUInt64(SpawnPacket, PlayerSpawnPacket.StatusFlag); }
            set
            {
                ulong OldV = StatusFlag;
                if (value != OldV)
                {
                    WriteUInt64(value, PlayerSpawnPacket.StatusFlag, SpawnPacket);
                    UpdateEffects(true);
                }
            }
        }
        public ulong StatusFlag2
        {
            get { return _Stateff2; }
            set
            {
                ulong OldV = StatusFlag2;
                if (value != OldV)
                {
                    _Stateff2 = value;
                    WriteUInt64(value, PlayerSpawnPacket.StatusFlag2, SpawnPacket);
                    UpdateEffects(true);
                }
            }
        }
        public ulong StatusFlag3
        {
            get { return _Stateff3; }
            set
            {
                ulong OldV = StatusFlag3;
                if (value != OldV)
                {
                    _Stateff3 = value;
                    WriteUInt64(value, PlayerSpawnPacket.StatusFlag3, SpawnPacket);
                    UpdateEffects(true);
                }
            }
        }
        public ulong StatusFlag4
        {
            get { return _Stateff4; }
            set
            {
                ulong OldV = StatusFlag4;
                if (value != OldV)
                {
                    _Stateff4 = value;
                    WriteUInt64(value, PlayerSpawnPacket.StatusFlag4, SpawnPacket);
                    UpdateEffects(true);
                }
            }
        }
        public void AddFlag(ulong flag)
        {
            StatusFlag |= flag;
        }
        public bool ContainsFlag(ulong flag)
        {
            ulong aux = StatusFlag;
            aux &= ~flag;
            return !(aux == StatusFlag);
        }
        public void RemoveFlag(ulong flag)
        {
            if (ContainsFlag(flag))
            {
                StatusFlag &= ~flag;
            }
        }
        public void AddFlag2(ulong flag)
        {
            if (flag == (ulong)PacketFlag.Flags.SoulShackle)
            {
                StatusFlag2 |= flag; return;
            }
            if (!ContainsFlag((ulong)PacketFlag.Flags.Dead) && !ContainsFlag((ulong)PacketFlag.Flags.Ghost))
                StatusFlag2 |= flag;
        }
        public bool ContainsFlag2(ulong flag)
        {
            ulong aux = StatusFlag2;
            aux &= ~flag;
            return !(aux == StatusFlag2);
        }
        public void RemoveFlag2(ulong flag)
        {
            if (ContainsFlag2(flag))
            {
                StatusFlag2 &= ~flag;
            }
        }
        public void AddFlag3(ulong flag)
        {
            StatusFlag3 |= flag;
        }
        public bool ContainsFlag3(ulong flag)
        {
            ulong aux = StatusFlag3;
            aux &= ~flag;
            return !(aux == StatusFlag3);
        }
        public void RemoveFlag3(ulong flag)
        {
            if (ContainsFlag3(flag))
            {
                StatusFlag3 &= ~flag;
            }
        }
        public void AddFlag4(ulong flag)
        {
            StatusFlag4 |= flag;
        }
        public bool ContainsFlag4(ulong flag)
        {
            ulong aux = StatusFlag4;
            aux &= ~flag;
            return !(aux == StatusFlag4);
        }
        public void RemoveFlag4(ulong flag)
        {
            if (ContainsFlag4(flag))
            {
                StatusFlag4 &= ~flag;
            }
        }
        public void SetVisible()
        {
            SpawnPacket[PlayerSpawnPacket.WindowSpawn] = 0;
        }
        public MsgNationality.Mode CountryID
        {
            get { return (MsgNationality.Mode)Core.BitConverter.ToUInt16(SpawnPacket, PlayerSpawnPacket.CountryFlag); }
            set { WriteUInt16((ushort)value, PlayerSpawnPacket.CountryFlag, SpawnPacket); }
        }
        public bool Assassin()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 613)
                        return true;
                    else if (weapons.Item2 != null)
                        if (weapons.Item2.ID / 1000 == 613)
                            return true;
            }
            return false;
        }
        public bool EpicTrojan()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 614)
                        return true;
                    else if (weapons.Item2 != null)
                        if (weapons.Item2.ID / 1000 == 614)
                            return true;
            }
            return false;
        }
        private bool spiritFocus;
        public bool SpiritFocus
        {
            get
            {
                if (!spiritFocus) return false;
                if (Time32.Now > spiritFocusStamp.AddSeconds(5))
                    return true;
                return false;
            }
            set { spiritFocus = value; if (value) spiritFocusStamp = Time32.Now; }
        }
        public int SuperItemBless;
        public float SpiritFocusPercent;
        public bool Aura_isActive;
        public ulong Aura_actType;
        public ulong Aura_actType2;
        public uint Aura_actPower;
        public uint Aura_actLevel;
        public bool JustCreated = false;
        public int ChainboltTime;
        public int FlameTime;
        public int FrozenTime;
        public PKMode PrevPKMode;
        private uint assassinBP;
        internal void PreviousTeleport()
        {
            Teleport(PreviousMapID, PrevX, PrevY);
            BringToLife();
        }
        public bool IsThisLeftGate(int X, int Y)
        {
            if (Game.GuildWar.RightGate == null)
                return false;
            if (MapID == 1038)
            {
                if ((X == 223 || X == 222) && (Y >= 175 && Y <= 185))
                {
                    if (Game.GuildWar.RightGate.Mesh / 10 == 27)
                    {
                        return true;
                    }
                }
            }
            if (Game.Kingdom.RightGate == null)
                return false;
            if (MapID == 10380)
            {
                if ((X == 223 || X == 222) && (Y >= 175 && Y <= 185))
                {
                    if (Game.Kingdom.RightGate.Mesh / 10 == 27)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsThisRightGate(int X, int Y)
        {
            if (Game.GuildWar.LeftGate == null)
                return false;
            if (MapID == 1038)
            {
                if ((Y == 210 || Y == 209) && (X >= 154 && X <= 166))
                {
                    if (Game.GuildWar.LeftGate.Mesh / 10 == 24)
                    {
                        return true;
                    }
                }
            }
            if (Game.Kingdom.LeftGate == null)
                return false;
            if (MapID == 10380)
            {
                if ((Y == 210 || Y == 209) && (X >= 154 && X <= 166))
                {
                    if (Game.Kingdom.LeftGate.Mesh / 10 == 24)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool InJail()
        {
            if (MapID == 6000 || MapID == 6001)
                return true;
            return false;
        }
        public bool ThroughGate(int X, int Y)
        {
            return IsThisLeftGate(X, Y) || IsThisRightGate(X, Y);
        }
        public uint AssassinColor
        {
            get
            {
                return System.BitConverter.ToUInt32(this.SpawnPacket, 254);
            }
            set
            {
                WriteUInt32(value, 254, this.SpawnPacket);
            }
        }
        public uint AssassinBP
        {
            get { return assassinBP; }
            set
            {
                assassinBP = value;
                Writer.WriteUInt32(value, PlayerSpawnPacket.EquipmentColor, SpawnPacket);
            }
        }
        private uint _GuildSharedBp, _boundCps, _luckypoints, _RacePoints;
        private uint Flor;
        public uint FlowerRank
        {
            get { return Flor; }
            set
            {
                Flor = value;
                Writer.WriteUInt32(value + 10000, PlayerSpawnPacket.Flower, SpawnPacket);
            }
        }
        public uint GuildSharedBp
        {
            get { return _GuildSharedBp; }
            set
            {
                switch (PlayerFlag)
                {
                    case PlayerFlag.Player:
                        {
                            if (FullyLoaded)
                            {
                                Update((byte)PacketFlag.DataType.GuildBattlePower, Math.Min(value, 15), false);
                                WriteUInt32(Math.Min(value, 15), PlayerSpawnPacket.GuildSharedBp, SpawnPacket);
                            }
                            break;
                        }
                }
                _GuildSharedBp = value;
            }
        }
        public uint ClanSharedBp
        {
            get { return Core.BitConverter.ToUInt32(this.SpawnPacket, PlayerSpawnPacket.ClanSharedBp); }
            set
            {
                if ((PlayerFlag == PlayerFlag.Player) && FullyLoaded)
                {
                    MsgFamily getClan = GetClan;
                    Update((byte)PacketFlag.DataType.ClanBattlePower, value, false);
                }
                Writer.WriteUint(value, PlayerSpawnPacket.ClanSharedBp, SpawnPacket);
            }
        }
        public uint BoundCps
        {
            get { return _boundCps; }
            set
            {
                value = (uint)Math.Max(0, (uint)value);
                if (value >= uint.MaxValue)
                {
                    value = uint.MaxValue;
                    Owner.MessageBox("Max Allowed ConquerPoints :" + uint.MaxValue);
                }
                switch (PlayerFlag)
                {
                    case PlayerFlag.Player:
                        {
                            if (FullyLoaded)
                            {
                                UpdateDB("BoundCPS", value);
                                Update((byte)PacketFlag.DataType.BoundConquerPoints, value, false);
                            }
                            break;
                        }
                }
                _boundCps = value;
            }
        }

        public uint Lukypionts
        {
            get { return _luckypoints; }
            set
            {
                _luckypoints = value;
                if (FullyLoaded)
                    UpdateDB("LuckyPoints", value);
            }
        }
        public uint RacePoints
        {
            get { return _RacePoints; }
            set
            {
                if (value <= 0)
                    value = 0;
                _RacePoints = value;
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.RaceShopPoints, (uint)value, false);
                }
            }
        }
        public void Save(String row, String value)
        {
            using (var Command = new MySqlCommand(MySqlCommandType.UPDATE))
                Command.Update("entities").Set(row, value).Where("uid", UID).Execute();
        }
        public void Save(String row, UInt16 value)
        {
            using (var Command = new MySqlCommand(MySqlCommandType.UPDATE))
                Command.Update("entities").Set(row, value).Where("uid", UID).Execute();
        }
        public void Save(String row, Boolean value)
        {
            using (var Command = new MySqlCommand(MySqlCommandType.UPDATE))
                Command.Update("entities").Set(row, value).Where("uid", UID).Execute();
        }
        public void Save(String row, UInt32 value)
        {
            using (var Command = new MySqlCommand(MySqlCommandType.UPDATE))
                Command.Update("entities").Set(row, value).Where("uid", UID).Execute();
        }
        #endregion
        #region SendScreen
        public void SendScreen(Interfaces.IPacket Data)
        {
            GameState[] Chars = new GameState[Kernel.GamePool.Count];
            Kernel.GamePool.Values.CopyTo(Chars, 0);
            foreach (GameState C in Chars)
                if (C != null)
                    if (C.Player != null)
                        if (Calculations.PointDistance(X, Y, C.Player.X, C.Player.Y) <= 20)
                            C.Send(Data);
            Chars = null;
        }
        #endregion
        #region Functions
        public ushort BattlePowerCalc(Player e)
        {
            UInt16 BP = (ushort)(e.Level + ExtraBattlePower);
            if (e == null) return 0;
            if (e.Owner == null) return 0;
            if (e.Owner.Equipment == null) return 0;
            var weapons = e.Owner.Weapons;
            foreach (var i in e.Owner.Equipment.Objects.Where(i => i != null))
            {
                if (i == null) continue;
                int pos = i.Position;
                if (pos > 20) pos -= 20;
                if (pos != MsgItemInfo.Bottle &&
                    pos != MsgItemInfo.Garment && pos != MsgItemInfo.RightWeaponAccessory &&
                    pos != MsgItemInfo.LeftWeaponAccessory && pos != MsgItemInfo.SteedArmor)
                {
                    if (!i.IsWorn) continue;
                    if (pos == MsgItemInfo.RightWeapon || pos == MsgItemInfo.LeftWeapon)
                        continue;
                    BP += ItemBatlePower(i);
                }
            }
            if (weapons.Item1 != null)
            {
                var i = weapons.Item1;
                Byte Multiplier = 1;
                if (i.IsTwoHander())
                    Multiplier = weapons.Item2 == null ? (Byte)2 : (Byte)1;
                BP += (ushort)(ItemBatlePower(i) * Multiplier);
            }
            if (weapons.Item2 != null)
            {
                BP += ItemBatlePower(weapons.Item2);
            }
            if (PlayerFlag == PlayerFlag.Player)
            {
                BP += (Byte)e.NobilityRank;
            }
            BP += (Byte)(e.Reborn * 5);
            BP += (Byte)GuildSharedBp;
            BP += (ushort)ExtraBattlePower;
            EquipmentColor = BP;
            return BP;
        }
        private ushort ItemBatlePower(MsgItemInfo i)
        {
            Byte Multiplier = 1;
            Byte quality = (Byte)(i.ID % 10);
            int BP = 0;
            if (quality >= 6)
            {
                BP += (Byte)((quality - 5) * Multiplier);
            }
            if (i.SocketOne != 0)
            {
                BP += (Byte)(1 * Multiplier);
                if ((Byte)i.SocketOne % 10 == 3)
                    BP += (Byte)(1 * Multiplier);
                if (i.SocketTwo != 0)
                {
                    BP += (Byte)(1 * Multiplier);
                    if ((Byte)i.SocketTwo % 10 == 3)
                        BP += (Byte)(1 * Multiplier);
                }
            }
            BP += (Byte)(i.Plus * Multiplier);
            return (ushort)BP;
        }
        public Player(PlayerFlag Flag, bool companion)
        {
            LastClientTick = 0;
            LastRecClientTick = NativeTime32.Now.Value;
            LastReqClientTick = NativeTime32.Now.Value;

            Companion = companion;
            PlayerFlag = Flag;
            Mode = Enums.Mode.None;
            update = new MsgUpdate(true);
            update.UID = UID;
            switch (Flag)
            {
                case PlayerFlag.Player:
                    {
                        MapObjType = MapObjectType.Player;
                        Halos = new ConcurrentDictionary<int, DateTime>();
                        break;
                    }
                case PlayerFlag.Monster:
                    {
                        MapObjType = MapObjectType.Monster;
                        break;
                    }
            }
            SpawnPacket = new byte[0];
        }
        public void Ressurect()
        {
            if (PlayerFlag == PlayerFlag.Player)
                Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = DB.MapsTable.MapInformations[Owner.Map.ID].Status, Weather = DB.MapsTable.MapInformations[Owner.Map.ID].Weather });
        }
        public void BringToLife()
        {
            Hitpoints = MaxHitpoints;
            TransformationID = 0;
            Stamina = 100;
            FlashingNameTime = 0;
            FlashingNameStamp = Time32.Now;
            RemoveFlag((ulong)PacketFlag.Flags.FlashingName);
            RemoveFlag((ulong)PacketFlag.Flags.Dead | (ulong)PacketFlag.Flags.Ghost);
            if (PlayerFlag == PlayerFlag.Player)
                Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = DB.MapsTable.MapInformations[Owner.Map.ID].Status });
            if (PlayerFlag == PlayerFlag.Player)
            {
                Owner.ReviveStamp = Time32.Now;
                Owner.Player.AutoRev = 0;
                Owner.Attackable = false;
            }
        }
        
        public void DropRandomStuff(Player KillerName)
        {
            if (Money > 100)
            {
                int amount = (int)(Money / 2);
                amount = Kernel.Random.Next(amount);
                if (MyMath.Success(40))
                {
                    uint ItemID = ItemHandler.MoneyItemID((uint)amount);
                    ushort x = X, y = Y;
                    Game.Map Map = Kernel.Maps[MapID];
                    if (Map.SelectCoordonates(ref x, ref y))
                    {
                        Money -= (uint)amount;
                        MsgMapItem floorItem = new MsgMapItem(true);
                        floorItem.ValueType = MsgMapItem.FloorValueType.Money;
                        floorItem.Value = (uint)amount;
                        floorItem.ItemID = ItemID;
                        floorItem.MapID = MapID;
                        floorItem.MapObjType = Game.MapObjectType.Item;
                        floorItem.X = x;
                        floorItem.Y = y;
                        floorItem.Type = PacketMsgMapItem.Drop;
                        floorItem.OnFloor = Time32.Now;
                        floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                        while (Map.Npcs.ContainsKey(floorItem.UID))
                            floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                        Map.AddFloorItem(floorItem);
                        Owner.SendScreenSpawn(floorItem, true);
                    }
                }
            }
            if (Owner.Inventory.Count > 0)
            {
                var array = Owner.Inventory.Objects.ToArray();
                uint count = (uint)(array.Length / 4);
                byte startfrom = (byte)Kernel.Random.Next((int)count);
                for (int c = 0; c < count; c++)
                {
                    int index = c + startfrom;
                    if (array[index] != null)
                    {
                        var infos = DB.ConquerItemInformation.BaseInformations[(uint)array[index].ID];
                        if (infos.Type == ConquerItemBaseInformation.ItemType.Dropable)
                        {
                            if (array[index].Lock == 0)
                            {
                                {
                                    if (!array[index].Bound && !array[index].Inscribed && array[index].ID != 723753)
                                    {
                                        if (!array[index].Suspicious && array[index].Lock != 1 && array[index].ID != 723755 && array[index].ID != 723767 && array[index].ID != 723772)
                                        {
                                            if (MyMath.Success(140) && array[index].ID != 723774 && array[index].ID != 723776)
                                            {
                                                var Item = array[index];
                                                if (Item.ID >= 729960 && Item.ID <= 729970)
                                                    return;
                                                Item.Lock = 0;
                                                ushort x = X, y = Y;
                                                Game.Map Map = Kernel.Maps[MapID];
                                                if (Map.SelectCoordonates(ref x, ref y))
                                                {
                                                    Network.GamePackets.MsgMapItem floorItem = new Network.GamePackets.MsgMapItem(true);
                                                    Owner.Inventory.Remove(Item, Enums.ItemUse.Remove);
                                                    floorItem.Item = Item;
                                                    floorItem.ValueType = Network.GamePackets.MsgMapItem.FloorValueType.Item;
                                                    floorItem.ItemID = (uint)Item.ID;
                                                    floorItem.MapID = MapID;
                                                    floorItem.MapObjType = Game.MapObjectType.Item;
                                                    floorItem.X = x;
                                                    floorItem.Y = y;
                                                    floorItem.Type = PacketMsgMapItem.Drop;
                                                    floorItem.OnFloor = Time32.Now;
                                                    floorItem.ItemColor = floorItem.Item.Color;
                                                    floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                    while (Map.Npcs.ContainsKey(floorItem.UID))
                                                        floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                    Map.AddFloorItem(floorItem);
                                                    Owner.SendScreenSpawn(floorItem, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #region RedName
            if (PKPoints >= 30 && Killer != null && Killer.Owner != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    var rnd = Kernel.Random.Next(19);
                    if (Owner.AlternateEquipment)
                        rnd = Kernel.Random.Next(10, 29);

                    var item = Owner.Equipment.TryGetItem((byte)rnd);
                    var Item = item;

                    if (Item != null)
                    {
                        byte dwp = 20;
                        if (!Owner.AlternateEquipment)
                        {
                            dwp = 0;
                            if (Item.Position >= 20)
                                continue;
                        }

                        // Verifica posições protegidas (incluindo as novas)
                        if (Item.Position == 7 + dwp ||   // Colar
                            Item.Position == 9 + dwp ||   // Garment
                            Item.Position == 12 ||        // Horse (não usa dwp)
                            Item.Position == 15 + dwp ||  // Anel Esquerdo
                            Item.Position == 16 + dwp ||  // Anel Direito
                            Item.Position == 17 + dwp ||  // Bottle
                            Item.Position == 18 + dwp)    // Acessório de Montaria
                        {
                            continue;
                        }

                        // Verificação especial para mão esquerda (posição 4)
                        if (Item.Position == 4 + dwp)
                        {
                            if (!Owner.Equipment.Free((byte)(5 + dwp)))
                            {
                                Item = Owner.Equipment.TryGetItem((byte)(5 + dwp));
                                if (Item == null ||
                                    Item.Bound ||
                                    Item.Position == 7 + dwp ||
                                    Item.Position == 9 + dwp ||
                                    Item.Position == 12 ||
                                    Item.Position == 15 + dwp ||
                                    Item.Position == 16 + dwp ||
                                    Item.Position == 17 + dwp ||
                                    Item.Position == 18 + dwp)
                                {
                                    continue;
                                }
                            }
                        }

                        // Verifica itens com ID começando com 105
                        if (Item.Position == 5 + dwp && Item.ID.ToString().StartsWith("105"))
                            continue;

                        // Verifica se o item é bound
                        if (Item.Bound)
                            continue;

                        // Chance de dropar o item
                        if (MyMath.Success(25 + (int)(PKPoints > 30 ? 75 : 0)))
                        {
                            ushort x = X, y = Y;
                            Game.Map Map = Kernel.Maps[MapID];
                            if (Map.SelectCoordonates(ref x, ref y))
                            {
                                Owner.Equipment.RemoveToGround(Item.Position);
                                var infos = DB.ConquerItemInformation.BaseInformations[(uint)Item.ID];

                                Network.GamePackets.MsgMapItem floorItem = new Network.GamePackets.MsgMapItem(true);
                                floorItem.Item = Item;
                                floorItem.ValueType = Network.GamePackets.MsgMapItem.FloorValueType.Item;
                                floorItem.ItemID = (uint)Item.ID;
                                floorItem.MapID = MapID;
                                floorItem.MapObjType = Game.MapObjectType.Item;
                                floorItem.X = x;
                                floorItem.Y = y;
                                floorItem.Type = PacketMsgMapItem.Drop;
                                floorItem.OnFloor = Time32.Now;
                                floorItem.ItemColor = floorItem.Item.Color;
                                floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                while (Map.Npcs.ContainsKey(floorItem.UID))
                                    floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                Map.AddFloorItem(floorItem);
                                Owner.SendScreenSpawn(floorItem, true);

                                Owner.Equipment.UpdateEntityPacket();
                                MsgItemEquip eq = new MsgItemEquip(Owner);
                                eq.DoEquips(Owner);
                                Owner.Send(eq.ToArray());
                                Owner.LoadItemStats();

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                Kernel.SendWorldMessage(new MsgTalk(Name + " has been captured by " + KillerName.Name + " and sent in jail! The world is now safer!",
                    System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk), Server.GamePool);
            }
            #endregion
            if (PKPoints > 99)
            {
                if (KillerName.PlayerFlag == PlayerFlag.Player)
                {
                    Kernel.SendWorldMessage(new MsgTalk(Name + " has been captured by " + KillerName.Name + " and sent in jail! The world is now safer!", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk), Server.GamePool);
                    Teleport(6000, 50, 50);
                }
                else
                {
                    Kernel.SendWorldMessage(new Network.GamePackets.MsgTalk(Name + " has been captured and sent in jail! The world is now safer!", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk), Server.GamePool);
                    Teleport(6000, 50, 50);
                }
            }
        }
        public void Die(uint killer)
        {

            if (Owner.Companion != null)
            {
                Owner.Map.RemoveEntity(Owner.Companion);
                var data = new MsgAction(true);
                data.UID = Owner.Companion.UID;
                data.ID = PacketMsgAction.Mode.RemoveEntity;
                Owner.Companion.MonsterInfo.SendScreen(data);
                Owner.Companion = null;
            }
            if (MyClones.Count != 0)
            {
                foreach (var clone in MyClones)
                    clone.RemoveThat();
                MyClones.Clear();
            }
            if (PlayerFlag == PlayerFlag.Player)
            {
                Owner.XPCount = 0;
                if (Owner.Booth != null)
                {
                    Owner.Booth.Remove();
                    Owner.Booth = null;
                }
            }
            Killed = true;
            Hitpoints = 0;
            DeathStamp = Time32.Now;
            ToxicFogLeft = 0;
            if (Companion)
            {
                if (Hitpoints < 1)
                {
                    Hitpoints = 0;
                    AddFlag((ulong)PacketFlag.Flags.Ghost | (ulong)PacketFlag.Flags.Dead | (ulong)PacketFlag.Flags.FadeAway);
                    MsgInteract attack = new MsgInteract(true);
                    attack.Attacked = UID;
                    attack.InteractType = MsgInteract.Kill;
                    attack.X = X;
                    attack.Y = Y;
                    MonsterInfo.SendScreen(attack);
                    Owner.Map.RemoveEntity(this);
                }
            }
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (Constants.PKFreeMaps.Contains(MapID))
                    goto Over;
                Over:
                AddFlag((ulong)PacketFlag.Flags.Dead);
                AddFlag((ulong)PacketFlag.Flags.Ghost);
                RemoveFlag((ulong)PacketFlag.Flags.Fly);
                RemoveFlag((ulong)PacketFlag.Flags.Ride);
                RemoveFlag((ulong)PacketFlag.Flags.Cyclone);
                RemoveFlag((ulong)PacketFlag.Flags.Superman);
                RemoveFlag((ulong)PacketFlag.Flags.FatalStrike);
                RemoveFlag((ulong)PacketFlag.Flags.FlashingName);
                RemoveFlag((ulong)PacketFlag.Flags.ShurikenVortex);
                
                RemoveFlag((ulong)PacketFlag.Flags.Praying);
                RemoveFlag2((ulong)PacketFlag.Flags.Oblivion);
                RemoveFlag3((ulong)PacketFlag.Flags.SuperCyclone);
                RemoveFlag3((ulong)PacketFlag.Flags.DragonCyclone);
                RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                RemoveFlag((ulong)PacketFlag.Flags.GodlyShield);
                RemoveFlag2((ulong)PacketFlag.Flags.CarryingFlag);

                RemoveFlag2((ulong)PacketFlag.Flags.EarthAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.FireAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.WaterAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.WoodAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.MetalAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.FendAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.TyrantAuraIcon);
                RemoveFlag2((ulong)PacketFlag.Flags.EarthAura);
                RemoveFlag2((ulong)PacketFlag.Flags.FireAura);
                RemoveFlag2((ulong)PacketFlag.Flags.WaterAura);
                RemoveFlag2((ulong)PacketFlag.Flags.WoodAura);
                RemoveFlag2((ulong)PacketFlag.Flags.MetalAura);
                RemoveFlag2((ulong)PacketFlag.Flags.FendAura);
                RemoveFlag2((ulong)PacketFlag.Flags.TyrantAura);

                RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer);
                RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer2);
                RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer3);
                RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer4);
                RemoveFlag4((ulong)PacketFlag.Flags.Omnipotence);
                RemoveFlag4((ulong)PacketFlag.Flags.RevengeTaill);
                RemoveFlag4((ulong)PacketFlag.Flags.JusticeChant);
                RemoveFlag4((ulong)PacketFlag.Flags.ChillingSnow);
                RemoveFlag4((ulong)PacketFlag.Flags.FreezingPelter);
                RemoveFlag4((ulong)PacketFlag.Flags.HealingSnow);
                RemoveFlag4((ulong)PacketFlag.Flags.ShadowofChaser);
                RemoveFlag3((ulong)PacketFlag.Flags.BackFire);
                RemoveFlag3((ulong)PacketFlag.Flags.ManiacDance);
                if (ContainsFlag3((ulong)PacketFlag.Flags.AuroraLotus))
                {
                    AuroraLotusEnergy = 0;
                    Lotus(AuroraLotusEnergy, (byte)PacketFlag.DataType.AuroraLotus);
                }
                if (ContainsFlag3((ulong)PacketFlag.Flags.FlameLotus))
                {
                    FlameLotusEnergy = 0;
                    Lotus(FlameLotusEnergy, (byte)PacketFlag.DataType.FlameLotus);
                }

                MsgInteract Interact = new MsgInteract(true);
                Interact.InteractType = Network.GamePackets.MsgInteract.Kill;
                Interact.X = X;
                Interact.Y = Y;
                Interact.Attacked = UID;
                Interact.Attacker = killer;
                Interact.Damage = 0;
                Owner.SendScreen(Interact, true);
                Owner.removeAuraBonuses(Aura_actType, Aura_actPower, 1);
                Aura_isActive = false;
                Aura_actType = 0;
                Aura_actPower = 0;

                if (Body % 10 < 3)
                    TransformationID = 99;
                else
                    TransformationID = 98;

                Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = DB.MapsTable.MapInformations[Owner.Map.ID].Status, Weather = DB.MapsTable.MapInformations[Owner.Map.ID].Weather });
                Owner.EndQualifier();
            }
            else
            {
                Kernel.Maps[MapID].Floor[X, Y, MapObjType, this] = true;
            }
            if (PlayerFlag == PlayerFlag.Player)
                if (OnDeath != null) OnDeath(this);
        }
        public Player Killer;
        public byte DragonSwingReflectVal = 0;
        public byte RestorePercent;
        public Time32 ReStoreStamine;
        public Time32 SpiritStamp;
        public byte AutoRev = 0;
        public SafeDictionary<uint, MsgPkStatistic> PkStatistic = new SafeDictionary<uint, MsgPkStatistic>();
        public static void SendCursed(uint Time, GameState client)
        {
            client.Player.Cursed = Time32.Now;
            MsgUpdate update = new MsgUpdate(true);
            update.UID = client.Player.UID;
            update.Append((byte)PacketFlag.DataType.CursedTimer, Time);
            client.Send(update.ToArray());
            client.Player.AddFlag((ulong)PacketFlag.Flags.Cursed);
            client.Send("You`ve killed too many players. you are strongty advised to stop.");
        }
        public uint First;
        public Time32 ThunderCloudStamp;
        public bool GmHide;
        public bool cantdie = false;  
        public void Die(Player killer)
        {
            #region Die Eventos

            #endregion
            #region IronPoints
            if (killer.MapID == Pezzi.ServerEvents.IronMap.ID)
            {
                if (killer.MapID == Pezzi.ServerEvents.IronMap.ID)
                {
                    killer.Owner.IronPoints += 1;
                    Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "You Have Now " + killer.Owner.IronPoints + "  Points Congratz!");
                    npc.OptionID = 255;
                    killer.Owner.Send(npc.ToArray());
                }
            }
            #endregion
            #region Room SS/FB
            if (MapID == 1238)
            {
                if (ConquerPoints >= 5000000)
                {
                    killer.ConquerPoints += 5000000;
                    killer.Owner.SendA("You have killed your opponent. You receive 5kk CPs.");
                    ConquerPoints -= 5000000;
                    Owner.SendA("You died. You lost 5kk CPs. You now have " + ConquerPoints + ".");
                }
            }
            #endregion
            #region Privxy rooms
            if (PlayerFlag == PlayerFlag.Player && killer.PlayerFlag == PlayerFlag.Player)
                Pezzi.ServerEvents.CheckRooms(this, killer);
            #endregion
            #region CaptureTheFlag
            if (killer.GuildID != 0 && killer.MapID == MsgWarFlag.MapID && MsgWarFlag.IsWar)
            {
                if (GuildID != 0)
                {
                    if (killer.Owner.Guild.Enemy.ContainsKey(GuildID))
                        killer.Owner.Guild.CTFPoints += 1;
                    else if (killer.Owner.Guild.Ally.ContainsKey(GuildID))
                        killer.Owner.Guild.CTFPoints += 1;
                }
                if (ContainsFlag2((ulong)PacketFlag.Flags.CarryingFlag))
                {
                    StaticEntity entity = new StaticEntity((uint)(X * 1000 + Y), X, Y, MapID);
                    entity.DoFlag();
                    Owner.Map.AddStaticEntity(entity);
                    RemoveFlag2((ulong)PacketFlag.Flags.CarryingFlag);
                    Owner.Send(Server.Thread.CTF.generateTimer(0));
                    Owner.Send(Server.Thread.CTF.generateEffect(Owner));
                    if (killer == null && killer.GuildID != GuildID)
                    {
                        Killer.AddFlag2((ulong)PacketFlag.Flags.CarryingFlag);
                        Time32 end = FlagStamp.AddSeconds(60) - Time32.Now;
                        killer.FlagStamp = end;
                        killer.Owner.Send(Server.Thread.CTF.generateTimer((uint)end.Value));
                        killer.Owner.Send(Server.Thread.CTF.generateEffect(killer.Owner));
                        killer.Owner.Guild.CTFPoints += 3;
                    }
                }
            }
            #endregion
            #region DieGuildMessage
            if (killer.PlayerFlag == PlayerFlag.Player && PlayerFlag == PlayerFlag.Player)
            {
                if (Owner.Guild != null && killer.Owner.Guild != null && killer.GuildID != GuildID && !Constants.PKFreeMaps.Contains(MapID))
                {
                    if (Owner.Guild.pkp_donation >= 3)
                    {
                        Owner.Guild.pkp_donation -= 3;
                    }
                    killer.Owner.Guild.pkp_donation += 3;
                    killer.Owner.Send(("You killed the enemy Guild " + Owner.Guild.Name + "`s " + Owner.AsMember.Rank + " " + Owner.AsMember.Name + " and received 0 EXP and 3 PK Donation!"));
                    Owner.Send(("You were killed by the enemy Guild " + killer.Owner.Guild.Name + "`s " + killer.Owner.AsMember.Rank + " " + killer.Owner.AsMember.Name + " and lost 0 EXP and 3 PK Donation!"));
                }
            }
            #endregion
            if (PlayerFlag == PlayerFlag.Player)
            {
                Owner.XPCount = 0;
                if (Owner.Booth != null)
                {
                    Owner.Booth.Remove();
                    Owner.Booth = null;
                }
            }
            //if (PKPoints > 30)
            //{
            //    DropRandomItemOnPKDeath(killer);
            //}
            if (killer.CountKilling >= 100 && killer.HeavenBlessing > 0 && killer.BlessedHunting < 4578)
            {
                killer.CountKilling = 0;
                killer.BlessedHunting += 1;
            }
            else if (killer.BlessedHunting >= 4578)
            {
                killer.Owner.MessageBox("You have reached the maximum amount of EXP available from bonus hinting. Would you like to claim it, now?",
                (p) =>
                {
                    killer.Owner.IncreaseExperience((ulong)(killer.BlessedHunting / 600.0 * killer.Owner.ExpBall), false);
                    killer.BlessedHunting = 0;
                    MsgGodExp Premio = new MsgGodExp();
                    Premio.Action = MsgGodExp.Mode.Show;
                    Premio.OnlineTraining = killer.OnlineTraining;
                    Premio.BlessedHunting = killer.BlessedHunting;
                    killer.Owner.Send(Premio.ToArray());
                });
            }
            killer.CountKilling++;
            killer.KillCount++;
            killer.KillCount2++;
            Killer = killer;
            Hitpoints = 0;
            DeathStamp = Time32.Now;
            ToxicFogLeft = 0;
            if (Companion)
            {
                if (Hitpoints < 1)
                {
                    Hitpoints = 0;
                    AddFlag((ulong)PacketFlag.Flags.Ghost | (ulong)PacketFlag.Flags.Dead | (ulong)PacketFlag.Flags.FadeAway);
                    MsgInteract Interact = new MsgInteract(true);
                    Interact.Attacked = UID;
                    Interact.InteractType = Network.GamePackets.MsgInteract.Kill;
                    Interact.X = X;
                    Interact.Y = Y;
                    MonsterInfo.SendScreen(Interact);
                    Owner.Map.RemoveEntity(this);
                }
                if (MyClones.Count != 0)
                {
                    foreach (var clone in MyClones)
                        clone.RemoveThat();
                    MyClones.Clear();
                }
                if (Owner.Companion != null)
                {
                    Owner.Map.RemoveEntity(Owner.Companion);
                    var data = new MsgAction(true);
                    data.UID = Owner.Companion.UID;
                    data.ID = PacketMsgAction.Mode.RemoveEntity;
                    Owner.Companion.MonsterInfo.SendScreen(data);
                    Owner.Companion = null;
                }
            }
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (killer.PlayerFlag == PlayerFlag.Player)
                {
                    if (Owner.Spells.ContainsKey(12660))
                    {
                        XPCountTwist = Owner.XPCount;
                    }
                    #region Kill em Eventos
                    #region Mate Por CPs
                    if (Constants.EventosREVO.Contains(killer.MapID))
                    {
                        // Stamina - HP - CPs //
                        killer.Stamina += 100;
                        killer.Hitpoints += 100000;
                        killer.ConquerPoints += 100;

                        // Enviando a mensagem GLOBAL do Kill do Player X no Player Y no Evento
                        Kernel.SendWorldMessage(new MsgTalk(
                            killer.Name + " eliminou " + Owner.Player.Name + " no evento Mate Por CPs e recebeu 100 CPs",
                            System.Drawing.Color.Yellow,
                            2000
                        ));

                        // Enviando a mensgem para o Player X (Killer)
                        var msg = new MsgTalk(
                            killer.Name + " seu HP/Stamina foram restaurados por fazer a kill no evento!",
                            System.Drawing.Color.YellowGreen,
                            MsgTalk.System
                        );

                        // Convertendo para Array
                        killer.Send(msg.ToArray());

                        goto Over;
                    }
                    #endregion
                    #endregion
                    #region PKFreeMaps
                    if (Constants.PKFreeMaps.Contains(killer.MapID))
                        goto Over;
                    if (killer.Owner.Map.BaseID == 700)
                        goto Over;
                    #endregion
                    #region Kongfu
                    if (killer.PKMode == PKMode.Kongfu && killer.KongfuActive == true)
                    {
                        if (PKMode == PKMode.Kongfu || KongfuActive == true)
                        {
                            if (killer.MyKongFu != null && MyKongFu != null)
                            {
                                if ((killer.MyKongFu.Talent >= 1 && killer.MyKongFu.Talent <= 5) && (MyKongFu.Talent >= 1 && MyKongFu.Talent <= 5))
                                {
                                    #region killed > killer
                                    if (MyKongFu.Talent > killer.MyKongFu.Talent)
                                    {
                                        if (MyKongFu.Talent >= 2)
                                        {
                                            MyKongFu.Talent -= 1;
                                        }
                                        if (killer.MyKongFu.Talent == 5)
                                        {
                                            //return;
                                        }
                                        else
                                        {
                                            killer.MyKongFu.Talent += 1;
                                        }
                                        Killer.MyKongFu.FreeCourse += 10000;
                                    }
                                    #endregion
                                    #region killed < killer
                                    else if (MyKongFu.Talent < killer.MyKongFu.Talent)
                                    {
                                        if (killer.MyKongFu.Talent == 5)
                                        {
                                            //return;
                                        }
                                        Killer.MyKongFu.FreeCourse += 10000;
                                    }
                                    #endregion
                                    #region killed = killer
                                    else if (MyKongFu.Talent == killer.MyKongFu.Talent)
                                    {
                                        if (MyKongFu.Talent > 1)
                                        {
                                            MyKongFu.Talent -= 1;
                                            if (killer.MyKongFu.Talent == 5)
                                            {
                                                //return;
                                            }
                                            else
                                            {
                                                killer.MyKongFu.Talent += 1;
                                            }
                                        }
                                        else if (MyKongFu.Talent == 1)
                                        {
                                            killer.MyKongFu.Talent += 1;
                                        }
                                        Killer.MyKongFu.FreeCourse += 10000;
                                    }
                                    #endregion
                                    #region TalentStatus
                                    #region Killer Status
                                    Killer.TalentStaus = Killer.MyKongFu.Talent;
                                    Killer.KongfuActive = Killer.MyKongFu.OnJiangMode;
                                    Writer.WriteByte(Killer.TalentStaus, PlayerSpawnPacket.KongfuTalen, Killer.SpawnPacket);
                                    MyKongFu.SendInfo(Killer.Owner, 7, new string[] { Killer.UID.ToString(), Killer.TalentStaus.ToString(), Killer.MyKongFu.OnJiangMode ? "1" : "2" });
                                    MyKongFu.SendInfo(Killer.Owner, 5, Killer.ToString(), Killer.MyKongFu.Talent.ToString());
                                    Killer.Owner.SendScreen(Killer.SpawnPacket, false);
                                    #endregion
                                    #region Killed Status
                                    TalentStaus = MyKongFu.Talent;
                                    KongfuActive = MyKongFu.OnJiangMode;
                                    Writer.WriteByte(TalentStaus, PlayerSpawnPacket.KongfuTalen, SpawnPacket);
                                    MyKongFu.SendInfo(Owner, 7, new string[] { UID.ToString(), TalentStaus.ToString(), MyKongFu.OnJiangMode ? "1" : "2" });
                                    MyKongFu.SendInfo(Owner, 5, ToString(), MyKongFu.Talent.ToString());
                                    Owner.SendScreen(SpawnPacket, false);
                                    #endregion
                                    DB.KongFuTable.SaveKongFu();
                                    #endregion
                                    goto Over;
                                }
                            }
                        }
                    }
                    #endregion
                    if (((killer.PKMode != PKMode.Kongfu) && !ContainsFlag((ulong)PacketFlag.Flags.FlashingName)) && !ContainsFlag((ulong)PacketFlag.Flags.BlackName))
                    {
                        killer.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                        killer.FlashingNameStamp = Time32.Now;
                        killer.FlashingNameTime = 60;
                        if (killer.GuildID != 0)
                        {
                            if (killer.Owner.Guild.Enemy.ContainsKey(GuildID))
                            {
                                killer.PKPoints += 3;
                            }
                            else
                            {
                                if (!killer.Owner.Enemy.ContainsKey(UID))
                                    killer.PKPoints += 10;
                                else killer.PKPoints += 5;
                            }
                        }
                        else
                        {
                            if (!killer.Owner.Enemy.ContainsKey(UID))
                                killer.PKPoints += 10;
                            else killer.PKPoints += 5;
                        }
                        if (killer.PlayerFlag == PlayerFlag.Player)
                        {
                            if (PlayerFlag == PlayerFlag.Player)
                            {
                                MsgPkStatistic pk = new MsgPkStatistic();
                                if (!killer.PkStatistic.ContainsKey(UID))
                                {
                                    pk.UID = killer.UID;
                                    pk.killedUID = UID;
                                    pk.Name = Name;
                                    pk.Map = MapID;
                                    pk.Killing = 1;
                                    pk.BattlePower = (uint)BattlePower;
                                    pk.Time = DateTime.Now;
                                    PkStatisticTable.Insert(killer.Owner, pk);
                                }
                                else
                                {
                                    pk.UID = killer.UID;
                                    pk.killedUID = UID;
                                    pk.Name = Name;
                                    killer.PkStatistic[UID].Map = MapID;
                                    pk.Map = killer.PkStatistic[UID].Map;
                                    killer.PkStatistic[UID].Killing += 1;
                                    pk.Killing = killer.PkStatistic[UID].Killing;
                                    killer.PkStatistic[UID].BattlePower = (uint)BattlePower;
                                    pk.BattlePower = killer.PkStatistic[UID].BattlePower;
                                    pk.Time = DateTime.Now;
                                    PkStatisticTable.Update(killer.Owner, pk);
                                }
                                if (killer.HeavenBlessing == 0 && HeavenBlessing != 0)
                                {
                                    killer.CursedTime += 300;
                                    SendCursed(killer.CursedTime, killer.Owner);
                                    Owner.Send("Your heaven Blessing takes effect! you lose no EXP.");
                                }
                                if (ExpProtectionTime > 600)
                                {
                                    ExpProtectionTime -= 600;
                                    MsgUpdate update = new MsgUpdate(true);
                                    update.UID = UID;
                                    update.Append((byte)PacketFlag.DataType.ExpProtection, 0, ExpProtectionTime, 1, 0);
                                    Owner.Send(update.ToArray());
                                    killer.Owner.Inventory.Add(3002560, 0, 1);
                                    Owner.Send("Your heaven Protection takes effect! you lose no EXP.");
                                }
                                if (InHangUp)
                                {
                                    MsgHangUp AutoHunt = new MsgHangUp();
                                    AutoHunt.Action = MsgHangUp.Mode.KilledBy;
                                    AutoHunt.Unknown = 3329;
                                    AutoHunt.KilledName = killer.Name;
                                    AutoHunt.EXPGained = HangUpEXP;
                                    Owner.Send(AutoHunt.ToArray());
                                    AutoRevStamp = Time32.Now;
                                    AutoRev = 20;
                                }
                            }
                        }
                    }
                    MsgFriend.AddEnemys(Owner, killer.Owner);
                }
                uint ran = (uint)Kernel.Random.Next(1, 10);
                if (killer.PlayerFlag == PlayerFlag.Player)
                {
                    if (ran > 5)
                    {
                        DropRandomStuff(Killer);
                        killer.Owner.Send("If you have any problem in your item, relogin");
                    }
                }
            }
            RemoveFlag((ulong)PacketFlag.Flags.FlashingName);
            Over:
            MsgInteract attack = new MsgInteract(true);
            attack.Attacker = killer.UID;
            attack.Attacked = UID;
            attack.InteractType = Network.GamePackets.MsgInteract.Kill;
            attack.X = X;
            attack.Y = Y;
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (Hitpoints < 1)
                {
                    Hitpoints = 0;
                    AddFlag((ulong)PacketFlag.Flags.Dead);
                    AddFlag((ulong)PacketFlag.Flags.Ghost);
                    RemoveFlag((ulong)PacketFlag.Flags.Fly);
                    RemoveFlag((ulong)PacketFlag.Flags.Ride);
                    RemoveFlag((ulong)PacketFlag.Flags.Cyclone);
                    RemoveFlag((ulong)PacketFlag.Flags.Superman);
                    RemoveFlag((ulong)PacketFlag.Flags.FatalStrike);
                    RemoveFlag((ulong)PacketFlag.Flags.FlashingName);
                    RemoveFlag((ulong)PacketFlag.Flags.ShurikenVortex);
                    RemoveFlag((ulong)PacketFlag.Flags.GodlyShield);
                    RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                    RemoveFlag((ulong)PacketFlag.Flags.Praying);
                    RemoveFlag2((ulong)PacketFlag.Flags.Oblivion);
                    RemoveFlag3((ulong)PacketFlag.Flags.SuperCyclone);
                    RemoveFlag3((ulong)PacketFlag.Flags.DragonCyclone);

                    RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer);
                    RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer2);
                    RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer3);
                    RemoveFlag3((ulong)PacketFlag.Flags.FlameLayer4);
                    RemoveFlag3((ulong)PacketFlag.Flags.BackFire);
                    RemoveFlag3((ulong)PacketFlag.Flags.ManiacDance);
                    RemoveFlag2((ulong)PacketFlag.Flags.EarthAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.FireAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.WaterAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.WoodAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.MetalAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.FendAuraIcon);
                    RemoveFlag2((ulong)PacketFlag.Flags.TyrantAuraIcon);

                    RemoveFlag2((ulong)PacketFlag.Flags.EarthAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.FireAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.WaterAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.WoodAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.MetalAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.FendAura);
                    RemoveFlag2((ulong)PacketFlag.Flags.TyrantAura);

                    Owner.removeAuraBonuses(Aura_actType, Aura_actPower, 1);
                    Aura_isActive = false;
                    Aura_actType = 0;
                    Aura_actPower = 0;

                    RemoveFlag2((ulong)PacketFlag.Flags.AzureShield);
                    MagicDefender = 0;
                    BlockShield = 0;
                    SpiritFocus = false;
                }
                if (Body % 10 < 3)
                    TransformationID = 99;
                else
                    TransformationID = 98;

                if (ContainsFlag3((ulong)PacketFlag.Flags.AuroraLotus))
                {
                    AuroraLotusEnergy = 0;
                    Lotus(AuroraLotusEnergy, (byte)PacketFlag.DataType.AuroraLotus);
                }
                if (ContainsFlag3((ulong)PacketFlag.Flags.FlameLotus))
                {
                    FlameLotusEnergy = 0;
                    Lotus(FlameLotusEnergy, (byte)PacketFlag.DataType.FlameLotus);
                }
                Owner.SendScreen(attack, true);
                Owner.Send(new MsgMapInfo()
                {
                    BaseID = Owner.Map.BaseID,
                    ID = Owner.Map.ID,
                    Status = MapsTable.MapInformations[Owner.Map.ID].Status,
                    Weather = MapsTable.MapInformations[Owner.Map.ID].Weather
                });
                Owner.EndQualifier();
            }
            else
            {
                if (!Companion && !IsDropped)
                    MonsterInfo.Drop(killer);
                Kernel.Maps[MapID].Floor[X, Y, MapObjType, this] = true;
                if (killer.PlayerFlag == PlayerFlag.Player)
                {
                    killer.Owner.IncreaseExperience(MaxHitpoints, true);
                    if (killer.Owner.Team != null)
                    {
                        foreach (GameState teammate in killer.Owner.Team.Teammates)
                        {
                            if (Kernel.GetDistance(killer.X, killer.Y, teammate.Player.X, teammate.Player.Y) <= InfoFile.pScreenDistance)
                            {
                                if (killer.UID != teammate.Player.UID)
                                {
                                    uint extraExperience = MaxHitpoints / 2;
                                    if (killer.Spouse == teammate.Player.Name)
                                        extraExperience = MaxHitpoints * 2;
                                    byte TLevelN = teammate.Player.Level;
                                    if (killer.Owner.Team.CanGetNoobExperience(teammate))
                                    {
                                        if (teammate.Player.Level < 140)
                                        {
                                            if (MonsterInfo.ID == teammate.Player.kilid)
                                            {
                                                if (teammate.Inventory.Contains(750000, 1))
                                                {
                                                    teammate.Player.Status4 += 1;
                                                }
                                            }
                                            extraExperience *= 2;
                                            teammate.IncreaseExperience(extraExperience, false);
                                            teammate.Send(DefineConstantsEn_Res.NoobTeamExperience(extraExperience));
                                        }
                                    }
                                    else if (teammate.Player.Level < 137)
                                    {
                                        if (MonsterInfo.ID == teammate.Player.kilid)
                                        {
                                            if (teammate.Inventory.Contains(750000, 1))
                                            {
                                                teammate.Player.Status4 += 1;
                                            }
                                        }
                                        teammate.IncreaseExperience(extraExperience, false);
                                        teammate.Send(DefineConstantsEn_Res.TeamExperience(extraExperience));
                                    }
                                    byte TLevelNn = teammate.Player.Level;
                                    byte newLevel = (byte)(TLevelNn - TLevelN);
                                    if (newLevel != 0)
                                    {
                                        if (TLevelN < 70)
                                        {
                                            for (int i = TLevelN; i < TLevelNn; i++)
                                            {
                                                teammate.Team.SendMessage(new MsgTalk("The leader, " + teammate.Team.Teammates[0].Player.Name + ", has gained " + (uint)(i * 7.7F) + " virtue points for power leveling the rookies.", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.World));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (killer.Level < 140)// Leveo 0
                    {
                        uint extraExp = MaxHitpoints;
                        extraExp *= InfoFile.ExtraExperienceRate;
                        extraExp += (uint)(extraExp * killer.Gems[3] / 100);
                        extraExp += (uint)(extraExp * ((float)killer.BattlePower / 100));
                        if (killer.DoubleExperienceTime > 0)
                            extraExp *= 2;
                        if (killer.HeavenBlessing > 0)
                            extraExp += (uint)(extraExp * 20 / 100);
                        if (killer.Reborn >= 2)
                            extraExp /= 3;
                        killer.Owner.Send(DefineConstantsEn_Res.ExtraExperience(extraExp));
                    }
                    killer.Owner.XPCount++;
                    if (killer.OnKOSpell())
                        killer.KOSpellTime++;
                }
            }
            if (PlayerFlag == PlayerFlag.Player)
                if (OnDeath != null) OnDeath(this);
        }
        public void RemoveMagicDefender()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (MagicDefenderOwner && HasMagicDefender)
                {
                    if (Owner.Team != null && HasMagicDefender && MagicDefenderOwner)
                    {
                        foreach (var mate in Owner.Team.Teammates)
                        {
                            mate.Player.HasMagicDefender = false;
                            mate.Player.MagicDefenderSecs = 0;
                            mate.Player.RemoveFlag3((ulong)PacketFlag.Flags.MagicDefender);
                            mate.Player.RemoveFlag2((ulong)PacketFlag.Flags.MagicDefenderIcon);
                            MsgUpdate upgrade = new MsgUpdate(true);
                            upgrade.UID = mate.Player.UID;
                            upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, 0, 0, 0);
                            mate.Send(upgrade.ToArray());
                        }
                    }
                    MagicDefenderOwner = false;
                }
                RemoveFlag3((ulong)PacketFlag.Flags.MagicDefender);
                RemoveFlag2((ulong)PacketFlag.Flags.MagicDefenderIcon);
                MsgUpdate upgrad = new MsgUpdate(true);
                upgrad.UID = UID;
                upgrad.Append((byte)PacketFlag.DataType.AzureShield, 128, 0, 0, 0);
                Owner.Send(upgrad.ToArray());
                HasMagicDefender = false;
            }
        }
        public void Update(byte type, uint value, uint secondvalue)
        {
            MsgUpdate upd = new MsgUpdate(true);
            upd.Append(type, value);
            upd.Append(type, secondvalue);
            upd.UID = UID;
            Owner.Send(upd);
        }
        public void Update(byte type, byte value, bool screen)
        {
            if (!SendUpdates) return;
            if (this.Owner == null) return;
            update = new MsgUpdate(true);
            update.UID = UID;
            update.Append(type, value, (byte)UpdateOffset1, (byte)UpdateOffset2, (byte)UpdateOffset3, (byte)UpdateOffset4, (byte)UpdateOffset5, (byte)UpdateOffset6, (byte)UpdateOffset7);
            if (!screen)
                update.Send(Owner);
            else Owner.SendScreen(update, true);
        }
        public void Update(byte type, ushort value, bool screen)
        {
            if (!SendUpdates) return;
            update = new MsgUpdate(true);
            update.UID = UID;
            update.Append(type, value);
            if (!screen)
                update.Send(Owner as GameState);
            else (Owner as GameState).SendScreen(update, true);
        }
        public void Update(byte type, uint value, bool screen)
        {
            if (!SendUpdates) return;
            update = new MsgUpdate(true);
            update.UID = UID;
            update.Append(type, value);
            if (!screen)
                update.Send(Owner as GameState);
            else (Owner as GameState).SendScreen(update, true);
        }
        public void Update(byte type, ulong value, bool screen)
        {
            if (!SendUpdates) return;
            update = new MsgUpdate(true);
            update.UID = UID;
            update.Append(type, value);
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else (Owner as Client.GameState).SendScreen(update, true);
            }
            else MonsterInfo.SendScreen(update);
        }
        public void Update(MsgName.Mode Action, string value, bool screen)
        {
            if (!SendUpdates) return;
            MsgName update = new MsgName(true);
            update.UID = this.UID;
            update.Action = Action;
            update.TextsCount = 1;
            update.Texts.Add(value);
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as GameState);
                else (Owner as GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }
        public void Update(MsgName.Mode Action, string value, uint UIDs, bool screen)
        {
            if (!SendUpdates) return;
            MsgName update = new MsgName(true);
            update.UID = UIDs;
            update.Action = Action;
            update.TextsCount = 1;
            update.Texts.Add(value);
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as Client.GameState);
                else (Owner as Client.GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }
        public void UpdateEffects(bool screen)
        {
            if (!SendUpdates) return;
            update = new MsgUpdate(true);
            update.UID = UID;
            update.Append((byte)PacketFlag.DataType.StatusFlag, StatusFlag, StatusFlag2, StatusFlag3, StatusFlag4);
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!screen)
                    update.Send(Owner as GameState);
                else
                    (Owner as GameState).SendScreen(update, true);
            }
            else
            {
                MonsterInfo.SendScreen(update);
            }
        }
        private void UpdateDB(string column, byte value)
        {
            if (PlayerFlag == PlayerFlag.Player)
                if (FullyLoaded)
                    new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", UID).Execute();

        }
        private void UpdateDB(string column, long value)
        {
            if (PlayerFlag == PlayerFlag.Player)
                if (FullyLoaded)
                    new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", UID).Execute();

        }
        public void UpdateDB(string column, ulong value)
        {
            if (PlayerFlag == PlayerFlag.Player)
                if (FullyLoaded)
                    new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", UID).Execute();

        }
        public void UpdateDB(string column, bool value)
        {
            if (PlayerFlag == PlayerFlag.Player)
                if (FullyLoaded)
                    new MySqlCommand(MySqlCommandType.UPDATE).Update("entities").Set(column, value).Where("UID", UID).Execute();
        }
        public static sbyte[] XDir = new sbyte[] { 0, -1, -1, -1, 0, 1, 1, 1 };
        public static sbyte[] YDir = new sbyte[] { 1, 1, 0, -1, -1, -1, 0, 1 };
        public static sbyte[] XDir2 = new sbyte[] { 0, -2, -2, -2, 0, 2, 2, 2, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1 };
        public bool Move(Enums.ConquerAngle Direction, int teleport = 1)
        {
            ushort _X = X, _Y = Y;
            Facing = Direction;
            int dir = ((int)Direction) % XDir.Length;
            sbyte xi = XDir[dir], yi = YDir[dir];
            _X = (ushort)(X + xi);
            _Y = (ushort)(Y + yi);
            Game.Map Map = null;
            if (Kernel.Maps.TryGetValue(MapID, out Map))
            {
                var objType = MapObjType;
                if (Map.Floor[_X, _Y, objType])
                {
                    if (objType == MapObjectType.Monster)
                    {
                        Map.Floor[_X, _Y, MapObjType] = false;
                        Map.Floor[X, Y, MapObjType] = true;
                    }
                    X = _X;
                    Y = _Y;
                    return true;
                }
                else
                {
                    if (Mode == Enums.Mode.None)
                        if (PlayerFlag != PlayerFlag.Monster)
                            if (teleport == 1)
                                Teleport(MapID, X, Y);
                    return false;
                }
            }
            else
            {
                if (PlayerFlag != PlayerFlag.Monster)
                    Teleport(MapID, X, Y);
                else
                    return false;
            }
            return true;
        }
        public bool Move(Enums.ConquerAngle Direction, bool slide)
        {
            ushort _X = X, _Y = Y;
            if (!slide)
                return Move((Enums.ConquerAngle)((byte)Direction % 8));

            int dir = ((int)Direction) % XDir2.Length;
            Facing = Direction;
            sbyte xi = XDir2[dir], yi = YDir2[dir];
            _X = (ushort)(X + xi);
            _Y = (ushort)(Y + yi);

            Game.Map Map = null;

            if (Kernel.Maps.TryGetValue(MapID, out Map))
            {
                if (Map.Floor[_X, _Y, MapObjType])
                {
                    if (MapObjType == MapObjectType.Monster)
                    {
                        Map.Floor[_X, _Y, MapObjType] = false;
                        Map.Floor[X, Y, MapObjType] = true;
                    }
                    X = _X;
                    Y = _Y;
                    return true;
                }
                else
                {
                    if (Mode == Enums.Mode.None)
                    {
                        if (PlayerFlag != PlayerFlag.Monster)
                            Teleport(MapID, X, Y);
                        else
                            return false;
                    }
                }
            }
            else
            {
                if (PlayerFlag != PlayerFlag.Monster)
                    Teleport(MapID, X, Y);
                else
                    return false;
            }
            return true;
        }
        public static sbyte[] YDir2 = new sbyte[] { 2, 2, 0, -2, -2, -2, 0, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2 };
        //public bool Move(Enums.ConquerAngle Direction, int teleport = 1)
        //{
        //    ushort _X = X, _Y = Y;
        //    Facing = Direction;
        //    int dir = ((int)Direction) % XDir.Length;
        //    sbyte xi = XDir[dir], yi = YDir[dir];
        //    _X = (ushort)(X + xi);
        //    _Y = (ushort)(Y + yi);
        //    Game.Map Map = null;
        //    if (Kernel.Maps.TryGetValue(MapID, out Map))
        //    {
        //        var objType = MapObjType;
        //        if (Map.Floor[_X, _Y, objType])
        //        {
        //            if (objType == MapObjectType.Monster)
        //            {
        //                Map.Floor[_X, _Y, MapObjType] = false;
        //                Map.Floor[X, Y, MapObjType] = true;
        //            }
        //            X = _X;
        //            Y = _Y;
        //            return true;
        //        }
        //        else
        //        {
        //            if (Mode == Enums.Mode.None)
        //                if (PlayerFlag != PlayerFlag.Monster)
        //                    if (teleport == 1)
        //                        Teleport(MapID, X, Y);
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        if (PlayerFlag != PlayerFlag.Monster)
        //            Teleport(MapID, X, Y);
        //        else
        //            return false;
        //    }
        //    return true;
        //}
        //public bool Move(Enums.ConquerAngle Direction, bool slide)
        //{
        //    #region Remove MagicDefender
        //    if (PlayerFlag == PlayerFlag.Player)
        //    {
        //        if (Owner.Spells.ContainsKey(11200) && SpellTable.SpellInformations.ContainsKey(11200))
        //        {
        //            if (ContainsFlag3((ulong)PacketFlag.Flags.MagicDefender))
        //            {
        //                if (Time32.Now <= this.MagicDefenderStamp.AddSeconds(this.MagicDefender))
        //                {
        //                    RemoveFlag3((ulong)PacketFlag.Flags.MagicDefender);
        //                    RemoveFlag2((ulong)PacketFlag.Flags.MagicDefenderIcon);
        //                    this.MagicDefender = 0;
        //                }
        //            }
        //        }
        //    }
        //    #endregion
        //    ushort _X = X, _Y = Y;
        //    if (!slide)
        //        return Move((Enums.ConquerAngle)((byte)Direction % 8));
        //    int dir = ((int)Direction) % XDir2.Length;
        //    Facing = Direction;
        //    sbyte xi = XDir2[dir], yi = YDir2[dir];
        //    _X = (ushort)(X + xi);
        //    _Y = (ushort)(Y + yi);
        //    Map Map = null;
        //    if (Kernel.Maps.TryGetValue(MapID, out Map))
        //    {
        //        if (Map.Floor[_X, _Y, MapObjType])
        //        {
        //            if (MapObjType == MapObjectType.Monster)
        //            {
        //                Map.Floor[_X, _Y, MapObjType] = false;
        //                Map.Floor[X, Y, MapObjType] = true;
        //            }
        //            X = _X;
        //            Y = _Y;
        //            return true;
        //        }
        //        else
        //        {
        //            if (Mode == Enums.Mode.None)
        //            {
        //                if (PlayerFlag != PlayerFlag.Monster)
        //                    Teleport(MapID, X, Y);
        //                else
        //                    return false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (PlayerFlag != PlayerFlag.Monster)
        //            Teleport(MapID, X, Y);
        //        else
        //            return false;
        //    }
        //    return true;
        //}
        public void SendSpawn(Client.GameState client)
        {
            SendSpawn(client, true);
        }
        public void SendSpawn(Client.GameState client, bool checkScreen = true)
        {
            if (!this.Invisable)
            {
                if (client.Screen.Add(this) || !checkScreen)
                {
                    if (PlayerFlag == PlayerFlag.Player)
                    {
                        if (GmHide)
                            return;

                        if (client.InQualifier() && this.Owner.IsWatching()
                            || (this.SkillTeamWatchingElitePKMatch != null
                            || this.Owner.WatchingElitePKMatch != null)
                            || this.Invisable) return;
                        if (Owner.WatchingElitePKMatch != null) return;
                        if (Invisable) return;
                        if (this.Owner.IsFairy)
                        {
                            MsgSuitStatus FS = new MsgSuitStatus(true);
                            FS.Action = (MsgSuitStatus.Mode)this.Owner.SType;
                            FS.FairyAction = this.Owner.FairyType;
                            FS.UID = this.UID;
                            client.Send(FS.ToArray());
                        }
                        MsgAction generalData = new MsgAction(true);
                        generalData.UID = client.Player.UID;
                        generalData.ID = PacketMsgAction.Mode.AppearanceType;
                        generalData.dwParam = (uint)client.Player.Appearance;
                        client.SendScreen(generalData, true);
                        if (cantdie)
                            return;
                        client.Send(SpawnPacket);
                    }
                    else
                    {
                        client.Send(SpawnPacket);
                    }
                    if (PlayerFlag == PlayerFlag.Player)
                    {
                        if (Away == 1)
                        {
                            byte[] buffer = new byte[50];
                            Writer.WriteUInt16(42, 0, buffer);
                            Writer.WriteUInt16(0x271A, 2, buffer);
                            Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 4, buffer);
                            Writer.WriteUInt32(UID, 8, buffer);
                            Writer.WriteByte(1, 12, buffer);
                            Writer.WriteUInt32((uint)Time32.timeGetTime().GetHashCode(), 20, buffer);
                            Writer.WriteByte(0xA1, 24, buffer);
                            client.SendScreen(buffer, false);
                        }
                        if (Owner.Booth != null)
                        {
                            client.Send(Owner.Booth);
                            if (Owner.Booth.HawkMessage != null)
                                client.Send(Owner.Booth.HawkMessage);
                        }
                        if (/*TransformationID != 0 &&*/ TransformationID != 98)
                        {
                            if (!SendUpdates) return;
                            update = new MsgUpdate(true);
                            update.UID = UID;
                            update.Append((byte)PacketFlag.DataType.MaxHitpoints, MaxHitpoints);
                            Owner.SendScreen(update, false);

                        }
                    }
                }
            }
        }
        public void Shift(ushort X, ushort Y, uint mapID, Interfaces.IPacket shift = null)
        {
            if (_mapid != mapID) return;
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!DB.MapsTable.MapInformations.ContainsKey(MapID)) return;
            }
            this.X = X;
            this.Y = Y;
            if (shift == null)
            {
                shift = new MsgAction(true)
                {
                    UID = UID,
                    ID = PacketMsgAction.Mode.FlashStep,
                    dwParam = MapID,
                    X = X,
                    Y = Y
                };
            }
            if (PlayerFlag == PlayerFlag.Player)
            {
                Owner.SendScreen(shift, true);
                Owner.Screen.Reload(shift);
            }
        }
        public void Shift(ushort X, ushort Y)
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!DB.MapsTable.MapInformations.ContainsKey(MapID))
                    return;
                this.X = X;
                this.Y = Y;
                MsgAction Data = new MsgAction(true);
                Data.UID = UID;
                Data.ID = PacketMsgAction.Mode.FlashStep;
                Data.dwParam = MapID;
                Data.X = X;
                Data.Y = Y;
                Owner.SendScreen(Data, true);
                Owner.Screen.Reload(null);
            }
        }
        public bool fMove(Enums.ConquerAngle Direction, ref ushort _X, ref ushort _Y)
        {
            Facing = Direction;
            sbyte xi = 0, yi = 0;
            switch (Direction)
            {
                case Enums.ConquerAngle.North: xi = -1; yi = -1; break;
                case Enums.ConquerAngle.South: xi = 1; yi = 1; break;
                case Enums.ConquerAngle.East: xi = 1; yi = -1; break;
                case Enums.ConquerAngle.West: xi = -1; yi = 1; break;
                case Enums.ConquerAngle.NorthWest: xi = -1; break;
                case Enums.ConquerAngle.SouthWest: yi = 1; break;
                case Enums.ConquerAngle.NorthEast: yi = -1; break;
                case Enums.ConquerAngle.SouthEast: xi = 1; break;
            }
            _X = (ushort)(_X + xi);
            _Y = (ushort)(_Y + yi);
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (Owner.Map.Floor[_X, _Y, MapObjType, null])
                    return true;
                else
                    return false;
            }
            else
            {
                Game.Map Map = null;
                if (Kernel.Maps.TryGetValue(MapID, out Map))
                {
                    if (Map.Floor[_X, _Y, MapObjType, null])
                        return true;
                    else
                        return false;
                }
                return true;
            }
        }
        public void Teleport(ushort X, ushort Y)
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (!DB.MapsTable.MapInformations.ContainsKey(MapID) && !Owner.InQualifier())
                {
                    MapID = 1002;
                    X = 300;
                    Y = 278;
                }
                if (Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
                    Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                if (Owner.Team != null)
                    Owner.Team.GetClanShareBp(Owner);
                this.X = X;
                this.Y = Y;
                MsgAction Data = new MsgAction(true);
                Data.UID = UID;
                Data.ID = PacketMsgAction.Mode.Teleport;
                Data.dwParam = DB.MapsTable.MapInformations[MapID].BaseID;
                Data.X = X;
                Data.Y = Y;
                Owner.Send(Data);
                Owner.Screen.Reload(Data);
                Owner.Screen.FullWipe();
                Owner.Screen.Reload(null);
                Owner.Player.MapRegion = Regions.FindRegion((uint)Owner.Map.BaseID, Owner.Player.X, Owner.Player.Y);
                Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = DB.MapsTable.MapInformations[Owner.Map.ID].Status, Weather = DB.MapsTable.MapInformations[Owner.Map.ID].Weather });
            }
        }
        public void SetLocation(ushort MapID, ushort X, ushort Y)
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                this.X = X;
                this.Y = Y;
                this.MapID = MapID;
            }
        }
        public void AdvancedTeleport(bool remove = false)
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (remove)
                    Owner.RemoveScreenSpawn(Owner.Player, false);
            }
        }
        public void Teleport(ushort MapID, ushort X, ushort Y)
        {
            if (this != null && Owner != null)
            {
                if (Owner.Companion != null)
                {
                    Owner.Map.RemoveEntity(Owner.Companion);
                    var data = new MsgAction(true);
                    data.UID = Owner.Companion.UID;
                    data.ID = PacketMsgAction.Mode.RemoveEntity;
                    Owner.Companion.MonsterInfo.SendScreen(data);
                    Owner.Companion = null;
                }
                #region AutoHunt
                if (Owner.Player.InHangUp)
                {
                    MsgHangUp AutoHunt = new MsgHangUp();
                    AutoHunt.Action = MsgHangUp.Mode.ChangedMap;
                    AutoHunt.EXPGained = Owner.Player.HangUpEXP;
                    Owner.Send(AutoHunt.ToArray());

                    AutoHunt.Action = MsgHangUp.Mode.EXPGained;
                    AutoHunt.EXPGained = Owner.Player.HangUpEXP;
                    Owner.Send(AutoHunt.ToArray());

                    Owner.Player.RemoveFlag3((ulong)PacketFlag.Flags.AutoHunting);
                    Owner.Player.HangUpEXP = 0;
                    Owner.Player.InHangUp = false;
                }
                #endregion
                /*#region ShadowClone
                if (MyClones.Count != 0)
                {
                    foreach (var clone in MyClones)
                        clone.RemoveThat();
                    MyClones.Clear();
                }
                #endregion*/
                if (PlayerFlag == PlayerFlag.Player)
                {
                    #region IfThePlayerInSameMapThatWillTeleportToIt
                    if (this.MapID == MapID)
                    {
                        if (!DMaps.LoadMap(MapID) && DB.MapsTable.MapInformations.ContainsKey(MapID) && !Owner.InQualifier())
                        {
                            MapID = 1002;
                            X = 300;
                            Y = 278;
                        }
                        this.X = X;
                        this.Y = Y;
                        MsgAction Data = new Network.GamePackets.MsgAction(true);
                        Data.UID = UID;
                        Data.ID = PacketMsgAction.Mode.Teleport;
                        Data.dwParam = DB.MapsTable.MapInformations[MapID].BaseID;
                        Data.X = X;
                        Data.Y = Y;
                        Owner.Send(Data);
                        Owner.Screen.FullWipe();
                        Owner.Screen.Reload(null);
                        Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = MapsTable.MapInformations[Owner.Map.ID].Status, Weather = MapsTable.MapInformations[Owner.Map.ID].Weather });
                        return;
                    }
                    #endregion
                    ushort baseID = 0;
                    if (!Kernel.Maps.ContainsKey(MapID) && !DB.DMaps.LoadMap(MapID))
                    {
                    }
                    else if (DB.DMaps.LoadMap(MapID))
                    {
                        baseID = Kernel.Maps[MapID].BaseID;
                    }
                    else return;
                    if (Owner.InQualifier())
                        if (MapID != 700 && MapID < 11000)
                            Owner.EndQualifier();
                    this.PrevX = this.X;
                    this.PrevY = this.Y;
                    this.X = X;
                    this.Y = Y;
                    this.PreviousMapID = this.MapID;
                    this.MapID = MapID;
                    MsgAction data = new MsgAction(true);
                    data.UID = UID;
                    data.ID = PacketMsgAction.Mode.Teleport;
                    data.dwParam = baseID;
                    data.X = X;
                    data.Y = Y;
                    Owner.Send(data);
                    Owner.Screen.FullWipe();
                    Owner.Send(new MsgMapInfo() { BaseID = Owner.Map.BaseID, ID = Owner.Map.ID, Status = MapsTable.MapInformations[Owner.Map.ID].Status, Weather = MapsTable.MapInformations[Owner.Map.ID].Weather });
                    Owner.Player.MapRegion = Regions.FindRegion((uint)Owner.Map.BaseID, Owner.Player.X, Owner.Player.Y);
                    Owner.Player.Action = Enums.ConquerAction.None;
                    Owner.ReviveStamp = Time32.Now;
                    Owner.Attackable = false;
                    Owner.Screen.Reload(null);
                    if (!Owner.Equipment.Free(12))
                            RemoveFlag((ulong)PacketFlag.Flags.Ride);
                    AdvancedTeleport(true);
                }
            }

        }
        public ushort PrevX, PrevY;
        public void Teleport(ushort BaseID, ushort DynamicID, ushort X, ushort Y)
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
                    Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                if (!DMaps.MapPaths.ContainsKey(BaseID)) return;
                if (Owner.InQualifier())
                    if (BaseID != 700 && BaseID < 11000)
                        Owner.EndQualifier();
                if (!Kernel.Maps.ContainsKey(DynamicID)) new Map(DynamicID, BaseID, DMaps.MapPaths[BaseID]);
                PrevX = this.X;
                PrevY = this.Y;
                this.X = X;
                this.Y = Y;
                PreviousMapID = MapID;
                MapID = DynamicID;
                MsgAction Data = new MsgAction(true);
                Data.UID = UID;
                Data.ID = PacketMsgAction.Mode.Teleport;
                Data.dwParam = BaseID;
                Data.X = X;
                Data.Y = Y;
                Owner.Send(Data);
                Owner.Screen.Reload(Data);
                Owner.Screen.FullWipe();
                Owner.Screen.Reload(null);
                Owner.Player.Action = Enums.ConquerAction.None;
                Owner.ReviveStamp = Time32.Now;
                Owner.Attackable = false;
                Owner.Player.MapRegion = Regions.FindRegion((uint)Owner.Map.BaseID, Owner.Player.X, Owner.Player.Y);
                if (!Owner.Equipment.Free(12))
                        RemoveFlag((ulong)PacketFlag.Flags.Ride);
            }
        }
        public bool OnBladeFlurry()
        {
            return ContainsFlag((ulong)PacketFlag.Flags.BladeFlurry);
        }
        public bool OnKOSpell()
        {
            return OnCyclone() || OnSuperman() || OnSuperCyclone() || OnDragonCyclone() || OnManiacDance();
        }
        public bool OnBackFire()
        {
            return ContainsFlag3((ulong)PacketFlag.Flags.BackFire);
        }
        public bool OnSuperCyclone()
        {
            return ContainsFlag3((ulong)PacketFlag.Flags.SuperCyclone);
        }
        public bool OnManiacDance()
        {
            return ContainsFlag3((ulong)PacketFlag.Flags.ManiacDance);
        }
        public bool OnDragonCyclone()
        {
            return ContainsFlag3((ulong)PacketFlag.Flags.DragonCyclone);
        }
        public bool OnOblivion()
        {
            return ContainsFlag2((ulong)PacketFlag.Flags.Oblivion);
        }
        public bool OnCyclone()
        {
            return ContainsFlag((ulong)PacketFlag.Flags.Cyclone);
        }
        public bool OnSuperman()
        {
            return ContainsFlag((ulong)PacketFlag.Flags.Superman);
        }
        public bool OnFatalStrike()
        {
            return ContainsFlag((ulong)PacketFlag.Flags.FatalStrike);
        }
        public bool OnScurvyBomb()
        {
            return ContainsFlag2((ulong)PacketFlag.Flags.ScurvyBomb);
        }
        public void Untransform()
        {
            if (MapID == 1036 && TransformationTime == 3600) return;
            this.TransformationID = 0;
            double maxHP = TransformationMaxHP;
            double HP = Hitpoints;
            double point = HP / maxHP;
            Hitpoints = (uint)(MaxHitpoints * point);
            Update((byte)PacketFlag.DataType.MaxHitpoints, MaxHitpoints, false);
        }
        public byte[] WindowSpawn()
        {
            byte[] buffer = new byte[SpawnPacket.Length];
            SpawnPacket.CopyTo(buffer, 0);
            buffer[PlayerSpawnPacket.WindowSpawn] = 1;
            return buffer;
        }
        #endregion
        #region New Work
        public uint FlameLotusEnergy;
        public uint AuroraLotusEnergy;
        public Time32 LotusEnergyStamp;
        public Time32 FlameLayerStamp;
        public float FlameLayerPercent;
        public int FlameLayerLeft;
        public bool EpicMonk()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                if (Class >= 60 && Class <= 65)
                {
                    var weapons = Owner.Weapons;
                    if (weapons.Item1 != null)
                        if (weapons.Item1.ID / 1000 == 622)
                            if (weapons.Item2 != null)
                                if (weapons.Item2.ID / 1000 == 622)
                                    return true;
                }
            }
            return false;
        }
        public bool Lotus(uint LotusEnergy, uint aura = (byte)PacketFlag.DataType.AuroraLotus)
        {
            if (Owner.Weapons != null)
            {
                if (Owner.Weapons.Item1 != null)
                {
                    if (Owner.Weapons.Item1.ID / 1000 != 620) return false;
                    MsgUpdate upgrade = new MsgUpdate(true);
                    upgrade.UID = UID;
                    upgrade.Append((byte)PacketFlag.DataType.StatusFlag, StatusFlag, StatusFlag2, StatusFlag3, StatusFlag4);
                    upgrade.Append((byte)PacketFlag.DataType.Lotus, aura, 5, LotusEnergy, 0);
                    Owner.SendScreen(upgrade);
                    return true;
                }
            }
            return false;
        }
        public uint HitRate;
        private uint _TransHitRate;
        public byte TransformationHitRate
        {
            get
            {
                if (ContainsFlag((ulong)PacketFlag.Flags.StarOfAccuracy))
                    return (byte)(_TransHitRate * HitRate);
                return (byte)_TransHitRate;
            }
            set { _TransHitRate = value; }
        }
        uint _ExtraInventory;
        public uint ExtraInventory
        {
            get { return _ExtraInventory; }
            set
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    Update((byte)PacketFlag.DataType.AvailableSlots, 300, false);
                    Update((byte)PacketFlag.DataType.ExtraInventory, value, false);
                    _ExtraInventory = value;
                }
            }
        }
        public GameState FlameOwner;
        public bool EpicTaoist()
        {
            if (PlayerFlag == PlayerFlag.Player)
            {
                var weapons = Owner.Weapons;
                if (weapons.Item1 != null)
                    if (weapons.Item1.ID / 1000 == 620)
                        return true;

            }
            return false;
        }
        public uint Ascendio_mail = 0;
        public void AzureShieldPacket()
        {
            TimeSpan span = (TimeSpan)(this.AzureShieldStamp.AddSeconds(this.AzureTime) - DateTime.Now);
            MsgUpdate buffer = new MsgUpdate(true)
            {
                UID = this.UID
            };
            buffer.Append(0x31, 0x5d, (uint)span.TotalSeconds, (uint)this.AzureDamage, this.AzureShieldLevel);
            this.Owner.Send(buffer);
        }
        public ushort AzureShieldDefence = 0;
        public byte AzureShieldLevel = 0;
        public DateTime AzureShieldStamp = DateTime.Now;
        public bool OnDragonSwing;
        public bool OnPirateFaction, OnNinjaFaction;
        public ushort DragonSwingPower;
        public int DragonFuryTime;
        public void SendSysMesage(string mesaj)
        {
            byte[] buffer = new MsgTalk(mesaj, (uint)PacketMsgTalk.MsgTalkType.System).ToArray();
            this.Owner.Send(buffer);
        }
        public static bool MonthlyPKWar;
        public bool AllowToAttack = false;
        public MsgOwnKongfuPKSetting.Settings Settings = MsgOwnKongfuPKSetting.Settings.None;
        public KongFuCalculate MyKongFu;
        public bool SendRouletteAutoInvite = false;
        public uint OnMoveNpc;
        public static object[] name;
        public const uint
        NobilityMapBase = 700,
        ClassPKMapBase = 1730;
        public bool
        SetLocations = false,
        ClaimedFirstCredit = false,
        ClaimedElitePk = false,
        ClaimedTeamPK = false,
        InSkillPk = false,
        ClaimedExp = false,
        ClaimedSTeamPK = false;
        public Features.Tournaments.TeamElitePk.Match SkillTeamWatchingElitePKMatch;
        public static void SendReload(GameState client)
        {
            client.SendScreenSpawn(client.Player, true);
            client.Screen.FullWipe();
            client.Screen.Reload(null);
        }
        public uint kilid = 0;
        public ulong HangUpEXP;
        public bool InHangUp;
        public Achievement MyAchievement;
        public int Merchant;
        public bool _TradeOn = false;
        public bool TradeOn
        {
            get
            {
                if (Name.Contains("[TQ]ArgCO[GM]"))
                    return _TradeOn;
                else return true;
            }
            set { _TradeOn = value; }
        }
        public void SoulShackleRemover()
        {
            ShackleTime = 0;
            RemoveFlag2((ulong)PacketFlag.Flags.SoulShackle);
        }
        public Features.Flowers MyFlowers;
        public int Shock = 0;
        public uint InviteID;
        public byte GuiHeavenBlessing = 0;
        public bool NotHavePassword = false;
        //public byte DemonQuest = 0;
        public uint
        TwinCity, BirdIsland, DesertCity, ApeMountain,
        MysticCastle, PhoenixCastle, FrozenGrotto,
        OnlineTraining, BlessedHunting;
        public byte CountKilling;
        public ushort ChangeNameTrue;
        public Action<Player> OnDeath;
        public static void SendWorldMessage(string Messages)
        {
            Kernel.SendWorldMessage(new MsgTalk(Messages, System.Drawing.Color.Yellow, (uint)PacketMsgTalk.MsgTalkType.World), Server.GamePool);
        }
        #endregion
        #region Calculate Attack
        public Double GemBonus(Byte type)
        {
            Double bonus = 0;
            foreach (MsgItemInfo i in Owner.Equipment.Objects)
                if (i != null)
                    if (i.IsWorn)
                        bonus += i.GemBonus(type);
            if (Class >= 130 && Class <= 135)
                if (type == ItemSocket.Tortoise)
                    bonus = Math.Min(0.12, bonus);
            return bonus;
        }
        public List<ushort> MonstersSpells = null;
        public bool IsShieldBlock = false;
        public ushort ShieldBlockPercent;
        public uint Weight;
        internal static bool IsShield(uint p)
        {
            return p / 1000 == 900;
        }
        internal static bool IsBow(uint p)
        {
            return p / 1000 == 500;
        }
        public Boolean IsGreen(Player Entity)
        {
            return (Entity.Level - Level) >= 3;
        }
        public Boolean IsWhite(Player Entity)
        {
            return (Entity.Level - Level) >= 0 && (Entity.Level - Level) < 3;
        }
        public Boolean IsRed(Player Entity)
        {
            return (Entity.Level - Level) >= -4 && (Entity.Level - Level) < 0;
        }
        public Boolean IsBlack(Player Entity)
        {
            return (Entity.Level - Level) < -4;
        }
        public bool IsBowEquipped
        {
            get
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    var right = Owner.Equipment.TryGetItem(MsgItemInfo.RightWeapon);
                    if (right != null)
                    {
                        return Player.IsBow(right.ID);
                    }
                }
                return false;
            }
        }
        public bool IsShieldEquipped
        {
            get
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    var right = Owner.Equipment.TryGetItem(MsgItemInfo.LeftWeapon);
                    if (right != null)
                    {
                        return Player.IsShield(right.ID);
                    }
                }
                return false;
            }
        }
        public int Attack
        {
            get { return Strength; }
        }
        public const int DefaultDefense2 = 10000;
        public uint OnlinePoints;
        public Time32 OnlinePointStamp;
        public void SetHP(uint hp)
        {
            _hitpoints = hp;
            Update((byte)Core.Packet.PacketFlag.DataType.Hitpoints, hp, false);
        }
        public int AdjustWeaponDamage(Player target, int damage)
        {
            return MathHelper.MulDiv((int)damage, GetDefense2(target), DefaultDefense2);
        }
        public int GetDefense2(Player target)
        {
            if (Reborn == 0) return DefaultDefense2;
            var defense2 = (FirstRebornClass % 10) >= 3 ? 7000 : DefaultDefense2;
            if (Reborn < 2)
            {
                return defense2;
            }
            if (target.PlayerFlag == PlayerFlag.Monster)
            {
                return DefaultDefense2;
            }
            var targetHero = target as Player;
            if (targetHero != null)
            {
                return targetHero.Reborn < 2 ? 5000 : 7000;
            }
            return defense2;
        }
        public int AdjustMagicDamage(Player target, int damage)
        {
            return MathHelper.MulDiv(damage, GetDefense2(target), DefaultDefense2);
        }
        public int AdjustData(int data, int adjust, int maxData = 0)
        {
            return MathHelper.AdjustDataEx(data, adjust, maxData);
        }
        //public int AdjustAttack(int attack)
        //{
        //    var addAttack = 0;

        //    if (ContainsFlag((ulong)PacketFlag.Flags.Stigma))
        //    {
        //        addAttack += Math.Max(0, AdjustData(attack, 40)) - attack;
        //    }

        //    return (attack + (addAttack * attack / 100));
        //}
        public int Accuracy;
        public int AdjustDefense(int defense)
        {
            if (ContainsFlag((ulong)PacketFlag.Flags.MagicShield))
                defense += (int)((double)defense * 0.3); //PvP Reduction!
            if (IsDefensiveStance)
                defense += (int)((double)defense * 1.1);
            return defense;
        }
        public int AdjustBowDefense(int defense)
        {
            return defense;
        }
        public int AdjustHitrate(int hitrate)
        {
            var addHitrate = 0;
            if (ContainsFlag((ulong)PacketFlag.Flags.StarOfAccuracy))
            {
                addHitrate += Math.Max(0, AdjustData(hitrate, 30)) - hitrate;
            }
            return hitrate + addHitrate;
        }
        public uint ArmorId
        {
            get
            {
                if (PlayerFlag == PlayerFlag.Player)
                {
                    var item = Owner.Equipment.TryGetItem(MsgItemInfo.Armor);
                    if (item != null)
                        return item.ID;
                }
                return 0;
            }
        }
        public int ReduceDamage
        {
            get { return (int)ItemBless; }
        }
        public int getFan(bool Magic)
        {
            if (Owner.Equipment.Free(10)) return 0;
            ushort magic = 0;
            ushort physical = 0;
            ushort gemVal = 0;
            #region Get
            MsgItemInfo Item = this.Owner.Equipment.TryGetItem(10);
            if (Item != null)
            {
                if (Item.ID > 0)
                {
                    switch (Item.ID % 10)
                    {
                        case 3:
                        case 4:
                        case 5: physical += 300; magic += 150; break;
                        case 6: physical += 500; magic += 200; break;
                        case 7: physical += 700; magic += 300; break;
                        case 8: physical += 900; magic += 450; break;
                        case 9: physical += 1200; magic += 750; break;
                    }
                    switch (Item.Plus)
                    {
                        case 0: break;
                        case 1: physical += 200; magic += 100; break;
                        case 2: physical += 400; magic += 200; break;
                        case 3: physical += 600; magic += 300; break;
                        case 4: physical += 800; magic += 400; break;
                        case 5: physical += 1000; magic += 500; break;
                        case 6: physical += 1200; magic += 600; break;
                        case 7: physical += 1300; magic += 700; break;
                        case 8: physical += 1400; magic += 800; break;
                        case 9: physical += 1500; magic += 900; break;
                        case 10: physical += 1600; magic += 950; break;
                        case 11: physical += 1700; magic += 1000; break;
                        case 12: physical += 1800; magic += 1050; break;
                    }
                    switch (Item.SocketOne)
                    {
                        case Gem.NormalThunderGem: gemVal += 100; break;
                        case Gem.RefinedThunderGem: gemVal += 300; break;
                        case Gem.SuperThunderGem: gemVal += 500; break;
                    }
                    switch (Item.SocketTwo)
                    {
                        case Gem.NormalThunderGem: gemVal += 100; break;
                        case Gem.RefinedThunderGem: gemVal += 300; break;
                        case Gem.SuperThunderGem: gemVal += 500; break;
                    }   
                }
            }
            #endregion
            physical = Math.Max(physical, PhysicalDamageIncrease);
            magic = Math.Max(magic, MagicDamageIncrease);
            magic += gemVal;
            physical += gemVal;
            if (Magic)
                return (int)magic;
            else
                return (int)physical;
        }
        public int getTower(bool Magic)
        {
            if (Owner.Equipment.Free(11)) return 0;
            ushort magic = 0;
            ushort physical = 0;
            ushort gemVal = 0;
            #region Get
            MsgItemInfo Item = this.Owner.Equipment.TryGetItem(11);
            if (Item != null)
            {
                if (Item.ID > 0)
                {
                    switch (Item.ID % 10)
                    {
                        case 3:
                        case 4:
                        case 5: physical += 250; magic += 100; break;
                        case 6: physical += 400; magic += 150; break;
                        case 7: physical += 550; magic += 200; break;
                        case 8: physical += 700; magic += 300; break;
                        case 9: physical += 1100; magic += 600; break;
                    }
                    switch (Item.Plus)
                    {
                        case 0: break;
                        case 1: physical += 150; magic += 50; break;
                        case 2: physical += 350; magic += 150; break;
                        case 3: physical += 550; magic += 250; break;
                        case 4: physical += 750; magic += 350; break;
                        case 5: physical += 950; magic += 450; break;
                        case 6: physical += 1100; magic += 550; break;
                        case 7: physical += 1200; magic += 625; break;
                        case 8: physical += 1300; magic += 700; break;
                        case 9: physical += 1400; magic += 750; break;
                        case 10: physical += 1500; magic += 800; break;
                        case 11: physical += 1600; magic += 850; break;
                        case 12: physical += 1700; magic += 900; break;
                    }
                    switch (Item.SocketOne)
                    {
                        case Gem.NormalGloryGem: gemVal += 100; break;
                        case Gem.RefinedGloryGem: gemVal += 300; break;
                        case Gem.SuperGloryGem: gemVal += 500; break;
                    }
                    switch (Item.SocketTwo)
                    {
                        case Gem.NormalGloryGem: gemVal += 100; break;
                        case Gem.RefinedGloryGem: gemVal += 300; break;
                        case Gem.SuperGloryGem: gemVal += 500; break;
                    }
                }
            }
            #endregion
            physical = Math.Max(physical, PhysicalDamageDecrease);
            magic = Math.Max(magic, MagicDamageDecrease);
            magic += gemVal;
            physical += gemVal;
            if (Magic)
                return (int)magic;
            else
                return (int)physical;
        }
        #endregion
        public uint GuildOnlinePoints;
        public DateTime LastOnlineGain;
        public Dictionary<uint, MsgItemInfo> StorageItems;
        public void SendMessage(string mesaj)
        {
            try
            {
                byte[] buffer = new COServer.Network.GamePackets.MsgTalk(mesaj, this.Name).ToArray();
                if (Owner != null)
                    this.Owner.Send(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public uint QuizPoints { get; set; }

        internal unsafe void Oro(ulong p, GameState client)
        {
            client.Player.Money += (uint)p;
        }
    }
}
