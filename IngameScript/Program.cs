using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // ============  ОБЯЗАТЕЛЬНЫЕ НАСТРОЙКИ ============
        // Список тегов в имени блоков магазинов
        string[] storeType = new string[6] {
            "Components",   // Тег блока магазина с компонентами
            "Ingots",       // Тег блока магазина слитков
            "Ores",         // Тег блока магазина руды
            "Tools",        // Тег блока магазина инструментов
            "Consumables",  // Тег блока магазина расходников (еда, напитки)
            "Seeds"         // Тег блока магазина семян
        };

        // ============ ОПЦИОНАЛЬНЫЕ НАСТРОЙКИ МАГАЗИНА ============
        int timeRefresh = 3600; // Интервал для обновления товаров в магазине в секундах (3600 сек = 1 час)
        const string kTagExclude = "Исключить";
        const string kGroupContainersForTrade = "";
        const string kTagContainerForTrade = "";
        int globalMarkupPercent = 25; // 25% наценка по умолчанию. Если у конкретного товара не включён флаг UseMarkup, будет использоваться этот глобальный процент (если он > 0).

        bool tradeComponents = true;
        bool tradeIngots = true;
        bool tradeOres = true;
        bool tradeTools = true;
        bool tradeConsumables = true;
        bool tradeSeeds = true;

        /* -------------------------------------------------------------
         * КОНФИГУРАЦИЯ ТОВАРОВ
         * -------------------------------------------------------------
         * MyItem(граница, цена_закупки, закупка_разрешена, цена_продажи, продажа_разрешена, режим,
         *        использовать_наценку, процент_наценки)
         * 
         * Режимы: TradeModel.Storage, TradeModel.Shop, TradeModel.SellOnly
         * 
         * Если useMarkup = true, то цена продажи = цена_закупки * (1 + процент_наценки/100).
         * При этом параметр "цена_продажи" игнорируется.
         * Если useMarkup = false, то используется фиксированная цена_продажи.
         * 
         * Процент_наценки = 0 означает использование глобальной настройки globalMarkupPercent.
         */

        // ========== КОМПОНЕНТЫ ==========
        static internal Dictionary<string, MyItem> Components = new Dictionary<string, MyItem>()
        {
            ["BulletproofGlass"] = new MyItem(235, 830, true, 3606, true, TradeModel.SellOnly, true, 0),
            ["Canvas"] = new MyItem(10, 2500, true, 13709, true, TradeModel.SellOnly, true, 0),
            ["Computer"] = new MyItem(400, 45, true, 167, true, TradeModel.SellOnly, true, 0),
            ["Construction"] = new MyItem(1000, 430, true, 1923, true, TradeModel.SellOnly, true, 0),
            ["Detector"] = new MyItem(30, 2236, true, 11638, true, TradeModel.SellOnly, true, 0),
            ["Display"] = new MyItem(30, 381, true, 1442, true, TradeModel.SellOnly, true, 0),
            ["Explosives"] = new MyItem(0, 33633, false, 168544, false, TradeModel.SellOnly, true, 0),
            ["Girder"] = new MyItem(100, 360, true, 1442, true, TradeModel.SellOnly, true, 0),
            ["GravityGenerator"] = new MyItem(0, 150000, false, 1750595, false, TradeModel.SellOnly, true, 0),
            ["InteriorPlate"] = new MyItem(200, 154, true, 721, true, TradeModel.SellOnly, true, 0),
            ["LargeTube"] = new MyItem(200, 1702, true, 8940, true, TradeModel.SellOnly, true, 0),
            ["Medical"] = new MyItem(0, 40000, false, 194488, false, TradeModel.SellOnly, true, 0),
            ["MetalGrid"] = new MyItem(300, 3265, true, 12495, true, TradeModel.SellOnly, true, 0),
            ["Motor"] = new MyItem(150, 2008, true, 9759, true, TradeModel.SellOnly, true, 0),
            ["PowerCell"] = new MyItem(50, 1078, true, 5380, true, TradeModel.SellOnly, true, 0),
            ["RadioCommunication"] = new MyItem(30, 515, true, 3334, true, TradeModel.SellOnly, true, 0),
            ["Reactor"] = new MyItem(0, 6410, false, 39421, false, TradeModel.SellOnly, true, 0),
            ["SmallTube"] = new MyItem(300, 267, true, 1202, true, TradeModel.SellOnly, true, 0),
            ["SolarCell"] = new MyItem(0, 641, false, 3361, false, TradeModel.SellOnly, true, 0),
            ["SteelPlate"] = new MyItem(2500, 1236, true, 5048, true, TradeModel.SellOnly, true, 0),
            ["Superconductor"] = new MyItem(0, 21354, false, 132429, false, TradeModel.SellOnly, true, 0),
            ["Thrust"] = new MyItem(0, 41325, false, 162463, false, TradeModel.SellOnly, true, 0),
            ["ZoneChip"] = new MyItem(0, 100000, false, 100500, false, TradeModel.SellOnly, true, 0),
            ["EngineerPlushie"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly, true, 0),
            ["EngineerPlushieSE2"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly, true, 0),
            ["SabiroidPlushie"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly, true, 0),
            ["PrototechFrame"] = new MyItem(10, 100000, false, 105000, true, TradeModel.SellOnly, true, 0),
            ["PrototechPanel"] = new MyItem(10, 323580, false, 325000, true, TradeModel.SellOnly, true, 0),
            ["PrototechCapacitor"] = new MyItem(10, 635439, false, 640000, true, TradeModel.SellOnly, true, 0),
            ["PrototechPropulsionUnit"] = new MyItem(10, 1460119, false, 1465000, true, TradeModel.SellOnly, true, 0),
            ["PrototechMachinery"] = new MyItem(10, 353407, false, 355000, true, TradeModel.SellOnly, true, 0),
            ["PrototechCircuitry"] = new MyItem(10, 664209, false, 670000, true, TradeModel.SellOnly, true, 0),
            ["PrototechCoolingUnit"] = new MyItem(10, 1787887, false, 1800000, true, TradeModel.SellOnly, true, 0),
        };

        // ========== СЛИТКИ ==========
        static internal Dictionary<string, MyItem> Ingots = new Dictionary<string, MyItem>()
        {
            ["Cobalt"] = new MyItem(1000, 1535, true, 1600, true, TradeModel.SellOnly, true, 0),
            ["Gold"] = new MyItem(1000, 23355, true, 24000, true, TradeModel.SellOnly, true, 0),
            ["Iron"] = new MyItem(1000, 150, true, 170, true, TradeModel.SellOnly, true, 0),
            ["Magnesium"] = new MyItem(1000, 34054, true, 34500, true, TradeModel.SellOnly, true, 0),
            ["Nickel"] = new MyItem(1000, 306, true, 310, true, TradeModel.SellOnly, true, 0),
            ["Platinum"] = new MyItem(10, 122815, true, 123000, true, TradeModel.SellOnly, true, 0),
            ["Silicon"] = new MyItem(1000, 173, true, 180, true, TradeModel.SellOnly, true, 0),
            ["Silver"] = new MyItem(1000, 2585, true, 2600, true, TradeModel.SellOnly, true, 0),
            ["Uranium"] = new MyItem(50, 80664, true, 80700, true, TradeModel.SellOnly, true, 0),
        };

        // ========== РУДЫ ==========ы
        static internal Dictionary<string, MyItem> Ores = new Dictionary<string, MyItem>()
        {
            ["Cobalt"] = new MyItem(1000, 300, true, 310, true, TradeModel.SellOnly, true, 0),
            ["Gold"] = new MyItem(1000, 210, true, 230, true, TradeModel.SellOnly, true, 0),
            ["Stone"] = new MyItem(1000, 10, true, 11, true, TradeModel.SellOnly, true, 0),
            ["Iron"] = new MyItem(1000, 105, true, 110, true, TradeModel.SellOnly, true, 0),
            ["Magnesium"] = new MyItem(1000, 210, true, 212, true, TradeModel.SellOnly, true, 0),
            ["Nickel"] = new MyItem(1000, 100, true, 105, true, TradeModel.SellOnly, true, 0),
            ["Platinum"] = new MyItem(1000, 420, true, 435, true, TradeModel.SellOnly, true, 0),
            ["Silicon"] = new MyItem(1000, 100, true, 110, true, TradeModel.SellOnly, true, 0),
            ["Silver"] = new MyItem(1000, 210, true, 212, true, TradeModel.SellOnly, true, 0),
            ["Uranium"] = new MyItem(1000, 350, true, 505, true, TradeModel.SellOnly, true, 0),
            ["Ice"] = new MyItem(1000, 50, true, 51, true, TradeModel.SellOnly, true, 0),
        };

        // ========== ИНСТРУМЕНТЫ, ОРУЖИЕ, БАЛЛОНЫ, БОЕПРИПАСЫ ==========
        static internal Dictionary<string, MyItem> Tools = new Dictionary<string, MyItem>()
        {
            ["WelderItem"] = new MyItem(0, 2908, true, 2909, true, TradeModel.SellOnly, true, 0),
            ["Welder2Item"] = new MyItem(1, 10518, true, 11000, true, TradeModel.SellOnly, true, 0),
            ["Welder3Item"] = new MyItem(1, 32664, true, 35000, true, TradeModel.SellOnly, true, 0),
            ["Welder4Item"] = new MyItem(2, 302438, true, 310000, true, TradeModel.SellOnly, true, 0),
            ["AngleGrinderItem"] = new MyItem(2, 3433, true, 3500, true, TradeModel.SellOnly, true, 0),
            ["AngleGrinder2Item"] = new MyItem(2, 10578, true, 11000, true, TradeModel.SellOnly, true, 0),
            ["AngleGrinder3Item"] = new MyItem(2, 32821, true, 35000, true, TradeModel.SellOnly, true, 0),
            ["AngleGrinder4Item"] = new MyItem(2, 302901, true, 310000, true, TradeModel.SellOnly, true, 0),
            ["HandDrillItem"] = new MyItem(2, 4851, true, 5000, true, TradeModel.SellOnly, true, 0),
            ["HandDrill2Item"] = new MyItem(2, 15155, true, 17000, true, TradeModel.SellOnly, true, 0),
            ["HandDrill3Item"] = new MyItem(2, 44764, true, 45000, true, TradeModel.SellOnly, true, 0),
            ["HandDrill4Item"] = new MyItem(2, 338084, true, 340000, true, TradeModel.SellOnly, true, 0),
            ["FlareGunItem"] = new MyItem(1, 520, true, 550, true, TradeModel.SellOnly, true, 0),
            ["SemiAutoPistolItem"] = new MyItem(1, 566, true, 600, true, TradeModel.SellOnly, true, 0),
            ["FullAutoPistolItem"] = new MyItem(2, 3140, true, 3500, true, TradeModel.SellOnly, true, 0),
            ["ElitePistolItem"] = new MyItem(2, 64155, true, 65000, true, TradeModel.SellOnly, true, 0),
            ["AutomaticRifleItem"] = new MyItem(1, 4338, true, 5000, true, TradeModel.SellOnly, true, 0),
            ["PreciseAutomaticRifleItem"] = new MyItem(1, 19148, true, 20000, true, TradeModel.SellOnly, true, 0),
            ["RapidFireAutomaticRifleItem"] = new MyItem(1, 15233, true, 16000, true, TradeModel.SellOnly, true, 0),
            ["UltimateAutomaticRifleItem"] = new MyItem(1, 111783, true, 115000, true, TradeModel.SellOnly, true, 0),
            ["BasicHandHeldLauncherItem"] = new MyItem(1, 33807, true, 35000, true, TradeModel.SellOnly, true, 0),
            ["AdvancedHandHeldLauncherItem"] = new MyItem(1, 275942, true, 280000, true, TradeModel.SellOnly, true, 0),
        };

        static internal Dictionary<string, MyItem> Oxygen = new Dictionary<string, MyItem>()
        {
            ["OxygenBottle"] = new MyItem(2, 68909, true, 70000, true, TradeModel.SellOnly, true, 0)
        };

        static internal Dictionary<string, MyItem> Hydrogen = new Dictionary<string, MyItem>()
        {
            ["HydrogenBottle"] = new MyItem(2, 68909, true, 70000, true, TradeModel.SellOnly, true, 0)
        };

        static internal Dictionary<string, MyItem> Ammo = new Dictionary<string, MyItem>()
        {
            ["SemiAutoPistolMagazine"] = new MyItem(20, 112, true, 150, true, TradeModel.SellOnly, true, 0),
            ["FullAutoPistolMagazine"] = new MyItem(50, 8699, true, 9200, true, TradeModel.SellOnly, true, 0),
            ["ElitePistolMagazine"] = new MyItem(50, 8163, true, 8653, true, TradeModel.SellOnly, true, 0),
            ["FlareClip"] = new MyItem(10, 95, true, 100, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxBlue"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxGreen"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxRed"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxPink"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxYellow"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["FireworksBoxRainbow"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly, true, 0),
            ["AutomaticRifleGun_Mag_20rd"] = new MyItem(10, 15113, true, 15600, true, TradeModel.SellOnly, true, 0),
            ["RapidFireAutomaticRifleGun_Mag_50rd"] = new MyItem(10, 40220, true, 40720, true, TradeModel.SellOnly, true, 0),
            ["PreciseAutomaticRifleGun_Mag_5rd"] = new MyItem(10, 15113, true, 15613, true, TradeModel.SellOnly, true, 0),
            ["UltimateAutomaticRifleGun_Mag_30rd"] = new MyItem(10, 25185, true, 25685, true, TradeModel.SellOnly, true, 0),
            ["AutocannonClip"] = new MyItem(10, 181002, true, 182002, true, TradeModel.SellOnly, true, 0),
            ["Missile200mm"] = new MyItem(10, 170482, true, 171482, true, TradeModel.SellOnly, true, 0),
            ["LargeCalibreAmmo"] = new MyItem(10, 581700, true, 590000, true, TradeModel.SellOnly, true, 0),
            ["MediumCalibreAmmo"] = new MyItem(10, 96327, true, 100000, true, TradeModel.SellOnly, true, 0),
            ["LargeRailgunAmmo"] = new MyItem(10, 158889, true, 165000, true, TradeModel.SellOnly, true, 0),
            ["SmallRailgunAmmo"] = new MyItem(10, 26866, true, 30000, true, TradeModel.SellOnly, true, 0),
            ["NATO_25x184mm"] = new MyItem(10, 314791, true, 320000, true, TradeModel.SellOnly, true, 0),
            ["NATO_5p56x45mm"] = new MyItem(10, 96327, true, 100000, true, TradeModel.SellOnly, true, 0),
        };

        // ========== РАСХОДНИКИ (ЕДА, НАПИТКИ, МЕДИЦИНА) ==========
        static internal Dictionary<string, MyItem> Consumables = new Dictionary<string, MyItem>()
        {
            ["Medkit"] = new MyItem(10, 500, false, 600, true, TradeModel.SellOnly, true, 0),
            ["Powerkit"] = new MyItem(10, 800, false, 950, true, TradeModel.SellOnly, true, 0),
            ["RadiationKit"] = new MyItem(5, 1200, false, 1400, true, TradeModel.SellOnly, true, 0),
            ["ClangCola"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["CosmicCoffee"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_KelpCrisp"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_FruitBar"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_GardenSlaw"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_RedPellets"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Chili"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Ramen"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Flatbread"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_FruitPastry"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_VeggieBurger"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_GreenPellets"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Curry"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Dumplings"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Spaghetti"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Lasagna"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Burrito"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_FrontierStew"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_SearedSabiroid"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_SteakDinner"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["Fruit"] = new MyItem(0, 1240, false, 1300, true, TradeModel.SellOnly, true, 0),
            ["Mushrooms"] = new MyItem(0, 1164, false, 1200, true, TradeModel.SellOnly, true, 0),
            ["Vegetables"] = new MyItem(0, 1189, false, 1250, true, TradeModel.SellOnly, true, 0),
            ["MammalMeatRaw"] = new MyItem(0, 1027, false, 1100, true, TradeModel.SellOnly, true, 0),
            ["MammalMeatCooked"] = new MyItem(0, 1027, false, 1100, true, TradeModel.SellOnly, true, 0),
            ["InsectMeatRaw"] = new MyItem(0, 856, false, 950, true, TradeModel.SellOnly, true, 0),
            ["InsectMeatCooked"] = new MyItem(0, 856, false, 950, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Unknown"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_FoodPaste"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_SynthLoaf"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_ClangCrunchies"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_BananaBeef"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_Hardtack"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
            ["MealPack_ExpiredSlop"] = new MyItem(0, 100, false, 100, true, TradeModel.SellOnly, true, 0),
        };

        // ========== СЕМЕНА ==========
        static internal Dictionary<string, MyItem> Seeds = new Dictionary<string, MyItem>()
        {
            ["Fruit"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly, true, 0),
            ["Grain"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly, true, 0),
            ["Mushrooms"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly, true, 0),
            ["Vegetables"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly, true, 0),
        };

        // ============ КОНЕЦ НАСТРОЕК ============

        MyAutoStore AutoStore;
        readonly string[] arguments = new string[] {
            "магазин.разместить",
            "магазин.очистить",
            "магазин.список"
        };

        // Режим работы с товаром
        public enum TradeModel : byte { Shop, Storage, SellOnly }
        string oldCommand = "";

        public Program()
        {
            AutoStore = new MyAutoStore(ref tradeComponents, ref tradeIngots, ref tradeOres, ref tradeTools,
                                        ref tradeConsumables, ref tradeSeeds, globalMarkupPercent, timeRefresh);
            AutoStore.GetStoreBlock(GridTerminalSystem, Me.CubeGrid, ref storeType);
            CheckingSystem();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            AvailableCommands();
        }

        public void Main(string arg, UpdateType updateSource)
        {
            if (AutoStore.TimeCheckStore.IsOut())
            {
                if (Runtime.UpdateFrequency != UpdateFrequency.Update10) Runtime.UpdateFrequency = UpdateFrequency.Update10;
                AutoStore.StoreUpdate(GridTerminalSystem, Me.CubeGrid);
            }
            else if (Runtime.UpdateFrequency != UpdateFrequency.Update100) Runtime.UpdateFrequency = UpdateFrequency.Update100;

            if (arg != string.Empty) Arguments(arg);

            Echo($"=== Автоматический магазин ===");
            //Echo($"Выполнение {Runtime.LastRunTimeMs} мс");
            if (AutoStore.TimeCheckStore.Launched)
                Echo($"Обновление предложений \n* через {AutoStore.TimeCheckStore.RestTime}");
                
            AvailableCommands();
        }

        void CheckingSystem()
        {
            Me.CustomData = "";
            
            if (AutoStore.StoreComp.Block != null)
                Me.CustomData += $"{AutoStore.StoreComp.Block.CustomName} подключен. Торговля: {tradeComponents}";
            else
                Me.CustomData += $"Магазин для {storeType[0]} не подключен.";
            
            if (AutoStore.StoreIng.Block != null)
                Me.CustomData += $"\n{AutoStore.StoreIng.Block.CustomName} подключен. Торговля: {tradeIngots}";
            else
                Me.CustomData += $"\nМагазин для {storeType[1]} не подключен.";
            
            if (AutoStore.StoreOre.Block != null)
                Me.CustomData += $"\n{AutoStore.StoreOre.Block.CustomName} подключен. Торговля: {tradeOres}";
            else
                Me.CustomData += $"\nМагазин для {storeType[2]} не подключен.";
            
            if (AutoStore.StoreTool.Block != null)
                Me.CustomData += $"\n{AutoStore.StoreTool.Block.CustomName} подключен. Торговля: {tradeTools}";
            else
                Me.CustomData += $"\nМагазин для {storeType[3]} не подключен.";
            
            if (AutoStore.StoreConsumables.Block != null)
                Me.CustomData += $"\n{AutoStore.StoreConsumables.Block.CustomName} подключен. Торговля: {tradeConsumables}";
            else
                Me.CustomData += $"\nМагазин для {storeType[4]} не подключен.";
            
            if (AutoStore.StoreSeeds.Block != null)
                Me.CustomData += $"\n{AutoStore.StoreSeeds.Block.CustomName} подключен. Торговля: {tradeSeeds}";
            else
                Me.CustomData += $"\nМагазин для {storeType[5]} не подключен.";

            Me.CustomData += $"\n\n* Добавьте нужным контейнерам тег '{kTagContainerForTrade}' в имя блока.";
            Me.CustomData += $"\n* Или создайте группу '{kGroupContainersForTrade}' с нужными контейнерами.";
        }
        void Arguments(string arg)
        {
            if (arg.ToLower() == arguments[0])
            {
                AutoStore.TimeCheckStore.Stop();
                oldCommand = arguments[0];
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[1])
            {
                AutoStore.StoreComp.ClearAll();
                AutoStore.StoreIng.ClearAll();
                AutoStore.StoreOre.ClearAll();
                AutoStore.StoreTool.ClearAll();
                AutoStore.StoreConsumables.ClearAll();
                AutoStore.StoreSeeds.ClearAll();
                oldCommand = $"{arguments[1]}\n=> Очистка завершена";
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[2])
            {
                Me.CustomData = AutoStore.StoreComp.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreIng.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreOre.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreTool.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreConsumables.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreSeeds.GetOrdersAndOffers();
                oldCommand = $"{arguments[2]}\nТовары из магазина выведены в данные ПБ";
                AvailableCommands();
            }
        }
        void AvailableCommands()
        {
            string info = $"\nПред.аргумент: {oldCommand}\n\nВозможные аргументы:";
            foreach (var arg in arguments) { info += $"\n{arg}"; }
            Echo(info);
        }
    }
}
