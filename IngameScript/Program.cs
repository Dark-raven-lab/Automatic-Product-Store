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
        const string storeName = "Магазин компонентов"; // Имя блока магазина (если не указано то будет выбран первый подходящий в гирде)

        // ============ ОПЦИОНАЛЬНЫЕ НАСТРОЙКИ МАГАЗИНА ============ 
        const int timeRefresh = 3600; // Интервал для обновления товаров в магазине в секундах (3600 сек = 1 час)

        // Тег в названии блока для исключения из работы скрипта
        const string tagExclude = "Исключить";

        // Имя группы контейнеров для проверки ресурсов и торговли (заранее создать группу)
        const string groupContainersForTrade = "";

        // Тег в названии контейнера для проверки на наличие ресурсов и торговли
        const string tagContainerForTrade = "";

        // Список компонентов (меняем только указанные цены и закупку/продажу. Названия не трогать!)
        static internal Dictionary<string, MyItem> Components = new Dictionary<string, MyItem>()
        {// Граница закупки и продажи / Цена закупки / Вкл закупку / Цена продажи / Вкл продажу
            ["BulletproofGlass"] = new MyItem(235, 830, true, 1065, true),// Бронестекло
            ["Canvas"] = new MyItem(10, 2500, true, 3105, true),// Парашут
            ["Computer"] = new MyItem(400, 45, true, 47, true),// Компьютеры
            ["Construction"] = new MyItem(1000, 430, true, 498, true),// Строительные компоненты
            ["Detector"] = new MyItem(30, 2236, true, 2743, true),// Компоненты детектора
            ["Display"] = new MyItem(30, 381, true, 416, true),// Экраны
            ["Explosives"] = new MyItem(0, 33633, false, 37475, false),// Взрывчатка
            ["Girder"] = new MyItem(100, 360, true, 374, true),// Балка
            ["GravityGenerator"] = new MyItem(0, 150000, false, 385875, false),// Компоненты гравигенератора
            ["InteriorPlate"] = new MyItem(200, 154, true, 187, true),// Внутренние пластины
            ["LargeTube"] = new MyItem(200, 1702, true, 2079, true),// Большие трубы
            ["Medical"] = new MyItem(0, 40000, false, 43072, false),// Медицинские компоненты
            ["MetalGrid"] = new MyItem(300, 3265, true, 3625, true),// Компоненты решетки
            ["Motor"] = new MyItem(150, 2008, true, 2228, true),// Моторы
            ["PowerCell"] = new MyItem(50, 1078, true, 1180, true),// Батарейки
            ["RadioCommunication"] = new MyItem(30, 515, true, 710, true),//Радиоантенна
            ["Reactor"] = new MyItem(0, 6410, false, 8478, false),// Компоненты реактора
            ["SmallTube"] = new MyItem(300, 267, true, 311, true),// Малые трубки
            ["SolarCell"] = new MyItem(0, 641, false, 849, false),// Солнечные панели
            ["SteelPlate"] = new MyItem(2500, 1236, true, 1311, true),// Стальные пластины
            ["Superconductor"] = new MyItem(0, 21354, false, 26524, false),// Сверхпроводник
            ["Thrust"] = new MyItem(0, 41325, false, 45068, false),// Компоненты двигателей
            ["ZoneChip"] = new MyItem(0, 105000, false, 100000, false),// Ключи
        };
        static internal Dictionary<string, MyItem> Tools = new Dictionary<string, MyItem>()
        {
            ["UltimateAutomaticRifleItem"] = new MyItem(),// Элитная виновка
            ["AngleGrinder4Item"] = new MyItem(), // Элитная пила
            ["HandDrill4Item"] = new MyItem(), // Элитный бур
            ["Welder4Item"] = new MyItem(), // Элитная горелка
            ["HydrogenBottle"] = new MyItem(), // Водородный баллон
            ["OxygenBottle"] = new MyItem(), // Кислородный баллон
            ["Missile200mm"] = new MyItem(), // Ракеты
            ["NATO_25x184mm"] = new MyItem(), // Коробка патронов
            ["NATO_5p56x45mm"] = new MyItem(), // Магазин с патронами
        };
        static internal Dictionary<string, MyItem> Ingots = new Dictionary<string, MyItem>()
        {

        };
        static internal Dictionary<string, MyItem> Ores = new Dictionary<string, MyItem>()
        {

        };

        // ============ КОНЕЦ НАСТРОЕК ============
        MyAutoStore AutoStore;
        readonly string[] arguments = new string[] {
            "магазин.разместить",
            "магазин.очистить",
            "магазин.список"
        };

        public Program()
        {
            AutoStore = new MyAutoStore(GridTerminalSystem, Me.CubeGrid, storeName, timeRefresh);
            if (AutoStore.StoreComp.Block != null)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
                Echo("Скрипт запущен\n"); AvailableCommands();
            }
            else Echo("Магазин не найден\nВыполнение остановлено");
        }

        public void Main(string arg, UpdateType updateSource)
        {
            string txt = $"Время выполнения: {Math.Round(Runtime.LastRunTimeMs, 3)}";
            txt += $"\nОбновление магазина через {AutoStore.TimeCheckStore.RestTime} сек";
            if (AutoStore.TimeToUpgrade()) // Ожидаем, пока не настанет время обновления магазина
                AutoStore.StoreUpdate(GridTerminalSystem, Me, groupContainersForTrade, tagContainerForTrade, tagExclude); // Главный метод обновления товаров в магазине
            Me.GetSurface(0).WriteText(txt);
            if (arg != string.Empty) Arguments(arg);
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
                Echo(" => \nОчистка завершено");
                AvailableCommands();
            }
            else if (arg.ToLower() == arguments[2])
            {
                Me.CustomData = AutoStore.StoreComp.GetOrdersAndOffers();
                Echo(" => \nТовары из магазина\nвыведны в данные ПБ");
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
