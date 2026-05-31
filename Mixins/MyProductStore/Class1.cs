using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class MyProductBlock
        {
            List<MyStoreQueryItem> _storeItems = new List<MyStoreQueryItem>();
            internal IMyStoreBlock Block { get; set; } = null;
            internal bool Trading { get; set; } = true;
            internal StringBuilder TradeInfo { get; private set; } = new StringBuilder();

            internal MyProductBlock(bool trading) { Trading = trading; }
            internal MyProductBlock(IMyStoreBlock StoreBlock) { Block = StoreBlock; }
            internal MyProductBlock(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string nameStore) { GetBlocks(TerminalSystem, CubeGrid, nameStore); }

            internal void GetBlocks(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid ThisCubeGrid, string tagStoreName)
            {
                List<IMyStoreBlock> temp = new List<IMyStoreBlock>();
                TerminalSystem.GetBlocksOfType(temp, x => x.CubeGrid == ThisCubeGrid && x.CustomName.ToLower().Contains(tagStoreName.ToLower()));
                foreach (var t in temp) { Block = t; break; }
            }

            internal void PlaceOfferingsAndSales(ref Dictionary<string, MyItem> ItemsForSaleBuy, string MyObjectBuilder_name, int globalMarkupPercent, bool append = false)
            {
                if (!Trading || Block == null || !Block.IsWorking) return;
                _storeItems.Clear();
                if (!append) { TradeInfo.Clear(); TradeInfo.AppendLine($"Выкладка товаров осуществлена {DateTime.Now:g}"); }
                Block.GetPlayerStoreItems(_storeItems);
                foreach (var Item in ItemsForSaleBuy)
                {
                    if (Item.Value.Mode == TradeModel.Storage)
                    {
                        if (Item.Value.AlowBuy && Item.Value.Amount < Item.Value.MaxAmount)
                            CreateOrder(ref MyObjectBuilder_name, Item.Key, Item.Value.BuyPrice, Item.Value.MaxAmount - Item.Value.Amount, true);
                        else if (Item.Value.AlowSale && Item.Value.Amount > Item.Value.MaxAmount)
                        {
                            int salePrice = Item.Value.GetEffectiveSalePrice(globalMarkupPercent);
                            CreateOffer(ref MyObjectBuilder_name, Item.Key, salePrice, Item.Value.Amount - Item.Value.MaxAmount, true);
                        }
                    }
                    else if (Item.Value.Mode == TradeModel.Shop)
                    {
                        if (Item.Value.AlowBuy && Item.Value.Amount < Item.Value.MaxAmount)
                            CreateOrder(ref MyObjectBuilder_name, Item.Key, Item.Value.BuyPrice, Item.Value.MaxAmount - Item.Value.Amount);
                        if (Item.Value.AlowSale && Item.Value.Amount > 0)
                        {
                            int salePrice = Item.Value.GetEffectiveSalePrice(globalMarkupPercent);
                            CreateOffer(ref MyObjectBuilder_name, Item.Key, salePrice, Item.Value.Amount);
                        }
                    }
                    else if (Item.Value.Mode == TradeModel.SellOnly)
                    {
                        if (Item.Value.AlowSale && Item.Value.Amount > 0)
                        {
                            int salePrice = Item.Value.GetEffectiveSalePrice(globalMarkupPercent);
                            CreateOffer(ref MyObjectBuilder_name, Item.Key, salePrice, Item.Value.Amount, true);
                        }
                    }
                }
                if (append) Block.CustomData += TradeInfo.ToString();
                else Block.CustomData = TradeInfo.ToString();
                _storeItems.Clear();
                TradeInfo.Clear();
            }

            void CreateOrder(ref string MyObjectBuilder_name, string itemName, int BuyPrice, int dif, bool agressiveRemove = false)
            {
                if (!IsPosted(MyObjectBuilder_name, itemName, BuyPrice, dif))
                {
                    if (agressiveRemove) RemoveDuplicates(ref MyObjectBuilder_name, ref itemName, _storeItems);
                    else RemoveDuplicates(ref MyObjectBuilder_name, ref itemName, _storeItems, BuyPrice);
                    InsertOrder(MyObjectBuilder_name + "/" + itemName, dif, BuyPrice);
                }
                else TradeInfo.AppendLine($"[No update] Закупка {itemName} в кол-ве {dif}шт по цене {BuyPrice}кр. уже размещена");
            }

            void CreateOffer(ref string MyObjectBuilder_name, string itemName, int SalePrice, int dif, bool agressiveRemove = false)
            {
                if (!IsPosted(MyObjectBuilder_name, itemName, SalePrice, dif))
                {
                    if (agressiveRemove) RemoveDuplicates(ref MyObjectBuilder_name, ref itemName, _storeItems);
                    else RemoveDuplicates(ref MyObjectBuilder_name, ref itemName, _storeItems, SalePrice);
                    InsertOffer(MyObjectBuilder_name + "/" + itemName, dif, SalePrice);
                }
                else TradeInfo.AppendLine($"[No update] Продажа {itemName} в кол-ве {dif}шт по цене {SalePrice}кр. уже размещена");
            }

            internal string GetOrdersAndOffers()
            {
                if (Block == null) return $"\n[Магазин не подключен]";
                _storeItems.Clear();
                TradeInfo.Clear();
                Block.GetPlayerStoreItems(_storeItems);
                TradeInfo.AppendLine($"\n{Block.CustomName} выложено {_storeItems.Count} товаров");
                foreach (var item in _storeItems) { TradeInfo.AppendLine($"\n{item.ItemId.SubtypeId} {item.Amount} шт по цене {item.PricePerUnit}"); }
                _storeItems.Clear();
                return TradeInfo.ToString();
            }

            internal void ClearAll()
            {
                if (Block == null) return;
                _storeItems.Clear();
                Block.GetPlayerStoreItems(_storeItems);
                foreach (var item in _storeItems) { Block.CancelStoreItem(item.Id); }
                Block.CustomData = $"Очистка магазина\n..удалено {_storeItems.Count} позиций";
                _storeItems.Clear();
            }

            void RemoveDuplicates(ref string TypeId, ref string SubtypeId, List<MyStoreQueryItem> storeItems, int price = 0)
            {
                if (price == 0) foreach (var item in storeItems) { if (item.ItemId.TypeIdString == TypeId && item.ItemId.SubtypeId == SubtypeId) RemoveItem(item.ItemId.SubtypeId, item.Amount, item.PricePerUnit, item.Id); }
                else foreach (var item in storeItems) { if (item.ItemId.TypeIdString == TypeId && item.ItemId.SubtypeId == SubtypeId && item.PricePerUnit == price) RemoveItem(item.ItemId.SubtypeId, item.Amount, item.PricePerUnit, item.Id); }
            }

            void RemoveItem(string SubtypeId, int Amount, int PricePerUnit, long Id)
            {
                TradeInfo.AppendLine($"[Remove] Товар {SubtypeId} {Amount} шт по цене {PricePerUnit} снят"); Block.CancelStoreItem(Id);
            }

            bool IsPosted(string TypeId, string SubtypeId, int price, int amount)
            {
                return _storeItems.Exists(x => x.ItemId.TypeIdString == TypeId && x.ItemId.SubtypeId == SubtypeId && x.PricePerUnit == price && x.Amount == amount);
            }

            void InsertOrder(string itemTypeSubtype, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemTypeSubtype, out definitionId))
                    Block.InsertOrder(new MyStoreItemDataSimple(definitionId, amount, price), out orderId);
                else
                    TradeInfo.AppendLine($"ОШИБКА! [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType");
                if (orderId != 0) TradeInfo.AppendLine($"[Create] Закупка [{itemTypeSubtype}] {amount}шт по цене {price}кр");
                else TradeInfo.AppendLine($"ОШИБКА! Закупка [{itemTypeSubtype}] не удалась! Проверьте имя товара");
            }

            void InsertOffer(string itemTypeSubtype, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemTypeSubtype, out definitionId))
                    Block.InsertOffer(new MyStoreItemDataSimple(definitionId, amount, price), out orderId);
                else
                    TradeInfo.AppendLine($"ОШИБКА! Объект [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType/ & Subtype");
                if (orderId != 0) TradeInfo.AppendLine($"[Create] Продажа [{itemTypeSubtype}] {amount}шт по цене {price}кр");
                else TradeInfo.AppendLine($"ОШИБКА! Продажа [{itemTypeSubtype}] не удалась! Проверьте имя и цену[{price}]");
            }
        }
    }
}
