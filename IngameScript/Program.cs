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
        string tagExclude = "Исключить";
        string groupContainersForTrade = "";
        string tagContainerForTrade = "";

        bool tradeComponents = true;
        bool tradeIngots = true;
        bool tradeOres = true;
        bool tradeTools = true;
        bool tradeConsumables = true;
        bool tradeSeeds = true;

        /* -------------------------------------------------------------
         * КОНФИГУРАЦИЯ ТОВАРОВ
         * -------------------------------------------------------------
         * MyItem(граница, цена_закупки, закупка_разрешена, цена_продажи, продажа_разрешена, режим)
         * Режимы: TradeModel.Storage, TradeModel.Shop, TradeModel.SellOnly
         */

        // ========== КОМПОНЕНТЫ ==========
        static internal Dictionary<string, MyItem> Components = new Dictionary<string, MyItem>()
        {
            ["BulletproofGlass"] = new MyItem(235, 830, true, 3606, true, TradeModel.SellOnly),
            ["Canvas"] = new MyItem(10, 2500, true, 13709, true, TradeModel.SellOnly),
            ["Computer"] = new MyItem(400, 45, true, 167, true, TradeModel.SellOnly),
            ["Construction"] = new MyItem(1000, 430, true, 1923, true, TradeModel.SellOnly),
            ["Detector"] = new MyItem(30, 2236, true, 11638, true, TradeModel.SellOnly),
            ["Display"] = new MyItem(30, 381, true, 1442, true, TradeModel.SellOnly),
            ["Explosives"] = new MyItem(0, 33633, false, 168544, false, TradeModel.SellOnly),
            ["Girder"] = new MyItem(100, 360, true, 1442, true, TradeModel.SellOnly),
            ["GravityGenerator"] = new MyItem(0, 150000, false, 1750595, false, TradeModel.SellOnly),
            ["InteriorPlate"] = new MyItem(200, 154, true, 721, true, TradeModel.SellOnly),
            ["LargeTube"] = new MyItem(200, 1702, true, 8940, true, TradeModel.SellOnly),
            ["Medical"] = new MyItem(0, 40000, false, 194488, false, TradeModel.SellOnly),
            ["MetalGrid"] = new MyItem(300, 3265, true, 12495, true, TradeModel.SellOnly),
            ["Motor"] = new MyItem(150, 2008, true, 9759, true, TradeModel.SellOnly),
            ["PowerCell"] = new MyItem(50, 1078, true, 5380, true, TradeModel.SellOnly),
            ["RadioCommunication"] = new MyItem(30, 515, true, 3334, true, TradeModel.SellOnly),
            ["Reactor"] = new MyItem(0, 6410, false, 39421, false, TradeModel.SellOnly),
            ["SmallTube"] = new MyItem(300, 267, true, 1202, true, TradeModel.SellOnly),
            ["SolarCell"] = new MyItem(0, 641, false, 3361, false, TradeModel.SellOnly),
            ["SteelPlate"] = new MyItem(2500, 1236, true, 5048, true, TradeModel.SellOnly),
            ["Superconductor"] = new MyItem(0, 21354, false, 132429, false, TradeModel.SellOnly),
            ["Thrust"] = new MyItem(0, 41325, false, 162463, false, TradeModel.SellOnly),
            ["ZoneChip"] = new MyItem(0, 100000, false, 100500, false, TradeModel.SellOnly),
            ["EngineerPlushie"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly),
            ["EngineerPlushieSE2"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly),
            ["SabiroidPlushie"] = new MyItem(1, 30000, false, 30500, true, TradeModel.SellOnly),
            ["PrototechFrame"] = new MyItem(10, 100000, false, 105000, true, TradeModel.SellOnly),
            ["PrototechPanel"] = new MyItem(10, 323580, false, 325000, true, TradeModel.SellOnly),
            ["PrototechCapacitor"] = new MyItem(10, 635439, false, 640000, true, TradeModel.SellOnly),
            ["PrototechPropulsionUnit"] = new MyItem(10, 1460119, false, 1465000, true, TradeModel.SellOnly),
            ["PrototechMachinery"] = new MyItem(10, 353407, false, 355000, true, TradeModel.SellOnly),
            ["PrototechCircuitry"] = new MyItem(10, 664209, false, 670000, true, TradeModel.SellOnly),
            ["PrototechCoolingUnit"] = new MyItem(10, 1787887, false, 1800000, true, TradeModel.SellOnly),
        };

        // ========== СЛИТКИ ==========
        static internal Dictionary<string, MyItem> Ingots = new Dictionary<string, MyItem>()
        {
            ["Cobalt"] = new MyItem(1000, 1535, true, 1600, true, TradeModel.SellOnly),
            ["Gold"] = new MyItem(1000, 23355, true, 24000, true, TradeModel.SellOnly),
            ["Iron"] = new MyItem(1000, 150, true, 170, true, TradeModel.SellOnly),
            ["Magnesium"] = new MyItem(1000, 34054, true, 34500, true, TradeModel.SellOnly),
            ["Nickel"] = new MyItem(1000, 306, true, 310, true, TradeModel.SellOnly),
            ["Platinum"] = new MyItem(10, 122815, true, 123000, true, TradeModel.SellOnly),
            ["Silicon"] = new MyItem(1000, 173, true, 180, true, TradeModel.SellOnly),
            ["Silver"] = new MyItem(1000, 2585, true, 2600, true, TradeModel.SellOnly),
            ["Uranium"] = new MyItem(50, 80664, true, 80700, true, TradeModel.SellOnly),
        };

        // ========== РУДЫ ==========
        static internal Dictionary<string, MyItem> Ores = new Dictionary<string, MyItem>()
        {
            ["Cobalt"] = new MyItem(1000, 300, true, 310, true, TradeModel.SellOnly),
            ["Gold"] = new MyItem(1000, 210, true, 230, true, TradeModel.SellOnly),
            ["Stone"] = new MyItem(1000, 10, true, 11, true, TradeModel.SellOnly),
            ["Iron"] = new MyItem(1000, 105, true, 110, true, TradeModel.SellOnly),
            ["Magnesium"] = new MyItem(1000, 210, true, 212, true, TradeModel.SellOnly),
            ["Nickel"] = new MyItem(1000, 100, true, 105, true, TradeModel.SellOnly),
            ["Platinum"] = new MyItem(1000, 420, true, 435, true, TradeModel.SellOnly),
            ["Silicon"] = new MyItem(1000, 100, true, 110, true, TradeModel.SellOnly),
            ["Silver"] = new MyItem(1000, 210, true, 212, true, TradeModel.SellOnly),
            ["Uranium"] = new MyItem(1000, 350, true, 505, true, TradeModel.SellOnly),
            ["Ice"] = new MyItem(1000, 50, true, 51, true, TradeModel.SellOnly),
        };

        // ========== ИНСТРУМЕНТЫ, БАЛЛОНЫ, БОЕПРИПАСЫ ==========
        static internal Dictionary<string, MyItem> Tools = new Dictionary<string, MyItem>()
        {
            ["WelderItem"] = new MyItem(0, 2908, true, 2909, true, TradeModel.SellOnly),
            ["Welder2Item"] = new MyItem(1, 10518, true, 11000, true, TradeModel.SellOnly),
            ["Welder3Item"] = new MyItem(1, 32664, true, 35000, true, TradeModel.SellOnly),
            ["Welder4Item"] = new MyItem(2, 302438, true, 310000, true, TradeModel.SellOnly),
            ["AngleGrinderItem"] = new MyItem(2, 3433, true, 3500, true, TradeModel.SellOnly),
            ["AngleGrinder2Item"] = new MyItem(2, 10578, true, 11000, true, TradeModel.SellOnly),
            ["AngleGrinder3Item"] = new MyItem(2, 32821, true, 35000, true, TradeModel.SellOnly),
            ["AngleGrinder4Item"] = new MyItem(2, 302901, true, 310000, true, TradeModel.SellOnly),
            ["HandDrillItem"] = new MyItem(2, 4851, true, 5000, true, TradeModel.SellOnly),
            ["HandDrill2Item"] = new MyItem(2, 15155, true, 17000, true, TradeModel.SellOnly),
            ["HandDrill3Item"] = new MyItem(2, 44764, true, 45000, true, TradeModel.SellOnly),
            ["HandDrill4Item"] = new MyItem(2, 338084, true, 340000, true, TradeModel.SellOnly),
            ["FlareGunItem"] = new MyItem(1, 520, true, 550, true, TradeModel.SellOnly),
            ["SemiAutoPistolItem"] = new MyItem(1, 566, true, 600, true, TradeModel.SellOnly),
            ["FullAutoPistolItem"] = new MyItem(2, 3140, true, 3500, true, TradeModel.SellOnly),
            ["ElitePistolItem"] = new MyItem(2, 64155, true, 65000, true, TradeModel.SellOnly),
            ["AutomaticRifleItem"] = new MyItem(1, 4338, true, 5000, true, TradeModel.SellOnly),
            ["PreciseAutomaticRifleItem"] = new MyItem(1, 19148, true, 20000, true, TradeModel.SellOnly),
            ["RapidFireAutomaticRifleItem"] = new MyItem(1, 15233, true, 16000, true, TradeModel.SellOnly),
            ["UltimateAutomaticRifleItem"] = new MyItem(1, 111783, true, 115000, true, TradeModel.SellOnly),
            ["BasicHandHeldLauncherItem"] = new MyItem(1, 33807, true, 35000, true, TradeModel.SellOnly),
            ["AdvancedHandHeldLauncherItem"] = new MyItem(1, 275942, true, 280000, true, TradeModel.SellOnly),
        };

        static internal Dictionary<string, MyItem> Oxygen = new Dictionary<string, MyItem>()
        {
            ["OxygenBottle"] = new MyItem(2, 68909, true, 70000, true, TradeModel.SellOnly)
        };

        static internal Dictionary<string, MyItem> Hydrogen = new Dictionary<string, MyItem>()
        {
            ["HydrogenBottle"] = new MyItem(2, 68909, true, 70000, true, TradeModel.SellOnly)
        };

        static internal Dictionary<string, MyItem> Ammo = new Dictionary<string, MyItem>()
        {
            ["SemiAutoPistolMagazine"] = new MyItem(20, 112, true, 150, true, TradeModel.SellOnly),
            ["FullAutoPistolMagazine"] = new MyItem(50, 8699, true, 9200, true, TradeModel.SellOnly),
            ["ElitePistolMagazine"] = new MyItem(50, 8163, true, 8653, true, TradeModel.SellOnly),
            ["FlareClip"] = new MyItem(10, 95, true, 100, true, TradeModel.SellOnly),
            ["FireworksBoxBlue"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["FireworksBoxGreen"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["FireworksBoxRed"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["FireworksBoxPink"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["FireworksBoxYellow"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["FireworksBoxRainbow"] = new MyItem(10, 45899, true, 46400, true, TradeModel.SellOnly),
            ["AutomaticRifleGun_Mag_20rd"] = new MyItem(10, 15113, true, 15600, true, TradeModel.SellOnly),
            ["RapidFireAutomaticRifleGun_Mag_50rd"] = new MyItem(10, 40220, true, 40720, true, TradeModel.SellOnly),
            ["PreciseAutomaticRifleGun_Mag_5rd"] = new MyItem(10, 15113, true, 15613, true, TradeModel.SellOnly),
            ["UltimateAutomaticRifleGun_Mag_30rd"] = new MyItem(10, 25185, true, 25685, true, TradeModel.SellOnly),
            ["AutocannonClip"] = new MyItem(10, 181002, true, 182002, true, TradeModel.SellOnly),
            ["Missile200mm"] = new MyItem(10, 170482, true, 171482, true, TradeModel.SellOnly),
            ["LargeCalibreAmmo"] = new MyItem(10, 581700, true, 590000, true, TradeModel.SellOnly),
            ["MediumCalibreAmmo"] = new MyItem(10, 96327, true, 100000, true, TradeModel.SellOnly),
            ["LargeRailgunAmmo"] = new MyItem(10, 158889, true, 165000, true, TradeModel.SellOnly),
            ["SmallRailgunAmmo"] = new MyItem(10, 26866, true, 30000, true, TradeModel.SellOnly),
            ["NATO_25x184mm"] = new MyItem(10, 314791, true, 320000, true, TradeModel.SellOnly),
            ["NATO_5p56x45mm"] = new MyItem(10, 96327, true, 100000, true, TradeModel.SellOnly),
        };

        // ========== РАСХОДНИКИ (ЕДА, НАПИТКИ) ==========
        static internal Dictionary<string, MyItem> Consumables = new Dictionary<string, MyItem>()
        {
            // Базовые (были)
            ["Medkit"] = new MyItem(10, 500, false, 600, true, TradeModel.SellOnly),
            ["Powerkit"] = new MyItem(10, 800, false, 950, true, TradeModel.SellOnly),
            ["RadiationKit"] = new MyItem(5, 1200, false, 1400, true, TradeModel.SellOnly),
            // Напитки
            ["ClangCola"] = new MyItem(0, 100, false, 105, true, TradeModel.SellOnly),
            ["CosmicCoffee"] = new MyItem(0, 100, false, 105, true, TradeModel.SellOnly),
            // Готовые рационы
            ["MealPack_KelpCrisp"] = new MyItem(0, 3491, false, 3900, true, TradeModel.SellOnly),
            ["MealPack_FruitBar"] = new MyItem(0, 8005, false, 8505, true, TradeModel.SellOnly),
            ["MealPack_GardenSlaw"] = new MyItem(0, 11567, false, 12000, true, TradeModel.SellOnly),
            ["MealPack_RedPellets"] = new MyItem(0, 11730, false, 12200, true, TradeModel.SellOnly),
            ["MealPack_Chili"] = new MyItem(0, 15369, false, 15800, true, TradeModel.SellOnly),
            ["MealPack_Ramen"] = new MyItem(0, 9588, false, 10000, true, TradeModel.SellOnly),
            ["MealPack_Flatbread"] = new MyItem(0, 11807, false, 12350, true, TradeModel.SellOnly),
            ["MealPack_FruitPastry"] = new MyItem(0, 16011, false, 16500, true, TradeModel.SellOnly),
            ["MealPack_VeggieBurger"] = new MyItem(0, 15365, false, 15800, true, TradeModel.SellOnly),
            ["MealPack_GreenPellets"] = new MyItem(0, 14294, false, 14750, true, TradeModel.SellOnly),
            ["MealPack_Curry"] = new MyItem(0, 15771, false, 16270, true, TradeModel.SellOnly),
            ["MealPack_Dumplings"] = new MyItem(0, 15605, false, 16100, true, TradeModel.SellOnly),
            ["MealPack_Spaghetti"] = new MyItem(0, 19646, false, 20100, true, TradeModel.SellOnly),
            ["MealPack_Lasagna"] = new MyItem(0, 22380, false, 22800, true, TradeModel.SellOnly),
            ["MealPack_Burrito"] = new MyItem(0, 26182, false, 26600, true, TradeModel.SellOnly),
            ["MealPack_FrontierStew"] = new MyItem(0, 21977, false, 22400, true, TradeModel.SellOnly),
            ["MealPack_SearedSabiroid"] = new MyItem(0, 21184, false, 21680, true, TradeModel.SellOnly),
            ["MealPack_SteakDinner"] = new MyItem(0, 26025, false, 26525, true, TradeModel.SellOnly),
            // Ингредиенты (съедобные)
            ["Fruit"] = new MyItem(0, 1240, false, 1300, true, TradeModel.SellOnly),
            ["Mushrooms"] = new MyItem(0, 1164, false, 1200, true, TradeModel.SellOnly),
            ["Vegetables"] = new MyItem(0, 1189, false, 1250, true, TradeModel.SellOnly),
            // Мясо
            ["MammalMeatRaw"] = new MyItem(0, 1027, false, 1100, true, TradeModel.SellOnly),
            ["MammalMeatCooked"] = new MyItem(0, 1027, false, 1100, true, TradeModel.SellOnly),
            ["InsectMeatRaw"] = new MyItem(0, 856, false, 950, true, TradeModel.SellOnly),
            ["InsectMeatCooked"] = new MyItem(0, 856, false, 950, true, TradeModel.SellOnly),
        };

        // ========== СЕМЕНА ==========
        static internal Dictionary<string, MyItem> Seeds = new Dictionary<string, MyItem>()
        {
            ["Fruit"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly),
            ["Grain"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly),
            ["Mushrooms"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly),
            ["Vegetables"] = new MyItem(0, 100, false, 110, true, TradeModel.SellOnly),
        };

        // ============ КОНЕЦ НАСТРОЕК ============

        MyAutoStore AutoStore;
        readonly string[] arguments = new string[] {
            "магазин.разместить",
            "магазин.очистить",
            "магазин.список",
            "магазин.время"
        };

        // Режим работы с товаром
        public enum TradeModel : byte { Shop, Storage, SellOnly }
        string oldCommand = "";

        public Program()
        {
            AutoStore = new MyAutoStore(ref tradeComponents, ref tradeIngots, ref tradeOres, ref tradeTools, ref tradeConsumables, ref tradeSeeds, timeRefresh);
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
                AutoStore.StoreUpdate(GridTerminalSystem, Me, ref groupContainersForTrade, ref tagContainerForTrade, ref tagExclude);
            }
            else if (Runtime.UpdateFrequency != UpdateFrequency.Update100) Runtime.UpdateFrequency = UpdateFrequency.Update100;

            if (arg != string.Empty) Arguments(arg);
            Echo($"Выполнение {Runtime.LastRunTimeMs} мс");
            AvailableCommands();
        }

        void CheckingSystem()
        {
            Me.CustomData = "";
            if (AutoStore.StoreComp.Block != null)
                Me.CustomData = $"\nМагазин {AutoStore.StoreComp.Block.CustomName} подключен. Торговля: {tradeComponents}";
            else
                Me.CustomData = $"\nМагазин {storeType[0]} не подключен";
            if (AutoStore.StoreIng.Block != null)
                Me.CustomData += $"\nМагазин {AutoStore.StoreIng.Block.CustomName} подключен. Торговля: {tradeIngots}";
            else
                Me.CustomData += $"\nМагазин {storeType[1]} не подключен";
            if (AutoStore.StoreOre.Block != null)
                Me.CustomData += $"\nМагазин {AutoStore.StoreOre.Block.CustomName} подключен. Торговля: {tradeOres}";
            else
                Me.CustomData += $"\nМагазин {storeType[2]} не подключен";
            if (AutoStore.StoreTool.Block != null)
                Me.CustomData += $"\nМагазин {AutoStore.StoreTool.Block.CustomName} подключен. Торговля: {tradeTools}";
            else
                Me.CustomData += $"\nМагазин {storeType[3]} не подключен";
            if (AutoStore.StoreConsumables.Block != null)
                Me.CustomData += $"\nМагазин {AutoStore.StoreConsumables.Block.CustomName} подключен. Торговля: {tradeConsumables}";
            else
                Me.CustomData += $"\nМагазин {storeType[4]} не подключен";
            if (AutoStore.StoreSeeds.Block != null)
                Me.CustomData += $"\nМагазин {AutoStore.StoreSeeds.Block.CustomName} подключен. Торговля: {tradeSeeds}";
            else
                Me.CustomData += $"\nМагазин {storeType[5]} не подключен";
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
            else if (arg.ToLower() == arguments[3])
            {
                oldCommand = $"{arguments[3]}\nОбновление магазина через\n{AutoStore.TimeCheckStore.RestTime}";
                AvailableCommands();
            }
        }
        void AvailableCommands()
        {
            string info = $"Пред.аргумент:{oldCommand}\n\nВозможные аргументы:";
            foreach (var arg in arguments) { info += $"\n{arg}"; }
            Echo(info);
        }
    }
}
