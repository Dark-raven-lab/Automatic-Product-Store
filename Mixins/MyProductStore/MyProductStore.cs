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
        public class MyProductStore
        {
            List<MyStoreQueryItem> _storeItems = new List<MyStoreQueryItem>();// Список для товаров в магазине
            /// <summary>
            /// Игровой блок магазина
            /// </summary>
            internal IMyStoreBlock Block { get; set; } = null;
            /// <summary>
            /// Разрешить работу магазина или нет
            /// </summary>
            internal bool Trading { get; set; } = true;
            internal StringBuilder TradeInfo { get; private set; } = new StringBuilder();
            internal MyProductStore(bool trading)
            {
                Trading = trading;
            }
            internal MyProductStore(IMyStoreBlock StoreBlock)
            {
                Block = StoreBlock;
            }
            internal MyProductStore(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string nameStore)
            {
                GetBlocks(TerminalSystem, CubeGrid, nameStore);
            }

            /// <summary>
            /// Поиск необходимых блоков
            /// </summary>
            /// <param name="TerminalSystem">IMyGridTerminalSystem</param>
            /// <param name="ThisCubeGrid">Грид, на котором установлен магазин</param>
            /// <param name="tagStoreName">Тег блока магазина</param>
            internal void GetBlocks(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid ThisCubeGrid, string tagStoreName)
            {
                List<IMyStoreBlock> temp = new List<IMyStoreBlock>();
                TerminalSystem.GetBlocksOfType(temp, x => x.CubeGrid == ThisCubeGrid && x.CustomName.ToLower().Contains(tagStoreName.ToLower()));
                foreach (var t in temp) { Block = t; break; }
            }

            /// <summary>
            /// Размещение списка товаров в магазине
            /// </summary>
            /// <param name="ItemsForSaleBuy">Список объектов для размещения</param>
            /// <param name="MyObjectBuilder_name">Текстовое представление типа объекта</param>
            /// <param name="append">Дописать или заменить информацию о работе магазина в "мои данные" блока</param>
            internal void PlaceOfferingsAndSales(Dictionary<string, MyItem> ItemsForSaleBuy, string MyObjectBuilder_name, bool append = false)
            {
                if (!Trading || Block == null || !Block.IsWorking) return;
                _storeItems.Clear();// Очищаем список для товаров в магазине
                if (!append) { TradeInfo.Clear(); TradeInfo.AppendLine($"Выкладка товаров осуществленна {DateTime.Now:g}"); }
                Block.GetPlayerStoreItems(_storeItems); // Получение списка из магазина
                foreach (var Item in ItemsForSaleBuy) // Проверка каждого товара для выкладки на докупку/продажу
                {
                    if (Item.Value.Mode == StoreMode.Storage) // Режим поддержки на складе
                    {
                        if (Item.Value.AlowBuy && Item.Value.Amount < Item.Value.MaxAmount) // Если разрешена закупка этого товара и его мало на складе
                            CreateOrder(ref MyObjectBuilder_name, Item.Key, Item.Value.BuyPrice, Item.Value.MaxAmount - Item.Value.Amount, true);
                        else if (Item.Value.AlowSale && Item.Value.Amount > Item.Value.MaxAmount) // Если разрешена продажа этого товара и его много на складе
                            CreateOffer(ref MyObjectBuilder_name, Item.Key, Item.Value.SalePrice, Item.Value.Amount - Item.Value.MaxAmount, true);
                    }
                    else if (Item.Value.Mode == StoreMode.Shop) // Режим магазина
                    {
                        if (Item.Value.AlowBuy && Item.Value.Amount < Item.Value.MaxAmount) // Если разрешена закупка - закупаем недостаток
                            CreateOrder(ref MyObjectBuilder_name, Item.Key, Item.Value.BuyPrice, Item.Value.MaxAmount - Item.Value.Amount);
                        if (Item.Value.AlowSale && Item.Value.Amount > 0) // Если разрешена продажа - продаём что есть на складах
                            CreateOffer(ref MyObjectBuilder_name, Item.Key, Item.Value.SalePrice, Item.Value.Amount);
                    }
                }
                if (append) Block.CustomData += TradeInfo.ToString();
                else Block.CustomData = TradeInfo.ToString();
                _storeItems.Clear();
                TradeInfo.Clear();
            }

            /// <summary>
            /// Создание закупки с валидацией и удалением дубликатов
            /// </summary>
            /// <param name="MyObjectBuilder_name"></param>
            /// <param name="itemName">Названия товара</param>
            /// <param name="BuyPrice">Цена закупки</param>
            /// <param name="dif">Кол-во</param>
            /// <param name="agressiveRemove">Удалить все дубликаты в продажах и закупках</param>
            void CreateOrder(ref string MyObjectBuilder_name, string itemName, int BuyPrice, int dif, bool agressiveRemove = false)
            {
                if (!IsPosted(MyObjectBuilder_name, itemName, BuyPrice, dif, _storeItems))  // Если такое объявление еще не создавали
                {
                    if (agressiveRemove) RemoveDuplicates(MyObjectBuilder_name, itemName, _storeItems); // Удаляем все с таким же именем
                    else RemoveDuplicates(MyObjectBuilder_name, itemName, _storeItems, BuyPrice); // Удаляем все с такой же ценой
                    InsertOrder(MyObjectBuilder_name + "/" + itemName, dif, BuyPrice); // Создаём ордер на закупку
                }
                else TradeInfo.AppendLine($"[No update] Закупка {itemName} в кол-ве {dif}шт по цене {BuyPrice}кр. уже размещена");
            }

            /// <summary>
            /// Создание продажи с валидацией и удалением дубликатов
            /// </summary>
            /// <param name="MyObjectBuilder_name"></param>
            /// <param name="itemName">Названия товара</param>
            /// <param name="SalePrice">Цена продажи</param>
            /// <param name="dif">Кол-во</param>
            /// <param name="agressiveRemove">Удалить все дубликаты в продажах и закупках</param>
            void CreateOffer(ref string MyObjectBuilder_name, string itemName, int SalePrice, int dif, bool agressiveRemove = false)
            {
                if (!IsPosted(MyObjectBuilder_name, itemName, SalePrice, dif, _storeItems)) // Если такое объявление еще не создавали
                {
                    if (agressiveRemove) RemoveDuplicates(MyObjectBuilder_name, itemName, _storeItems); // Удаляем все с таким же именем
                    else RemoveDuplicates(MyObjectBuilder_name, itemName, _storeItems, SalePrice); // Удаляем все с такой же ценой
                    InsertOffer(MyObjectBuilder_name + "/" + itemName, dif, SalePrice); // Создаём ордер на продажу
                }
                else TradeInfo.AppendLine($"[No update] Продажа {itemName} в кол-ве {dif}шт по цене {SalePrice}кр. уже размещена");
            }

            /// <summary>
            /// Получить список заказов и предложений из магазина
            /// </summary>
            /// <returns>Возвращает заказы и предложения в текстовом виде</returns>
            internal string GetOrdersAndOffers()
            {
                _storeItems.Clear();
                TradeInfo.Clear();
                Block.GetPlayerStoreItems(_storeItems);
                TradeInfo.AppendLine($"\n{Block.CustomName} выложено {_storeItems.Count} товаров");
                foreach (var item in _storeItems) { TradeInfo.AppendLine($"\n{item.ItemId.SubtypeId} {item.Amount} шт по цене {item.PricePerUnit}"); }
                _storeItems.Clear();
                return TradeInfo.ToString();
            }

            /// <summary>
            /// Очистка всех заказов и предложений в магазине
            /// </summary>
            internal void ClearAll()
            {
                _storeItems.Clear();// Товары в магазине
                Block.GetPlayerStoreItems(_storeItems);
                foreach (var item in _storeItems) { Block.CancelStoreItem(item.Id); }
                Block.CustomData = $"Очистка магазина\n..удалено {_storeItems.Count} позиций";
                _storeItems.Clear();
            }

            /// <summary>
            /// Поиск и удаление объявления о продаже из магазина
            /// </summary>
            /// <param name="TypeId">Тип объекта товара "MyObjectBuilder_Component"</param>
            /// <param name="SubtypeId">Название товара "BulletproofGlass"</param>
            /// <param name="storeItems">Список объявлений из магазина</param>
            /// <param name="price">Цена товара. 0 для удаления ВСЕХ дубликатов</param>
            void RemoveDuplicates(string TypeId, string SubtypeId, List<MyStoreQueryItem> storeItems, int price = 0)
            {
                if (price == 0) foreach (var item in storeItems) {if (item.ItemId.TypeIdString == TypeId && item.ItemId.SubtypeId == SubtypeId) RemoveItem(item.ItemId.SubtypeId, item.Amount, item.PricePerUnit, item.Id); }
                else foreach (var item in storeItems) { if (item.ItemId.TypeIdString == TypeId && item.ItemId.SubtypeId == SubtypeId && item.PricePerUnit == price) RemoveItem(item.ItemId.SubtypeId, item.Amount, item.PricePerUnit, item.Id); }
            }

            void RemoveItem(string SubtypeId, int Amount, int PricePerUnit, long Id)
            {
                TradeInfo.AppendLine($"[Remove] Товар {SubtypeId} {Amount} шт по цене {PricePerUnit} снят"); Block.CancelStoreItem(Id);
            }

            /// <summary>
            /// Определяет содержится ли такое объявление в списке
            /// </summary>
            /// <param name="TypeId">Тип объекта товара "MyObjectBuilder_Component"</param>
            /// <param name="SubtypeId">Название товара "SteelPlate"</param>
            /// <param name="price">Цена</param>
            /// <param name="amount">Колличество</param>
            /// <param name="storeItems">Список объявлений из магазина</param>
            /// <returns>Возвраает true, если объявление найдено</returns>
            bool IsPosted(string TypeId, string SubtypeId, int price, int amount, List<MyStoreQueryItem> storeItems)
            {
                return storeItems.Exists(x => x.ItemId.TypeIdString == TypeId && x.ItemId.SubtypeId == SubtypeId && x.PricePerUnit == price && x.Amount == amount);
            }

            /// <summary>
            /// Создаёт предложение для закупки в блоке магазина
            /// </summary>
            /// <param name="itemTypeSubtype">Тип объекта</param>
            /// <param name="amount">Колличество</param>
            /// <param name="price">Цена</param>
            /// <param name="orderId">ID заказа в магазине</param>
            void InsertOrder(string itemTypeSubtype, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemTypeSubtype, out definitionId))
                    Block.InsertOrder(new MyStoreItemDataSimple(definitionId, amount, price), out orderId); // Мы закупаем
                else
                    TradeInfo.AppendLine($"ОШИБКА! [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType");
                if (orderId != 0) TradeInfo.AppendLine($"[Create] Закупка [{itemTypeSubtype}] {amount}шт по цене {price}кр");
                else TradeInfo.AppendLine($"ОШИБКА! Закупка [{itemTypeSubtype}] не удалась! Проверьте имя товара");
            }

            /// <summary>
            /// Создаёт предложение для продажи в блоке магазина
            /// </summary>
            /// <param name="itemTypeSubtype">Тип объекта</param>
            /// <param name="amount">Колличество</param>
            /// <param name="price">Цена</param>
            void InsertOffer(string itemTypeSubtype, int amount, int price)
            {
                long orderId = 0;
                MyDefinitionId definitionId;
                if (MyDefinitionId.TryParse(itemTypeSubtype, out definitionId))
                    Block.InsertOffer(new MyStoreItemDataSimple(definitionId, amount, price), out orderId); // Мы закупаем
                else
                    TradeInfo.AppendLine($"ОШИБКА! Объект [MyDefinitionId] НЕ создан. Вероятно указан не правильный itemType/ & Subtype");
                if (orderId != 0) TradeInfo.AppendLine($"[Create] Продажа [{itemTypeSubtype}] {amount}шт по цене {price}кр");
                else TradeInfo.AppendLine($"ОШИБКА! Продажа [{itemTypeSubtype}] не удалась! Проверьте имя и цену[{price}]");
            }
        }
    }
}
