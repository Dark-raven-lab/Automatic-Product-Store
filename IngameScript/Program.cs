﻿using Sandbox.ModAPI.Ingame;
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
        // Можно добавить 1 тег на каждый магазин или сразу все в один магазин(тогда всё будет в одном магазине)

        string[] storeType = new string[4] { 
            "Components", // Тег блока магазина с компонентами
            "Ingots", // Тег блока магазина слитков
            "Ores", // Тег блока магазина руды
            "Tools" // Пока ничего не делает
        };

        // ============ ОПЦИОНАЛЬНЫЕ НАСТРОЙКИ МАГАЗИНА ============ 
        const int timeRefresh = 3600; // Интервал для обновления товаров в магазине в секундах (3600 сек = 1 час)
        // Тег в названии блоков для исключения из работы скрипта (не работает для магазинов)
        const string tagExclude = "Исключить";
        // Имя группы контейнеров для проверки ресурсов и торговли (заранее создать группу)
        const string groupContainersForTrade = "";
        // Тег в названии контейнера для проверки на наличие ресурсов и торговли
        const string tagContainerForTrade = "";

        const bool tradeComponents = true; // Разрешить или запретить торговлю компонентами
        const bool tradeIngots = true; // Разрешить или запретить торговлю слитками
        const bool tradeOres = true; // Разрешить или запретить торговлю рудами
        const bool tradeTools = true; // Разрешить или запретить всю торговлю инструментами

        // Списки компонентов и т.д. (меняем только указанные цены и закупку/продажу и режим. Названия не трогать!)
        /* Разберём пример: ["BulletproofGlass"] = new MyItem(235, 830, true, 1065, true, StoreMode.Storage),
         * Меняем только то что в круглых скобках - (235, 830, true, 1065, true, StoreMode.Storage)
         * 235 это граница до которой закупать или продавать
         * 830 это цена закупки магазином (может быть любой)
         * true это включить или выключить закупку.(true или false)
         * 1065 это цена продажи магазином (не можем быть меньше определенной границы)
         * true это включить или выключить продажу
         * StoreMode.Storage это режим торговли данным товаром. Есть 2 режима
         *  TradeModel.Storage - поддержание нужного уровня на складе.
         *  Закупка, если товара ниже границы и продажа, если его больше границы.
         *  TradeModel.Shop - одновременная закупки и продажа.
         *  Продаём всё что есть на складе и закупаем до указанной границы. Больше подходит для замкнутого магазина
         */
        static internal Dictionary<string, MyItem> Components = new Dictionary<string, MyItem>()
        {
            ["BulletproofGlass"] = new MyItem(235, 830, true, 1065, true, TradeModel.Storage),      // Бронестекло
            ["Canvas"] = new MyItem(10, 2500, true, 3105, true, TradeModel.Storage),                // Парашут
            ["Computer"] = new MyItem(400, 45, true, 47, true, TradeModel.Storage),                 // Компьютеры
            ["Construction"] = new MyItem(1000, 430, true, 498, true, TradeModel.Storage),          // Строительные компоненты
            ["Detector"] = new MyItem(30, 2236, true, 2743, true, TradeModel.Storage),              // Компоненты детектора
            ["Display"] = new MyItem(30, 381, true, 416, true, TradeModel.Storage),                 // Экраны
            ["Explosives"] = new MyItem(0, 33633, false, 37475, false, TradeModel.Storage),         // Взрывчатка
            ["Girder"] = new MyItem(100, 360, true, 374, true, TradeModel.Storage),                 // Балка
            ["GravityGenerator"] = new MyItem(0, 150000, false, 385875, false, TradeModel.Storage), // Компоненты гравигенератора
            ["InteriorPlate"] = new MyItem(200, 154, true, 187, true, TradeModel.Storage),          // Внутренние пластины
            ["LargeTube"] = new MyItem(200, 1702, true, 2079, true, TradeModel.Storage),            // Большие трубы
            ["Medical"] = new MyItem(0, 40000, false, 43072, false, TradeModel.Storage),             // Медицинские компоненты
            ["MetalGrid"] = new MyItem(300, 3265, true, 3625, true, TradeModel.Storage),            // Компоненты решетки
            ["Motor"] = new MyItem(150, 2008, true, 2228, true, TradeModel.Storage),                // Моторы
            ["PowerCell"] = new MyItem(50, 1078, true, 1180, true, TradeModel.Storage),             // Батарейки
            ["RadioCommunication"] = new MyItem(30, 515, true, 710, true, TradeModel.Storage),      //Радиоантенна
            ["Reactor"] = new MyItem(0, 6410, false, 8478, false, TradeModel.Storage),              // Компоненты реактора
            ["SmallTube"] = new MyItem(300, 267, true, 311, true, TradeModel.Storage),              // Малые трубки
            ["SolarCell"] = new MyItem(0, 641, false, 849, false, TradeModel.Storage),              // Солнечные панели
            ["SteelPlate"] = new MyItem(2500, 1236, true, 1311, true, TradeModel.Storage),          // Стальные пластины
            ["Superconductor"] = new MyItem(0, 21354, false, 26524, false, TradeModel.Storage),     // Сверхпроводник
            ["Thrust"] = new MyItem(0, 41325, false, 45068, false, TradeModel.Storage),             // Компоненты двигателей
            ["ZoneChip"] = new MyItem(0, 105000, false, 100000, false, TradeModel.Storage),         // Ключи
        };

        static internal Dictionary<string, MyItem> Ingots = new Dictionary<string, MyItem>()
        {   // Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["Cobalt"] = new MyItem(1000, 1535, true, 1600, true, TradeModel.Storage),      // Кобальт
            ["Gold"] = new MyItem(1000, 23355, true, 24000, true, TradeModel.Storage),        // Золото
            ["Iron"] = new MyItem(1000, 150, true, 170, true, TradeModel.Storage), // Железо
            ["Magnesium"] = new MyItem(1000, 34054, true, 34500, true, TradeModel.Storage),   // Магний
            ["Nickel"] = new MyItem(1000, 306, true, 310, true, TradeModel.Storage),      // Никель
            ["Platinum"] = new MyItem(10, 122815, true, 123000, true, TradeModel.Storage),    // Платина
            ["Silicon"] = new MyItem(1000, 173, true, 180, true, TradeModel.Storage),     // Кремний
            ["Silver"] = new MyItem(1000, 2585, true, 2600, true, TradeModel.Storage),      // Серебро
            ["Uranium"] = new MyItem(50, 80664, true, 80700, true, TradeModel.Storage),     // Уран
        };

        static internal Dictionary<string, MyItem> Ores = new Dictionary<string, MyItem>()
        {   // Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["Cobalt"] = new MyItem(1000, 300, true, 310, true, TradeModel.Storage),        // Кобальт
            ["Gold"] = new MyItem(1000, 210, true, 230, true, TradeModel.Storage),          // Золото
            ["Stone"] = new MyItem(1000, 10, true, 11, true, TradeModel.Storage),         // Камень
            ["Iron"] = new MyItem(1000, 105, true, 110, true, TradeModel.Storage),          // Железо
            ["Magnesium"] = new MyItem(1000, 210, true, 212, true, TradeModel.Storage),     // Магний
            ["Nickel"] = new MyItem(1000, 100, true, 105, true, TradeModel.Storage),        // Никель
            ["Platinum"] = new MyItem(1000, 420, true, 435, true, TradeModel.Storage),      // Платина
            ["Silicon"] = new MyItem(1000, 100, true, 110, true, TradeModel.Storage),       // Кремний
            ["Silver"] = new MyItem(1000, 210, true, 212, true, TradeModel.Storage),        // Серебро
            ["Uranium"] = new MyItem(1000, 500, true, 505, true, TradeModel.Storage),       // Уран
            ["Ice"] = new MyItem(1000, 50, true, 51, true, TradeModel.Storage),       // Лёд
        };

        static internal Dictionary<string, MyItem> Tools = new Dictionary<string, MyItem>()
        {// Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["UltimateAutomaticRifleItem"] = new MyItem(2, 385085, true, 400000, true, TradeModel.Storage),   // Элитная виновка
            ["AngleGrinder4Item"] = new MyItem(2, 191220, true, 200000, true, TradeModel.Storage),            // Элитная пила
            ["HandDrill4Item"] = new MyItem(2, 192637, true, 200000, true, TradeModel.Storage),               // Элитный бур
            ["Welder4Item"] = new MyItem(2, 190240, true, 200000, true, TradeModel.Storage),                  // Элитный сварщик
            ["RapidFireAutomaticRifleItem"] = new MyItem(2, 1712, true, 2000, true, TradeModel.Storage),  // Скорострельная винтовка
            ["PreciseAutomaticRifleItem"] = new MyItem(2, 5215, true, 6000, true, TradeModel.Storage),   // Точная винтовка
        };

        static internal Dictionary<string, MyItem> Oxygen = new Dictionary<string, MyItem>()
        {// Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["OxygenBottle"] = new MyItem(2, 13858, true, 14000, true, TradeModel.Storage)                 // Кислородный баллон
        };

        static internal Dictionary<string, MyItem> Hydrogen = new Dictionary<string, MyItem>()
        {// Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["HydrogenBottle"] = new MyItem(2, 13858, true, 14000, true, TradeModel.Storage),               // Водородный баллон
        };

        static internal Dictionary<string, MyItem> Ammo = new Dictionary<string, MyItem>()
        {// Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["Missile200mm"] = new MyItem(100, 38620, true, 40000, true, TradeModel.Storage),                 // Ракеты
            ["NATO_25x184mm"] = new MyItem(50, 65844, true, 66000, true, TradeModel.Storage),                // Коробка патронов
            ["NATO_5p56x45mm"] = new MyItem(50, 3169, true, 3300, true, TradeModel.Storage),               // Магазин с патронами
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
        public enum TradeModel : byte { Shop, Storage }

        public Program()
        {
            AutoStore = new MyAutoStore(tradeComponents, tradeIngots, tradeOres, tradeTools, timeRefresh);
            AutoStore.GetStoreBlock(GridTerminalSystem, Me.CubeGrid, storeType);
            CheckingSystem();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            AvailableCommands();
        }

        public void Main(string arg, UpdateType updateSource)
        {
            if (AutoStore.TimeCheckStore.IsOut()) // Ожидаем, пока не настанет время обновления магазина
            {
                AutoStore.StoreUpdate(GridTerminalSystem, Me, groupContainersForTrade, tagContainerForTrade, tagExclude); // Главный метод обновления товаров в магазине
                Me.GetSurface(0).WriteText(AutoStore.Warning);
                if (Runtime.UpdateFrequency != UpdateFrequency.Update10) Runtime.UpdateFrequency = UpdateFrequency.Update10;
            }
            else if (Runtime.UpdateFrequency != UpdateFrequency.Update100) Runtime.UpdateFrequency = UpdateFrequency.Update100;
            
            Echo($"Выполнение {Runtime.LastRunTimeMs} мс");
            if (arg != string.Empty) Arguments(arg);
        }

        void CheckingSystem()
        {
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
        }
        void Arguments(string arg)
        {
            if (arg.ToLower() == arguments[0])
            {
                AutoStore.TimeCheckStore.Stop();
                Echo(" => \nРазмещение товаров");
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[1])
            {
                AutoStore.StoreComp.ClearAll();
                AutoStore.StoreIng.ClearAll();
                AutoStore.StoreOre.ClearAll();
                AutoStore.StoreTool.ClearAll();
                Echo(" => \nОчистка завершено");
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[2])
            {
                Me.CustomData = AutoStore.StoreComp.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreIng.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreOre.GetOrdersAndOffers();
                Me.CustomData += AutoStore.StoreTool.GetOrdersAndOffers();
                Echo(" => \nТовары из магазина\nвыведны в данные ПБ");
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[3])
            {
                Echo($"Обновление магазина через\n{AutoStore.TimeCheckStore.RestTime}");
                AvailableCommands();
            }
            else AvailableCommands();
        }
        void AvailableCommands()
        {
            string info = "\nВозможные аргументы:";
            foreach (var arg in arguments) { info += $"\n{arg}"; }
            Echo(info);
        }
    }
}
