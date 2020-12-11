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
            int _invenoryCounter = 0; // Счётчик для инвентаря
            internal MyProductStore StoreComp; // Подсистема магазина компонентов
            internal MyProductStore StoreIng; // Подсистема магазина слитков
            internal MyProductStore StoreOre; // Подсистема магазина руды
            internal MyProductStore StoreTool; // Подсистема магазина инструментов
            internal Timer TimeCheckStore; // Таймер для выкладки товара в магазин
            List<IMyCargoContainer> _containers = new List<IMyCargoContainer>();

            string _infoComponents, _infoIngots, _infoOres, _infoTools;
            internal string ComponentsInfo { get { return _infoComponents; } }
            internal string InfoIngots { get { return _infoIngots; } } 
            internal string InfoOres { get { return _infoOres; } }
            internal string InfoTools { get { return _infoTools; } }

            internal string Warning { get; private set; }
            /// <summary>
            /// Конструктор с поиском блока магазина
            /// </summary>
            /// <param name="TerminalSystem"></param>
            /// <param name="CubeGrid"></param>
            /// <param name="storeName">Имя блока магазина</param>
            /// <param name="secondsForUpdate">Интервал обновления предложений в магазине</param>
            internal MyAutoStore(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string storeName, int secondsForUpdate = 3600)
            {
                StoreComp = new MyProductStore(TerminalSystem, CubeGrid, storeName);
                StoreIng = new MyProductStore(TerminalSystem, CubeGrid, "слитки");
                StoreOre = new MyProductStore(TerminalSystem, CubeGrid, "руды");
                StoreTool = new MyProductStore(TerminalSystem, CubeGrid, "инструменты");
                TimeCheckStore = new Timer(secondsForUpdate, false);
            }
            internal MyAutoStore(IMyStoreBlock Store, int secondsForUpdate = 3600)
            {
                this.StoreComp = new MyProductStore(Store);
                // Добавить магазины в конструктор!
                TimeCheckStore = new Timer(secondsForUpdate, false);
            }
            internal void GetStoreBlock(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, string storeName = "")
            {
                StoreComp.GetBlocks(TerminalSystem, CubeGrid, storeName);
            }
            /// <summary>
            /// Возвращает true, если настало время обновлять торговые предложения
            /// </summary>
            internal bool TimeToUpgrade(){ { return TimeCheckStore.IsOut(); } }
            /// <summary>
            /// Обновление торговых предложений
            /// </summary>
            /// <param name="terminalSystem">Интерфейс для поиска блоков</param>
            /// <param name="Me">Программный блок</param>
            internal void StoreUpdate(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, string group, string tagContainer, string tagExclude = "исключить")
            {
                GetCargoBlocks(terminalSystem, Me, group, tagContainer, tagExclude);
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
            
            void PlaceOffers()
            {
                if (_containers.Count == 0) return;
                if (SortingContentsInventories()) // Ждём окончания сортировки объектов
                {
                    StoreComp.OfferingsAndSales(Components); // Выкладываем товары в магазин компонентов
                    // тут добавляем другие магазины
                    _containers.Clear(); // Чистим список контейнеров
                    TimeCheckStore.Start(); // Запускаем таймер
                }
            }
            /// <summary>
            /// Выполняет поиск и сортировку в инвентаре
            /// </summary>
            /// <returns>Возвращает true при окончании сортировки</returns>
            bool SortingContentsInventories()
            {
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
                    WriteItemsInfo(Ingots, "СЛИТКИ", out _infoComponents);
                    WriteItemsInfo(Ores, "РУДЫ", out _infoComponents);
                    WriteItemsInfo(Tools, "ИНСТРУМЕНТЫ", out _infoComponents);
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
                    else if (item.Type.TypeId == "MyObjectBuilder_Tool" && Tools.ContainsKey(item.Type.SubtypeId)) // Название не факт
                        Tools[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else
                        Warning += $"\nХрень [{item.Type.TypeId}] отсутствует в словарях";
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
