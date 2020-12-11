using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class MyProductStore
        {
            internal IMyStoreBlock Block { get; private set; }

            /// <summary>
            /// Поиск необходимых блоков
            /// </summary>
            /// <param name="terminalSystem">IMyGridTerminalSystem</param>
            /// <param name="ThisCubeGrid">Грид, на котором установлен магазин</param>
            /// <param name="nameStore">Имя блока магазина</param>
            internal MyProductStore(IMyStoreBlock StoreBlock)
            {
                Block = StoreBlock;
            }
            internal MyProductStore(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string nameStore)
            {
                GetBlocks(TerminalSystem, CubeGrid, nameStore);
            }
            internal void GetBlocks(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid ThisCubeGrid, string nameStore)
            {
                Block = TerminalSystem.GetBlockWithName(nameStore) as IMyStoreBlock;
                if (Block == null)
                {
                    List<IMyStoreBlock> temp = new List<IMyStoreBlock>();
                    TerminalSystem.GetBlocksOfType(temp, x => x.CubeGrid == ThisCubeGrid);
                    foreach (var t in temp) { if (t.IsWorking) Block = t; break; }
                }
            }
            /// <summary>
            /// Размещение списка товаров в магазине
            /// </summary>
            /// <param name="ItemsForSaleBuy"></param>
            internal void OfferingsAndSales(Dictionary<string, MyItem> ItemsForSaleBuy, string MyObjectBuilder_name)
            {
                if (Block != null && Block.IsWorking)
                {
                    List<MyStoreQueryItem> storeItems = new List<MyStoreQueryItem>();// Товары в магазине
                    Block.CustomData = "";
                    // Получение списка из магазина
                    Block.GetPlayerStoreItems(storeItems);
                    // Проверка каждого товара на докупку/продажу
                    foreach (var Item in ItemsForSaleBuy)
                    {
                        // Если разрешена закупка этого товара и его мало на складе
                        if (Item.Value.AlowBuy && Item.Value.Amount < Item.Value.MaxAmount)
                        {
                            int dif = Item.Value.MaxAmount - Item.Value.Amount; // Кол-во недостающего товара
                            if (!OfferOrderIsPosted(Item.Key, Item.Value.BuyPrice, dif, storeItems)) // Если такое объявление еще не создавали
                            {
                                FindAndDelete(Item.Key, storeItems); // Удаляем похожие объявления
                                InsertOrder("MyObjectBuilder_Component/" + Item.Key, dif, Item.Value.BuyPrice); // Создаём ордер на закупку
                            }
                            else Block.CustomData += $"\n[No update] Закупка {Item.Key}:{dif} шт по цене {Item.Value.BuyPrice} кр. уже размещена";
                        }
                        // Если разрешена продажа этого товара и его много на складе
                        else if (Item.Value.AlowSale && Item.Value.Amount > Item.Value.MaxAmount)
                        {
                            int dif = Item.Value.Amount - Item.Value.MaxAmount; // Кол-во лишнего товара
                            if (!OfferOrderIsPosted(Item.Key, Item.Value.SalePrice, dif, storeItems)) // Если такое объявление еще не создавали
                            {
                                FindAndDelete(Item.Key, storeItems); // Удаляем похожие объявления
                                InsertOffer("MyObjectBuilder_Component/" + Item.Key, dif, Item.Value.SalePrice); // Создаём ордер на продажу
                            }
                            else Block.CustomData += $"\n[No update] Продажа {Item.Key}:{dif} шт по цене {Item.Value.BuyPrice} кр. уже размещена";
                        }
                    }
                    storeItems.Clear();
                }
            }
            /// <summary>
            /// Поиск и удаление объявления о продаже из магазина
            /// </summary>
            /// <param name="SubtypeId">Название товара, ввиде "SteelPlate"</param>
            /// <param name="storeItems">Список объявлений из магазина</param>
            internal void FindAndDelete(string SubtypeId, List<MyStoreQueryItem> storeItems)
            {
                foreach (var item in storeItems) { if (item.ItemId.SubtypeId == SubtypeId) { Block.CustomData += $"\n[Remove] Товар {item.ItemId.SubtypeId} снят"; Block.CancelStoreItem(item.Id); } }
            }
            /// <summary>
            /// Определяет содержится ли такое объявление в списке
            /// </summary>
            /// <param name="SubtypeId">Название товара, ввиде "SteelPlate"</param>
            /// <param name="price">Цена</param>
            /// <param name="amount">Колличество</param>
            /// <param name="storeItems">Список объявлений из магазина</param>
            /// <returns>Возвраает true, если объявление найдено</returns>
            internal bool OfferOrderIsPosted(string SubtypeId, int price, int amount, List<MyStoreQueryItem> storeItems)
            {
                return storeItems.Exists(x => x.ItemId.SubtypeId == SubtypeId && x.PricePerUnit == price && x.Amount == amount);
            }
            /// <summary>
            /// Создаёт предложение для закупки в блоке магазина
            /// </summary>
            /// <param name="itemType">Тип объекта</param>
            /// <param name="amount">Колличество</param>
            /// <param name="price">Цена</param>
            /// <param name="orderId">ID заказа в магазине</param>
            internal void InsertOrder(string itemType, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemType, out definitionId))
                {
                    Block.InsertOrder(new MyStoreItemDataSimple(definitionId, amount, price), out orderId); // Мы закупаем
                }
                else Block.CustomData += $"\nОШИБКА [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType";
                if (orderId != 0) Block.CustomData += $"\n[Create] Создан заказ [{itemType}] {amount} шт по цене {price}";
                else Block.CustomData += $"\nОШИБКА! Заказ [{itemType}] не создан. Проверьте имя товара";
            }
            /// <summary>
            /// Создаёт предложение для продажи в блоке магазина
            /// </summary>
            /// <param name="itemType">Тип объекта</param>
            /// <param name="amount">Колличество</param>
            /// <param name="price">Цена</param>
            internal void InsertOffer(string itemType, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemType, out definitionId))
                {
                    Block.InsertOffer(new MyStoreItemDataSimple(definitionId, amount, price), out orderId); // Мы закупаем
                }
                else Block.CustomData += $"\n[Error] [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType";
                if (orderId != 0) Block.CustomData += $"\n[Create] Создано предложение продажи [{itemType}] {amount} шт по цене {price}";
                else Block.CustomData += $"\n[Error]! Заказ [{itemType}] не создан.Проверьте имя и цену[{price}]";
            }
            /// <summary>
            /// Получить список заказов и предложений из магазина
            /// </summary>
            /// <returns>Возвращает заказы и предложения в текстовом виде</returns>
            internal string GetOrdersAndOffers()
            {
                List<MyStoreQueryItem> storeItems = new List<MyStoreQueryItem>();// Товары в магазине
                string txt;
                Block.GetPlayerStoreItems(storeItems);
                txt = $"Товаров в магазине {storeItems.Count}";
                foreach (var item in storeItems)
                {
                    txt += $"\n{item.ItemId.SubtypeId} {item.Amount} шт по цене {item.PricePerUnit}";
                }
                return txt;
            }
            /// <summary>
            /// Очистка всех заказов и предложений в магазине
            /// </summary>
            internal void ClearAll()
            {
                List<MyStoreQueryItem> storeItems = new List<MyStoreQueryItem>();// Товары в магазине
                Block.CustomData = "Очистка магазина";
                Block.GetPlayerStoreItems(storeItems);
                foreach (var item in storeItems) { Block.CancelStoreItem(item.Id); }
                Block.CustomData += $"\n..удалено {storeItems.Count} позиций";
                storeItems.Clear();
            }
        }
    }
}
