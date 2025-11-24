using System;

namespace MyMenuSystem
{
    public interface IMenu
    {
        // Методы для работы с меню
        void ShowMenu(); // Отображает меню
        void HandleChoice(int choice); // Обрабатывает выбор пользователя
        void Exit(); // Завершает работу программы
        void BackToMainMenu(); // Возвращает в главное меню
        void Run(); // Запускает цикл обработки выбора пользователя
    }
}