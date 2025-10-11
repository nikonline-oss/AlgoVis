using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMenuSystem
{
    public class MenuItem
    {
        public string Text { get; set; }
        public Action Action { get; set; }

        public MenuItem(string text, Action action)
        {
            Text = text;
            Action = action;
        }
    }

    public class MenuManager
    {
        public string LastTextShowMenu { get; set; } = "Выйти";
        public Action ActionExite { get; set; } = null;
        public bool SubMenuBool { get; set; } = false;

        private const int MaxMenuItems = 20;
        private MenuItem[] _menuItems = new MenuItem[MaxMenuItems];
        private int _menuItemCounter = 0;
        private IMenu _mainMenu;

        public MenuManager(IMenu mainMenu)
        {
            _mainMenu = mainMenu;
        }

        public void ShowMenu()
        {
            Console.WriteLine("\n" + new string('═', 40));
            Console.WriteLine("📋 МЕНЮ:");
            Console.WriteLine(new string('─', 40));

            for (int i = 0; i < _menuItemCounter; i++)
            {
                Console.WriteLine($"{"•",2} {i + 1,-2} - {_menuItems[i].Text}");
            }

            Console.WriteLine(new string('─', 40));
            Console.WriteLine($"{"•",2} {_menuItemCounter + 1,-2} - {LastTextShowMenu}");
            Console.WriteLine(new string('═', 40));
            Console.Write("🎯 Введите ваш выбор: ");
        }

        public void HandleChoice(int choice)
        {
            Console.WriteLine();

            if (choice > 0 && choice <= _menuItemCounter)
            {
                try
                {
                    _menuItems[choice - 1].Action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка выполнения: {ex.Message}");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
            else if (choice == _menuItemCounter + 1)
            {
                Exit();
            }
            else
            {
                Console.WriteLine("❌ Неверный выбор. Попробуйте снова.");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        public void Exit()
        {
            if (SubMenuBool)
            {
                _mainMenu.BackToMainMenu();
            }
            else
            {
                ActionExite?.Invoke();
                Console.WriteLine("\n👋 До свидания!");
                Environment.Exit(0);
            }
        }

        public void BackToMainMenu()
        {
            _mainMenu.Run();
            SubMenuBool = false;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                ShowMenu();

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    HandleChoice(choice);
                }
                else
                {
                    Console.WriteLine("❌ Некорректный ввод. Попробуйте снова.");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        public void AddMenuItem(string text, Action action)
        {
            if (_menuItemCounter < MaxMenuItems)
            {
                _menuItems[_menuItemCounter++] = new MenuItem(text, action);
            }
            else
            {
                throw new InvalidOperationException("Меню переполнено");
            }
        }

        public void ClearMenu()
        {
            _menuItemCounter = 0;
            Array.Clear(_menuItems, 0, _menuItems.Length);
        }
    }
}