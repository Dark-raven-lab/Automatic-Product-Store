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
        public class MyMarket
        {
            int _invenoryCounter = 0, _storeCount = 0; // Счётчик для инвентаря
            internal MyProductBlock StoreComp { get; private set; } // Подсистема магазина компонентов
            internal MyProductBlock StoreIng { get; private set; } // Подсистема магазина слитков
            internal MyProductBlock StoreOre { get; private set; } // Подсистема магазина руды
            internal MyProductBlock StoreTool { get; private set; } // Подсистема магазина инструментов
            internal Timer TimeCheckStore; // Таймер для выкладки товара в магазин
            List<IMyCargoContainer> _containers = new List<IMyCargoContainer>();

            string _infoComponents = "", _infoIngOre = "", _infoTools = "";
            internal string InfoComponents { get { return _infoComponents; } }
            internal string InfoIngOre { get { return _infoIngOre; } }
            internal string InfoTools { get { return _infoTools; } }

            internal string Warning { get; private set; } = "";

            /// <summary>
            /// Конструктор с поиском блока магазина
            /// </summary>
            /// <param name="TerminalSystem"></param>
            /// <param name="CubeGrid"></param>
            /// <param name="storeTags">Имя блока магазина</param>
            /// <param name="secondsForUpdate">Интервал обновления предложений в магазине</param>
            internal MyMarket(ref bool tradeComponents, ref bool tradeIngots, ref bool tradeOres, ref bool tradeTools, int secondsForUpdate = 3600)
            {
                StoreComp = new MyProductBlock(tradeComponents);
                StoreIng = new MyProductBlock(tradeIngots);
                StoreOre = new MyProductBlock(tradeOres);
                StoreTool = new MyProductBlock(tradeTools);
                TimeCheckStore = new Timer(secondsForUpdate, false);
            }

            /// <summary>
            /// Поиск и сохранение блоков магазина
            /// </summary>
            /// <param name="TerminalSystem"></param>
            /// <param name="CubeGrid"></param>
            /// <param name="storeTags">теги для магазинов</param>
            internal void GetStoreBlock(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid CubeGrid, ref string[] storeTags)
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
            internal void StoreUpdate(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, ref string group, ref string tagContainer, ref string tagExclude)
            {
                if (_containers.Count == 0 || _storeCount == 0) GetCargoBlocks(terminalSystem, Me, group, tagContainer, tagExclude);
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
            /// Обновление список объектов в инветаре без продажи. Возвращает true когда с контейнеров всё отсортировано.
            /// </summary>
            /// <returns></returns>
            internal bool UpdateCargoItems(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, ref string tagExclude)
            {
                if (_containers.Count == 0) GetCargoBlocks(terminalSystem, Me, tagExclude);
                return SortingContentsInventories();
            }

            /// <summary>
            /// Получение блоков контейнера для учёта объектов
            /// </summary>
            /// <param name="terminalSystem">GridTerminalSystem</param>
            /// <param name="Me">Me</param>
            /// <param name="group">(опционально)группа блоков контейнеров</param>
            /// <param name="tagContainer">(опционально) тег контейнеров</param>
            /// <param name="tagExclude">(опционально) исключение контейнеров при общем поиске</param>
            void GetCargoBlocks(IMyGridTerminalSystem terminalSystem, IMyProgrammableBlock Me, string group = "", string tagContainer = "", string tagExclude = "исключить")
            {
                Warning = "";
                _containers.Clear();
                if (group != string.Empty)
                {
                    var groupCargo = terminalSystem.GetBlockGroupWithName(group);
                    if (groupCargo != null) groupCargo.GetBlocksOfType<IMyCargoContainer>(_containers);
                }
                else if (tagContainer != string.Empty)
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
                            StoreComp.PlaceOfferingsAndSales(ref Components, "MyObjectBuilder_Component"); // Выкладываем товары в магазин компонентов
                            break;
                        case 1:
                            StoreIng.PlaceOfferingsAndSales(ref Ingots, "MyObjectBuilder_Ingot"); // Выкладываем товары в магазин слитков
                            break;
                        case 2:
                            StoreOre.PlaceOfferingsAndSales(ref Ores, "MyObjectBuilder_Ore"); // Выкладываем товары в магазин руд
                            break;
                        case 3:
                            StoreTool.PlaceOfferingsAndSales(ref Tools, "MyObjectBuilder_PhysicalGunObject");
                            StoreTool.PlaceOfferingsAndSales(ref Oxygen, "MyObjectBuilder_OxygenContainerObject", true);
                            StoreTool.PlaceOfferingsAndSales(ref Hydrogen, "MyObjectBuilder_GasContainerObject", true);
                            StoreTool.PlaceOfferingsAndSales(ref Ammo, "MyObjectBuilder_AmmoMagazine", true);
                            break;
                        default:
                            _storeCount = 0;
                            _containers.Clear();
                            TimeCheckStore.Start();
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
                if (_storeCount > 0) return true; // если сортировку уже делали и начали выкладывать товары
                if (_invenoryCounter == 0) SetAmountZero(); // Очищаем предыдущее кол-во
                if (_invenoryCounter < _containers.Count)
                {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    _containers[_invenoryCounter].GetInventory().GetItems(items);
                    SortingItems(ref items); // Сортировка найденных объектов
                    _invenoryCounter++; // Переходим к следующему инвентарю
                    return false;
                }
                else
                {
                    CreateListInfo(ref Components, "КОМПОНЕНТЫ", ref _infoComponents);
                    MergeIngOreListInfo();
                    CreateListInfo(ref Tools, "ИНСТРУМЕНТЫ", ref _infoTools);
                    AppendListInfo(ref Oxygen, "БАЛЛОНЫ", ref _infoTools);
                    AppendListInfo(ref Hydrogen, "", ref _infoTools);
                    AppendListInfo(ref Ammo, "БОЕПРИПАСЫ", ref _infoTools);
                    _invenoryCounter = 0;
                    return true;
                }
            }

            void SetAmountZero()
            {
                foreach (var item in Components) { item.Value.Amount = 0; }
                foreach (var item in Ingots) { item.Value.Amount = 0; }
                foreach (var item in Ores) { item.Value.Amount = 0; }
                foreach (var item in Tools) { item.Value.Amount = 0; }
                foreach (var item in Oxygen) { item.Value.Amount = 0; }
                foreach (var item in Hydrogen) { item.Value.Amount = 0; }
                foreach (var item in Ammo) { item.Value.Amount = 0; }
            }

            void SortingItems(ref List<MyInventoryItem> items)
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

            void MergeIngOreListInfo()
            {
                _infoIngOre = $"\n=== СЛИТКИ / РУДЫ ===";
                foreach (var item in Ingots)
                {
                    _infoIngOre += $"\n{TranslateName_components(item.Key)} : {item.Value.Amount} кг";
                    if (Ores.ContainsKey(item.Key)) _infoIngOre += $" ( {Math.Round((double)Ores[item.Key].Amount / 1000)} т руды )";
                }
            }
            void AppendListInfo(ref Dictionary<string, MyItem> DictItems, string header, ref string info)
            {
                if (header != "") info += $"\n=== {header} ===";
                WriteItemsListInfo(ref DictItems, ref info);
            }
            void CreateListInfo(ref Dictionary<string, MyItem> DictItems, string header, ref string info)
            {
                info = $"\n=== {header} ===";
                WriteItemsListInfo(ref DictItems, ref info);
            }
            void WriteItemsListInfo(ref Dictionary<string, MyItem> DictItems, ref string info)
            {
                foreach (var Item in DictItems)
                { info += $"\n{TranslateName_components(Item.Key)} : {Item.Value.Amount}"; }
            }

            string TranslateName_components(string name)
            {
                switch (name)
                {
                    case "BulletproofGlass": return "Бронированное стекло";
                    case "Canvas": return "Парашют";
                    case "Computer": return "Компьютеры";
                    case "Construction": return "Строительные компоненты";
                    case "Detector": return "Компоненты детектора";
                    case "Display": return "Экран";
                    case "Explosives": return "Взрывчатка";
                    case "Girder": return "Балка";
                    case "GravityGenerator": return "Компоненты грав. генератора";
                    case "InteriorPlate": return "Внутренняя пластина";
                    case "LargeTube": return "Большая стальная труба";
                    case "Medical": return "Медицинские компоненты";
                    case "MetalGrid": return "Компоненты решетки";
                    case "Motor": return "Мотор";
                    case "PowerCell": return "Энергоячейка";
                    case "RadioCommunication": return "Радиокомпоненты";
                    case "Reactor": return "Компоненты реактора";
                    case "SmallTube": return "Малая трубка";
                    case "SolarCell": return "Солнечная панель";
                    case "SteelPlate": return "Стальная пластина";
                    case "Superconductor": return "Сверхпроводник";
                    case "Thrust": return "Детали ионного двигателя";
                    case "ZoneChip": return "Ключ безопасности";
                    case "Cobalt": return "Кобальт";
                    case "Gold": return "Золото";
                    case "Stone": return "Камень";
                    case "Iron": return "Железо";
                    case "Magnesium": return "Магний";
                    case "Nickel": return "Никель";
                    case "Platinum": return "Платина";
                    case "Silicon": return "Кремний";
                    case "Silver": return "Серебро";
                    case "Uranium": return "Уран";
                    case "Ice": return "Лёд";
                    case "UltimateAutomaticRifleItem": return "Продвинутая винтовка";
                    case "AngleGrinder4Item": return "Элитная болгарка";
                    case "HandDrill4Item": return "Элитный ручной бур";
                    case "Welder4Item": return "Элитный сварщика";
                    case "RapidFireAutomaticRifleItem": return "Скорострельная автоматическая винтовка";
                    case "PreciseAutomaticRifleItem": return "Точная винтовка";
                    case "OxygenBottle": return "Кислородный баллон";
                    case "HydrogenBottle": return "Водородный баллон";
                    case "Missile200mm": return "Ракета 200мм";
                    case "NATO_25x184mm": return "Боеприпасы 25х184";
                    case "NATO_5p56x45mm": return "Магазин 5.56х45mm";
                    default: return name;
                }
            }
        }
    }
}
