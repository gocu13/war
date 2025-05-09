using System;
using System.Linq;
using System.Drawing;
using COServer.Network;
using COServer.Interfaces;
using System.Collections.Generic;
using COServer.Network.GamePackets;
using COServer.DB;
using Core.Packet;
using Core.Enums;
using Core.AttackProcessors;
using Game.Features.Event;
using COServer.Game.ConquerStructures;

namespace COServer.Game.Attacking
{
    public unsafe class Handle
    {
        public struct Location
        {
            public short X;
            public short Y;
        }
        public static List<Location> GetLocation(int X, int Y)
        {
            List<Location> myList = new List<Location>();
            var distance = 6;

            myList.Add(new Location() { X = (short)(X + -distance), Y = (short)(Y + -distance) });
            myList.Add(new Location() { X = (short)(X + -distance), Y = (short)(Y + distance) });
            myList.Add(new Location() { X = (short)(X + distance), Y = (short)(Y + distance) });
            myList.Add(new Location() { X = (short)(X + distance), Y = (short)(Y + -distance) });
            return myList;
        }
        public static List<Location> GetLocation2(int X, int Y)
        {
            List<Location> myList = new List<Location>();
            var distance = 6;

            myList.Add(new Location() { X = (short)(X + -distance), Y = (short)(Y + -distance) });
            myList.Add(new Location() { X = (short)(X + -distance), Y = (short)(Y + distance) });
            //myList.Add(new Location() { X = (short)(X + distance), Y = (short)(Y + distance) });
            myList.Add(new Location() { X = (short)(X + distance), Y = (short)(Y + -distance) });
            return myList;
        }
        public static void StomperMelee(MsgMapItem item, ushort SpellID)
        {
            if (item.Owner.Player == null || item.Owner.Player.Dead) return;
            var attack = item.Attack as MsgInteract;
            var spell = DB.SpellTable.GetSpell(SpellID, item.Owner);
            MsgMagicEffect suse = new MsgMagicEffect(true);
            suse.Attacker = SpellID == 12990 ? item.UID : item.Owner.Player.UID;
            suse.SpellID = SpellID;
            suse.SpellLevel = spell.Level;
            suse.SpecialEffect = 1;
            if (SpellID == 12990)
            {
                suse.X = item.X2;
                suse.Y = item.Y2;
                foreach (Interfaces.IMapObject _obj in item.Owner.Screen.Objects)
                {
                    if (_obj.MapObjType == MapObjectType.Monster)
                    {
                        if (_obj == null) continue;
                        var attacked1 = _obj as Player;
                        if (CanAttack(item.Owner.Player, attacked1, spell, false))
                        {
                            var spellange = Enums.HorrorofStomperAngle(item.Angle);
                            ushort xxxx = item.X;
                            ushort yyyy = item.Y;
                            Map.Pushback(ref xxxx, ref yyyy, spellange, 7);
                            Fan sector = new Fan(item.X, item.Y, xxxx, yyyy, spell.Range, spell.Sector);
                            if (sector.IsInFan(attacked1.X, attacked1.Y))
                            {
                                uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, spell, ref attack) / 2;
                                ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
                                suse.AddTarget(attacked1, damage, null);
                                continue;
                            }
                            var spellange1 = Enums.OppositeAngle(Enums.HorrorofStomperAngle(item.Angle));
                            ushort xxxx1 = item.X;
                            ushort yyyy1 = item.Y;
                            Map.Pushback(ref xxxx1, ref yyyy1, spellange, 7);
                            Fan sector1 = new Fan(item.X, item.Y, xxxx1, yyyy1, spell.Range, spell.Sector);
                            if (sector1.IsInFan(attacked1.X, attacked1.Y))
                            {
                                uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, spell, ref attack) / 2;
                                ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
                                suse.AddTarget(attacked1, damage, null);
                            }
                        }
                    }
                    if (_obj.MapObjType == MapObjectType.SobNpc)
                    {
                        if (_obj == null) continue;
                        var attacked1sob = _obj as MsgNpcInfoEX;
                        if (CanAttack(item.Owner.Player, attacked1sob, spell))
                        {
                            {
                                var spellange = Enums.HorrorofStomperAngle(item.Angle);
                                ushort xxxx = item.X;
                                ushort yyyy = item.Y;
                                Map.Pushback(ref xxxx, ref yyyy, spellange, 7);
                                Fan sector = new Fan(item.X, item.Y, xxxx, yyyy, spell.Range, spell.Sector);
                                if (sector.IsInFan(attacked1sob.X, attacked1sob.Y))
                                {
                                    uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1sob, ref attack);
                                    ReceiveAttack(item.Owner.Player, attacked1sob, attack, damage, spell);
                                    suse.AddTarget(attacked1sob, damage, null);
                                    continue;
                                }
                            }
                            {
                                var spellange = Enums.OppositeAngle(Enums.HorrorofStomperAngle(item.Angle));
                                ushort xxxx = item.X;
                                ushort yyyy = item.Y;
                                Map.Pushback(ref xxxx, ref yyyy, spellange, 7);
                                Fan sector = new Fan(item.X, item.Y, xxxx, yyyy, spell.Range, spell.Sector);
                                if (sector.IsInFan(attacked1sob.X, attacked1sob.Y))
                                {
                                    uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1sob, ref attack);
                                    ReceiveAttack(item.Owner.Player, attacked1sob, attack, damage, spell);
                                    suse.AddTarget(attacked1sob, damage, null);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                suse.X = item.X;
                suse.Y = item.Y;
                foreach (Interfaces.IMapObject _obj in item.Owner.Screen.Objects)
                {
                    if (_obj.MapObjType == MapObjectType.Monster)
                    {
                        if (_obj == null) continue;
                        var attacked1 = _obj as Player;
                        if (item == null) return;
                        if (attacked1 == null) return;
                        if (Kernel.GetDistance(item.X, item.Y, attacked1.X, attacked1.Y) <= spell.Range)
                        {
                            if (CanAttack(item.Owner.Player, attacked1, spell, false))
                            {
                                uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, spell, ref attack);
                                ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
                                suse.AddTarget(attacked1, damage, null);
                            }
                        }
                    }
                    if (_obj.MapObjType == MapObjectType.SobNpc)
                    {
                        if (_obj == null) continue;
                        var attacked1sob = _obj as Player;
                        if (item == null) return;
                        if (attacked1sob == null) return;
                        if (Kernel.GetDistance(item.X, item.Y, attacked1sob.X, attacked1sob.Y) <= 7)
                        {
                            if (CanAttack(item.Owner.Player, attacked1sob, spell, false))
                            {
                                uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1sob, spell, ref attack);
                                ReceiveAttack(item.Owner.Player, attacked1sob, attack, ref damage, spell);
                                suse.AddTarget(attacked1sob, damage, null);
                            }
                        }
                    }
                }
            }
            item.Owner.SendScreen(suse, true);
            item.Owner.Player.AttackPacket = null;
        }
        //public static void StomperMelee(MsgMapItem item, ushort SpellID)
        //{
        //    var attack = item.Attack;
        //    var spell = SpellTable.GetSpell(SpellID, item.Owner);
        //    MsgMagicEffect suse = new MsgMagicEffect(true);
        //    suse.Attacker = SpellID == 12990 ? item.UID : item.Owner.Player.UID;
        //    suse.SpellID = SpellID;
        //    suse.SpellLevel = spell.Level;
        //    suse.SpecialEffect = 1;
        //    if (SpellID == 12990)
        //    {
        //        suse.X = item.X2;
        //        suse.Y = item.Y2;
        //        foreach (Interfaces.IMapObject _obj in item.Owner.Screen.Objects)
        //        {
        //            if (_obj.MapObjType == MapObjectType.Monster)
        //            {
        //                if (_obj == null) continue;
        //                var attacked1 = _obj as Player;
        //                if (CanAttack(item.Owner.Player, attacked1, spell, false))
        //                {
        //                    {
        //                        var spellange = Enums.HorrorofStomperAngle(item.Angle);
        //                        ushort xxxx = item.X;
        //                        ushort yyyy = item.Y;
        //                        Map.Pushback(ref xxxx, ref  yyyy, spellange, 7);
        //                        Fan sector = new Fan(item.X, item.Y, xxxx, yyyy, spell.Range, spell.Sector);
        //                        if (sector.IsInFan(attacked1.X, attacked1.Y))
        //                        {
        //                            uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, ref attack) / 4;
        //                            ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
        //                            suse.AddTarget(attacked1.UID, damage, null);
        //                            continue;
        //                        }
        //                    }
        //                    {
        //                        var spellange = Enums.OppositeAngle(Enums.HorrorofStomperAngle(item.Angle));
        //                        ushort xxxx = item.X;
        //                        ushort yyyy = item.Y;
        //                        Map.Pushback(ref xxxx, ref  yyyy, spellange, 7);
        //                        Fan sector = new Fan(item.X, item.Y, xxxx, yyyy, spell.Range, spell.Sector);
        //                        if (sector.IsInFan(attacked1.X, attacked1.Y))
        //                        {
        //                            uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, ref attack) / 4;
        //                            ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
        //                            suse.AddTarget(attacked1.UID, damage, null);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        suse.X = item.X;
        //        suse.Y = item.Y;
        //        foreach (Interfaces.IMapObject _obj in item.Owner.Screen.Objects)
        //        {
        //            if (_obj.MapObjType == MapObjectType.Monster)
        //            {
        //                if (_obj == null) continue;
        //                var attacked1 = _obj as Player;
        //                if (Kernel.GetDistance(item.X, item.Y, attacked1.X, attacked1.Y) <= spell.Range)
        //                {
        //                    if (CanAttack(item.Owner.Player, attacked1, spell, false))
        //                    {
        //                        uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked1, ref attack) / 4;
        //                        ReceiveAttack(item.Owner.Player, attacked1, attack, ref damage, spell);
        //                        suse.AddTarget(attacked1.UID, damage, null);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    item.Owner.SendScreen(suse, true);
        //    item.Owner.Player.AttackPacket = null;
        //}
        public static void ShadowofChaser(Player attacker, Player attacked, MsgInteract attack, byte oneortwo)
        {
            #region ShadowofChaser(Active)
            IMapObject lastAttacked = attacked;
            if (oneortwo == 1)
            {
                if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.ShadowofChaser) && attacker.IsChaser())
                {
                    #region MsgMapItem
                    var map = Kernel.Maps[attacker.MapID];
                    Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                    flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    while (map.Npcs.ContainsKey(flooritem.UID))
                        flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                    flooritem.ItemID = MsgMapItem.ShadowofChaser;
                    flooritem.X = lastAttacked.X;
                    flooritem.MapID = attacker.MapID;
                    flooritem.Y = lastAttacked.Y;
                    flooritem.Type = MsgMapItem.Effect;
                    flooritem.mColor = 14;
                    flooritem.FlowerType = 3;
                    flooritem.Unknown37 = 1;
                    flooritem.Attack = attack;
                    flooritem.OwnerUID = attacker.UID;
                    flooritem.Owner = attacker.Owner;
                    flooritem.OwnerGuildUID = attacker.GuildID;
                    flooritem.OnFloor = Time32.Now;
                    flooritem.Name = "ShadowofChaser";
                    flooritem.ShadowofChaserAttacked = attacked;
                    flooritem.ShadowofChaserAttacker = attacker;
                    attacker.Owner.Map.AddFloorItem(flooritem);
                    attacker.Owner.SendScreenSpawn(flooritem, true);
                    #endregion
                    return;
                }
            }
            else if (oneortwo == 2)
            {
                var spell = SpellTable.GetSpell(13090, attacker.Owner);
                MsgMagicEffect suse = new MsgMagicEffect(true);
                suse.Attacker = attacker.UID;
                suse.Attacker1 = attacker.UID;
                suse.SpellID = spell.ID;
                suse.SpellLevel = spell.Level;
                if (CanAttack(attacker, attacked, spell, false))
                {
                    lastAttacked = attacked;
                    uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) / 8;
                    Handle.ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                    suse.AddTarget(attacked.UID, damage, null);
                }
                attacker.Owner.SendScreen(suse, true);
                MsgMagicEffect suse2 = new MsgMagicEffect(true);
                suse2.Attacker = attacker.UID;
                suse2.SpellID = spell.ID;
                suse2.SpellLevel = spell.Level;
                suse2.X = attacked.X;
                suse2.Y = attacked.Y;
                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                {
                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                    {
                        if (_obj == null) continue;
                        var attacked1 = _obj as Player;
                        if (lastAttacked.UID == attacked1.UID) continue;
                        if (Kernel.GetDistance(lastAttacked.X, lastAttacked.Y, attacked1.X, attacked1.Y) <= 4)
                        {
                            if (CanAttack(attacker, attacked1, spell, false))
                            {
                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked1, ref attack) / 8;
                                attack.Damage = damage;
                                ReceiveAttack(attacker, attacked1, attack, ref damage, spell);
                                suse2.AddTarget(attacked1.UID, damage, null);
                            }
                        }
                    }
                }
                attacker.Owner.SendScreen(suse2, true);
            }
            #endregion
            attacker.AttackPacket = null;
        }
        public static void RageOfWarTrap(MsgMapItem item)
        {
            var attack = new MsgInteract(true);
            var spell = DB.SpellTable.GetSpell(12930, item.Owner);
            var suse = new MsgMagicEffect(true);
            attack.Attacker = suse.Attacker = item.Owner.Player.UID;
            attack.SpellID = suse.SpellID = spell.ID;
            suse.X = item.X;
            suse.Y = item.Y;
            suse.SpellLevel = spell.Level;
            suse.SpecialEffect = 1;
            Sector sector = new Sector(item.X, item.Y, item.X, item.Y);
            sector.Arrange(spell.Sector, 3);
            Player attacked = null;
            foreach (Interfaces.IMapObject _obj in item.Owner.Screen.Objects)
            {
                if (_obj == null) continue;
                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                {
                    attacked = _obj as Player;
                    if (sector.Inside(attacked.X, attacked.Y))
                    {
                        if (CanAttack(item.Owner.Player, attacked, spell, attack.InteractType == MsgInteract.Melee))
                        {
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = Game.Attacking.Calculate.Melee(item.Owner.Player, attacked, spell, ref attack);
                            attack.Damage = damage;
                            suse.Effect = attack.Effect;
                            ReceiveAttack(item.Owner.Player, attacked, attack, ref damage, spell);
                            suse.AddTarget(attacked.UID, damage, attack);
                        }
                    }
                }
            }
            item.Owner.SendScreen(suse, true);
        }
        private MsgInteract attack;
        private Player attacker, attacked;
        #region Interations
        public class InteractionRequest
        {
            public InteractionRequest(MsgInteract attack, Game.Player a_client)
            {
                Client.GameState client = a_client.Owner;
                client.Player.InteractionInProgress = false;
                client.Player.InteractionWith = attack.Attacked;
                client.Player.InteractionType = 0;
                client.InteractionEffect = attack.ResponseDamage;
                if (Kernel.GamePool.ContainsKey(attack.Attacked))
                {
                    Client.GameState clienttarget = Kernel.GamePool[attack.Attacked];
                    clienttarget.Player.InteractionInProgress = false;
                    clienttarget.Player.InteractionWith = client.Player.UID;
                    clienttarget.Player.InteractionType = 0;
                    clienttarget.InteractionEffect = attack.ResponseDamage;
                    attack.Attacker = client.Player.UID;
                    attack.X = clienttarget.Player.X;
                    attack.Y = clienttarget.Player.Y;
                    attack.InteractType = 46;
                    clienttarget.Send(attack);
                    clienttarget.Send(attack);
                }
            }
        }
        public class InteractionEffect
        {
            public InteractionEffect(MsgInteract attack, Game.Player a_client)
            {
                Client.GameState client = a_client.Owner;
                if (Kernel.GamePool.ContainsKey(client.Player.InteractionWith))
                {
                    Client.GameState clienttarget = Kernel.GamePool[client.Player.InteractionWith];
                    if (clienttarget.Player.X == client.Player.X && clienttarget.Player.Y == client.Player.Y)
                    {
                        attack.Damage = client.Player.InteractionType;
                        attack.ResponseDamage = client.InteractionEffect;
                        clienttarget.Player.InteractionSet = true;
                        client.Player.InteractionSet = true;
                        attack.Attacker = clienttarget.Player.UID;
                        attack.Attacked = client.Player.UID;
                        attack.InteractType = 47;
                        attack.X = clienttarget.Player.X;
                        attack.Y = clienttarget.Player.Y;
                        clienttarget.Send(attack);
                        attack.InteractType = 49;
                        attack.Attacker = client.Player.UID;
                        attack.Attacked = clienttarget.Player.UID;
                        client.SendScreen(attack, true);
                        attack.Attacker = clienttarget.Player.UID;
                        attack.Attacked = client.Player.UID;
                        client.SendScreen(attack, true);
                    }
                }
            }
        }
        public class InteractionAccept
        {
            public InteractionAccept(MsgInteract attack, Game.Player a_client)
            {

                Client.GameState client = a_client.Owner;
                if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                    client.Player.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                if (client.Player.InteractionWith != attack.Attacked) return;
                attack.ResponseDamage = client.InteractionEffect;
                client.Player.InteractionSet = false;
                if (Kernel.GamePool.ContainsKey(attack.Attacked))
                {
                    Client.GameState clienttarget = Kernel.GamePool[attack.Attacked];
                    if (clienttarget.Player.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                        clienttarget.Player.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                    clienttarget.Player.InteractionSet = false;
                    if (clienttarget.Player.InteractionWith != client.Player.UID) return;
                    if (clienttarget.Player.Body == 1003 || clienttarget.Player.Body == 1004)
                    {
                        attack.Attacker = client.Player.UID;
                        attack.X = client.Player.X;
                        attack.Y = client.Player.Y;
                        clienttarget.Send(attack);
                        clienttarget.Player.InteractionInProgress = true;
                        client.Player.InteractionInProgress = true;
                        clienttarget.Player.InteractionType = attack.Damage;
                        clienttarget.Player.InteractionX = client.Player.X;
                        clienttarget.Player.InteractionY = client.Player.Y;
                        client.Player.InteractionType = attack.Damage;
                        client.Player.InteractionX = client.Player.X;
                        client.Player.InteractionY = client.Player.Y;
                        if (clienttarget.Player.X == client.Player.X && clienttarget.Player.Y == client.Player.Y)
                        {
                            attack.Damage = client.Player.InteractionType;
                            clienttarget.Player.InteractionSet = true;
                            client.Player.InteractionSet = true;
                            attack.Attacker = clienttarget.Player.UID;
                            attack.Attacked = client.Player.UID;
                            attack.InteractType = 47;
                            attack.X = clienttarget.Player.X;
                            attack.Y = clienttarget.Player.Y;
                            attack.ResponseDamage = clienttarget.InteractionEffect;
                            clienttarget.Send(attack);
                            attack.InteractType = 49;
                            attack.Attacker = client.Player.UID;
                            attack.Attacked = clienttarget.Player.UID;
                            client.SendScreen(attack, true);
                            attack.Attacker = clienttarget.Player.UID;
                            attack.Attacked = client.Player.UID;
                            client.SendScreen(attack, true);
                        }
                    }
                    else
                    {
                        attack.InteractType = 47;
                        attack.Attacker = client.Player.UID;
                        attack.X = client.Player.X;
                        attack.Y = client.Player.Y;
                        clienttarget.Send(attack);
                        clienttarget.Player.InteractionInProgress = true;
                        client.Player.InteractionInProgress = true;
                        clienttarget.Player.InteractionType = attack.Damage;
                        clienttarget.Player.InteractionX = clienttarget.Player.X;
                        clienttarget.Player.InteractionY = clienttarget.Player.Y;
                        client.Player.InteractionType = attack.Damage;
                        client.Player.InteractionX = clienttarget.Player.X;
                        client.Player.InteractionY = clienttarget.Player.Y;
                        if (clienttarget.Player.X == client.Player.X && clienttarget.Player.Y == client.Player.Y)
                        {
                            clienttarget.Player.InteractionSet = true;
                            client.Player.InteractionSet = true;
                            attack.Attacker = clienttarget.Player.UID;
                            attack.Attacked = client.Player.UID;
                            attack.X = clienttarget.Player.X;
                            attack.Y = clienttarget.Player.Y;
                            attack.ResponseDamage = clienttarget.InteractionEffect;
                            clienttarget.Send(attack);
                            attack.InteractType = 49;
                            client.SendScreen(attack, true);
                            attack.Attacker = client.Player.UID;
                            attack.Attacked = clienttarget.Player.UID;
                            client.SendScreen(attack, true);
                        }
                    }
                }
            }
        }
        public class InteractionStopEffect
        {
            public InteractionStopEffect(MsgInteract attack, Game.Player a_client)
            {
                Client.GameState client = a_client.Owner;
                if (Kernel.GamePool.ContainsKey(attack.Attacked))
                {
                    Client.GameState clienttarget = Kernel.GamePool[attack.Attacked];
                    attack.Attacker = client.Player.UID;
                    attack.Attacked = clienttarget.Player.UID;
                    attack.Damage = client.Player.InteractionType;
                    attack.X = client.Player.X;
                    attack.Y = client.Player.Y;
                    attack.InteractType = 50;
                    client.SendScreen(attack, true);
                    attack.Attacker = clienttarget.Player.UID;
                    attack.Attacked = client.Player.UID;
                    clienttarget.SendScreen(attack, true);
                    client.Player.Teleport(client.Player.MapID, client.Player.X, client.Player.Y);
                    clienttarget.Player.Teleport(clienttarget.Player.MapID, clienttarget.Player.X, clienttarget.Player.Y);
                    client.Player.InteractionType = 0;
                    client.Player.InteractionWith = 0;
                    client.Player.InteractionInProgress = false;
                    clienttarget.Player.InteractionType = 0;
                    clienttarget.Player.InteractionWith = 0;
                    clienttarget.Player.InteractionInProgress = false;
                }
            }
        }
        public class InteractionRefuse
        {
            public InteractionRefuse(MsgInteract attack, Game.Player a_client)
            {
                Client.GameState client = a_client.Owner;
                client.Player.InteractionType = 0;
                client.Player.InteractionWith = 0;
                client.Player.InteractionInProgress = false;
                if (Kernel.GamePool.ContainsKey(attack.Attacked))
                {
                    Client.GameState clienttarget = Kernel.GamePool[attack.Attacked];
                    attack.Attacker = clienttarget.Player.UID;
                    attack.InteractType = 48;
                    attack.X = clienttarget.Player.X;
                    attack.Y = clienttarget.Player.Y;
                    clienttarget.Send(attack);
                    clienttarget.Player.InteractionType = 0;
                    clienttarget.Player.InteractionWith = 0;
                    clienttarget.Player.InteractionInProgress = false;
                }
            }
        }
        #endregion
        private void Execute()
        {
            #region interactions
            if (attack != null)
            {
                switch (attack.InteractType)
                {
                    case MsgInteract.InteractionRequest:
                        {
                            new InteractionRequest(attack, attacker); return;
                        }
                    case MsgInteract.InteractionEffect:
                        {
                            new InteractionEffect(attack, attacker); return;
                        }
                    case MsgInteract.InteractionAccept:
                        {
                            new InteractionAccept(attack, attacker); return;
                        }
                    case MsgInteract.InteractionRefuse:
                        {
                            new InteractionRefuse(attack, attacker); return;
                        }
                    case MsgInteract.InteractionStopEffect:
                        {
                            new InteractionStopEffect(attack, attacker); return;
                        }
                }
            }
            #endregion
            #region Monster -> Player \ Monster
            if (attack == null)
            {
                if (attacker.PlayerFlag != PlayerFlag.Monster) return;
                if (attacker.Companion && attacker.Name != "Thundercloud")
                {
                    if (Constants.PKForbiddenMaps.Contains(attacker.MapID)) return;
                }
                if (attacked.PlayerFlag == PlayerFlag.Player)
                {
                    if (!attacked.Owner.Attackable) return;
                    if (attacked.Dead)
                        if (attacked.Dead)
                        {
                            attacked.Die(attacker);
                            return;
                        }
                    #region SnowBanhe
                    /* if (attacker.Name == "SnowBanshee" || attacker.Name == "SnowBansheeSoul" || attacker.Name == "PurpleBanshee")
                    {

                        uint rand = (uint)GameServer.Kernel.Random.Next(1, 8);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 10001;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 30010;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 30011;
                                break;
                            case 6:
                                attacker.MonsterInfo.SpellID = 30012;
                                break;
                        }
                        #region IceThrom AngerCop
                        if (attacker.MonsterInfo.SpellID == 30010 || attacker.MonsterInfo.SpellID == 10001)
                        {
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                        #region Chill
                        if (attacker.MonsterInfo.SpellID == 30011)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            attack = new MsgInteract(true);
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            suse.Effect = attack.Effect;
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                                attacked.Owner.FrightenStamp = Time32.Now;
                                var upd = new MsgRaceTrackStatus(true);
                                upd.UID = attacked.UID;
                                upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                                attacked.Owner.SendScreen(upd, true);
                                attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);

                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            foreach (var obj in attacked.Owner.Screen.Objects)
                            {
                                if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 14))
                                {
                                    if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                            continue;
                                        attacked = obj as Player;
                                        if (attacked.Hitpoints <= damage)
                                        {
                                            attacked.Die(attacker);
                                        }
                                        else
                                        {
                                            attacked.Hitpoints -= damage;
                                            attacked.Owner.FrightenStamp = Time32.Now;
                                            var upd = new MsgRaceTrackStatus(true);
                                            upd.UID = attacked.UID;
                                            upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                                            attacked.Owner.SendScreen(upd, true);
                                            attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);
                                        }

                                        suse.AddTarget(attacked, damage, attack);
                                    }
                                }
                            }
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                        #region AngerCrop
                        if (attacker.MonsterInfo.SpellID == 30012)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            attack = new MsgInteract(true);
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            suse.Effect = attack.Effect;
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                                attacked.Owner.Player.FrozenStamp = Time32.Now;
                                attacked.Owner.Player.FrozenTime = 5;
                                MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                update.UID = attacked.UID;
                                update.Add(MsgRaceTrackStatus.Freeze, 0, 5);
                                attacked.Owner.SendScreen(update, true);
                                attacked.AddFlag((ulong)PacketFlag.Flags.Freeze);
                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            foreach (var obj in attacked.Owner.Screen.Objects)
                            {
                                if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 10))
                                {
                                    if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                            continue;
                                        attacked = obj as Player;
                                        if (attacked.Hitpoints <= damage)
                                        {
                                            attacked.Die(attacker);
                                        }
                                        else
                                        {
                                            attacked.Hitpoints -= damage;
                                            attacked.Owner.Player.FrozenStamp = Time32.Now;
                                            attacked.Owner.Player.FrozenTime = 5;
                                            MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                            update.UID = attacked.UID;
                                            update.Add(MsgRaceTrackStatus.Freeze, 0, 5);
                                            attacked.Owner.SendScreen(update, true);
                                            attacked.AddFlag((ulong)PacketFlag.Flags.Freeze);
                                        }
                                        suse.AddTarget(attacked, damage, attack);
                                    }
                                }
                            }
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                    }*/
                    #endregion
                    #region NemesisTyrant
                    /* if (attacker.Name == "NemesisTyrant")
                    {
                        uint rand = (uint)Kernel.Random.Next(1, 22);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 10001;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 30010;
                                break;
                            case 3:
                                attacker.MonsterInfo.SpellID = 10001;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 30010;
                                break;
                            case 5:
                                attacker.MonsterInfo.SpellID = 30011;
                                break;
                            case 6:
                                attacker.MonsterInfo.SpellID = 30012;
                                break;
                            case 7:
                                attacker.MonsterInfo.SpellID = 7014;
                                break;
                            case 8:
                                attacker.MonsterInfo.SpellID = 7017;
                                break;
                            case 9:
                                attacker.MonsterInfo.SpellID = 7017;
                                break;
                            case 10:
                                attacker.MonsterInfo.SpellID = 7012;
                                break;
                            case 11:
                                attacker.MonsterInfo.SpellID = 7013;
                                break;
                            case 12:
                                attacker.MonsterInfo.SpellID = 7015;
                                break;
                            case 13:
                                attacker.MonsterInfo.SpellID = 7016;
                                break;
                            case 14:
                                attacker.MonsterInfo.SpellID = 10502;
                                break;
                            case 15:
                                attacker.MonsterInfo.SpellID = 10504;
                                break;
                            case 16:
                                attacker.MonsterInfo.SpellID = 10506;
                                break;
                            case 17:
                                attacker.MonsterInfo.SpellID = 10505;
                                break;
                            case 18:
                                attacker.MonsterInfo.SpellID = 10500;
                                break;
                            case 19:
                                attacker.MonsterInfo.SpellID = 10363;
                                break;
                            case 20:
                                attacker.MonsterInfo.SpellID = 10360;
                                attacked.AddFlag((ulong)PacketFlag.Flags.Stun);
                                attacked.ShockStamp = Time32.Now;
                                attacked.Shock = 5;
                                var upd = new MsgRaceTrackStatus(true);
                                upd.UID = attacked.UID;
                                upd.Add(MsgRaceTrackStatus.Dizzy, 0, 5);
                                attacked.Owner.SendScreen(upd, true);
                                break;
                            case 21:
                                attacker.MonsterInfo.SpellID = 10361;
                                attacked.AddFlag((ulong)PacketFlag.Flags.Stun);
                                attacked.ShockStamp = Time32.Now;
                                attacked.Shock = 5;
                                var upd1 = new MsgRaceTrackStatus(true);
                                upd1.UID = attacked.UID;
                                upd1.Add(MsgRaceTrackStatus.Dizzy, 0, 5);
                                attacked.Owner.SendScreen(upd1, true);
                                break;
                            case 22:
                                attacker.MonsterInfo.SpellID = 10362;
                                break;
                        }
                        #region IceThrom AngerCop
                        if (attacker.MonsterInfo.SpellID == 30010 || attacker.MonsterInfo.SpellID == 10001)
                        {
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                        #region Chill
                        if (attacker.MonsterInfo.SpellID == 30011)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            attack = new MsgInteract(true);
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            suse.Effect = attack.Effect;
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                                attacked.Owner.FrightenStamp = Time32.Now;
                                var upd = new MsgRaceTrackStatus(true);
                                upd.UID = attacked.UID;
                                upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                                attacked.Owner.SendScreen(upd, true);
                                attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);

                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            foreach (var obj in attacked.Owner.Screen.Objects)
                            {
                                if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 14))
                                {
                                    if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                            continue;
                                        attacked = obj as Player;
                                        if (attacked.Hitpoints <= damage)
                                        {
                                            attacked.Die(attacker);
                                        }
                                        else
                                        {
                                            attacked.Hitpoints -= damage;
                                            attacked.Owner.FrightenStamp = Time32.Now;
                                            var upd = new MsgRaceTrackStatus(true);
                                            upd.UID = attacked.UID;
                                            upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                                            attacked.Owner.SendScreen(upd, true);
                                            attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);
                                        }

                                        suse.AddTarget(attacked, damage, attack);
                                    }
                                }
                            }
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                        #region AngerCrop
                        if (attacker.MonsterInfo.SpellID == 30012)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            attack = new MsgInteract(true);
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            suse.Effect = attack.Effect;
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                                attacked.Owner.Player.FrozenStamp = Time32.Now;
                                attacked.Owner.Player.FrozenTime = 5;
                                MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                update.UID = attacked.UID;
                                update.Add(MsgRaceTrackStatus.Freeze, 0, 5);
                                attacked.Owner.SendScreen(update, true);
                                attacked.AddFlag((ulong)PacketFlag.Flags.Freeze);
                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            foreach (var obj in attacked.Owner.Screen.Objects)
                            {
                                if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 10))
                                {
                                    if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                            continue;
                                        attacked = obj as Player;
                                        if (attacked.Hitpoints <= damage)
                                        {
                                            attacked.Die(attacker);
                                        }
                                        else
                                        {
                                            attacked.Hitpoints -= damage;
                                            attacked.Owner.Player.FrozenStamp = Time32.Now;
                                            attacked.Owner.Player.FrozenTime = 5;
                                            MsgRaceTrackStatus update = new MsgRaceTrackStatus(true);
                                            update.UID = attacked.UID;
                                            update.Add(MsgRaceTrackStatus.Freeze, 0, 5);
                                            attacked.Owner.SendScreen(update, true);
                                            attacked.AddFlag((ulong)PacketFlag.Flags.Freeze);
                                        }

                                        suse.AddTarget(attacked, damage, attack);
                                    }
                                }
                            }
                            attacked.Owner.SendScreen(suse, true);
                        }
                        #endregion
                    }*/
                    #endregion
                    #region TreatoDragon
                    if (attacker.Name == "TeratoDragon" || attacker.Name == "LavaBeast" || attacker.Name == "ShadowClone" || attacker.Name == "SoulStrangler(Nor.)" || attacker.Name == "AlienDragon(Nor.)")
                    {
                        uint rand = (uint)COServer.Kernel.Random.Next(1, 7);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 7014;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 7017;
                                break;
                            case 3:
                                attacker.MonsterInfo.SpellID = 7017;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 7012;
                                break;
                            case 5:
                                attacker.MonsterInfo.SpellID = 7013;
                                break;
                            case 6:
                                attacker.MonsterInfo.SpellID = 7015;
                                break;
                            case 7:
                                attacker.MonsterInfo.SpellID = 7016;
                                break;
                        }
                        #region TD Area
                        if (attacker.MonsterInfo.SpellID == 7014 || attacker.MonsterInfo.SpellID == 7017)
                        {
                            MsgMagicEffect suse = new MsgMagicEffect(true);
                            attack = new MsgInteract(true);
                            attack.Effect = MsgInteract.InteractEffects.None;
                            uint damage = 0;
                            damage += (uint)Kernel.Random.Next(1000, 2000);
                            suse.Effect = attack.Effect;
                            if (attacked.Hitpoints <= damage)
                            {
                                attacked.Die(attacker);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage;
                            }
                            if (attacker.Companion)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                            suse.Attacker = attacker.UID;
                            suse.SpellID = attacker.MonsterInfo.SpellID;
                            suse.X = attacked.X;
                            suse.Y = attacked.Y;
                            suse.AddTarget(attacked, damage, attack);
                            foreach (var obj in attacked.Owner.Screen.Objects)
                            {
                                if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 14))
                                {
                                    if (obj.MapObjType == MapObjectType.Player)
                                    {
                                        if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                            continue;
                                        attacked = obj as Player;
                                        if (attacked.Hitpoints <= damage)
                                        {
                                            attacked.Die(attacker);
                                        }
                                        else
                                        {
                                            attacked.Hitpoints -= damage;
                                        }

                                        suse.AddTarget(attacked, damage, attack);
                                    }
                                }
                            }
                            attacked.Owner.SendScreen(suse, true);
                        }
                    }
                        #endregion
                    #region 2nd skill
                    if (attacker.MonsterInfo.SpellID == 7013)
                    {
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = 0;
                        damage += (uint)Kernel.Random.Next(1000, 2000);
                        suse.Effect = attack.Effect;
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                            attacked.Owner.FrightenStamp = Time32.Now;
                            attacked.Owner.Player.Fright = 5;
                            var upd = new MsgRaceTrackStatus(true);
                            upd.UID = attacked.UID;
                            upd.Add(MsgRaceTrackStatus.Dizzy, 0, 5);
                            attacked.Owner.SendScreen(upd, true);
                            attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.FreezeSmall);
                        }
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        foreach (var obj in attacked.Owner.Screen.Objects)
                        {
                            if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 14))
                            {
                                if (obj.MapObjType == MapObjectType.Player)
                                {
                                    if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                        continue;
                                    attacked = obj as Player;
                                    if (attacked.Hitpoints <= damage)
                                    {
                                        attacked.Die(attacker);
                                    }
                                    else
                                    {
                                        attacked.Hitpoints -= damage;
                                        attacked.Owner.FrightenStamp = Time32.Now;
                                        attacked.Owner.Player.Fright = 5;
                                        var upd = new MsgRaceTrackStatus(true);
                                        upd.UID = attacked.UID;
                                        upd.Add(MsgRaceTrackStatus.Dizzy, 0, 5);
                                        attacked.Owner.SendScreen(upd, true);
                                        attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.FreezeSmall);
                                    }

                                    suse.AddTarget(attacked, damage, attack);
                                }
                            }
                        }
                        attacked.Owner.SendScreen(suse, true);
                    }
                    #endregion
                    #region Chill
                    if (attacker.MonsterInfo.SpellID == 7015)
                    {
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = 0;
                        damage += (uint)Kernel.Random.Next(1000, 2000);
                        suse.Effect = attack.Effect;
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                            attacked.Owner.FrightenStamp = Time32.Now;
                            var upd = new MsgRaceTrackStatus(true);
                            upd.UID = attacked.UID;
                            upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                            attacked.Owner.SendScreen(upd, true);
                            attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);

                        }
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);

                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        foreach (var obj in attacked.Owner.Screen.Objects)
                        {
                            if (Calculations.InBox(obj.X, obj.Y, attacker.X, attacker.Y, 14))
                            {
                                if (obj.MapObjType == MapObjectType.Player)
                                {
                                    if (obj.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.Dead))
                                        continue;
                                    attacked = obj as Player;
                                    if (attacked.Hitpoints <= damage)
                                    {
                                        attacked.Die(attacker);
                                    }
                                    else
                                    {
                                        attacked.Hitpoints -= damage;
                                        attacked.Owner.FrightenStamp = Time32.Now;
                                        var upd = new MsgRaceTrackStatus(true);
                                        upd.UID = attacked.UID;
                                        upd.Add(MsgRaceTrackStatus.Flustered, 0, 5);
                                        attacked.Owner.SendScreen(upd, true);
                                        attacked.Owner.Player.AddFlag((ulong)PacketFlag.Flags.ChaosCycle);
                                    }

                                    suse.AddTarget(attacked, damage, attack);
                                }
                            }
                        }
                        attacked.Owner.SendScreen(suse, true);
                    }
                    #endregion
                    #endregion
                    #region SwordMaster
                    if (attacker.Name == "SwordMaster" || attacker.Name == "HollowBeast" || attacker.Name == "RuthlessAsura(Nor.)")
                    {
                        uint rand = (uint)COServer.Kernel.Random.Next(1, 5);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 10502;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 10504;
                                break;
                            case 3:
                                attacker.MonsterInfo.SpellID = 10506;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 10505;
                                break;
                            case 5:
                                attacker.MonsterInfo.SpellID = 10500;
                                break;
                        }
                        uint damage = 0;
                        damage += (uint)Kernel.Random.Next(3000, 3500);

                        if (attacked.Hitpoints <= damage)
                            attacked.Die(attacker);
                        else
                            attacked.Hitpoints -= damage;
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        attacked.Owner.SendScreen(suse, true);
                    }
                    #endregion
                    #region ThrillingSpook
                    if (attacker.Name == "ThrillingSpook" || attacker.Name == "ThrillingSpook2" || attacker.Name == "ThrillingSpook3" || attacker.Name == "DarkGlutton(Nor.)")
                    {
                        uint rand = (uint)COServer.Kernel.Random.Next(1, 7);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 10363;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 10360;
                                attacked.AddFlag((ulong)PacketFlag.Flags.Stun);
                                attacked.ShockStamp = Time32.Now;
                                attacked.Shock = 5;
                                var upd = new MsgRaceTrackStatus(true);
                                upd.UID = attacked.UID;
                                upd.Add(MsgRaceTrackStatus.SoulShacle, 0, 5);
                                attacked.Owner.SendScreen(upd, true);
                                break;
                            case 3:
                                attacker.MonsterInfo.SpellID = 10362;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 10363;
                                break;
                            case 5:
                                attacker.MonsterInfo.SpellID = 10363;
                                break;
                            case 6:
                                attacker.MonsterInfo.SpellID = 10362;
                                break;
                            case 7:
                                attacker.MonsterInfo.SpellID = 10361;
                                attacked.AddFlag((ulong)PacketFlag.Flags.Stun);
                                attacked.ShockStamp = Time32.Now;
                                attacked.Shock = 5;
                                var upd1 = new MsgRaceTrackStatus(true);
                                upd1.UID = attacked.UID;
                                upd1.Add(MsgRaceTrackStatus.SoulShacle, 0, 5);
                                attacked.Owner.SendScreen(upd1, true);
                                break;
                        }
                        uint damage = 0;
                        damage += (uint)Kernel.Random.Next(1000, 2000);
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                        }
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        attacked.Owner.SendScreen(suse, true);
                    }
                    #endregion
                    #region Guard1
                    if (attacker.Name == "Guard1")
                    {
                        uint rand = (uint)Kernel.Random.Next(1, 15);
                        switch (rand)
                        {
                            case 1:
                                attacker.MonsterInfo.SpellID = 7012;
                                break;
                            case 2:
                                attacker.MonsterInfo.SpellID = 7013;
                                break;
                            case 3:
                                attacker.MonsterInfo.SpellID = 7015;
                                break;
                            case 4:
                                attacker.MonsterInfo.SpellID = 7016;
                                break;
                            case 5:
                                attacker.MonsterInfo.SpellID = 7017;
                                break;
                            case 6:
                                attacker.MonsterInfo.SpellID = 10001;
                                break;
                            case 7:
                                attacker.MonsterInfo.SpellID = 10363;
                                break;
                            case 8:
                                attacker.MonsterInfo.SpellID = 10362;
                                break;
                            case 9:
                                attacker.MonsterInfo.SpellID = 10502;
                                break;
                            case 10:
                                attacker.MonsterInfo.SpellID = 10504;
                                break;
                            case 11:
                                attacker.MonsterInfo.SpellID = 10506;
                                break;
                            case 12:
                                attacker.MonsterInfo.SpellID = 10505;
                                break;
                            case 13:
                                attacker.MonsterInfo.SpellID = 30012;
                                break;
                            case 14:
                                attacker.MonsterInfo.SpellID = 10001;
                                break;
                            case 15:
                                attacker.MonsterInfo.SpellID = 30015;
                                break;
                        }
                        uint damage = 0;
                        damage += (uint)Kernel.Random.Next(50000, 60000);
                        if (attacked == null)
                            return;
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                        }
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        attacked.Owner.SendScreen(suse, true);
                    }
                    #endregion
                    #region ThunderCloud
                    if (attacker.Name == "Thundercloud")
                    {
                        if (Kernel.GamePool.ContainsKey(attacker.OwnerUID))
                        {
                            var owner = Kernel.GamePool[attacker.OwnerUID];
                            var spell = DB.SpellTable.GetSpell(12840, owner);
                            var spell2 = DB.SpellTable.GetSpell(12970, owner);
                            byte percent = 0;
                            if (spell2 != null)
                            {
                                if (spell2.Level == 0) percent = 130;
                                if (spell2.Level == 1) percent = 140;
                                if (spell2.Level == 2) percent = 150;
                                if (spell2.Level == 3) percent = 160;
                                if (spell2.Level == 4) percent = 170;
                                if (spell2.Level == 5) percent = 180;
                                if (spell2.Level == 6) percent = 200;
                            }
                            attack = new MsgInteract(true);
                            attack.Attacker = attacker.UID;
                            attack.Attacked = attacked.UID;
                            attack.InteractType = MsgInteract.Kill;
                            attack.X = attacked.X;
                            attack.Y = attacked.Y;
                            attack.Damage = 1;
                            uint damage2 = (uint)(Calculate.Melee(owner.Player, attacked, ref attack) * spell.FirstDamage / 100);
                            if (attacker.SpawnPacket[50] == 128)//ThunderBolt
                                damage2 = (uint)(damage2 * percent / 100);
                            MsgMagicEffect suse2 = new MsgMagicEffect(true);
                            suse2.Attacker = attacker.UID;
                            suse2.Attacker1 = attacked.UID;
                            suse2.SpellID = 13190;
                            suse2.X = attacked.X;
                            suse2.Y = attacked.Y;
                            suse2.AddTarget(attacked, damage2, attack);
                            attacker.MonsterInfo.SendScreen(suse2);
                            if (attacked.Hitpoints <= damage2)
                            {
                                attacked.Die(attacker);
                                attack.ResponseDamage = damage2;
                                attacker.MonsterInfo.SendScreen(attack);
                            }
                            else
                            {
                                attacked.Hitpoints -= damage2;
                            }
                            return;
                        }
                        else
                            return;
                    }
                    #endregion
                    #region ThunderCloudSight
                    foreach (var th in Kernel.Maps[attacker.MapID].Entities.Values.Where(i => i.Name == "Thundercloud"))
                    {
                        if (th.OwnerUID == attacked.UID)
                        {
                            if (attacker == null || Kernel.GetDistance(attacker.X, attacker.Y, th.X, th.Y) > th.MonsterInfo.AttackRange || attacker.Dead) break;
                            th.MonsterInfo.InSight = attacker.UID;
                            break;
                        }
                    }
                    #endregion
                    attack = new MsgInteract(true);
                    attack.Effect = MsgInteract.InteractEffects.None;
                    attack = new MsgInteract(true);
                    attack.Attacker = attacker.UID;
                    attack.Attacked = attacker.MonsterInfo.ID;
                    attack.X = attacked.X;
                    attack.Y = attacked.Y;
                    attack.InteractType = 52;
                    attack.MonsterSpellID = attacker.MonsterInfo.SpellID;
                    attacker.MonsterInfo.SendScreen(attack);
                    if (attacker.MonsterInfo.SpellID == 0)
                    {
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = Calculate.MonsterDamage(attacker, attacked, ref attack, false);
                        attack.Attacker = attacker.UID;
                        attack.Attacked = attacked.UID;
                        attack.InteractType = MsgInteract.Melee;
                        attack.Damage = damage;
                        attack.X = attacked.X;
                        attack.Y = attacked.Y;
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Owner.SendScreen(attack, true);
                            attacked.Die(attacker.UID);
                            if (attacked.InHangUp)
                            {
                                MsgHangUp AutoHunt = new MsgHangUp();
                                AutoHunt.Action = MsgHangUp.Mode.KilledBy;
                                AutoHunt.Unknown = 3329;
                                AutoHunt.KilledName = attacker.MonsterInfo.Name;
                                AutoHunt.EXPGained = attacked.HangUpEXP;
                                attacked.Owner.Send(AutoHunt.ToArray());
                                attacked.AutoRevStamp = Time32.Now;
                                attacked.AutoRev = 20;
                            }
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                            attacked.Owner.SendScreen(attack, true);
                        }
                    }
                    else
                    {
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = Calculate.MonsterDamage(attacker, attacked, attacker.MonsterInfo.SpellID);
                        suse.Effect = attack.Effect;
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker.UID);
                            if (attacked.InHangUp)
                            {
                                MsgHangUp AutoHunt = new MsgHangUp();
                                AutoHunt.Action = MsgHangUp.Mode.KilledBy;
                                AutoHunt.Unknown = 3329;
                                AutoHunt.KilledName = attacker.MonsterInfo.Name;
                                AutoHunt.EXPGained = attacked.HangUpEXP;
                                attacked.Owner.Send(AutoHunt.ToArray());
                                attacked.AutoRevStamp = Time32.Now;
                                attacked.AutoRev = 20;
                            }
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                        }
                        if (attacker.Companion)
                            attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        attacked.Owner.SendScreen(suse, true);
                    }
                }
                else
                {
                    if (attacker.MonsterInfo.SpellID == 0)
                    {
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = Calculate.MonsterDamage(attacker, attacked, ref attack, false);
                        attack.Attacker = attacker.UID;
                        attack.Attacked = attacked.UID;
                        attack.InteractType = MsgInteract.Melee;
                        attack.Damage = damage;
                        attack.X = attacked.X;
                        attack.Y = attacked.Y;
                        attacked.MonsterInfo.SendScreen(attack);
                        if (attacker.Companion)
                            if (damage > attacked.Hitpoints)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                            else attacker.Owner.IncreaseExperience(damage, true);
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                            attack = new MsgInteract(true);
                            attack.Attacker = attacker.UID;
                            attack.Attacked = attacked.UID;
                            attack.InteractType = Network.GamePackets.MsgInteract.Kill;
                            attack.X = attacked.X;
                            attack.Y = attacked.Y;
                            attacked.MonsterInfo.SendScreen(attack);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                        }
                    }
                    else
                    {
                        #region ThunderCloud
                        if (attacker.Name == "Thundercloud")
                        {
                            if (Kernel.GamePool.ContainsKey(attacker.OwnerUID))
                            {
                                var owner = Kernel.GamePool[attacker.OwnerUID];
                                var spell = SpellTable.GetSpell(12840, owner);
                                var spell2 = SpellTable.GetSpell(12970, owner);
                                uint damage2 = (uint)(Calculate.Melee(owner.Player, attacked, ref attack, 0) * (spell.Level < 8 ? spell.FirstDamage : 50) / 100);
                                if (attacker.SpawnPacket[50] == 128)
                                    damage2 = (uint)(damage2 * spell2.FirstDamage);
                                MsgMagicEffect suse2 = new MsgMagicEffect(true);
                                suse2.Attacker = attacker.UID;
                                suse2.Attacker1 = attacked.UID;
                                suse2.SpellID = 13190;
                                suse2.X = attacked.X;
                                suse2.Y = attacked.Y;
                                suse2.AddTarget(attacked, damage2, attack);
                                attacker.MonsterInfo.SendScreen(suse2);
                                if (attacked.Hitpoints <= damage2)
                                {
                                    attacked.Die(attacker);
                                    attack = new MsgInteract(true);
                                    attack.Attacker = attacker.UID;
                                    attack.Attacked = attacked.UID;
                                    attack.InteractType = MsgInteract.Kill;
                                    attack.X = attacked.X;
                                    attack.Y = attacked.Y;
                                    attack.Damage = 1;
                                    attack.ResponseDamage = damage2;
                                    attacker.MonsterInfo.SendScreen(attack);
                                }
                                else
                                {
                                    attacked.Hitpoints -= damage2;
                                }
                                return;
                            }
                            else
                                return;
                        }
                        #endregion
                        MsgMagicEffect suse = new MsgMagicEffect(true);
                        attack = new MsgInteract(true);
                        attack.Effect = MsgInteract.InteractEffects.None;
                        uint damage = Calculate.MonsterDamage(attacker, attacked, attacker.MonsterInfo.SpellID);
                        suse.Effect = attack.Effect;
                        suse.Attacker = attacker.UID;
                        suse.SpellID = attacker.MonsterInfo.SpellID;
                        suse.X = attacked.X;
                        suse.Y = attacked.Y;
                        suse.AddTarget(attacked, damage, attack);
                        attacked.MonsterInfo.SendScreen(suse);
                        if (attacker.Companion)
                            if (damage > attacked.Hitpoints)
                                attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                            else attacker.Owner.IncreaseExperience(damage, true);
                        if (attacked.Hitpoints <= damage)
                        {
                            attacked.Die(attacker);
                            attack = new MsgInteract(true);
                            attack.Attacker = attacker.UID;
                            attack.Attacked = attacked.UID;
                            attack.InteractType = MsgInteract.Kill;
                            attack.X = attacked.X;
                            attack.Y = attacked.Y;
                            attacked.MonsterInfo.SendScreen(attack);
                        }
                        else
                        {
                            attacked.Hitpoints -= damage;
                        }
                    }
                }
            }
            #endregion
            #region Player -> Player \ Monster \ Sob Npc
            else
            {
                #region Merchant
                if (attack.InteractType == MsgInteract.MerchantAccept || attack.InteractType == MsgInteract.MerchantRefuse)
                {
                    attacker.AttackPacket = null;
                    return;
                }
                #endregion
                #region Marriage
                if (attack.InteractType == MsgInteract.MarriageAccept || attack.InteractType == MsgInteract.MarriageRequest)
                {
                    if (attack.InteractType == MsgInteract.MarriageRequest)
                    {
                        Client.GameState Spouse = null;
                        uint takeout = attack.Attacked;
                        if (takeout == attacker.UID)
                            takeout = attack.Attacker;
                        if (Kernel.GamePool.TryGetValue(takeout, out Spouse))
                        {
                            MsgRelation Relation = new MsgRelation();
                            Relation.Requester = attacker.UID;
                            Relation.Receiver = Spouse.Player.UID;
                            Relation.Level = attacker.Level;
                            Relation.BattlePower = (uint)attacker.BattlePower;
                            Relation.Spouse = attacker.Name == Spouse.Player.Spouse;
                            Relation.Friend = attacker.Owner.Friends.ContainsKey(Spouse.Player.UID);
                            Relation.TradePartner = attacker.Owner.Partners.ContainsKey(Spouse.Player.UID);
                            if (attacker.Owner.Mentor != null)
                                Relation.Mentor = attacker.Owner.Mentor.ID == Spouse.Player.UID;
                            Relation.Apprentice = attacker.Owner.Apprentices.ContainsKey(Spouse.Player.UID);
                            if (attacker.Owner.Team != null)
                                Relation.Teammate = attacker.Owner.Team.IsTeammate(Spouse.Player.UID);
                            if (attacker.Owner.Guild != null)
                                Relation.GuildMember = attacker.Owner.Guild.Members.ContainsKey(Spouse.Player.UID);
                            Relation.Enemy = attacker.Owner.Enemy.ContainsKey(Spouse.Player.UID);
                            Spouse.Send(Relation);

                            if (attacker.Spouse != "None" || Spouse.Player.Spouse != "None")
                            {
                                attacker.Owner.Send(new MsgTalk("You cannot marry someone that is already married with someone else!", Color.Black, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                            }
                            else
                            {
                                uint id1 = attacker.Mesh % 10, id2 = Spouse.Player.Mesh % 10;
                                if (id1 <= 2 && id2 >= 3 || id1 >= 2 && id2 <= 3)
                                {
                                    attack.X = Spouse.Player.X;
                                    attack.Y = Spouse.Player.Y;
                                    Spouse.Send(attack);
                                }
                                else
                                {
                                    attacker.Owner.Send(new MsgTalk("You cannot marry someone of your gender!", Color.Black, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                                }
                            }
                        }
                    }
                    else
                    {
                        Client.GameState Spouse = null;
                        if (Kernel.GamePool.TryGetValue(attack.Attacked, out Spouse))
                        {
                            if (attacker.Spouse != "None" || Spouse.Player.Spouse != "None")
                            {
                                attacker.Owner.Send(new MsgTalk("You cannot marry someone that is already married with someone else!", Color.Black, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                            }
                            else
                            {
                                if (attacker.Mesh % 10 <= 2 && Spouse.Player.Mesh % 10 >= 3 || attacker.Mesh % 10 >= 3 && Spouse.Player.Mesh % 10 <= 2)
                                {
                                    Spouse.Player.Spouse = attacker.Name;
                                    attacker.Spouse = Spouse.Player.Name;
                                    MsgTalk message = null;
                                    if (Spouse.Player.Mesh % 10 >= 3)
                                        message = new MsgTalk("Joy and happiness! " + Spouse.Player.Name + " and " + attacker.Name + " have joined together in the holy marriage. We wish them a stone house.", Color.BurlyWood, (uint)PacketMsgTalk.MsgTalkType.Center);
                                    else message = new MsgTalk("Joy and happiness! " + attacker.Name + " and " + attacker.Spouse + " have joined together in the holy marriage. We wish them a stone house.", Color.BurlyWood, (uint)PacketMsgTalk.MsgTalkType.Center);
                                    foreach (Client.GameState client in Server.GamePool)
                                    {
                                        client.Send(message);
                                    }
                                    Spouse.Player.Update(MsgName.Mode.Effect, "firework-2love", true);
                                    attacker.Update(MsgName.Mode.Effect, "firework-2love", true);
                                }
                                else
                                {
                                    attacker.Owner.Send(new MsgTalk("You cannot marry someone of your gender!", System.Drawing.Color.Black, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                                }
                            }
                        }
                    }
                }
                #endregion
                #region Attacking
                else
                {
                    attacker.Owner.Attackable = true;
                    Player attacked = null;
                    MsgNpcInfoEX attackedsob = null;
                    #region Checks
                    if (attack.Attacker != attacker.UID) return;
                    if (attacker.PlayerFlag != PlayerFlag.Player) return;
                    attacker.RemoveFlag((ulong)PacketFlag.Flags.Invisibility);
                    bool pass = false;
                    if (attack.InteractType == MsgInteract.Melee)
                    {
                        if (attacker.OnFatalStrike())
                        {
                            if (attack.Attacked < 600000)
                            {
                                pass = true;
                            }
                        }
                    }
                    ushort decrease = 0;
                    if (attacker.OnCyclone())
                        decrease = 1;
                    if (attacker.OnSuperman())
                        decrease = 300;
                    if (!pass && attack.InteractType != MsgInteract.Magic)
                    {
                        int milliSeconds = 1000 - decrease;
                        if (milliSeconds < 0 || milliSeconds > 5000)
                            milliSeconds = 0;
                        if (Time32.Now < attacker.AttackStamp.AddMilliseconds(milliSeconds)) return;
                        attacker.AttackStamp = Time32.Now;
                    }
                    if (attacker.Dead)
                    {
                        if (attacker.AttackPacket != null)
                            attacker.AttackPacket = null; return;
                    }
                    if (attacker.Owner.InQualifier())
                    {
                        if (Time32.Now < attacker.Owner.ImportTime().AddSeconds(12))
                        {
                            return;
                        }
                    }
                    //if (attacker.SkillTeamWatchingElitePKMatch != null)
                    //{
                    //    if (Time32.Now < attacker.SkillTeamWatchingElitePKMatch.ImportTime.AddSeconds(4))
                    //    {

                    //    }
                    //}
                    bool doWep1Spell = false, doWep2Spell = false;
                restart:
                    #region Extract attack information
                    ushort SpellID = 0, X = 0, Y = 0;
                    uint Target = 0;
                    if (attack.InteractType == MsgInteract.Magic)
                    {
                        if (!attack.Decoded)
                        {
                            #region GetSkillID
                            SpellID = Convert.ToUInt16(((long)attack.ToArray()[24 + 4] & 0xFF) | (((long)attack.ToArray()[25 + 4] & 0xFF) << 8));
                            SpellID ^= (ushort)0x915d;
                            SpellID ^= (ushort)attacker.UID;
                            SpellID = (ushort)(SpellID << 0x3 | SpellID >> 0xd);
                            SpellID -= 0xeb42;
                            #endregion
                            #region GetCoords
                            X = (ushort)((attack.ToArray()[16 + 4] & 0xFF) | ((attack.ToArray()[17 + 4] & 0xFF) << 8));
                            X = (ushort)(X ^ (uint)(attacker.UID & 0xffff) ^ 0x2ed6);
                            X = (ushort)(((X << 1) | ((X & 0x8000) >> 15)) & 0xffff);
                            X = (ushort)((X | 0xffff0000) - 0xffff22ee);

                            Y = (ushort)((attack.ToArray()[18 + 4] & 0xFF) | ((attack.ToArray()[19 + 4] & 0xFF) << 8));
                            Y = (ushort)(Y ^ (uint)(attacker.UID & 0xffff) ^ 0xb99b);
                            Y = (ushort)(((Y << 5) | ((Y & 0xF800) >> 11)) & 0xffff);
                            Y = (ushort)((Y | 0xffff0000) - 0xffff8922);
                            #endregion
                            #region GetTarget
                            Target = ((uint)attack.ToArray()[12 + 4] & 0xFF) | (((uint)attack.ToArray()[13 + 4] & 0xFF) << 8) | (((uint)attack.ToArray()[14 + 4] & 0xFF) << 16) | (((uint)attack.ToArray()[15 + 4] & 0xFF) << 24);
                            Target = ((((Target & 0xffffe000) >> 13) | ((Target & 0x1fff) << 19)) ^ 0x5F2D2463 ^ attacker.UID) - 0x746F4AE6;
                            #endregion

                            attack.X = X;
                            attack.Y = Y;
                            attack.Damage = SpellID;
                            attack.Attacked = Target;
                            attack.Decoded = true;
                        }
                        else
                        {
                            X = attack.X;
                            Y = attack.Y;
                            SpellID = (ushort)attack.Damage;
                            Target = attack.Attacked;
                        }
                    }
                    #endregion
                    if (!pass && attack.InteractType == MsgInteract.Magic)
                    {
                        if (!(doWep1Spell || doWep2Spell))
                        {
                            if (SpellID == 1045 || SpellID == 1046 || SpellID == 11005 || SpellID == 11000 || SpellID == 1100) // FB and SS
                            {
                                //do checks
                            }
                            else
                            {
                                int Agility = attacker.Agility;
                                if (Agility > 320) Agility = 320;
                                int milliSeconds = 1000 - Agility - decrease;
                                if (milliSeconds < 0 || milliSeconds > 5000)
                                    milliSeconds = 0;
                                if (Time32.Now < attacker.AttackStamp.AddMilliseconds(milliSeconds)) return;
                            }

                            attacker.AttackStamp = Time32.Now;
                        }
                    }
                    #endregion
                    if (attacker.MapID == SteedRace.MAPID)
                    {
                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                        {
                            attacker.Owner.Send(new NpcReply(NpcReply.MessageBox, "Do you want to quit the steed race?"));
                            attacker.Owner.MessageOK = (pClient) =>
                            {
                                pClient.Player.Teleport(1002, 302, 278);
                                pClient.Player.RemoveFlag((ulong)Core.Packet.PacketFlag.Flags.Ride);
                            };
                            attacker.Owner.MessageCancel = null;
                        }
                        return;
                    }
                    if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Ride) && attacker.Owner.Equipment.TryGetItem(18) == null)
                    {
                        if (attack.InteractType != MsgInteract.Magic)
                            attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                        else if (!(SpellID == 7003 || SpellID == 7002))
                            attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                    }
                    if (attacker.ContainsFlag((ulong)PacketFlag.Flags.CastPray))
                        attacker.RemoveFlag((ulong)PacketFlag.Flags.CastPray);
                    if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Praying))
                        attacker.RemoveFlag((ulong)PacketFlag.Flags.Praying);
                    #region Dash
                    if (SpellID == 1051)
                    {
                        if (Kernel.GetDistance(attack.X, attack.Y, attacker.X, attacker.Y) > 4)
                        {
                            attacker.Owner.Disconnect(); return;
                        }
                        attacker.X = attack.X; attacker.Y = attack.Y;
                        ushort x = attacker.X, y = attacker.Y;
                        Game.Map.UpdateCoordonatesForAngle(ref x, ref y, (Enums.ConquerAngle)Target);
                        foreach (Interfaces.IMapObject obj in attacker.Owner.Screen.Objects)
                        {
                            if (obj == null) continue;
                            if (obj.X == x && obj.Y == y && (obj.MapObjType == MapObjectType.Monster || obj.MapObjType == MapObjectType.Player))
                            {
                                Player entity = obj as Player;
                                if (!entity.Dead)
                                {
                                    Target = obj.UID;
                                    break;
                                }
                            }
                        }
                    }
                    #endregion
                    #region CounterKill
                    if (attack.InteractType == MsgInteract.CounterKillSwitch)
                    {
                        if (attacked != null)
                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) { attacker.AttackPacket = null; return; }
                        if (attacker != null)
                            if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Fly)) { attacker.AttackPacket = null; return; }
                        if (attacker.Owner.Spells.ContainsKey(6003))
                        {
                            if (!attacker.CounterKillSwitch)
                            {
                                if (Time32.Now >= attacker.CounterKillStamp.AddSeconds(30))
                                {
                                    attacker.CounterKillStamp = Time32.Now;
                                    attacker.CounterKillSwitch = true;
                                    MsgInteract m_attack = new MsgInteract(true);
                                    m_attack.Attacked = attacker.UID;
                                    m_attack.Attacker = attacker.UID;
                                    m_attack.InteractType = MsgInteract.CounterKillSwitch;
                                    m_attack.Damage = 1;
                                    m_attack.X = attacker.X;
                                    m_attack.Y = attacker.Y;
                                    m_attack.Send(attacker.Owner);
                                }
                            }
                            else
                            {
                                attacker.CounterKillSwitch = false;
                                MsgInteract m_attack = new MsgInteract(true);
                                m_attack.Attacked = attacker.UID;
                                m_attack.Attacker = attacker.UID;
                                m_attack.InteractType = MsgInteract.CounterKillSwitch;
                                m_attack.Damage = 0;
                                m_attack.X = attacker.X;
                                m_attack.Y = attacker.Y;
                                m_attack.Send(attacker.Owner);
                            }

                            attacker.Owner.IncreaseSpellExperience(100, 6003);
                            attacker.AttackPacket = null;
                        }
                    }
                    #endregion
                    #region Melee Antiguo
                    //else if (attack.InteractType == MsgInteract.Melee)
                    //{
                    //    if (attacker.MapID == 9987)
                    //    {
                    //        attacker.Owner.Send(new MsgTalk("You have to use manual linear skills(FastBlade/ScentSword)", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk));
                    //        return;
                    //    }
                    //    if (attacker.MapID == 1707)
                    //    {
                    //        attacker.Owner.Send(new MsgTalk("You have to use manual linear skills(FastBlade/ScentSword)", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk));
                    //        return;
                    //    }
                    //    if (attacker.Owner.Screen.TryGetValue(attack.Attacked, out attacked))
                    //    {
                    //        CheckForExtraWeaponPowers(attacker.Owner, attacked);
                    //        if (!CanAttack(attacker, attacked, null, attack.InteractType == MsgInteract.Melee)) return;
                    //        pass = false;
                    //        if (attacker.OnFatalStrike())
                    //        {
                    //            if (attacked.PlayerFlag == PlayerFlag.Monster)
                    //            {
                    //                pass = true;
                    //            }
                    //        }
                    //        ushort range = attacker.AttackRange;
                    //        if (attacker.Transformed)
                    //            range = (ushort)attacker.TransformationAttackRange;
                    //        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= range || pass)
                    //        {
                    //            #region Saint~Monk
                    //            if (MyMath.Success(50))
                    //            {
                    //                if (attack.SpellID == 12580 || attack.SpellID == 12590 || attack.SpellID == 12600)
                    //                {
                    //                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    //                    suse.Attacker = attacker.UID;
                    //                    suse.SpellID = 0;
                    //                    suse.SpellLevel = 0;
                    //                    suse.X = attacker.X;
                    //                    suse.Y = attacker.Y;
                    //                    foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                    //                    {
                    //                        if (_obj == null) continue;
                    //                        if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                    //                        {
                    //                            attacked = _obj as Player;
                    //                            if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= 7)
                    //                            {
                    //                                if (CanAttack(attacker, attacked, null, attack.InteractType == MsgInteract.Melee))
                    //                                {
                    //                                    attack.Effect = MsgInteract.InteractEffects.None;
                    //                                    uint damages = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                    //                                    attack.Attacked = attacked.UID;
                    //                                    attack.Damage = damages;
                    //                                    suse.Effect = attack.Effect;
                    //                                    ReceiveAttack(attacker, attacked, attack, ref damages, null);
                    //                                }
                    //                            }
                    //                        }
                    //                    }

                    //                    attacker.AttackPacket = null;
                    //                    attack = null;
                    //                    return;
                    //                }
                    //            }
                    //            #endregion
                    //            #region Earth Sweep
                    //            if (attack.SpellID == 12220 || attack.SpellID == 12210)
                    //            {
                    //                MsgMagicEffect suse = new MsgMagicEffect(true);
                    //                suse.Attacker = attacker.UID;
                    //                suse.SpellID = 12220;
                    //                suse.SpellLevel = 0;
                    //                suse.X = attacker.X;
                    //                suse.Y = attacker.Y;
                    //                Fan fan = new Fan(attacker.X, attacker.Y, attacked.X, attacked.Y, 7, 180);
                    //                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                    //                {
                    //                    if (_obj == null) continue;
                    //                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                    //                    {
                    //                        attacked = _obj as Player;
                    //                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= 7)
                    //                        {
                    //                            if (CanAttack(attacker, attacked, null, attack.InteractType == MsgInteract.Melee))
                    //                            {
                    //                                attack.Effect = MsgInteract.InteractEffects.None;
                    //                                uint damages = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                    //                                attack.Attacked = 0;
                    //                                suse.Effect = attack.Effect;
                    //                                ReceiveAttack(attacker, attacked, attack, ref damages, null);
                    //                                suse.AddTarget(attacked, damages, attack);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //                attacker.Owner.SendScreen(suse, true);
                    //                attacker.AttackPacket = null;
                    //                attack = null;
                    //                return;
                    //            }
                    //            #endregion
                    //            #region Stomper(Melee)
                    //            #region Circle~Sector
                    //            if (attack.SpellID == 13040 || attack.SpellID == 13050)
                    //            {
                    //                var sus1 = new MsgMagicEffect(true);
                    //                sus1.Attacker = attacker.UID;
                    //                sus1.SpellID = (ushort)attack.SpellID;
                    //                sus1.X = attack.X;
                    //                sus1.Y = attack.Y;
                    //                var spell = DB.SpellTable.GetSpell((ushort)attack.SpellID, 0);
                    //                //Fan fan = new Fan(attacker.X, attacker.Y, attack.X + 3, attack.Y + 3, spell.Range, spell.Sector);
                    //                Sector fan = new Sector(attacker.X, attacker.Y, attack.X, attack.Y);
                    //                fan.Arrange(spell.Sector, spell.Range);
                    //                foreach (var obj in attacker.Owner.Screen.Objects.Where(e => e.MapObjType == MapObjectType.Monster || e.MapObjType == MapObjectType.Player))
                    //                {
                    //                    attacked = obj as Player;
                    //                    if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= 3)
                    //                    {
                    //                        if (fan.Inside(attacked.X, attacked.Y))
                    //                        {
                    //                            if (CanAttack(attacker, attacked, spell, true))
                    //                            {
                    //                                //  if (attacked.UID == attack.Attacked) continue;
                    //                                sus1.Effect = attack.Effect = MsgInteract.InteractEffects.None;
                    //                                uint damage2 = Calculate.Melee(attacker, attacked, spell, ref attack) / 2;
                    //                                attack.Damage = attack.ResponseDamage = damage2;
                    //                                ReceiveAttack(attacker, attacked, attack, ref damage2, spell);
                    //                                sus1.AddTarget(attacked, damage2, attack);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //                attacker.Owner.SendScreen(sus1, false);
                    //            }
                    //            #endregion
                    //            var lastattacked = attacked;
                    //            var spell5 = DB.SpellTable.GetSpell(12980, attacker.Owner);
                    //            if (Kernel.Rate(spell5.Percent) && attacker.Owner.Spells.ContainsKey(12980) && attacker.IsStomper2())
                    //            {
                    //                #region AngerofStomper
                    //                {
                    //                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    //                    suse.Attacker = attacker.UID;
                    //                    suse.SpellID = spell5.ID;
                    //                    suse.SpellLevel = spell5.Level;
                    //                    suse.X = lastattacked.X;
                    //                    suse.Y = lastattacked.Y;
                    //                    foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                    //                    {
                    //                        if (_obj == null)
                    //                            continue;
                    //                        attacked = _obj as Player;
                    //                        if (attacked == null) continue;
                    //                        if (Kernel.GetDistance(attacked.X, attacked.Y, attacker.X, attacker.Y) <= spell5.Range)
                    //                        {
                    //                            if (_obj.MapObjType == MapObjectType.Player)
                    //                            {
                    //                                if (!CanAttack(attacker, attacked, null, attack.InteractType == MsgInteract.Melee)) continue;
                    //                                attack.Effect = MsgInteract.InteractEffects.None;
                    //                                uint damage2 = Game.Attacking.Calculate.Melee(attacker, attacked, spell5, ref attack) / 2;
                    //                                suse.Effect = attack.Effect;
                    //                                attack.Damage = 0;
                    //                                ReceiveAttack(attacker, attacked, attack, ref damage2, spell5);
                    //                                suse.AddTarget(attacked, damage2, attack);

                    //                            }
                    //                            else if (_obj.MapObjType == MapObjectType.Monster)
                    //                            {
                    //                                if (!CanAttack(attacker, attacked, null, attack.InteractType == MsgInteract.Melee)) continue;
                    //                                uint damage2 = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                    //                                attack.Damage = 0;
                    //                                ReceiveAttack(attacker, attacked, attack, ref damage2, spell5);
                    //                                suse.AddTarget(attacked, damage2, attack);
                    //                            }
                    //                        }

                    //                    }
                    //                    attacker.AttackPacket = null;
                    //                    attacker.Owner.SendScreen(suse, true);

                    //                }
                    //                #endregion
                    //                #region HorrorofStomper
                    //                {
                    //                    var spell2 = DB.SpellTable.GetSpell(12990, attacker.Owner);
                    //                    if (spell2 == null) return;
                    //                    attack.Damage = 0;
                    //                    MsgMagicEffect S = new MsgMagicEffect(true);
                    //                    S.Attacker = attacker.UID;
                    //                    S.SpellID = spell2.ID;
                    //                    S.SpellLevel = spell2.Level;
                    //                    S.X = lastattacked.X;
                    //                    S.Y = lastattacked.Y;
                    //                    attacker.AttackPacket = null;
                    //                    attacker.Owner.SendScreen(S, true);
                    //                    #region Flooritem
                    //                    var map = Kernel.Maps[attacker.MapID];
                    //                    Network.GamePackets.MsgMapItem F = new Network.GamePackets.MsgMapItem(true);
                    //                    F.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    while (map.Npcs.ContainsKey(F.UID))
                    //                        F.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    F.ItemID = 1530;
                    //                    F.X = lastattacked.X;
                    //                    F.Y = lastattacked.Y;
                    //                    F.MapObjType = MapObjectType.Item;
                    //                    F.Type = PacketMsgMapItem.Effect;
                    //                    F.mColor = 14;
                    //                    F.OwnerUID = attacker.UID;
                    //                    F.OwnerGuildUID = attacker.GuildID;
                    //                    F.FlowerType = 3;
                    //                    F.unknow = 1;
                    //                    F.Start = attacker.X;
                    //                    F.End = attacker.Y;
                    //                    F.MapID = map.ID;
                    //                    F.Owner = attacker.Owner;
                    //                    F.OnFloor = Time32.Now;
                    //                    F.Owner = attacker.Owner;
                    //                    map.AddFloorItem(F);
                    //                    attacker.Owner.SendScreenSpawn(F, true);
                    //                    #endregion
                    //                    #region
                    //                    var mapp = Kernel.Maps[attacker.MapID];
                    //                    Network.GamePackets.MsgMapItem FF = new Network.GamePackets.MsgMapItem(true);
                    //                    FF.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    while (mapp.Npcs.ContainsKey(FF.UID))
                    //                        FF.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    FF.ItemID = 1530;
                    //                    FF.Attack = attack;
                    //                    FF.X = lastattacked.X;
                    //                    FF.Y = lastattacked.Y;
                    //                    FF.Type = 10;
                    //                    FF.mColor = 14;
                    //                    FF.OwnerUID = attacker.UID;
                    //                    FF.OwnerGuildUID = attacker.GuildID;
                    //                    FF.FlowerType = 2;
                    //                    FF.MapID = map.ID;
                    //                    FF.Start = 409;
                    //                    FF.Now = 26625;
                    //                    FF.End = 360;
                    //                    FF.Owner = attacker.Owner;
                    //                    FF.OnFloor = Time32.Now;
                    //                    FF.Owner = attacker.Owner;
                    //                    mapp.AddFloorItem(FF);
                    //                    attacker.Owner.SendScreenSpawn(FF, true);
                    //                    #endregion

                    //                }
                    //                #endregion
                    //                #region PeaceofStomper
                    //                {
                    //                    var spell3 = DB.SpellTable.GetSpell(13000, attacker.Owner);
                    //                    if (spell3 == null) return;
                    //                    attack.Damage = 0;
                    //                    MsgMagicEffect suse = new MsgMagicEffect(true);
                    //                    suse.Attacker = attacker.UID;
                    //                    suse.SpellLevel = spell3.Level;
                    //                    suse.SpellID = spell3.ID;
                    //                    suse.X = lastattacked.X;
                    //                    suse.Y = lastattacked.Y;
                    //                    attacker.AttackPacket = null;
                    //                    attacker.Owner.SendScreen(suse, true);
                    //                    #region Flooritem
                    //                    var map = Kernel.Maps[attacker.MapID];
                    //                    Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                    //                    flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    while (map.Npcs.ContainsKey(flooritem.UID))
                    //                        flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                    //                    flooritem.ItemID = 1540;
                    //                    flooritem.Attack = attack;
                    //                    flooritem.X = lastattacked.X;
                    //                    flooritem.Y = lastattacked.Y;
                    //                    flooritem.Start = attacker.X;
                    //                    flooritem.End = attacker.Y;
                    //                    flooritem.MapObjType = MapObjectType.Item;
                    //                    flooritem.Type = PacketMsgMapItem.Effect;
                    //                    flooritem.mColor = 14;
                    //                    flooritem.OwnerUID = attacker.UID;
                    //                    flooritem.OwnerGuildUID = attacker.GuildID;
                    //                    flooritem.FlowerType = 3;
                    //                    flooritem.unknow = 1;
                    //                    flooritem.MapID = map.ID;
                    //                    flooritem.Owner = attacker.Owner;
                    //                    flooritem.OnFloor = Time32.Now;
                    //                    flooritem.Owner = attacker.Owner;
                    //                    map.AddFloorItem(flooritem);
                    //                    attacker.Owner.SendScreenSpawn(flooritem, true);
                    //                    #endregion
                    //                }
                    //                #endregion
                    //                return;
                    //            }
                    //            #endregion
                    //            attack.Effect = MsgInteract.InteractEffects.None;
                    //            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                    //            attack.Damage = damage;
                    //            if (attacker.OnFatalStrike())
                    //            {
                    //                if (attacked.PlayerFlag == PlayerFlag.Monster)
                    //                {
                    //                    var weaps = attacker.Owner.Weapons;
                    //                    bool can = false;
                    //                    if (weaps.Item1 != null)
                    //                        if (weaps.Item1.ID / 1000 == 601 || weaps.Item1.ID / 1000 == 616)
                    //                            can = true;
                    //                    if (weaps.Item2 != null)
                    //                        if (weaps.Item2.ID / 1000 == 601 || weaps.Item2.ID / 1000 == 616)
                    //                            can = true;
                    //                    if (!can) return;
                    //                    ushort x = attacked.X;
                    //                    ushort y = attacked.Y;
                    //                    Map.UpdateCoordonatesForAngle(ref x, ref y, Kernel.GetAngle(attacked.X, attacked.Y, attacker.X, attacker.Y));
                    //                    attacker.Shift(x, y);
                    //                    attack.X = x;
                    //                    attack.Y = y;
                    //                    attack.Damage = damage / 3;
                    //                    attack.InteractType = MsgInteract.FatalStrike;
                    //                }
                    //            }
                    //            var weapons = attacker.Owner.Weapons;
                    //            if (weapons.Item1 != null)
                    //            {
                    //                MsgItemInfo rightweapon = weapons.Item1;
                    //                ushort wep1subyte = (ushort)(rightweapon.ID / 1000), wep2subyte = 0;
                    //                bool wep1bs = false, wep2bs = false;
                    //                if (wep1subyte == 421)
                    //                {
                    //                    wep1bs = true;
                    //                    wep1subyte--;
                    //                }
                    //                ushort wep1spellid = 0, wep2spellid = 0;
                    //                if (DB.SpellTable.WeaponSpells.ContainsKey(wep1subyte))
                    //                {
                    //                    DB.SpellTable.WeaponSpells[wep1subyte].Shuffle();
                    //                    wep1spellid = DB.SpellTable.WeaponSpells[wep1subyte].FirstOrDefault();
                    //                }
                    //                DB.SpellInformation wep1spell = null, wep2spell = null;
                    //                if (attacker.Owner.Spells.ContainsKey(wep1spellid) && DB.SpellTable.SpellInformations.ContainsKey(wep1spellid))
                    //                {
                    //                    wep1spell = DB.SpellTable.SpellInformations[wep1spellid][attacker.Owner.Spells[wep1spellid].Level];
                    //                    doWep1Spell = MyMath.Success(wep1spell.Percent);
                    //                    //if (attacked.PlayerFlag == PlayerFlag.Player && wep1spellid == 10490)
                    //                    //    doWep1Spell = MyMath.Success(45);
                    //                }
                    //                if (!doWep1Spell)
                    //                {
                    //                    if (weapons.Item2 != null)
                    //                    {
                    //                        MsgItemInfo leftweapon = weapons.Item2;
                    //                        wep2subyte = (ushort)(leftweapon.ID / 1000);
                    //                        if (wep2subyte == 421)
                    //                        {
                    //                            wep2bs = true;
                    //                            wep2subyte--;
                    //                        }
                    //                        if (DB.SpellTable.WeaponSpells.ContainsKey(wep2subyte))
                    //                        {
                    //                            DB.SpellTable.WeaponSpells[wep2subyte].Shuffle();
                    //                            wep2spellid = DB.SpellTable.WeaponSpells[wep2subyte].FirstOrDefault();
                    //                        }
                    //                        if (attacker.Owner.Spells.ContainsKey(wep2spellid) && DB.SpellTable.SpellInformations.ContainsKey(wep2spellid))
                    //                        {
                    //                            wep2spell = DB.SpellTable.SpellInformations[wep2spellid][attacker.Owner.Spells[wep2spellid].Level];
                    //                            doWep2Spell = MyMath.Success(wep2spell.Percent);
                    //                            //if (attacked.PlayerFlag == PlayerFlag.Player && wep2spellid == 10490)
                    //                            //    doWep2Spell = MyMath.Success(45);
                    //                        }
                    //                    }
                    //                }
                    //                if (!attacker.Transformed)
                    //                {
                    //                    if (doWep1Spell)
                    //                    {
                    //                        attack.InteractType = MsgInteract.Magic;
                    //                        attack.Decoded = true;
                    //                        attack.CheckWeponSpell = true;
                    //                        attack.X = attacked.X;
                    //                        attack.Y = attacked.Y;
                    //                        attack.Attacked = attacked.UID;
                    //                        attack.Damage = wep1spell.ID;
                    //                        goto restart;
                    //                    }
                    //                    if (doWep2Spell)
                    //                    {
                    //                        attack.InteractType = MsgInteract.Magic;
                    //                        attack.Decoded = true;
                    //                        attack.CheckWeponSpell = true;
                    //                        attack.X = attacked.X;
                    //                        attack.Y = attacked.Y;
                    //                        attack.Attacked = attacked.UID;
                    //                        attack.Damage = wep2spell.ID;
                    //                        goto restart;
                    //                    }
                    //                    if (wep1bs)
                    //                        wep1subyte++;
                    //                    if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag != PlayerFlag.Player)
                    //                        if (damage > attacked.Hitpoints)
                    //                        {
                    //                            attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attacked.Hitpoints), wep1subyte);
                    //                            if (wep2subyte != 0)
                    //                            {
                    //                                if (wep2bs)
                    //                                    wep2subyte++;
                    //                                attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attacked.Hitpoints), wep2subyte);
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            attacker.Owner.IncreaseProficiencyExperience(damage, wep1subyte);
                    //                            if (wep2subyte != 0)
                    //                            {
                    //                                if (wep2bs)
                    //                                    wep2subyte++;
                    //                                attacker.Owner.IncreaseProficiencyExperience(damage, wep2subyte);
                    //                            }
                    //                        }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (!attacker.Transformed)
                    //                {
                    //                    if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag != PlayerFlag.Player)
                    //                        if (damage > attacked.Hitpoints)
                    //                        {
                    //                            attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attacked.Hitpoints), 0);
                    //                        }
                    //                        else
                    //                        {
                    //                            attacker.Owner.IncreaseProficiencyExperience(damage, 0);
                    //                        }
                    //                }
                    //            }
                    //            ReceiveAttack(attacker, attacked, attack, ref damage, null);
                    //            attack.InteractType = MsgInteract.Melee;
                    //        }
                    //        else
                    //        {
                    //            attacker.AttackPacket = null;
                    //        }
                    //    }
                    //    else if (attacker.Owner.Screen.TryGetSob(attack.Attacked, out attackedsob))
                    //    {
                    //        CheckForExtraWeaponPowers(attacker.Owner, null);
                    //        if (CanAttack(attacker, attackedsob, null))
                    //        {
                    //            ushort range = attacker.AttackRange;
                    //            if (attacker.Transformed)
                    //                range = (ushort)attacker.TransformationAttackRange;
                    //            if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= range)
                    //            {
                    //                attack.Effect = MsgInteract.InteractEffects.None;
                    //                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                    //                var weapons = attacker.Owner.Weapons;
                    //                if (weapons.Item1 != null)
                    //                {
                    //                    MsgItemInfo rightweapon = weapons.Item1;
                    //                    ushort wep1subyte = (ushort)(rightweapon.ID / 1000), wep2subyte = 0;
                    //                    bool wep1bs = false, wep2bs = false;
                    //                    if (wep1subyte == 421)
                    //                    {
                    //                        wep1bs = true;
                    //                        wep1subyte--;
                    //                    }
                    //                    ushort wep1spellid = 0, wep2spellid = 0;
                    //                    if (DB.SpellTable.WeaponSpells.ContainsKey(wep1subyte))
                    //                    {
                    //                        DB.SpellTable.WeaponSpells[wep1subyte].Shuffle();
                    //                        wep1spellid = DB.SpellTable.WeaponSpells[wep1subyte].FirstOrDefault();
                    //                    }
                    //                    DB.SpellInformation wep1spell = null, wep2spell = null;
                    //                    if (attacker.Owner.Spells.ContainsKey(wep1spellid) && DB.SpellTable.SpellInformations.ContainsKey(wep1spellid))
                    //                    {
                    //                        wep1spell = DB.SpellTable.SpellInformations[wep1spellid][attacker.Owner.Spells[wep1spellid].Level];
                    //                        doWep1Spell = MyMath.Success(wep1spell.Percent);
                    //                    }
                    //                    if (!doWep1Spell)
                    //                    {
                    //                        if (weapons.Item2 != null)
                    //                        {
                    //                            MsgItemInfo leftweapon = weapons.Item2;
                    //                            wep2subyte = (ushort)(leftweapon.ID / 1000);
                    //                            if (wep2subyte == 421)
                    //                            {
                    //                                wep2bs = true;
                    //                                wep2subyte--;
                    //                            }
                    //                            if (DB.SpellTable.WeaponSpells.ContainsKey(wep2subyte))
                    //                            {
                    //                                DB.SpellTable.WeaponSpells[wep2subyte].Shuffle();
                    //                                wep2spellid = DB.SpellTable.WeaponSpells[wep2subyte].FirstOrDefault();
                    //                            }
                    //                            if (attacker.Owner.Spells.ContainsKey(wep2spellid) && DB.SpellTable.SpellInformations.ContainsKey(wep2spellid))
                    //                            {
                    //                                wep2spell = DB.SpellTable.SpellInformations[wep2spellid][attacker.Owner.Spells[wep2spellid].Level];
                    //                                doWep2Spell = MyMath.Success(wep2spell.Percent);
                    //                            }
                    //                        }
                    //                    }
                    //                    if (!attacker.Transformed)
                    //                    {
                    //                        if (doWep1Spell)
                    //                        {
                    //                            attack.InteractType = MsgInteract.Magic;
                    //                            attack.Decoded = true;
                    //                            attack.CheckWeponSpell = true;
                    //                            attack.X = attackedsob.X;
                    //                            attack.Y = attackedsob.Y;
                    //                            attack.Attacked = attackedsob.UID;
                    //                            attack.Damage = wep1spell.ID;
                    //                            goto restart;
                    //                        }
                    //                        if (doWep2Spell)
                    //                        {
                    //                            attack.InteractType = MsgInteract.Magic;
                    //                            attack.Decoded = true;
                    //                            attack.CheckWeponSpell = true;
                    //                            attack.X = attackedsob.X;
                    //                            attack.Y = attackedsob.Y;
                    //                            attack.Attacked = attackedsob.UID;
                    //                            attack.Damage = wep2spell.ID;
                    //                            goto restart;
                    //                        }
                    //                        if (attacker.MapID == 1039)
                    //                        {
                    //                            if (wep1bs)
                    //                                wep1subyte++;
                    //                            if (attacker.PlayerFlag == PlayerFlag.Player)
                    //                                if (damage > attackedsob.Hitpoints)
                    //                                {
                    //                                    attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attackedsob.Hitpoints), wep1subyte);
                    //                                    if (wep2subyte != 0)
                    //                                    {
                    //                                        if (wep2bs)
                    //                                            wep2subyte++;
                    //                                        attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attackedsob.Hitpoints), wep2subyte);
                    //                                    }
                    //                                }
                    //                                else
                    //                                {
                    //                                    attacker.Owner.IncreaseProficiencyExperience(damage, wep1subyte);
                    //                                    if (wep2subyte != 0)
                    //                                    {
                    //                                        if (wep2bs)
                    //                                            wep2subyte++;
                    //                                        attacker.Owner.IncreaseProficiencyExperience(damage, wep2subyte);
                    //                                    }
                    //                                }
                    //                        }
                    //                    }
                    //                }
                    //                attack.Damage = damage;
                    //                ReceiveAttack(attacker, attackedsob, attack, damage, null);
                    //            }
                    //            else
                    //            {
                    //                attacker.AttackPacket = null;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        attacker.AttackPacket = null;
                    //    }
                    //}
                    #endregion
                    #region Melee Nuevo
                    if (attack.InteractType == MsgInteract.Melee)
                    {
                        ushort wep1type = 0, wep2type = 0;
                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.ShurikenVortex))
                            return;
                            #region Base Variables
                        attacker.AttackStamp = Time32.Now;
                        if (attacker.Owner.Screen.TryGetValue(attack.Attacked, out attacked) || attacker.Owner.Screen.TryGetSob(attack.Attacked, out attackedsob))
                        {
                            if (attacked != null && (attacked.Dead || attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly) || !CanAttack(attacker, attacked, null, true)))
                            {
                                attacker.AttackPacket = null;
                                return;
                            }
                            uint damage = 0;
                            var dist = 3;
                            if (attacker.Owner.Equipment.TryGetItem(MsgItemInfo.RightWeapon, out MsgItemInfo Item))
                                if (Item != null)
                                    if (ItemHandler.IsTwoHand(Item.ID))
                                        dist = 4;

                            if (attacked != null && Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) > dist)
                            {
                                attacker.AttackPacket = null;
                                return;
                            }
                            if (attackedsob != null && (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) > dist || !CanAttack(attacker, attackedsob, null)))
                            {
                                attacker.AttackPacket = null;
                                return;
                            }
                            var weapons = attacker.Owner.Weapons;
                            MsgItemInfo rightweapon = weapons.Item1;
                            MsgItemInfo leftweapon = weapons.Item2;
                            bool HandleSecondWeapon = true;
                            #endregion
                            #region RightWeapon

                            if (rightweapon != null)
                            {
                                ushort subtype = (ushort)(rightweapon.ID / 1000);
                                wep1type = subtype;
                                if (subtype == 421)
                                    subtype--;
                                if (HandleSecondWeapon && SpellTable.WeaponSpells.ContainsKey(subtype))
                                {
                                    subtype = SpellTable.WeaponSpells[subtype].FirstOrDefault();
                                    if (attacker.Owner.Spells.ContainsKey(subtype))
                                    {
                                        var spell = SpellTable.SpellInformations[subtype][attacker.Owner.Spells[subtype].Level];
                                        if (spell != null)
                                        {
                                            //if(Kernel.Rate(90))
                                            if (Kernel.Rate(10 + attacker.AjustHitRate(spell, attacked != null ? attacked : null)))//AdjustHitRate Erroneo
                                            {
                                                attack.InteractType = MsgInteract.Magic;
                                                attack.MagicType = spell.ID;
                                                attack.MagicLevel = spell.Level;

                                                MsgInteract.Check(attacker, attack);
                                               
                                                if (attacker.Owner.Account.State == AccountTable.AccountState.Administrator)
                                                {
                                                    attacker.Owner.Send("Spell ID: " + spell.ID);
                                                }
                                                if (attacked != null && !attacked.Dead || attackedsob != null)
                                                {
                                                    attack.InteractType = MsgInteract.Melee;
                                                    attacker.AttackPacket = attack;
                                                }
                                                return;

                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region LeftWeapon
                            if (HandleSecondWeapon && leftweapon != null)
                            {
                                ushort subtype = (ushort)(leftweapon.ID / 1000);
                                wep2type = subtype;
                                if (subtype == 421)
                                    subtype--;
                                if (wep2type != wep1type)
                                {
                                    if (SpellTable.WeaponSpells.ContainsKey(subtype))
                                    {
                                        subtype = SpellTable.WeaponSpells[subtype].FirstOrDefault();
                                        if (attacker.Owner.Spells.ContainsKey(subtype))
                                        {
                                            SpellInformation spell = SpellTable.SpellInformations[subtype][attacker.Owner.Spells[subtype].Level];
                                            if (spell != null)
                                            {
                                                //   if (Kernel.Rate(90))
                                                if (Kernel.Rate(20 + attacker.AjustHitRate(spell, attacked != null ? attacked : null)))
                                                {
                                                    attack.InteractType = MsgInteract.Magic;
                                                    attack.MagicType = spell.ID;
                                                    attack.MagicLevel = spell.Level;
                                                    MsgInteract.Check(attacker, attack);
                                                  
                                                    if (attacker.Owner.Account.State == AccountTable.AccountState.Administrator)
                                                    {
                                                        attacker.Owner.Send("Spell ID1: " + spell.ID);
                                                    }
                                                    if (attacked != null && !attacked.Dead || attackedsob != null)
                                                    {
                                                        attack.InteractType = MsgInteract.Melee;
                                                        attacker.AttackPacket = attack;
                                                    }
                                                    return;

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Normal DMG

                            if (attacked != null && attacked.PlayerFlag == PlayerFlag.Player)
                            {

                                CheckForExtraWeaponPowers(attacker.Owner, attacked);
                                pass = false;
                                if (attacker.OnFatalStrike())
                                {
                                    if (attacked.PlayerFlag == PlayerFlag.Monster)
                                    {
                                        pass = true;
                                    }
                                }
                                var d = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) * 0.6;
                                if(attacker.Class >= 80 && attacker.Class <= 85)
                                {
                                    damage = damage * 2;
                                }
                                damage = (uint)Math.Max(d, 1);
                                if(attacker.BattlePower >= 400 && attacker.BattlePower <= 405)
                                {
                                    if(damage < 30000)
                                    {
                                        damage = damage * 2;
                                    }
                                }
                
                                if (attacker.OnFatalStrike())
                                {
                                    if (attacked.PlayerFlag == PlayerFlag.Monster)
                                    {
                                        var weaps = attacker.Owner.Weapons;
                                        bool can = false;
                                        if (weaps.Item1 != null)
                                            if (weaps.Item1 != null) if (weaps.Item1.ID / 1000 == 601 || weaps.Item1.ID / 1000 == 616) can = true; if (weaps.Item2 != null) if (weaps.Item2.ID / 1000 == 601 || weaps.Item2.ID / 1000 == 616) can = true;
                                        can = true;
                                        if (weaps.Item2 != null)
                                            if (weaps.Item1 != null) if (weaps.Item1.ID / 1000 == 601 || weaps.Item1.ID / 1000 == 616) can = true; if (weaps.Item2 != null) if (weaps.Item2.ID / 1000 == 601 || weaps.Item2.ID / 1000 == 616) can = true;
                                        can = true;
                                        if (!can)
                                            return;
                                        ushort x = attacked.X;
                                        ushort y = attacked.Y;
                                        Map.UpdateCoordonatesForAngle(ref x, ref y, Kernel.GetAngle(attacked.X, attacked.Y, attacker.X, attacker.Y));
                                        attacker.Shift(x, y);
                                        attack.X = x;
                                        attack.Y = y;
                                        attack.Damage = damage;

                                        attack.InteractType = MsgInteract.FatalStrike;
                                    }
                                }
                                #region Acomodado Fisicos [Player]
                                if (attacked.PlayerFlag == PlayerFlag.Player)
                                {
                                    //if (attacker.Class >= 10 && attacker.Class <= 15)
                                    //{
                                    //    damage = damage
                                    //}
                                    //if (attacker.IsStomper1() && attacker.Class >= 160 && attacker.Class <= 165)
                                    //{
                                    //    damage = damage
                                    //}
                                    //if (attacker.Class >= 70 && attacker.Class <= 75)
                                    //{
                                    //    damage = damage;
                                    //}
                                    if (attacker.Class >= 20 && attacker.Class <= 25)
                                    {
                                        if (attacker.BattlePower >= 400 && attacker.BattlePower <= 405)
                                        {
                                            if (damage < 30000)
                                            {
                                                damage = damage * 2;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                #region Acomodado Fisicos [Monster]
                                if (attacked.PlayerFlag == PlayerFlag.Monster)
                                {
                                    damage = damage / 4;
                                    attack.Damage = damage;
                                }
                                #endregion
                            }
                            else if (attacked != null && attacked.PlayerFlag == PlayerFlag.Monster)
                            {
                                CheckForExtraWeaponPowers(attacker.Owner, attacked);
                                pass = false;
                                if (attacker.OnFatalStrike())
                                {
                                    if (attacked.PlayerFlag == PlayerFlag.Monster)
                                    {
                                        pass = true;
                                    }
                                }
                                var d = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) * 0.85;
                                damage = (uint)Math.Max(d, 1);
                                if (attacker.OnFatalStrike())
                                {
                                    if (attacked.PlayerFlag == PlayerFlag.Monster)
                                    {
                                        var weaps = attacker.Owner.Weapons;
                                        bool can = false;
                                        if (weaps.Item1 != null)
                                            if (weaps.Item1 != null) if (weaps.Item1.ID / 1000 == 601 || weaps.Item1.ID / 1000 == 616) can = true; if (weaps.Item2 != null) if (weaps.Item2.ID / 1000 == 601 || weaps.Item2.ID / 1000 == 616) can = true;
                                        can = true;
                                        if (weaps.Item2 != null)
                                            if (weaps.Item1 != null) if (weaps.Item1.ID / 1000 == 601 || weaps.Item1.ID / 1000 == 616) can = true; if (weaps.Item2 != null) if (weaps.Item2.ID / 1000 == 601 || weaps.Item2.ID / 1000 == 616) can = true;
                                        can = true;
                                        if (!can)
                                            return;
                                        ushort x = attacked.X;
                                        ushort y = attacked.Y;
                                        Map.UpdateCoordonatesForAngle(ref x, ref y, Kernel.GetAngle(attacked.X, attacked.Y, attacker.X, attacker.Y));
                                        attacker.Shift(x, y);
                                        attack.X = x;
                                        attack.Y = y;
                                        attack.Damage = damage;

                                        attack.InteractType = MsgInteract.FatalStrike;
                                    }
                                }
                            }
                            else
                            {
                                damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                            }
                            attack.Damage = damage;
                            #endregion
                            #region Proficiency DMG
                            if (damage > 0)
                            {
                                if (attacked != null && attacked.PlayerFlag == PlayerFlag.Monster)
                                {
                                    attacker.Owner.IncreaseProficiencyExperience(Math.Min(attacked.Hitpoints, damage), wep1type);
                                    if (wep2type != wep1type && wep2type != 0)
                                        attacker.Owner.IncreaseProficiencyExperience(Math.Min(attacked.Hitpoints, damage), wep2type);
                                }
                                else if (attackedsob != null)
                                {
                                    uint profdmg = Math.Min(attackedsob.Hitpoints, damage);
                                    if (attacker.MapID == 1039)
                                        profdmg /= 10;
                                    attacker.Owner.IncreaseProficiencyExperience(Math.Min(attackedsob.Hitpoints, profdmg), wep1type);
                                    if (wep2type != wep1type)
                                        attacker.Owner.IncreaseProficiencyExperience(Math.Min(attackedsob.Hitpoints, profdmg), wep2type);

                                }
                            }
                            #endregion
                            #region Deal DMG
                            if (attacked != null && (attacked.PlayerFlag == PlayerFlag.Player || attacked.PlayerFlag == PlayerFlag.Monster))
                                ReceiveAttack(attacker, attacked, attack, ref damage, null);
                            else
                                ReceiveAttack(attacker, attackedsob, attack, damage, null);
                            #endregion
                            attacker.AttackPacket = attack;
                        }
                        else
                            attacker.AttackPacket = null;
                    }
                    #endregion
                    #region Ranged
                    else if (attack.InteractType == MsgInteract.Ranged)
                    {
                        if (attacker.MapID == 1707)
                        {
                            attacker.Owner.Send(new MsgTalk("You have to use manual linear skills(FastBlade/ScentSword)", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk));
                            return;
                        }
                        if (attacker.MapID == 9987)
                        {
                            attacker.Owner.Send(new MsgTalk("You have to use manual linear skills(FastBlade/ScentSword)", System.Drawing.Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk));
                            return;
                        }
                        if (attacker.Owner.Screen.TryGetValue(attack.Attacked, out attacked))
                        {
                            CheckForExtraWeaponPowers(attacker.Owner, attacked);
                            if (!CanAttack(attacker, attacked, null, false)) return;
                            var weapons = attacker.Owner.Weapons;
                            if (weapons.Item1 == null) return;
                            if (weapons.Item1.ID / 1000 != 500 && weapons.Item1.ID / 1000 != 613 && weapons.Item1.ID / 1000 != 626) return;
                            if (weapons.Item1.ID / 1000 == 500)
                                if (weapons.Item2 != null)
                                    if (!ItemHandler.IsArrow(weapons.Item2.ID)) return;
                            #region Kinetic Spark
                            if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.KineticSpark))
                            {
                                var spell = DB.SpellTable.GetSpell(11590, attacker.Owner);
                                if (spell != null)
                                {
                                    spell.CanKill = true;
                                    if (MyMath.Success(spell.Percent))
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = attacker.X;
                                        suse.Y = attacker.Y;
                                        IMapObject lastAttacked = attacker;
                                        uint p = 0;
                                        if (Handle.CanAttack(attacker, attacked, spell, false))
                                        {
                                            lastAttacked = attacked;
                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                            suse.Effect = attack.Effect;
                                            if (attacker.SpiritFocus)
                                            {
                                                damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                attacker.SpiritFocus = false;
                                            }
                                            damage = damage - damage * (p += 20) / 100;
                                            Handle.ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                            suse.AddTarget(attacked, damage, attack);
                                        }
                                        foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                        {
                                            if (_obj == null) continue;
                                            if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                            {
                                                if (_obj.UID == attacked.UID) continue;
                                                var attacked1 = _obj as Player;
                                                if (Kernel.GetDistance(lastAttacked.X, lastAttacked.Y, attacked1.X, attacked1.Y) <= 5)
                                                {
                                                    if (Handle.CanAttack(attacker, attacked1, spell, false))
                                                    {
                                                        lastAttacked = attacked1;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked1, spell, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        if (attacker.SpiritFocus)
                                                        {
                                                            damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                            attacker.SpiritFocus = false;
                                                        }
                                                        damage = damage - damage * (p += 20) / 100;
                                                        if (damage == 0) break;
                                                        ReceiveAttack(attacker, attacked1, attack, ref damage, spell);
                                                        suse.AddTarget(attacked1, damage, attack);
                                                    }
                                                }
                                            }
                                            else if (_obj.MapObjType == MapObjectType.SobNpc)
                                            {
                                                attackedsob = _obj as MsgNpcInfoEX;
                                                if (Kernel.GetDistance(lastAttacked.X, lastAttacked.Y, attackedsob.X, attackedsob.Y) <= 5)
                                                {
                                                    if (Handle.CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        lastAttacked = attackedsob;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        if (attacker.SpiritFocus)
                                                        {
                                                            damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                            attacker.SpiritFocus = false;
                                                        }
                                                        damage = damage - damage * (p += 20) / 100;
                                                        if (damage == 0) break;
                                                        Handle.ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            #region ShadowofChaser
                            if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.ShadowofChaser) && attacker.IsChaser())
                            {
                                var spell = SpellTable.GetSpell(13090, attacker.Owner);
                                if (spell != null)
                                {
                                    spell.CanKill = true;
                                    if (Kernel.Rate(spell.Percent))
                                    {
                                        ShadowofChaser(attacker, attacked, attack, 1);
                                        return;
                                    }
                                }
                            }
                            #endregion
                            if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= InfoFile.pScreenDistance)
                            {
                                attack.Effect = MsgInteract.InteractEffects.None;
                                uint damage = 0;
                                if (!attacker.Assassin())
                                    damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack) / 4;
                                else damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) / 2;
                                attack.Damage = damage;
                                if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag != PlayerFlag.Player)
                                    if (damage > attacked.Hitpoints)
                                    {
                                        attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attacked.Hitpoints), (ushort)(weapons.Item1.ID / 1000));
                                    }
                                    else
                                    {
                                        attacker.Owner.IncreaseProficiencyExperience(damage, (ushort)(weapons.Item1.ID / 1000));
                                    }
                                ReceiveAttack(attacker, attacked, attack, ref damage, null);
                            }
                        }
                        else if (attacker.Owner.Screen.TryGetSob(attack.Attacked, out attackedsob))
                        {
                            if (CanAttack(attacker, attackedsob, null))
                            {
                                if (attacker.Owner.Equipment.TryGetItem(MsgItemInfo.LeftWeapon) == null) return;
                                var weapons = attacker.Owner.Weapons;
                                if (weapons.Item1 == null) return;
                                if (weapons.Item1.ID / 1000 != 500 && weapons.Item1.ID / 1000 != 613 && weapons.Item1.ID / 1000 != 626) return;

                                if (attacker.MapID != 1039)
                                    if (weapons.Item1.ID / 1000 == 500)
                                        if (weapons.Item2 != null)
                                            if (!ItemHandler.IsArrow(weapons.Item2.ID)) return;
                                #region Kinetic Spark
                                if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.KineticSpark))
                                {
                                    var spell = DB.SpellTable.GetSpell(11590, attacker.Owner);
                                    if (spell != null)
                                    {
                                        spell.CanKill = true;
                                        if (MyMath.Success(spell.Percent))
                                        {
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = attacker.X;
                                            suse.Y = attacker.Y;

                                            IMapObject lastAttacked = attacker;
                                            uint p = 0;
                                            if (Handle.CanAttack(attacker, attackedsob, spell))
                                            {
                                                lastAttacked = attackedsob;
                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                suse.Effect = attack.Effect;
                                                if (attacker.SpiritFocus)
                                                {
                                                    damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                    attacker.SpiritFocus = false;
                                                }
                                                damage = damage - damage * (p += 20) / 100;
                                                Handle.ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                suse.AddTarget(attackedsob, damage, attack);
                                            }
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    var attacked1 = _obj as Player;
                                                    if (Kernel.GetDistance(lastAttacked.X, lastAttacked.Y, attacked1.X, attacked1.Y) <= 5)
                                                    {
                                                        if (Handle.CanAttack(attacker, attacked1, spell, false))
                                                        {
                                                            lastAttacked = attacked1;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked1, spell, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            if (attacker.SpiritFocus)
                                                            {
                                                                damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                attacker.SpiritFocus = false;
                                                            }
                                                            damage = damage - damage * (p += 20) / 100;
                                                            if (damage == 0) break;
                                                            ReceiveAttack(attacker, attacked1, attack, ref damage, spell);
                                                            suse.AddTarget(attacked1, damage, attack);
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    if (_obj.UID == Target) continue;
                                                    var attackedsob1 = _obj as MsgNpcInfoEX;
                                                    if (Kernel.GetDistance(lastAttacked.X, lastAttacked.Y, attackedsob1.X, attackedsob1.Y) <= 5)
                                                    {
                                                        if (Handle.CanAttack(attacker, attackedsob1, spell))
                                                        {
                                                            lastAttacked = attackedsob1;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob1, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            if (attacker.SpiritFocus)
                                                            {
                                                                damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                attacker.SpiritFocus = false;
                                                            }
                                                            damage = damage - damage * (p += 20) / 100;
                                                            if (damage == 0) break;
                                                            Handle.ReceiveAttack(attacker, attackedsob1, attack, damage, spell);
                                                            suse.AddTarget(attackedsob1, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                            return;
                                        }
                                    }
                                }
                                #endregion
                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= InfoFile.pScreenDistance)
                                {
                                    attack.Effect = MsgInteract.InteractEffects.None;
                                    uint damage = 0;
                                    if (!attacker.Assassin())
                                        damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack) / 2;
                                    else damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                    attack.Damage = damage;
                                    ReceiveAttack(attacker, attackedsob, attack, damage, null);
                                    if (damage > attackedsob.Hitpoints)
                                    {
                                        attacker.Owner.IncreaseProficiencyExperience(Math.Min(damage, attackedsob.Hitpoints), (ushort)(weapons.Item1.ID / 1000));
                                    }
                                    else
                                    {
                                        attacker.Owner.IncreaseProficiencyExperience(damage, (ushort)(weapons.Item1.ID / 1000));
                                    }
                                }
                            }
                        }
                        else
                        {
                            attacker.AttackPacket = null;
                        }
                    }
                    #endregion
                    #region Magic
                    else if (attack.InteractType == MsgInteract.Magic)
                    {
                       // if (SpellTable.cheakrspell.ContainsKey(SpellID) && !attack.CheckWeponSpell) return;
                        if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.DragonFury)) return;
                        #region BreathFocus
                        if (MyMath.Success(30))
                        {
                            if (attacker.Owner != null)
                            {
                                if (attacker.Owner.Spells.ContainsKey(11960))
                                {
                                    if (attacker.Owner.AlternateEquipment)
                                    {
                                        if (attacker.Owner.Equipment.Free(MsgItemInfo.RightWeapon))
                                        {
                                            UInt32 iType = attacker.Owner.Equipment.TryGetItem(MsgItemInfo.AlternateRightWeapon).ID / 1000;
                                            if (iType == 614)
                                            {
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = 11960;
                                                suse.SpellLevel = attacker.Owner.Spells[11960].Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                if (attacker.HeavenBlessing == 0)
                                                {
                                                    attacker.Stamina = (byte)Math.Min((int)(attacker.Stamina + 20), 100);
                                                }
                                                else
                                                    attacker.Stamina = (byte)Math.Min((int)(attacker.Stamina + 20), 150);
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!attacker.Owner.Equipment.Free(MsgItemInfo.RightWeapon))
                                        {
                                            UInt32 iType = attacker.Owner.Equipment.TryGetItem(MsgItemInfo.RightWeapon).ID / 1000;
                                            if (iType == 614)
                                            {
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = 11960;
                                                suse.SpellLevel = attacker.Owner.Spells[11960].Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                if (attacker.HeavenBlessing == 0)
                                                {
                                                    attacker.Stamina = (byte)Math.Min((int)(attacker.Stamina + 20), 100);
                                                }
                                                else
                                                    attacker.Stamina = (byte)Math.Min((int)(attacker.Stamina + 20), 150);
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        #endregion
                        CheckForExtraWeaponPowers(attacker.Owner, attacked);
                        uint Experience = 100;
                        bool shuriken = false;
                        ushort spellID = SpellID;
                        if (SpellID >= 3090 && SpellID <= 3306)
                            spellID = 3090;
                        if (spellID == 6012)
                            shuriken = true;
                        if (attacker == null) return;
                        if (attacker.Owner == null)
                        {
                            attacker.AttackPacket = null; return;
                        }
                        if (attacker.Owner.Spells == null)
                        {
                            attacker.Owner.Spells = new SafeDictionary<ushort, Interfaces.ISkill>(10000);
                            attacker.AttackPacket = null; return;
                        }
                        if (attacker.Owner.Spells[spellID] == null && spellID != 6012 && spellID != 10320)
                        {
                            attacker.AttackPacket = null; return;
                        }
                        DB.SpellInformation spell = null;
                        if (shuriken)
                            spell = DB.SpellTable.SpellInformations[6010][0];
                        else
                        {
                            byte choselevel = 0;
                            if (spellID == SpellID)
                                choselevel = attacker.Owner.Spells[spellID].Level;
                            if (DB.SpellTable.SpellInformations[SpellID] != null && !DB.SpellTable.SpellInformations[SpellID].ContainsKey(choselevel))
                                choselevel = (byte)(DB.SpellTable.SpellInformations[SpellID].Count - 1);
                            spell = DB.SpellTable.SpellInformations[SpellID][choselevel];
                        }
                        if (spell == null)
                        {
                            attacker.AttackPacket = null; return;
                        }
                        attacked = null;
                        attackedsob = null;
                        if (attacker.Owner.Screen.TryGetValue(Target, out attacked) || attacker.Owner.Screen.TryGetSob(Target, out attackedsob) || Target == attacker.UID || spell.Sort != 1)
                        {
                            if (Target == attacker.UID)
                                attacked = attacker;
                            if (attacked != null)
                            {
                                if (attacked.Dead && spell.Sort != SpellSort.Revive && spell.ID != 10405 && spell.ID != 10425)
                                {
                                    attacker.AttackPacket = null; return;
                                }
                            }
                            if (Target >= 400000 && Target <= 600000 || Target >= 800000)
                            {
                                if (attacked == null && attackedsob == null) return;
                            }
                            else if (Target != 0 && attackedsob == null) return;
                            if (attacked != null)
                            {
                                if (attacked.PlayerFlag == PlayerFlag.Monster)
                                {
                                    if (spell.CanKill)
                                    {
                                        if (attacked.MonsterInfo.InSight == 0)
                                        {
                                            attacked.MonsterInfo.InSight = attacker.UID;
                                        }
                                    }
                                }
                            }
                            if (!attacker.Owner.Spells.ContainsKey(spellID))
                            {
                                if (spellID != 6012 && spellID != 10320) return;
                            }
                            var weapons = attacker.Owner.Weapons;
                            if (spell != null)
                            {
                                if (spell.OnlyWithThisWeaponSubtype.Count != 0)
                                {
                                    uint firstwepsubtype, secondwepsubtype;
                                    if (weapons.Item1 != null)
                                    {
                                        firstwepsubtype = weapons.Item1.ID / 1000;
                                        if (firstwepsubtype == 421) firstwepsubtype = 420;
                                        if (weapons.Item2 != null)
                                        {
                                            secondwepsubtype = weapons.Item2.ID / 1000;
                                            if (!spell.OnlyWithThisWeaponSubtype.Contains((ushort)firstwepsubtype))
                                            {
                                                if (!spell.OnlyWithThisWeaponSubtype.Contains((ushort)secondwepsubtype))
                                                {
                                                    attacker.AttackPacket = null;
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!spell.OnlyWithThisWeaponSubtype.Contains((ushort)firstwepsubtype))
                                            {
                                                attacker.AttackPacket = null;
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        attacker.AttackPacket = null;
                                        return;
                                    }
                                }
                            }
                            Interfaces.ISkill client_Spell;
                            if (!attacker.Owner.Spells.TryGetValue(spell.ID, out client_Spell))
                                if (!attacker.Owner.Spells.TryGetValue(spellID, out client_Spell))
                                    return;

                            if (attacker.Owner.Account.State == AccountTable.AccountState.Administrator)
                            {
                                attacker.Owner.Send("Spell ID: " + spellID);
                            }
                            switch (spellID)
                            {
                                #region AuroraLotus
                                case 12370:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.AuroraLotus))
                                                return;

                                            var map = attacker.Owner.Map;
                                            if (!map.Floor[X, Y, MapObjectType.Item, null]) return;
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.AddTarget(attacker, 0, null);
                                            attacker.Owner.SendScreen(suse, true);

                                            MsgMapItem floorItem = new MsgMapItem(true);
                                            floorItem.MapObjType = MapObjectType.Item;
                                            floorItem.ItemID = PacketMsgMapItem.AuroraLotus;
                                            floorItem.MapID = attacker.MapID;
                                            floorItem.Type = PacketMsgMapItem.Effect;
                                            floorItem.X = X;
                                            floorItem.Y = Y;
                                            floorItem.OnFloor = Time32.Now;
                                            floorItem.Owner = attacker.Owner;
                                            floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                            while (map.FloorItems.ContainsKey(floorItem.UID))
                                                floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                            floorItem.MaxLife = 25;
                                            floorItem.Life = 25;
                                            floorItem.mColor = 13;
                                            floorItem.OwnerUID = attacker.UID;
                                            floorItem.OwnerGuildUID = attacker.GuildID;
                                            floorItem.FlowerType = 0;
                                            floorItem.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(7));
                                            floorItem.Name = "AuroraLotus";

                                            map.AddFloorItem(floorItem);
                                            attacker.Owner.SendScreenSpawn(floorItem, true);

                                            attacker.AuroraLotusEnergy = 0;
                                            attacker.Lotus(attacker.AuroraLotusEnergy, (byte)PacketFlag.DataType.AuroraLotus);

                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #region FlameLotus
                                case 12380:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.FlameLotus))
                                                return;

                                            var map = attacker.Owner.Map;
                                            if (!map.Floor[X, Y, MapObjectType.Item, null]) return;
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.AddTarget(attacker, 0, null);
                                            attacker.Owner.SendScreen(suse, true);

                                            MsgMapItem floorItem = new MsgMapItem(true);
                                            floorItem.MapObjType = MapObjectType.Item;
                                            floorItem.ItemID = PacketMsgMapItem.FlameLotus;
                                            floorItem.MapID = attacker.MapID;
                                            floorItem.Type = PacketMsgMapItem.Effect;
                                            floorItem.X = X;
                                            floorItem.Y = Y;
                                            floorItem.OnFloor = Time32.Now;
                                            floorItem.Owner = attacker.Owner;
                                            floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                            while (map.FloorItems.ContainsKey(floorItem.UID))
                                                floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                            floorItem.MaxLife = 25;
                                            floorItem.Life = 25;
                                            floorItem.mColor = 13;
                                            floorItem.OwnerUID = attacker.UID;
                                            floorItem.OwnerGuildUID = attacker.GuildID;
                                            floorItem.FlowerType = 0;
                                            floorItem.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(7));
                                            floorItem.Name = "FlameLotus";

                                            map.AddFloorItem(floorItem);
                                            attacker.Owner.SendScreenSpawn(floorItem, true);

                                            attacker.FlameLotusEnergy = 0;
                                            attacker.Lotus(attacker.FlameLotusEnergy, (byte)PacketFlag.DataType.FlameLotus);

                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion


                                #endregion
                                #region Single magic damage spells
                                case 11030:
                                case 1000:
                                case 1001:
                                case 1002:
                                case 1150:
                                case 1160:
                                case 1180:
                                case 1320:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                           
                                            if (attacked != null)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage / 2, (int)spell.Power);
                                                        if (spell.ID == 11030)
                                                        {
                                                            damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 2.2);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 2.3);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 2.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 2.5);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 2.1);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 2.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 2.1);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 2.2);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                            {
                                                                damage = (uint)(damage * 2.3);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 2.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 2.3);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 2.5);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 2.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 2.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 2.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 2.6);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion

                                                            #endregion
                                                        }

                                                        if (spell.ID == 1001)
                                                        {
                                                            damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage / 2, (int)spell.Power);
                                                        }
                                                        if (spell.ID == 1002)
                                                        {
                                                            damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage * (int)0.8, (int)spell.Power);
                                                            if(attacker.EpicTaoist())
                                                            {
                                                                attacker.FlameLotusEnergy += 7;
                                                            }
                                                        }
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                        attacker.Owner.Player.IsEagleEyeShooted = true;
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                        var attackd = attacked as Player;
                                                        if (Kernel.BlackSpoted.ContainsKey(attackd.UID) && spell.ID == 11030)
                                                        {
                                                            attacker.Owner.Player.IsEagleEyeShooted = false;
                                                            if (attacker.Owner.Spells.ContainsKey(11130))
                                                            {
                                                                var s = attacker.Owner.Spells[11130];
                                                                var sspell = DB.SpellTable.SpellInformations[s.ID][s.Level];
                                                                if (spell != null)
                                                                {
                                                                    attacker.EagleEyeStamp = Time32.Now.AddSeconds(-100);
                                                                    attacker.Owner.Player.IsEagleEyeShooted = false;
                                                                    MsgMagicEffect ssuse = new MsgMagicEffect(true);
                                                                    ssuse.Attacker = attacker.UID;
                                                                    ssuse.SpellID = sspell.ID;
                                                                    ssuse.SpellLevel = sspell.Level;
                                                                    ssuse.AddTarget(attacker.Owner.Player, new MsgMagicEffect.DamageClass().Damage = 11030, attack);
                                                                    if (attacker.PlayerFlag == PlayerFlag.Player)
                                                                    {
                                                                        attacker.Owner.SendScreen(ssuse, true);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                            else
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                        attacker.Owner.SendScreen(suse, true);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region Single heal/meditation spells
                                case 1190:
                                case 1195:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            uint damage = spell.Power;
                                            if (spell.ID == 1190)
                                            {
                                                Experience = damage = Math.Min(damage, attacker.MaxHitpoints - attacker.Hitpoints);
                                                attacker.Hitpoints += damage;
                                            }
                                            else
                                            {
                                                Experience = damage = Math.Min(damage, (uint)(attacker.MaxMana - attacker.Mana));
                                                attacker.Mana += (ushort)damage;
                                            }
                                            suse.AddTarget(attacker, spell.Power, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Multi heal spells
                                case 1005:
                                case 1055:
                                case 1170:
                                case 1175:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attackedsob != null)
                                            {
                                                if (attacker.MapID == 1038) break;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Distance)
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    uint damage = spell.Power;
                                                    damage = Math.Min(damage, attackedsob.MaxHitpoints - attackedsob.Hitpoints);
                                                    attackedsob.Hitpoints += damage;
                                                    Experience += damage;
                                                    suse.AddTarget(attackedsob, damage, attack);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                            else
                                            {
                                                if (spell.Multi)
                                                {
                                                    if (attacker.Owner.Team != null)
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        foreach (Client.GameState teammate in attacker.Owner.Team.Teammates)
                                                        {
                                                            if (Kernel.GetDistance(attacker.X, attacker.Y, teammate.Player.X, teammate.Player.Y) <= spell.Distance)
                                                            {
                                                                uint damage = spell.Power;
                                                                damage = Math.Min(damage, teammate.Player.MaxHitpoints - teammate.Player.Hitpoints);
                                                                teammate.Player.Hitpoints += damage;
                                                                Experience += damage;
                                                                suse.AddTarget(teammate.Player, damage, attack);
                                                                if (spell.NextSpellID != 0)
                                                                {
                                                                    attack.Damage = spell.NextSpellID;
                                                                    attacker.AttackPacket = attack;
                                                                }
                                                                else
                                                                {
                                                                    attacker.AttackPacket = null;
                                                                }
                                                            }
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                    }
                                                    else
                                                    {
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                        {
                                                            PrepareSpell(spell, attacker.Owner);
                                                            uint damage = spell.Power;
                                                            damage = Math.Min(damage, attacked.MaxHitpoints - attacked.Hitpoints);
                                                            attacked.Hitpoints += damage;
                                                            Experience += damage;
                                                            suse.AddTarget(attacked, damage, attack);

                                                            if (spell.NextSpellID != 0)
                                                            {
                                                                attack.Damage = spell.NextSpellID;
                                                                attacker.AttackPacket = attack;
                                                            }
                                                            else
                                                            {
                                                                attacker.AttackPacket = null;
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                                                attacked.Owner.SendScreen(suse, true);
                                                            else attacked.MonsterInfo.SendScreen(suse);
                                                        }
                                                        else
                                                        {
                                                            attacker.AttackPacket = null;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        uint damage = spell.Power;
                                                        damage = Math.Min(damage, attacked.MaxHitpoints - attacked.Hitpoints);
                                                        attacked.Hitpoints += damage;
                                                        Experience += damage;
                                                        suse.AddTarget(attacked, damage, attack);
                                                        if (spell.NextSpellID != 0)
                                                        {
                                                            attack.Damage = spell.NextSpellID;
                                                            attacker.AttackPacket = attack;
                                                        }
                                                        else
                                                        {
                                                            attacker.AttackPacket = null;
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                    }
                                                    else
                                                    {
                                                        attacker.AttackPacket = null;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region Revive
                                case 1050:
                                case 1100:
                                    {
                                        if (attackedsob != null) return;
                                        if (Constants.NoRevAuroraLotus.Contains(attacker.MapID)) return;
                                        if (attacked.MapID == Pezzi.ServerEvents.Duke.ID || attacked.MapID == Pezzi.ServerEvents.King.ID || attacked.MapID == Pezzi.ServerEvents.Earl.ID || attacked.MapID == Pezzi.ServerEvents.Prince.ID) return;
                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                suse.AddTarget(attacked, 0, attack);
                                                attacked.Owner.Player.Action = Enums.ConquerAction.None;
                                                attacked.Owner.ReviveStamp = Time32.Now;
                                                attacked.Owner.Attackable = false;
                                                attacked.Owner.Player.TransformationID = 0;
                                                attacked.Owner.Player.AutoRev = 0;
                                                attacked.Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.Dead);
                                                attacked.Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.Ghost);
                                                attacked.Owner.Player.Hitpoints = attacked.Owner.Player.MaxHitpoints;
                                                attacked.Ressurect();
                                                attacked.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Linear spells
                               
                                case 1045:
                                case 1046:
                                case 11000:
                                case 11005://ViperFang
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            InLineAlgorithm ila = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                        if(spell.ID == 11005)
                                                        {
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion

                                                            #endregion
                                                        }
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (ila.InLine(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack) * 2;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region XPSpells inoffensive
                                case 1015:
                                case 1020:
                                case 1025:
                                case 1110:
                                case 6011:
                                case 10390:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.AddTarget(attacked, 0, attack);
                                            if (spell.ID == 6011)
                                            {
                                                attacked.FatalStrikeStamp = Time32.Now;
                                                attacked.FatalStrikeTime = 60;
                                                attacked.AddFlag((ulong)PacketFlag.Flags.FatalStrike);
                                                attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                            }
                                            else if (spell.ID == 1110 || spell.ID == 1025 || spell.ID == 10390)
                                            {
                                                if (!attacked.OnKOSpell())
                                                    attacked.KOCount = 0;
                                                attacked.KOSpell = spell.ID;
                                                if (spell.ID == 1110)
                                                {
                                                    attacked.CycloneStamp = Time32.Now;
                                                    attacked.CycloneTime = 20;
                                                    attacked.AddFlag((ulong)PacketFlag.Flags.Cyclone);
                                                }
                                                else if (spell.ID == 10390)
                                                {
                                                    attacked.OblivionStamp = Time32.Now;
                                                    attacked.OblivionTime = 20;
                                                    attacked.AddFlag2((ulong)PacketFlag.Flags.Oblivion);
                                                }
                                                else
                                                {
                                                    attacked.SupermanStamp = Time32.Now;
                                                    attacked.SupermanTime = 20;
                                                    attacked.AddFlag((ulong)PacketFlag.Flags.Superman);
                                                }
                                            }
                                            else if (spell.ID == 1020)
                                            {
                                                attacked.ShieldStamp = Time32.Now;
                                                attacked.MagicShieldStamp = Time32.Now;
                                                attacked.MagicShieldTime = 0;
                                                attacked.ShieldTime = 0;

                                                attacked.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                                                attacked.ShieldStamp = Time32.Now;
                                                attacked.ShieldIncrease = spell.PowerPercent;
                                                attacked.ShieldTime = 60;
                                            }
                                            else
                                            {
                                                attacked.AccuracyStamp = Time32.Now;
                                                attacked.StarOfAccuracyStamp = Time32.Now;
                                                attacked.StarOfAccuracyTime = 0;
                                                attacked.AccuracyTime = 0;

                                                attacked.AddFlag((ulong)PacketFlag.Flags.StarOfAccuracy);
                                                attacked.AccuracyStamp = Time32.Now;
                                                attacked.AccuracyTime = (byte)spell.Duration;
                                            }
                                            attacked.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Circle spells
                                case 1010:
                                case 1115:
                                case 1120:
                                case 1125:
                                case 3090:
                                case 5001:
                                case 8030:
                                case 10315:
                                    {
                                        if (spell.ID == 10315)
                                        {
                                            if (attacker.Owner.Weapons.Item1 == null) return;
                                            if (attacker.Owner.Weapons.Item1.IsTwoHander()) return;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            UInt16 ox, oy;
                                            ox = attacker.X;
                                            oy = attacker.Y;
                                            if (spellID == 10315)
                                            {
                                                MsgInteract npacket = new MsgInteract(true);
                                                npacket.Attacker = attacker.UID;
                                                npacket.InteractType = 53;
                                                npacket.X = X;
                                                npacket.Y = Y;
                                                Writer.WriteUInt16(spell.ID, 28, npacket.ToArray());
                                                Writer.WriteByte(spell.Level, 30, npacket.ToArray());
                                                attacker.Owner.SendScreen(npacket, true);
                                                attacker.X = X;
                                                attacker.Y = Y;
                                                attacker.SendSpawn(attacker.Owner);
                                                attacker.Owner.Screen.Reload(npacket);
                                            }
                                            List<IMapObject> objects = new List<IMapObject>();
                                            if (attacker.Owner.Screen.Objects.Count() > 0)
                                                objects = GetObjects(ox, oy, attacker.Owner);
                                            if (objects != null)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Range)
                                                {
                                                    if (spellID == 10315)
                                                    {
                                                        foreach (IMapObject objs in objects.ToArray())
                                                        {
                                                            if (objs == null) continue;
                                                            if (objs.MapObjType == MapObjectType.Monster || objs.MapObjType == MapObjectType.Player)
                                                            {
                                                                attacked = objs as Player;
                                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                                {
                                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                                    {
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack) * 100 / 135;
                                                                        //damage = damage - (uint)(damage * .30);
                                                                        suse.Effect = attack.Effect;
                                                                        if (spell.Power > 0)
                                                                        {
                                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                                            damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                                            damage = damage / 2;
                                                                            suse.Effect = attack.Effect;
                                                                        }
                                                                        if (spell.ID == 10315)
                                                                        {
                                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                                            damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) * 100 / 110;
                                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                        }
                                                                        if (spell.ID == 8030)
                                                                        {
                                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                                            damage = Game.Attacking.Calculate.Ranged(attacker, attacked, spell, ref attack);
                                                                        }
                                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                        suse.AddTarget(attacked, damage, attack);
                                                                    }
                                                                }
                                                            }
                                                            else if (objs.MapObjType == MapObjectType.SobNpc)
                                                            {
                                                                attackedsob = objs as MsgNpcInfoEX;
                                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                                {
                                                                    if (CanAttack(attacker, attackedsob, spell))
                                                                    {
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                        if (spell.Power > 0)
                                                                        {
                                                                            damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                                        }
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        if (spell.ID == 8030)
                                                                            damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);
                                                                        suse.Effect = attack.Effect;
                                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                        suse.AddTarget(attackedsob, damage, attack);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                        {
                                                            if (_obj == null) continue;
                                                            if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                            {
                                                                attacked = _obj as Player;
                                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                                {
                                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                                    {
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                        // damage = damage - (uint)(damage * .30);
                                                                        suse.Effect = attack.Effect;
                                                                        if (spell.Power > 0)
                                                                        {
                                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                                            damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                                            suse.Effect = attack.Effect;
                                                                        }
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        if (spell.ID == 8030)
                                                                            damage = Game.Attacking.Calculate.Ranged(attacker, attacked, spell, ref attack);
                                                                        if (spell.ID == 1115)
                                                                        {
                                                                            damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) * 100 / 135;
                                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                        }
                                                                        if (spell.ID == 1120)
                                                                        {
                                                                            if (!attacker.Owner.Equipment.Free(4) && attacker.Owner.Spells.ContainsKey(12400))
                                                                            {

                                                                                var item = attacker.Owner.Equipment.TryGetItem(4);
                                                                                if (ItemHandler.IsTaoistEpicWeapon(item.ID))
                                                                                {
                                                                                    if (attacked.PlayerFlag == PlayerFlag.Player)
                                                                                        attacked.Owner.BreakTouch(attacker.Owner);

                                                                                    attacker.FlameLotusEnergy = (uint)Math.Min(330, attacker.FlameLotusEnergy + 1);
                                                                                    attacker.Lotus(attacker.FlameLotusEnergy, (byte)PacketFlag.DataType.FlameLotus);
                                                                                    damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                                                }
                                                                            }

                                                                        }
                                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                        suse.AddTarget(attacked, damage, attack);
                                                                    }
                                                                }
                                                            }
                                                            else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                            {
                                                                attackedsob = _obj as MsgNpcInfoEX;
                                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                                {
                                                                    if (CanAttack(attacker, attackedsob, spell))
                                                                    {
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                        if (spell.Power > 0)
                                                                            damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                                        if (spell.ID == 8030)
                                                                            damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);
                                                                        suse.Effect = attack.Effect;
                                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                        suse.AddTarget(attackedsob, damage, attack);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                            Calculations.IsBreaking(attacker.Owner, ox, oy);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Buffers
                                case 1075:
                                case 1085:
                                case 1090:
                                case 1095:
                                case 3080:
                                case 30000:
                                    {
                                        if (attackedsob != null)
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                suse.AddTarget(attackedsob, 0, null);
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        else
                                        {
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                            {
                                                if (CanUseSpell(spell, attacker.Owner))
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    suse.AddTarget(attacked, 0, null);
                                                    if (spell.ID == 1075 || spell.ID == 1085)
                                                    {
                                                        if (spell.ID == 1075)
                                                        {
                                                            attacked.AddFlag((ulong)PacketFlag.Flags.Invisibility);
                                                            attacked.InvisibilityStamp = Time32.Now;
                                                            attacked.InvisibilityTime = (byte)spell.Duration;
                                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                                                attacked.Owner.Send(DefineConstantsEn_Res.Invisibility(spell.Duration));
                                                        }
                                                        else
                                                        {
                                                            attacked.AccuracyStamp = Time32.Now;
                                                            attacked.StarOfAccuracyStamp = Time32.Now;
                                                            attacked.StarOfAccuracyTime = 0;
                                                            attacked.AccuracyTime = 0;
                                                            attacked.AddFlag((ulong)PacketFlag.Flags.StarOfAccuracy);
                                                            attacked.StarOfAccuracyStamp = Time32.Now;
                                                            attacked.StarOfAccuracyTime = (byte)spell.Duration;
                                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                                                attacked.Owner.Send(DefineConstantsEn_Res.Accuracy(spell.Duration));
                                                        }
                                                    }
                                                    else if (spell.ID == 1090)
                                                    {
                                                        attacked.ShieldTime = 0;
                                                        attacked.ShieldStamp = Time32.Now;
                                                        attacked.MagicShieldStamp = Time32.Now;
                                                        attacked.MagicShieldTime = 0;
                                                        attacked.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                                                        attacked.MagicShieldStamp = Time32.Now;
                                                        attacked.MagicShieldIncrease = spell.PowerPercent;
                                                        attacked.MagicShieldTime = (byte)spell.Duration;
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.Send(DefineConstantsEn_Res.Shield(spell.PowerPercent, spell.Duration));
                                                    }
                                                    else if (spell.ID == 1095)
                                                    {
                                                        attacked.AddFlag((ulong)PacketFlag.Flags.Stigma);
                                                        attacked.StigmaStamp = Time32.Now;
                                                        attacked.StigmaIncrease = spell.PowerPercent;
                                                        attacked.StigmaTime = (byte)spell.Duration;
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.Send(DefineConstantsEn_Res.Stigma(spell.PowerPercent, spell.Duration));
                                                    }
                                                    else if (spell.ID == 30000)
                                                    {
                                                        //Samak 
                                                        if (attacked.ContainsFlag2((ulong)PacketFlag.Flags.AzureShield))
                                                        {
                                                            return;
                                                        }

                                                        if (spell.Level == 0)
                                                            attacked.AzureShieldDefence = 3000;
                                                        else
                                                            attacked.AzureShieldDefence = (ushort)(3000 * spell.Level);
                                                        attacked.AzureShieldLevel = spell.Level;
                                                        attacked.MagicShieldStamp = Time32.Now;

                                                        attacked.AzureShieldStamp = DateTime.Now;
                                                        attacked.AddFlag2((ulong)PacketFlag.Flags.AzureShield);
                                                        attacked.MagicShieldTime = spell.Percent;
                                                        attacked.AzureShieldPacket();
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.Send(DefineConstantsEn_Res.Shield(12000, attacked.MagicShieldTime));
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player)
                                                        attacked.Owner.SendScreen(suse, true);
                                                    else attacked.MonsterInfo.SendScreen(suse);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region ChainBolt
                                case 10309:
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = attacker.X;
                                        suse.Y = attacker.Y;
                                        if (attacked != null)
                                        {
                                            if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.ChainBoltActive))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                suse.X = X;
                                                suse.Y = Y;
                                                int maxR = spell.Distance;
                                                if (attacked != null)
                                                {
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= maxR)
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            var damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                var Array = attacker.Owner.Screen.Objects;
                                                var closestTarget = findClosestTarget(attacked, attacked.X, attacked.Y, Array);
                                                ushort x = closestTarget.X, y = closestTarget.Y;
                                                int targets = Math.Max((int)spell.Level, 1);
                                                foreach (Interfaces.IMapObject _obj in Array)
                                                {
                                                    if (targets == 0) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(x, y, attacked.X, attacked.Y) <= maxR)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                maxR = 6;
                                                                var damage2 = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                                damage2 = (uint)MathHelper.AdjustDataEx((int)damage2, (int)spell.Power);
                                                                ReceiveAttack(attacker, attacked, attack, ref damage2, spell);
                                                                suse.AddTarget(attacked, damage2, attack);
                                                                x = attacked.X;
                                                                y = attacked.Y;
                                                                targets--;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (suse.Targets.Count == 0) return;
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                            else if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                attacker.ChainboltStamp = Time32.Now;
                                                attacker.ChainboltTime = spell.Duration;
                                                attacker.AddFlag2((ulong)PacketFlag.Flags.ChainBoltActive);
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Heaven Blade
                                case 10310:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacked != null)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attacked, spell, false))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        var damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                        if (MyMath.Success(spell.Percent))
                                                        {
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                        else
                                                        {
                                                            damage = 0;
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                        attacker.Owner.SendScreen(suse, true);
                                                    }
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                            else
                                            {
                                                if (attackedsob != null)
                                                {
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Distance)
                                                    {
                                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                                        suse.Attacker = attacker.UID;
                                                        suse.SpellID = spell.ID;
                                                        suse.SpellLevel = spell.Level;
                                                        suse.X = X;
                                                        suse.Y = Y;
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            PrepareSpell(spell, attacker.Owner);
                                                            var damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                            if (MyMath.Success(spell.Percent))
                                                            {
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                            else
                                                            {
                                                                damage = 0;
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                            attacker.Owner.SendScreen(suse, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region FireOfHell
                                case 7014:
                                case 7017:
                                case 7015:
                                case 7011:
                                case 7012:
                                case 1165:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, spell.Distance);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (sector.Inside(attacked.X, attacked.Y))
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.MagicPerfectos(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage / 2, (int)spell.Power);
                                                            if (attacker.EpicTaoist())
                                                            {
                                                                attacker.FlameLotusEnergy += 7;
                                                            }
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Trasnformations
                                case 1270:
                                case 1280:
                                case 1350:
                                case 1360:
                                case 3321:
                                case 12480:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacker.MapID == 1036) return;
                                            if (attacker.MapID == 1950) return;
                                            bool wasTransformated = attacker.Transformed;
                                            PrepareSpell(spell, attacker.Owner);
                                            #region Atributes
                                            switch (spell.ID)
                                            {
                                                case 12480:
                                                    {
                                                        attacker.TransformationMaxAttack = 30000;
                                                        attacker.TransformationMinAttack = 20000;
                                                        attacker.TransformationDefence = 19000;
                                                        attacker.TransformationMagicDefence = 99;
                                                        attacker.TransformationDodge = 55;
                                                        attacker.TransformationTime = 35;
                                                        attacker.TransformationID = 275;//275  
                                                        attacker.Hitpoints = attacker.MaxHitpoints;
                                                        attacker.Mana = attacker.MaxMana;
                                                        break;
                                                    }
                                                case 3321://GM skill
                                                    {
                                                        attacker.TransformationMaxAttack = 2000000;
                                                        attacker.TransformationMinAttack = 2000000;
                                                        attacker.TransformationDefence = 65355;
                                                        attacker.TransformationMagicDefence = 65355;
                                                        attacker.TransformationDodge = 35;
                                                        attacker.TransformationTime = 65355;
                                                        attacker.TransformationID = 223;
                                                        attacker.Hitpoints = attacker.MaxHitpoints;
                                                        attacker.Mana = attacker.MaxMana;
                                                        break;
                                                    }
                                                case 1350:
                                                    {
                                                        switch (spell.Level)
                                                        {
                                                            case 0:
                                                                {
                                                                    attacker.TransformationMaxAttack = 182;
                                                                    attacker.TransformationMinAttack = 122;
                                                                    attacker.TransformationDefence = 1300;
                                                                    attacker.TransformationMagicDefence = 94;
                                                                    attacker.TransformationDodge = 35;
                                                                    attacker.TransformationTime = 39;
                                                                    attacker.TransformationID = 207;
                                                                    break;
                                                                }
                                                            case 1:
                                                                {
                                                                    attacker.TransformationMaxAttack = 200;
                                                                    attacker.TransformationMinAttack = 134;
                                                                    attacker.TransformationDefence = 1400;
                                                                    attacker.TransformationMagicDefence = 96;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 49;
                                                                    attacker.TransformationID = 207;
                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    attacker.TransformationMaxAttack = 240;
                                                                    attacker.TransformationMinAttack = 160;
                                                                    attacker.TransformationDefence = 1500;
                                                                    attacker.TransformationMagicDefence = 97;
                                                                    attacker.TransformationDodge = 45;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 207;
                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    attacker.TransformationMaxAttack = 258;
                                                                    attacker.TransformationMinAttack = 172;
                                                                    attacker.TransformationDefence = 1600;
                                                                    attacker.TransformationMagicDefence = 98;
                                                                    attacker.TransformationDodge = 50;
                                                                    attacker.TransformationTime = 69;
                                                                    attacker.TransformationID = 267;
                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    attacker.TransformationMaxAttack = 300;
                                                                    attacker.TransformationMinAttack = 200;
                                                                    attacker.TransformationDefence = 1900;
                                                                    attacker.TransformationMagicDefence = 99;
                                                                    attacker.TransformationDodge = 55;
                                                                    attacker.TransformationTime = 79;
                                                                    attacker.TransformationID = 267;
                                                                    break;
                                                                }
                                                        }
                                                        break;
                                                    }
                                                case 1270:
                                                    {
                                                        switch (spell.Level)
                                                        {
                                                            case 0:
                                                                {
                                                                    attacker.TransformationMaxAttack = 282;
                                                                    attacker.TransformationMinAttack = 179;
                                                                    attacker.TransformationDefence = 73;
                                                                    attacker.TransformationMagicDefence = 34;
                                                                    attacker.TransformationDodge = 9;
                                                                    attacker.TransformationTime = 34;
                                                                    attacker.TransformationID = 214;
                                                                    break;
                                                                }
                                                            case 1:
                                                                {
                                                                    attacker.TransformationMaxAttack = 395;
                                                                    attacker.TransformationMinAttack = 245;
                                                                    attacker.TransformationDefence = 126;
                                                                    attacker.TransformationMagicDefence = 45;
                                                                    attacker.TransformationDodge = 12;
                                                                    attacker.TransformationTime = 39;
                                                                    attacker.TransformationID = 214;
                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    attacker.TransformationMaxAttack = 616;
                                                                    attacker.TransformationMinAttack = 367;
                                                                    attacker.TransformationDefence = 180;
                                                                    attacker.TransformationMagicDefence = 53;
                                                                    attacker.TransformationDodge = 15;
                                                                    attacker.TransformationTime = 44;
                                                                    attacker.TransformationID = 214;
                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    attacker.TransformationMaxAttack = 724;
                                                                    attacker.TransformationMinAttack = 429;
                                                                    attacker.TransformationDefence = 247;
                                                                    attacker.TransformationMagicDefence = 53;
                                                                    attacker.TransformationDodge = 15;
                                                                    attacker.TransformationTime = 49;
                                                                    attacker.TransformationID = 214;
                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1231;
                                                                    attacker.TransformationMinAttack = 704;
                                                                    attacker.TransformationDefence = 499;
                                                                    attacker.TransformationMagicDefence = 50;
                                                                    attacker.TransformationDodge = 20;
                                                                    attacker.TransformationTime = 54;
                                                                    attacker.TransformationID = 274;
                                                                    break;
                                                                }
                                                            case 5:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1573;
                                                                    attacker.TransformationMinAttack = 941;
                                                                    attacker.TransformationDefence = 601;
                                                                    attacker.TransformationMagicDefence = 53;
                                                                    attacker.TransformationDodge = 25;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 274;
                                                                    break;
                                                                }
                                                            case 6:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1991;
                                                                    attacker.TransformationMinAttack = 1107;
                                                                    attacker.TransformationDefence = 1029;
                                                                    attacker.TransformationMagicDefence = 55;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 64;
                                                                    attacker.TransformationID = 274;
                                                                    break;
                                                                }
                                                            case 7:
                                                                {
                                                                    attacker.TransformationMaxAttack = 2226;
                                                                    attacker.TransformationMinAttack = 1235;
                                                                    attacker.TransformationDefence = 1029;
                                                                    attacker.TransformationMagicDefence = 55;
                                                                    attacker.TransformationDodge = 35;
                                                                    attacker.TransformationTime = 69;
                                                                    attacker.TransformationID = 274;
                                                                    break;
                                                                }
                                                        }
                                                        break;
                                                    }
                                                case 1360:
                                                    {
                                                        switch (spell.Level)
                                                        {
                                                            case 0:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1215;
                                                                    attacker.TransformationMinAttack = 610;
                                                                    attacker.TransformationDefence = 100;
                                                                    attacker.TransformationMagicDefence = 96;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 217;
                                                                    break;
                                                                }
                                                            case 1:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1310;
                                                                    attacker.TransformationMinAttack = 650;
                                                                    attacker.TransformationDefence = 400;
                                                                    attacker.TransformationMagicDefence = 97;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 79;
                                                                    attacker.TransformationID = 217;
                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1420;
                                                                    attacker.TransformationMinAttack = 710;
                                                                    attacker.TransformationDefence = 650;
                                                                    attacker.TransformationMagicDefence = 98;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 89;
                                                                    attacker.TransformationID = 217;
                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1555;
                                                                    attacker.TransformationMinAttack = 780;
                                                                    attacker.TransformationDefence = 720;
                                                                    attacker.TransformationMagicDefence = 98;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 99;
                                                                    attacker.TransformationID = 277;
                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1660;
                                                                    attacker.TransformationMinAttack = 840;
                                                                    attacker.TransformationDefence = 1200;
                                                                    attacker.TransformationMagicDefence = 99;
                                                                    attacker.TransformationDodge = 30;
                                                                    attacker.TransformationTime = 109;
                                                                    attacker.TransformationID = 277;
                                                                    break;
                                                                }
                                                        }
                                                        break;
                                                    }
                                                case 1280:
                                                    {
                                                        switch (spell.Level)
                                                        {
                                                            case 0:
                                                                {
                                                                    attacker.TransformationMaxAttack = 930;
                                                                    attacker.TransformationMinAttack = 656;
                                                                    attacker.TransformationDefence = 290;
                                                                    attacker.TransformationMagicDefence = 45;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 29;
                                                                    attacker.TransformationID = 213;
                                                                    break;
                                                                }
                                                            case 1:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1062;
                                                                    attacker.TransformationMinAttack = 750;
                                                                    attacker.TransformationDefence = 320;
                                                                    attacker.TransformationMagicDefence = 46;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 34;
                                                                    attacker.TransformationID = 213;
                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1292;
                                                                    attacker.TransformationMinAttack = 910;
                                                                    attacker.TransformationDefence = 510;
                                                                    attacker.TransformationMagicDefence = 50;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 39;
                                                                    attacker.TransformationID = 213;
                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1428;
                                                                    attacker.TransformationMinAttack = 1000;
                                                                    attacker.TransformationDefence = 600;
                                                                    attacker.TransformationMagicDefence = 53;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 44;
                                                                    attacker.TransformationID = 213;
                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1570;
                                                                    attacker.TransformationMinAttack = 1100;
                                                                    attacker.TransformationDefence = 700;
                                                                    attacker.TransformationMagicDefence = 55;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 49;
                                                                    attacker.TransformationID = 213;
                                                                    break;
                                                                }
                                                            case 5:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1700;
                                                                    attacker.TransformationMinAttack = 1200;
                                                                    attacker.TransformationDefence = 880;
                                                                    attacker.TransformationMagicDefence = 57;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 54;
                                                                    attacker.TransformationID = 273;
                                                                    break;
                                                                }
                                                            case 6:
                                                                {
                                                                    attacker.TransformationMaxAttack = 1900;
                                                                    attacker.TransformationMinAttack = 1300;
                                                                    attacker.TransformationDefence = 1540;
                                                                    attacker.TransformationMagicDefence = 59;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 273;
                                                                    break;
                                                                }
                                                            case 7:
                                                                {
                                                                    attacker.TransformationMaxAttack = 2100;
                                                                    attacker.TransformationMinAttack = 1500;
                                                                    attacker.TransformationDefence = 1880;
                                                                    attacker.TransformationMagicDefence = 61;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 273;
                                                                    break;
                                                                }
                                                            case 8:
                                                                {
                                                                    attacker.TransformationMaxAttack = 2300;
                                                                    attacker.TransformationMinAttack = 1600;
                                                                    attacker.TransformationDefence = 1970;
                                                                    attacker.TransformationMagicDefence = 63;
                                                                    attacker.TransformationDodge = 40;
                                                                    attacker.TransformationTime = 59;
                                                                    attacker.TransformationID = 273;
                                                                    break;
                                                                }
                                                        }
                                                        break;
                                                    }

                                            }
                                            #endregion
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.Attacker = attacker.UID;
                                            MsgMagicEffect.SpellID = spell.ID;
                                            MsgMagicEffect.SpellLevel = spell.Level;
                                            MsgMagicEffect.X = X;
                                            MsgMagicEffect.Y = Y;
                                            MsgMagicEffect.AddTarget(attacker, (uint)0, attack);
                                            attacker.Owner.SendScreen(MsgMagicEffect, true);
                                            attacker.TransformationStamp = Time32.Now;
                                            attacker.TransformationMaxHP = 3000;
                                            if (spell.ID == 1270)
                                                attacker.TransformationMaxHP = 50000;
                                            attacker.TransformationAttackRange = 3;
                                            if (spell.ID == 1360)
                                                attacker.TransformationAttackRange = 10;
                                            if (!wasTransformated)
                                            {
                                                double maxHP = attacker.MaxHitpoints;
                                                double HP = attacker.Hitpoints;
                                                double point = HP / maxHP;
                                                attacker.Hitpoints = (uint)(attacker.TransformationMaxHP * point);
                                            }
                                            attacker.Update((byte)PacketFlag.DataType.MaxHitpoints, attacker.TransformationMaxHP, false);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Bless
                                case 9876:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.Attacker = attacker.UID;
                                            MsgMagicEffect.SpellID = spell.ID;
                                            MsgMagicEffect.SpellLevel = spell.Level;
                                            MsgMagicEffect.X = X;
                                            MsgMagicEffect.Y = Y;
                                            attacker.AddFlag((ulong)PacketFlag.Flags.CastPray);
                                            MsgMagicEffect.AddTarget(attacker, 0, attack);
                                            attacker.Owner.SendScreen(MsgMagicEffect, true);
                                        }
                                        break;
                                    }
                                #endregion

                                #region Companions
                                case 4000:
                                case 4010:
                                case 4020:
                                case 4050:
                                case 4060:
                                case 4070:
                                case 12610:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PetsControler.MakeApet(attacker, spell, X, Y);
                                        }
                                        break;
                                    }
                                case 12020:
                                case 12030:
                                case 12040:
                                case 12050:
                                case 12470:

                                    //275
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PetsControler.MakeApet(attacker, spell, X, Y);
                                        }
                                        break;
                                    }
                                #endregion
                                #region WeaponSpells
                                #region Circle
                                case 5010:
                                case 7020:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            if (suse.SpellID != 10415)
                                            {
                                                suse.X = X;
                                                suse.Y = Y;
                                            }
                                            else
                                            {
                                                suse.X = 6;
                                            }
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= attacker.AttackRange + 1)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                
                                #region Sector
                                case 1250:
                                case 5050:
                                case 5020:
                                case 1300:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, spell.Range);
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance + 1)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (sector.Inside(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Archer Spells
                                #region Fly
                                case 8002:
                                case 8003:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacker.MapID == 1950) return;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacked.FlyStamp = Time32.Now;
                                            attacked.FlyTime = (byte)spell.Duration;
                                            suse.AddTarget(attacker, 0, null);
                                            attacker.AddFlag((ulong)PacketFlag.Flags.Fly);
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Scatter
                                case 8001:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, spell.Distance);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Monster)
                                                {
                                                    attacked = _obj as Player;
                                                    if (sector.Inside(attacked.X, attacked.Y))
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, spell, ref attack);
                                                            damage = (uint)(damage * 8.0);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                if (_obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (sector.Inside(attacked.X, attacked.Y))
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, spell, ref attack);
                                                            damage = (uint)(damage * 0.9);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            if (damage == 0) damage = 1;
                                                            damage = Game.Attacking.Calculate.Percent((int)damage, spell.PowerPercent);
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region StarArrow
                                case 10313:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;

                                            if (attacked != null)
                                            {
                                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                {
                                                    ushort _X = attacked.X, _Y = attacked.Y;
                                                    byte dist = 5;
                                                    var angle = Kernel.GetAngle(attacker.X, attacker.Y, attacked.X, attacked.Y);
                                                    while (dist != 0)
                                                    {
                                                        if (attacked.fMove(angle, ref _X, ref _Y))
                                                        {
                                                            X = _X;
                                                            Y = _Y;
                                                        }
                                                        else break;
                                                        dist--;
                                                    }
                                                    suse.X = attacked.X = X;
                                                    suse.Y = attacked.Y = Y;
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                    damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                    suse.AddTarget(attacked, damage, attack);
                                                }
                                            }
                                            else if (attackedsob != null)
                                            {
                                                if (CanAttack(attacker, attackedsob, spell))
                                                {
                                                    suse.X = attackedsob.X;
                                                    suse.Y = attackedsob.Y;
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);
                                                    suse.Effect = attack.Effect;
                                                    if (damage == 0)
                                                        damage = 1;
                                                    damage = Game.Attacking.Calculate.Percent((int)damage, spell.PowerPercent);

                                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                    suse.AddTarget(attackedsob, damage, attack);
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region RapidFire
                                case 8000:
                                    {
                                        if (attackedsob != null)
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                if (CanAttack(attacker, attackedsob, spell))
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = attackedsob.X;
                                                    suse.Y = attackedsob.Y;
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Calculate.Ranged(attacker, attackedsob, ref attack);
                                                    suse.Effect = attack.Effect;
                                                    suse.AddTarget(attackedsob, damage, attack);
                                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!attacked.Dead)
                                            {
                                                if (CanUseSpell(spell, attacker.Owner))
                                                {
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                                        suse.Attacker = attacker.UID;
                                                        suse.SpellID = spell.ID;
                                                        suse.SpellLevel = spell.Level;
                                                        suse.X = attacked.X;
                                                        suse.Y = attacked.Y;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Calculate.Ranged(attacker, attacked, spell, ref attack);
                                                        suse.AddTarget(attacked, damage, attack);
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        attacker.Owner.SendScreen(suse, true);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Ninja Spells
                                #region TwilightDance
                                case 12070:
                                    {
                                        //if (Time32.Now >= attacker.SpellStamp.AddMilliseconds(4000))

                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;

                                            TwilightAction(attacker, suse, spell, X, Y);

                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AttackPacket = null;

                                        }
                                        break;
                                    }
                                #endregion
                                #region SuperTwofoldBlade
                                case 12080:
                                    {
                                        // if (Time32.Now >= attacker.SpellStamp.AddMilliseconds(500))
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                ushort Xx, Yx;
                                                if (attacked != null)
                                                {
                                                    Xx = attacked.X;
                                                    Yx = attacked.Y;
                                                }
                                                else
                                                {
                                                    Xx = attackedsob.X;
                                                    Yx = attackedsob.Y;
                                                }
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, Xx, Yx) <= spell.Distance)
                                                {
                                                    if (attackedsob == null)
                                                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                    // if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                    if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    bool send = false;
                                                    if (attackedsob == null)
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.7);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 2);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                            {
                                                                damage = (uint)(damage * 1.3);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion

                                                            #endregion
                                                            //if (attacked.PlayerFlag == PlayerFlag.Monster)
                                                            //    damage = damage / 2;
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked.UID, damage, attack);
                                                            send = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Calculate.Melee(attacker, attackedsob, ref attack);
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.Effect = attack.Effect;
                                                            suse.AddTarget(attackedsob.UID, damage, attack);
                                                            send = true;
                                                        }
                                                    }
                                                    if (send)
                                                        attacker.Owner.SendScreen(suse, true);
                                                    attacker.SpellStamp = Time32.Now;
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region ShadowClones
                                case 12090:
                                    {
                                        if (attacker.MyClones.Count != 0)
                                        {
                                            foreach (var clone in attacker.MyClones)
                                                clone.RemoveThat();
                                            attacker.MyClones.Clear();
                                            break;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            switch (attacker.Owner.Spells[SpellID].Level)
                                            {
                                                case 0:
                                                case 1:
                                                case 2:
                                                    {
                                                        attacker.MyClones.Add(new Clone(attacker, "ShadowClone", 10003));
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        attacker.MyClones.Add(new Clone(attacker, "ShadowClone", 3));
                                                        attacker.MyClones.Add(new Clone(attacker, "ShadowClone", 10003));
                                                        break;
                                                    }
                                            }
                                            foreach (var clone in attacker.MyClones)
                                            {
                                                MsgAction Data = new MsgAction(true);
                                                Data.ID = PacketMsgAction.Mode.Revive;
                                                Data.UID = clone.UID;
                                                Data.X = attacker.X;
                                                Data.Y = attacker.Y;
                                                attacker.Owner.SendScreen(Data, true);
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }

                                #endregion
                                #region Vortex
                                case 6010:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacker.AddFlag((ulong)PacketFlag.Flags.ShurikenVortex);
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                            attacker.ShurikenVortexStamp = Time32.Now;
                                            attacker.ShurikenVortexTime = 20;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.VortexPacket = new MsgInteract(true);
                                            attacker.VortexPacket.Decoded = true;
                                            attacker.VortexPacket.Damage = 6012;
                                            attacker.VortexPacket.InteractType = MsgInteract.Magic;
                                            attacker.VortexPacket.Attacker = attacker.UID;
                                        }
                                        break;
                                    }
                                #endregion
                                #region VortexRespone
                                case 6012:
                                    {
                                        if (!attacker.ContainsFlag((ulong)PacketFlag.Flags.ShurikenVortex))
                                        {
                                            attacker.AttackPacket = null; break;
                                        }
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = attacker.X;
                                        suse.Y = attacker.Y;
                                        foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                        {
                                            if (_obj == null) continue;
                                            if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                            {
                                                attacked = _obj as Player;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                {
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage * 8, (int)spell.Power);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                            }
                                            else if (_obj.MapObjType == MapObjectType.SobNpc)
                                            {
                                                attackedsob = _obj as MsgNpcInfoEX;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                {
                                                    if (CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region ToxicFog
                                case 6001:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Player || _obj.MapObjType == MapObjectType.Monster)
                                                    {
                                                        attacked = _obj as Player;
                                                      
                                                        if (attacked.MapObjType == MapObjectType.Monster)
                                                            if (attacked.MonsterInfo.Boss) continue;
                                                        if (Kernel.GetDistance(X, Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                int potDifference = attacker.BattlePower - attacked.BattlePower;
                                                                int rate = spell.Percent + potDifference - 20;
                                                                if (MyMath.Success(rate))
                                                                {
                                                                    attacked.ToxicFogStamp = Time32.Now;
                                                                    attacked.ToxicFogLeft = 20;
                                                                    attacked.ToxicFogPercent = 0.5F;
                                                                    attacked.AddFlag((ulong)PacketFlag.Flags.Poisoned);
                                                                    suse.AddTarget(attacked, 1, null);
                                                                }
                                                                else
                                                                {
                                                                    suse.AddTarget(attacked, 0, null);
                                                                    suse.Targets[attacked.UID].Hit = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region TwofoldBlades
                                case 6000:
                                    {
                                        if (Time32.Now >= attacker.SpellStamp.AddMilliseconds(500))
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                ushort Xx, Yx;
                                                if (attacked != null)
                                                {
                                                    Xx = attacked.X;
                                                    Yx = attacked.Y;
                                                }
                                                else
                                                {
                                                    Xx = attackedsob.X;
                                                    Yx = attackedsob.Y;
                                                }
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, Xx, Yx) <= spell.Range)
                                                {
                                                    if (attackedsob == null)
                                                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                    if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    bool send = false;
                                                    if (attackedsob == null)
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                            send = true;
                                                            //if (attacker.Owner.Spells.ContainsKey(11230) && !attacked.Dead)
                                                            //{
                                                            //    var s = attacker.Owner.Spells[11230];
                                                            //    var spellz = DB.SpellTable.SpellInformations[s.ID][s.Level];
                                                            //    if (spellz != null)
                                                            //    {
                                                            //        if (GameServer.MyMath.Success(spellz.Percent))
                                                            //        {
                                                            //            MsgMagicEffect ssuse = new MsgMagicEffect(true);
                                                            //            ssuse.Attacker = attacker.UID;
                                                            //            ssuse.SpellID = spellz.ID;
                                                            //            ssuse.SpellLevel = spellz.Level;
                                                            //            damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            //            ssuse.AddTarget(attacked, new MsgMagicEffect.DamageClass().Damage = damage, attack);
                                                            //            ReceiveAttack(attacker, attacked, attack, ref damage, spellz);
                                                            //            attacker.Owner.SendScreen(ssuse, true);
                                                            //        }
                                                            //    }
                                                            //}
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.Effect = attack.Effect;
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                            send = true;
                                                        }
                                                    }
                                                    if (send)
                                                        attacker.Owner.SendScreen(suse, true);
                                                    attacker.SpellStamp = Time32.Now;
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region PoisonStar
                                case 6002:
                                    {
                                        if (attackedsob != null) return;
                                        if (attacked.PlayerFlag == PlayerFlag.Monster) return;
                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                int potDifference = attacker.BattlePower - attacked.BattlePower;
                                                int rate = spell.Percent + potDifference;
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                if (CanAttack(attacker, attacked, spell, false))
                                                {
                                                    suse.AddTarget(attacked, 1, attack);
                                                    if (MyMath.Success(rate))
                                                    {
                                                        attacked.NoDrugsStamp = Time32.Now;
                                                        attacker.Stamina -= 20;
                                                        attacked.NoDrugsTime = (short)spell.Duration;
                                                        attacked.AddFlag2((ulong)PacketFlag.Flags.PoisonStar);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.Send(DefineConstantsEn_Res.NoDrugs(spell.Duration));
                                                    }
                                                    else
                                                    {
                                                        suse.Targets[attacked.UID].Hit = false;
                                                    }
                                                    attacked.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region ArcherBane
                                case 6004:
                                    {
                                        if (attackedsob != null) return;
                                        if (attacked.PlayerFlag == PlayerFlag.Monster) return;
                                        if (!attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                int potDifference = attacker.BattlePower - attacked.BattlePower;
                                                int rate = spell.Percent + potDifference;
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                if (CanAttack(attacker, attacked, spell, false))
                                                {
                                                    uint dmg = Calculate.Percent(attacked, 0.1F);
                                                    suse.AddTarget(attacked, dmg, attack);
                                                    if (MyMath.Success(rate))
                                                    {
                                                        attacked.Hitpoints -= dmg;
                                                        attacked.RemoveFlag((ulong)PacketFlag.Flags.Fly);
                                                    }
                                                    else
                                                    {
                                                        suse.Targets[attacked.UID].Hit = false;
                                                    }
                                                    attacked.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region MortalDrag
                                case 11180:
                                    {
                                        if (attacked != null)
                                        {
                                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly)) return;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                            {
                                                if (CanUseSpell(spell, attacker.Owner))
                                                {
                                                    if (!MyMath.Success(Math.Max(5, 100 - (attacked.BattlePower - attacker.BattlePower) / 5))) return;
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = attacked.X;
                                                    suse.Y = attacked.Y;
                                                    ushort newx = attacker.X;
                                                    ushort newy = attacker.Y;
                                                    Map.Pushback(ref newx, ref newy, attacked.Facing, 5);
                                                    if (attacker.Owner.Map.Floor[newx, newy, MapObjectType.Player, attacked])
                                                    {
                                                        suse.X = attacked.X = newx;
                                                        suse.Y = attacked.Y = newy;
                                                    }
                                                    MsgMagicEffect.DamageClass tar = new MsgMagicEffect.DamageClass();
                                                    if (CanAttack(attacker, attacked, spell, false))
                                                    {
                                                        tar.Damage = Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        tar.Damage = (uint)MathHelper.AdjustDataEx((int)tar.Damage, (int)spell.Power);
                                                        suse.AddTarget(attacked, tar, attack);
                                                        ReceiveAttack(attacker, attacked, attack, ref tar.Damage, spell);
                                                    }
                                                    if (attacker.PlayerFlag == PlayerFlag.Player)
                                                        attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region BloodyScythe
                                case 11170:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            MsgMagicEffect.DamageClass tar = new MsgMagicEffect.DamageClass();
                                            foreach (var t in attacker.Owner.Screen.Objects)
                                            {
                                                if (t == null) continue;
                                                if (t.MapObjType == MapObjectType.Player || t.MapObjType == MapObjectType.Monster)
                                                {
                                                    var target = t as Player;
                                                    if (Kernel.GetDistance(X, Y, target.X, target.Y) <= spell.Range)
                                                    {
                                                        if (CanAttack(attacker, target, spell, false))
                                                        {
                                                            tar.Damage = Calculate.Ranged(attacker, target, spell, ref attack) * 100 / 135;
                                                            tar.Hit = true;
                                                            suse.AddTarget(target, tar, attack);
                                                            ReceiveAttack(attacker, target, attack, ref tar.Damage, spell);
                                                        }
                                                    }
                                                }
                                            }
                                            if (attacker.PlayerFlag == PlayerFlag.Player)
                                                attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Steed Spells
                                #region Riding
                                case 7001:
                                    {
                                        if (Constants.NoRiding.Contains(attacker.MapID))
                                        {
                                            attacker.Owner.Send(new MsgTalk("Mr: " + attacker.Name + " You Can`t Use Riding Here !!!", System.Drawing.Color.Red, MsgTalk.World));
                                            return;
                                        }
                                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.ShurikenVortex))
                                            return;
                                        if (!attacker.Owner.Equipment.Free(12))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacker.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                                            {
                                                attacker.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                            }
                                            else
                                            {
                                                if (attacker.Stamina >= 30)
                                                {
                                                    attacker.AddFlag((ulong)PacketFlag.Flags.Ride);
                                                    attacker.Stamina -= 30;
                                                    attacker.Vigor = (ushort)(attacker.Owner.MaxVigor / 2);
                                                    Network.GamePackets.Vigor vigor = new Network.GamePackets.Vigor(true);
                                                    vigor.Amount = attacker.Owner.Vigor;
                                                    vigor.Send(attacker.Owner);
                                                }
                                            }
                                            suse.AddTarget(attacker.UID, 0, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }

                                #endregion
                                #region Spook
                                case 7002:
                                    {
                                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Ride) && attacker.ContainsFlag((ulong)PacketFlag.Flags.Ride))
                                        {
                                            MsgItemInfo attackedSteed = null, attackerSteed = null;
                                            if ((attackedSteed = attacked.Owner.Equipment.TryGetItem(MsgItemInfo.Steed)) != null)
                                            {
                                                if ((attackerSteed = attacker.Owner.Equipment.TryGetItem(MsgItemInfo.Steed)) != null)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    suse.AddTarget(attacked, 0, attack);
                                                    if (attackedSteed.Plus < attackerSteed.Plus)
                                                        attacked.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                                    else if (attackedSteed.Plus == attackerSteed.Plus && attackedSteed.PlusProgress <= attackerSteed.PlusProgress)
                                                        attacked.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                                    else
                                                        suse.Targets[attacked.UID].Hit = false;
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region WarCry
                                case 7003:
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        MsgItemInfo attackedSteed = null, attackerSteed = null;
                                        foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                        {
                                            if (_obj == null) continue;
                                            if (_obj.MapObjType == MapObjectType.Player && _obj.UID != attacker.UID)
                                            {
                                                attacked = _obj as Player;
                                                if ((attackedSteed = attacked.Owner.Equipment.TryGetItem(MsgItemInfo.Steed)) != null)
                                                {
                                                    if ((attackerSteed = attacker.Owner.Equipment.TryGetItem(MsgItemInfo.Steed)) != null)
                                                    {
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= attackedSteed.Plus)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                suse.AddTarget(attacked, 0, attack);
                                                                if (attackedSteed.Plus < attackerSteed.Plus)
                                                                    attacked.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                                                else if (attackedSteed.Plus == attackerSteed.Plus && attackedSteed.PlusProgress <= attackerSteed.PlusProgress)
                                                                    attacked.RemoveFlag((ulong)PacketFlag.Flags.Ride);
                                                                else
                                                                    suse.Targets[attacked.UID].Hit = false;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region ChargingVortex
                                case 11190:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.DefensiveStance))
                                                return;
                                            bool isriding = attacker.ContainsFlag((ulong)PacketFlag.Flags.Ride);

                                            if (!isriding)
                                                attacker.AddFlag((ulong)PacketFlag.Flags.Ride);

                                            if (attacker.Owner.AlternateEquipment)
                                            {
                                                if (attacker.Owner.Equipment.Free(MsgItemInfo.RightWeapon))
                                                {
                                                    if (attacker.Owner.Equipment.Free(MsgItemInfo.AlternateRightWeapon))
                                                        return;
                                                    if (!Network.GamePackets.ItemHandler.IsTwoHand(attacker.Owner.Equipment.TryGetItem(MsgItemInfo.AlternateRightWeapon).ID))
                                                        return;
                                                }
                                                else
                                                {
                                                    if (attacker.Owner.Equipment.Free(MsgItemInfo.AlternateRightWeapon))
                                                    {
                                                        if (!Network.GamePackets.ItemHandler.IsTwoHand(attacker.Owner.Equipment.TryGetItem(MsgItemInfo.RightWeapon).ID))
                                                            return;
                                                    }
                                                    else
                                                        if (!Network.GamePackets.ItemHandler.IsTwoHand(attacker.Owner.Equipment.TryGetItem(MsgItemInfo.AlternateRightWeapon).ID))
                                                            return;
                                                }
                                            }
                                            else
                                            {
                                                if (attacker.Owner.Equipment.Free(MsgItemInfo.RightWeapon))
                                                    return;
                                                if (!Network.GamePackets.ItemHandler.IsTwoHand(attacker.Owner.Equipment.TryGetItem(MsgItemInfo.RightWeapon).ID))
                                                    return;
                                            }
                                            if (attacker.Owner.Map.Floor[X, Y, MapObjectType.InvalidCast, null]) break;
                                            spell.UseStamina = 20;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            UInt16 ox, oy;
                                            ox = attacker.X;
                                            oy = attacker.Y;
                                            attack.X = X;
                                            attack.Y = Y;
                                            attack.Attacker = attacker.UID;
                                            attack.InteractType = 53;
                                            attack.X = X;
                                            attack.Y = Y;
                                            attacker.Owner.SendScreen(attack, true);
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Range + 2)
                                            {
                                                var Array = attacker.Owner.Screen.Objects;
                                                foreach (Interfaces.IMapObject _obj in Array)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(X, Y, attacked.X, attacked.Y) > spell.Range) continue;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 1.5);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.50);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.60);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 2);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.3);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.25);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                            {
                                                                damage = (uint)(damage * 1.2);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.38);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion

                                                            #endregion
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            Calculations.IsBreaking(attacker.Owner, ox, oy);
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.Owner.Screen.Reload(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Monk Spells
                                #region ElementalAura
                                case 10395:
                                case 10410:
                                case 10420:
                                case 10421:
                                case 10422:
                                case 10423:
                                case 10424:
                                    {
                                        HandleAuraMonk(attacker, spell);
                                        break;
                                    }
                                #endregion
                                #region Compassion
                                case 10430:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacker.Owner.Team != null)
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                foreach (Client.GameState teammate in attacker.Owner.Team.Teammates)
                                                {
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, teammate.Player.X, teammate.Player.Y) <= spell.Distance)
                                                    {
                                                        teammate.Player.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                                                        suse.AddTarget(teammate.Player, 0, attack);
                                                    }
                                                }
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                            else
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                attacker.RemoveFlag((ulong)PacketFlag.Flags.Poisoned);
                                                suse.AddTarget(attacker, 0, attack);
                                                if (attacked.PlayerFlag == PlayerFlag.Player)
                                                    attacked.Owner.SendScreen(suse, true);
                                                else attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Serenity
                                case 10400:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            if (attacker == null) return;
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.Targets.Add(attacker.UID, 1);
                                            attacker.ToxicFogLeft = 0;
                                            attacker.NoDrugsTime = 0;
                                            attacker.ScurbyBomb = 0;
                                            attacker.DragonFuryTime = 0;
                                            if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.ScurvyBomb))
                                                attacker.RemoveFlag2((ulong)PacketFlag.Flags.ScurvyBomb);
                                            if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.PoisonStar))
                                                attacker.RemoveFlag2((ulong)PacketFlag.Flags.PoisonStar);
                                            MsgUpdate upgrade = new MsgUpdate(true);
                                            upgrade.UID = attacker.UID;
                                            upgrade.Append((byte)PacketFlag.DataType.Fatigue, 0, 0, 0, 0);
                                            attacker.Owner.Send(upgrade.ToArray());
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Tranquility
                                case 10425:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            if (attacked == null) return;
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;


                                            suse.AddTarget(attacked.UID, 0, attack);

                                            attacked.ToxicFogLeft = 0;
                                            attacked.RemoveFlag2((ulong)PacketFlag.Flags.SoulShackle);
                                            //attacked.Owner.Send(new GameCharacterUpdates(true) { UID = attacked.UID, }
                                            //            .Remove(GameCharacterUpdates.SoulShacle));
                                            attacked.NoDrugsTime = 0;

                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                                attacked.Owner.SendScreen(suse, true);
                                            else
                                                attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region WhirlwindKick
                                case 10415:
                                    {
                                        if (Time32.Now >= attacker.SpellStamp.AddMilliseconds(4))
                                        {
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= 4)
                                            {
                                                if (CanUseSpell(spell, attacker.Owner))
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = (ushort)Kernel.Random.Next(3, 10);
                                                    suse.Y = 0;
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= 3)
                                                    {
                                                        for (int c = 0; c < attacker.Owner.Screen.Objects.Length; c++)
                                                        {
                                                            if (c >= attacker.Owner.Screen.Objects.Length) break;
                                                            Interfaces.IMapObject _obj = attacker.Owner.Screen.Objects[c];
                                                            if (_obj == null) continue;
                                                            if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                            {
                                                                attacked = _obj as Player;
                                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                                {
                                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Ranged))
                                                                    {
                                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                        #region Ataque por nobleza
                                                                        #region Rey atacando a otros
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                                        {
                                                                            //Rey vs Rey
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }

                                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                                        #endregion
                                                                        #region Duque atacando a otros

                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                                        {
                                                                            damage = (uint)(damage * 2.3);
                                                                        }
                                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                                        #endregion
                                                                        #region Marquez atacando a otros
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                                        {
                                                                            damage = (uint)(damage * 2.2);
                                                                        }
                                                                        #endregion
                                                                        #region Earl atacando a otros
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                                        {
                                                                            damage = (uint)(damage * 2.4);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                                        {
                                                                            damage = (uint)(damage * 2.4);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                                        {
                                                                            damage = (uint)(damage * 2.4);
                                                                        }
                                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                                        {
                                                                            damage = (uint)(damage * 2.4);
                                                                        }
                                                                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                                        #endregion

                                                                        #endregion
                                                                        suse.Effect = attack.Effect;
                                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                        attacked.Stunned = true;
                                                                        attacked.StunStamp = Time32.Now;
                                                                        suse.AddTarget(attacked, damage, attack);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        attacker.AttackPacket = null;
                                                    }
                                                    else
                                                    {
                                                        attacker.AttackPacket = null;
                                                        return;
                                                    }
                                                    attacker.Owner.SendScreen(suse, true);
                                                    attacker.SpellStamp = Time32.Now;
                                                    suse.Targets = new SafeDictionary<uint, MsgMagicEffect.DamageClass>();
                                                    attacker.AttackPacket = null;
                                                    return;
                                                }
                                            }
                                            attacker.AttackPacket = null;
                                        }
                                        attacker.AttackPacket = null;
                                        return;
                                    }
                                #endregion
                                #region Palm
                                case 10381:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            //var dis = spell.Distance;
                                            //if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= dis)
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance)
                                            {
                                                PrepareSpell(spell, attacker.Owner);

                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                //suse.SpellLevelHu = client_Spell.UseSpell;
                                                Game.Attacking.InLineAlgorithm ila = new COServer.Game.Attacking.InLineAlgorithm(attacker.X,
                              X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                                bool aaAttack = false;
                                                var Array = attacker.Owner.Screen.Objects;
                                                foreach (Interfaces.IMapObject _obj in Array)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= 5)
                                                        {
                                                            
                                                          
                                                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                                {
                                                                aaAttack = true;
                                                                var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                #region Ataque por nobleza
                                                                #region Rey atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    //Rey vs Rey
                                                                    damage = (uint)(damage * 1.5);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }

                                                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                                #endregion
                                                                #region Duque atacando a otros

                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                                #endregion
                                                                #region Marquez atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 0.9);
                                                                }
                                                                #endregion
                                                                #region Earl atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                                #endregion

                                                                #endregion
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                                    suse.AddTarget(attacked, damage, attack);
                                                                }
                                                            
                                                            else if (ila.InLine(attacked.X, attacked.Y))
                                                            {
                                                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                                {
                                                                    aaAttack = true;
                                                                    var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                    damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                                    suse.AddTarget(attacked, damage, attack);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (attackedsob.UID == Target)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                aaAttack = true;
                                                                // if (!moveIn.InRange(attackedsob.X, attackedsob.Y, 4, ranger))
                                                                //  continue;
                                                                var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack) * 3;
                                                                //damage += damage * 15 / 100;
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                                if (aaAttack)
                                                    attacker.Owner.SendScreen(suse, true);
                                                //attacker.RadiantStamp = DateTime.Now;
                                            }

                                        }
                                        break;
                                    }


                                #endregion
                                #region SoulShackle
                                case 10405:
                                    {
                                        //if (Time32.Now >= attacker.SpellStamp.AddMilliseconds(9000))

                                        if (CanUseSpell(spell, attacker.Owner) && attacked.Dead)
                                        {
                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                            {
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                suse.AddTarget(attacked, 0, attack);
                                                if (attacked.Dead)
                                                {
                                                    if (attacked.BattlePower - attacker.BattlePower >= 10)
                                                        return;
                                                    {


                                                        attacked.ShackleStamp = Time32.Now;
                                                        attacked.ShackleTime = (short)(30 + 15 * spell.Level);

                                                        MsgUpdate upgrade = new MsgUpdate(true);
                                                        upgrade.UID = attacked.UID;
                                                        upgrade.Append((byte)PacketFlag.DataType.SoulShackle, 111, (uint)spell.Duration, 0, 0);
                                                        attacked.Owner.Send(upgrade.ToArray());
                                                        attacked.AddFlag2((ulong)PacketFlag.Flags.SoulShackle);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);

                                                    }
                                                }
                                            }
                                            attacker.AttackPacket = null;
                                            break;

                                        }
                                        break;
                                    }
                                #endregion
                                #region Saint~Monk
                                #region InfernalEcho
                                case 12550:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            var map = attacker.Owner.Map;
                                            if (!map.Floor[X, Y, MapObjectType.Item, null]) return;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.SpecialEffect = 0;
                                            suse.AddTarget(attacker, 0, null);
                                            attacker.Owner.SendScreen(suse, true);

                                            Random R = new Random();
                                            int Nr = R.Next(1, 3);
                                            switch (spell.Level)
                                            {
                                                #region case 0 - 1 - 2 - 3 - 4 - 5
                                                case 0:
                                                case 1:
                                                case 2:
                                                case 3:
                                                case 4:
                                                case 5:
                                                    {
                                                        var area2 = GetLocation2(attacker.X, attacker.Y);
                                                        int count = 3;
                                                        List<System.Drawing.Point> Area = new List<Point>();
                                                        for (int i = 0; i < 360; i += spell.Sector)
                                                        {
                                                            if (Area.Count >= count)
                                                            {
                                                                break;
                                                            }
                                                            int r = i;
                                                            var distance = Kernel.Random.Next(spell.Range, spell.Distance);
                                                            var x2 = (ushort)(X + (distance * Math.Cos(r)));
                                                            var y2 = (ushort)(Y + (distance * Math.Sin(r)));
                                                            System.Drawing.Point point = new System.Drawing.Point((int)x2, (int)y2);
                                                            if (!Area.Contains(point))
                                                            {
                                                                Area.Add(point);
                                                            }
                                                            else
                                                            {
                                                                i--;
                                                            }
                                                        }
                                                        if (Nr == 1)
                                                        {
                                                            foreach (var a in area2)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;
                                                            }
                                                        }
                                                        else if (Nr == 2)
                                                        {
                                                            foreach (var a in Area)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;
                                                            }
                                                        }
                                                        else if (Nr == 3)
                                                        {
                                                            foreach (var a in Area)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                #endregion
                                                #region case 6
                                                case 6:
                                                    {
                                                        var area2 = GetLocation(attacker.X, attacker.Y);
                                                        int count = 4;
                                                        List<System.Drawing.Point> Area = new List<Point>();
                                                        for (int i = 0; i < 360; i += spell.Sector)
                                                        {
                                                            if (Area.Count >= count)
                                                            {
                                                                break;
                                                            }
                                                            int r = i;
                                                            var distance = Kernel.Random.Next(spell.Range, spell.Distance);
                                                            var x2 = (ushort)(X + (distance * Math.Cos(r)));
                                                            var y2 = (ushort)(Y + (distance * Math.Sin(r)));
                                                            System.Drawing.Point point = new System.Drawing.Point((int)x2, (int)y2);
                                                            if (!Area.Contains(point))
                                                            {
                                                                Area.Add(point);
                                                            }
                                                            else
                                                            {
                                                                i--;
                                                            }
                                                        }
                                                        if (Nr == 1)
                                                        {
                                                            foreach (var a in area2)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;

                                                            }
                                                        }
                                                        else if (Nr == 2)
                                                        {
                                                            foreach (var a in Area)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;
                                                            }
                                                        }
                                                        else if (Nr == 3)
                                                        {
                                                            foreach (var a in Area)
                                                            {
                                                                MsgMapItem item = new MsgMapItem(true);
                                                                item.ItemID = 1397;
                                                                item.MapID = attacker.MapID;
                                                                item.Type = MsgMapItem.Effect;
                                                                item.X = (ushort)a.X;
                                                                item.Y = (ushort)a.Y;
                                                                item.OnFloor = Time32.Now;
                                                                item.Owner = attacker.Owner;
                                                                item.UID = MsgMapItem.FloorUID.Next;
                                                                while (attacker.Owner.Map.FloorItems.ContainsKey(item.UID))
                                                                    item.UID = MsgMapItem.FloorUID.Next;
                                                                item.mColor = 14;
                                                                item.OwnerUID = attacker.UID;
                                                                item.OwnerGuildUID = attacker.GuildID;
                                                                item.FlowerType = 0;
                                                                item.Time = Kernel.TqTimer(DateTime.Now.AddSeconds(3));
                                                                item.Name = "InfernalEcho";
                                                                attacker.Owner.SendScreenSpawn(item, true);
                                                                attacker.AttackPacket = null;
                                                            }
                                                        }
                                                        break;
                                                    }
                                                    #endregion
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region UpSweep
                                case 12580:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacked == null)
                                                return;
                                            var angle = Kernel.GetAngle(attacker.X, attacker.Y, attacked.X, attacked.Y);
                                            Map.UpdateCoordonatesForAngle(ref X, ref Y, angle);
                                            Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, _obj.X, _obj.Y) <= 7)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;

                                                        if (sector.IsInFan(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;

                                                        if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region DownSweep
                                case 12590://down                                                          
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacked == null)
                                                return;
                                            var angle = Kernel.GetAngle(attacker.X, attacker.Y, attacked.X, attacked.Y);
                                            angle = Enums.OppositeAngle(angle);
                                            Map.UpdateCoordonatesForAngle(ref X, ref Y, angle);

                                            Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);

                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, _obj.X, _obj.Y) <= 7)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;

                                                        if (sector.IsInFan(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;

                                                        if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region strike
                                case 12600:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;

                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= attacker.AttackRange + 1)
                                            {
                                                if (attackedsob != null)
                                                {
                                                    if (CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                                else
                                                {
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);

                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        suse.Effect = attack.Effect;

                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                attacker.AttackPacket = null;
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        attacker.AttackPacket = null;
                                        break;
                                    }
                                #endregion
                                #region WrathOfEmperor
                                case 12570:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.SpecialEffect = 1;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Range)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null)
                                                        continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                                                                return;
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);

                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                suse.Effect = attack.Effect;

                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region GraceOfHeaven
                                case 12560:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            SpellID = 10425;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacker.AttackPacket = null;
                                            if (attacker.Owner.Team != null)
                                            {

                                                foreach (Client.GameState teammate in attacker.Owner.Team.Teammates)
                                                {
                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, teammate.Player.X, teammate.Player.Y) <= spell.Range)
                                                    {
                                                        if (teammate.Player.ContainsFlag2((ulong)PacketFlag.Flags.SoulShackle))
                                                        {
                                                            teammate.Player.RemoveFlag2((ulong)PacketFlag.Flags.SoulShackle);

                                                            suse.AddTarget(teammate.Player, 0, attack);

                                                            MsgUpdate upgrade = new MsgUpdate(true);
                                                            upgrade.UID = teammate.Player.UID;
                                                            upgrade.Append((byte)PacketFlag.DataType.SoulShackle
                                                                , 111
                                                                , 0, 0, spell.Level);
                                                            if (teammate.Player.PlayerFlag == PlayerFlag.Player)
                                                                teammate.Player.Owner.Send(upgrade.ToArray());
                                                        }
                                                    }
                                                }
                                                attacker.Owner.SendScreen(suse, true);

                                            }
                                            else
                                            {
                                                suse.AddTarget(attacked, 0, attack);

                                                attacked.ToxicFogLeft = 0;
                                                attacked.RemoveFlag2((ulong)PacketFlag.Flags.SoulShackle);

                                                MsgUpdate upgrade = new MsgUpdate(true);
                                                upgrade.UID = attacked.UID;
                                                upgrade.Append((byte)PacketFlag.DataType.SoulShackle
                                                    , 111
                                                    , 0, 0, spell.Level);
                                                if (attacked.PlayerFlag == PlayerFlag.Player)
                                                    attacked.Owner.Send(upgrade.ToArray());

                                                attacked.ToxicFogLeft = 0;
                                                attacked.ScurbyBomb = 0;
                                                attacked.NoDrugsTime = 0;
                                                attacked.DragonFuryTime = 0;
                                                attacked.NoDrugsTime = 0;
                                                if (attacked.PlayerFlag == PlayerFlag.Player)
                                                    attacked.Owner.SendScreen(suse, true);
                                                else
                                                    attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        break;
                                    }
                                #endregion

                                #endregion
                                #endregion
                                #region Pirate Spells
                                #region GaleBomb
                                case 11070:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            Map map;
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            MsgMagicEffect.DamageClass tar = new MsgMagicEffect.DamageClass();
                                            int num = 0;
                                            switch (spell.Level)
                                            {
                                                case 0:
                                                case 1: num = 3; break;
                                                case 2:
                                                case 3: num = 4; break;
                                                default: num = 5; break;
                                            }
                                            int i = 0;
                                            Kernel.Maps.TryGetValue(attacker.Owner.Map.BaseID, out map);
                                            foreach (var t in attacker.Owner.Screen.Objects)
                                            {
                                                if (t == null) continue;
                                                if (t.MapObjType == MapObjectType.Player || t.MapObjType == MapObjectType.Monster)
                                                {
                                                    var target = t as Player;
                                                    if (Kernel.GetDistance(X, Y, target.X, target.Y) <= spell.Range)
                                                    {
                                                        if (CanAttack(attacker, target, spell, false))
                                                        {
                                                            tar.Damage = Calculate.Melee(attacker, target, ref attack);
                                                            tar.Damage = (uint)MathHelper.AdjustDataEx((int)tar.Damage * 3, (int)spell.Power);
                                                            
                                                            tar.Hit = true;
                                                            tar.newX = target.X;
                                                            tar.newY = target.Y;
                                                            Map.Pushback(ref tar.newX, ref tar.newY, attacker.Facing, 5);
                                                            if (map != null)
                                                            {
                                                                if (map.Floor[tar.newX, tar.newY, MapObjectType.Player, attacker])
                                                                {
                                                                    target.X = tar.newX;
                                                                    target.Y = tar.newY;
                                                                }
                                                                else
                                                                {
                                                                    tar.newX = target.X;
                                                                    tar.newY = target.Y;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (attacker.Owner.Map.Floor[tar.newX, tar.newY, MapObjectType.Player, attacker])
                                                                {
                                                                    target.X = tar.newX;
                                                                    target.Y = tar.newY;
                                                                }
                                                                else
                                                                {
                                                                    target.X = tar.newX;
                                                                    target.Y = tar.newY;
                                                                }
                                                            }
                                                            suse.AddTarget(target, tar, attack);
                                                            ReceiveAttack(attacker, target, attack, ref tar.Damage, spell);
                                                            i++;
                                                            if (i > num) break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (attacker.PlayerFlag == PlayerFlag.Player)
                                                attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region BladeTempest
                                case 11110:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            ushort _X = attacker.X, _Y = attacker.Y;
                                            ushort _tX = X, _tY = Y;
                                            byte dist = (byte)spell.Distance;
                                            var Array = attacker.Owner.Screen.Objects;
                                            InLineAlgorithm algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist, InLineAlgorithm.Algorithm.DDA);
                                            X = attacker.X;
                                            Y = attacker.Y;
                                            int i = 0;
                                            for (i = 0; i < algo.lcoords.Count; i++)
                                            {
                                                if (attacker.Owner.Map.Floor[algo.lcoords[i].X, algo.lcoords[i].Y, MapObjectType.Player]
                                                    && !attacker.ThroughGate(algo.lcoords[i].X, algo.lcoords[i].Y))
                                                {
                                                    X = (ushort)algo.lcoords[i].X;
                                                    Y = (ushort)algo.lcoords[i].Y;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (!attacker.Owner.Map.Floor[X, Y, MapObjectType.Player, null]) return;
                                            double disth = 1.5;
                                            foreach (Interfaces.IMapObject _obj in Array)
                                            {
                                                bool hit = false;
                                                for (int j = 0; j < i; j++)
                                                    if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                        hit = true;
                                                if (hit)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);

                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.1);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.1);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.1);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.1);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 0.9);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                            {
                                                                damage = (uint)(damage * 1.1);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 0.9);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.4);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.6);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion

                                                            #endregion
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.PX = attacker.X;
                                            attacker.PY = attacker.Y;
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.Owner.Screen.Reload(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Kraken`sRevenge
                                case 11100:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            int num = 0;
                                            switch (spell.Level)
                                            {
                                                case 0:
                                                case 1: num = 3; break;
                                                case 2:
                                                case 3: num = 4; break;
                                                default: num = 5; break;
                                            }
                                            int i = 0;
                                            MsgDeadMark bsp = new MsgDeadMark();
                                            foreach (var t in attacker.Owner.Screen.Objects.OrderBy(x => Kernel.GetDistance(x.X, x.Y, attacker.Owner.Player.X, attacker.Owner.Player.Y)))
                                            {
                                                if (t == null)
                                                    continue;
                                                if (t.MapObjType == MapObjectType.Player || t.MapObjType == MapObjectType.Monster)
                                                {
                                                    var target = t as Player;
                                                    if (CanAttack(attacker, target, spell, false))
                                                    {
                                                        target.IsBlackSpotted = true;
                                                        target.BlackSpotStamp = Time32.Now;
                                                        target.BlackSpotStepSecs = spell.Duration;
                                                        Kernel.BlackSpoted.TryAdd(target.UID, target);
                                                        suse.AddTarget(target, new MsgMagicEffect.DamageClass(), attack);
                                                        i++;
                                                        if (i == num) break;
                                                    }
                                                }
                                            }
                                            if (attacker.PlayerFlag == PlayerFlag.Player)
                                                attacker.Owner.SendScreen(suse, true);
                                            foreach (var h in Server.GamePool)
                                            {
                                                foreach (var t in suse.Targets.Keys)
                                                {
                                                    h.Send(bsp.ToArray(true, t));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region ScurvyBomb
                                case 11040:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(X, Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                int potDifference = attacker.BattlePower - attacked.BattlePower;
                                                                int rate = spell.Percent + potDifference;
                                                                if (MyMath.Success(rate))
                                                                {
                                                                    if (_obj.MapObjType == MapObjectType.Player)
                                                                    {
                                                                        attacked.ScurbyBombStamp = Time32.Now;
                                                                        attacked.ScurbyBomb2Stamp = Time32.Now;
                                                                        attacked.ScurbyBomb = 20;
                                                                        attacked.AddFlag2((ulong)PacketFlag.Flags.ScurvyBomb);
                                                                        MsgUpdate upgrade = new MsgUpdate(true);
                                                                        upgrade.UID = attacked.UID;
                                                                        upgrade.Append((byte)PacketFlag.DataType.Fatigue, 20, 20, 0, 0);
                                                                        attacked.Owner.Send(upgrade.ToArray());
                                                                    }
                                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                                    uint damage = Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                    #region Ataque por nobleza
                                                                    #region Rey atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        //Rey vs Rey
                                                                        damage = (uint)(damage * 1.65);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.7);
                                                                    }

                                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                                    #endregion
                                                                    #region Duque atacando a otros

                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.3);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.5);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.5);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                                    #endregion
                                                                    #region Marquez atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 1.3);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.5);
                                                                    }
                                                                    #endregion
                                                                    #region Earl atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                                    #endregion

                                                                    #endregion
                                                                    suse.Effect = attack.Effect;
                                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                    suse.Targets.Add(attacked.UID, damage);
                                                                }
                                                                else
                                                                {
                                                                    suse.Targets.Add(attacked.UID, 0);
                                                                    suse.Targets[attacked.UID].Hit = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Cannon Barrage
                                case 11050:
                                    {
                                        if (attacker.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                            attacker.Owner.Player.AddFlag2((ulong)PacketFlag.Flags.CannonBarrage);
                                            attacker.Owner.Player.CannonBarrageStamp = Time32.Now;
                                            return;
                                        }
                                        PrepareSpell(spell, attacker.Owner);
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        MsgMagicEffect.DamageClass tar = new MsgMagicEffect.DamageClass();
                                        foreach (var t in attacker.Owner.Screen.Objects)
                                        {
                                            if (t == null) continue;
                                            if (t.MapObjType == MapObjectType.Player || t.MapObjType == MapObjectType.Monster)
                                            {
                                                var target = t as Player;
                                                if (Kernel.GetDistance(attacker.Owner.Player.X, attacker.Owner.Player.Y, target.X, target.Y) <= spell.Range)
                                                {
                                                    if (CanAttack(attacker, target, spell, false))
                                                    {
                                                        tar.Damage = Calculate.Ranged(attacker, target, ref attack);
                                                        ReceiveAttack(attacker, target, attack, ref tar.Damage, spell);
                                                        suse.AddTarget(target, tar, attack);
                                                    }
                                                }
                                            }
                                        }
                                        if (attacker.PlayerFlag == PlayerFlag.Player)
                                            attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region Blackbeard`sRage
                                case 11060:
                                    {
                                        if (attacker.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                            attacker.Owner.Player.AddFlag2((ulong)PacketFlag.Flags.BlackbeardsRage);
                                            attacker.Owner.Player.BlackbeardsRageStamp = Time32.Now;
                                            return;
                                        }
                                        int targets = 0;
                                        switch (spell.Level)
                                        {
                                            case 0:
                                            case 1: targets = 3; break;
                                            case 2:
                                            case 3: targets = 4; break;
                                            default: targets = 5; break;
                                        }
                                        int i = 0;
                                        PrepareSpell(spell, attacker.Owner);
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        MsgMagicEffect.DamageClass tar = new MsgMagicEffect.DamageClass();
                                        foreach (var t in attacker.Owner.Screen.Objects)
                                        {
                                            if (t == null) continue;
                                            if (t.MapObjType == MapObjectType.Player || t.MapObjType == MapObjectType.Monster)
                                            {
                                                var target = t as Player;
                                                if (Kernel.GetDistance(attacker.Owner.Player.X, attacker.Owner.Player.Y, target.X, target.Y) <= spell.Range)
                                                {
                                                    if (CanAttack(attacker, target, spell, false))
                                                    {
                                                        tar.Damage = Calculate.Ranged(attacker, target, ref attack);
                                                        suse.AddTarget(target, tar, attack);
                                                        ReceiveAttack(attacker, target, attack, ref tar.Damage, spell);
                                                        i++;
                                                        if (i == targets) break;
                                                    }
                                                }
                                            }
                                        }
                                        if (attacker.PlayerFlag == PlayerFlag.Player)
                                            attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Warrior Spells
                                #region ExtraXP
                                case 1040:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacker.Owner.Team != null)
                                            {
                                                foreach (Client.GameState teammate in attacker.Owner.Team.Teammates)
                                                {
                                                    if (teammate.Player.UID != attacker.UID)
                                                    {
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, teammate.Player.X, teammate.Player.Y) <= spell.Distance)
                                                        {
                                                            teammate.XPCount += 20;
                                                            MsgUpdate update = new MsgUpdate(true);
                                                            update.UID = teammate.Player.UID;
                                                            update.Append((byte)PacketFlag.DataType.XPCircle, teammate.XPCount);
                                                            update.Send(teammate);
                                                            suse.Targets.Add(teammate.Player.UID, 20);
                                                            if (spell.NextSpellID != 0)
                                                            {
                                                                attack.Damage = spell.NextSpellID;
                                                                attacker.AttackPacket = attack;
                                                            }
                                                            else attacker.AttackPacket = null;
                                                        }
                                                    }
                                                }
                                            }
                                            if (attacked.PlayerFlag == PlayerFlag.Player)
                                                attacked.Owner.SendScreen(suse, true);
                                            else attacked.MonsterInfo.SendScreen(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Dash
                                case 1051:
                                    {
                                        if (attacked != null)
                                        {
                                            if (!attacked.Dead)
                                            {
                                                var direction = Kernel.GetAngle(attacker.X, attacker.Y, attacked.X, attacked.Y);
                                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                {
                                                    attack = new MsgInteract(true);
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Calculate.Ranged(attacker, attacked, ref attack);
                                                    attack.InteractType = MsgInteract.Dash;
                                                    attack.X = attacked.X;
                                                    attack.Y = attacked.Y;
                                                    attack.Attacker = attacker.UID;
                                                    attack.Attacked = attacked.UID;
                                                    attack.Damage = damage;
                                                    attack.ToArray()[27] = (byte)direction;
                                                    attacked.Move(direction);
                                                    attacker.Move(direction);
                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                    //attacker.Owner.SendScreen(attack, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region MagicDefender
                                case 11200:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacker.Class >= 20 && attacker.Class <= 25)
                                            {
                                                if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.MagicDefender))
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.MagicDefender))
                                                    {
                                                        attacker.RemoveFlag3((ulong)PacketFlag.Flags.MagicDefender);
                                                    }
                                                    attacker.MagicDefenderStamp = Time32.Now;
                                                    attacker.MagicDefender = spell.Duration;
                                                    attacker.AddFlag3((ulong)PacketFlag.Flags.MagicDefender);
                                                    MsgUpdate upgrade = new MsgUpdate(true);
                                                    upgrade.UID = attacker.UID;
                                                    upgrade.Append((byte)PacketFlag.DataType.AzureShield, 128, (uint)spell.Duration, 0, spell.Level);
                                                    attacker.Owner.Send(upgrade.ToArray());
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region ShieldBlock
                                case 10470:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacker.Class >= 20 && attacker.Class <= 25)
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                attacker.BlockShieldStamp = Time32.Now;
                                                attacker.BlockShield = spell.Duration;
                                                attacker.BlockShieldCheck = true;
                                                attacker.Owner.ReloadBlock();
                                                MsgUpdate upgrade = new MsgUpdate(true);
                                                upgrade.UID = attacker.UID;
                                                upgrade.Append((byte)PacketFlag.DataType.AzureShield, 113, (uint)spell.Duration, spell.Power, spell.Level);
                                                attacker.Owner.Send(upgrade.ToArray());
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region DefensiveStance
                                case 11160:
                                    {
                                        if (Time32.Now >= attacker.DefensiveStanceStamp.AddSeconds(10))
                                        {
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                if (attacker.Class >= 20 && attacker.Class <= 25)
                                                {
                                                    PrepareSpell(spell, attacker.Owner);
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (attacker.ContainsFlag2((ulong)PacketFlag.Flags.DefensiveStance))
                                                    {
                                                        attacker.RemoveFlag2((ulong)PacketFlag.Flags.DefensiveStance);
                                                        attacker.IsDefensiveStance = false;
                                                    }
                                                    else if (!attacker.ContainsFlag2((ulong)PacketFlag.Flags.DefensiveStance))
                                                    {
                                                        attacker.DefensiveStanceStamp = Time32.Now;
                                                        attacker.DefensiveStance = spell.Duration;
                                                        attacker.IsDefensiveStance = true;
                                                        attacker.AddFlag2((ulong)PacketFlag.Flags.DefensiveStance);
                                                        MsgUpdate upgrade = new MsgUpdate(true);
                                                        upgrade.UID = attacker.UID;
                                                        upgrade.Append((byte)PacketFlag.DataType.DefensiveStance, 126, (uint)spell.Duration, (uint)spell.PowerPercent, spell.Level);
                                                        attacker.Owner.Send(upgrade.ToArray());
                                                        attacker.Owner.SendScreen(suse, true);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Assassin skills
                                #region PathOfShadow
                                case 11620:
                                    {
                                        var weps = attacker.Owner.Weapons;
                                        if ((weps.Item1 != null && weps.Item1.ID / 1000 != 613) || (weps.Item2 != null && weps.Item2.ID / 1000 != 613))
                                        {
                                            attacker.Owner.Send(new MsgTalk("You need to wear only knifes!", Color.Red, (uint)PacketMsgTalk.MsgTalkType.Talk));
                                            return;
                                        }
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = attacker.UID;
                                        MsgMagicEffect.SpellID = spell.ID;
                                        MsgMagicEffect.SpellLevel = spell.Level;
                                        MsgMagicEffect.X = X;
                                        MsgMagicEffect.Y = Y;
                                        attacker.Owner.SendScreen(MsgMagicEffect, true);
                                        attacker.EquipmentColor = (uint)attacker.BattlePower;
                                        if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow))
                                        {
                                            attacker.RemoveFlag3((ulong)PacketFlag.Flags.PathOfShadow);
                                            if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.BladeFlurry))
                                                attacker.RemoveFlag3((ulong)PacketFlag.Flags.BladeFlurry);
                                            if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.KineticSpark))
                                                attacker.RemoveFlag3((ulong)PacketFlag.Flags.KineticSpark);
                                        }
                                        else attacker.AddFlag3((ulong)PacketFlag.Flags.PathOfShadow);
                                        break;
                                    }
                                #endregion
                                #region Blade Furry
                                case 11610:
                                    {
                                        if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow)) return;
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.BladeFlurryTime = 20;
                                            attacker.AddFlag3((ulong)PacketFlag.Flags.BladeFlurry);
                                            attacker.BladeFlurryStamp = Time32.Now;
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                        }
                                        else
                                        {
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Range)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                if (attacker.SpiritFocus)
                                                                {
                                                                    damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                    attacker.SpiritFocus = false;
                                                                }
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                if (attacker.SpiritFocus)
                                                                {
                                                                    damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                    attacker.SpiritFocus = false;
                                                                }
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region Mortal Wound
                                case 11660:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow)) return;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attackedsob == null)
                                            {
                                                if (CanAttack(attacker, attacked, spell, false))
                                                {
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                    damage = (uint)MathHelper.AdjustDataEx((int)damage / 10, (int)spell.Power / 2);
                                                    #region Ataque por nobleza
                                                    #region Rey atacando a otros
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                    {
                                                        //Rey vs Rey
                                                        damage = (uint)(damage * 1.0);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                    {
                                                        damage = (uint)(damage * 1.05);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                    {
                                                        damage = (uint)(damage * 1.06);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                    {
                                                        damage = (uint)(damage * 1.06);
                                                    }

                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                    #endregion
                                                    #region Duque atacando a otros

                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                    {
                                                        damage = (uint)(damage * 1.02);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                    {
                                                        damage = (uint)(damage * 1.00);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                    {
                                                        damage = (uint)(damage * 1.01);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                    {
                                                        damage = (uint)(damage * 1.05);
                                                    }
                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                    #endregion
                                                    #region Marquez atacando a otros
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                    {
                                                        damage = (uint)(damage * 1.05);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                    {
                                                        damage = (uint)(damage * 1.10);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                    {
                                                        damage = (uint)(damage * 1.00);
                                                    }
                                                    #endregion
                                                    #region Earl atacando a otros
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                    {
                                                        damage = (uint)(damage * 1.0);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                    {
                                                        damage = (uint)(damage * 1.0);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                    {
                                                        damage = (uint)(damage * 1.0);
                                                    }
                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                    {
                                                        damage = (uint)(damage * 1.0);
                                                    }
                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                    #endregion

                                                    #endregion
                                                    if (attacker.SpiritFocus)
                                                    {
                                                        damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                        attacker.SpiritFocus = false;
                                                    }
                                                    suse.Effect = attack.Effect;
                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                    suse.AddTarget(attacked, damage, attack);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                            else
                                            {
                                                if (CanAttack(attacker, attackedsob, spell))
                                                {
                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                    if (attacker.SpiritFocus)
                                                    {
                                                        damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                        attacker.SpiritFocus = false;
                                                    }
                                                    suse.Effect = attack.Effect;
                                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                    suse.AddTarget(attackedsob, damage, attack);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Blistering Wave
                                case 11650:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow)) return;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, spell.Distance);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (sector.Inside(attacked.X, attacked.Y))
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage / 2, (int)spell.Power / 2);
                                                            #region Ataque por nobleza
                                                            #region Rey atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                //Rey vs Rey
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.05);
                                                            }

                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                            #endregion
                                                            #region Duque atacando a otros

                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 0.9);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 0.85);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.01);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.05);
                                                            }
                                                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                            #endregion
                                                            #region Marquez atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.05);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.10);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 0.7);
                                                            }
                                                            #endregion
                                                            #region Earl atacando a otros
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                            {
                                                                damage = (uint)(damage * 1.0);
                                                            }
                                                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                            #endregion
                                                            
                                                            #endregion
                                                            suse.Effect = attack.Effect;
                                                            if (attacker.SpiritFocus)
                                                            {
                                                                damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                attacker.SpiritFocus = false;
                                                            }
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            if (attacker.SpiritFocus)
                                                            {
                                                                damage = (uint)(damage * attacker.SpiritFocusPercent);
                                                                attacker.SpiritFocus = false;
                                                            }
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region SpiritFocus
                                case 9000:
                                case 11670:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacker.IntensifyPercent = spell.PowerPercent;
                                            attacker.IntensifyStamp = Time32.Now;
                                            attacker.SpiritFocus = true;
                                            MsgInteract Attacker = new MsgInteract(true)
                                            {
                                                Attacker = this.attacker.UID,
                                                InteractType = 24,
                                                X = X,
                                                Y = Y
                                            };
                                            Writer.WriteUInt16(0, 4, Attacker.ToArray());
                                            Writer.WriteUInt16(attack.X, 20, Attacker.ToArray());
                                            Writer.WriteUInt16(attack.Y, 22, Attacker.ToArray());
                                            Writer.WriteUInt16(spell.ID, 28, Attacker.ToArray());
                                            Writer.WriteByte(spell.Level, 30, Attacker.ToArray());
                                            attacker.Owner.SendScreen(Attacker, true);
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            attacker.SendSpawn(this.attacker.Owner);
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Kinetic Spark
                                case 11590:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow)) return;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.Attacker = attacker.UID;
                                            MsgMagicEffect.SpellID = spell.ID;
                                            MsgMagicEffect.SpellLevel = spell.Level;
                                            MsgMagicEffect.X = X;
                                            MsgMagicEffect.Y = Y;
                                            attacker.Owner.SendScreen(MsgMagicEffect, true);
                                            if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.KineticSpark))
                                                attacker.RemoveFlag3((ulong)PacketFlag.Flags.KineticSpark);
                                            else attacker.AddFlag3((ulong)PacketFlag.Flags.KineticSpark);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Dagger Storm
                                case 11600:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow))
                                                return;

                                            var map = attacker.Owner.Map;
                                            if (!map.Floor[X, Y, MapObjectType.Item, null]) return;
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.AddTarget(attacker, 0, null);
                                            attacker.Owner.SendScreen(suse, true);

                                            MsgMapItem floorItem = new MsgMapItem(true);
                                            floorItem.MapObjType = MapObjectType.Item;
                                            if (attacker.Owner.Spells[spellID].LevelHu2 == 1)
                                                floorItem.ItemID = PacketMsgMapItem.FuryofEgg;
                                            else if (attacker.Owner.Spells[spellID].LevelHu2 == 2)
                                                floorItem.ItemID = PacketMsgMapItem.ShacklingIce;
                                            else
                                                floorItem.ItemID = PacketMsgMapItem.DaggerStorm;
                                            floorItem.MapID = attacker.MapID;
                                            floorItem.ItemColor = Enums.Color.Black;
                                            floorItem.Type = PacketMsgMapItem.Effect;
                                            floorItem.X = X;
                                            floorItem.Y = Y;
                                            floorItem.OnFloor = Time32.Now;
                                            floorItem.Owner = attacker.Owner;
                                            while (map.Npcs.ContainsKey(floorItem.UID))
                                                floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                            map.AddFloorItem(floorItem);
                                            attacker.Owner.SendScreenSpawn(floorItem, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Trojan Skills
                                #region CruelShabe
                                case 3050:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attackedsob != null)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        uint damage = Game.Attacking.Calculate.Percent(attackedsob, spell.PowerPercent);
                                                        attackedsob.Hitpoints -= damage;
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                        attacker.Owner.SendScreen(suse, true);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        uint damage = Game.Attacking.Calculate.Percent(attacked, spell.PowerPercent);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                        {
                                                            attacker.Owner.UpdateQualifier(damage);
                                                        }
                                                        attacked.Hitpoints -= damage;
                                                        suse.AddTarget(attacked, damage, attack);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region SuperCyclone
                                case 11970:
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        if (attacker.OnSuperCyclone()) return;
                                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacked.SuperCycloneTime = 40;
                                            attacker.AddFlag3((ulong)PacketFlag.Flags.SuperCyclone);
                                            attacker.SuperCycloneStamp = Time32.Now;
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region FatalCross
                                case 11980:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            RangeMove moveIn = new RangeMove();
                                            List<RangeMove.Coords> ranger = moveIn.MoveCoords(attacker.X, attacker.Y, X, Y);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (!moveIn.InRange(attacked.X, attacked.Y, 2, ranger)) continue;
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                        #region Ataque por nobleza
                                                        #region Rey atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            //Rey vs Rey
                                                            damage = (uint)(damage * 1.5);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 1.50);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 1.60);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 2);
                                                        }

                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                        #endregion
                                                        #region Duque atacando a otros

                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 1.3);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 1.25);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                        {
                                                            damage = (uint)(damage * 1.2);
                                                        }
                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                        #endregion
                                                        #region Marquez atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 1.38);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        #endregion
                                                        #region Earl atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 1.6);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 1.6);
                                                        }
                                                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                        #endregion

                                                        #endregion
                                                        suse.Effect = attack.Effect;
                                                        attack.Damage = damage;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.Monster)
                                                {
                                                    attacked = _obj as Player;
                                                    if (!moveIn.InRange(attacked.X, attacked.Y, 2, ranger)) continue;
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                     

                                                        suse.Effect = attack.Effect;
                                                        attack.Damage = damage;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (attackedsob == null) continue;
                                                    if (!moveIn.InRange(attackedsob.X, attackedsob.Y, 2, ranger)) continue;
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        attack.Damage = damage;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region MortalStrike
                                case 11990:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, 5);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;

                                                    if (sector.Inside(attacked.X, attacked.Y))
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack) * 100 / 150;
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)25240);
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;

                                                    if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            if (damage == 0)
                                                                damage = 1;
                                                            damage = Game.Attacking.Calculate.Percent((int)damage, spell.PowerPercent);

                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }

                                        break;
                                    }
                                #endregion
                                #endregion
                                #region LeeLong Skills
                                #region DragonFlow
                                case 12270:
                                    {
                                        if (attacker.ContainsFlag3((ulong)PacketFlag.Flags.DragonFlow))
                                        {
                                            attacker.RemoveFlag3((ulong)PacketFlag.Flags.DragonFlow);
                                        }
                                        else
                                        {
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.Attacker = attacker.UID;
                                            MsgMagicEffect.Attacker1 = attacker.UID;
                                            MsgMagicEffect.SpellID = spell.ID;
                                            MsgMagicEffect.SpellLevel = spell.Level;
                                            MsgMagicEffect.X = X;
                                            MsgMagicEffect.Y = Y;
                                            MsgMagicEffect.AddTarget(attacker, 1, null);
                                            attacker.Owner.SendScreen(MsgMagicEffect, true);
                                            attacker.AddFlag3((ulong)PacketFlag.Flags.DragonFlow);
                                            attacker.DragonFlowStamp = Time32.Now;
                                        }
                                        break;
                                    }
                                #endregion
                                #region DragonRoar
                                case 12280:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            PrepareSpell(spell, attacker.Owner);
                                            if (attacker.Owner.Team != null)
                                            {
                                                foreach (Client.GameState teammate in attacker.Owner.Team.Teammates)
                                                {
                                                    if (teammate.Player.UID != attacker.UID)
                                                    {
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, teammate.Player.X, teammate.Player.Y) <= spell.Range)
                                                        {
                                                            teammate.Player.Stamina += (byte)spell.Power;
                                                            suse.AddTarget(teammate.Player, spell.Power, null);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region DragonCyclone
                                case 12290:
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.KOCount = 0;
                                            attacker.DragonCycloneTime = 40;
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                            attacker.AddFlag3((ulong)PacketFlag.Flags.DragonCyclone);
                                            attacker.DragonCycloneStamp = Time32.Now;
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        else
                                        {
                                            ushort _X = attacker.X, _Y = attacker.Y;
                                            ushort _tX = X, _tY = Y;
                                            byte dist = (byte)spell.Distance;
                                            var Array = attacker.Owner.Screen.Objects;
                                            InLineAlgorithm algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist, InLineAlgorithm.Algorithm.DDA);
                                            X = attacker.X;
                                            Y = attacker.Y;
                                            int i = 0;
                                            for (i = 0; i < algo.lcoords.Count; i++)
                                            {
                                                if (attacker.Owner.Map.Floor[algo.lcoords[i].X, algo.lcoords[i].Y, MapObjectType.Player] && !attacker.ThroughGate(algo.lcoords[i].X, algo.lcoords[i].Y))
                                                {
                                                    X = (ushort)algo.lcoords[i].X;
                                                    Y = (ushort)algo.lcoords[i].Y;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (!attacker.Owner.Map.Floor[X, Y, MapObjectType.Player, null]) return;
                                            double disth = 1.5;
                                            foreach (Interfaces.IMapObject _obj in Array)
                                            {
                                                bool hit = false;
                                                for (int j = 0; j < i; j++)
                                                    if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                        hit = true;
                                                if (hit)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage, 30030);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.PX = attacker.X;
                                            attacker.PY = attacker.Y;
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.Owner.Screen.Reload(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #region DragonSlash
                                case 12350:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            InLineAlgorithm ila = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                        #region Ataque por nobleza
                                                        #region Rey atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 0.8);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }

                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                        #endregion
                                                        #region Duque atacando a otros

                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 0.8);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                        #endregion
                                                        #region Marquez atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 0.9);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 0.8);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 0.8);
                                                        }
                                                        #endregion
                                                        #region Earl atacando a otros
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                        {
                                                            damage = (uint)(damage * 1.4);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                        {
                                                            damage = (uint)(damage * 1.6);
                                                        }
                                                        if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                        {
                                                            damage = (uint)(damage * 1.6);
                                                        }
                                                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                        #endregion

                                                        #endregion
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (ila.InLine(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #region AirKick
                                case 12320:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            ushort _X = attacker.X, _Y = attacker.Y;
                                            ushort _tX = X, _tY = Y;
                                            byte dist = (byte)spell.Distance;
                                            var Array = attacker.Owner.Screen.Objects;
                                            InLineAlgorithm algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist, InLineAlgorithm.Algorithm.DDA);
                                            X = attacker.X;
                                            Y = attacker.Y;
                                            int i = 0;
                                            for (i = 0; i < algo.lcoords.Count; i++)
                                            {
                                                if (attacker.Owner.Map.Floor[algo.lcoords[i].X, algo.lcoords[i].Y, MapObjectType.Player]
                                                    && !attacker.ThroughGate(algo.lcoords[i].X, algo.lcoords[i].Y))
                                                {
                                                    X = (ushort)algo.lcoords[i].X;
                                                    Y = (ushort)algo.lcoords[i].Y;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (!attacker.Owner.Map.Floor[X, Y, MapObjectType.Player, null]) return;
                                            double disth = 1.5;
                                            foreach (Interfaces.IMapObject _obj in Array)
                                            {
                                                bool hit = false;
                                                for (int j = 0; j < i; j++)
                                                    if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                        hit = true;
                                                if (hit)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack) / 2;
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                            damage = (uint)MathHelper.AdjustDataEx((int)damage * 2, (int)spell.Power);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                            suse.AddTarget(attacked, damage, attack);
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (CanAttack(attacker, attackedsob, spell))
                                                        {
                                                            attack.Effect = MsgInteract.InteractEffects.None;
                                                            var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                            suse.Effect = attack.Effect;
                                                            ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                            suse.AddTarget(attackedsob, damage, attack);
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.PX = attacker.X;
                                            attacker.PY = attacker.Y;
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.Owner.Screen.Reload(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #region Kick`s
                                case 12330:
                                case 12340:
                                    {

                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= attacker.AttackRange + 1)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null)
                                                        continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                                                                return;
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);

                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) / 2;
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked.UID, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                PrepareSpell(spell, attacker.Owner);
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                                suse.AddTarget(attackedsob.UID, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }

                                        break;
                                    }
                                #endregion
                                #region Speed Kick
                                case 12120:
                                case 12130:
                                case 12140:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (attacked != null)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage * 2, (int)spell.Power); 
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.Effect = attack.Effect;
                                                        suse.AddTarget(attacked, damage, attack);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                    }
                                                    attacker.PX = attacker.X;
                                                    attacker.PY = attacker.Y;
                                                    attacker.X = X;
                                                    attacker.Y = Y;
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                            }
                                            else
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Distance)
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attackedsob, spell))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Magic(attacker, attackedsob, spell, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                    attacker.PX = attacker.X;
                                                    attacker.PY = attacker.Y;
                                                    attacker.X = X;
                                                    attacker.Y = Y;
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region Cracking swipe
                                case 12170:
                                case 12160:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            if (Time32.Now > attacker.CrackingSwipeStamp.AddMilliseconds(480))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.SpellID = spell.ID;
                                                suse.SpellLevel = spell.Level;
                                                suse.X = X;
                                                suse.Y = Y;
                                                Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance + 1)
                                                {
                                                    foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                    {
                                                        if (_obj == null) continue;
                                                        if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                        {
                                                            attacked = _obj as Player;
                                                            if (sector.IsInFan(attacked.X, attacked.Y))
                                                            {
                                                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                                {
                                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                    damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                    #region Ataque por nobleza
                                                                    #region Rey atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        //Rey vs Rey
                                                                        damage = (uint)(damage * 1.2);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.2);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.2);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }

                                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                                    #endregion
                                                                    #region Duque atacando a otros

                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 0.9);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 0.8);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.3);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.55);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                                    {
                                                                        damage = (uint)(damage * 1.56);
                                                                    }
                                                                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                                    #endregion
                                                                    #region Marquez atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.0);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 1.0);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 0.9);
                                                                    }
                                                                    #endregion
                                                                    #region Earl atacando a otros
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                                    {
                                                                        damage = (uint)(damage * 1.4);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                                    {
                                                                        damage = (uint)(damage * 1.6);
                                                                    }
                                                                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                                    #endregion

                                                                    #endregion
                                                                    suse.Effect = attack.Effect;
                                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                    suse.AddTarget(attacked, damage, attack);
                                                                }
                                                            }
                                                        }
                                                        else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                        {
                                                            attackedsob = _obj as MsgNpcInfoEX;
                                                            if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                            {
                                                                if (CanAttack(attacker, attackedsob, spell))
                                                                {
                                                                    attack.Effect = MsgInteract.InteractEffects.None;
                                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                    suse.Effect = attack.Effect;
                                                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                    suse.AddTarget(attackedsob, damage, attack);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket = null;
                                                }
                                                attacker.Owner.SendScreen(suse, true);
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Dragon Punch
                                case 12240:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            InLineAlgorithm ila = new InLineAlgorithm(attacker.X,
                                        X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            continue;

                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack);
                                                        damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                        suse.Effect = attack.Effect;

                                                        attack.Damage = damage;

                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                        suse.AddTarget(attacked, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;

                                                    if (ila.InLine(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell))
                                                            continue;

                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        attack.Damage = damage;

                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                        suse.AddTarget(attackedsob, damage, attack);
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }

                                #endregion
                                #region DragonFury
                                case 12300:
                                    {
                                        if (attacked != null)
                                        {
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Distance)
                                            {
                                                if (CanUseSpell(spell, attacker.Owner))
                                                {
                                                    MsgMagicEffect suse = new MsgMagicEffect(true);
                                                    suse.Attacker = attacker.UID;
                                                    suse.SpellID = spell.ID;
                                                    suse.SpellLevel = spell.Level;
                                                    suse.X = X;
                                                    suse.Y = Y;
                                                    if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                        suse.Effect = attack.Effect;
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                        suse.AddTarget(attacked, damage, attack);
                                                        attacker.Owner.Player.IsEagleEyeShooted = true;
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                            attacked.Owner.SendScreen(suse, true);
                                                        else attacked.MonsterInfo.SendScreen(suse);
                                                        if (attacked.PlayerFlag == PlayerFlag.Player)
                                                        {
                                                            int potDifference = attacker.BattlePower - attacked.BattlePower;
                                                            int rate = spell.Percent + potDifference - 20;
                                                            if (MyMath.Success(rate))
                                                            {
                                                                attacked.AddFlag3((ulong)PacketFlag.Flags.DragonFury);
                                                                attacked.DragonFuryStamp = Time32.Now;
                                                                attacked.DragonFuryTime = spell.Duration;
                                                                MsgUpdate upgrade = new MsgUpdate(true);
                                                                upgrade.UID = attacked.UID;
                                                                upgrade.Append((byte)PacketFlag.DataType.DragonFury, (uint)spell.Status, (uint)spell.Duration, spell.Power, spell.Level);
                                                                attacker.Owner.Send(upgrade.ToArray());
                                                                if (attacked.PlayerFlag == PlayerFlag.Player)
                                                                    attacked.Owner.Send(upgrade);
                                                                else attacked.MonsterInfo.SendScreen(upgrade);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region DragonSwing
                                case 12200:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.Attacker = attacker.UID;
                                            MsgMagicEffect.SpellID = spell.ID;
                                            MsgMagicEffect.SpellLevel = spell.Level;
                                            MsgMagicEffect.X = X;
                                            MsgMagicEffect.Y = Y;
                                            attacker.Owner.SendScreen(MsgMagicEffect, true);
                                            if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.DragonSwing))
                                            {
                                                attacker.AddFlag3((ulong)PacketFlag.Flags.DragonSwing);
                                                MsgUpdate upgrade = new MsgUpdate(true);
                                                upgrade.UID = attacker.UID;
                                                upgrade.Append((byte)PacketFlag.DataType.DragonSwing, 1, 1000000, 0, 0);
                                                attacker.Owner.Send(upgrade.ToArray());
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region AirStrike EarthSweep Kick
                                case 12210:
                                case 12220:
                                case 12230:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);

                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attacked == null)
                                                return;
                                            var angle = Kernel.GetAngle(attacker.X, attacker.Y, attacked.X, attacked.Y);
                                            Map.UpdateCoordonatesForAngle(ref X, ref Y, angle);
                                            Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, _obj.X, _obj.Y) <= 7)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;

                                                        if (sector.IsInFan(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;

                                                        if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #region Epic~Warrior
                                #region Epic Warrior
                                #region ManiacDance
                                case 12700:
                                    {
                                        if (attacker.Owner.Player.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.Owner.Player.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                            attacker.Owner.Player.AddFlag3((ulong)PacketFlag.Flags.ManiacDance);
                                            attacker.Owner.Player.ManiacDanceStamp = Time32.Now;
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = attacker.X;
                                            suse.Y = attacker.Y;
                                            attack.KOCount = 0;
                                            attacker.KOCount = 0;
                                            attacker.Owner.SendScreen(suse, true);
                                            break;
                                        }
                                        break;
                                    }
                                #endregion
                                #region Backfire
                                case 12680:
                                    {
                                        if (!attacker.EpicWarrior())
                                            return;
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        if (attacker.OnBackFire()) return;
                                        attacked.BackFireTime = 20;
                                        attacker.AddFlag3((ulong)PacketFlag.Flags.BackFire);
                                        attacker.BackFireStamp = Time32.Now;
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                //#region Backfire
                                //case 12680:
                                //    {
                                //        if (!attacker.ContainsFlag3((ulong)PacketFlag.Flags.BackFire))
                                //        {
                                //            attacker.BackFireStamp = Time32.Now;
                                //            attacker.AddFlag3((ulong)PacketFlag.Flags.BackFire);
                                //            PrepareSpell(spell, attacker.Owner);
                                //            MsgMagicEffect suse = new MsgMagicEffect(true);
                                //            suse.Attacker = attacker.UID;
                                //            suse.SpellID = spell.ID;
                                //            suse.SpellLevel = spell.Level;
                                //            suse.X = attacker.X;
                                //            suse.Y = attacker.Y;
                                //            attacker.Owner.SendScreen(suse, true);
                                //        }
                                //        break;
                                //    }
                                //#endregion
                                #region Pounce
                                case 12770:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            //suse.SpellLevelHu = client_Spell.LevelHu2;
                                            suse.SpellLevel = spell.Level;
                                            ushort _X = attacker.X, _Y = attacker.Y;
                                            ushort _tX = X, _tY = Y;
                                            byte dist = (byte)spell.Distance;
                                            var Array = attacker.Owner.Screen.Objects;
                                            InLineAlgorithm algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist,
                                                                               InLineAlgorithm.Algorithm.DDA);
                                            X = attacker.X;
                                            Y = attacker.Y;
                                            int i = 0;
                                            for (i = 0; i < algo.lcoords.Count; i++)
                                            {
                                                if (attacker.Owner.Map.Floor[algo.lcoords[i].X, algo.lcoords[i].Y, MapObjectType.Player]
                                                    && !attacker.ThroughGate(algo.lcoords[i].X, algo.lcoords[i].Y))
                                                {
                                                    X = (ushort)algo.lcoords[i].X;
                                                    Y = (ushort)algo.lcoords[i].Y;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (!attacker.Owner.Map.Floor[X, Y, MapObjectType.Player, null])
                                                return;
                                            if (Kernel.GetDistance(suse.X, suse.Y, X, Y) <= spell.Range)
                                            {
                                                for (int c = 0; c < attacker.Owner.Screen.Objects.Length; c++)
                                                {
                                                    //For a multi threaded application, while we go through the collection
                                                    //the collection might change. We will make sure that we wont go off  
                                                    //the limits with a check.
                                                    if (c >= attacker.Owner.Screen.Objects.Length)
                                                        break;
                                                    Interfaces.IMapObject _obj = attacker.Owner.Screen.Objects[c];
                                                    if (_obj == null)
                                                        continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(suse.X, suse.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Ranged))
                                                            {
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage * 3, (int)spell.Power);
                                                                attacker.Stamina += 50;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                attacked.Stunned = true;
                                                                attacked.StunStamp = Time32.Now;
                                                                suse.AddTarget(attacked, damage, attack);

                                                            }
                                                        }
                                                    }
                                                }
                                                attacker.AttackPacket = null;
                                            }
                                            ushort newx = attacker.X;
                                            ushort newy = attacker.Y;
                                            Map.Pushback(ref newx, ref newy, attacked.Facing, spell.Distance);
                                            attacker.PX = attacker.X;
                                            attacker.PY = attacker.Y;
                                            attacker.X = X;
                                            attacker.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.Owner.Screen.Reload(suse);
                                        }
                                        break;
                                    }
                                #endregion
                                #region WaveOfBlood
                                case 12690:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance + 1)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (sector.IsInFan(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                                                #region Ataque por nobleza
                                                                #region Rey atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    //Rey vs Rey
                                                                    damage = (uint)(damage * 1.1);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.1);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 1.1);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.King && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }

                                                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////KingStage
                                                                #endregion
                                                                #region Duque atacando a otros

                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.0);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Prince && attacked.NobilityRank == NobilityRank.Serf)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Duque,Ataca Conde,Marques
                                                                #endregion
                                                                #region Marquez atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 1.2);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Duke && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 1.1);
                                                                }
                                                                #endregion
                                                                #region Earl atacando a otros
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Prince)
                                                                {
                                                                    damage = (uint)(damage * 1.4);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Duke)
                                                                {
                                                                    damage = (uint)(damage * 1.4);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.King)
                                                                {
                                                                    damage = (uint)(damage * 1.6);
                                                                }
                                                                if (attacked.PlayerFlag == PlayerFlag.Player && attacker.NobilityRank == NobilityRank.Earl && attacked.NobilityRank == NobilityRank.Earl)
                                                                {
                                                                    damage = (uint)(damage * 1.6);
                                                                }
                                                                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////EarlStage
                                                                #endregion

                                                                #endregion

                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (attackedsob == null)
                                                            return;
                                                        if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                attacker.AttackPacket = null;
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;

                                    }
                                #endregion
                                #endregion
                                #endregion
                                #region Windwalker
                                #region Stomper
                                #region ChillingSnow(Active)
                                case 12960:
                                    {
                                        if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.ChillingSnow))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.ChillingSnow);
                                            attacker.AttackPacket = null;
                                            break;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.HealingSnow);
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.FreezingPelter);
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.AddTarget(attacker.UID, 0, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.ChillingSnow);
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region HealingSnow(Active)
                                case 12950:
                                    {
                                        if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.HealingSnow))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.HealingSnow);
                                            attacker.AttackPacket = null;
                                            break;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.ChillingSnow);
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.FreezingPelter);
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.AddTarget(attacker.UID, 0, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.HealingSnow);
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region FreezingPelter(Active)
                                case 13020:
                                    {
                                        if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.FreezingPelter))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.FreezingPelter);
                                            attacker.Owner.LoadItemStats();
                                            attacker.AttackPacket = null;
                                            break;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.ChillingSnow);
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.HealingSnow);
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.AddTarget(attacker.UID, 0, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.FreezingPelter);
                                            attacker.Owner.LoadItemStats();
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region RevengeTail(Active)
                                case 13030:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            suse.AddTarget(attacker.UID, 0, attack);
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.RevengeTaill);
                                            attacker.RevengeTailStamp = Time32.Now;
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region BurntFrost(Passive)
                                case 12940:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);


                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Fan sector = new Fan(attacker.X, attacker.Y, X, Y, spell.Range, spell.Sector);
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Distance + 1)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        if (_obj == null)
                                                            continue;
                                                        attacked = _obj as Player;
                                                        if (attacked == null) continue;
                                                        if (sector.IsInFan(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked.UID, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        if (_obj == null)
                                                            continue;
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (attackedsob == null) continue;
                                                        if (sector.IsInFan(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.AddTarget(attackedsob.UID, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region RageofWar(Passive)
                                /* case 12930:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            InLineAlgorithm ila = new InLineAlgorithm(attacker.X,X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            bool first = false;
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            continue;
                                                        if (Kernel.GetDistance(attacked.X, attacked.Y, attacker.X, attacker.Y) > 11) continue;
                                                        if (!first)
                                                        {
                                                            var map = Kernel.Maps[attacker.MapID];
                                                            Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                                                            flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                            while (map.Npcs.ContainsKey(flooritem.UID))
                                                                flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                                            flooritem.ItemID = MsgMapItem.RageOfWarTrap;
                                                            flooritem.X = attacked.X;
                                                            flooritem.MapID = map.ID;
                                                            flooritem.Y = attacked.Y;
                                                            flooritem.Type = MsgMapItem.Effect;
                                                            flooritem.mColor = 15;
                                                            flooritem.OwnerUID = attacker.UID;
                                                            flooritem.OnFloor = Time32.Now;
                                                            flooritem.Owner = attacker.Owner;
                                                            flooritem.Name = "RageofWarTrap";
                                                            map.AddFloorItem(flooritem);
                                                            attacker.Owner.SendScreenSpawn(flooritem, true);
                                                            first = true;
                                                        }
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack) * 100 / 130;

                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                        suse.AddTarget(attacked.UID, damage, attack);
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.Monster)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            continue;
                                                        if (Kernel.GetDistance(attacked.X, attacked.Y, attacker.X, attacker.Y) > 11) continue;
                                                        if (!first)
                                                        {
                                                            var map = Kernel.Maps[attacker.MapID];
                                                            Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                                                            flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                            while (map.Npcs.ContainsKey(flooritem.UID))
                                                                flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                                            flooritem.ItemID = MsgMapItem.RageOfWarTrap;
                                                            flooritem.X = attacked.X;
                                                            flooritem.MapID = map.ID;
                                                            flooritem.Y = attacked.Y;
                                                            flooritem.Type = MsgMapItem.Effect;
                                                            flooritem.mColor = 15;
                                                            flooritem.OwnerUID = attacker.UID;
                                                            flooritem.OnFloor = Time32.Now;
                                                            flooritem.Owner = attacker.Owner;
                                                            flooritem.Name = "RageofWarTrap";
                                                            map.AddFloorItem(flooritem);
                                                            attacker.Owner.SendScreenSpawn(flooritem, true);
                                                            first = true;
                                                        }
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) * (spell.Power - 1000) / 990;

                                                        damage = (uint)(damage * 1.2);

                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                        suse.AddTarget(attacked.UID, damage, attack);
                                                    }


                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (attackedsob == null) continue;
                                                    if (ila.InLine(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell))
                                                            continue;
                                                        if (Kernel.GetDistance(attackedsob.X, attackedsob.Y, attacker.X, attacker.Y) > 11) continue;
                                                        if (!first)
                                                        {
                                                            var map = Kernel.Maps[attacker.MapID];
                                                            Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                                                            flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                            while (map.Npcs.ContainsKey(flooritem.UID))
                                                                flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                                            flooritem.ItemID = MsgMapItem.RageOfWarTrap;
                                                            flooritem.X = attackedsob.X;
                                                            flooritem.MapID = map.ID;
                                                            flooritem.Y = attackedsob.Y;
                                                            flooritem.Type = MsgMapItem.Effect;
                                                            flooritem.mColor = 15;
                                                            flooritem.OwnerUID = attacker.UID;
                                                            flooritem.OnFloor = Time32.Now;
                                                            flooritem.Owner = attacker.Owner;
                                                            flooritem.Name = "RageofWarTrap";
                                                            map.AddFloorItem(flooritem);
                                                            attacker.Owner.SendScreenSpawn(flooritem, true);
                                                            first = true;
                                                        }
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack) * (spell.Power - 5000) / 8000;

                                                        damage = (uint)(damage * 1.2);

                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                        suse.AddTarget(attackedsob.UID, damage, attack);
                                                    }


                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    } */
                                #endregion
                                #region RageOfWar[Stomper]
                                case 12930:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            InLineAlgorithm ila = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, (byte)spell.Range, InLineAlgorithm.Algorithm.DDA);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = SpellID;
                                            suse.SpellLevel = attacker.Owner.Spells[SpellID].Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            ushort Xo1 = 0, Yo1 = 0;
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null) continue;
                                                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;
                                                    if (ila.InLine(attacked.X, attacked.Y))
                                                    {
                                                        if (!CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack) * 100 / 130;
                                                        suse.Effect = attack.Effect;
                                                        attack.Damage = damage;
                                                        Xo1 = attacked.X;
                                                        Yo1 = attacked.Y;
                                                        suse.AddTarget(attacked.UID, damage, attack);
                                                        ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    attackedsob = _obj as MsgNpcInfoEX;
                                                    if (ila.InLine(attackedsob.X, attackedsob.Y))
                                                    {
                                                        if (!CanAttack(attacker, attackedsob, spell)) continue;
                                                        attack.Effect = MsgInteract.InteractEffects.None;
                                                        uint damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                        //damage = (damage * spell.IncreaseDMG);
                                                        //damage = (damage / spell.DecreaseDMG);
                                                        suse.AddTarget(attackedsob.UID, damage, attack);
                                                        ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                                    }
                                                }
                                            }
                                            if (Xo1 != 0 && Yo1 != 0)
                                            {
                                                var map = Kernel.Maps[attacker.MapID];
                                                Network.GamePackets.MsgMapItem flooritem = new Network.GamePackets.MsgMapItem(true);
                                                flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                                                while (map.Npcs.ContainsKey(flooritem.UID))
                                                    flooritem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;

                                                flooritem.ItemID = MsgMapItem.RageOfWarTrap;
                                                flooritem.X = Xo1;
                                                flooritem.MapID = map.ID;
                                                flooritem.Y = Yo1;
                                                flooritem.Type = MsgMapItem.Effect;
                                                flooritem.mColor = 15;
                                                flooritem.OwnerUID = attacker.UID;
                                                flooritem.OnFloor = Time32.Now;
                                                flooritem.Owner = attacker.Owner;
                                                flooritem.Name = "RageofWarTrap";
                                                map.AddFloorItem(flooritem);
                                                attacker.Owner.SendScreenSpawn(flooritem, true);
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        break;

                                    }

                                #endregion
                                #endregion
                                #region Chaser
                                #region ShadowofChaser(Active)
                                case 13090:
                                    {
                                        if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.ShadowofChaser))
                                        {
                                            attacker.RemoveFlag4((ulong)PacketFlag.Flags.ShadowofChaser);
                                            attacker.AttackPacket = null;
                                            break;
                                        }
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.AddTarget(attacker.UID, 0, null);
                                            attack = new MsgInteract(true);
                                            attack.Attacker = attack.Attacked = attacker.UID;
                                            attack.X = attacker.X; attack.Y = attacker.Y;
                                            attack.InteractType = 24;
                                            attack.SpellID = spell.ID;
                                            attacker.Owner.SendScreen(suse, true);
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.ShadowofChaser);
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region TripleBlasts(Passive)
                                case 12850:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.Attacker1 = attackedsob == null ? attacked.UID : attackedsob.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            if (attackedsob == null)
                                            {
                                                if (CanAttack(attacker, attacked, spell, false))
                                                {
                                                    if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.ShadowofChaser) && attacker.IsChaser())
                                                    {
                                                        var spell2 = SpellTable.GetSpell(13090, attacker.Owner);
                                                        if (spell2 != null)
                                                        {
                                                            spell2.CanKill = true;
                                                            if (Kernel.Rate(spell2.Percent))
                                                            {
                                                                ShadowofChaser(attacker, attacked, attack, 1);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack) / 8;

                                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                    suse.AddTarget(attacked.UID, damage, attack);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }
                                            else
                                            {
                                                if (CanAttack(attacker, attackedsob, spell))
                                                {

                                                    uint damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);

                                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                    suse.AddTarget(attackedsob.UID, damage, attack);
                                                    attacker.Owner.SendScreen(suse, true);
                                                }
                                            }

                                        }
                                        break;
                                    }
                                #endregion
                                #region SwirlingStorm(Passive)
                                case 12890:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            Sector sector = new Sector(attacker.X, attacker.Y, X, Y);
                                            sector.Arrange(spell.Sector, spell.Distance);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                byte dist = (byte)spell.Distance;
                                                InLineAlgorithm algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist, InLineAlgorithm.Algorithm.DDA);
                                                int i = algo.lcoords.Count;
                                                double disth = 3.0;
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Player)
                                                {
                                                    bool hit = false;
                                                    for (int j = 0; j < i; j++)
                                                        if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                            hit = true;
                                                    if (hit)
                                                    {
                                                        attacked = _obj as Player;

                                                        if (sector.Inside(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, ref attack);
                                                                damage = (uint)MathHelper.AdjustDataEx((int)damage / 2, (int)spell.Power / 2);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.Monster)
                                                {
                                                    bool hit = false;
                                                    for (int j = 0; j < i; j++)
                                                        if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                            hit = true;
                                                    if (hit)
                                                    {
                                                        attacked = _obj as Player;

                                                        if (sector.Inside(attacked.X, attacked.Y))
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                                                damage = (uint)(damage / 3);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                {
                                                    bool hit = false;
                                                    for (int j = 0; j < i; j++)
                                                        if (Kernel.GetDDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= disth)
                                                            hit = true;
                                                    if (hit)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (attackedsob == null) continue;
                                                        if (sector.Inside(attackedsob.X, attackedsob.Y))
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);
                                                                ReceiveAttack(attacker, attackedsob, attack, damage, spell);
                                                                suse.Effect = attack.Effect;
                                                                suse.AddTarget(attackedsob, damage, attack);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            attacker.Owner.SendScreen(suse, true);
                                        }
                                        else
                                        {
                                            attacker.AttackPacket = null;
                                        }
                                        break;
                                    }
                                #endregion
                                #region ThunderCloud(Passive)
                                case 12840:
                                    {
                                        if (Time32.Now >= attacker.ThunderCloudStamp.AddSeconds(30))
                                        {
                                            attacker.ThunderCloudStamp = Time32.Now;
                                            if (CanUseSpell(spell, attacker.Owner))
                                            {
                                                PrepareSpell(spell, attacker.Owner);
                                                MsgMagicEffect suse = new MsgMagicEffect(true);
                                                suse.Attacker = attacker.UID;
                                                suse.X = X;
                                                suse.Y = Y;
                                                suse.SpellID = spell.ID;
                                                attacker.Owner.SendScreen(suse, true);
                                                //////////////////////////////////////////////////
                                                ///////////////////////////////////////////////////
                                                uint UID = Kernel.Maps[attacker.MapID].EntityUIDCounter.Next;
                                                uint Mesh = 980;
                                                //////////////////////////////////////////////////////
                                                byte[] Buffer = new byte[68 + 8];
                                                Writer.WriteInt32(Buffer.Length - 8, 0, Buffer);
                                                Writer.Write(2035, 2, Buffer);//Packet ID
                                                Writer.Write(UID, 4, Buffer);//FlowerUID
                                                Writer.Write(4264, 8, Buffer);
                                                Buffer[12] = 3;
                                                Writer.Write(Mesh, 16, Buffer);//FloorItemID
                                                Buffer[24] = 14;//AttackRange
                                                Writer.Write(X, 26, Buffer);
                                                Writer.Write(Y, 28, Buffer);
                                                Writer.Write("Thundercloud", 30, Buffer);
                                                //////////////////////////////////////////////////
                                                ///////////////////////////////////////////////////
                                                //////////////////////////////////////////////////////
                                                Player ThunderCloud = new Player(PlayerFlag.Monster, true);
                                                ThunderCloud.Name = "Thundercloud";
                                                ThunderCloud.Mesh = Mesh;
                                                ThunderCloud.UID = UID;
                                                ThunderCloud.GuildID = attacker.GuildID;
                                                ThunderCloud.MaxHitpoints = attacker.MaxHitpoints;
                                                ThunderCloud.Level = 140;
                                                ThunderCloud.X = X;
                                                ThunderCloud.Y = Y;
                                                ThunderCloud.Facing = attacker.Facing;
                                                ThunderCloud.Boss = 1;
                                                ThunderCloud.MapID = attacker.MapID;
                                                Writer.Write(3, 308, ThunderCloud.SpawnPacket);
                                                Writer.Write(15, 272, ThunderCloud.SpawnPacket);
                                                Writer.Write(3, 271, ThunderCloud.SpawnPacket);//AttackUser
                                                //Writer.Write(attacker.Owner.UnionID, 278, ThunderCloud.SpawnPacket);//UnionID
                                                ThunderCloud.OwnerUID = attacker.UID;
                                                ThunderCloud.Owner = new Client.GameState(null);
                                                ThunderCloud.Owner.Player = ThunderCloud;
                                                ThunderCloud.MonsterInfo = new MonsterInformation();
                                                ThunderCloud.MonsterInfo.InteractType = 24;
                                                ThunderCloud.MonsterInfo.AttackSpeed = 1000;
                                                ThunderCloud.MonsterInfo.AttackRange = 14;
                                                ThunderCloud.MonsterInfo.Boss = true;
                                                ThunderCloud.MonsterInfo.BoundX = X;
                                                ThunderCloud.Companion = true;
                                                ThunderCloud.MonsterInfo.Guard = false;
                                                ThunderCloud.MonsterInfo.BoundY = Y;
                                                ThunderCloud.MonsterInfo.Defence = attacker.Defence;
                                                ThunderCloud.MonsterInfo.Hitpoints = 10000;
                                                ThunderCloud.MonsterInfo.Mesh = 980;
                                                ThunderCloud.MonsterInfo.MoveSpeed = int.MaxValue;
                                                ThunderCloud.MonsterInfo.Name = "Thundercloud";
                                                ThunderCloud.MonsterInfo.Owner = attacker;
                                                ThunderCloud.MonsterInfo.ViewRange = 14;
                                                ThunderCloud.MonsterInfo.RespawnTime = 0;
                                                ThunderCloud.MonsterInfo.RunSpeed = int.MaxValue;
                                                ThunderCloud.MonsterInfo.SpellID = 13190;
                                                ThunderCloud.Hitpoints = 10000;
                                                Kernel.Maps[ThunderCloud.MapID].AddEntity(ThunderCloud);
                                                //////////////////////////////////////////////////////////////
                                                //////////////////////////////////////////////////////////
                                                //////////////////////////////////////////////////////////
                                                /////////////////////////////////////////////
                                                Network.GamePackets.MsgAction d = new MsgAction(true);
                                                d.UID = UID;
                                                d.ID = PacketMsgAction.Mode.SpawnEffect;
                                                d.Facing = attacker.Facing;
                                                d.X2 = ThunderCloud.X;
                                                d.Y2 = ThunderCloud.Y;
                                                ///////////////////////////////////////////////
                                                ////////////////////////////////////////////
                                                /////////////////////////////////////////
                                                attacker.Owner.SendScreen(Buffer, true);
                                                attacker.Owner.SendScreen(ThunderCloud.SpawnPacket, true);
                                                attacker.Owner.SendScreenSpawn(ThunderCloud, true);
                                                attacker.Owner.SendScreen(d, true);
                                                ThunderCloud.ThunderCloudStamp = Time32.Now;
                                                Server.Thread.Register(ThunderCloud);
                                                attacker.AttackPacket = null;
                                            }
                                        }
                                        break;

                                    }
                                #endregion
                                #region ThunderBolt(Active)
                                case 12970:
                                    {
                                        if (!attacker.Owner.Spells.ContainsKey(12840)) break;
                                        foreach (var th in Kernel.Maps[attacker.MapID].Entities.Values.Where(i => i.Name == "Thundercloud"))
                                        {
                                            if (th.OwnerUID == attacker.UID)
                                            {
                                                if (Kernel.GetDistance(attacker.X, attacker.Y, th.X, th.Y) <= th.MonsterInfo.AttackRange)
                                                {
                                                    if (CanUseSpell(spell, attacker.Owner))
                                                    {
                                                        PrepareSpell(spell, attacker.Owner);
                                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                                        suse.Attacker = attacker.UID;
                                                        suse.X = X;
                                                        suse.Y = Y;
                                                        suse.SpellID = spell.ID;
                                                        suse.AddTarget(th.UID, 0, null);
                                                        Writer.Write(128, 50, th.SpawnPacket);//Flag4(128)
                                                        attacker.Owner.SendScreen(suse, true);
                                                        attacker.AttackPacket = null;
                                                        foreach (var client in Kernel.GamePool.Values.Where(i => Kernel.GetDistance(th.X, th.Y, i.Player.X, i.Player.Y) < 17))
                                                        {
                                                            client.Send(th.SpawnPacket);
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #region Omnipotence(XP)
                                case 12860:
                                    {
                                        MsgMagicEffect suse = new MsgMagicEffect(true);
                                        suse.Attacker = attacker.UID;
                                        suse.SpellID = spell.ID;
                                        suse.SpellLevel = spell.Level;
                                        suse.X = X;
                                        suse.Y = Y;
                                        if (attacker.ContainsFlag((ulong)PacketFlag.Flags.XPList))
                                        {
                                            attacker.AddFlag4((ulong)PacketFlag.Flags.Omnipotence);
                                            attacker.OmnipotenceStamp = Time32.Now;
                                            attacker.RemoveFlag((ulong)PacketFlag.Flags.XPList);
                                        }
                                        else
                                        {
                                            if (Kernel.GetDistance(attacker.X, attacker.Y, X, Y) <= spell.Range)
                                            {
                                                foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                                {
                                                    if (_obj == null) continue;
                                                    if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                                                    {
                                                        attacked = _obj as Player;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Ranged(attacker, attacked, ref attack);
                                                                damage = (uint)(damage * 0.5);
                                                                suse.Effect = attack.Effect;
                                                                ReceiveAttack(attacker, attacked, attack, ref damage, spell);
                                                                suse.AddTarget(attacked.UID, damage, attack);
                                                            }
                                                        }
                                                    }
                                                    else if (_obj.MapObjType == MapObjectType.SobNpc)
                                                    {
                                                        attackedsob = _obj as MsgNpcInfoEX;
                                                        if (Kernel.GetDistance(attacker.X, attacker.Y, attackedsob.X, attackedsob.Y) <= spell.Range)
                                                        {
                                                            if (CanAttack(attacker, attackedsob, spell))
                                                            {
                                                                attack.Effect = MsgInteract.InteractEffects.None;
                                                                uint damage = Game.Attacking.Calculate.Ranged(attacker, attackedsob, ref attack);
                                                                damage = (uint)(damage * 0.5);
                                                                suse.Effect = attack.Effect;
                                                                suse.AddTarget(attackedsob.UID, damage, attack);
                                                                attacker.Owner.SendScreen(suse, true);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        attacker.Owner.SendScreen(suse, true);
                                        break;
                                    }
                                #endregion
                                #region FrostGaze(I-II-III)
                                case 12830:
                                case 13070:
                                case 13080:
                                    {
                                        if (CanUseSpell(spell, attacker.Owner))
                                        {
                                            PrepareSpell(spell, attacker.Owner);
                                            MsgMagicEffect suse = new MsgMagicEffect(true);
                                            suse.Attacker = attacker.UID;
                                            suse.SpellID = spell.ID;
                                            suse.SpellLevel = spell.Level;
                                            suse.X = X;
                                            suse.Y = Y;
                                            attacker.Owner.SendScreen(suse, true);
                                            foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                                            {
                                                if (_obj == null)
                                                    continue;
                                                if (_obj.MapObjType == MapObjectType.Player)
                                                {
                                                    attacked = _obj as Player;

                                                    if (Kernel.GetDistance(attacker.X, attacker.Y, attacked.X, attacked.Y) <= spell.Range)
                                                    {
                                                        if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                                        {
                                                            int Rate = 100;
                                                            int diff = attacked.BattlePower - attacker.BattlePower;
                                                            if (diff < 0) diff = 0;
                                                            Rate -= (byte)(diff * 5);
                                                            if (Rate < 0) Rate = 0;
                                                            if (Kernel.Rate(Rate))
                                                            {
                                                                if (attacked.Stamina >= (byte)spell.Power)
                                                                    attacked.Stamina -= (byte)spell.Power;
                                                                else attacked.Stamina = 0;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                #endregion
                                #endregion
                                #endregion
                                default:
                                    {
                                        if (attacker.Owner.Account.State == AccountTable.AccountState.Administrator)
                                            attacker.Owner.Send(new MsgTalk("Unknown Skill : " + spellID, System.Drawing.Color.CadetBlue, (uint)PacketMsgTalk.MsgTalkType.Talk));
                                        break;
                                    }
                            }
                                attacker.Owner.IncreaseSpellExperience(Experience, spellID);
                                if (attacker.MapID == 1039)
                                {
                                    if (spell.ID == 7001 || spell.ID == 9876)
                                    {
                                        attacker.AttackPacket = null; return;
                                    }
                                    if (attacker.AttackPacket != null)
                                    {
                                        attack.Damage = spell.ID;
                                        attacker.AttackPacket = attack;
                                        if (DB.SpellTable.WeaponSpells.ContainsKey(spell.ID))
                                        {
                                            if (attacker.AttackPacket == null)
                                            {
                                                attack.InteractType = MsgInteract.Melee;
                                                attacker.AttackPacket = attack;
                                            }
                                            else
                                            {
                                                attacker.AttackPacket.InteractType = MsgInteract.Melee;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (spell.NextSpellID != 0)
                                    {

                                        attack.Damage = spell.NextSpellID;
                                        attacker.AttackPacket = attack;
                                    }
                                    else
                                    {
                                        if (spell.ID == 12080)
                                        {
                                            ISkill gap;
                                            if (attacker.Owner.Spells.TryGetValue(11230, out gap))
                                            {
                                                spell = DB.SpellTable.SpellInformations[gap.ID][gap.Level];
                                                if (MyMath.Success(spell.Percent * 2))
                                                {
                                                    attack.InteractType = MsgInteract.Magic;
                                                    attack.Decoded = true;
                                                    attack.X = attacker.X;
                                                    attack.Y = attacker.Y;
                                                    attack.Attacker = attacker.UID;

                                                    if (attackedsob != null)
                                                        attack.Attacked = attackedsob.UID;
                                                    else
                                                        attack.Attacked = attacked.UID;
                                                    attack.Damage = spell.ID;
                                                    goto restart;
                                                }
                                            }
                                            attacker.AttackPacket = null;
                                        }
                                        else
                                        {
                                            if (!DB.SpellTable.WeaponSpells.ContainsKey(spell.ID) && spell.ID != 11230 || spell.ID == 9876 && spell.ID != 11230)
                                                attacker.AttackPacket = null;
                                            else
                                            {
                                                if (attacker.AttackPacket == null)
                                                {
                                                    attack.InteractType = MsgInteract.Melee;
                                                    attacker.AttackPacket = attack;
                                                }
                                                else
                                                {
                                                    attacker.AttackPacket.InteractType = MsgInteract.Melee;
                                                }
                                            }
                                        }
                                    }
                                }
                        }
                        else
                        {
                            attacker.AttackPacket = null;
                        }
                    }
                    #endregion
                }
                #endregion
            }
            #endregion
        }
        public static bool isArcherSkill(uint ID)
        {
            if (ID >= 8000 && ID <= 9875)
                return true;
            return false;
        }
        public static void QuitSteedRace(Player attacker)
        {
            attacker.Owner.MessageBox("Do you want to quit the steed race?", (pClient) =>
            {
                pClient.Player.Teleport(1002, 301, 279);
                pClient.Player.RemoveFlag((ulong)PacketFlag.Flags.Ride);
            });
        }
        public Handle(MsgInteract attack, Player attacker, Player attacked)
        {
            this.attack = attack;
            this.attacker = attacker;
            this.attacked = attacked;
            this.Execute();
        }
        public static List<ushort> GetWeaponSpell(SpellInformation spell)
        {
            return SpellTable.WeaponSpells.Values.Where(p => p.Contains(spell.ID)).FirstOrDefault();
        }
        public static void FlameLotus(MsgMapItem item, int count = 0)
        {
            var client = item.Owner;
            if (!client.Spells.ContainsKey(12380)) return;
            var spell = SpellTable.GetSpell(client.Spells[12380].ID, client.Spells[12380].Level);
            if (count == 0)
            {
                switch (spell.Level)
                {
                    case 0: count = 5; break;
                    case 1: count = 8; break;
                    case 2: count = 11; break;
                    case 3: count = 14; break;
                    case 4: count = 17; break;
                    case 5: count = 20; break;
                    case 6: count = 25; break;
                }
            }
            var targets = PlayerinRange(item, spell.Range).ToArray();
            targets = targets.Where(p => CanAttack(client.Player, p.Player, null, true)).ToArray();
            targets = targets.Take(count).ToArray();
            var attack = new MsgInteract(true);
            attack.Attacker = item.Owner.Player.UID;
            attack.InteractType = MsgInteract.Melee;
            foreach (var target in targets)
            {
                uint damage = Calculate.Magic(client.Player, target.Player, spell, ref attack);
                if (client.Spells.ContainsKey(1002))
                {
                    var spell2 = SpellTable.GetSpell(client.Spells[1002].ID, client.Spells[1002].Level);
                    damage = Game.Attacking.Calculate.MagicPerfectos(client.Player, target.Player, spell2, ref attack);
                }
                attack.Damage = damage;
                attack.Attacked = target.Player.UID;
                attack.X = target.Player.X;
                attack.Y = target.Player.Y;
                ReceiveAttack(client.Player, target.Player, attack, ref damage, spell);
                client.Player.AttackPacket = null;
            }
        }
        public static void AuroraLotus(MsgMapItem item, int count = 0)
        {
            var client = item.Owner;
            if (!client.Spells.ContainsKey(12370)) return;
            var spell = SpellTable.GetSpell(client.Spells[12370].ID, client.Spells[12370].Level);
            if (count == 0)
            {
                switch (spell.Level)
                {
                    case 0: count = 5; break;
                    case 1: count = 8; break;
                    case 2: count = 11; break;
                    case 3: count = 14; break;
                    case 4: count = 17; break;
                    case 5: count = 20; break;
                    case 6: count = 25; break;
                }
            }
            var deads = PlayerinRange(item, spell.Range).Where(p => p.Player.Dead).ToArray();
            if (client.Team != null)
                deads = deads.Where(p => client.Team.Contain(p.Player.UID)).ToArray();
            else if (client.Guild != null)
                if (client.Guild.Members != null && client.Guild.Ally != null)
                    deads = deads.Where(p => client.Guild.Members.ContainsKey(p.Player.UID) || client.Guild.Ally.ContainsKey(p.Player.GuildID)).ToArray();
                else
                    deads = deads.Where(p => client.Guild.ID == p.Player.GuildID).ToArray();
            deads = deads.Take(count).ToArray();
            if (deads != null)
            {
                foreach (var player in deads)
                {
                    player.Player.Action = Enums.ConquerAction.None;
                    player.ReviveStamp = Time32.Now;
                    player.Attackable = false;
                    player.Player.TransformationID = 0;
                    player.Player.RemoveFlag((ulong)PacketFlag.Flags.Dead);
                    player.Player.RemoveFlag((ulong)PacketFlag.Flags.Ghost);
                    player.Player.Hitpoints = player.Player.MaxHitpoints;
                    player.Player.Ressurect();
                    player.BlessTouch(client);
                }
            }
        }
        public static void InfroEcho(MsgMapItem item, int count = 0)
        {
            var client = item.Owner;
            var X = item.X;
            var Y = item.Y;
            if (!client.Spells.ContainsKey(12550))
                return;
            var spell = SpellTable.GetSpell(client.Spells[12550].ID, client.Spells[12550].Level);

            var attack = new MsgInteract(true);
            attack.Attacker = client.Player.UID;
            attack.X = X;
            attack.Y = Y;
            attack.Damage = spell.ID;
            attack.InteractType = MsgInteract.Magic;


            MsgMagicEffect suse = new MsgMagicEffect(true);
            suse.Attacker = client.Player.UID;
            suse.SpellID = spell.ID;
            suse.SpellLevel = spell.Level;
            suse.X = X;
            suse.Y = Y;
            suse.SpecialEffect = 1;
            foreach (var c in client.Screen.Objects)
            {

                Interfaces.IMapObject _obj = c as Player;
                if (_obj == null)
                    continue;
                if (_obj.MapObjType == MapObjectType.Monster || _obj.MapObjType == MapObjectType.Player)
                {
                    var attacked = _obj as Player;
                    if (Kernel.GetDistance(X, Y, attacked.X, attacked.Y) <= spell.Range)
                    {
                        if (CanAttack(client.Player, attacked, spell, attack.InteractType == MsgInteract.Ranged))
                        {
                            uint damage = Game.Attacking.Calculate.Melee(client.Player, attacked, ref attack) / 2;
                            damage = (uint)(damage * 1.0);
                            suse.Effect = attack.Effect;
                            ReceiveAttack(client.Player, attacked, attack, ref damage, spell);
                            attacked.Stunned = true;
                            attacked.StunStamp = Time32.Now;
                            suse.AddTarget(attacked, damage, attack);

                        }
                    }
                }
            }
            client.Player.AttackPacket = null;
            client.SendScreen(suse, true);
        }
        public static void HandleAuraMonk(Player Attacker, SpellInformation Spell)
        {
            ulong StatusFlag = 0, StatusFlag2 = 0;
            Enums.AuraType Aura = Enums.AuraType.TyrantAura;
            switch (Spell.ID)
            {
                case 10424: StatusFlag = (ulong)PacketFlag.Flags.EarthAura; break;
                case 10423: StatusFlag = (ulong)PacketFlag.Flags.FireAura; break;
                case 10422: StatusFlag = (ulong)PacketFlag.Flags.WaterAura; break;
                case 10421: StatusFlag = (ulong)PacketFlag.Flags.WoodAura; break;
                case 10420: StatusFlag = (ulong)PacketFlag.Flags.MetalAura; break;
                case 10410: StatusFlag = (ulong)PacketFlag.Flags.FendAura; break;
                case 10395: StatusFlag = (ulong)PacketFlag.Flags.TyrantAura; break;
            }
            switch (Spell.ID)
            {
                case 10424: StatusFlag2 = (ulong)PacketFlag.Flags.EarthAuraIcon; break;
                case 10423: StatusFlag2 = (ulong)PacketFlag.Flags.FireAuraIcon; break;
                case 10422: StatusFlag2 = (ulong)PacketFlag.Flags.WaterAuraIcon; break;
                case 10421: StatusFlag2 = (ulong)PacketFlag.Flags.WoodAuraIcon; break;
                case 10420: StatusFlag2 = (ulong)PacketFlag.Flags.MetalAuraIcon; break;
                case 10410: StatusFlag2 = (ulong)PacketFlag.Flags.FendAuraIcon; break;
                case 10395: StatusFlag2 = (ulong)PacketFlag.Flags.TyrantAuraIcon; break;
            }
            if (Attacker.Dead) return;
            if (Attacker.Aura_isActive)
            {
                switch (Attacker.Aura_actType)
                {
                    case 10424: Aura = Enums.AuraType.EarthAura; break;
                    case 10423: Aura = Enums.AuraType.FireAura; break;
                    case 10422: Aura = Enums.AuraType.WaterAura; break;
                    case 10421: Aura = Enums.AuraType.WoodAura; break;
                    case 10420: Aura = Enums.AuraType.MetalAura; break;
                    case 10410: Aura = Enums.AuraType.FendAura; break;
                    case 10395: Aura = Enums.AuraType.TyrantAura; break;
                }
                new MsgUpdate(true).Aura(Attacker, Enums.AuraDataTypes.Remove, Aura, Spell);
                Attacker.RemoveFlag2(Attacker.Aura_actType);
                Attacker.RemoveFlag2(Attacker.Aura_actType2);
                Attacker.Owner.removeAuraBonuses(Attacker.Aura_actType, Attacker.Aura_actPower, 1);
                Attacker.Aura_isActive = false;
                if (StatusFlag == Attacker.Aura_actType)
                {
                    Attacker.Aura_actType = 0;
                    Attacker.Aura_actType2 = 0;
                    Attacker.Aura_actPower = 0;
                    Attacker.Aura_actLevel = 0;
                    return;
                }
            }
            if (CanUseSpell(Spell, Attacker.Owner))
            {
                if (StatusFlag != 0)
                {
                    switch (Attacker.Aura_actType)
                    {
                        case 10424: Aura = Enums.AuraType.EarthAura; break;
                        case 10423: Aura = Enums.AuraType.FireAura; break;
                        case 10422: Aura = Enums.AuraType.WaterAura; break;
                        case 10421: Aura = Enums.AuraType.WoodAura; break;
                        case 10420: Aura = Enums.AuraType.MetalAura; break;
                        case 10410: Aura = Enums.AuraType.FendAura; break;
                        case 10395: Aura = Enums.AuraType.TyrantAura; break;
                    }
                    new MsgUpdate(true).Aura(Attacker, Enums.AuraDataTypes.Remove, Aura, Spell);
                    Attacker.RemoveFlag2(Attacker.Aura_actType);
                    Attacker.RemoveFlag2(Attacker.Aura_actType2);
                    Attacker.Owner.removeAuraBonuses(Attacker.Aura_actType, Attacker.Aura_actPower, 1);
                    Attacker.Aura_isActive = false;
                    if (StatusFlag == Attacker.Aura_actType)
                    {
                        Attacker.Aura_actType2 = 0;
                        Attacker.Aura_actType = 0;
                        Attacker.Aura_actPower = 0;
                        Attacker.Aura_actLevel = 0;
                    }
                }
                if (Spell.Power == 0)
                    Spell.Power = 45;
                PrepareSpell(Spell, Attacker.Owner);
                MsgMagicEffect suse = new MsgMagicEffect(true);
                suse.Attacker = Attacker.UID;
                suse.SpellID = Spell.ID;
                suse.SpellLevel = Spell.Level;
                suse.X = Attacker.X;
                suse.Y = Attacker.Y;
                suse.AddTarget(Attacker, 0, null);
                Attacker.Owner.SendScreen(suse, true);
                Attacker.AddFlag2(StatusFlag);
                Attacker.AddFlag2(StatusFlag2);
                Attacker.Aura_isActive = true;
                Attacker.Aura_actType = StatusFlag;
                Attacker.Aura_actType2 = StatusFlag2;
                Attacker.Aura_actPower = Spell.Power;
                Attacker.Aura_actLevel = Spell.Level;
                Attacker.Owner.doAuraBonuses(StatusFlag, Spell.Power, 1);
                switch (Spell.ID)
                {
                    case 10424: Aura = Enums.AuraType.EarthAura; break;
                    case 10423: Aura = Enums.AuraType.FireAura; break;
                    case 10422: Aura = Enums.AuraType.WaterAura; break;
                    case 10421: Aura = Enums.AuraType.WoodAura; break;
                    case 10420: Aura = Enums.AuraType.MetalAura; break;
                    case 10410: Aura = Enums.AuraType.FendAura; break;
                    case 10395: Aura = Enums.AuraType.TyrantAura; break;
                }
                new MsgUpdate(true).Aura(Attacker, Enums.AuraDataTypes.Add, Aura, Spell);
            }
        }
        private void TwilightAction(Player attacker, MsgMagicEffect suse, SpellInformation spell, ushort X, ushort Y)
        {
            byte dist = (byte)spell.Distance;
            var map = attacker.Owner.Map;

            var algo = new InLineAlgorithm(attacker.X, X, attacker.Y, Y, dist);

            var count = (double)algo.lcoords.Count / 3;
            int i = 1;
            var myx = attacker.X;
            var myy = attacker.Y;
            Server.Thread.DelayedTask.StartDelayedTask(() =>
            {
                var selected = (i * (int)count) - 2;
                selected = Math.Min(algo.lcoords.Count - 1, selected);
                X = (ushort)algo.lcoords[selected].X;
                Y = (ushort)algo.lcoords[selected].Y;


                MsgMapItem floorItem = new MsgMapItem(true);
                floorItem.MapObjType = MapObjectType.Item;
                floorItem.ItemID = PacketMsgMapItem.TwilightDance;
                floorItem.ItemColor = (Enums.Color)(i + 1);
                floorItem.MapID = attacker.MapID;
                floorItem.Type = PacketMsgMapItem.Effect;
                floorItem.X = X;
                floorItem.Y = Y;
                floorItem.OnFloor = Time32.Now;
                floorItem.Owner = attacker.Owner;
                while (map.Npcs.ContainsKey(floorItem.UID))
                    floorItem.UID = Network.GamePackets.MsgMapItem.FloorUID.Next;
                map.AddFloorItem(floorItem);

                attacker.Owner.SendScreenSpawn(floorItem, true);

                if (i != 0)
                {
                    MsgAction data = new Network.GamePackets.MsgAction(true);
                    data.UID = attacker.UID;
                    data.Xt1 = X;
                    data.Yt1 = Y;
                    data.ID = PacketMsgAction.Mode.RemoveTrap;
                    data.X = myx;
                    data.Y = myy;
                    attacker.Owner.SendScreen(data, true);

                    double percent = 1;
                    switch (i)
                    {
                        case 1:
                            percent = 0.92;
                            break;
                        case 2:
                            percent = 1.00;
                            break;
                        case 3:
                            percent = 1.1;
                            break;
                    }


                    foreach (Interfaces.IMapObject _obj in attacker.Owner.Screen.Objects)
                    {
                        bool hit = false;
                        var selected2 = Math.Max(0, i - 1) * (int)count;
                        selected2 = Math.Min(algo.lcoords.Count - 1, selected2);
                        if (Kernel.GetDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[selected].X, (ushort)algo.lcoords[selected].Y) <= spell.Range)
                            hit = true;
                        //for (int j = selected2; j < selected; j++)
                        //    if (Kernel.GetDistance(_obj.X, _obj.Y, (ushort)algo.lcoords[j].X, (ushort)algo.lcoords[j].Y) <= spell.Range)
                        //        hit = true;
                        if (hit)
                        {
                            if (_obj.MapObjType == MapObjectType.Monster)
                            {
                                attacked = _obj as Player;
                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                {
                                    var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack) / 2;
                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                    suse.AddTarget(attacked, damage, attack);
                                }
                            }
                            else if (_obj.MapObjType == MapObjectType.Player)
                            {
                                attacked = _obj as Player;
                                if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Intensify))
                                {
                                    attacked.RemoveFlag((ulong)PacketFlag.Flags.Intensify);
                                    attacked.Intensification = 0;
                                }
                                if (CanAttack(attacker, attacked, spell, attack.InteractType == MsgInteract.Melee))
                                {
                                    var damage = Game.Attacking.Calculate.Melee(attacker, attacked, spell, ref attack);
                                    damage = (uint)MathHelper.AdjustDataEx((int)damage, (int)spell.Power);
                                    ReceiveAttack(attacker, attacked, attack, ref damage, spell);

                                    suse.AddTarget(attacked, damage, attack);
                                }

                            }
                            else if (_obj.MapObjType == MapObjectType.SobNpc)
                            {
                                var attackedsob = _obj as MsgNpcInfoEX;
                                if (CanAttack(attacker, attackedsob, spell))
                                {
                                    var damage = Game.Attacking.Calculate.Melee(attacker, attackedsob, ref attack);

                                    ReceiveAttack(attacker, attackedsob, attack, damage, spell);

                                    suse.AddTarget(attackedsob, damage, attack);
                                }
                            }
                        }
                    }
                    if (suse.Targets.Count > 0)
                        attacker.Owner.SendScreen(suse, true);
                    suse.Targets.Clear();
                }
                i++;
            }, 0, 2, 200);
        }
        public static void LotusAttack(MsgMapItem item, Player attacker, MsgInteract attack)
        {
            //Console.WriteLine("For Test");
        }
        public static List<IMapObject> GetObjects(UInt16 ox, UInt16 oy, Client.GameState c)
        {
            UInt16 x, y;
            x = c.Player.X;
            y = c.Player.Y;
            var list = new List<IMapObject>();
            c.Player.X = ox;
            c.Player.Y = oy;
            foreach (IMapObject objects in c.Screen.Objects)
            {
                if (objects != null)
                    if (objects.UID != c.Player.UID)
                        if (!list.Contains(objects))
                            list.Add(objects);
            }
            c.Player.X = x;
            c.Player.Y = y;
            foreach (IMapObject objects in c.Screen.Objects)
            {
                if (objects != null)
                    if (objects.UID != c.Player.UID)
                        if (!list.Contains(objects))
                            list.Add(objects);
            }
            if (list.Count > 0)
                return list;
            return null;
        }
        public static IEnumerable<Client.GameState> PlayerinRange(Player attacker, Player attacked)
        {
            var dictionary = Kernel.GamePool.Values.ToArray();

            return dictionary.Where((player) => player.Player.MapID == attacked.MapID && Kernel.GetDistance(player.Player.X, player.Player.Y, attacker.X, attacker.Y) <= 7);
        }
        public static IEnumerable<Client.GameState> PlayerinRange(MsgMapItem item, int dist)
        {
            var dictionary = Kernel.GamePool.Values.ToArray();
            return dictionary.Where((player) => player.Player.MapID == item.MapID && Kernel.GetDistance(player.Player.X, player.Player.Y, item.X, item.Y) <= dist).OrderBy(player => Kernel.GetDistance(player.Player.X, player.Player.Y, item.X, item.Y));
        }
        public Player findClosestTarget(Player attacker, ushort X, ushort Y, IEnumerable<Interfaces.IMapObject> Array)
        {
            Player closest = attacker;
            int dPrev = 10000, dist = 0;
            foreach (var _obj in Array)
            {
                if (_obj == null) continue;
                if (_obj.MapObjType != MapObjectType.Player && _obj.MapObjType != MapObjectType.Monster) continue;
                dist = Kernel.GetDistance(X, Y, _obj.X, _obj.Y);
                if (dist < dPrev)
                {
                    dPrev = dist;
                    closest = (Player)_obj;
                }
            }
            return closest;
        }
        public static bool CanUseSpell(SpellInformation spell, Client.GameState client)
        {
            if (client.Player.SkillTeamWatchingElitePKMatch != null) return false;
            if (client.WatchingElitePKMatch != null) return false;
            if (client.WatchingGroup != null) return false;
            if (client.TeamWatchingGroup != null) return false;
            if (spell == null) return false;
            if (client.Player.Mana < spell.UseMana) return false;
            if (client.Player.Stamina < spell.UseStamina) return false;
            if (client.Player.MapID == 1707)
            {
                if (spell.ID != 1045 && spell.ID != 1046)
                {
                    client.Send("You can't use any skills here Except FB And SS!"); return false;
                }
            }
            if (spell.UseArrows > 0 && isArcherSkill(spell.ID))
            {
                var weapons = client.Weapons;
                if (weapons.Item2 != null)
                    if (!client.Player.ContainsFlag3((ulong)PacketFlag.Flags.PathOfShadow))
                        if (!ItemHandler.IsArrow(weapons.Item2.ID)) return false;
                return true;
            }
            if (spell.NeedXP == 1 && !client.Player.ContainsFlag((ulong)PacketFlag.Flags.XPList)) return false;
            return true;
        }
        public static void PrepareSpell(SpellInformation spell, Client.GameState client)
        {
            if (spell.NeedXP == 1)
                client.Player.RemoveFlag((ulong)PacketFlag.Flags.XPList);
            if (client.Map.ID != 1039)
            {
                if (spell.UseMana > 0)
                    if (client.Player.Mana >= spell.UseMana)
                        client.Player.Mana -= spell.UseMana;
                if (spell.UseStamina > 0)
                    if (client.Player.Stamina >= spell.UseStamina)
                        client.Player.Stamina -= spell.UseStamina;
            }
        }
        public static void CheckForExtraWeaponPowers(Client.GameState client, Player attacked)
        {
            #region Right Hand
            var weapons = client.Weapons;
            if (weapons.Item1 != null)
            {
                if (weapons.Item1.ID != 0)
                {
                    var Item = weapons.Item1;
                    if (Item.Effect != ItemEffect.None)
                    {
                        if (Kernel.Rate(30))
                        {
                            switch (Item.Effect)
                            {
                                case ItemEffect.HP:
                                    {
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1175;
                                        MsgMagicEffect.SpellLevel = 4;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 300, null);
                                        uint damage = Math.Min(300, client.Player.MaxHitpoints - client.Player.Hitpoints);
                                        client.Player.Hitpoints += damage;
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.MP:
                                    {
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1175;
                                        MsgMagicEffect.SpellLevel = 2;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 300, null);
                                        ushort damage = (ushort)Math.Min(300, client.Player.MaxMana - client.Player.Mana);
                                        client.Player.Mana += damage;
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.Shield:
                                    {
                                        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.MagicShield)) return;
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1020;
                                        MsgMagicEffect.SpellLevel = 0;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 120, null);
                                        client.Player.ShieldTime = 0;
                                        client.Player.ShieldStamp = Time32.Now;
                                        client.Player.MagicShieldStamp = Time32.Now;
                                        client.Player.MagicShieldTime = 0;
                                        client.Player.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                                        client.Player.MagicShieldStamp = Time32.Now;
                                        client.Player.MagicShieldIncrease = 2;
                                        client.Player.MagicShieldTime = 120;
                                        if (client.Player.PlayerFlag == PlayerFlag.Player)
                                            client.Send(DefineConstantsEn_Res.Shield(2, 120));
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.Poison:
                                    {
                                        if (attacked != null)
                                        {
                                            if (Constants.PKForbiddenMaps.Contains(client.Player.MapID)) return;
                                            if (client.Map.BaseID == 700) return;
                                            if (attacked.UID == client.Player.UID) return;
                                            if (attacked.ToxicFogLeft > 0) return;
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.SpellID = 5040;
                                            MsgMagicEffect.Attacker = attacked.UID;
                                            MsgMagicEffect.SpellLevel = 9;
                                            MsgMagicEffect.X = attacked.X;
                                            MsgMagicEffect.Y = attacked.Y;
                                            MsgMagicEffect.AddTarget(attacked, 0, null);
                                            MsgMagicEffect.Targets[attacked.UID].Hit = true;
                                            attacked.ToxicFogStamp = Time32.Now;
                                            attacked.ToxicFogLeft = 10;
                                            attacked.ToxicFogPercent = 0.01F;
                                            client.SendScreen(MsgMagicEffect, true);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            #endregion
            #region Left Hand
            if (weapons.Item2 != null)
            {
                if (weapons.Item2.ID != 0)
                {
                    var Item = weapons.Item2;
                    if (Item.Effect != ItemEffect.None)
                    {
                        if (Kernel.Rate(30))
                        {
                            switch (Item.Effect)
                            {
                                case ItemEffect.HP:
                                    {
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1175;
                                        MsgMagicEffect.SpellLevel = 4;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 300, null);
                                        uint damage = Math.Min(300, client.Player.MaxHitpoints - client.Player.Hitpoints);
                                        client.Player.Hitpoints += damage;
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.MP:
                                    {
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1175;
                                        MsgMagicEffect.SpellLevel = 2;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 300, null);
                                        ushort damage = (ushort)Math.Min(300, client.Player.MaxMana - client.Player.Mana);
                                        client.Player.Mana += damage;
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.Shield:
                                    {
                                        if (client.Player.ContainsFlag((ulong)PacketFlag.Flags.MagicShield))
                                            return;
                                        MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                        MsgMagicEffect.Attacker = 1;
                                        MsgMagicEffect.SpellID = 1020;
                                        MsgMagicEffect.SpellLevel = 0;
                                        MsgMagicEffect.X = client.Player.X;
                                        MsgMagicEffect.Y = client.Player.Y;
                                        MsgMagicEffect.AddTarget(client.Player, 120, null);
                                        client.Player.ShieldTime = 0;
                                        client.Player.ShieldStamp = Time32.Now;
                                        client.Player.MagicShieldStamp = Time32.Now;
                                        client.Player.MagicShieldTime = 0;

                                        client.Player.AddFlag((ulong)PacketFlag.Flags.MagicShield);
                                        client.Player.MagicShieldStamp = Time32.Now;
                                        client.Player.MagicShieldIncrease = 2;
                                        client.Player.MagicShieldTime = 120;
                                        if (client.Player.PlayerFlag == PlayerFlag.Player)
                                            client.Send(DefineConstantsEn_Res.Shield(2, 120));
                                        client.SendScreen(MsgMagicEffect, true);
                                        break;
                                    }
                                case ItemEffect.Poison:
                                    {
                                        if (attacked != null)
                                        {
                                            if (attacked.UID == client.Player.UID) return;
                                            if (Constants.PKForbiddenMaps.Contains(client.Player.MapID)) return;
                                            if (client.Map.BaseID == 700) return;
                                            if (attacked.ToxicFogLeft > 0) return;
                                            MsgMagicEffect MsgMagicEffect = new MsgMagicEffect(true);
                                            MsgMagicEffect.SpellID = 5040;
                                            MsgMagicEffect.Attacker = attacked.UID;
                                            MsgMagicEffect.SpellLevel = 9;
                                            MsgMagicEffect.X = attacked.X;
                                            MsgMagicEffect.Y = attacked.Y;
                                            MsgMagicEffect.AddTarget(attacked, 0, null);
                                            MsgMagicEffect.Targets[attacked.UID].Hit = true;
                                            attacked.ToxicFogStamp = Time32.Now;
                                            attacked.ToxicFogLeft = 10;
                                            attacked.ToxicFogPercent = 0.01F;
                                            client.SendScreen(MsgMagicEffect, true);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            #endregion
        }
        public static bool CanAttack(Player attacker, MsgNpcInfoEX attacked, SpellInformation spell)
        {
            #region Cps Stake
            if (attacker.MapID == 1002)
            {
                if (attacked.UID == 6462)
                {
                    attacked.Die(attacker);
                    {
                        attacker.ConquerPoints += 20000;
                        attacker.Owner.Send(new MsgTalk("killed  [ PronTo-Online ] and get [ 20000 ] Cps", System.Drawing.Color.Azure, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                    }
                }
            }
            if (attacker.MapID == 1002)
            {
                if (attacked.UID == 6463)
                {
                    attacked.Die(attacker);
                    {
                        attacker.ConquerPoints += 20000;
                        attacker.Owner.Send(new MsgTalk("killed  [ PronTo-Online ] and get [ 20000 ] Cps", System.Drawing.Color.Azure, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                    }
                }
            }
            if (attacker.MapID == 1002)
            {
                if (attacked.UID == 6464)
                {
                    attacked.Die(attacker);
                    {
                        attacker.ConquerPoints += 20000;
                        attacker.Owner.Send(new MsgTalk("killed  [ PronTo-Online ] and get [ 20,000 ] Cps", System.Drawing.Color.Azure, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                    }
                }
            }
            if (attacker.MapID == 1002)
            {
                if (attacked.UID == 6465)
                {
                    attacked.Die(attacker);
                    {
                        attacker.ConquerPoints += 20000;
                        attacker.Owner.Send(new MsgTalk("killed  [ PronTo-Online ] and get [ 20,000 ] Cps", System.Drawing.Color.Azure, (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                    }
                }
            }
            #endregion
            #region ClassPoleWar
            if (ClassPoleWar.IsWar)
            {
                if (attacker.MapID == ClassPoleWar.Map.ID)
                {
                    return ClassPoleWar.Attack(0, attacker, attacked);
                }
            }
            #endregion
            #region GuildScoreWar
            if (GuildScoreWar.Map != null)
            {
                if (attacker.MapID == GuildScoreWar.Map.ID)
                {
                    if (attacker.GuildID == 0 || !Game.GuildScoreWar.IsWar)
                    {
                        if (attacked.UID == GuildScoreWar.Pole.UID)
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
            #region NobilityPoleWar
            if (NobiltyPoleWar.IsWar)
            {
                if (attacker.MapID == NobiltyPoleWar.Map.ID)
                {
                    return NobiltyPoleWar.Attack(0, attacker, attacked);
                }
            }
            #endregion
            #region StatueWar
            if (Game.StatuesWar.IsWar)
            {
                if (attacker.MapID == Game.StatuesWar.Map.ID)
                {
                    if (attacker.GuildID == 0 || !Game.StatuesWar.IsWar)
                        if (attacked.UID == Game.StatuesWar.Pole.UID)
                            return false;
                    if (Game.StatuesWar.PoleKeeper != null)
                    {
                        if (Game.StatuesWar.PoleKeeper == attacker.Owner.AsMember)
                            if (attacked.UID == Game.StatuesWar.Pole.UID)
                                return false;

                            else if (attacked.UID == Game.StatuesWar.LeftGate.UID || attacked.UID == Game.StatuesWar.RightGate.UID)
                                if (Game.StatuesWar.PoleKeeper == attacker.Owner.AsMember)
                                    if (attacker.PKMode == PKMode.Team)
                                        return false;

                    }
                }
            }
            #endregion Twinwar
            #region GuildPoleWar
            if (GuildPoleWar.IsWar)
            {
                if (attacker.MapID == GuildPoleWar.Map.ID)
                {
                    return GuildPoleWar.Attack(0, attacker, attacked);
                }
            }
            #endregion
            #region Kingdom
            if (attacker.MapID == 10380)
            {
                if (attacker.GuildID == 0 || !Game.Kingdom.IsWar)
                {
                    if (attacked.UID == 811) return false;
                }
                if (Game.Kingdom.PoleKeeper != null)
                {
                    if (Game.Kingdom.PoleKeeper == attacker.Owner.Guild)
                    {
                        if (attacked.UID == 811) return false;
                    }
                    else if (attacked.UID == 516077 || attacked.UID == 516076)
                    {
                        if (Game.Kingdom.PoleKeeper == attacker.Owner.Guild)
                        {
                            if (attacker.PKMode == PKMode.Team) return false;
                        }
                    }
                }
            }
            #endregion
            #region PoleAssassin
            if (attacked.UID == 123456)
                if (attacked.Hitpoints > 0)
                    if (attacker.GuildID != 0 && attacker.GuildID != Server.Thread.PoleAssassin.KillerGuildID)
                        return true;
                    else return false;
                else return false;
            #endregion
            #region EliteGuildWar
            if (attacker.MapID == 2071)
            {
                if (attacker.GuildID == 0 || !EliteGuildWar.IsWar)
                {
                    if (attacked.UID == 812)
                    {
                        return false;
                    }
                }
                if (EliteGuildWar.PoleKeeper != null)
                {
                    if (EliteGuildWar.PoleKeeper == attacker.Owner.Guild)
                    {
                        if (attacked.UID == 812)
                        {
                            return false;
                        }
                    }
                    else if (attacked.UID == 516075 || attacked.UID == 516074)
                    {
                        if (EliteGuildWar.PoleKeeper == attacker.Owner.Guild)
                        {
                            if (attacker.PKMode == PKMode.Team)
                                return false;
                        }
                    }
                }
            }
            #endregion
            #region HeroOfGame
            if (attacker.MapID == 1507)
            {
                if (!attacker.AllowToAttack)
                    return false;
            }
            #endregion
            #region GuildWar
            if (attacker.MapID == 1038)
            {
                if (attacker.GuildID == 0 || !Game.GuildWar.IsWar)
                {
                    if (attacked.UID == 810) return false;
                }
                if (Game.GuildWar.PoleKeeper != null)
                {
                    if (Game.GuildWar.PoleKeeper == attacker.Owner.Guild)
                    {
                        if (attacked.UID == 810) return false;
                    }
                    else if (attacked.UID == 516075 || attacked.UID == 516074)
                    {
                        if (Game.GuildWar.PoleKeeper == attacker.Owner.Guild)
                        {
                            if (attacker.PKMode == PKMode.Team) return false;
                        }
                    }
                }
            }
            #endregion
            #region ClanWar
            if (attacker.MapID == 1509)
            {
                if (attacker.ClanId == 0 || !ClanWar.IsWar)
                {
                    if (attacked.UID == 813)
                    {
                        return false;
                    }
                }
                if (ClanWar.PoleKeeper != null)
                {
                    if (ClanWar.PoleKeeper == attacker.GetClan)
                    {
                        if (attacked.UID == 813)
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
            #region CaptureTheFlag
            if (attacker.MapID == MsgWarFlag.MapID)
            {
                if (Server.Thread.CTF.Bases.ContainsKey(attacked.UID))
                {
                    var _base = Server.Thread.CTF.Bases[attacked.UID];
                    if (_base.CapturerID == attacker.GuildID)
                        return false;
                }
                return true;
            }
            #endregion
            #region Crow
            if (attacker.MapID == 1039)
            {
                bool stake = true;
                if (attacked.LowerName.Contains("crow"))
                    stake = false;

                ushort levelbase = (ushort)(attacked.Mesh / 10);
                if (stake)
                    levelbase -= 42;
                else
                    levelbase -= 43;

                byte level = (byte)(20 + (levelbase / 3) * 5);
                if (levelbase == 108 || levelbase == 109)
                    level = 125;
                if (attacker.Level >= level)
                    return true;
                else
                {
                    attacker.AttackPacket = null;
                    attacker.Owner.Send(DefineConstantsEn_Res.DummyLevelTooHigh());
                    return false;
                }
            }
            #endregion
            #region Can't Attack Npc
            if (attacked.UID == 76112 || attacked.UID == 127123 || attacked.UID == 141198 || attacked.UID == 9683 || attacked.UID == 2015 || attacked.UID == 20140
                || attacked.UID == 9884 || attacked.UID == 9885 || attacked.UID == 9886 || attacked.UID == 9887
                || attacked.UID == 9994 || attacked.UID == 9995 || attacked.UID == 9996 || attacked.UID == 9997 || attacked.UID == 41162
                || attacked.UID == 180 || attacked.UID == 181 || attacked.UID == 182 || attacked.UID == 183 || attacked.UID == 801
                || attacked.UID == 184 || attacked.UID == 185 || attacked.UID == 7882 || attacked.UID == 1232 || attacked.UID == 16416 || attacked.UID == 16417
                || attacked.UID == 216341 || attacked.UID == 1231 || attacked.UID == 6567 || attacked.UID == 4132 || attacked.UID == 64132 || attacked.UID == 44821)
            {
                attacker.AttackPacket = null;
                return false;
            }
            #endregion
            return true;
        }
        public static bool CanAttack(Player attacker, Player attacked, SpellInformation spell, bool melee)
        {
            #region Kongfu
            if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag == PlayerFlag.Player)
            {
                if (attacker.PKMode == PKMode.Kongfu && attacker.KongfuActive == true && attacked.PKMode != PKMode.Kongfu && attacked.KongfuActive == false && !attacked.ContainsFlag((ulong)PacketFlag.Flags.FlashingName) && !attacked.ContainsFlag((ulong)PacketFlag.Flags.BlackName))
                {
                    if (attacked.Dead) return false;
                    attacker.Owner.Send(new MsgTalk("You can only attack fighters of the Jiang Hu while in the Jiang Hu", (uint)PacketMsgTalk.MsgTalkType.TopLeft));
                    return false;
                }
                else if (attacker.PKMode == PKMode.Kongfu && attacker.KongfuActive == true)
                {
                    if (attacked.PKMode == PKMode.Kongfu || attacked.KongfuActive == true || attacked.ContainsFlag((ulong)PacketFlag.Flags.FlashingName) || attacked.ContainsFlag((ulong)PacketFlag.Flags.BlackName))
                    {
                        if (attacked.Dead) return false;
                        if (attacker.MapID == 1002 || attacker.MapID == 1000 || attacker.MapID == 1015 || attacker.MapID == 1020 || attacker.MapID == 1011 || attacker.MapID == 3055)
                        {
                            try
                            {
                                if (attacker.Settings != MsgOwnKongfuPKSetting.Settings.None)
                                {
                                    if ((attacker.Settings & MsgOwnKongfuPKSetting.Settings.NotHitFriends) == MsgOwnKongfuPKSetting.Settings.NotHitFriends)
                                    {
                                        if (attacker.Owner.Friends.ContainsKey(attacked.UID))
                                            return false;
                                    }
                                    if ((attacker.Settings & MsgOwnKongfuPKSetting.Settings.NoHitAlliesClan) == MsgOwnKongfuPKSetting.Settings.NoHitAlliesClan)
                                    {
                                        var attacker_clan = attacker.GetClan;
                                        if (attacker_clan != null)
                                        {
                                            if (attacker_clan.Allies.ContainsKey(attacked.ClanId))
                                                return false;
                                        }
                                    }
                                    if ((attacker.Settings & MsgOwnKongfuPKSetting.Settings.NotHitAlliedGuild) == MsgOwnKongfuPKSetting.Settings.NotHitAlliedGuild)
                                    {
                                        if (attacker.Owner.Guild != null)
                                        {
                                            if (attacker.Owner.Guild.Ally.ContainsKey(attacked.GuildID))
                                                return false;
                                        }
                                    }
                                    if ((attacker.Settings & MsgOwnKongfuPKSetting.Settings.NotHitClanMembers) == MsgOwnKongfuPKSetting.Settings.NotHitClanMembers)
                                    {
                                        if (attacker.ClanId == attacked.ClanId)
                                            return false;
                                    }
                                    if ((attacker.Settings & MsgOwnKongfuPKSetting.Settings.NotHitGuildMembers) == MsgOwnKongfuPKSetting.Settings.NotHitGuildMembers)
                                    {
                                        if (attacker.GuildID == attacked.GuildID)
                                            return false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            #endregion
            #region ThunderCloud
            if (attacked.Name == "Thundercloud")
            {
                if (attacked.OwnerUID == attacker.UID) return false;
                if (!Constants.PKForbiddenMaps.Contains(attacker.MapID))
                {
                    if (attacker.PKMode != PKMode.PK &&
                     attacker.PKMode != PKMode.Team)
                        return false;
                    else
                    {
                        attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                        attacker.FlashingNameStamp = Time32.Now;
                        attacker.FlashingNameTime = 20;

                        return true;
                    }
                }
                else return false;
            }
            #endregion
            if (attacker.ContainsFlag4((ulong)PacketFlag.Flags.xChillingSnow))
                return false;
            if (attacker.MapID == 1507)
            {
                if (!attacker.AllowToAttack)
                    return false;
            }
            if (attacked.PlayerFlag == PlayerFlag.Monster)
            {
                if (attacked.Companion)
                {
                    if (attacked.Owner == attacker.Owner) return false;
                }
            }
            if (attacker.UID == attacked.UID)
                return false;
            if (attacker.PKMode == PKMode.Guild)
            {
                if (attacker.Owner.Guild != null && attacker.Owner.Guild.Enemy.ContainsKey(attacked.GuildID))
                {
                    if (attacked.Dead) return false;
                    if (attacker.UID == attacked.UID) return false;
                    if (attacker.MapID == 1000 || attacker.MapID == 1015 || attacker.MapID == 1020 || attacker.MapID == 1011)
                    {
                        attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                        attacker.FlashingNameStamp = Time32.Now;
                        attacker.FlashingNameTime = 10;
                        return true;
                    }
                }
            }
            if (attacker.PKMode == PKMode.Revenge)
            {
                if (attacker.Owner.Enemy.ContainsKey(attacked.UID))
                {
                    if (attacked.Dead) return false;
                    if (attacker.UID == attacked.UID) return false;
                    if (attacker.MapID == 1000 || attacker.MapID == 1015 || attacker.MapID == 1020 || attacker.MapID == 1011)
                    {
                        attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                        attacker.FlashingNameStamp = Time32.Now;
                        attacker.FlashingNameTime = 10;
                        return true;
                    }
                }
            }
            if (attacked.PlayerFlag == PlayerFlag.Monster)
            {
                if (attacked.MonsterInfo.ID == MonsterInformation.ReviverID)
                    return false;
            }
            if (attacked.Dead)
            {
                return false;
            }
            if (attacker.PlayerFlag == PlayerFlag.Player && attacker.Owner.WatchingElitePKMatch != null)
            {
                return false;
            }
            if (attacked.PlayerFlag == PlayerFlag.Player && attacked.Owner.WatchingElitePKMatch != null)
            {
                return false;
            }
            if (attacker.PlayerFlag == PlayerFlag.Player)
            {
                if (attacked != null && attacked.PlayerFlag == PlayerFlag.Player)
                    if (attacker.Owner.InTeamQualifier() && attacked.Owner.InTeamQualifier())
                        return !attacker.Owner.Team.IsTeammate(attacked.UID);
            }
            if (attacker.MapID == MsgWarFlag.MapID)
            {
                if (!MsgWarFlag.Attackable(attacker) || !MsgWarFlag.Attackable(attacked))
                    return false;
            }
            if (spell != null)
                if (attacker.PlayerFlag == PlayerFlag.Player)
                    if (attacker.Owner.WatchingGroup != null)
                        return false;
            if (attacked == null)
            {
                return false;
            }
            if (attacker.SkillTeamWatchingElitePKMatch != null)
            {
                return false;
            }
            if (attacked.Dead)
            {
                attacker.AttackPacket = null;
                return false;
            }
            if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag == PlayerFlag.Player && attacked.Owner.Team != null && attacker.Owner.Team != null && attacker.Owner.Team.EliteFighterStats != null && attacked.Owner.Team.EliteFighterStats != null && attacker.MapID == attacked.Owner.Team.EliteMatch.Map.ID && attacker.Owner.Team.EliteMatch != null)
            {
                return attacker.Owner.Team != attacked.Owner.Team;
            }
            if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag == PlayerFlag.Player)
                if ((attacker.Owner.InQualifier() && attacked.Owner.IsWatching()) || (attacked.Owner.InQualifier() && attacker.Owner.IsWatching()))
                    return false;
            if (attacker.PlayerFlag == PlayerFlag.Player)
                if (Time32.Now < attacker.Owner.CantAttack)
                    return false;
            if (attacked.PlayerFlag == PlayerFlag.Monster)
            {
                if (attacked.Companion)
                {
                    if (Constants.PKForbiddenMaps.Contains(attacker.Owner.Map.ID))
                    {
                        if (attacked.Owner == attacker.Owner) return false;
                        if (attacker.PKMode != PKMode.PK && attacker.PKMode != PKMode.Team) return false;
                        else
                        {
                            attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                            attacker.FlashingNameStamp = Time32.Now;
                            attacker.FlashingNameTime = 10;
                            return true;
                        }
                    }
                }
                if (attacked.Name.Contains("Guard"))
                {
                    if (attacker.PKMode != PKMode.PK && attacker.PKMode != PKMode.Team) return false;
                    else
                    {
                        attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                        attacker.FlashingNameStamp = Time32.Now;
                        attacker.FlashingNameTime = 10;
                        return true;
                    }
                }
                else return true;
            }
            else
            {
                if (attacked.PlayerFlag == PlayerFlag.Player)
                    if (!attacked.Owner.Attackable)
                        return false;
                if (attacker.PlayerFlag == PlayerFlag.Player)
                    if (attacker.Owner.WatchingGroup == null)
                        if (attacked.PlayerFlag == PlayerFlag.Player)
                            if (attacked.Owner.WatchingGroup != null)
                                return false;
                if (spell != null)
                {
                    if (spell.ID != 8001)
                    {
                        if (spell.OnlyGround)
                            if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                                return false;
                        if (melee && attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                    }
                }
                if (spell != null)
                {
                    if (spell.ID == 6010)
                    {
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                    }
                }
                if (spell != null)
                {
                    if (spell.ID == 10381)
                    {
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                    }
                }
                if (spell != null)
                {
                    if (spell.ID == 6000)
                    {
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                    }
                }
                if (spell != null)
                {
                    if (attacker.OnManiacDance())
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                }
                if (spell != null)
                {
                    if (spell.ID == 5030)
                    {
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;
                    }
                }
                if (spell == null)
                {
                    if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                        return false;
                }
                if (Constants.PKForbiddenMaps.Contains(attacker.Owner.Map.ID))
                {
                    if (((attacker.PKMode == PKMode.PK) || (attacker.PKMode == PKMode.Team)) || ((spell != null) && spell.CanKill))
                    {
                        attacker.Owner.Send(DefineConstantsEn_Res.PKForbidden);
                        attacker.AttackPacket = null;
                    }
                    return false;
                }
                if (attacker.PKMode == PKMode.Capture)
                {
                    if (attacked.ContainsFlag((ulong)PacketFlag.Flags.FlashingName) || attacked.PKPoints > 99)
                    {
                        return true;
                    }
                }
                if (attacker.PKMode == PKMode.Peace)
                {
                    return false;
                }
                if (attacker.UID == attacked.UID)
                    return false;
                if (attacker.PKMode == PKMode.Team)
                {
                    if (attacker.Owner.Team != null)
                    {
                        if (attacker.Owner.Team.IsTeammate(attacked.UID))
                        {
                            attacker.AttackPacket = null;
                            return false;
                        }
                    }
                    if (attacker.PKMode == PKMode.Team)
                    {
                        if (attacker.Owner.Team != null)
                        {
                            if (!attacker.Owner.Team.IsTeammate(attacked.UID))
                            {
                                if (attacker.InSkillPk == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    if (attacker.GuildID == attacked.GuildID && attacker.GuildID != 0)
                    {
                        attacker.AttackPacket = null;
                        return false;
                    }
                    if (attacker.ClanId == attacked.ClanId && attacker.ClanId != 0)
                    {
                        attacker.AttackPacket = null;
                        return false;
                    }
                    if (attacker.Owner.Friends.ContainsKey(attacked.UID))
                    {
                        attacker.AttackPacket = null;
                        return false;
                    }
                    if (attacker.Owner.Guild != null)
                    {
                        if (attacker.Owner.Guild.Ally.ContainsKey(attacked.GuildID))
                        {
                            attacker.AttackPacket = null;
                            return false;
                        }
                    }
                    if (attacker.ClanId != 0)
                    {
                        var clan = attacker.GetClan;
                        if (clan != null)
                            if (clan.Allies.ContainsKey(attacked.ClanId))
                            {
                                attacker.AttackPacket = null;
                                return false;
                            }
                    }
                }
                if (spell != null)
                    if (spell.OnlyGround)
                        if (attacked.ContainsFlag((ulong)PacketFlag.Flags.Fly))
                            return false;

                if (spell != null)
                    if (!spell.CanKill)
                        return true;

                if (attacker.PKMode != PKMode.PK &&
                    attacker.PKMode != PKMode.Team && attacked.PKPoints < 99)
                {
                    attacker.AttackPacket = null;
                    return false;
                }
                else
                {
                    if (!attacked.ContainsFlag((ulong)PacketFlag.Flags.FlashingName))
                    {
                        if (!attacked.ContainsFlag((ulong)PacketFlag.Flags.BlackName))
                        {
                            if (Constants.PKFreeMaps.Contains(attacker.MapID)) return true;
                            if (attacker.Owner.Map.BaseID == 700) return true;
                            attacker.AddFlag((ulong)PacketFlag.Flags.FlashingName);
                            attacker.FlashingNameStamp = Time32.Now;
                            attacker.FlashingNameTime = 10;
                        }
                    }
                    return true;
                }
            }
        }
        public static void ReceiveAttack(Player attacker, Player attacked, MsgInteract attack, ref uint damage, SpellInformation spell)
        {
            //  Perfection.DoEffects(attacker, attacked, attack, ref damage, spell);
            #region BackFire
            if (attacked.ContainsFlag3((ulong)PacketFlag.Flags.BackFire))
            {
                if (attacked.Owner != null && attacked.Owner.Spells != null && attacked.Owner.Spells.ContainsKey(12680))
                {
                    var spelll = DB.SpellTable.GetSpell(12680, attacked.Owner);
                    if (damage <= spelll.Power)
                    {
                        MsgMagicEffect suses = new MsgMagicEffect(true);
                        suses.Attacker = attacked.UID;
                        suses.Attacker1 = attacker.UID;
                        suses.SpellID = 12680;
                        suses.SpecialEffect = 1;
                        attack.InteractType = MsgInteract.BackFire;
                        suses.AddTarget(attacker.UID, damage, null);
                        if (attacker.Hitpoints <= damage)
                        {
                            attacker.Die(attacked);
                        }
                        else
                        {
                            attacker.Hitpoints -= damage;
                        }
                        attacked.Owner.SendScreen(suses, true);
                    }
                }
            }
            #endregion
            #region StatuesWar
            if (StatuesWar.IsWar)
            {
                if (attacker.MapID == StatuesWar.Map.ID)
                {
                    if (attacked.UID == StatuesWar.Pole.UID)
                    {
                        if (StatuesWar.PoleKeeper == attacker.Owner.AsMember)
                            return;
                        if (attacked.Hitpoints <= damage)
                            attacked.Hitpoints = 0;
                        StatuesWar.AddScore(damage, attacker.Owner.AsMember);
                    }
                }
            }
            #endregion
            #region ThunderCloud
            if (attacked.Name == "Thundercloud")
            {
                if (spell != null && spell.ID != 0)
                {
                    if (Kernel.Rate(75)) damage = 1;
                    else damage = 0;
                }
                else if (spell == null || spell.ID == 0)
                {
                    damage = 1;
                }
            }
            foreach (var th in Kernel.Maps[attacker.MapID].Entities.Values.Where(i => i.Name == "Thundercloud"))
            {
                if (th.OwnerUID == attacked.UID)
                {
                    if (attacker == null || Kernel.GetDistance(attacker.X, attacker.Y, th.X, th.Y) > th.MonsterInfo.AttackRange || attacker.Dead) break;
                    th.MonsterInfo.InSight = attacker.UID;
                    break;
                }
            }
            #endregion
            #region RevengeTaill
            if (attacked.ContainsFlag4((ulong)PacketFlag.Flags.RevengeTaill))
            {
                if (attacked.Owner != null && attacked.Owner.Spells != null && attacked.Owner.Spells.ContainsKey(13030))
                {
                    var spelll = SpellTable.GetSpell(13030, attacked.Owner);
                    if (damage <= spelll.Power)
                    {
                        MsgMagicEffect suses = new MsgMagicEffect(true);
                        suses.Attacker = attacked.UID;
                        suses.Attacker1 = attacker.UID;
                        suses.SpellID = 13030;
                        suses.SpecialEffect = 1;
                        suses.AddTarget(attacker.UID, damage, null);
                        if (attacker.Hitpoints <= damage)
                        {
                            attacker.Die(attacked);
                        }
                        else
                        {
                            attacker.Hitpoints -= damage;
                        }
                        attacked.Owner.SendScreen(suses, true);
                    }
                }
            }
            #endregion
            #region ChillingSnow
            if (attacked.ContainsFlag4((ulong)PacketFlag.Flags.ChillingSnow) && attacked.IsStomper2() && attacker.PlayerFlag == PlayerFlag.Player)
            {
                var spell1 = SpellTable.GetSpell(12960, attacked.Owner);
                int rate = 95;
                int diff = attacker.BattlePower - attacked.BattlePower;
                if (diff < 0) diff = 0;
                rate -= (byte)(diff * 5);
                if (rate < 0) rate = 0;
                if (Kernel.Rate(rate))
                {
                    attacker.AddFlag4((ulong)PacketFlag.Flags.xChillingSnow);
                    attacker.ChillingSnowStamp = Time32.Now;
                    attacker.ChillingSnow = (byte)(spell1.Level + 1);
                }
            }
            #endregion
            #region FreezingPelter
            if (attacked.ContainsFlag4((ulong)PacketFlag.Flags.FreezingPelter) && attacked.IsStomper2() && attacker.PlayerFlag == PlayerFlag.Player)
            {
                var spell1 = SpellTable.GetSpell(13020, attacked.Owner);
                int rate = 30;
                int diff = attacker.BattlePower - attacked.BattlePower;
                if (diff < 0) diff = 0;
                rate -= (byte)(diff * 5);
                if (rate < 0) rate = 0;
                if (Kernel.Rate(rate))
                {
                    attacker.AddFlag4((ulong)PacketFlag.Flags.xFreezingPelter);
                    attacker.FreezingPelterStamp = Time32.Now;
                    byte num = 0;
                    if (spell1.Level == 0) num = 1;
                    if (spell1.Level == 1) num = 1;
                    if (spell1.Level == 2) num = 2;
                    if (spell1.Level == 3) num = 2;
                    if (spell1.Level == 4) num = 3;
                    if (spell1.Level == 5) num = 3;
                    if (spell1.Level == 5) num = 4;
                    attacker.FreezingPelter = num;
                }
            }
            #endregion
            #region Bosses Scores
            if (attacked.PlayerFlag == PlayerFlag.Monster)
            {
                if (attacked.MonsterInfo != null)
                {
                    if (attacked.MonsterInfo.Score.ContainsKey(attacker))
                        attacked.MonsterInfo.Score[attacker] += damage;
                    else attacked.MonsterInfo.Score.Add(attacker, damage);
                }
            }
            #endregion
            #region Pet Defender
            if (attacked != null)
            {
                if (attacker.UID != attacked.UID)
                {
                    if (attacker.Owner.Companion != null)
                        attacker.Owner.Companion.MonsterInfo.InSight = attacked.UID;
                }
            }
            #endregion
            if (attacker.PlayerFlag == PlayerFlag.Monster && attacked.PlayerFlag == PlayerFlag.Player)
            {
                if (attacked.Action == Enums.ConquerAction.Sit)
                    if (attacked.Stamina > 20)
                        attacked.Stamina -= 20;
                    else attacked.Stamina = 0;
                attacked.Action = Enums.ConquerAction.None;
            }
            if (attack.InteractType == MsgInteract.Magic)
            {
                if (attacked.Hitpoints <= damage)
                {
                    if (attacker.PlayerFlag == PlayerFlag.Player && attacked.PlayerFlag == PlayerFlag.Player)
                    {
                        if (SpellTable.AllowSkillSoul == null) return;
                        if (spell == null) return;
                        if (SpellTable.AllowSkillSoul.Contains(spell.ID))
                        {
                            byte[] tets = new byte[12 + 8];
                            Writer.Ushort(12, 0, tets);
                            Writer.Ushort(2710, 2, tets);
                            Writer.Uint(spell.ID, 4, tets);
                            attacked.Owner.SendScreen(tets, true); attacker.Owner.SendScreen(tets, true);
                        }
                    }
                    if (attacked.Owner != null)
                    {
                        attacked.Owner.UpdateQualifier(attacked.Hitpoints);
                    }
                    //attacker.Owner.UpdateQualifier(attacker.Owner, attacked.Owner, attacked.Hitpoints);
                    attacked.CauseOfDeathIsMagic = true;
                    attacked.Die(attacker);
                    attacked.IsDropped = false;
                    if (attacked.Owner != null && attacker.Owner != null)
                    {
                        if (attacked.Owner.Team != null && attacker.Owner.Team != null)
                        {
                            if (attacker.Owner.Team.EliteFighterStats != null && attacked.Owner.Team.EliteFighterStats != null)
                            {
                                if (attacker.Owner.Team.EliteMatch != null)
                                {
                                    if (!attacked.Owner.Team.Alive)
                                    {
                                        attacker.Owner.Team.EliteFighterStats.Points += damage;
                                        attacker.Owner.Team.EliteMatch.End(attacked.Owner.Team);
                                    }
                                    else
                                    {
                                        attacker.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                        attacked.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (attacked.Name != "Thundercloud")
                    {
                        if (attacked.Owner != null && attacker.Owner != null)
                        {
                            if (attacked.Owner.Team != null && attacker.Owner.Team != null)
                            {
                                if (attacker.Owner.Team.EliteFighterStats != null && attacked.Owner.Team.EliteFighterStats != null)
                                {
                                    if (attacker.Owner.Team.EliteMatch != null)
                                    {
                                        attacker.Owner.Team.EliteFighterStats.Points += damage;
                                        attacker.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                        attacked.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                    }
                                }
                            }
                        }
                        if (attacked.Owner != null)
                        {
                            attacked.Owner.UpdateQualifier(damage);
                        }
                        attacked.Hitpoints -= damage;
                    }
                    else
                    {
                        attacked.Hitpoints -= 2100;
                    }
                }
            }
            else
            {
                if (attacked.Hitpoints <= damage)
                {
                    if (attacked.PlayerFlag == PlayerFlag.Player)
                    {
                        attacked.Owner.UpdateQualifier(attacked.Hitpoints);
                        attacked.Owner.SendScreen(attack, true);
                        attacker.AttackPacket = null;
                    }
                    else
                    {
                        attacked.MonsterInfo.SendScreen(attack);
                    }
                    attacked.Die(attacker);
                    if (attacker.PKMode == PKMode.Kongfu)
                    {
                        if (attacked.KongfuActive)
                        {
                            if (attacker.MyKongFu != null && attacker.MyKongFu != null)
                                attacker.MyKongFu.GetKill(attacker.Owner, attacked.MyKongFu);
                        }
                    }
                    if (attacked.Owner != null && attacker.Owner != null)
                    {
                        if (attacked.Owner.Team != null && attacker.Owner.Team != null)
                        {
                            if (attacker.Owner.Team.EliteFighterStats != null && attacked.Owner.Team.EliteFighterStats != null)
                            {
                                if (attacker.Owner.Team.EliteMatch != null)
                                {
                                    if (attacker.MapID == attacked.Owner.Team.EliteMatch.Map.ID)
                                    {
                                        if (!attacked.Owner.Team.Alive)
                                        {
                                            attacker.Owner.Team.EliteFighterStats.Points += damage;
                                            attacker.Owner.Team.EliteMatch.End(attacked.Owner.Team);
                                        }
                                        else
                                        {
                                            attacker.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                            attacked.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (attacked.Name != "Thundercloud")
                    {

                        if (attacked.Owner != null && attacker.Owner != null)
                        {
                            if (attacked.Owner.Team != null && attacker.Owner.Team != null)
                            {
                                if (attacker.Owner.Team.EliteFighterStats != null && attacked.Owner.Team.EliteFighterStats != null)
                                {
                                    if (attacker.Owner.Team.EliteMatch != null)
                                    {
                                        if (attacker.MapID == attacked.Owner.Team.EliteMatch.Map.ID)
                                        {
                                            attacker.Owner.Team.EliteFighterStats.Points += damage;
                                            attacker.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                            attacked.Owner.Team.SendMesageTeam(attacker.Owner.Team.EliteMatch.CreateUpdate().ToArray(), 0);
                                        }
                                    }
                                }
                            }
                        }
                        attacked.Hitpoints -= damage;
                        if (attacked.PlayerFlag == PlayerFlag.Player)
                        {
                            attacked.Owner.UpdateQualifier(damage);
                            attacked.Owner.SendScreen(attack, true);
                        }
                        else
                            attacked.MonsterInfo.SendScreen(attack);
                        attacker.AttackPacket = attack;
                        attacker.AttackStamp = Time32.Now;
                    }
                    else
                    {
                        attacked.Hitpoints -= 2100;
                    }
                }
            }
            #region ThunderCloud
            foreach (var th in Kernel.Maps[attacker.MapID].Entities.Values.Where(i => i.Name == "Thundercloud"))
            {
                if (th.OwnerUID == attacker.UID)
                {
                    if (attacked == null || Kernel.GetDistance(attacked.X, attacked.Y, th.X, th.Y) > th.MonsterInfo.AttackRange || attacked.Dead) break;
                    th.MonsterInfo.InSight = attacked.UID;
                    break;
                }
            }
            #endregion
        }
        public static void ReceiveAttack(Player attacker, MsgNpcInfoEX attacked, MsgInteract attack, uint damage, SpellInformation spell)
        {
          
            #region PoleAssassin
            if (attacked.UID == 123456)
            {
                if (Server.Thread.PoleAssassin.KillerGuildID == attacker.Owner.Guild.ID)
                    return;
                Server.Thread.PoleAssassin.AddScore(damage, attacker.Owner.Guild);
            }
            #endregion
            #region GuildScoreWar
            if (GuildScoreWar.IsWar)
            {
                if (GuildScoreWar.Map != null)
                {
                    if (attacker.MapID == GuildScoreWar.Map.ID)
                        GuildScoreWar.AddScore(damage, attacker.Owner.Guild);
                }
            }
            #endregion
            #region ClassPoleWar
            if (ClassPoleWar.IsWar)
            {
                if (attacker.MapID == ClassPoleWar.Map.ID)
                {
                    ClassPoleWar.Attack(damage, attacker, attacked);
                }
            }
            #endregion
            #region NobilityPoleWar
            if (Game.NobiltyPoleWar.IsWar)
            {
                if (attacker.MapID == NobiltyPoleWar.Map.ID)
                {
                    NobiltyPoleWar.Attack(damage, attacker, attacked);
                }
            }
            #endregion
            #region StatuesWar
            if (Game.StatuesWar.IsWar)
            {
                if (attacker.MapID == Game.StatuesWar.Map.ID)
                {
                    if (attacked.UID == Game.StatuesWar.Pole.UID)
                    {
                        if (Game.StatuesWar.PoleKeeper == attacker.Owner.AsMember)
                            return;
                        if (attacked.Hitpoints <= damage)
                            attacked.Hitpoints = 0;
                        Game.StatuesWar.AddScore(damage, attacker.Owner.AsMember);
                    }
                }
            }
            #endregion TWin War
            #region GuildPoleWar
            if (GuildPoleWar.IsWar)
            {
                if (attacker.MapID == GuildPoleWar.Map.ID)
                {
                    GuildPoleWar.Attack(damage, attacker, attacked);
                }
            }
            #endregion
            #region EliteGuildWar
            if (attacker.MapID == 2071)
            {
                if (attacked.UID == 812)
                {
                    if (EliteGuildWar.PoleKeeper == attacker.Owner.Guild)
                        return;
                    if (attacked.Hitpoints <= damage)
                        attacked.Hitpoints = 0;
                    EliteGuildWar.AddScore(damage, attacker.Owner.Guild);
                }

            }
            #endregion
            #region GuildWar
            if (Game.GuildWar.IsWar)
            {
                if (attacker.MapID == 1038)
                {
                    if (attacked.UID == 810)
                    {
                        if (Game.GuildWar.PoleKeeper == attacker.Owner.Guild) return;
                        if (attacked.Hitpoints <= damage) attacked.Hitpoints = 0;
                        Game.GuildWar.AddScore(damage, attacker.Owner.Guild);
                    }
                }
            }
            #endregion
            #region Kingdom
            if (Game.Kingdom.IsWar)
            {
                if (attacker.MapID == 10380)
                {
                    if (attacked.UID == 811)
                    {
                        if (Game.Kingdom.PoleKeeper == attacker.Owner.Guild) return;
                        if (attacked.Hitpoints <= damage) attacked.Hitpoints = 0;
                        Game.Kingdom.AddScore(damage, attacker.Owner.Guild);
                    }
                }
            }
            #endregion
            #region ClanWar
            if (attacker.MapID == 1509)
            {
                if (attacked.UID == 813)
                {
                    MsgFamily clan = attacker.GetClan;
                    if (ClanWar.PoleKeeper == clan)
                        return;
                    if (attacked.Hitpoints <= damage)
                        attacked.Hitpoints = 0;
                    ClanWar.AddScore(damage, clan);
                }
            }
            #endregion
            #region Crow
            if (attacker.PlayerFlag == PlayerFlag.Player)
                if (damage > attacked.Hitpoints)
                {
                    if (attacker.MapID == 1039)
                        attacker.Owner.IncreaseExperience(Math.Min(damage, attacked.Hitpoints), true);
                    if (spell != null)
                        attacker.Owner.IncreaseSpellExperience(Math.Min(damage, attacked.Hitpoints), spell.ID);
                }
                else
                {
                    if (attacker.MapID == 1039)
                        attacker.Owner.IncreaseExperience(damage, true);
                    if (spell != null)
                        attacker.Owner.IncreaseSpellExperience(damage, spell.ID);
                }
            #endregion
            #region CaptureTheFlag
            if (attacker.MapID == MsgWarFlag.MapID)
            {
                if (attacker.GuildID != 0 && Server.Thread.CTF.Bases[attacked.UID].CapturerID != attacker.GuildID)
                {
                    if (attacked.Hitpoints <= damage)
                    {
                        Server.Thread.CTF.FlagOwned(attacked);
                    }
                    Server.Thread.CTF.AddScore(damage, attacker.Owner.Guild, attacked);
                }
            }
            #endregion
            #region Hitpoints
            if (attack.InteractType == MsgInteract.Magic)
            {
                if (attacked.Hitpoints <= damage)
                {
                    attacked.Die(attacker);
                }
                else
                {
                    attacked.Hitpoints -= damage;
                }
            }
            else
            {
                attacker.Owner.SendScreen(attack, true);
                if (attacked.Hitpoints <= damage)
                {
                    attacked.Die(attacker);
                }
                else
                {
                    attacked.Hitpoints -= damage;
                    attacker.AttackPacket = attack;
                    attacker.AttackStamp = Time32.Now;
                }
            }
            #endregion
        }
    }
}
