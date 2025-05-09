// ☺ Editada y Reparada Por Pezzi Tomas / Fixed and Work by Pezzi Tomas
using System;
using System.IO;
using System.Linq;
using COServer.Game;
using System.Drawing;
using COServer.Client;
using COServer.Interfaces;
using COServer.Game.Features;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Core.Packet;
using Core.Enums;

namespace COServer.Network.GamePackets
{
    public static class GeneralData
    {
        public static string ReadString(byte[] data, ushort position, ushort count)
        {
            return Server.Encoding.GetString(data, position, count);
        }
        public static void WorldMessage(string message)
        {
            MsgTalk msg = new MsgTalk(message, System.Drawing.Color.MediumBlue, (uint)PacketMsgTalk.MsgTalkType.Center);
            foreach (Client.GameState pClient in Server.GamePool)
                pClient.Send(msg);
        }
        public static void ReincarnationHash(GameState client)
        {
            if (Kernel.ReincarnatedCharacters.ContainsKey(client.Player.UID))
            {
                if (client.Player.Level >= 110 && client.Player.Reborn == 2)
                {
                    ushort stats = 0;
                    uint lev1 = client.Player.Level;
                    Game.Features.Reincarnation.ReincarnateInfo info = Kernel.ReincarnatedCharacters[client.Player.UID];
                    client.Player.Level = info.Level;
                    client.Player.Experience = info.Experience;
                    Kernel.ReincarnatedCharacters.Remove(info.UID);
                    DB.ReincarnationTable.RemoveReincarnated(client.Player);
                    stats = (ushort)(((client.Player.Level - lev1) * 3) - 3);
                    client.Player.Atributes += stats;
                }
            }
        }
        public static void PrintPacket(byte[] packet)
        {
            foreach (byte D in packet)
            {
                Console.Write((Convert.ToString(D, 16)).PadLeft(2, '0') + " ");
            }
            Console.Write("\n\n");
        }
        public static bool PassLearn(byte ID, Player Entity)
        {
            bool Pass = false;
            switch ((SubPro.ProID)ID)
            {
                case SubPro.ProID.MartialArtist:
                    {
                        if (Entity.Owner.Inventory.Contains(721259, 5))
                        {
                            Entity.Owner.Inventory.Remove(721259, 5);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
                case SubPro.ProID.Warlock:
                    {
                        if (Entity.Owner.Inventory.Contains(721261, 10))
                        {
                            Entity.Owner.Inventory.Remove(721261, 10);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
                case SubPro.ProID.ChiMaster:
                    {
                        if (Entity.Owner.Inventory.Contains(711188, 1))
                        {
                            Entity.Owner.Inventory.Remove(711188, 1);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
                case SubPro.ProID.Sage:
                    {
                        if (Entity.Owner.Inventory.Contains(723087, 20))
                        {
                            Entity.Owner.Inventory.Remove(723087, 20);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
                case SubPro.ProID.Apothecary:
                    {
                        if (Entity.Owner.Inventory.Contains(1088002, 10))
                        {
                            Entity.Owner.Inventory.Remove(1088002, 10);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
                case SubPro.ProID.Performer:
                    {
                        if (Entity.Owner.Inventory.Contains(753003, 15) || Entity.Owner.Inventory.Contains(711679, 1))
                        {
                            if (Entity.Owner.Inventory.Contains(753003, 15))
                            {
                                Entity.Owner.Inventory.Remove(753003, 15);
                            }
                            else if (Entity.Owner.Inventory.Contains(711679, 1))
                            {
                                Entity.Owner.Inventory.Remove(711679, 1);
                            }
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                            break;
                        }
                        break;
                    }
                case SubPro.ProID.Wrangler:
                    {
                        if (Entity.Owner.Inventory.Contains(723903, 40))
                        {
                            Entity.Owner.Inventory.Remove(723903, 40);
                            Pass = true;
                            Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
                        }
                        break;
                    }
            }
            return Pass;
        }
        public static void ChangeAppearance(MsgAction action, GameState client)
        {
            if (client.Player.Tournament_Signed && ((Enums.AppearanceType)action.dwParam) != Enums.AppearanceType.Garment) return;
            action.UID = client.Player.UID;
            client.Player.Appearance = (Enums.AppearanceType)action.dwParam;
            client.SendScreen(action, true);
        }
        public static bool SwitchEquipment(GameState client, bool toAlternative)
        {
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Fly))
            {
                client.Send("You cannot switch equipment during flight.");
                return false;
            }
            if (client.Equipment.Free(MsgItemInfo.AlternateRightWeapon) && !client.Equipment.Free(MsgItemInfo.AlternateLeftWeapon))
            {
                client.Send("Invalid weapons! Missing the important weapons? Unequip the alternative left weapon.");
                return false;
            }
            foreach (var eq in client.Equipment.Objects)
            {
                if (eq != null)
                {
                    if (!DB.ConquerItemInformation.BaseInformations.ContainsKey(eq.ID))
                    {
                        client.Send("You cannot switch equipment because " + ((ItemPositionName)eq.Position).ToString().Replace("_", "~") + "'" + ((eq.Position % 20) == MsgItemInfo.Boots ? "" : "s") + " stats are not compatible with you (level or profession).");
                        return false;
                    }
                    var itemInfo = DB.ConquerItemInformation.BaseInformations[eq.ID];
                    if (!((ItemHandler.EquipPassLvlReq(itemInfo, client) || ItemHandler.EquipPassRbReq(itemInfo, client)) && ItemHandler.EquipPassJobReq(itemInfo, client)))
                    {
                        client.Send("You cannot switch equipment because " + ((ItemPositionName)eq.Position).ToString().Replace("_", "~") + "'" + ((eq.Position % 20) == MsgItemInfo.Boots ? "" : "s") + " stats are not compatible with you (level or profession).");
                        return false;
                    }
                }
            }
            client.Player.AttackPacket = null;
            if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow))
                client.Player.RemoveFlag3((ulong)PacketFlag.Flags.PathOfShadow);
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Fly);
            client.AlternateEquipment = toAlternative;
            client.LoadItemStats();
            client.Player.ChangeEquip = Time32.Now;
            client.Equipment.UpdateEntityPacket();
            MsgPlayerAttriInfo Stats = new MsgPlayerAttriInfo(client);
            client.Send(Stats.ToArray());
            return true;
        }
        public static void LevelUpSpell(MsgAction action, GameState client)
        {
            ushort spellID = (ushort)action.dwParam;
            ISkill spell = null;
            if (client.Spells.TryGetValue(spellID, out spell))
            {
                var spellInfo = DB.SpellTable.GetSpell(spellID, client);
                if (spellInfo != null)
                {
                    if (client.Trade.InTrade) return;
                    uint CpsCost = 0;
                    #region Costs
                    switch (spell.Level)
                    {
                        case 0: CpsCost = 27; break;
                        case 1: CpsCost = 81; break;
                        case 2: CpsCost = 122; break;
                        case 3: CpsCost = 181; break;
                        case 4: CpsCost = 300; break;
                        case 5: CpsCost = 400; break;
                        case 6: CpsCost = 500; break;
                        case 7: CpsCost = 600; break;
                        case 8: CpsCost = 800; break;
                        case 9: CpsCost = 1000; break;
                    }
                    #endregion
                    int max = Math.Max((int)spell.Experience, 1);
                    int percentage = 100 - (int)(max / Math.Max((spellInfo.NeedExperience / 100), 1));
                    CpsCost = (uint)(CpsCost * percentage / 100);
                    if (client.Player.ConquerPoints >= CpsCost)
                    {
                        client.Player.ConquerPoints -= CpsCost;
                        spell.Level++;
                        if (spell.Level == spell.PreviousLevel / 2)
                            spell.Level = spell.PreviousLevel;
                        spell.Experience = 0;
                        spell.Send(client);
                    }
                }
            }
        }
        public static void LevelUpProficiency(MsgAction action, GameState client)
        {
            ushort proficiencyID = (ushort)action.dwParam;
            IProf proficiency = null;
            if (client.Proficiencies.TryGetValue(proficiencyID, out proficiency))
            {
                if (proficiency.Level != 20)
                {
                    if (client.Trade.InTrade) return;
                    uint cpCost = 0;
                    #region Costs
                    switch (proficiency.Level)
                    {
                        case 1: cpCost = 28; break;
                        case 2: cpCost = 28; break;
                        case 3: cpCost = 28; break;
                        case 4: cpCost = 28; break;
                        case 5: cpCost = 28; break;
                        case 6: cpCost = 55; break;
                        case 7: cpCost = 81; break;
                        case 8: cpCost = 135; break;
                        case 9: cpCost = 162; break;
                        case 10: cpCost = 270; break;
                        case 11: cpCost = 324; break;
                        case 12: cpCost = 324; break;
                        case 13: cpCost = 324; break;
                        case 14: cpCost = 324; break;
                        case 15: cpCost = 375; break;
                        case 16: cpCost = 548; break;
                        case 17: cpCost = 799; break;
                        case 18: cpCost = 1154; break;
                        case 19: cpCost = 1420; break;
                    }
                    #endregion
                    uint needExperience = DB.DataHolder.ProficiencyLevelExperience(proficiency.Level);
                    int max = Math.Max((int)proficiency.Experience, 1);
                    int percentage = 100 - (int)(max / (needExperience / 100));
                    cpCost = (uint)(cpCost * percentage / 100);
                    if (client.Player.ConquerPoints >= cpCost)
                    {
                        client.Player.ConquerPoints -= cpCost;
                        proficiency.Level++;
                        if (proficiency.Level == proficiency.PreviousLevel / 2)
                        {
                            proficiency.Level = proficiency.PreviousLevel;
                            DB.DataHolder.ProficiencyLevelExperience((byte)(proficiency.Level + 1));
                        }
                        proficiency.Experience = 0;
                        proficiency.Send(client);
                    }
                    else
                    {
                        if (client.Player.BoundCps >= cpCost)
                        {
                            client.Player.BoundCps -= cpCost;
                            proficiency.Level++;
                            if (proficiency.Level == proficiency.PreviousLevel / 2)
                            {
                                proficiency.Level = proficiency.PreviousLevel;
                                DB.DataHolder.ProficiencyLevelExperience((byte)(proficiency.Level + 1));
                            }
                            proficiency.Experience = 0;
                            proficiency.Send(client);
                        }
                    }
                }
            }
            else
            {
                //break;
            }
            return;
        }
        public static void Revive(MsgAction action, GameState client)
        {
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.SoulShackle)) return;
            if (Time32.Now >= client.Player.DeathStamp.AddSeconds(18) && client.Player.Dead)
            {
                client.Player.Action = Enums.ConquerAction.None;
                client.ReviveStamp = Time32.Now;
                client.Attackable = false;
                client.Player.TransformationID = 0;
                client.Player.AutoRev = 0;
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Dead);
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Ghost);
                client.Player.Hitpoints = client.Player.MaxHitpoints;
                if (client.Player.MapID == 1518)
                {
                    client.Player.Teleport(1002, 300, 278);
                    return;
                }
                bool ReviveHere = action.dwParam == 1;
                if (client.Spells.ContainsKey(12660))
                {
                    client.XPCount = client.Player.XPCountTwist;
                }
                if (client.Player.MapID == 1038 && DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    client.Player.Teleport(1002, 300, 278);
                }
                else if (ReviveHere && client.Player.HeavenBlessing > 0 && !Constants.NoRevHere.Contains(client.Player.MapID))
                {
                    if (client.Player.MapID == Pezzi.ServerEvents.LastManStanding.ID || client.Player.MapID == Pezzi.ServerEvents.IronMap.ID || client.Player.MapID == Pezzi.ServerEvents.DailyMap.ID || client.Player.MapID == Pezzi.ServerEvents.ExtremePk.ID) return;
                    client.Send(new MsgMapInfo()
                    {
                        BaseID = client.Map.BaseID,
                        ID = client.Map.ID,
                        Status = DB.MapsTable.MapInformations[client.Map.ID].Status
                    });
                }
                else
                {
                    ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
                    client.Player.Teleport(Point[0], Point[1], Point[2]);
                }
            }
        }
        public static void UsePortal(MsgAction action, GameState client)
        {
            client.Player.Action = Enums.ConquerAction.None;
            client.ReviveStamp = Time32.Now;
            client.Attackable = false;
            ushort portal_X = (ushort)(action.dwParam & 65535);
            ushort portal_Y = (ushort)(action.dwParam >> 16);
            string portal_ID = portal_X.ToString() + ":" + portal_Y.ToString() + ":" + client.Map.ID.ToString();
            if (client.Account.State == DB.AccountTable.AccountState.Administrator)
                client.Send("Portal ID: " + portal_ID);
            foreach (Game.Portal portal in client.Map.Portals)
            {
                if (Kernel.GetDistance(portal.CurrentX, portal.CurrentY, client.Player.X, client.Player.Y) <= 4)
                {
                    client.Player.PrevX = client.Player.X;
                    client.Player.PrevY = client.Player.Y;
                    client.Player.Teleport(portal.DestinationMapID, portal.DestinationX, portal.DestinationY);
                    return;
                }
            }
            client.Player.Teleport(1002, 300, 278);
        }
        public static void ObserveEquipment(MsgAction action, GameState client)
        {
            if (ItemHandler.NulledClient(client)) return;
            GameState Observer, Observee;
            if (Kernel.GamePool.TryGetValue(action.UID, out Observer) && Kernel.GamePool.TryGetValue(action.dwParam, out Observee))
            {
                if (action.ID != PacketMsgAction.Mode.ObserveEquipment)
                    Observer.Send(Observee.Player.WindowSpawn());
                MsgPlayerAttriInfo Stats = new MsgPlayerAttriInfo(Observee);
                Observer.Send(Stats.ToArray());
                for (Byte pos = (Byte)MsgItemInfo.Head; pos <= MsgItemInfo.AlternateGarment; pos++)
                {
                    MsgItemInfo i = Observee.Equipment.TryGetItem((Byte)pos);
                    if (i != null)
                    {
                        if (i.IsWorn)
                        {
                            MsgItemInfoEx2 view = new MsgItemInfoEx2();
                            view.CostType = MsgItemInfoEx2.CostTypes.ViewEquip;
                            view.Identifier = Observee.Player.UID;
                            view.Position = (ItemHandler.Positions)(pos % 20);
                            view.ParseItem(i);
                            Observer.Send(view);
                            i.SendExtras(client);
                        }
                    }
                }
                if (Observee.WardRobe != null)
                {
                    MsgItemInfoEx2 view = new MsgItemInfoEx2();
                    view.CostType = MsgItemInfoEx2.CostTypes.ViewEquip;
                    view.Identifier = Observee.Player.UID;

                    var item = Observee.WardRobe.MyGarment.Item;
                    if (item != null)
                    {
                        view.Position = ItemHandler.Positions.Garment;
                        view.ParseItem(item);
                        Observer.Send(view);
                        item.SendExtras(client);
                    }
                    item = Observee.WardRobe.MySteedArmor.Item;
                    if (item != null)
                    {
                        view.Position = ItemHandler.Positions.SteedArmor;
                        view.ParseItem(item);
                        Observer.Send(view);
                        item.SendExtras(client);
                    }
                }
                MsgName Name = new MsgName(true);
                Name.Action = MsgName.Mode.QuerySpouse;
                Name.UID = client.Player.UID;
                Name.TextsCount = 1;
                Name.Texts = new List<string>()
                {
                    Observee.Player.Spouse
                };
                Observer.Send(Name);
                if (action.ID == PacketMsgAction.Mode.ObserveEquipment)
                {
                    Name.Action = MsgName.Mode.Effect;
                    Observer.Send(Name);
                }
                Observer.Send(action);
                Observee.Send(Observer.Player.Name + " is checking your equipment");
            }
        }
        public static void ChangeFace(MsgAction action, GameState client)
        {
            if (client.Player.Money >= 500)
            {
                uint newface = action.dwParam;
                if (client.Player.Body > 2000)
                {
                    newface = newface < 200 ? newface + 200 : newface;
                    client.Player.Face = (ushort)newface;
                }
                else
                {
                    newface = newface > 200 ? newface - 200 : newface;
                    client.Player.Face = (ushort)newface;
                }
            }
        }
        public static void CheckForRaceItems(GameState client)
        {
            StaticEntity item;
            if (client.Screen.GetRaceObject(p => { return Kernel.GetDistance(client.Player.X, client.Player.Y, p.X, p.Y) <= 1; }, out item))
            {
                if (item == null) return;
                if (!item.Viable) return;
                var type = item.Type;
                bool successful = false;
                if (type == RaceItemType.FrozenTrap && !item.QuestionMark)
                {
                    if (item.SetBy != client.Player.UID)
                    {
                        client.ApplyRacePotion(type, uint.MaxValue);
                        client.Map.RemoveStaticItem(item);
                        successful = true;
                    }
                }
                else
                {
                    if (client.Potions == null) client.Potions = new UsableRacePotion[5];
                    for (ushort i = 0; i < client.Potions.Length; i++)
                    {
                        var pot = client.Potions[i];
                        if (pot == null)
                        {
                            pot = (client.Potions[i] = new UsableRacePotion());
                            pot.Type = type;
                            pot.Count = item.Level;
                            client.Send(new MsgRaceTrackProp(true)
                            {
                                PotionType = type,
                                Amount = (ushort)pot.Count,
                                Location = (ushort)(i + 1)
                            });
                            successful = true;
                            break;
                        }
                        else if (pot.Type == type)
                        {
                            pot.Count += item.Level;
                            client.Send(new MsgRaceTrackProp(true)
                            {
                                PotionType = type,
                                Amount = (ushort)pot.Count,
                                Location = (ushort)(i + 1)
                            });
                            successful = true;
                            break;
                        }
                    }
                }
                if (successful)
                {
                    client.SendScreen(new MsgName(true)
                    {
                        Texts = new List<string>() { "eidolon" },
                        UID = client.Player.UID,
                        Action = MsgName.Mode.Effect
                    });
                    client.RemoveScreenSpawn(item, true);
                    item.Viable = false;
                    item.NotViableStamp = Time32.Now;
                }
            }
        }
        public static void PlayerJump(MsgAction action, GameState client)
        {

            if (client.Player.Dead || client.Player.ContainsFlag(((ulong)PacketFlag.Flags.Dead)) || client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ghost))
            {
                return;
            }
            if (client.Companion != null)
            {
                if (client.Companion.UID == action.UID)
                {
                    PetsControler.datajump(client, action);

                    return;
                }
            }
            client.Player.KillCount2 = 0;
            client.Player.SpiritFocus = false;
            ushort oldX = client.Player.X;
            ushort oldY = client.Player.Y;
            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Intensify);
            client.Player.IntensifyPercent = 0;
            if (client.Player.MapID == 1927 && Kernel.SpawnBanshee2)
            {
                foreach (INpc Npc in client.Map.Npcs.Values)
                {
                    if (Npc.MapID == 1927 && (Npc.UID == 2999) && Kernel.GetDistance(client.Player.X, client.Player.Y, Npc.X, Npc.Y) < 17)
                    {
                        Npc.SendSpawn(client);
                    }
                }
            }
            client.Player.Action = Enums.ConquerAction.None;
            client.Mining = false;
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
            {
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                foreach (var Client in client.Prayers)
                {
                    if (Client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
                    {
                        Client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                    }
                }
                client.Prayers.Clear();
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
            {
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                client.PrayLead = null;
            }
            Time32 Now = Time32.Now;
            client.Attackable = true;
            if (client.Player.AttackPacket != null)
            {
                client.Player.AttackPacket = null;
            }
            if (client.Player.Dead)
            {
                if (Now > client.Player.DeathStamp.AddSeconds(4))
                {
                    client.Disconnect();
                    return;
                }
            }
            ushort new_X = Core.BitConverter.ToUInt16(action.ToArray(), 12);
            ushort new_Y = Core.BitConverter.ToUInt16(action.ToArray(), 14);
            if (client.lastJumpDistance == 0) goto Jump;
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
            {
                int distance = Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y);
                ushort take = (ushort)(1.5F * (distance / 2));
                if (client.Vigor >= take)
                {
                    client.Vigor -= take;
                    Vigor vigor = new Vigor(true);
                    vigor.Amount = client.Vigor;
                    vigor.Send(client);
                }
                else
                {
                }
            }
            client.LastJumpTime = (int)Kernel.maxJumpTime(client.lastJumpDistance);
            //var serverstamp = Now.GetHashCode() - client.lastJumpTime.GetHashCode();
            //var clientstamp = action.TimeStamp.GetHashCode() - client.lastClientJumpTime.GetHashCode();
            //var speed = clientstamp - serverstamp;
            //if (speed > 100)
            //{
            //    client.speedHackSuspiction++;
            //    if (!client.Player.OnCyclone() && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride) && !client.Player.OnOblivion() && !client.Player.OnSuperman() && !client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DragonCyclone) && !client.Player.OnSuperCyclone() && !client.Player.Transformed && client.speedHackSuspiction >= 3)
            //    {
            //        client.Disconnect();
            //    }
            //}
            //else
            //{
            //    client.speedHackSuspiction = Math.Max(0, client.speedHackSuspiction - 1);
            //}
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
            {
                client.Player.LastTimeUseSlide = Time32.Now;
            }
            if (Now < client.lastJumpTime.AddMilliseconds(client.LastJumpTime))
            {
                bool doDisconnect = false;
                if (client.Player.Transformed)
                    if (client.Player.TransformationID != 207 && client.Player.TransformationID != 267)
                        doDisconnect = true;
                if (client.Player.Transformed && doDisconnect)
                {

                }
                if (client.Player.Transformed && !doDisconnect)
                {
                    goto Jump;
                }
                else if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                {
                    int time = (int)Kernel.maxJumpTime(client.lastJumpDistance);
                    int speedprc = DB.DataHolder.SteedSpeed(client.Equipment.TryGetItem(MsgItemInfo.Steed).Plus);
                    if (speedprc != 0)
                    {
                        if (Now < client.lastJumpTime.AddMilliseconds(time - (time * speedprc / 100)))
                        {

                        }
                    }
                    else
                    {

                    }
                }
            }
        Jump:
            client.lastJumpDistance = Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y);
            client.lastClientJumpTime = action.TimeStamp;
            client.lastJumpTime = Now;
            Game.Map Map = client.Map;
            if (Map != null)
            {
                if (Map.Floor[new_X, new_Y, Game.MapObjectType.Player, null])
                {
                    if (Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y) <= 20)
                    {
                        client.Player.Action = Enums.ConquerAction.Jump;
                        client.Player.Facing = Kernel.GetAngle(action.X, action.Y, new_X, new_Y);
                        client.Player.PX = client.Player.X;
                        client.Player.PY = client.Player.Y;
                        client.Player.X = new_X;
                        client.Player.Y = new_Y;
                        if (client.Player.MapID == MsgWarFlag.MapID)
                            CheckForFlag(client);
                        client.SendScreen(action, true);
                        client.Screen.Reload(action);
                        if (client.Player.MapID == 1351)
                        {
                            if (new_X == 14 && new_Y == 122)//stig
                            {
                                client.Player.Teleport(1002, 309, 338);
                            }
                        }
                        if (client.Player.InteractionInProgress && client.Player.InteractionSet)
                        {
                            if (client.Player.Body == 1003 || client.Player.Body == 1004)
                            {
                                if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
                                {
                                    GameState ch = Kernel.GamePool[client.Player.InteractionWith];
                                    Network.GamePackets.MsgAction general = new Network.GamePackets.MsgAction(true);
                                    general.UID = ch.Player.UID;
                                    general.X = new_X;
                                    general.Y = new_Y;
                                    general.ID = (PacketMsgAction.Mode)156;
                                    ch.Send(general.ToArray());
                                    ch.Player.Action = Enums.ConquerAction.Jump;
                                    ch.Player.X = new_X;
                                    ch.Player.Y = new_Y;
                                    ch.Player.Facing = Kernel.GetAngle(ch.Player.X, ch.Player.Y, new_X, new_Y);
                                    ch.SendScreen(action, true);
                                    ch.Screen.Reload(general);
                                    client.SendScreen(action, true);
                                    client.Screen.Reload(general);
                                }
                            }
                        }
                    }
                    else
                    {
                        client.Disconnect();
                    }
                }
                else
                {
                    if (client.Player.Mode == Enums.Mode.None)
                    {
                        client.Player.Teleport(client.Map.ID, client.Player.X, client.Player.Y);
                    }
                }
            }
            else
            {
                if (Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y) <= 20)
                {
                    client.Player.Action = Enums.ConquerAction.Jump;
                    client.Player.Facing = Kernel.GetAngle(action.X, action.Y, new_X, new_Y);
                    client.Player.X = new_X;
                    client.Player.Y = new_Y;
                    client.SendScreen(action, true);
                    client.Screen.Reload(action);
                }
                else
                {
                    client.Disconnect();
                }
            }
            if (client.Map.BaseID == 1038 && Game.GuildWar.IsWar || client.Map.BaseID == 10380 && Game.Kingdom.IsWar)
            {
                Game.Calculations.IsBreaking(client, oldX, oldY);
            }
            if (!client.Player.HasMagicDefender)
            {
                if (client.Team != null)
                {
                    var owners = client.Team.Teammates.Where(x => x.Player.MagicDefenderOwner);
                    if (owners != null)
                    {
                        foreach (var owner in owners)
                        {
                            if (Kernel.GetDistance(client.Player.X, client.Player.Y, owner.Player.X, owner.Player.Y) <= 4)
                            {
                                client.Player.HasMagicDefender = true;
                                client.Player.MagicDefenderStamp = Time32.Now;
                                client.Player.MagicDefenderSecs = (byte)(owner.Player.MagicDefenderStamp.AddSeconds(owner.Player.MagicDefenderSecs) - owner.Player.MagicDefenderStamp).AllSeconds();
                                client.Player.AddFlag3((ulong)PacketFlag.Flags.MagicDefender);
                                MsgUpdate upgrade = new MsgUpdate(true);
                                upgrade.UID = client.Player.UID;
                                upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, client.Player.MagicDefenderSecs, 0, 0);
                                client.Send(upgrade.ToArray());
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                client.Player.RemoveMagicDefender();
            }
        }
        private static void CheckForFlag(GameState client)
        {
            if (client.Player.GuildID == 0) return;
            foreach (var item in client.Map.StaticEntities.Values)
            {
                if (Kernel.GetDistance(item.X, item.Y, client.Player.X, client.Player.Y) == 0)
                {
                    client.Player.FlagStamp = Time32.Now;
                    client.Send(Server.Thread.CTF.generateTimer(60));
                    client.Send(Server.Thread.CTF.generateEffect(client));
                    MsgWarFlag.AddExploits(3, client.AsMember);
                    client.Guild.CTFPoints += 3;
                    client.Player.AddFlag2((ulong)PacketFlag.Flags.CarryingFlag);
                    MsgWarFlag.SendScores();
                    client.Map.RemoveStaticItem(item);
                    client.RemoveScreenSpawn(item, true);
                }
                else
                {
                    Server.Thread.CTF.AroundBase(client);
                }
            }
        }
        public static void PlayerGroundMovment(GroundMovement groundMovement, Client.GameState client)
        {
            client.Player.SpellStamp = Time32.Now.AddSeconds(-1);
            client.Player.KillCount2 = 0;
            client.Player.Action = Enums.ConquerAction.None;
            client.Attackable = true;
            client.Mining = false;
            var oldX = client.Player.X;
            var oldY = client.Player.Y;
            if (client.Companion != null)//PETS
            {
                if (groundMovement.UID == client.Companion.UID)
                {

                    PetsControler.MovementPacket(client, groundMovement);
                    return;
                }
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
            {
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                foreach (var Client in client.Prayers)
                {
                    if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
                    {
                        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                    }
                }
                client.Prayers.Clear();
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
            {
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                if (client.PrayLead != null)
                    client.PrayLead.Prayers.Remove(client);
                client.PrayLead = null;
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
            {
                client.Player.LastTimeUseSlide = Time32.Now;
                client.Player.Vigor -= 1;
                Network.GamePackets.Vigor vigor = new Network.GamePackets.Vigor(true);
                vigor.Amount = client.Player.Vigor;
                vigor.Send(client);
            }
            if (client.Player.AttackPacket != null)
            {
                client.Player.AttackPacket = null;
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride)) client.Vigor -= 1;
            client.Player.PX = client.Player.X;
            client.Player.PY = client.Player.Y;

            if (!client.Player.Move(groundMovement.Direction, groundMovement.GroundMovementType == GroundMovement.Slide)) return;

            if (client.Player.MapID == Game.SteedRace.MAPID)
            {
                CheckForRaceItems(client);
                if (!(DateTime.Now.Hour == 14 && DateTime.Now.Minute <= 25))
                {
                    if (client.Player.X <= Server.Thread.SteedRace.GateX + 1)
                    {
                        client.Player.Teleport(client.Player.MapID, client.Player.X, client.Player.Y);
                        return;
                    }
                }
            }
            if (client.Player.MapID == Game.SteedRace.MAPID)
                CheckForRaceItems(client);
            if (client.Player.MapID == MsgWarFlag.MapID)
                CheckForFlag(client);

            client.SendScreen(groundMovement, true);
            client.Screen.Reload(groundMovement);

            if (client.Player.InteractionInProgress)
            {
                if (!client.Player.InteractionSet)
                {
                    if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
                    {
                        Client.GameState ch = Kernel.GamePool[client.Player.InteractionWith];
                        if (ch.Player.InteractionInProgress && ch.Player.InteractionWith == client.Player.UID)
                        {
                            if (client.Player.InteractionX == client.Player.X && client.Player.Y == client.Player.InteractionY)
                            {
                                if (client.Player.X == ch.Player.X && client.Player.Y == ch.Player.Y)
                                {
                                    MsgInteract atac = new Network.GamePackets.MsgInteract(true);
                                    atac.Attacker = ch.Player.UID;
                                    atac.Attacked = client.Player.UID;
                                    atac.X = ch.Player.X;
                                    atac.Y = ch.Player.Y;
                                    atac.Damage = client.Player.InteractionType;
                                    atac.ResponseDamage = client.InteractionEffect;
                                    atac.InteractType = 47;
                                    ch.Send(atac);

                                    atac.InteractType = 49;
                                    atac.Attacker = client.Player.UID;
                                    atac.Attacked = ch.Player.UID;
                                    client.SendScreen(atac, true);

                                    atac.Attacker = ch.Player.UID;
                                    atac.Attacked = client.Player.UID;
                                    client.SendScreen(atac, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (client.Player.Body == 1003 || client.Player.Body == 1004)
                    {
                        if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
                        {
                            Client.GameState ch = Kernel.GamePool[client.Player.InteractionWith];

                            ch.Player.Facing = groundMovement.Direction;
                            ch.Player.Move(groundMovement.Direction);
                            MsgAction general = new MsgAction(true);
                            general.UID = ch.Player.UID;
                            general.wParam1 = ch.Player.X;
                            general.wParam2 = ch.Player.Y;
                            general.ID = (PacketMsgAction.Mode)0x9c;
                            ch.Send(general.ToArray());
                            ch.Screen.Reload(null);
                        }
                    }
                }
            }
            if (client.Map.BaseID == 1038 && Game.GuildWar.IsWar)
            {
                Game.Calculations.IsBreaking(client, oldX, oldY);
            }
            if (!client.Player.HasMagicDefender)
            {
                if (client.Team != null)
                {
                    var owners = client.Team.Teammates.Where(x => x.Player.MagicDefenderOwner);
                    if (owners != null)
                    {
                        foreach (var owner in owners)
                        {
                            if (Kernel.GetDistance(client.Player.X, client.Player.Y, owner.Player.X, owner.Player.Y) <= 4)
                            {
                                client.Player.HasMagicDefender = true;
                                client.Player.MagicDefenderStamp = Time32.Now;
                                client.Player.MagicDefenderSecs = (byte)(owner.Player.MagicDefenderStamp.AddSeconds(owner.Player.MagicDefenderSecs) - owner.Player.MagicDefenderStamp).AllSeconds();
                                client.Player.AddFlag3((ulong)PacketFlag.Flags.MagicDefender);
                                MsgUpdate upgrade = new MsgUpdate(true);
                                upgrade.UID = client.Player.UID;
                                upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, client.Player.MagicDefenderSecs, 0, 0);
                                client.Send(upgrade.ToArray());
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                client.Player.RemoveMagicDefender();
            }
        }
        public static void GetSurroundings(GameState client)
        {
            client.Screen.FullWipe();
            client.Screen.Reload(null);
            if (client.Player.PreviousMapID == MsgWarFlag.MapID)
                Server.Thread.CTF.CloseList(client);
        }
        public static void ChangeAction(MsgAction action, GameState client)
        {
            client.Player.Action = (ushort)action.dwParam;
            if (client.Companion != null)
            {
                if (action.UID == client.Companion.UID)
                {

                    PetsControler.ChangeActions(client, action);
                    return;
                }
            }
            if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
            {
                foreach (var Client in client.Prayers)
                {
                    action.UID = Client.Player.UID;
                    action.dwParam = (uint)client.Player.Action;
                    action.X = Client.Player.X;
                    action.Y = Client.Player.Y;
                    Client.Player.Action = client.Player.Action;
                    if (Time32.Now >= Client.CoolStamp.AddMilliseconds(1500))
                    {
                        if (Client.Equipment.IsAllSuper())
                            action.dwParam = (uint)(action.dwParam | (uint)(Client.Player.Class * 0x10000 + 0x1000000));
                        else if (Client.Equipment.IsArmorSuper())
                            action.dwParam = (uint)(action.dwParam | (uint)(Client.Player.Class * 0x10000));
                        Client.SendScreen(action, true);
                        Client.CoolStamp = Time32.Now;
                    }
                    else
                        Client.SendScreen(action, false);
                }
            }
            action.UID = client.Player.UID;
            action.dwParam = (uint)client.Player.Action;
            if (client.Player.Action == Enums.ConquerAction.Cool)
            {
                if (Time32.Now >= client.CoolStamp.AddMilliseconds(1500))
                {
                    if (client.Equipment.IsAllSuper())
                        action.dwParam = (uint)(action.dwParam | (uint)(client.Player.Class * 0x10000 + 0x1000000));
                    else if (client.Equipment.IsArmorSuper())
                        action.dwParam = (uint)(action.dwParam | (uint)(client.Player.Class * 0x10000));
                    client.SendScreen(action, true);
                    client.CoolStamp = Time32.Now;
                }
                else
                    client.SendScreen(action, false);
            }
            else
                client.SendScreen(action, false);
        }
        public static void ChangeDirection(MsgAction action, GameState client)
        {
            client.Player.Facing = (Enums.ConquerAngle)action.Facing;
            client.SendScreen(action, false);
        }
        public static void ChangePKMode(MsgAction action, GameState client)
        {
            if (client.Player.PKMode == PKMode.Kongfu)
            {
                if ((PKMode)(byte)action.dwParam != PKMode.Kongfu)
                {
                    client.Send("You`ll quit the Jiang Hu in 10 minutes.");
                }
            }
            if (client.InTeamQualifier()) return;
            if (client.InQualifier()) return;
            client.Player.AttackPacket = null;
            client.Player.PKMode = (PKMode)(byte)action.dwParam;
            client.Send(action);
            if ((client.Player.PKMode == PKMode.Kongfu) && (client.Player.MyKongFu != null))
            {
                client.Player.MyKongFu.OnJiangMode = true;
                client.Player.MyKongFu.SendStatusMode(client);
            }
            if (client.Player.PKMode == PKMode.PK)
            {
                client.Send("Free PK mode: you can attack monsters and all Players.");
            }
            else if (client.Player.PKMode == PKMode.Peace)
            {
                client.Send("Peace mode: You can only attack monsters.");
            }
            else if (client.Player.PKMode == PKMode.Team)
            {
                client.Send("Team mode: slay monsters, and all other players (including cross-server players) not in your current team or guild. ");
            }
            else if (client.Player.PKMode == PKMode.Capture)
            {
                client.Send("Capture mode: Slay monsters, black/blue-name criminals, and cross-server players.");
            }
            else if (client.Player.PKMode == PKMode.Revenge)
            {
                client.Send("revenge mode: Slay your listed enemies, monsters, and cross-server players.");
            }
            else if (client.Player.PKMode == PKMode.Union)
            {
                client.Send("The `Plander` mode only allow you to other players in enemy Union.");
            }
            else if (client.Player.PKMode == PKMode.Guild)
            {
                client.Send("Guild mode: Slay monsters, and players in your enemy guilds, and cross-server players.");
            }
            else if (client.Player.PKMode == PKMode.Kongfu)
            {
                client.Send("Jiang Hu mode: Slay Jiang Hu fighters, black/blue-name criminals, and cross-server players.");
            }
            else if (client.Player.PKMode == PKMode.CS)
            {
                client.Send("CS (Cross-Server) mode: Attack cross-server players. No Pk punishment.");
            }
            else if (client.Player.PKMode == PKMode.Invade)
            {
                client.Send("Invade mode: Only attack players of the target (current) server No Pk punishment.");
            }
        }
        public static void SetLocation(MsgAction action, GameState client)
        {
            if (client.Player.MyKongFu != null)
            {
                client.Player.MyKongFu.OnloginClient(client);
            }
            else if (client.Player.Reborn == 2)
            {
                MsgOwnKongfuBase hu = new MsgOwnKongfuBase
                {
                    Texts = { "0" }
                };
                hu.CreateArray();
                hu.Send(client);
            }
            SendFlower sendFlower = new SendFlower();
            sendFlower.Typing = (Flowers.IsBoy((uint)client.Player.Body) ? 3u : 2u);
            sendFlower.Apprend(client.Player.MyFlowers);
            client.Send(sendFlower.ToArray());
            if (client.Player.MyFlowers.aFlower > 0u)
            {
                client.Send(new SendFlower
                {
                    Typing = Flowers.IsBoy((uint)client.Player.Body) ? 2u : 3u
                }.ToArray());
            }
            if (client.Guild != null)
            {
                client.Guild.SendGuild(client);
                MsgDutyMinContri guild = new MsgDutyMinContri(31);
                guild.AprendGuild(client.Guild);
                client.Send(guild.ToArray());
            }
            MsgFamily clan = client.Player.GetClan;
            if (clan != null)
            {
                clan.Build(client, MsgFamily.Types.Info);
                client.Send(clan);
                client.Player.ClanName = clan.Name;
                client.Send(new MsgFamilyRelation(clan, MsgFamilyRelation.RelationTypes.Allies));
                client.Send(new MsgFamilyRelation(clan, MsgFamilyRelation.RelationTypes.Enemies));
            }

            foreach (Game.ConquerStructures.Society.Guild guild in Kernel.Guilds.Values)
            {
                guild.SendName(client);
                guild.SendName(client);
            }

            if (client.Player.EnlightmentTime > 0)
            {
                MsgMentorPlayer enlight = new MsgMentorPlayer(true);
                enlight.Enlighted = client.Player.UID;
                enlight.Enlighter = 0;

                if (client.Player.EnlightmentTime > 80)
                    client.Player.EnlightmentTime = 100;
                else if (client.Player.EnlightmentTime > 60)
                    client.Player.EnlightmentTime = 80;
                else if (client.Player.EnlightmentTime > 40)
                    client.Player.EnlightmentTime = 60;
                else if (client.Player.EnlightmentTime > 20)
                    client.Player.EnlightmentTime = 40;
                else if (client.Player.EnlightmentTime > 0)
                    client.Player.EnlightmentTime = 20;
                for (int count = 0; count < client.Player.EnlightmentTime; count += 20)
                {
                    client.Send(enlight.ToArray());
                }
            }

            if (client.Player.Hitpoints != 0)
            {
                if (client.Map.ID == 1036 || client.Map.ID == 1039)
                {
                    if (client.Player.PreviousMapID == 0)
                        client.Player.SetLocation(1002, 300, 278);
                    else
                    {
                        switch (client.Player.PreviousMapID)
                        {
                            default:
                                {
                                    client.Player.SetLocation(1002, 300, 278);
                                    break;
                                }
                            case 1000:
                                {
                                    client.Player.SetLocation(1000, 500, 650);
                                    break;
                                }
                            case 1020:
                                {
                                    client.Player.SetLocation(1020, 565, 562);
                                    break;
                                }
                            case 1011:
                                {
                                    client.Player.SetLocation(1011, 188, 264);
                                    break;
                                }
                            case 1015:
                                {
                                    client.Player.SetLocation(1015, 717, 571);
                                    break;
                                }
                        }
                    }
                }
            }
            else
            {
                if (client.Player.MapID == 1038)
                {
                    client.Player.SetLocation(1002, 300, 278);
                }
                else
                {
                    ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
                    client.Player.SetLocation(Point[0], Point[1], Point[2]);
                }
            }
            action.dwParam = client.Map.BaseID;
            action.X = client.Player.X;
            action.Y = client.Player.Y;
            client.Send(action);
        }
        public static object LoginSyncRoot = new object();
        public static void AppendConnect(MsgConnect Connect, GameState client)
        {
            if (client.LoggedIn)
            {
                client.Disconnect(true);
                return;
            }
            bool doLogin = false;
            lock (LoginSyncRoot)
            {
                DB.AccountTable Account = null;
                if (Kernel.AwaitingPool.TryGetValue(Connect.Identifier, out Account))
                {
                    if (!Account.MatchKey(Connect.Identifier))
                    {
                        client.Disconnect(false);
                        return;
                    }
                    client.Account = Account;
                    if (Account.EntityID == 0)
                    {
                        client.Send(new MsgTalk("NEW_ROLE", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
                        return;
                    }
                    if (Kernel.DisconnectPool.ContainsKey(Account.EntityID))
                    {
                        client.Send(new MsgTalk("Please try again after a minute!", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
                        return;
                    }
                    VariableVault variables;
                    DB.EntityVariableTable.Load(client.Account.EntityID, out variables);
                    client.Variables = variables;
                    if (client["banhours"] == 0)
                    {
                        client["banhours"] = -1;
                        client["banreason"] = "Infinite time.";
                        client["banstamp"] = DateTime.Now.AddYears(100);
                    }
                    if (Account.State == DB.AccountTable.AccountState.Banned)
                    {
                        if (client["banhours"] != -1)
                        {
                            DateTime banStamp = client["banstamp"];
                            if (DateTime.Now > banStamp.AddDays(((int)client["banhours"]) / 24).AddHours(((int)client["banhours"]) % 24))
                                Account.State = DB.AccountTable.AccountState.Player;
                        }
                    }
                    string Message = "";
                    if (Account.State == DB.AccountTable.AccountState.Banned)
                    {
                        DateTime banStamp = client["banstamp"];
                        banStamp = banStamp.AddHours(client["banhours"]);
                        Message = "You are banned for " + client["banhours"] + " hours [until " + banStamp.ToString("HH:mm MM/dd/yyyy") + "]. Reason: " + client["banreason"];
                    }
                    else if (Account.State == DB.AccountTable.AccountState.NotActivated)
                        Message = "You cannot login until your account is activated.";
                    Kernel.AwaitingPool.Remove(Connect.Identifier);
                    if (Message == string.Empty)
                    {
                        GameState aClient = null;
                        if (Kernel.GamePool.TryGetValue(Account.EntityID, out aClient))
                            aClient.Disconnect();
                        Kernel.GamePool.Remove(Account.EntityID);
                        client.Player = new Player(PlayerFlag.Monster, false);
                        Kernel.GamePool.Add(Account.EntityID, client);
                        doLogin = true;
                    }
                    else
                    {
                        client.Send(new MsgTalk(Message, "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
                        PlayerThread.Execute<GameState>((pClient, time) => { pClient.Disconnect(); }, client, 100);
                    }
                }
            }
            if (doLogin)
            {
                DoLogin(client);
            }
        }
        public static void Attack(MsgInteract attack, Client.GameState client)
        {
            client.Player.RemoveMagicDefender();
            client.Player.AttackPacket = attack;
            new Game.Attacking.Handle(attack, client.Player, null);
        }
        public static ConcurrentDictionary<string, byte[]> TreasurePointsAllowance = new ConcurrentDictionary<string, byte[]>();
        public static object TPASyncRoot = new object();
        public static void AddTPA(GameState client)
        {
            if (!TreasurePointsAllowance.ContainsKey(client.Socket.IP))
                TreasurePointsAllowance.Add(client.Socket.IP, new byte[3]);
            lock (TPASyncRoot)
            {
                byte[] data = TreasurePointsAllowance[client.Socket.IP];
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0)
                    {
                        client.AllowedTreasurePoints = true;
                        client.AllowedTreasurePointsIndex = i;
                        data[i] = 1;
                    }
                }
            }
        }
        public static void RemoveTPA(GameState client)
        {
            if (client.AllowedTreasurePoints)
            {
                lock (TPASyncRoot)
                {
                    byte[] data = TreasurePointsAllowance[client.Socket.IP];
                    data[client.AllowedTreasurePointsIndex] = 0;
                }
            }
        }
        public static void DoLogin(GameState client)
        {
            client.ReadyToPlay();
            if (DB.EntityTable.LoadEntity(client))
            {
                if (client.Player.FullyLoaded)
                {
                    AddTPA(client);
                    client.LoadData();
                    if (client.Player.GuildID != 0)
                    {
                        client.Player.GuildSharedBp = client.Guild.GetSharedBattlepower(client.Player.GuildRank);
                    }
                    client.ReviewMentor();
                    if (client.JustCreated)
                    {
                        #region AccountWhoJustCreatedItems
                        if (client.ItemGive)
                        {
                            client.Inventory.AddandWear(132013, 0, client);//Dress
                            if (client.Player.Class >= 10 && client.Player.Class <= 15)
                                client.Inventory.AddandWear(410301, 0, client);//Blade
                            if (client.Player.Class >= 20 && client.Player.Class <= 25)
                                client.Inventory.AddandWear(561301, 0, client);//Wand
                            if (client.Player.Class >= 40 && client.Player.Class <= 45)
                                client.Inventory.AddandWear(500301, 0, client);//Bow
                            if (client.Player.Class >= 50 && client.Player.Class <= 55)
                                client.Inventory.AddandWear(601301, 0, client);//Katana
                            if (client.Player.Class >= 60 && client.Player.Class <= 65)
                                client.Inventory.AddandWear(610301, 0, client);//Bead
                            if (client.Player.Class >= 70 && client.Player.Class <= 75)
                                client.Inventory.AddandWear(611301, 0, client);//Rapier
                            if (client.Player.Class >= 80 && client.Player.Class <= 85)
                                client.Inventory.AddandWear(617301, 0, client);//DragonWarriorWeapon
                            if (client.Player.Class >= 100 && client.Player.Class <= 145)
                                client.Inventory.AddandWear(421301, 0, client);//BackSword
                            if (client.Player.Class >= 160 && client.Player.Class <= 165)
                                client.Inventory.AddandWear(626003, 0, client);//Fan
                            client.ItemGive = false;
                        }
                        #endregion
                        client.ItemGive = true;
                        client.JustCreated = false;
                        Kernel.SendWorldMessage(new MsgTalk("Lets Welcome The New Player [" + client.Player.Name + "] , Has Joined Our Empire." + Kernel.GamePool.Count, System.Drawing.Color.Brown, 2012), Server.GamePool);
                        SetLocation(new MsgAction(true) { UID = client.Player.UID }, client);
                    }
                }
            }
            else
            {
                client.Send(new MsgTalk("Cannot find your character.", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
                client.Disconnect(false);
                return;
            }
            if (Kernel.GamePool.Count >= Server.PlayerCap)
            {
                client.Send(new MsgTalk("Player limit exceeded. (Online players: " + Kernel.GamePool + "/" + Server.PlayerCap + ")", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
                client.Disconnect(false);
                return;
            }
            client.Send(new MsgTalk("ANSWER_OK", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
            Server.Thread.Register(client);
            Kernel.GamePool[client.Account.EntityID] = client;
            DB.EntityTable.UpdateOnlineStatus(client, true);
            MsgUserInfo Info = new MsgUserInfo(client);
            client.Send(Info.ToArray());
            string IP = client.IP;
            client.Account.SetCurrentIP(IP);
            client.Account.Save();
            Server.UpdateConsoleTitle();
            client.LoggedIn = true;
            client.Action = 2;
        }
        public static readonly DateTime UnixEpoch = new DateTime();
        public static uint UnixTimestamp
        {
            get { return (uint)(DateTime.UtcNow - UnixEpoch).TotalSeconds; }
        }
        public static void RemoveBadSkills(GameState client)
        {
            if (client.Spells.ContainsKey(10405))
                if (!(client.Player.FirstRebornClass / 10 == client.Player.SecondRebornClass / 10 && client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 6))
                    client.RemoveSpell(new MsgMagicInfo(true) { ID = 10405 });
            if (!client.Spells.ContainsKey(10405))
                if (client.Player.FirstRebornClass / 10 == client.Player.SecondRebornClass / 10 && client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 6)
                    client.AddSpell(new MsgMagicInfo(true) { ID = 10405 });
            if (client.Spells.ContainsKey(6002))
                if (!(client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 5))
                    client.RemoveSpell(new MsgMagicInfo(true) { ID = 6002 });
            if (!((client.Player.Class >= 130 && client.Player.Class <= 135)))
            {
                client.RemoveSpell(new MsgMagicInfo(true) { ID = 1100 });
            }
            if (!((client.Player.Class >= 130 && client.Player.Class <= 135)))
                if (client.Spells.ContainsKey(30000))

                    if (client.Spells.ContainsKey(10309))
                        client.Spells.Remove(10309);
        }
        public static void LoginMessages(GameState client)
        {

            #region VIPDays
            if (client.Player.VIPLevel >= 1 && client.Player.VIPLevelDays == 0 && client.Player.VIPDays == 0)
            {
                client.Send(DefineConstantsEn_Res.VIPLifetime);
            }
            if (client.Player.VIPDays >= 1)
            {
                if (client.Player.VIPDays >= 1 && client.Player.VIPLevelDays >= 1)
                {
                    client.Player.VIPLevel = client.Player.VIPLevelDays;
                    client.Send(new MsgTalk("You can stay " + client.Player.VIPDays + " Days of Level " + client.Player.VIPLevelDays + " Of VIP.", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.World));
                }
            }
            if (client.Player.VIPDays == 0 || client.Player.VIPLevelDays == 0)
            {
                client.Player.VIPDays = 0;
                if (client.Player.VIPLevel == 0 && client.Player.VIPLevelDays >= 1)
                {
                    client.Send(DefineConstantsEn_Res.VIPExpired);
                }
            }
            #endregion  
            #region WardRobe
            client.WardRobe.Equip(client.WardRobe.MyGarment.UID, MsgItemInfo.Garment);
            client.WardRobe.Equip(client.WardRobe.MySteedArmor.UID, MsgItemInfo.SteedArmor);
            #endregion
            #region Prestige
            //client.Send(Prestige.Stats(client));
            //PerfectionRank.SendRankingQuery(new MsgRank(true) { Mode = 2 }, client, MsgRank.Prestige, PerfectionRank.FindRanking(client, 900), client.Player.Prestige);
            #endregion
            #region GameUpdates
            client.Send(new GameUpdates(GameUpdates.Mode.Header, "Welcome BakaConquer Private Project " + DateTime.Now.ToString()));
            client.Send(new GameUpdates(GameUpdates.Mode.Body, "1.Windwalker Done TQ\n"));
            client.Send(new GameUpdates(GameUpdates.Mode.Body, "2.Tournaments 90% Working\n"));
            client.Send(new GameUpdates(GameUpdates.Mode.Body, "3.Events No Officials 100%\n"));
            client.Send(new GameUpdates(GameUpdates.Mode.Body, "4.Bosses Full Fixed"));
            client.Send(new GameUpdates(GameUpdates.Mode.Body, "5.Invite Friends!"));
            client.Send(new GameUpdates(GameUpdates.Mode.Footer, "All Bug´s Has Been Fix.!"));
            #endregion
            #region SendReload
            Game.Player.SendReload(client);
            #endregion
            #region JiangHu
            MsgOwnKongfuBase.SendJiangHu(client);
            #endregion
            #region MsgTrainingInfo
            if (client.Player.MapID == 601)
            {
                MsgTrainingInfo sts = new MsgTrainingInfo(true);
                var T1 = new TimeSpan(DateTime.Now.Ticks);
                var T2 = new TimeSpan(client.OfflineTGEnterTime.Ticks);
                ushort minutes = (ushort)(T1.TotalMinutes - T2.TotalMinutes);
                minutes = (ushort)Math.Min((ushort)900, minutes);
                sts.TotalTrainingMinutesLeft = (ushort)(900 - minutes);
                sts.TrainedMinutes = minutes;
                ulong exp = client.Player.Experience;
                byte level = client.Player.Level;
                double expballGain = (double)300 * (double)minutes / (double)900;
                while (expballGain >= 100)
                {
                    expballGain -= 100;
                    exp += client.ExpBall;
                }
                if (expballGain != 0)
                    exp += (uint)(client.ExpBall * (expballGain / 100));

                while (exp >= DB.DataHolder.LevelExperience(level))
                {
                    exp -= DB.DataHolder.LevelExperience(level);
                    level++;
                }
                double percent = (double)exp * (double)100 / (double)DB.DataHolder.LevelExperience(level);

                sts.Character_NewExp = (ulong)(percent * 100000);
                sts.Character_AcquiredLevel = level;
                sts.Send(client);
            }
            #endregion
            #region SecondaryPassword
            if (client.WarehousePW != 0)
            {
                Msg2ndPsw SP = new Msg2ndPsw(true);
                SP.Action = Msg2ndPsw.Mode.PasswordCorrect;
                SP.OldPassword = 0x1;
                client.Send(SP.ToArray());
            }
            if (client.ForgetPassword)
            {
                if (client.WarehousePW != 0)
                {
                    DB.ForgetPasswordTable.Date(client);
                }
            }
            if (client.FinishForget)
            {
                MsgAction Data = new MsgAction(true);
                Data.ID = PacketMsgAction.Mode.OpenCustom;
                Data.UID = client.Player.UID;
                Data.TimeStamp = Time32.Now;
                Data.dwParam = 3391;
                Data.X = client.Player.X;
                Data.Y = client.Player.Y;
                client.Send(Data);
                client.FinishForget = false;
            }
            #endregion
            #region ElitePK
            bool going = false;
            System.Threading.Tasks.Parallel.ForEach(Game.Features.Tournaments.ElitePKTournament.Tournaments, epk =>
            {
                if (epk.State != ElitePK.States.GUI_Top8Ranking)
                    going = true;
            });
            if (going)
            {
                MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
                brackets.Type = MsgPKEliteMatchInfo.EPK_State;
                brackets.OnGoing = true;
                client.Send(brackets);
            }
            //bool going = false;
            //foreach (var epk in Game.Features.Tournaments.ElitePKTournament.Tournaments)
            //    if (epk.State != ElitePK.States.GUI_Top8Ranking)
            //        going = true;
            //if (going)
            //{
            //    MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
            //    brackets.Type = MsgPKEliteMatchInfo.EPK_State;
            //    brackets.OnGoing = true;
            //    client.Send(brackets);
            //}
            #endregion
            #region Time Server
            MsgData time = new MsgData();
            time.Year = (uint)DateTime.Now.Year;
            time.Month = (uint)DateTime.Now.Month;
            time.DayOfYear = (uint)DateTime.Now.DayOfYear;
            time.DayOfMonth = (uint)DateTime.Now.Day;
            time.Hour = (uint)DateTime.Now.Hour;
            time.Minute = (uint)DateTime.Now.Minute;
            time.Second = (uint)DateTime.Now.Second;
            client.Send(time);
            #endregion
            #region MentorInformation
            if (client.Mentor != null)
            {
                if (client.Mentor.IsOnline)
                {
                    MentorInformation Information = new MentorInformation(true);
                    Information.Mentor_Type = 1;
                    Information.Mentor_ID = client.Mentor.Client.Player.UID;
                    Information.Apprentice_ID = client.Player.UID;
                    Information.Enrole_Date = client.Mentor.EnroleDate;
                    Information.Mentor_Level = client.Mentor.Client.Player.Level;
                    Information.Mentor_Class = client.Mentor.Client.Player.Class;
                    Information.Mentor_PkPoints = client.Mentor.Client.Player.PKPoints;
                    Information.Mentor_Mesh = client.Mentor.Client.Player.Mesh;
                    Information.Mentor_Online = true;
                    Information.Shared_Battle_Power = client.Player.BattlePowerFrom(client.Mentor.Client.Player);
                    Information.String_Count = 3;
                    Information.Mentor_Name = client.Mentor.Client.Player.Name;
                    Information.Apprentice_Name = client.Player.Name;
                    Information.Mentor_Spouse_Name = client.Mentor.Client.Player.Spouse;
                    client.ReviewMentor();
                    client.Send(Information);

                    MsgGuideInfo AppInfo = new MsgGuideInfo();
                    AppInfo.Apprentice_ID = client.Player.UID;
                    AppInfo.Apprentice_Level = client.Player.Level;
                    AppInfo.Apprentice_Class = client.Player.Class;
                    AppInfo.Apprentice_PkPoints = client.Player.PKPoints;
                    AppInfo.Apprentice_Experience = client.AsApprentice.Actual_Experience;
                    AppInfo.Apprentice_Composing = client.AsApprentice.Actual_Plus;
                    AppInfo.Apprentice_Blessing = client.AsApprentice.Actual_HeavenBlessing;
                    AppInfo.Apprentice_Name = client.Player.Name;
                    AppInfo.Apprentice_Online = true;
                    AppInfo.Apprentice_Spouse_Name = client.Player.Spouse;
                    AppInfo.Enrole_date = client.Mentor.EnroleDate;
                    AppInfo.Mentor_ID = client.Mentor.ID;
                    AppInfo.Mentor_Mesh = client.Mentor.Client.Player.Mesh;
                    AppInfo.Mentor_Name = client.Mentor.Name;
                    AppInfo.Type = 2;
                    client.Mentor.Client.Send(AppInfo);
                }
                else
                {
                    MentorInformation Information = new MentorInformation(true);
                    Information.Mentor_Type = 1;
                    Information.Mentor_ID = client.Mentor.ID;
                    Information.Apprentice_ID = client.Player.UID;
                    Information.Enrole_Date = client.Mentor.EnroleDate;
                    Information.Mentor_Online = false;
                    Information.String_Count = 2;
                    Information.Mentor_Name = client.Mentor.Name;
                    Information.Apprentice_Name = client.Player.Name;
                    client.Send(Information);
                }
            }
            #endregion
            #region Nobility
            NobilityInfo update = new NobilityInfo(true);
            update.Type = NobilityInfo.Icon;
            update.dwParam = client.NobilityInformation.EntityUID;
            update.UpdateString(client.NobilityInformation);
            client.Send(update);
            #endregion
            #region ChiPowers Desativado
            //client.Send(new MsgTrainingVitalityInfo(true).Query(client));
            //MsgTrainingVitality.SendChiRankings(new MsgRank(true) { Mode = MsgRank.QueryCount }, client);
            #endregion
            #region Adding earned skills
            if (client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 9876 });
            if (client.Player.Class >= 51 && client.Player.Class <= 55 && client.Player.FirstRebornClass == 55 && client.Player.Reborn == 1)
                client.AddSpell(new MsgMagicInfo(true) { ID = 6002 });
            if (client.Player.FirstRebornClass == 15 && client.Player.SecondRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 10315 });
            if (client.Player.FirstRebornClass == 75 && client.Player.SecondRebornClass == 75 && client.Player.Class >= 71 && client.Player.Class <= 75 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 11040 });
            if (client.Player.FirstRebornClass == 25 && client.Player.SecondRebornClass == 25 && client.Player.Class >= 21 && client.Player.Class <= 25 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 10311 });
            if (client.Player.FirstRebornClass == 45 && client.Player.SecondRebornClass == 45 && client.Player.Class >= 41 && client.Player.Class <= 45 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 10313 });
            if (client.Player.FirstRebornClass == 55 && client.Player.SecondRebornClass == 55 && client.Player.Class >= 51 && client.Player.Class <= 55 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 6003 });
            if (client.Player.FirstRebornClass == 65 && client.Player.SecondRebornClass == 65 && client.Player.Class >= 61 && client.Player.Class <= 65 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 10405 });
            if (client.Player.FirstRebornClass == 135 && client.Player.SecondRebornClass == 135 && client.Player.Class >= 131 && client.Player.Class <= 135 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 30000 });
            if (client.Player.FirstRebornClass == 145 && client.Player.SecondRebornClass == 145 && client.Player.Class >= 140 && client.Player.Class <= 145 && client.Player.Reborn == 2)
                client.AddSpell(new MsgMagicInfo(true) { ID = 10310 });
            if (client.Player.Reborn == 1)
            {
                if (client.Player.FirstRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3050 });
                }
                else if (client.Player.FirstRebornClass == 25 && client.Player.Class >= 21 && client.Player.Class <= 25)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3060 });
                }
                else if (client.Player.FirstRebornClass == 145 && client.Player.Class >= 142 && client.Player.Class <= 145)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3080 });
                }
                else if (client.Player.FirstRebornClass == 135 && client.Player.Class >= 132 && client.Player.Class <= 135)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3090 });
                }
            }
            if (client.Player.Reborn == 2)
            {
                if (client.Player.SecondRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3050 });
                }
                else if (client.Player.SecondRebornClass == 25)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3060 });
                }
                else if (client.Player.SecondRebornClass == 145 && client.Player.Class >= 142 && client.Player.Class <= 145)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3080 });
                }
                else if (client.Player.SecondRebornClass == 135 && client.Player.Class >= 132 && client.Player.Class <= 135)
                {
                    client.AddSpell(new MsgMagicInfo(true) { ID = 3090 });
                }
            }
            #endregion

            #region LoginInfo
            MsgPCServerConfig mpccc = new MsgPCServerConfig(true) { Type = 15 };
            client.Send(mpccc);
            DB.EntityTable.LoginNow(client);
            client.Send(new MsgServerInfo().ToArray());
            #endregion
            #region AutoHunt
            client.Send(new MsgHangUp() { Icon = 341 }.ToArray());
            #endregion
            #region WentToComplete
            client.Filtering = true;
            if (client.WentToComplete) return;
            RemoveBadSkills(client);
            client.WentToComplete = true;
            client.Player.SendUpdates = true;
            #endregion
            #region Guild
            foreach (var Guild in Kernel.Guilds.Values)
            {
                Guild.SendName(client);
            }
            if (client.Guild != null)
            {
                client.Guild.SendAllyAndEnemy(client);
                client.Player.GuildSharedBp = client.Guild.GetSharedBattlepower(client.Player.GuildRank);
            }
            #endregion
            #region Equipment
            foreach (MsgItemInfo item in client.Inventory.Objects)
                item.Send(client);
            foreach (MsgItemInfo item in client.Equipment.Objects)
            {
                if (item != null)
                {
                    if (DB.ConquerItemInformation.BaseInformations.ContainsKey(item.ID))
                    {
                        item.Send(client);
                    }
                    else
                    {
                        client.Equipment.DestroyArrow(item.Position);
                    }
                }
            }
            client.LoadItemStats();
            if (!client.Equipment.Free(5))
            {
                if (ItemHandler.IsArrow(client.Equipment.TryGetItem(5).ID))
                {
                    if (client.Equipment.Free(4))
                        client.Equipment.DestroyArrow(5);
                    else
                    {
                        if (client.Equipment.TryGetItem(4).ID / 1000 != 500)
                            client.Equipment.DestroyArrow(5);
                    }
                }
            }
            client.GemAlgorithm();
            client.CalculateStatBonus();
            client.CalculateHPBonus();
            client.Player.Stamina = 100;
            #endregion
            #region WelcomeMessages
            string[] wm = File.ReadAllLines(InfoFile.WelcomeMessages);
            foreach (string line in wm)
            {
                if (line.Length == 0) continue;
                if (line[0] == ';') continue;
                client.Send(line);
            }
            #endregion
            #region VIPLevel
            client.Player.VIPLevel = (byte)(client.Player.VIPLevel + 0);
            if (client.Player.VIPLevel > 0)
            {
                MsgVipFunctionValidNotify vip = new MsgVipFunctionValidNotify();
                client.Send(vip.GetArray());
            }
            #endregion
            #region MapStatus
            client.Send(new MsgMapInfo() { BaseID = client.Map.BaseID, ID = client.Map.ID, Status = DB.MapsTable.MapInformations[client.Map.ID].Status, Weather = DB.MapsTable.MapInformations[client.Map.ID].Weather });
            if (client.Player.Hitpoints == 0)
            {
                ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
                client.Player.Teleport(Point[0], Point[1], Point[2]);
                client.Player.Hitpoints = 1;
            }
            #endregion
            #region MentorBattlePower
            if (client.Player.MentorBattlePower != 0)
                client.Player.Update((byte)PacketFlag.DataType.ExtraBattlePower, client.Player.MentorBattlePower, false);
            #endregion
            #region Broadcast
            if (Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityID > 2)
                client.Send(new MsgTalk(Game.ConquerStructures.Broadcast.CurrentBroadcast.Message, "ALLUSERS", Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityName, Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage, Game.ConquerStructures.Broadcast.CurrentBroadcast.UnionTitle));
            #endregion
            #region BlessTime
            client.Player.ExpProtectionTime = (ushort)(client.Player.ExpProtectionTime + (1 - 1));
            client.Player.DoubleExperienceTime = (ushort)(client.Player.DoubleExperienceTime + (1 - 1));
            client.Player.HandleTiming = true;
            client.Player.Update((byte)PacketFlag.DataType.RaceShopPoints, client.Player.RacePoints, false);
            client.Player.Update((byte)PacketFlag.DataType.LuckyTimeTimer, client.BlessTime, false);
            if (client.Player.HeavenBlessing != 0)
            {
                client.Send("Heaven Blessing Expire: " + DateTime.Now.AddSeconds(client.Player.HeavenBlessing).ToString("yyyy:MM-dd:HH"));
                client.Player.Update((byte)PacketFlag.DataType.OnlineTraining, client.OnlineTrainingPoints, false);
            }
            #endregion
            #region ClaimableItem
            if (client.ClaimableItem.Count > 0)
            {
                foreach (var item in client.ClaimableItem.Values)
                {
                    DB.ItemAddingTable.GetAddingsForItem(item.Item);
                    item.Send(client);
                    item.Item.SendExtras(client);
                }
            }
            #endregion
            #region DeatinedItem
            if (client.DeatinedItem.Count > 0)
            {
                foreach (var item in client.DeatinedItem.Values)
                {
                    DB.ItemAddingTable.GetAddingsForItem(item.Item);
                    item.Send(client);
                    item.Item.SendExtras(client);
                }
            }
            client.Equipment.UpdateEntityPacket();
            #endregion
            #region Sash
            client.Player.Update((byte)PacketFlag.DataType.AvailableSlots, 300, false);
            client.Player.Update((byte)PacketFlag.DataType.ExtraInventory, client.Player.ExtraInventory, false);
            #endregion
            #region DailySignin
            client.Send(new DailySignIn(true) { Type = DailySignIn.Action.Info, Claimed = client.SignClaim, CumulativeDays = client.CumulativeDays, LateSignChance = client.LateSignChance });
            #endregion
            #region HP & Mana
            client.Player.Hitpoints = client.Player.MaxHitpoints;
            client.Player.Mana = client.Player.MaxMana;
            #endregion
            #region Achievement
            if (client.Player.MyAchievement != null)
                client.Player.MyAchievement.Send();
            #endregion
            client.Player.UpdateEffects(true);


            #region InnerPower // Inner Desativado // 
            //if (!MaTrix.Inner.InnerPower.InnerPowerPolle.TryGetValue(client.Player.UID, out client.Player.InnerPower))
            //{
            //    client.Player.InnerPower = new MaTrix.Inner.InnerPower(client.Player.Name, client.Player.UID);
            //    Database.InnerPowerTable.New(client);
            //}
            //client.Player.InnerPower.UpdateStatus();
            //client.Player.InnerPower.AddPotency(null, client, 0);
            //client.LoadItemStats();
            #endregion
            #region Merchant
            if (client.Player.Merchant == 255)
            {
                client.Player.Update((byte)PacketFlag.DataType.Merchant, 255, false);
            }
            else if (client.Player.Merchant == 1)
            {
                MsgInteract send = new MsgInteract(true);
                send.InteractType = Network.GamePackets.MsgInteract.MerchantProgress;
                client.Send(send.ToArray());
            }
            #endregion
            WindWalker.JusticeChainEquipment(client);
        }
    }
    //public static class GeneralData
    //{
    //    public static string ReadString(byte[] data, ushort position, ushort count)
    //    {
    //        return Server.Encoding.GetString(data, position, count);
    //    }
    //    public static void WorldMessage(string message)
    //    {
    //        MsgTalk msg = new MsgTalk(message, System.Drawing.Color.MediumBlue, (uint)PacketMsgTalk.MsgTalkType.Center);
    //        foreach (Client.GameState pClient in Server.GamePool)
    //            pClient.Send(msg);
    //    }
    //    public static void ReincarnationHash(GameState client)
    //    {
    //        if (Kernel.ReincarnatedCharacters.ContainsKey(client.Player.UID))
    //        {
    //            if (client.Player.Level >= 110 && client.Player.Reborn == 2)
    //            {
    //                ushort stats = 0;
    //                uint lev1 = client.Player.Level;
    //                Game.Features.Reincarnation.ReincarnateInfo info = Kernel.ReincarnatedCharacters[client.Player.UID];
    //                client.Player.Level = info.Level;
    //                client.Player.Experience = info.Experience;
    //                Kernel.ReincarnatedCharacters.Remove(info.UID);
    //                DB.ReincarnationTable.RemoveReincarnated(client.Player);
    //                stats = (ushort)(((client.Player.Level - lev1) * 3) - 3);
    //                client.Player.Atributes += stats;
    //            }
    //        }
    //    }
    //    public static void PrintPacket(byte[] packet)
    //    {
    //        foreach (byte D in packet)
    //        {
    //            Console.Write((Convert.ToString(D, 16)).PadLeft(2, '0') + " ");
    //        }
    //        Console.Write("\n\n");
    //    }
    //    public static bool PassLearn(byte ID, Player Entity)
    //    {
    //        bool Pass = false;
    //        switch ((SubPro.ProID)ID)
    //        {
    //            case SubPro.ProID.MartialArtist:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(721259, 5))
    //                    {
    //                        Entity.Owner.Inventory.Remove(721259, 5);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.Warlock:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(721261, 10))
    //                    {
    //                        Entity.Owner.Inventory.Remove(721261, 10);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.ChiMaster:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(711188, 1))
    //                    {
    //                        Entity.Owner.Inventory.Remove(711188, 1);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.Sage:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(723087, 20))
    //                    {
    //                        Entity.Owner.Inventory.Remove(723087, 20);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.Apothecary:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(1088002, 10))
    //                    {
    //                        Entity.Owner.Inventory.Remove(1088002, 10);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.Performer:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(753003, 15) || Entity.Owner.Inventory.Contains(711679, 1))
    //                    {
    //                        if (Entity.Owner.Inventory.Contains(753003, 15))
    //                        {
    //                            Entity.Owner.Inventory.Remove(753003, 15);
    //                        }
    //                        else if (Entity.Owner.Inventory.Contains(711679, 1))
    //                        {
    //                            Entity.Owner.Inventory.Remove(711679, 1);
    //                        }
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                        break;
    //                    }
    //                    break;
    //                }
    //            case SubPro.ProID.Wrangler:
    //                {
    //                    if (Entity.Owner.Inventory.Contains(723903, 40))
    //                    {
    //                        Entity.Owner.Inventory.Remove(723903, 40);
    //                        Pass = true;
    //                        Entity.Update(MsgName.Mode.Effect, "get_special_dancer", true);
    //                    }
    //                    break;
    //                }
    //        }
    //        return Pass;
    //    }
    //    public static void ChangeAppearance(MsgAction action, GameState client)
    //    {
    //        if (client.Player.Tournament_Signed && ((Enums.AppearanceType)action.dwParam) != Enums.AppearanceType.Garment) return;
    //        action.UID = client.Player.UID;
    //        client.Player.Appearance = (Enums.AppearanceType)action.dwParam;
    //        client.SendScreen(action, true);
    //    }
    //    public static bool SwitchEquipment(GameState client, bool toAlternative)
    //    {
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Fly))
    //        {
    //            client.Send("You cannot switch equipment during flight.");
    //            return false;
    //        }
    //        if (client.Equipment.Free(MsgItemInfo.AlternateRightWeapon) && !client.Equipment.Free(MsgItemInfo.AlternateLeftWeapon))
    //        {
    //            client.Send("Invalid weapons! Missing the important weapons? Unequip the alternative left weapon.");
    //            return false;
    //        }
    //        foreach (var eq in client.Equipment.Objects)
    //        {
    //            if (eq != null)
    //            {
    //                if (!DB.ConquerItemInformation.BaseInformations.ContainsKey(eq.ID))
    //                {
    //                    client.Send("You cannot switch equipment because " + ((ItemPositionName)eq.Position).ToString().Replace("_", "~") + "'" + ((eq.Position % 20) == MsgItemInfo.Boots ? "" : "s") + " stats are not compatible with you (level or profession).");
    //                    return false;
    //                }
    //                var itemInfo = DB.ConquerItemInformation.BaseInformations[eq.ID];
    //                if (!((ItemHandler.EquipPassLvlReq(itemInfo, client) || ItemHandler.EquipPassRbReq(itemInfo, client)) && ItemHandler.EquipPassJobReq(itemInfo, client)))
    //                {
    //                    client.Send("You cannot switch equipment because " + ((ItemPositionName)eq.Position).ToString().Replace("_", "~") + "'" + ((eq.Position % 20) == MsgItemInfo.Boots ? "" : "s") + " stats are not compatible with you (level or profession).");
    //                    return false;
    //                }
    //            }
    //        }
    //        client.Player.AttackPacket = null;
    //        if (client.Player.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow))
    //            client.Player.RemoveFlag3((ulong)PacketFlag.Flags.PathOfShadow);
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Fly))
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Fly);
    //        client.AlternateEquipment = toAlternative;
    //        client.LoadItemStats();
    //        client.Player.ChangeEquip = Time32.Now;
    //        client.Equipment.UpdateEntityPacket();
    //        MsgPlayerAttriInfo Stats = new MsgPlayerAttriInfo(client);
    //        client.Send(Stats.ToArray());
    //        return true;
    //    }
    //    public static void LevelUpSpell(MsgAction action, GameState client)
    //    {
    //        ushort spellID = (ushort)action.dwParam;
    //        ISkill spell = null;
    //        if (client.Spells.TryGetValue(spellID, out spell))
    //        {
    //            var spellInfo = DB.SpellTable.GetSpell(spellID, client);
    //            if (spellInfo != null)
    //            {
    //                if (client.Trade.InTrade) return;
    //                uint CpsCost = 0;
    //                #region Costs
    //                switch (spell.Level)
    //                {
    //                    case 0: CpsCost = 27; break;
    //                    case 1: CpsCost = 81; break;
    //                    case 2: CpsCost = 122; break;
    //                    case 3: CpsCost = 181; break;
    //                    case 4: CpsCost = 300; break;
    //                    case 5: CpsCost = 400; break;
    //                    case 6: CpsCost = 500; break;
    //                    case 7: CpsCost = 600; break;
    //                    case 8: CpsCost = 800; break;
    //                    case 9: CpsCost = 1000; break;
    //                }
    //                #endregion
    //                int max = Math.Max((int)spell.Experience, 1);
    //                int percentage = 100 - (int)(max / Math.Max((spellInfo.NeedExperience / 100), 1));
    //                CpsCost = (uint)(CpsCost * percentage / 100);
    //                if (client.Player.ConquerPoints >= CpsCost)
    //                {
    //                    client.Player.ConquerPoints -= CpsCost;
    //                    spell.Level++;
    //                    if (spell.Level == spell.PreviousLevel / 2)
    //                        spell.Level = spell.PreviousLevel;
    //                    spell.Experience = 0;
    //                    spell.Send(client);
    //                }
    //            }
    //        }
    //    }
    //    public static void LevelUpProficiency(MsgAction action, GameState client)
    //    {
    //        ushort proficiencyID = (ushort)action.dwParam;
    //        IProf proficiency = null;
    //        if (client.Proficiencies.TryGetValue(proficiencyID, out proficiency))
    //        {
    //            if (proficiency.Level != 20)
    //            {
    //                if (client.Trade.InTrade) return;
    //                uint cpCost = 0;
    //                #region Costs
    //                switch (proficiency.Level)
    //                {
    //                    case 1: cpCost = 28; break;
    //                    case 2: cpCost = 28; break;
    //                    case 3: cpCost = 28; break;
    //                    case 4: cpCost = 28; break;
    //                    case 5: cpCost = 28; break;
    //                    case 6: cpCost = 55; break;
    //                    case 7: cpCost = 81; break;
    //                    case 8: cpCost = 135; break;
    //                    case 9: cpCost = 162; break;
    //                    case 10: cpCost = 270; break;
    //                    case 11: cpCost = 324; break;
    //                    case 12: cpCost = 324; break;
    //                    case 13: cpCost = 324; break;
    //                    case 14: cpCost = 324; break;
    //                    case 15: cpCost = 375; break;
    //                    case 16: cpCost = 548; break;
    //                    case 17: cpCost = 799; break;
    //                    case 18: cpCost = 1154; break;
    //                    case 19: cpCost = 1420; break;
    //                }
    //                #endregion
    //                uint needExperience = DB.DataHolder.ProficiencyLevelExperience(proficiency.Level);
    //                int max = Math.Max((int)proficiency.Experience, 1);
    //                int percentage = 100 - (int)(max / (needExperience / 100));
    //                cpCost = (uint)(cpCost * percentage / 100);
    //                if (client.Player.ConquerPoints >= cpCost)
    //                {
    //                    client.Player.ConquerPoints -= cpCost;
    //                    proficiency.Level++;
    //                    if (proficiency.Level == proficiency.PreviousLevel / 2)
    //                    {
    //                        proficiency.Level = proficiency.PreviousLevel;
    //                        DB.DataHolder.ProficiencyLevelExperience((byte)(proficiency.Level + 1));
    //                    }
    //                    proficiency.Experience = 0;
    //                    proficiency.Send(client);
    //                }
    //                else
    //                {
    //                    if (client.Player.BoundCps >= cpCost)
    //                    {
    //                        client.Player.BoundCps -= cpCost;
    //                        proficiency.Level++;
    //                        if (proficiency.Level == proficiency.PreviousLevel / 2)
    //                        {
    //                            proficiency.Level = proficiency.PreviousLevel;
    //                            DB.DataHolder.ProficiencyLevelExperience((byte)(proficiency.Level + 1));
    //                        }
    //                        proficiency.Experience = 0;
    //                        proficiency.Send(client);
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            //break;
    //        }
    //        return;
    //    }
    //    public static void Revive(MsgAction action, GameState client)
    //    {
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.SoulShackle)) return;
    //        if (Time32.Now >= client.Player.DeathStamp.AddSeconds(18) && client.Player.Dead)
    //        {
    //            client.Player.Action = Enums.ConquerAction.None;
    //            client.ReviveStamp = Time32.Now;
    //            client.Attackable = false;
    //            client.Player.TransformationID = 0;
    //            client.Player.AutoRev = 0;
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Dead);
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Ghost);
    //            client.Player.Hitpoints = client.Player.MaxHitpoints;
    //            if (client.Player.MapID == 1518)
    //            {
    //                client.Player.Teleport(1002, 300, 278);
    //                return;
    //            }
    //            bool ReviveHere = action.dwParam == 1;
    //            if (client.Spells.ContainsKey(12660))
    //            {
    //                client.XPCount = client.Player.XPCountTwist;
    //            }
    //            if (client.Player.MapID == 1038 && DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
    //            {
    //                client.Player.Teleport(1002, 300, 278);
    //            }
    //            else if (ReviveHere && client.Player.HeavenBlessing > 0 && !Constants.NoRevHere.Contains(client.Player.MapID))
    //            {
    //                if (client.Player.MapID == Pezzi.ServerEvents.LastManStanding.ID || client.Player.MapID == Pezzi.ServerEvents.IronMap.ID || client.Player.MapID == Pezzi.ServerEvents.DailyMap.ID || client.Player.MapID == Pezzi.ServerEvents.ExtremePk.ID) return;
    //                client.Send(new MsgMapInfo()
    //                {
    //                    BaseID = client.Map.BaseID,
    //                    ID = client.Map.ID,
    //                    Status = DB.MapsTable.MapInformations[client.Map.ID].Status
    //                });
    //            }
    //            else
    //            {
    //                ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
    //                client.Player.Teleport(Point[0], Point[1], Point[2]);
    //            }
    //        }
    //    }
    //    public static void UsePortal(MsgAction action, GameState client)
    //    {
    //        client.Player.Action = Enums.ConquerAction.None;
    //        client.ReviveStamp = Time32.Now;
    //        client.Attackable = false;
    //        ushort portal_X = (ushort)(action.dwParam & 65535);
    //        ushort portal_Y = (ushort)(action.dwParam >> 16);
    //        string portal_ID = portal_X.ToString() + ":" + portal_Y.ToString() + ":" + client.Map.ID.ToString();
    //        if (client.Account.State == DB.AccountTable.AccountState.Administrator)
    //            client.Send("Portal ID: " + portal_ID);
    //        foreach (Game.Portal portal in client.Map.Portals)
    //        {
    //            if (Kernel.GetDistance(portal.CurrentX, portal.CurrentY, client.Player.X, client.Player.Y) <= 4)
    //            {
    //                client.Player.PrevX = client.Player.X;
    //                client.Player.PrevY = client.Player.Y;
    //                client.Player.Teleport(portal.DestinationMapID, portal.DestinationX, portal.DestinationY);
    //                return;
    //            }
    //        }
    //        client.Player.Teleport(1002, 300, 278);
    //    }
    //    public static void ObserveEquipment(MsgAction action, GameState client)
    //    {
    //        if (ItemHandler.NulledClient(client)) return;
    //        GameState Observer, Observee;
    //        if (Kernel.GamePool.TryGetValue(action.UID, out Observer) && Kernel.GamePool.TryGetValue(action.dwParam, out Observee))
    //        {
    //            if (action.ID != PacketMsgAction.Mode.ObserveEquipment)
    //                Observer.Send(Observee.Player.WindowSpawn());
    //            MsgPlayerAttriInfo Stats = new MsgPlayerAttriInfo(Observee);
    //            Observer.Send(Stats.ToArray());
    //            for (Byte pos = (Byte)MsgItemInfo.Head; pos <= MsgItemInfo.AlternateGarment; pos++)
    //            {
    //                MsgItemInfo i = Observee.Equipment.TryGetItem((Byte)pos);
    //                if (i != null)
    //                {
    //                    if (i.IsWorn)
    //                    {
    //                        MsgItemInfoEx2 view = new MsgItemInfoEx2();
    //                        view.CostType = MsgItemInfoEx2.CostTypes.ViewEquip;
    //                        view.Identifier = Observee.Player.UID;
    //                        view.Position = (ItemHandler.Positions)(pos % 20);
    //                        view.ParseItem(i);
    //                        Observer.Send(view);
    //                        i.SendExtras(client);
    //                    }
    //                }
    //            }
    //            if (Observee.WardRobe != null)
    //            {
    //                MsgItemInfoEx2 view = new MsgItemInfoEx2();
    //                view.CostType = MsgItemInfoEx2.CostTypes.ViewEquip;
    //                view.Identifier = Observee.Player.UID;

    //                var item = Observee.WardRobe.MyGarment.Item;
    //                if (item != null)
    //                {
    //                    view.Position = ItemHandler.Positions.Garment;
    //                    view.ParseItem(item);
    //                    Observer.Send(view);
    //                    item.SendExtras(client);
    //                }
    //                item = Observee.WardRobe.MySteedArmor.Item;
    //                if (item != null)
    //                {
    //                    view.Position = ItemHandler.Positions.SteedArmor;
    //                    view.ParseItem(item);
    //                    Observer.Send(view);
    //                    item.SendExtras(client);
    //                }
    //            }
    //            MsgName Name = new MsgName(true);
    //            Name.Action = MsgName.Mode.QuerySpouse;
    //            Name.UID = client.Player.UID;
    //            Name.TextsCount = 1;
    //            Name.Texts = new List<string>()
    //            {
    //                Observee.Player.Spouse
    //            };
    //            Observer.Send(Name);
    //            if (action.ID == PacketMsgAction.Mode.ObserveEquipment)
    //            {
    //                Name.Action = MsgName.Mode.Effect;
    //                Observer.Send(Name);
    //            }
    //            Observer.Send(action);
    //            Observee.Send(Observer.Player.Name + " is checking your equipment");
    //        }
    //    }
    //    public static void ChangeFace(MsgAction action, GameState client)
    //    {
    //        if (client.Player.Money >= 500)
    //        {
    //            uint newface = action.dwParam;
    //            if (client.Player.Body > 2000)
    //            {
    //                newface = newface < 200 ? newface + 200 : newface;
    //                client.Player.Face = (ushort)newface;
    //            }
    //            else
    //            {
    //                newface = newface > 200 ? newface - 200 : newface;
    //                client.Player.Face = (ushort)newface;
    //            }
    //        }
    //    }
    //    public static void CheckForRaceItems(GameState client)
    //    {
    //        StaticEntity item;
    //        if (client.Screen.GetRaceObject(p => { return Kernel.GetDistance(client.Player.X, client.Player.Y, p.X, p.Y) <= 1; }, out item))
    //        {
    //            if (item == null) return;
    //            if (!item.Viable) return;
    //            var type = item.Type;
    //            bool successful = false;
    //            if (type == RaceItemType.FrozenTrap && !item.QuestionMark)
    //            {
    //                if (item.SetBy != client.Player.UID)
    //                {
    //                    client.ApplyRacePotion(type, uint.MaxValue);
    //                    client.Map.RemoveStaticItem(item);
    //                    successful = true;
    //                }
    //            }
    //            else
    //            {
    //                if (client.Potions == null) client.Potions = new UsableRacePotion[5];
    //                for (ushort i = 0; i < client.Potions.Length; i++)
    //                {
    //                    var pot = client.Potions[i];
    //                    if (pot == null)
    //                    {
    //                        pot = (client.Potions[i] = new UsableRacePotion());
    //                        pot.Type = type;
    //                        pot.Count = item.Level;
    //                        client.Send(new MsgRaceTrackProp(true)
    //                        {
    //                            PotionType = type,
    //                            Amount = (ushort)pot.Count,
    //                            Location = (ushort)(i + 1)
    //                        });
    //                        successful = true;
    //                        break;
    //                    }
    //                    else if (pot.Type == type)
    //                    {
    //                        pot.Count += item.Level;
    //                        client.Send(new MsgRaceTrackProp(true)
    //                        {
    //                            PotionType = type,
    //                            Amount = (ushort)pot.Count,
    //                            Location = (ushort)(i + 1)
    //                        });
    //                        successful = true;
    //                        break;
    //                    }
    //                }
    //            }
    //            if (successful)
    //            {
    //                client.SendScreen(new MsgName(true)
    //                {
    //                    Texts = new List<string>() { "eidolon" },
    //                    UID = client.Player.UID,
    //                    Action = MsgName.Mode.Effect
    //                });
    //                client.RemoveScreenSpawn(item, true);
    //                item.Viable = false;
    //                item.NotViableStamp = Time32.Now;
    //            }
    //        }
    //    }
    //    public static void PlayerJump(MsgAction action, GameState client)
    //    {

    //        if (client.Player.Dead || client.Player.ContainsFlag(((ulong)PacketFlag.Flags.Dead)) || client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ghost))
    //        {
    //            return;
    //        }
    //        if (client.Companion != null)
    //        {
    //            if (client.Companion.UID == action.UID)
    //            {
    //                PetsControler.datajump(client, action);

    //                return;
    //            }
    //        }
    //        client.Player.KillCount2 = 0;
    //        client.Player.SpiritFocus = false;
    //        ushort oldX = client.Player.X;
    //        ushort oldY = client.Player.Y;
    //        client.Player.RemoveFlag((ulong)PacketFlag.Flags.Intensify);
    //        client.Player.IntensifyPercent = 0;
    //        if (client.Player.MapID == 1927 && Kernel.SpawnBanshee2)
    //        {
    //            foreach (INpc Npc in client.Map.Npcs.Values)
    //            {
    //                if (Npc.MapID == 1927 && (Npc.UID == 2999) && Kernel.GetDistance(client.Player.X, client.Player.Y, Npc.X, Npc.Y) < 17)
    //                {
    //                    Npc.SendSpawn(client);
    //                }
    //            }
    //        }
    //        client.Player.Action = Enums.ConquerAction.None;
    //        client.Mining = false;
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
    //        {
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
    //            foreach (var Client in client.Prayers)
    //            {
    //                if (Client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
    //                {
    //                    Client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
    //                }
    //            }
    //            client.Prayers.Clear();
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
    //        {
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
    //            client.PrayLead = null;
    //        }
    //        Time32 Now = Time32.Now;
    //        client.Attackable = true;
    //        if (client.Player.AttackPacket != null)
    //        {
    //            client.Player.AttackPacket = null;
    //        }
    //        if (client.Player.Dead)
    //        {
    //            if (Now > client.Player.DeathStamp.AddSeconds(4))
    //            {
    //                client.Disconnect();
    //                return;
    //            }
    //        }
    //        ushort new_X = Core.BitConverter.ToUInt16(action.ToArray(), 12);
    //        ushort new_Y = Core.BitConverter.ToUInt16(action.ToArray(), 14);
    //        if (client.lastJumpDistance == 0) goto Jump;
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
    //        {
    //            int distance = Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y);
    //            ushort take = (ushort)(1.5F * (distance / 2));
    //            if (client.Vigor >= take)
    //            {
    //                client.Vigor -= take;
    //                Vigor vigor = new Vigor(true);
    //                vigor.Amount = client.Vigor;
    //                vigor.Send(client);
    //            }
    //            else
    //            {
    //            }
    //        }
    //        client.LastJumpTime = (int)Kernel.maxJumpTime(client.lastJumpDistance);
    //        //var serverstamp = Now.GetHashCode() - client.lastJumpTime.GetHashCode();
    //        //var clientstamp = action.TimeStamp.GetHashCode() - client.lastClientJumpTime.GetHashCode();
    //        //var speed = clientstamp - serverstamp;
    //        //if (speed > 100)
    //        //{
    //        //    client.speedHackSuspiction++;
    //        //    if (!client.Player.OnCyclone() && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride) && !client.Player.OnOblivion() && !client.Player.OnSuperman() && !client.Player.ContainsFlag3((ulong)PacketFlag.Flags.DragonCyclone) && !client.Player.OnSuperCyclone() && !client.Player.Transformed && client.speedHackSuspiction >= 3)
    //        //    {
    //        //        client.Disconnect();
    //        //    }
    //        //}
    //        //else
    //        //{
    //        //    client.speedHackSuspiction = Math.Max(0, client.speedHackSuspiction - 1);
    //        //}
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
    //        {
    //            client.Player.LastTimeUseSlide = Time32.Now;
    //        }
    //        if (Now < client.lastJumpTime.AddMilliseconds(client.LastJumpTime))
    //        {
    //            bool doDisconnect = false;
    //            if (client.Player.Transformed)
    //                if (client.Player.TransformationID != 207 && client.Player.TransformationID != 267)
    //                    doDisconnect = true;
    //            if (client.Player.Transformed && doDisconnect)
    //            {

    //            }
    //            if (client.Player.Transformed && !doDisconnect)
    //            {
    //                goto Jump;
    //            }
    //            else if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
    //            {
    //                int time = (int)Kernel.maxJumpTime(client.lastJumpDistance);
    //                int speedprc = DB.DataHolder.SteedSpeed(client.Equipment.TryGetItem(MsgItemInfo.Steed).Plus);
    //                if (speedprc != 0)
    //                {
    //                    if (Now < client.lastJumpTime.AddMilliseconds(time - (time * speedprc / 100)))
    //                    {

    //                    }
    //                }
    //                else
    //                {

    //                }
    //            }
    //        }
    //    Jump:
    //        client.lastJumpDistance = Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y);
    //        client.lastClientJumpTime = action.TimeStamp;
    //        client.lastJumpTime = Now;
    //        Game.Map Map = client.Map;
    //        if (Map != null)
    //        {
    //            if (Map.Floor[new_X, new_Y, Game.MapObjectType.Player, null])
    //            {
    //                if (Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y) <= 20)
    //                {
    //                    client.Player.Action = Enums.ConquerAction.Jump;
    //                    client.Player.Facing = Kernel.GetAngle(action.X, action.Y, new_X, new_Y);
    //                    client.Player.PX = client.Player.X;
    //                    client.Player.PY = client.Player.Y;
    //                    client.Player.X = new_X;
    //                    client.Player.Y = new_Y;
    //                    if (client.Player.MapID == MsgWarFlag.MapID)
    //                        CheckForFlag(client);
    //                    client.SendScreen(action, true);
    //                    client.Screen.Reload(action);
    //                    if (client.Player.MapID == 1351)
    //                    {
    //                        if (new_X == 14 && new_Y == 122)//stig
    //                        {
    //                            client.Player.Teleport(1002, 309, 338);
    //                        }
    //                    }
    //                    if (client.Player.InteractionInProgress && client.Player.InteractionSet)
    //                    {
    //                        if (client.Player.Body == 1003 || client.Player.Body == 1004)
    //                        {
    //                            if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
    //                            {
    //                                GameState ch = Kernel.GamePool[client.Player.InteractionWith];
    //                                Network.GamePackets.MsgAction general = new Network.GamePackets.MsgAction(true);
    //                                general.UID = ch.Player.UID;
    //                                general.X = new_X;
    //                                general.Y = new_Y;
    //                                general.ID = (PacketMsgAction.Mode)156;
    //                                ch.Send(general.ToArray());
    //                                ch.Player.Action = Enums.ConquerAction.Jump;
    //                                ch.Player.X = new_X;
    //                                ch.Player.Y = new_Y;
    //                                ch.Player.Facing = Kernel.GetAngle(ch.Player.X, ch.Player.Y, new_X, new_Y);
    //                                ch.SendScreen(action, true);
    //                                ch.Screen.Reload(general);
    //                                client.SendScreen(action, true);
    //                                client.Screen.Reload(general);
    //                            }
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    client.Disconnect();
    //                }
    //            }
    //            else
    //            {
    //                if (client.Player.Mode == Enums.Mode.None)
    //                {
    //                    client.Player.Teleport(client.Map.ID, client.Player.X, client.Player.Y);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (Kernel.GetDistance(new_X, new_Y, client.Player.X, client.Player.Y) <= 20)
    //            {
    //                client.Player.Action = Enums.ConquerAction.Jump;
    //                client.Player.Facing = Kernel.GetAngle(action.X, action.Y, new_X, new_Y);
    //                client.Player.X = new_X;
    //                client.Player.Y = new_Y;
    //                client.SendScreen(action, true);
    //                client.Screen.Reload(action);
    //            }
    //            else
    //            {
    //                client.Disconnect();
    //            }
    //        }
    //        if (client.Map.BaseID == 1038 && Game.GuildWar.IsWar || client.Map.BaseID == 10380 && Game.Kingdom.IsWar)
    //        {
    //            Game.Calculations.IsBreaking(client, oldX, oldY);
    //        }
    //        if (!client.Player.HasMagicDefender)
    //        {
    //            if (client.Team != null)
    //            {
    //                var owners = client.Team.Teammates.Where(x => x.Player.MagicDefenderOwner);
    //                if (owners != null)
    //                {
    //                    foreach (var owner in owners)
    //                    {
    //                        if (Kernel.GetDistance(client.Player.X, client.Player.Y, owner.Player.X, owner.Player.Y) <= 4)
    //                        {
    //                            client.Player.HasMagicDefender = true;
    //                            client.Player.MagicDefenderStamp = Time32.Now;
    //                            client.Player.MagicDefenderSecs = (byte)(owner.Player.MagicDefenderStamp.AddSeconds(owner.Player.MagicDefenderSecs) - owner.Player.MagicDefenderStamp).AllSeconds();
    //                            client.Player.AddFlag3((ulong)PacketFlag.Flags.MagicDefender);
    //                            MsgUpdate upgrade = new MsgUpdate(true);
    //                            upgrade.UID = client.Player.UID;
    //                            upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, client.Player.MagicDefenderSecs, 0, 0);
    //                            client.Send(upgrade.ToArray());
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            client.Player.RemoveMagicDefender();
    //        }
    //    }
    //    private static void CheckForFlag(GameState client)
    //    {
    //        if (client.Player.GuildID == 0) return;
    //        foreach (var item in client.Map.StaticEntities.Values)
    //        {
    //            if (Kernel.GetDistance(item.X, item.Y, client.Player.X, client.Player.Y) == 0)
    //            {
    //                client.Player.FlagStamp = Time32.Now;
    //                client.Send(Server.Thread.CTF.generateTimer(60));
    //                client.Send(Server.Thread.CTF.generateEffect(client));
    //                MsgWarFlag.AddExploits(3, client.AsMember);
    //                client.Guild.CTFPoints += 3;
    //                client.Player.AddFlag2((ulong)PacketFlag.Flags.CarryingFlag);
    //                MsgWarFlag.SendScores();
    //                client.Map.RemoveStaticItem(item);
    //                client.RemoveScreenSpawn(item, true);
    //            }
    //            else
    //            {
    //                Server.Thread.CTF.AroundBase(client);
    //            }
    //        }
    //    }
    //    public static void PlayerGroundMovment(GroundMovement groundMovement, Client.GameState client)
    //    {
    //        client.Player.SpellStamp = Time32.Now.AddSeconds(-1);
    //        client.Player.KillCount2 = 0;
    //        client.Player.Action = Enums.ConquerAction.None;
    //        client.Attackable = true;
    //        client.Mining = false;
    //        var oldX = client.Player.X;
    //        var oldY = client.Player.Y;
    //        if (client.Companion != null)//PETS
    //        {
    //            if (groundMovement.UID == client.Companion.UID)
    //            {

    //                PetsControler.MovementPacket(client, groundMovement);
    //                return;
    //            }
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
    //        {
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
    //            foreach (var Client in client.Prayers)
    //            {
    //                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
    //                {
    //                    client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
    //                }
    //            }
    //            client.Prayers.Clear();
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Praying))
    //        {
    //            client.Player.RemoveFlag((ulong)PacketFlag.Flags.Praying);
    //            if (client.PrayLead != null)
    //                client.PrayLead.Prayers.Remove(client);
    //            client.PrayLead = null;
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
    //        {
    //            client.Player.LastTimeUseSlide = Time32.Now;
    //            client.Player.Vigor -= 1;
    //            Network.GamePackets.Vigor vigor = new Network.GamePackets.Vigor(true);
    //            vigor.Amount = client.Player.Vigor;
    //            vigor.Send(client);
    //        }
    //        if (client.Player.AttackPacket != null)
    //        {
    //            client.Player.AttackPacket = null;
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride)) client.Vigor -= 1;
    //        client.Player.PX = client.Player.X;
    //        client.Player.PY = client.Player.Y;

    //        if (!client.Player.Move(groundMovement.Direction, groundMovement.GroundMovementType == GroundMovement.Slide)) return;

    //        if (client.Player.MapID == Game.SteedRace.MAPID)
    //        {
    //            CheckForRaceItems(client);
    //            if (!(DateTime.Now.Hour == 14 && DateTime.Now.Minute <= 25))
    //            {
    //                if (client.Player.X <= Server.Thread.SteedRace.GateX + 1)
    //                {
    //                    client.Player.Teleport(client.Player.MapID, client.Player.X, client.Player.Y);
    //                    return;
    //                }
    //            }
    //        }
    //        if (client.Player.MapID == Game.SteedRace.MAPID)
    //            CheckForRaceItems(client);
    //        if (client.Player.MapID == MsgWarFlag.MapID)
    //            CheckForFlag(client);

    //        client.SendScreen(groundMovement, true);
    //        client.Screen.Reload(groundMovement);

    //        if (client.Player.InteractionInProgress)
    //        {
    //            if (!client.Player.InteractionSet)
    //            {
    //                if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
    //                {
    //                    Client.GameState ch = Kernel.GamePool[client.Player.InteractionWith];
    //                    if (ch.Player.InteractionInProgress && ch.Player.InteractionWith == client.Player.UID)
    //                    {
    //                        if (client.Player.InteractionX == client.Player.X && client.Player.Y == client.Player.InteractionY)
    //                        {
    //                            if (client.Player.X == ch.Player.X && client.Player.Y == ch.Player.Y)
    //                            {
    //                                MsgInteract atac = new Network.GamePackets.MsgInteract(true);
    //                                atac.Attacker = ch.Player.UID;
    //                                atac.Attacked = client.Player.UID;
    //                                atac.X = ch.Player.X;
    //                                atac.Y = ch.Player.Y;
    //                                atac.Damage = client.Player.InteractionType;
    //                                atac.ResponseDamage = client.InteractionEffect;
    //                                atac.InteractType = 47;
    //                                ch.Send(atac);

    //                                atac.InteractType = 49;
    //                                atac.Attacker = client.Player.UID;
    //                                atac.Attacked = ch.Player.UID;
    //                                client.SendScreen(atac, true);

    //                                atac.Attacker = ch.Player.UID;
    //                                atac.Attacked = client.Player.UID;
    //                                client.SendScreen(atac, true);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                if (client.Player.Body == 1003 || client.Player.Body == 1004)
    //                {
    //                    if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
    //                    {
    //                        Client.GameState ch = Kernel.GamePool[client.Player.InteractionWith];

    //                        ch.Player.Facing = groundMovement.Direction;
    //                        ch.Player.Move(groundMovement.Direction);
    //                        MsgAction general = new MsgAction(true);
    //                        general.UID = ch.Player.UID;
    //                        general.wParam1 = ch.Player.X;
    //                        general.wParam2 = ch.Player.Y;
    //                        general.ID = (PacketMsgAction.Mode)0x9c;
    //                        ch.Send(general.ToArray());
    //                        ch.Screen.Reload(null);
    //                    }
    //                }
    //            }
    //        }
    //        if (client.Map.BaseID == 1038 && Game.GuildWar.IsWar)
    //        {
    //            Game.Calculations.IsBreaking(client, oldX, oldY);
    //        }
    //        if (!client.Player.HasMagicDefender)
    //        {
    //            if (client.Team != null)
    //            {
    //                var owners = client.Team.Teammates.Where(x => x.Player.MagicDefenderOwner);
    //                if (owners != null)
    //                {
    //                    foreach (var owner in owners)
    //                    {
    //                        if (Kernel.GetDistance(client.Player.X, client.Player.Y, owner.Player.X, owner.Player.Y) <= 4)
    //                        {
    //                            client.Player.HasMagicDefender = true;
    //                            client.Player.MagicDefenderStamp = Time32.Now;
    //                            client.Player.MagicDefenderSecs = (byte)(owner.Player.MagicDefenderStamp.AddSeconds(owner.Player.MagicDefenderSecs) - owner.Player.MagicDefenderStamp).AllSeconds();
    //                            client.Player.AddFlag3((ulong)PacketFlag.Flags.MagicDefender);
    //                            MsgUpdate upgrade = new MsgUpdate(true);
    //                            upgrade.UID = client.Player.UID;
    //                            upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, client.Player.MagicDefenderSecs, 0, 0);
    //                            client.Send(upgrade.ToArray());
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            client.Player.RemoveMagicDefender();
    //        }
    //    }
    //    public static void GetSurroundings(GameState client)
    //    {
    //        client.Screen.FullWipe();
    //        client.Screen.Reload(null);
    //        if (client.Player.PreviousMapID == MsgWarFlag.MapID)
    //            Server.Thread.CTF.CloseList(client);
    //    }
    //    public static void ChangeAction(MsgAction action, GameState client)
    //    {
    //        client.Player.Action = (ushort)action.dwParam;
    //        if (client.Companion != null)
    //        {
    //            if (action.UID == client.Companion.UID)
    //            {

    //                PetsControler.ChangeActions(client, action);
    //                return;
    //            }
    //        }
    //        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
    //        {
    //            foreach (var Client in client.Prayers)
    //            {
    //                action.UID = Client.Player.UID;
    //                action.dwParam = (uint)client.Player.Action;
    //                action.X = Client.Player.X;
    //                action.Y = Client.Player.Y;
    //                Client.Player.Action = client.Player.Action;
    //                if (Time32.Now >= Client.CoolStamp.AddMilliseconds(1500))
    //                {
    //                    if (Client.Equipment.IsAllSuper())
    //                        action.dwParam = (uint)(action.dwParam | (uint)(Client.Player.Class * 0x10000 + 0x1000000));
    //                    else if (Client.Equipment.IsArmorSuper())
    //                        action.dwParam = (uint)(action.dwParam | (uint)(Client.Player.Class * 0x10000));
    //                    Client.SendScreen(action, true);
    //                    Client.CoolStamp = Time32.Now;
    //                }
    //                else
    //                    Client.SendScreen(action, false);
    //            }
    //        }
    //        action.UID = client.Player.UID;
    //        action.dwParam = (uint)client.Player.Action;
    //        if (client.Player.Action == Enums.ConquerAction.Cool)
    //        {
    //            if (Time32.Now >= client.CoolStamp.AddMilliseconds(1500))
    //            {
    //                if (client.Equipment.IsAllSuper())
    //                    action.dwParam = (uint)(action.dwParam | (uint)(client.Player.Class * 0x10000 + 0x1000000));
    //                else if (client.Equipment.IsArmorSuper())
    //                    action.dwParam = (uint)(action.dwParam | (uint)(client.Player.Class * 0x10000));
    //                client.SendScreen(action, true);
    //                client.CoolStamp = Time32.Now;
    //            }
    //            else
    //                client.SendScreen(action, false);
    //        }
    //        else
    //            client.SendScreen(action, false);
    //    }
    //    public static void ChangeDirection(MsgAction action, GameState client)
    //    {
    //        client.Player.Facing = (Enums.ConquerAngle)action.Facing;
    //        client.SendScreen(action, false);
    //    }
    //    public static void ChangePKMode(MsgAction action, GameState client)
    //    {
    //        if (client.Player.PKMode == PKMode.Kongfu)
    //        {
    //            if ((PKMode)(byte)action.dwParam != PKMode.Kongfu)
    //            {
    //                client.Send("You`ll quit the Jiang Hu in 10 minutes.");
    //            }
    //        }
    //        if (client.InTeamQualifier()) return;
    //        if (client.InQualifier()) return;
    //        client.Player.AttackPacket = null;
    //        client.Player.PKMode = (PKMode)(byte)action.dwParam;
    //        client.Send(action);
    //        if ((client.Player.PKMode == PKMode.Kongfu) && (client.Player.MyKongFu != null))
    //        {
    //            client.Player.MyKongFu.OnJiangMode = true;
    //            client.Player.MyKongFu.SendStatusMode(client);
    //        }
    //        if (client.Player.PKMode == PKMode.PK)
    //        {
    //            client.Send("Free PK mode: you can attack monsters and all Players.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Peace)
    //        {
    //            client.Send("Peace mode: You can only attack monsters.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Team)
    //        {
    //            client.Send("Team mode: slay monsters, and all other players (including cross-server players) not in your current team or guild. ");
    //        }
    //        else if (client.Player.PKMode == PKMode.Capture)
    //        {
    //            client.Send("Capture mode: Slay monsters, black/blue-name criminals, and cross-server players.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Revenge)
    //        {
    //            client.Send("revenge mode: Slay your listed enemies, monsters, and cross-server players.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Union)
    //        {
    //            client.Send("The `Plander` mode only allow you to other players in enemy Union.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Guild)
    //        {
    //            client.Send("Guild mode: Slay monsters, and players in your enemy guilds, and cross-server players.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Kongfu)
    //        {
    //            client.Send("Jiang Hu mode: Slay Jiang Hu fighters, black/blue-name criminals, and cross-server players.");
    //        }
    //        else if (client.Player.PKMode == PKMode.CS)
    //        {
    //            client.Send("CS (Cross-Server) mode: Attack cross-server players. No Pk punishment.");
    //        }
    //        else if (client.Player.PKMode == PKMode.Invade)
    //        {
    //            client.Send("Invade mode: Only attack players of the target (current) server No Pk punishment.");
    //        }
    //    }
    //    public static void SetLocation(MsgAction action, GameState client)
    //    {
    //        if (client.Player.MyKongFu != null)
    //        {
    //            client.Player.MyKongFu.OnloginClient(client);
    //        }
    //        else if (client.Player.Reborn == 2)
    //        {
    //            MsgOwnKongfuBase hu = new MsgOwnKongfuBase
    //            {
    //                Texts = { "0" }
    //            };
    //            hu.CreateArray();
    //            hu.Send(client);
    //        }
    //        SendFlower sendFlower = new SendFlower();
    //        sendFlower.Typing = (Flowers.IsBoy((uint)client.Player.Body) ? 3u : 2u);
    //        sendFlower.Apprend(client.Player.MyFlowers);
    //        client.Send(sendFlower.ToArray());
    //        if (client.Player.MyFlowers.aFlower > 0u)
    //        {
    //            client.Send(new SendFlower
    //            {
    //                Typing = Flowers.IsBoy((uint)client.Player.Body) ? 2u : 3u
    //            }.ToArray());
    //        }
    //        if (client.Guild != null)
    //        {
    //            client.Guild.SendGuild(client);
    //            MsgDutyMinContri guild = new MsgDutyMinContri(31);
    //            guild.AprendGuild(client.Guild);
    //            client.Send(guild.ToArray());
    //        }
    //        MsgFamily clan = client.Player.GetClan;
    //        if (clan != null)
    //        {
    //            clan.Build(client, MsgFamily.Types.Info);
    //            client.Send(clan);
    //            client.Player.ClanName = clan.Name;
    //            client.Send(new MsgFamilyRelation(clan, MsgFamilyRelation.RelationTypes.Allies));
    //            client.Send(new MsgFamilyRelation(clan, MsgFamilyRelation.RelationTypes.Enemies));
    //        }

    //        foreach (Game.ConquerStructures.Society.Guild guild in Kernel.Guilds.Values)
    //        {
    //            guild.SendName(client);
    //            guild.SendName(client);
    //        }

    //        if (client.Player.EnlightmentTime > 0)
    //        {
    //            MsgMentorPlayer enlight = new MsgMentorPlayer(true);
    //            enlight.Enlighted = client.Player.UID;
    //            enlight.Enlighter = 0;

    //            if (client.Player.EnlightmentTime > 80)
    //                client.Player.EnlightmentTime = 100;
    //            else if (client.Player.EnlightmentTime > 60)
    //                client.Player.EnlightmentTime = 80;
    //            else if (client.Player.EnlightmentTime > 40)
    //                client.Player.EnlightmentTime = 60;
    //            else if (client.Player.EnlightmentTime > 20)
    //                client.Player.EnlightmentTime = 40;
    //            else if (client.Player.EnlightmentTime > 0)
    //                client.Player.EnlightmentTime = 20;
    //            for (int count = 0; count < client.Player.EnlightmentTime; count += 20)
    //            {
    //                client.Send(enlight.ToArray());
    //            }
    //        }

    //        if (client.Player.Hitpoints != 0)
    //        {
    //            if (client.Map.ID == 1036 || client.Map.ID == 1039)
    //            {
    //                if (client.Player.PreviousMapID == 0)
    //                    client.Player.SetLocation(1002, 300, 278);
    //                else
    //                {
    //                    switch (client.Player.PreviousMapID)
    //                    {
    //                        default:
    //                            {
    //                                client.Player.SetLocation(1002, 300, 278);
    //                                break;
    //                            }
    //                        case 1000:
    //                            {
    //                                client.Player.SetLocation(1000, 500, 650);
    //                                break;
    //                            }
    //                        case 1020:
    //                            {
    //                                client.Player.SetLocation(1020, 565, 562);
    //                                break;
    //                            }
    //                        case 1011:
    //                            {
    //                                client.Player.SetLocation(1011, 188, 264);
    //                                break;
    //                            }
    //                        case 1015:
    //                            {
    //                                client.Player.SetLocation(1015, 717, 571);
    //                                break;
    //                            }
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (client.Player.MapID == 1038)
    //            {
    //                client.Player.SetLocation(1002, 300, 278);
    //            }
    //            else
    //            {
    //                ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
    //                client.Player.SetLocation(Point[0], Point[1], Point[2]);
    //            }
    //        }
    //        action.dwParam = client.Map.BaseID;
    //        action.X = client.Player.X;
    //        action.Y = client.Player.Y;
    //        client.Send(action);
    //    }
    //    public static object LoginSyncRoot = new object();
    //    public static void AppendConnect(MsgConnect Connect, GameState client)
    //    {
    //        if (client.LoggedIn)
    //        {
    //            client.Disconnect(true);
    //            return;
    //        }
    //        bool doLogin = false;
    //        lock (LoginSyncRoot)
    //        {
    //            DB.AccountTable Account = null;
    //            if (Kernel.AwaitingPool.TryGetValue(Connect.Identifier, out Account))
    //            {
    //                if (!Account.MatchKey(Connect.Identifier))
    //                {
    //                    client.Disconnect(false);
    //                    return;
    //                }
    //                client.Account = Account;
    //                if (Account.EntityID == 0)
    //                {
    //                    client.Send(new MsgTalk("NEW_ROLE", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //                    return;
    //                }
    //                if (Kernel.DisconnectPool.ContainsKey(Account.EntityID))
    //                {
    //                    client.Send(new MsgTalk("Please try again after a minute!", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //                    return;
    //                }
    //                VariableVault variables;
    //                DB.EntityVariableTable.Load(client.Account.EntityID, out variables);
    //                client.Variables = variables;
    //                if (client["banhours"] == 0)
    //                {
    //                    client["banhours"] = -1;
    //                    client["banreason"] = "Infinite time.";
    //                    client["banstamp"] = DateTime.Now.AddYears(100);
    //                }
    //                if (Account.State == DB.AccountTable.AccountState.Banned)
    //                {
    //                    if (client["banhours"] != -1)
    //                    {
    //                        DateTime banStamp = client["banstamp"];
    //                        if (DateTime.Now > banStamp.AddDays(((int)client["banhours"]) / 24).AddHours(((int)client["banhours"]) % 24))
    //                            Account.State = DB.AccountTable.AccountState.Player;
    //                    }
    //                }
    //                string Message = "";
    //                if (Account.State == DB.AccountTable.AccountState.Banned)
    //                {
    //                    DateTime banStamp = client["banstamp"];
    //                    banStamp = banStamp.AddHours(client["banhours"]);
    //                    Message = "You are banned for " + client["banhours"] + " hours [until " + banStamp.ToString("HH:mm MM/dd/yyyy") + "]. Reason: " + client["banreason"];
    //                }
    //                else if (Account.State == DB.AccountTable.AccountState.NotActivated)
    //                    Message = "You cannot login until your account is activated.";
    //                Kernel.AwaitingPool.Remove(Connect.Identifier);
    //                if (Message == string.Empty)
    //                {
    //                    GameState aClient = null;
    //                    if (Kernel.GamePool.TryGetValue(Account.EntityID, out aClient))
    //                        aClient.Disconnect();
    //                    Kernel.GamePool.Remove(Account.EntityID);
    //                    client.Player = new Player(PlayerFlag.Monster, false);
    //                    Kernel.GamePool.Add(Account.EntityID, client);
    //                    doLogin = true;
    //                }
    //                else
    //                {
    //                    client.Send(new MsgTalk(Message, "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //                    PlayerThread.Execute<GameState>((pClient, time) => { pClient.Disconnect(); }, client, 100);
    //                }
    //            }
    //        }
    //        if (doLogin)
    //        {
    //            DoLogin(client);
    //        }
    //    }
    //    public static void Attack(MsgInteract attack, Client.GameState client)
    //    {
    //        client.Player.RemoveMagicDefender();
    //        client.Player.AttackPacket = attack;
    //        new Game.Attacking.Handle(attack, client.Player, null);
    //    }
    //    public static ConcurrentDictionary<string, byte[]> TreasurePointsAllowance = new ConcurrentDictionary<string, byte[]>();
    //    public static object TPASyncRoot = new object();
    //    public static void AddTPA(GameState client)
    //    {
    //        if (!TreasurePointsAllowance.ContainsKey(client.Socket.IP))
    //            TreasurePointsAllowance.Add(client.Socket.IP, new byte[3]);
    //        lock (TPASyncRoot)
    //        {
    //            byte[] data = TreasurePointsAllowance[client.Socket.IP];
    //            for (int i = 0; i < data.Length; i++)
    //            {
    //                if (data[i] == 0)
    //                {
    //                    client.AllowedTreasurePoints = true;
    //                    client.AllowedTreasurePointsIndex = i;
    //                    data[i] = 1;
    //                }
    //            }
    //        }
    //    }
    //    public static void RemoveTPA(GameState client)
    //    {
    //        if (client.AllowedTreasurePoints)
    //        {
    //            lock (TPASyncRoot)
    //            {
    //                byte[] data = TreasurePointsAllowance[client.Socket.IP];
    //                data[client.AllowedTreasurePointsIndex] = 0;
    //            }
    //        }
    //    }
    //    public static void DoLogin(GameState client)
    //    {
    //        client.ReadyToPlay();
    //        if (DB.EntityTable.LoadEntity(client))
    //        {
    //            if (client.Player.FullyLoaded)
    //            {
    //                AddTPA(client);
    //                client.LoadData();
    //                if (client.Player.GuildID != 0)
    //                {
    //                    client.Player.GuildSharedBp = client.Guild.GetSharedBattlepower(client.Player.GuildRank);
    //                }
    //                client.ReviewMentor();
    //                if (client.JustCreated)
    //                {
    //                    #region AccountWhoJustCreatedItems
    //                    if (client.ItemGive)
    //                    {
    //                        client.Inventory.AddandWear(132013, 0, client);//Dress
    //                        if (client.Player.Class >= 10 && client.Player.Class <= 15)
    //                            client.Inventory.AddandWear(410301, 0, client);//Blade
    //                        if (client.Player.Class >= 20 && client.Player.Class <= 25)
    //                            client.Inventory.AddandWear(561301, 0, client);//Wand
    //                        if (client.Player.Class >= 40 && client.Player.Class <= 45)
    //                            client.Inventory.AddandWear(500301, 0, client);//Bow
    //                        if (client.Player.Class >= 50 && client.Player.Class <= 55)
    //                            client.Inventory.AddandWear(601301, 0, client);//Katana
    //                        if (client.Player.Class >= 60 && client.Player.Class <= 65)
    //                            client.Inventory.AddandWear(610301, 0, client);//Bead
    //                        if (client.Player.Class >= 70 && client.Player.Class <= 75)
    //                            client.Inventory.AddandWear(611301, 0, client);//Rapier
    //                        if (client.Player.Class >= 80 && client.Player.Class <= 85)
    //                            client.Inventory.AddandWear(617301, 0, client);//DragonWarriorWeapon
    //                        if (client.Player.Class >= 100 && client.Player.Class <= 145)
    //                            client.Inventory.AddandWear(421301, 0, client);//BackSword
    //                        if (client.Player.Class >= 160 && client.Player.Class <= 165)
    //                            client.Inventory.AddandWear(626003, 0, client);//Fan
    //                        client.ItemGive = false;
    //                    }
    //                    #endregion
    //                    client.ItemGive = true;
    //                    client.JustCreated = false;
    //                    Kernel.SendWorldMessage(new MsgTalk("Lets Welcome The New Player [" + client.Player.Name + "] , Has Joined Our Empire." + Kernel.GamePool.Count, System.Drawing.Color.Brown, 2012), Server.GamePool);
    //                    SetLocation(new MsgAction(true) { UID = client.Player.UID }, client);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            client.Send(new MsgTalk("Cannot find your character.", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //            client.Disconnect(false);
    //            return;
    //        }
    //        if (Kernel.GamePool.Count >= Server.PlayerCap)
    //        {
    //            client.Send(new MsgTalk("Player limit exceeded. (Online players: " + Kernel.GamePool + "/" + Server.PlayerCap + ")", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //            client.Disconnect(false);
    //            return;
    //        }
    //        client.Send(new MsgTalk("ANSWER_OK", "ALLUSERS", Color.Orange, (uint)PacketMsgTalk.MsgTalkType.Dialog));
    //        Server.Thread.Register(client);
    //        Kernel.GamePool[client.Account.EntityID] = client;
    //        DB.EntityTable.UpdateOnlineStatus(client, true);
    //        MsgUserInfo Info = new MsgUserInfo(client);
    //        client.Send(Info.ToArray());
    //        string IP = client.IP;
    //        client.Account.SetCurrentIP(IP);
    //        client.Account.Save();
    //        Server.UpdateConsoleTitle();
    //        client.LoggedIn = true;
    //        client.Action = 2;
    //    }
    //    public static readonly DateTime UnixEpoch = new DateTime();
    //    public static uint UnixTimestamp
    //    {
    //        get { return (uint)(DateTime.UtcNow - UnixEpoch).TotalSeconds; }
    //    }
    //    public static void RemoveBadSkills(GameState client)
    //    {
    //        if (client.Spells.ContainsKey(10405))
    //            if (!(client.Player.FirstRebornClass / 10 == client.Player.SecondRebornClass / 10 && client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 6))
    //                client.RemoveSpell(new MsgMagicInfo(true) { ID = 10405 });
    //        if (!client.Spells.ContainsKey(10405))
    //            if (client.Player.FirstRebornClass / 10 == client.Player.SecondRebornClass / 10 && client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 6)
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 10405 });
    //        if (client.Spells.ContainsKey(6002))
    //            if (!(client.Player.SecondRebornClass / 10 == client.Player.Class / 10 && client.Player.Class / 10 == 5))
    //                client.RemoveSpell(new MsgMagicInfo(true) { ID = 6002 });
    //        if (!((client.Player.Class >= 130 && client.Player.Class <= 135)))
    //        {
    //            client.RemoveSpell(new MsgMagicInfo(true) { ID = 1100 });
    //        }
    //        if (!((client.Player.Class >= 130 && client.Player.Class <= 135)))
    //            if (client.Spells.ContainsKey(30000))

    //                if (client.Spells.ContainsKey(10309))
    //                    client.Spells.Remove(10309);
    //    }
    //    public static void LoginMessages(GameState client)
    //    {

    //        #region VIPDays
    //        if (client.Player.VIPLevel >= 1 && client.Player.VIPLevelDays == 0 && client.Player.VIPDays == 0)
    //        {
    //            client.Send(DefineConstantsEn_Res.VIPLifetime);
    //        }
    //        if (client.Player.VIPDays >= 1)
    //        {
    //            if (client.Player.VIPDays >= 1 && client.Player.VIPLevelDays >= 1)
    //            {
    //                client.Player.VIPLevel = client.Player.VIPLevelDays;
    //                client.Send(new MsgTalk("You can stay " + client.Player.VIPDays + " Days of Level " + client.Player.VIPLevelDays + " Of VIP.", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.World));
    //            }
    //        }
    //        if (client.Player.VIPDays == 0 || client.Player.VIPLevelDays == 0)
    //        {
    //            client.Player.VIPDays = 0;
    //            if (client.Player.VIPLevel == 0 && client.Player.VIPLevelDays >= 1)
    //            {
    //                client.Send(DefineConstantsEn_Res.VIPExpired);
    //            }
    //        }
    //        #endregion  
    //        #region WardRobe
    //        client.WardRobe.Equip(client.WardRobe.MyGarment.UID, MsgItemInfo.Garment);
    //        client.WardRobe.Equip(client.WardRobe.MySteedArmor.UID, MsgItemInfo.SteedArmor);
    //        #endregion
    //        #region Prestige
    //        client.Send(Prestige.Stats(client));
    //        PerfectionRank.SendRankingQuery(new MsgRank(true) { Mode = 2 }, client, MsgRank.Prestige, PerfectionRank.FindRanking(client, 900), client.Player.Prestige);
    //        #endregion
    //        #region GameUpdates
    //        client.Send(new GameUpdates(GameUpdates.Mode.Header, "Welcome BakaConquer Private Project " + DateTime.Now.ToString()));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Body, "1.Windwalker Done TQ\n"));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Body, "2.Tournaments 90% Working\n"));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Body, "3.Events No Officials 100%\n"));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Body, "4.Bosses Full Fixed"));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Body, "5.Invite Friends!"));
    //        client.Send(new GameUpdates(GameUpdates.Mode.Footer, "All Bug´s Has Been Fix.!"));
    //        #endregion
    //        #region SendReload
    //        Game.Player.SendReload(client);
    //        #endregion
    //        #region JiangHu
    //        MsgOwnKongfuBase.SendJiangHu(client);
    //        #endregion
    //        #region MsgTrainingInfo
    //        if (client.Player.MapID == 601)
    //        {
    //            MsgTrainingInfo sts = new MsgTrainingInfo(true);
    //            var T1 = new TimeSpan(DateTime.Now.Ticks);
    //            var T2 = new TimeSpan(client.OfflineTGEnterTime.Ticks);
    //            ushort minutes = (ushort)(T1.TotalMinutes - T2.TotalMinutes);
    //            minutes = (ushort)Math.Min((ushort)900, minutes);
    //            sts.TotalTrainingMinutesLeft = (ushort)(900 - minutes);
    //            sts.TrainedMinutes = minutes;
    //            ulong exp = client.Player.Experience;
    //            byte level = client.Player.Level;
    //            double expballGain = (double)300 * (double)minutes / (double)900;
    //            while (expballGain >= 100)
    //            {
    //                expballGain -= 100;
    //                exp += client.ExpBall;
    //            }
    //            if (expballGain != 0)
    //                exp += (uint)(client.ExpBall * (expballGain / 100));

    //            while (exp >= DB.DataHolder.LevelExperience(level))
    //            {
    //                exp -= DB.DataHolder.LevelExperience(level);
    //                level++;
    //            }
    //            double percent = (double)exp * (double)100 / (double)DB.DataHolder.LevelExperience(level);

    //            sts.Character_NewExp = (ulong)(percent * 100000);
    //            sts.Character_AcquiredLevel = level;
    //            sts.Send(client);
    //        }
    //        #endregion
    //        #region SecondaryPassword
    //        if (client.WarehousePW != 0)
    //        {
    //            Msg2ndPsw SP = new Msg2ndPsw(true);
    //            SP.Action = Msg2ndPsw.Mode.PasswordCorrect;
    //            SP.OldPassword = 0x1;
    //            client.Send(SP.ToArray());
    //        }
    //        if (client.ForgetPassword)
    //        {
    //            if (client.WarehousePW != 0)
    //            {
    //                DB.ForgetPasswordTable.Date(client);
    //            }
    //        }
    //        if (client.FinishForget)
    //        {
    //            MsgAction Data = new MsgAction(true);
    //            Data.ID = PacketMsgAction.Mode.OpenCustom;
    //            Data.UID = client.Player.UID;
    //            Data.TimeStamp = Time32.Now;
    //            Data.dwParam = 3391;
    //            Data.X = client.Player.X;
    //            Data.Y = client.Player.Y;
    //            client.Send(Data);
    //            client.FinishForget = false;
    //        }
    //        #endregion
    //        #region ElitePK
    //        bool going = false;
    //        System.Threading.Tasks.Parallel.ForEach(Game.Features.Tournaments.ElitePKTournament.Tournaments, epk =>
    //        {
    //            if (epk.State != ElitePK.States.GUI_Top8Ranking)
    //                going = true;
    //        });
    //        if (going)
    //        {
    //            MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
    //            brackets.Type = MsgPKEliteMatchInfo.EPK_State;
    //            brackets.OnGoing = true;
    //            client.Send(brackets);
    //        }
    //        //bool going = false;
    //        //foreach (var epk in Game.Features.Tournaments.ElitePKTournament.Tournaments)
    //        //    if (epk.State != ElitePK.States.GUI_Top8Ranking)
    //        //        going = true;
    //        //if (going)
    //        //{
    //        //    MsgPKEliteMatchInfo brackets = new MsgPKEliteMatchInfo(true, 0);
    //        //    brackets.Type = MsgPKEliteMatchInfo.EPK_State;
    //        //    brackets.OnGoing = true;
    //        //    client.Send(brackets);
    //        //}
    //        #endregion
    //        #region Time Server
    //        MsgData time = new MsgData();
    //        time.Year = (uint)DateTime.Now.Year;
    //        time.Month = (uint)DateTime.Now.Month;
    //        time.DayOfYear = (uint)DateTime.Now.DayOfYear;
    //        time.DayOfMonth = (uint)DateTime.Now.Day;
    //        time.Hour = (uint)DateTime.Now.Hour;
    //        time.Minute = (uint)DateTime.Now.Minute;
    //        time.Second = (uint)DateTime.Now.Second;
    //        client.Send(time);
    //        #endregion
    //        #region MentorInformation
    //        if (client.Mentor != null)
    //        {
    //            if (client.Mentor.IsOnline)
    //            {
    //                MentorInformation Information = new MentorInformation(true);
    //                Information.Mentor_Type = 1;
    //                Information.Mentor_ID = client.Mentor.Client.Player.UID;
    //                Information.Apprentice_ID = client.Player.UID;
    //                Information.Enrole_Date = client.Mentor.EnroleDate;
    //                Information.Mentor_Level = client.Mentor.Client.Player.Level;
    //                Information.Mentor_Class = client.Mentor.Client.Player.Class;
    //                Information.Mentor_PkPoints = client.Mentor.Client.Player.PKPoints;
    //                Information.Mentor_Mesh = client.Mentor.Client.Player.Mesh;
    //                Information.Mentor_Online = true;
    //                Information.Shared_Battle_Power = client.Player.BattlePowerFrom(client.Mentor.Client.Player);
    //                Information.String_Count = 3;
    //                Information.Mentor_Name = client.Mentor.Client.Player.Name;
    //                Information.Apprentice_Name = client.Player.Name;
    //                Information.Mentor_Spouse_Name = client.Mentor.Client.Player.Spouse;
    //                client.ReviewMentor();
    //                client.Send(Information);

    //                MsgGuideInfo AppInfo = new MsgGuideInfo();
    //                AppInfo.Apprentice_ID = client.Player.UID;
    //                AppInfo.Apprentice_Level = client.Player.Level;
    //                AppInfo.Apprentice_Class = client.Player.Class;
    //                AppInfo.Apprentice_PkPoints = client.Player.PKPoints;
    //                AppInfo.Apprentice_Experience = client.AsApprentice.Actual_Experience;
    //                AppInfo.Apprentice_Composing = client.AsApprentice.Actual_Plus;
    //                AppInfo.Apprentice_Blessing = client.AsApprentice.Actual_HeavenBlessing;
    //                AppInfo.Apprentice_Name = client.Player.Name;
    //                AppInfo.Apprentice_Online = true;
    //                AppInfo.Apprentice_Spouse_Name = client.Player.Spouse;
    //                AppInfo.Enrole_date = client.Mentor.EnroleDate;
    //                AppInfo.Mentor_ID = client.Mentor.ID;
    //                AppInfo.Mentor_Mesh = client.Mentor.Client.Player.Mesh;
    //                AppInfo.Mentor_Name = client.Mentor.Name;
    //                AppInfo.Type = 2;
    //                client.Mentor.Client.Send(AppInfo);
    //            }
    //            else
    //            {
    //                MentorInformation Information = new MentorInformation(true);
    //                Information.Mentor_Type = 1;
    //                Information.Mentor_ID = client.Mentor.ID;
    //                Information.Apprentice_ID = client.Player.UID;
    //                Information.Enrole_Date = client.Mentor.EnroleDate;
    //                Information.Mentor_Online = false;
    //                Information.String_Count = 2;
    //                Information.Mentor_Name = client.Mentor.Name;
    //                Information.Apprentice_Name = client.Player.Name;
    //                client.Send(Information);
    //            }
    //        }
    //        #endregion
    //        #region Nobility
    //        NobilityInfo update = new NobilityInfo(true);
    //        update.Type = NobilityInfo.Icon;
    //        update.dwParam = client.NobilityInformation.EntityUID;
    //        update.UpdateString(client.NobilityInformation);
    //        client.Send(update);
    //        #endregion
    //        #region ChiPowers
    //        client.Send(new MsgTrainingVitalityInfo(true).Query(client));
    //        MsgTrainingVitality.SendChiRankings(new MsgRank(true) { Mode = MsgRank.QueryCount }, client);
    //        #endregion
    //        #region Adding earned skills
    //        if (client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 9876 });
    //        if (client.Player.Class >= 51 && client.Player.Class <= 55 && client.Player.FirstRebornClass == 55 && client.Player.Reborn == 1)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 6002 });
    //        if (client.Player.FirstRebornClass == 15 && client.Player.SecondRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 10315 });
    //        if (client.Player.FirstRebornClass == 75 && client.Player.SecondRebornClass == 75 && client.Player.Class >= 71 && client.Player.Class <= 75 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 11040 });
    //        if (client.Player.FirstRebornClass == 25 && client.Player.SecondRebornClass == 25 && client.Player.Class >= 21 && client.Player.Class <= 25 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 10311 });
    //        if (client.Player.FirstRebornClass == 45 && client.Player.SecondRebornClass == 45 && client.Player.Class >= 41 && client.Player.Class <= 45 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 10313 });
    //        if (client.Player.FirstRebornClass == 55 && client.Player.SecondRebornClass == 55 && client.Player.Class >= 51 && client.Player.Class <= 55 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 6003 });
    //        if (client.Player.FirstRebornClass == 65 && client.Player.SecondRebornClass == 65 && client.Player.Class >= 61 && client.Player.Class <= 65 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 10405 });
    //        if (client.Player.FirstRebornClass == 135 && client.Player.SecondRebornClass == 135 && client.Player.Class >= 131 && client.Player.Class <= 135 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 30000 });
    //        if (client.Player.FirstRebornClass == 145 && client.Player.SecondRebornClass == 145 && client.Player.Class >= 140 && client.Player.Class <= 145 && client.Player.Reborn == 2)
    //            client.AddSpell(new MsgMagicInfo(true) { ID = 10310 });
    //        if (client.Player.Reborn == 1)
    //        {
    //            if (client.Player.FirstRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3050 });
    //            }
    //            else if (client.Player.FirstRebornClass == 25 && client.Player.Class >= 21 && client.Player.Class <= 25)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3060 });
    //            }
    //            else if (client.Player.FirstRebornClass == 145 && client.Player.Class >= 142 && client.Player.Class <= 145)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3080 });
    //            }
    //            else if (client.Player.FirstRebornClass == 135 && client.Player.Class >= 132 && client.Player.Class <= 135)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3090 });
    //            }
    //        }
    //        if (client.Player.Reborn == 2)
    //        {
    //            if (client.Player.SecondRebornClass == 15 && client.Player.Class >= 11 && client.Player.Class <= 15)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3050 });
    //            }
    //            else if (client.Player.SecondRebornClass == 25)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3060 });
    //            }
    //            else if (client.Player.SecondRebornClass == 145 && client.Player.Class >= 142 && client.Player.Class <= 145)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3080 });
    //            }
    //            else if (client.Player.SecondRebornClass == 135 && client.Player.Class >= 132 && client.Player.Class <= 135)
    //            {
    //                client.AddSpell(new MsgMagicInfo(true) { ID = 3090 });
    //            }
    //        }
    //        #endregion

    //        #region LoginInfo
    //        MsgPCServerConfig mpccc = new MsgPCServerConfig(true) { Type = 15 };
    //        client.Send(mpccc);
    //        DB.EntityTable.LoginNow(client);
    //        client.Send(new MsgServerInfo().ToArray());
    //        #endregion
    //        #region AutoHunt
    //        client.Send(new MsgHangUp() { Icon = 341 }.ToArray());
    //        #endregion
    //        #region WentToComplete
    //        client.Filtering = true;
    //        if (client.WentToComplete) return;
    //        RemoveBadSkills(client);
    //        client.WentToComplete = true;
    //        client.Player.SendUpdates = true;
    //        #endregion
    //        #region Guild
    //        foreach (var Guild in Kernel.Guilds.Values)
    //        {
    //            Guild.SendName(client);
    //        }
    //        if (client.Guild != null)
    //        {
    //            client.Guild.SendAllyAndEnemy(client);
    //            client.Player.GuildSharedBp = client.Guild.GetSharedBattlepower(client.Player.GuildRank);
    //        }
    //        #endregion
    //        #region Equipment
    //        foreach (MsgItemInfo item in client.Inventory.Objects)
    //            item.Send(client);
    //        foreach (MsgItemInfo item in client.Equipment.Objects)
    //        {
    //            if (item != null)
    //            {
    //                if (DB.ConquerItemInformation.BaseInformations.ContainsKey(item.ID))
    //                {
    //                    item.Send(client);
    //                }
    //                else
    //                {
    //                    client.Equipment.DestroyArrow(item.Position);
    //                }
    //            }
    //        }
    //        client.LoadItemStats();
    //        if (!client.Equipment.Free(5))
    //        {
    //            if (ItemHandler.IsArrow(client.Equipment.TryGetItem(5).ID))
    //            {
    //                if (client.Equipment.Free(4))
    //                    client.Equipment.DestroyArrow(5);
    //                else
    //                {
    //                    if (client.Equipment.TryGetItem(4).ID / 1000 != 500)
    //                        client.Equipment.DestroyArrow(5);
    //                }
    //            }
    //        }
    //        client.GemAlgorithm();
    //        client.CalculateStatBonus();
    //        client.CalculateHPBonus();
    //        client.Player.Stamina = 100;
    //        #endregion
    //        #region WelcomeMessages
    //        string[] wm = File.ReadAllLines(InfoFile.WelcomeMessages);
    //        foreach (string line in wm)
    //        {
    //            if (line.Length == 0) continue;
    //            if (line[0] == ';') continue;
    //            client.Send(line);
    //        }
    //        #endregion
    //        #region VIPLevel
    //        client.Player.VIPLevel = (byte)(client.Player.VIPLevel + 0);
    //        if (client.Player.VIPLevel > 0)
    //        {
    //            MsgVipFunctionValidNotify vip = new MsgVipFunctionValidNotify();
    //            client.Send(vip.GetArray());
    //        }
    //        #endregion
    //        #region MapStatus
    //        client.Send(new MsgMapInfo() { BaseID = client.Map.BaseID, ID = client.Map.ID, Status = DB.MapsTable.MapInformations[client.Map.ID].Status, Weather = DB.MapsTable.MapInformations[client.Map.ID].Weather });
    //        if (client.Player.Hitpoints == 0)
    //        {
    //            ushort[] Point = DB.DataHolder.FindReviveSpot(client.Map.ID);
    //            client.Player.Teleport(Point[0], Point[1], Point[2]);
    //            client.Player.Hitpoints = 1;
    //        }
    //        #endregion
    //        #region MentorBattlePower
    //        if (client.Player.MentorBattlePower != 0)
    //            client.Player.Update((byte)PacketFlag.DataType.ExtraBattlePower, client.Player.MentorBattlePower, false);
    //        #endregion
    //        #region Broadcast
    //        if (Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityID > 2)
    //            client.Send(new MsgTalk(Game.ConquerStructures.Broadcast.CurrentBroadcast.Message, "ALLUSERS", Game.ConquerStructures.Broadcast.CurrentBroadcast.EntityName, Color.Red, (uint)PacketMsgTalk.MsgTalkType.BroadcastMessage, Game.ConquerStructures.Broadcast.CurrentBroadcast.UnionTitle));
    //        #endregion
    //        #region BlessTime
    //        client.Player.ExpProtectionTime = (ushort)(client.Player.ExpProtectionTime + (1 - 1));
    //        client.Player.DoubleExperienceTime = (ushort)(client.Player.DoubleExperienceTime + (1 - 1));
    //        client.Player.HandleTiming = true;
    //        client.Player.Update((byte)PacketFlag.DataType.RaceShopPoints, client.Player.RacePoints, false);
    //        client.Player.Update((byte)PacketFlag.DataType.LuckyTimeTimer, client.BlessTime, false);
    //        if (client.Player.HeavenBlessing != 0)
    //        {
    //            client.Send("Heaven Blessing Expire: " + DateTime.Now.AddSeconds(client.Player.HeavenBlessing).ToString("yyyy:MM-dd:HH"));
    //            client.Player.Update((byte)PacketFlag.DataType.OnlineTraining, client.OnlineTrainingPoints, false);
    //        }
    //        #endregion
    //        #region ClaimableItem
    //        if (client.ClaimableItem.Count > 0)
    //        {
    //            foreach (var item in client.ClaimableItem.Values)
    //            {
    //                DB.ItemAddingTable.GetAddingsForItem(item.Item);
    //                item.Send(client);
    //                item.Item.SendExtras(client);
    //            }
    //        }
    //        #endregion
    //        #region DeatinedItem
    //        if (client.DeatinedItem.Count > 0)
    //        {
    //            foreach (var item in client.DeatinedItem.Values)
    //            {
    //                DB.ItemAddingTable.GetAddingsForItem(item.Item);
    //                item.Send(client);
    //                item.Item.SendExtras(client);
    //            }
    //        }
    //        client.Equipment.UpdateEntityPacket();
    //        #endregion
    //        #region Sash
    //        client.Player.Update((byte)PacketFlag.DataType.AvailableSlots, 300, false);
    //        client.Player.Update((byte)PacketFlag.DataType.ExtraInventory, client.Player.ExtraInventory, false);
    //        #endregion
    //        #region DailySignin
    //        client.Send(new DailySignIn(true) { Type = DailySignIn.Action.Info, Claimed = client.SignClaim, CumulativeDays = client.CumulativeDays, LateSignChance = client.LateSignChance });
    //        #endregion
    //        #region HP & Mana
    //        client.Player.Hitpoints = client.Player.MaxHitpoints;
    //        client.Player.Mana = client.Player.MaxMana;
    //        #endregion
    //        #region Achievement
    //        if (client.Player.MyAchievement != null)
    //            client.Player.MyAchievement.Send();
    //        #endregion
    //        client.Player.UpdateEffects(true);


    //        #region InnerPower
    //        if (!MaTrix.Inner.InnerPower.InnerPowerPolle.TryGetValue(client.Player.UID, out client.Player.InnerPower))
    //        {
    //            client.Player.InnerPower = new MaTrix.Inner.InnerPower(client.Player.Name, client.Player.UID);
    //            Database.InnerPowerTable.New(client);
    //        }
    //        client.Player.InnerPower.UpdateStatus();
    //        client.Player.InnerPower.AddPotency(null, client, 0);
    //        client.LoadItemStats();
    //        #endregion
    //        #region Merchant
    //        if (client.Player.Merchant == 255)
    //        {
    //            client.Player.Update((byte)PacketFlag.DataType.Merchant, 255, false);
    //        }
    //        else if (client.Player.Merchant == 1)
    //        {
    //            MsgInteract send = new MsgInteract(true);
    //            send.InteractType = Network.GamePackets.MsgInteract.MerchantProgress;
    //            client.Send(send.ToArray());
    //        }
    //        #endregion
    //        WindWalker.JusticeChainEquipment(client);
    //    }
    //}
}
