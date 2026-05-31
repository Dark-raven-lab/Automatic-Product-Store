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
        public class MyAutoStore
        {
            int _invenoryCounter = 0, _storeCount = 0;
            internal MyProductBlock StoreComp { get; private set; }
            internal MyProductBlock StoreIng { get; private set; }
            internal MyProductBlock StoreOre { get; private set; }
            internal MyProductBlock StoreTool { get; private set; }
            internal MyProductBlock StoreConsumables { get; private set; }
            internal MyProductBlock StoreSeeds { get; private set; }
            internal Timer TimeCheckStore;
            List<IMyCargoContainer> _containers = new List<IMyCargoContainer>();
            int _globalMarkupPercent;

            string _infoComponents = "", _infoIngOre = "", _infoTools = "", _infoConsumables = "", _infoSeeds = "";
            internal List<IMyCargoContainer> Containers { get { return _containers; } }
            internal string InfoComponents { get { return _infoComponents; } }
            internal string InfoIngOre { get { return _infoIngOre; } }
            internal string InfoTools { get { return _infoTools; } }
            internal string InfoConsumables { get { return _infoConsumables; } }
            internal string InfoSeeds { get { return _infoSeeds; } }
            internal string Warning { get; private set; } = "";

            internal MyAutoStore(ref bool tradeComponents, ref bool tradeIngots, ref bool tradeOres, ref bool tradeTools,
                                 ref bool tradeConsumables, ref bool tradeSeeds, int globalMarkupPercent, int secondsForUpdate = 3600)
            {
                StoreComp = new MyProductBlock(tradeComponents);
                StoreIng = new MyProductBlock(tradeIngots);
                StoreOre = new MyProductBlock(tradeOres);
                StoreTool = new MyProductBlock(tradeTools);
                StoreConsumables = new MyProductBlock(tradeConsumables);
                StoreSeeds = new MyProductBlock(tradeSeeds);
                TimeCheckStore = new Timer(secondsForUpdate, false);
                _globalMarkupPercent = globalMarkupPercent;
            }

            internal void GetStoreBlock(IMyGridTerminalSystem TerminalSystem, IMyCubeGrid cubeGrid, ref string[] storeTags)
            {
                List<IMyStoreBlock> AllStoreBlock = new List<IMyStoreBlock>();
                TerminalSystem.GetBlocksOfType(AllStoreBlock, x => x.CubeGrid == cubeGrid);
                foreach (var thisBlock in AllStoreBlock)
                {
                    string nameLower = thisBlock.CustomName.ToLower();
                    if (nameLower.Contains(storeTags[0].ToLower())) StoreComp.Block = thisBlock;
                    if (nameLower.Contains(storeTags[1].ToLower())) StoreIng.Block = thisBlock;
                    if (nameLower.Contains(storeTags[2].ToLower())) StoreOre.Block = thisBlock;
                    if (nameLower.Contains(storeTags[3].ToLower())) StoreTool.Block = thisBlock;
                    if (storeTags.Length > 4 && nameLower.Contains(storeTags[4].ToLower())) StoreConsumables.Block = thisBlock;
                    if (storeTags.Length > 5 && nameLower.Contains(storeTags[5].ToLower())) StoreSeeds.Block = thisBlock;
                }
            }

            internal void StoreUpdate(IMyGridTerminalSystem terminalSystem, IMyCubeGrid cubeGrid)
            {
                if (_containers.Count == 0 || _storeCount == 0) GetCargoBlocks(terminalSystem, cubeGrid);
                PlaceOffers();
            }


            void GetCargoBlocks(IMyGridTerminalSystem terminalSystem, IMyCubeGrid cubeGrid)
            {
                Warning = "";
                _containers.Clear();
                if (kGroupContainersForTrade != string.Empty)
                {
                    var groupCargo = terminalSystem.GetBlockGroupWithName(kGroupContainersForTrade);
                    if (groupCargo != null) groupCargo.GetBlocksOfType<IMyCargoContainer>(_containers);
                }
                else if (kTagContainerForTrade != string.Empty)
                {
                    terminalSystem.GetBlocksOfType(_containers, x => x.CubeGrid == cubeGrid &&
                        x.CustomName.ToLower().Contains(kTagContainerForTrade.ToLower()));
                }
                if (_containers.Count == 0)
                {
                    terminalSystem.GetBlocksOfType(_containers, x => x.CubeGrid == cubeGrid &&
                        !x.CustomName.ToLower().Contains(kTagExclude.ToLower()));
                }
            }

            void PlaceOffers()
            {
                if (_containers.Count == 0) { Warning = "Размещение отменено. Нет конейнеров"; return; }
                if (!SortingContentsInventories()) return;
                switch (_storeCount)
                {
                    case 0:
                        StoreComp.PlaceOfferingsAndSales(ref Components, "MyObjectBuilder_Component", _globalMarkupPercent);
                        break;
                    case 1:
                        StoreIng.PlaceOfferingsAndSales(ref Ingots, "MyObjectBuilder_Ingot", _globalMarkupPercent);
                        break;
                    case 2:
                        StoreOre.PlaceOfferingsAndSales(ref Ores, "MyObjectBuilder_Ore", _globalMarkupPercent);
                        break;
                    case 3:
                        StoreTool.PlaceOfferingsAndSales(ref Tools, "MyObjectBuilder_PhysicalGunObject", _globalMarkupPercent);
                        StoreTool.PlaceOfferingsAndSales(ref Oxygen, "MyObjectBuilder_OxygenContainerObject", _globalMarkupPercent, true);
                        StoreTool.PlaceOfferingsAndSales(ref Hydrogen, "MyObjectBuilder_GasContainerObject", _globalMarkupPercent, true);
                        StoreTool.PlaceOfferingsAndSales(ref Ammo, "MyObjectBuilder_AmmoMagazine", _globalMarkupPercent, true);
                        break;
                    case 4:
                        StoreConsumables.PlaceOfferingsAndSales(ref Consumables, "MyObjectBuilder_ConsumableItem", _globalMarkupPercent);
                        break;
                    case 5:
                        StoreSeeds.PlaceOfferingsAndSales(ref Seeds, "MyObjectBuilder_SeedItem", _globalMarkupPercent);
                        break;
                    default:
                        _storeCount = 0;
                        _containers.Clear();
                        TimeCheckStore.Start();
                        return;
                }
                _storeCount++;
            }

            bool SortingContentsInventories()
            {
                if (_storeCount > 0) return true;
                if (_invenoryCounter == 0) SetAmountZero();
                if (_invenoryCounter < _containers.Count)
                {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    _containers[_invenoryCounter].GetInventory().GetItems(items);
                    SortingItems(ref items);
                    _invenoryCounter++;
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
                    CreateListInfo(ref Consumables, "РАСХОДНИКИ", ref _infoConsumables);
                    CreateListInfo(ref Seeds, "СЕМЕНА", ref _infoSeeds);
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
                foreach (var item in Consumables) { item.Value.Amount = 0; }
                foreach (var item in Seeds) { item.Value.Amount = 0; }
            }

            void SortingItems(ref List<MyInventoryItem> items)
            {
                foreach (var item in items)
                {
                    if (item.Type.TypeId == "MyObjectBuilder_Component" && Components.ContainsKey(item.Type.SubtypeId))
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
                    else if (item.Type.TypeId == "MyObjectBuilder_ConsumableItem" && Consumables.ContainsKey(item.Type.SubtypeId))
                        Consumables[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
                    else if (item.Type.TypeId == "MyObjectBuilder_SeedItem" && Seeds.ContainsKey(item.Type.SubtypeId))
                        Seeds[item.Type.SubtypeId].Amount += item.Amount.ToIntSafe();
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
                    case "Welder4Item": return "Элитный сварщик";
                    case "RapidFireAutomaticRifleItem": return "Скорострельная автоматическая винтовка";
                    case "PreciseAutomaticRifleItem": return "Точная винтовка";
                    case "OxygenBottle": return "Кислородный баллон";
                    case "HydrogenBottle": return "Водородный баллон";
                    case "Missile200mm": return "Ракета 200мм";
                    case "NATO_25x184mm": return "Боеприпасы 25х184";
                    case "NATO_5p56x45mm": return "Магазин 5.56х45mm";
                    // Расходники
                    case "Medkit": return "Медицинский набор";
                    case "Powerkit": return "Энергетический набор";
                    case "RadiationKit": return "Противорадиационный набор";
                    case "ClangCola": return "Кланг-Кола";
                    case "CosmicCoffee": return "Космический кофе";
                    case "MealPack_KelpCrisp": return "Хрустящая ламинария";
                    case "MealPack_FruitBar": return "Фруктовый батончик";
                    case "MealPack_GardenSlaw": return "Огородный салат";
                    case "MealPack_RedPellets": return "Красные гранулы";
                    case "MealPack_Chili": return "Чили";
                    case "MealPack_Ramen": return "Лапша";
                    case "MealPack_Flatbread": return "Лепешка";
                    case "MealPack_FruitPastry": return "Фруктовая выпечка";
                    case "MealPack_VeggieBurger": return "Вегетарианский бургер";
                    case "MealPack_GreenPellets": return "Зеленые гранулы";
                    case "MealPack_Curry": return "Карри";
                    case "MealPack_Dumplings": return "Пельмени";
                    case "MealPack_Spaghetti": return "Спагетти";
                    case "MealPack_Lasagna": return "Лазанья";
                    case "MealPack_Burrito": return "Буррито";
                    case "MealPack_FrontierStew": return "Походное рагу";
                    case "MealPack_SearedSabiroid": return "Жареный сабироид";
                    case "MealPack_SteakDinner": return "Стейк-ужин";
                    case "Fruit": return "Фрукты";
                    case "Mushrooms": return "Грибы";
                    case "Vegetables": return "Овощи";
                    case "MammalMeatRaw": return "Сырое мясо млекопитающего";
                    case "MammalMeatCooked": return "Приготовленное мясо млекопитающего";
                    case "InsectMeatRaw": return "Сырое мясо насекомого";
                    case "InsectMeatCooked": return "Приготовленное мясо насекомого";
                    case "MealPack_Unknown": return "Неизвестный рацион";
                    case "MealPack_FoodPaste": return "Пищевая паста";
                    case "MealPack_SynthLoaf": return "Синтетический батон";
                    case "MealPack_ClangCrunchies": return "Хрустящие Кланга";
                    case "MealPack_BananaBeef": return "Банан-говядина";
                    case "MealPack_Hardtack": return "Галета";
                    case "MealPack_ExpiredSlop": return "Просроченная похлебка";
                    // Семена
                    case "Fruit_Seed": return "Семена фруктов";
                    case "Grain_Seed": return "Семена зерновых";
                    case "Mushrooms_Seed": return "Споры грибов";
                    case "Vegetables_Seed": return "Семена овощей";
                    default: return name;
                }
            }
        }
    }
}
