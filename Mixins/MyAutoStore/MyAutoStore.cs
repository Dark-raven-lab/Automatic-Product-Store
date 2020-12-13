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
        public class MyAutoStore
        {
            int _invenoryCounter = 0, _storeCount = 0; // Счётчик для инвентаря
            internal MyProductStore StoreComp { get; private set; } // Подсистема магазина компонентов
            internal MyProductStore StoreIng { get; private set; } // Подсистема магазина слитков
            internal MyProductStore StoreOre { get; private set; } // Подсистема магазина руды
            internal MyProductStore StoreTool { get; private set; } // Подсистема магазина инструментов
            internal Timer TimeCheckStore; // Таймер для выкладки товара в магазин
            List<IMyCargoContainer> _containers = new List<IMyCargoContainer>();

            string _infoComponents = "", _infoIngots = "", _infoOres = ""/*, _infoTools = ""*/;
            internal string ComponentsInfo { get { return _infoComponents; } }
            internal string InfoIngots { get { return _infoIngots; } } 
            internal string InfoOres { get { return _infoOres; } }
            //internal string InfoTools { get { return _infoTools; } }

            internal string Warning { get; private set; } = "";
            
            /// <summary>
            /// Конструктор с поиском блока магазина
            /// </summary>
            /// <param name="TerminalSystem"></param>
            /// <param name="CubeGrid"></param>
            /// <param name="storeTags">Имя блока магазина</param>
            /// <param name="secondsForUpdate">Интервал обновления предложений в магазине</param>
            internal MyAutoStore(bool tradeComponents, bool tradeIngots, bool tradeOres, bool tradeTools, int secondsForUpdate = 3600)
            {
                StoreComp = new MyProductStore(tradeComponents);
                StoreIng = new MyProductStore(tradeIngots);
                StoreOre = new MyProductStore(tradeOres);
                StoreTool = new MyProductStore(tradeTools);
                TimeCheckStore = new Timer(secondsForUpdate, false);
            }
            
            /// <summary>
            /// Поиск и сохранение блоков магазина
            /// </summary>
            /// <param name="TerminalSystem"></param>
            /// <param name="CubeGrid"></param>
            /// <param name="storeTags">теги для магазинов</param>
            internal void GetStoreBlock(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string[] storeTags)
            {
                List<IMyStoreBlock> AllStoreBlock = new List<IMyStoreBlock>();
                TerminalSystem.GetBlocksOfType(AllStoreBlock, x => x.CubeGrid == CubeGrid);
                foreach (var thisBlock in AllStoreBlock)
                {
                    if (thisBlock.CustomName.ToLower().Contains(storeTags[0].ToLower())) StoreComp.Block = thisBlock;
                    else if (thisBlock.CustomName.ToLower().Contains(storeTags[1].ToLower())) StoreIng.Block = thisBlock;
                    else if (thisBlock.CustomName.ToLower().Contains(storeTags[2].ToLower())) StoreOre.Block = thisBlock;
                    else if (thisBlock.CustomName.ToLower().Contains(storeTags[3].ToLower())) StoreTool.Block = thisBlock;
                }
            }

            /// <summary>
            /// Обновление торговых предложений
            /// </summary>
            /// <param name="terminalSystem">Интерфейс для поиска блоков</param>
            /// <param name="Me">Программный блок</param>
            internal void StoreUpdate(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, string group, string tagContainer, string tagExclude = "исключить")
            {
                if (_containers.Count == 0) GetCargoBlocks(terminalSystem, Me, group, tagContainer, tagExclude);
                PlaceOffers();
            }
            
            /// <summary>
            /// Обновление торговых предложений
            /// </summary>
            /// <param name="containers">Список контейнеров</param>
            internal void StoreUpdate(List<IMyCargoContainer> containers)
            {
                _containers = containers;
                PlaceOffers();
            }

            /// <summary>
            /// Получение блоков контейнера для учёта объектов
            /// </summary>
            /// <param name="terminalSystem">GridTerminalSystem</param>
            /// <param name="Me">Me</param>
            /// <param name="group">(опционально)группа блоков контейнеров</param>
            /// <param name="tagContainer">(опционально) тег контейнеров</param>
            /// <param name="tagExclude">(опционально) исключение контейнеров при общем поиске</param>
            void GetCargoBlocks(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, string group, string tagContainer, string tagExclude = "исключить")
            {
                if (group != string.Empty)
                {
                    var groupCargo = terminalSystem.GetBlockGroupWithName(group);
                    if (groupCargo != null) groupCargo.GetBlocksOfType<IMyCargoContainer>(_containers);
                } else if (tagContainer != string.Empty)
                    terminalSystem.GetBlocksOfType(_containers, x => x.CubeGrid == Me.CubeGrid && x.CustomName.ToLower().Contains(tagContainer.ToLower()));
                if (_containers.Count == 0) terminalSystem.GetBlocksOfType(_containers, x => x.CubeGrid == Me.CubeGrid && !x.CustomName.ToLower().Contains(tagExclude.ToLower()));
            }
            
            /// <summary>
            /// Размещение товаров в магазине(-ах)
            /// </summary>
            void PlaceOffers()
            {
                if (_containers.Count == 0) { Warning = "Размещение отменено. Нет конейнеров"; return; }
                if (SortingContentsInventories()) // Ждём окончания сортировки объектов
                {
                    switch (_storeCount)
                    {
                        case 0:
                            StoreComp.PlaceOfferingsAndSales(Components, "MyObjectBuilder_Component"); // Выкладываем товары в магазин компонентов
                            break;
                        case 1:
                            StoreIng.PlaceOfferingsAndSales(Ingots, "MyObjectBuilder_Ingot"); // Выкладываем товары в магазин слитков
                            break;
                        case 2:
                            StoreOre.PlaceOfferingsAndSales(Ores, "MyObjectBuilder_Ore"); // Выкладываем товары в магазин руд
                            break;
                        case 3:
                            StoreTool.PlaceOfferingsAndSales(Tools, "MyObjectBuilder_PhysicalGunObject");
                            StoreTool.PlaceOfferingsAndSales(Oxygen, "MyObjectBuilder_OxygenContainerObject");
                            StoreTool.PlaceOfferingsAndSales(Hydrogen, "MyObjectBuilder_GasContainerObject");
                            StoreTool.PlaceOfferingsAndSales(Ammo, "MyObjectBuilder_AmmoMagazine");
                            break;
                        default:
                            _storeCount = 0;
                            _containers.Clear(); // Чистим список контейнеров
                            TimeCheckStore.Start(); // Запускаем таймер
                            return;
                    }
                    _storeCount++;
                }
            }

            /// <summary>
            /// Выполняет поиск и сортировку в инвентаре
            /// </summary>
            /// <returns>Возвращает true при окончании сортировки</returns>
            bool SortingContentsInventories()
            {
                if (_storeCount != 0) return true;
                if (_invenoryCounter == 0) foreach (var component in Components) { component.Value.Amount = 0; }
                if (_invenoryCounter < _containers.Count)
                {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    //_containers[_invenoryCounter].GetInventory().GetItems(items, x => x.Type.TypeId.ToLower().Contains("component"));
                    _containers[_invenoryCounter].GetInventory().GetItems(items);
                    SortingItems(items); // Сортировка найденных объектов
                    _invenoryCounter++; // Переходим к следующему инвентарю
                    return false;
                }
                else
                {
                    WriteItemsInfo(Components, "КОМПОНЕНТЫ", out _infoComponents);
                    WriteItemsInfo(Ingots, "СЛИТКИ", out _infoIngots);
                    WriteItemsInfo(Ores, "РУДЫ", out _infoOres);
                    //WriteItemsInfo(Tools, "ИНСТРУМЕНТЫ", out _infoTools);
                    _invenoryCounter = 0;
                    return true;
                }
            }
            
            void SortingItems(List<MyInventoryItem> items)
            {
                foreach (var item in items)
                {
                    if (item.Type.TypeId == "MyObjectBuilder_Component" && Components.ContainsKey(item.Type.SubtypeId)) // Если имя сходится с тем что в словаре
                        Components[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_Ingot" && Ingots.ContainsKey(item.Type.SubtypeId))
                        Ingots[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_Ore" && Ores.ContainsKey(item.Type.SubtypeId))
                        Ores[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_PhysicalGunObject" && Tools.ContainsKey(item.Type.SubtypeId))
                        Tools[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_OxygenContainerObject" && Oxygen.ContainsKey(item.Type.SubtypeId))
                        Oxygen[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_GasContainerObject" && Hydrogen.ContainsKey(item.Type.SubtypeId))
                        Hydrogen[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_AmmoMagazine" && Ammo.ContainsKey(item.Type.SubtypeId))
                        Ammo[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else
                        Warning += $"\n[{item.Type.TypeId}/{item.Type.SubtypeId}] отсутствует в словарях";
                }
            }
            
            void WriteItemsInfo(Dictionary<string, MyItem> DictItems, string header, out string info)
            {
                info = $"\n=== {header} ===";
                foreach (var Item in DictItems)
                { info += $"\n{TranslateName_components(Item.Key)} : {Item.Value.Amount}"; }
            }

            string TranslateName_components(string name)
            {
                string newname = "";
                switch (name)
                {
                    case "BulletproofGlass": newname = "Бронированное стекло"; break;
                    case "Canvas": newname = "Парашют"; break;
                    case "Computer": newname = "Компьютеры"; break;
                    case "Construction": newname = "Строительные компоненты"; break;
                    case "Detector": newname = "Компоненты детектора"; break;
                    case "Display": newname = "Экран"; break;
                    case "Explosives": newname = "Взрывчатка"; break;
                    case "Girder": newname = "Балка"; break;
                    case "GravityGenerator": newname = "Компоненты грав. генератора"; break;
                    case "InteriorPlate": newname = "Внутренняя пластина"; break;
                    case "LargeTube": newname = "Большая стальная труба"; break;
                    case "Medical": newname = "Медицинские компоненты"; break;
                    case "MetalGrid": newname = "Компоненты решетки"; break;
                    case "Motor": newname = "Мотор"; break;
                    case "PowerCell": newname = "Энергоячейка"; break;
                    case "RadioCommunication": newname = "Радиокомпоненты"; break;
                    case "Reactor": newname = "Компоненты реактора"; break;
                    case "SmallTube": newname = "Малая трубка"; break;
                    case "SolarCell": newname = "Солнечная панель"; break;
                    case "SteelPlate": newname = "Стальная пластина"; break;
                    case "Superconductor": newname = "Сверхпроводник"; break;
                    case "Thrust": newname = "Детали ионного двигателя"; break;
                    case "ZoneChip": newname = "Ключ безопасности"; break;
                    default: newname = name; break;
                }
                return newname;
            }
        }
    }
}
