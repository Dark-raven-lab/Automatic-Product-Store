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
            internal MyProductStore StoreComp; // Подсистема магазина
            internal Timer TimeCheckStore; // Таймер для выкладки товара в магазин
            List<IMyCargoContainer> _containers = new List<IMyCargoContainer>();
          
            internal string ComponentsInfo { get; private set; } = "";
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
                TimeCheckStore = new Timer(secondsForUpdate, false);
            }
            internal MyAutoStore(IMyStoreBlock Store, int secondsForUpdate = 3600)
            {
                this.StoreComp = new MyProductStore(Store);
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
                    StoreComp.OfferingsAndSales(Components); // Выкладываем товары в магазин
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
                    _containers[_invenoryCounter].GetInventory().GetItems(items, x => x.Type.TypeId.ToLower().Contains("component"));
                    SortingItems(items); // Сортировка найденных объектов
                    _invenoryCounter++; // Переходим к следующему инвентарю
                    return false;
                }
                else
                {
                    ComponentsInfo = "\n=== КОМПОНЕНТЫ ===";
                    foreach (var component in Components)
                    { ComponentsInfo += $"\n{TranslateName_components(component.Key)} : {component.Value.Amount}"; }
                    _invenoryCounter = 0;
                    return true;
                }
            }
            void SortingItems(List<MyInventoryItem> items)
            {
                foreach (var item in items)
                {
                    if (Components.ContainsKey(item.Type.SubtypeId))
                    {
                        Components[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    }
                    else ComponentsInfo += $"\nКомпонент [{item.Type.SubtypeId}] отсутствует в изначальном словаре";
                }
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
